using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Serializing the fields so that it shows up in the Unity inspector
    [SerializeField] private float speed = 5f; // Default values, change in Unity
    [SerializeField] private float jump = 5f;
    private Rigidbody2D body;

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        // Getting the Rigidbody2D component from the game object
        body = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Setting the velocity of the Rigidbody2D component to the input axis on the X axis
        body.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * speed, body.velocity.y);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Setting the velocity of the Rigidbody2D component to the input axis on the Y axis
            body.velocity = new Vector2(body.velocity.x, jump);
        }
    }
}
