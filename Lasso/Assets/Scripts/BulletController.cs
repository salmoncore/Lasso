using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    // If the bullet collides with an object, destroy the bullet
    void OnCollisionEnter2D(Collision2D col)
    {
		Destroy(gameObject);
	}
}
