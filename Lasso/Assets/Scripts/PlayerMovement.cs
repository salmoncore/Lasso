using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private BoxCollider2D boxCollider;
    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    private float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;
    public PauseManager pause;

    private PlayerInput playerInput;
    private Vector2 movementDirection;

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        // Grab references from game object
        pause = FindObjectOfType<PauseManager>();
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();

        playerInput = GetComponent<PlayerInput>();
        playerInput.onActionTriggered += PlayerInput_onActionTriggered;
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

			if (context.canceled && !isGrounded())
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
            if (grounded) {
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
            anim.SetBool("isGrounded", grounded);
            anim.SetBool("isFalling", !grounded);

            // Leaving this in for the sprite animations
            anim.SetBool("run", horizontalInput != 0);
            anim.SetBool("grounded", grounded);
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
		Vector2 boxCastSize = boxCollider.bounds.size;
		boxCastSize.x -= boxCrop;
		
		RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCastSize, 0f, Vector2.down, 0.1f, groundLayer);
		return raycastHit.collider != null;
	}

    private bool isOnObject()
    {
		Vector2 boxCastSize = boxCollider.bounds.size;
		boxCastSize.x -= boxCrop;

		RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCastSize, 0f, Vector2.down, 0.1f, interactiveLayer);
		return raycastHit.collider != null;
	}

    private bool isCollidingLeft()
    {
		Vector2 boxCastSize = boxCollider.bounds.size;
		boxCastSize.y -= boxCrop;

		RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCastSize, 0f, Vector2.left, 0.1f, groundLayer);
		return raycastHit.collider != null;
	}

    private bool isCollidingRight()
    {
        Vector2 boxCastSize = boxCollider.bounds.size;
        boxCastSize.y -= boxCrop;

        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCastSize, 0f, Vector2.right, 0.1f, groundLayer);
        return raycastHit.collider != null;
    }

	public bool canAttack()
    {
        // Modify to prevent attacking when doing certain movement
        return true;
    }
}
