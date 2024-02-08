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
    [SerializeField] private float groundCheckSize = 0.5f;
    [SerializeField] private LayerMask groundLayer;
    private Rigidbody2D body;
    private Animator anim;
    private BoxCollider2D boxCollider;

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

        // Setting the velocity of the Rigidbody2D component to the input axis on the X axis
        body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);

        // Flipping the sprite based on the input axis
        if (horizontalInput > 0.01f)
        {
            transform.localScale = new Vector3(6, 6, 1);
        }
        else if (horizontalInput < -0.01f)
        {
            transform.localScale = new Vector3(-6, 6, 1);
        }


        // Jumping
        if (Input.GetButtonDown("Jump") && isGrounded()) {
            jump = true;
        } 
        
        if (Input.GetButtonUp("Jump") && !isGrounded()) {
            jumpCancel = true;
        }

        // Setting the animation parameters
        anim.SetBool("run", horizontalInput != 0);
        anim.SetBool("grounded", isGrounded());
    }

    void FixedUpdate() {
        // Normal jump at full speed
        if (jump) {
            Debug.Log(jumpSpeed);
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
        boxCastSize.x -= groundCheckSize; // Reduce the width of the BoxCast

        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCastSize, 0f, Vector2.down, 0.1f, groundLayer);
        return raycastHit.collider != null;
    }

    public bool canAttack()
    {
        // Modify to prevent attacking when doing certain movement
        return true;
    }
}
