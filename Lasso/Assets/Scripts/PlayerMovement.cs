using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	[SerializeField] private float accelerationTime = 0.2f; // Time to reach full speed
	private float accelSmoothing;
	private Rigidbody2D body;
    private Animator anim;
    private BoxCollider2D boxCollider;
    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    private float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        // Grab references from game object
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

	private void Update()
	{
        float horizontalInput = Input.GetAxis("Horizontal");

		bool grounded = isGrounded();
		bool collidingLeft = isCollidingLeft();
		bool collidingRight = isCollidingRight();

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
        }

        // jump buffer
        if (Input.GetButtonDown("Jump")) {
            jumpBufferCounter = jumpBufferTime;
        } else {
            jumpBufferCounter -= Time.deltaTime;
        }

        // jumping with coyote time and jump buffer
		if (coyoteTimeCounter > 0f && jumpBufferCounter > 0f) {
            jumpBufferCounter = 0f;
			jump = true;
		}

		if (Input.GetButtonUp("Jump") && !grounded) {
            coyoteTimeCounter = 0f;
			jumpCancel = true;
		}

		flipSprite(horizontalInput);
		anim.SetBool("run", horizontalInput != 0);
		anim.SetBool("grounded", grounded);
	}

	void flipSprite(float horizontalInput)
    {
		if (horizontalInput > 0.01f)
		{
			transform.localScale = new Vector3(6, 6, 1);
		}
		else if (horizontalInput < -0.01f)
		{
			transform.localScale = new Vector3(-6, 6, 1);
		}
	}

    void FixedUpdate() {
        // Normal jump at full speed
        if (jump) {
            body.velocity = new Vector2(body.velocity.x, jumpSpeed);
            jump = false;
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
