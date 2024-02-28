using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LassoHandler : MonoBehaviour
{
	private float initialSpeed;

	public void Initialize(float speed)
	{
		initialSpeed = speed;
		GetComponent<Rigidbody2D>().gravityScale = 0;
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		// PS this is for when the projectile becomes not a trigger and needs to have gravity applied. 

		Rigidbody2D rb = GetComponent<Rigidbody2D>();
		rb.velocity = new Vector2(0, rb.velocity.y);

		rb.gravityScale = 1;
	}

	void OnTriggerEnter2D(Collider2D collision)
	{
		// Occurs when a projectile hits an enemy. 
		// collision.gameObject is the object that hit the enemy.
		// The purpose of this method is to check if the projectile is an enemy projectile and if so, stun the enemy, while enabling physics on the projectile and changing it's tag and layer to crumpled

		// Check to see if the projectile is an enemy projectile
		if (collision.gameObject.CompareTag("EnemyProjectile"))
		{
			// If the enemy that was hit has a stun method, call it.
			if (collision.gameObject.CompareTag("EnemyProjectile") && gameObject.TryGetComponent<EnemyControl>(out EnemyControl enemyControl))
			{
				enemyControl.Stun();
			}
			if (collision.gameObject.CompareTag("Interactable"))
			{
				return;
			}

			// Change the projectile's tag and layer to crumpled, and enable physics on the projectile
			collision.gameObject.tag = "CrumpledEnemy";
			collision.gameObject.layer = LayerMask.NameToLayer("CrumpledEnemies");

			collision.gameObject.GetComponent<Collider2D>().isTrigger = false;
			collision.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
			collision.gameObject.GetComponent<Rigidbody2D>().gravityScale = 1;
		}
	}

}