using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathVoid : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
		// if the object that collided with this object has the tag "Player"
		if (collision.gameObject.tag == "Player")
        {
			// die
			collision.gameObject.GetComponent<Health>().TakeDamage(-1);
		}
		else if (collision.gameObject.tag == "Ground")
		{
			// do nothing
		}

		// If it's not the player just destroy the object
		else
		{
			Destroy(collision.gameObject);
		}
	}
}
