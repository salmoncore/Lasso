using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    // Serializing the fields so that it shows up in the Unity inspector
    [SerializeField] private float speed = 5f; // Default values, change in Unity
    [SerializeField] private float jumpShortSpeed = 4f;
    [SerializeField] private float jumpSpeed = 11.5f;
    [SerializeField] private bool jump = false;
    [SerializeField] private bool jumpCancel = false;
    [SerializeField] private float boxCrop = 0.5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask interactiveLayer;
	[SerializeField] private float accelerationTime = 0.2f; // Time to reach full speed
    [SerializeField] private Vector2 boxCastSize;
    [SerializeField] private Vector2 boxCastOffset;
    [SerializeField] private Vector2 sideCheckSize;
    [SerializeField] private Vector2 sideCheckOffset;
	private float accelSmoothing;
	private Rigidbody2D body;
    private Animator anim;
    private CapsuleCollider2D playerCollider;
    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    private float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;
    public PauseManager pause;

    private PlayerInput playerInput;
    private Vector2 movementDirection;

    // Win Conditions - Use winOnClear and winOnGoal to determine how the player wins.
    [SerializeField] private bool winOnClear = false;
    [SerializeField] private bool winOnGoal = false;
    private bool goalFlag = false;
    private bool clearFlag = false;
    private bool winFlag = false;
    private GameObject ObjectiveText;
    private int initialEnemyCount;
    private int enemiesRemaining;

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        // Grab references from game object
        pause = FindObjectOfType<PauseManager>();
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        playerCollider = GetComponent<CapsuleCollider2D>();

        playerInput = GetComponent<PlayerInput>();
        playerInput.onActionTriggered += PlayerInput_onActionTriggered;

        if (winOnGoal && GameObject.FindGameObjectsWithTag("Goal").Length != 1)
        {
			Debug.Log("Warning: Multiple goals exist within scene.");
		}

        // ObjectiveText is the name of a gameobject that is the child of the UICanvas gameobject in the scene
        ObjectiveText = GameObject.Find("TextBackground");

        if (ObjectiveText == null)
        {
			Debug.Log("ObjectiveText object not found, is UICanvas in the scene?");
		}

        initialEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length + GameObject.FindGameObjectsWithTag("StunnedEnemy").Length;
    }

    // Public method that returns if the player has won
    public bool HasWon()
    {
		return winFlag;
	}

    // Clear Detection - If there are no more gameobjects with the tag "Enemy" or "EnemyProjectile" or "FragileProjectile" or "Breaking", the player wins.
    private void checkClear()
    {
        enemiesRemaining = (GameObject.FindGameObjectsWithTag("Enemy").Length) + GameObject.FindGameObjectsWithTag("StunnedEnemy").Length;

        // Check for "Enemy", "EnemyProjectile", "FragileProjectile", and "Breaking" tags
		if (enemiesRemaining == 0)
        {
            // If none exist, clearFlag is true
			clearFlag = true;

			ObjectiveText.GetComponent<UnityEngine.UI.Text>().text = "ONTO THE GOAL!";
            
            // pulse the text color between yellow and green
            ObjectiveText.GetComponent<UnityEngine.UI.Text>().color = new Color(1, Mathf.PingPong(Time.time, 1), 0, 1);
		}
        else
        {
			// If any exist, clearFlag is false
			clearFlag = false;

            if (enemiesRemaining == 1)
            {
                ObjectiveText.GetComponent<UnityEngine.UI.Text>().text = "1 VARMIT REMAINS";
                ObjectiveText.GetComponent<UnityEngine.UI.Text>().color = new Color(1, Mathf.PingPong(Time.time, 1), Mathf.PingPong(Time.time, 1), 1);
            }
            if (enemiesRemaining == initialEnemyCount)
            {
				ObjectiveText.GetComponent<UnityEngine.UI.Text>().text = "ADMINISTER JUSTICE";
                // pulse the text color between red and yellow
                ObjectiveText.GetComponent<UnityEngine.UI.Text>().color = new Color(1, Mathf.PingPong(Time.time, 1), 0, 1);
			}
            else
            {
                ObjectiveText.GetComponent<UnityEngine.UI.Text>().text = enemiesRemaining + " VARMITS REMAINING";
                ObjectiveText.GetComponent<UnityEngine.UI.Text>().color = new Color(1, 1 - (float)enemiesRemaining / initialEnemyCount, 1 - (float)enemiesRemaining / initialEnemyCount, 1);
            }
		}
	}

    // Checks to see if the player has won, using the win conditions set in the inspector
    private bool playerWins()
    {
        checkClear();

		// If the clearFlag is true, and the game is set to win ONLY on clear, the player wins
		if (clearFlag && (winOnClear && !winOnGoal))
		{
            Debug.Log("1");
			winFlag = true;
		}

		// If the goalFlag is true, and the game is set to win ONLY on goal, the player wins
        if (goalFlag && (winOnGoal && !winOnClear))
        {
            Debug.Log("2");
            winFlag = true;
		}

        // If the clearFlag is true, and the goalFlag is true, and the game is set to win on both, the player wins
        if (clearFlag && goalFlag && (winOnClear && winOnGoal))
        {
            Debug.Log("3");
            winFlag = true;
        }

        return winFlag;
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
		if (collision.CompareTag("Goal"))
        {
			goalFlag = true;
		}
	}

	private void OnTriggerExit2D(Collider2D notcollisionlol)
	{
		if (notcollisionlol.CompareTag("Goal"))
        {
			goalFlag = false;
		}
	}

	private void OnEnable()
	{
		playerInput.onControlsChanged += PlayerInput_onControlsChanged;
	}

    private void OnDisable()
    {
		playerInput.onControlsChanged -= PlayerInput_onControlsChanged;
	}

	private void PlayerInput_onControlsChanged(PlayerInput obj)
	{
        // Nintendo switch pro controller xinput/hid input fix
		var gamepads = Gamepad.all;
		foreach (var gamepad in gamepads)
		{
			// Check if the current gamepad is a Nintendo Switch Pro Controller
			if (gamepad.description.interfaceName == "HID" &&
				gamepad.description.manufacturer.Contains("Nintendo"))
			{
				//Debug.Log($"Nintendo Switch Pro Controller detected: {gamepad}");

				// Loop through all gamepads again to find any XInput device activated at the same time
				foreach (var otherGamepad in gamepads)
				{
					// Check if the other gamepad is an XInput device and not the same as the current gamepad
					if (otherGamepad != gamepad &&
						otherGamepad.description.interfaceName == "XInput" &&
						Math.Abs(otherGamepad.lastUpdateTime - gamepad.lastUpdateTime) < 0.1)
					{
						// Log and disable the XInput device
						//Debug.Log($"Disabling XInput device due to Nintendo Switch Pro Controller detection: {otherGamepad}");
						InputSystem.DisableDevice(otherGamepad);
					}
				}
			}
		}
	}

	private void PlayerInput_onActionTriggered(InputAction.CallbackContext context)
    {
		if (context.action.name == playerInput.actions["Jump"].name)
        {
			if (context.performed)
            {
				jumpBufferCounter = jumpBufferTime;
			}

			if (context.canceled && !isGrounded() && !isOnObject())
            {
				coyoteTimeCounter = 0f;
				jumpCancel = true;
			}
		}

		if (context.action.name == playerInput.actions["Movement"].name)
		{
			movementDirection = context.ReadValue<Vector2>();
		}
	}

	private void Update()
	{
        float horizontalInput;

		bool grounded = isGrounded();
        bool onObject = isOnObject();
        bool collidingLeft = isCollidingLeft();
        bool collidingRight = isCollidingRight();

        if (collidingLeft)
        { 
            Debug.Log("Colliding Left");
        }
        if (collidingRight)
        {
            Debug.Log("Colliding Right");
        }

        if (playerWins())
        {
			// Enters here if player has won
            // Maybe add in a delay or a win screen?
			SceneManager.LoadScene("MainMenu");
		}

        if (pause.isPaused) {

        } else {
            horizontalInput = movementDirection.x;
            float targetVelocityX = 0;
            if (!(collidingLeft && horizontalInput < 0) && !(collidingRight && horizontalInput > 0))
            {
                targetVelocityX = horizontalInput * speed;
            }

            body.velocity = new Vector2(
                Mathf.SmoothDamp(body.velocity.x, targetVelocityX, ref accelSmoothing, accelerationTime),
                body.velocity.y
            );

            // coyote time stuff with jump
            if (grounded || onObject) {
                coyoteTimeCounter = coyoteTime;
            } else {
                coyoteTimeCounter -= Time.deltaTime;
				jumpBufferCounter -= Time.deltaTime;
			}

			// jumping with coyote time and jump buffer
			if (coyoteTimeCounter > 0f && jumpBufferCounter > 0f) {
                jumpBufferCounter = 0f;
                jump = true;
            }

            flipSprite(horizontalInput);

            anim.SetBool("isRunning", horizontalInput != 0);
            anim.SetBool("isGrounded", grounded || onObject);
            anim.SetBool("isFalling", !grounded && !onObject);

            // Leaving this in for the sprite animations
            anim.SetBool("run", horizontalInput != 0);
            anim.SetBool("grounded", grounded || onObject);
        }
	}

	void flipSprite(float horizontalInput)
    {
        Vector3 currentScale = transform.localScale;
		if (horizontalInput > 0.01f && currentScale.z < 0)
		{
            // flip horizontally to the right - FOR THE 3D MODEL, THIS MUST BE CHANGED TO THE CURRENTSCALE OF THE Z
            transform.localScale = new Vector3(currentScale.x, currentScale.y, Mathf.Abs(currentScale.z));
        }
		else if (horizontalInput < -0.01f && currentScale.z > 0)
		{
            // flip horizontally to the left - FOR THE 3D MODEL, THIS MUST BE CHANGED TO THE CURRENTSCALE OF THE Z
            transform.localScale = new Vector3(currentScale.x, currentScale.y, -Mathf.Abs(currentScale.z));
        }
	}

    void FixedUpdate() {
        // Normal jump at full speed
        if (jump)
        {
            body.velocity = new Vector2(body.velocity.x, jumpSpeed);
            jump = false;

            // Start the isJumping animation and have it finish before setting it to false
            anim.SetBool("isJumping", true);
            StartCoroutine(StopJumping());
        }

        IEnumerator StopJumping()
        {
			yield return new WaitForSeconds(0.5f);
			anim.SetBool("isJumping", false);
		}

        if (jumpCancel) {
            if (body.velocity.y > jumpShortSpeed)
                body.velocity = new Vector2(body.velocity.x, jumpShortSpeed);
            jumpCancel = false;
        }
    }

	public bool isGrounded()
    {
		Vector3 boxcastOrigin = playerCollider.bounds.center + new Vector3(boxCastOffset.x, boxCastOffset.y, 0);

        RaycastHit2D hit = Physics2D.BoxCast(boxcastOrigin, boxCastSize, 0, Vector2.down, 0.1f, groundLayer);

		Vector3 topRight = boxcastOrigin + new Vector3(boxCastSize.x / 2, boxCastSize.y / 2, 0);
		Vector3 topLeft = boxcastOrigin + new Vector3(-boxCastSize.x / 2, boxCastSize.y / 2, 0);
		Vector3 bottomRight = boxcastOrigin + new Vector3(boxCastSize.x / 2, -boxCastSize.y / 2, 0);
		Vector3 bottomLeft = boxcastOrigin + new Vector3(-boxCastSize.x / 2, -boxCastSize.y / 2, 0);

		Debug.DrawLine(topLeft, topRight, Color.red); // Top edge
		Debug.DrawLine(topRight, bottomRight, Color.red); // Right edge
		Debug.DrawLine(bottomRight, bottomLeft, Color.red); // Bottom edge
		Debug.DrawLine(bottomLeft, topLeft, Color.red); // Left edge

		return hit.collider != null;
	}

    private bool isOnObject()
    {
		Vector3 boxcastOrigin = playerCollider.bounds.center + new Vector3(boxCastOffset.x, boxCastOffset.y, 0);

        RaycastHit2D hit = Physics2D.BoxCast(boxcastOrigin, boxCastSize, 0, Vector2.down, 0.1f, interactiveLayer);

        Vector3 topRight = boxcastOrigin + new Vector3(boxCastSize.x / 2, boxCastSize.y / 2, 0);
        Vector3 topLeft = boxcastOrigin + new Vector3(-boxCastSize.x / 2, boxCastSize.y / 2, 0);
        Vector3 bottomRight = boxcastOrigin + new Vector3(boxCastSize.x / 2, -boxCastSize.y / 2, 0);
        Vector3 bottomLeft = boxcastOrigin + new Vector3(-boxCastSize.x / 2, -boxCastSize.y / 2, 0);

        Debug.DrawLine(topLeft, topRight, Color.red); // Top edge
        Debug.DrawLine(topRight, bottomRight, Color.red); // Right edge
        Debug.DrawLine(bottomRight, bottomLeft, Color.red); // Bottom edge
        Debug.DrawLine(bottomLeft, topLeft, Color.red); // Left edge

        return hit.collider != null;
	}

    private bool isCollidingLeft()
    {
        Vector3 sidecastOrigin = playerCollider.bounds.center + new Vector3(sideCheckOffset.x, sideCheckOffset.y, 0);

        RaycastHit2D hit = Physics2D.BoxCast(sidecastOrigin, sideCheckSize, 0, Vector2.left, 0.1f, groundLayer);

        Vector3 topRight = sidecastOrigin + new Vector3(sideCheckSize.x / 2, sideCheckSize.y / 2, 0);
        Vector3 topLeft = sidecastOrigin + new Vector3(-sideCheckSize.x / 2, sideCheckSize.y / 2, 0);
        Vector3 bottomRight = sidecastOrigin + new Vector3(sideCheckSize.x / 2, -sideCheckSize.y / 2, 0);
        Vector3 bottomLeft = sidecastOrigin + new Vector3(-sideCheckSize.x / 2, -sideCheckSize.y / 2, 0);

        Debug.DrawLine(topLeft, topRight, Color.red); // Top edge
        Debug.DrawLine(topRight, bottomRight, Color.red); // Right edge
        Debug.DrawLine(bottomRight, bottomLeft, Color.red); // Bottom edge
        Debug.DrawLine(bottomLeft, topLeft, Color.red); // Left edge

        return hit.collider != null;
	}
    
    private bool isCollidingRight()
    {
        Vector3 sidecastOrigin = playerCollider.bounds.center + new Vector3(-sideCheckOffset.x, sideCheckOffset.y, 0);

        RaycastHit2D hit = Physics2D.BoxCast(sidecastOrigin, sideCheckSize, 0, Vector2.right, 0.1f, groundLayer);

        Vector3 topRight = sidecastOrigin + new Vector3(sideCheckSize.x / 2, sideCheckSize.y / 2, 0);
        Vector3 topLeft = sidecastOrigin + new Vector3(-sideCheckSize.x / 2, sideCheckSize.y / 2, 0);
        Vector3 bottomRight = sidecastOrigin + new Vector3(sideCheckSize.x / 2, -sideCheckSize.y / 2, 0);
        Vector3 bottomLeft = sidecastOrigin + new Vector3(-sideCheckSize.x / 2, -sideCheckSize.y / 2, 0);

        Debug.DrawLine(topLeft, topRight, Color.blue); // Top edge
        Debug.DrawLine(topRight, bottomRight, Color.blue); // Right edge
        Debug.DrawLine(bottomRight, bottomLeft, Color.blue); // Bottom edge
        Debug.DrawLine(bottomLeft, topLeft, Color.blue); // Left edge

        return hit.collider != null;
    }

	public bool canAttack()
    {
        // Modify to prevent attacking when doing certain movement
        return true;
    }
}
