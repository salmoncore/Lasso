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
    }

    // Clear Detection - If there are no more gameobjects with the tag "Enemy" or "EnemyProjectile" or "FragileProjectile" or "Breaking", the player wins.
    private void checkClear()
    {
        // Check for "Enemy", "EnemyProjectile", "FragileProjectile", and "Breaking" tags
		if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            // If none exist, clearFlag is true
			clearFlag = true;
		}
        else
        {
			// If any exist, clearFlag is false
			clearFlag = false;
		}
	}

    // Checks to see if the player has won, using the win conditions set in the inspector
    private bool playerWins()
    {
        checkClear();

		// If the clearFlag is true, and the game is set to win ONLY on clear, the player wins
		if (clearFlag && (winOnClear && !winOnGoal))
		{
			winFlag = true;
		}

		// If the goalFlag is true, and the game is set to win ONLY on goal, the player wins
        if (goalFlag && (winOnGoal && !winOnClear))
        {
			winFlag = true;
		}

        // If the clearFlag is true, and the goalFlag is true, and the game is set to win on both, the player wins
        if (clearFlag && goalFlag && (winOnClear && winOnGoal))
        {
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

    private bool isGrounded()
    {
		Vector2 boxCastSize = playerCollider.bounds.size;
		boxCastSize.x -= boxCrop;
		
		RaycastHit2D raycastHit = Physics2D.BoxCast(playerCollider.bounds.center, boxCastSize, 0f, Vector2.down, 0.1f, groundLayer);
		return raycastHit.collider != null;
	}

    private bool isOnObject()
    {
		Vector2 boxCastSize = playerCollider.bounds.size;
		boxCastSize.x -= boxCrop;

		RaycastHit2D raycastHit = Physics2D.BoxCast(playerCollider.bounds.center, boxCastSize, 0f, Vector2.down, 0.1f, interactiveLayer);
		return raycastHit.collider != null;
	}

    private bool isCollidingLeft()
    {
		Vector2 boxCastSize = playerCollider.bounds.size;
		boxCastSize.y -= boxCrop;

		RaycastHit2D raycastHit = Physics2D.BoxCast(playerCollider.bounds.center, boxCastSize, 0f, Vector2.left, 0.1f, groundLayer);
		return raycastHit.collider != null;
	}

    private bool isCollidingRight()
    {
        Vector2 boxCastSize = playerCollider.bounds.size;
        boxCastSize.y -= boxCrop;

        RaycastHit2D raycastHit = Physics2D.BoxCast(playerCollider.bounds.center, boxCastSize, 0f, Vector2.right, 0.1f, groundLayer);
        return raycastHit.collider != null;
    }

	public bool canAttack()
    {
        // Modify to prevent attacking when doing certain movement
        return true;
    }
}
