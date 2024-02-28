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

		if (collision.gameObject.CompareTag("Ground"))
		{
			GetComponent<Rigidbody2D>().gravityScale = 3;
		}

		if (collision.gameObject.CompareTag("EnemyProjectile"))
		{
			// If the enemy that was hit has a stun method, call it.
			if (collision.gameObject.CompareTag("EnemyProjectile") && gameObject.TryGetComponent<EnemyControl>(out EnemyControl hitEnemyControl))
			{
				hitEnemyControl.Stun();
			}
			if (collision.gameObject.CompareTag("Interactable"))
			{
				return;
			}

			// This might be something you'd want to be conditional.
			GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
			GetComponent<Rigidbody2D>().gravityScale = 3;

			// Change the projectile's tag and layer to crumpled, and enable physics on the projectile
			collision.gameObject.tag = "CrumpledEnemy";
			collision.gameObject.layer = LayerMask.NameToLayer("CrumpledEnemies");

			// If the collision.gameObject has a EnemyControl script, disable it.
			//if (gameObject.TryGetComponent<EnemyControl>(out EnemyControl projectileEnemyControl))
			//{
			//	projectileEnemyControl.enabled = false;
			//}

			collision.gameObject.GetComponent<Collider2D>().isTrigger = false;
			collision.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
			collision.gameObject.GetComponent<Rigidbody2D>().gravityScale = 1;
		}

		// TODO: This is probably a good spot to begin the funny bounce and fall out of stage thing!
	}

}