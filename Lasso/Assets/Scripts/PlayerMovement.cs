using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Serializing the fields so that it shows up in the Unity inspector
    [SerializeField] private float speed = 5f; // Default values, change in Unity
    [SerializeField] private float jump = 5f;
    private Rigidbody2D body;
    private Animator anim;
    private bool grounded;

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        // Grab references from game object
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
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

        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            Jump();
        }

        // Setting the animation parameters
        anim.SetBool("run", horizontalInput != 0);
        anim.SetBool("grounded", grounded);
    }

    private void Jump() 
    {
        body.velocity = new Vector2(body.velocity.x, jump);
        anim.SetTrigger("jump");
        grounded = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            grounded = true;
        }
    }
}
