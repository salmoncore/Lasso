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
            transform.localScale = Vector3.one;
        }
        else if (horizontalInput < -0.01f)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Setting the velocity of the Rigidbody2D component to the input axis on the Y axis
            body.velocity = new Vector2(body.velocity.x, jump);
        }

        // Setting the animation parameters
        anim.SetBool("run", horizontalInput != 0);

    }
}
