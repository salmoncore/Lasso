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
		//Debug.Log("Collision with " + collision.gameObject.name + " with tag " + collision.gameObject.tag + " and layer " + LayerMask.LayerToName(collision.gameObject.layer) + " on " + gameObject.name + " with tag " + gameObject.tag + " and layer " + LayerMask.LayerToName(gameObject.layer));

		// collision.gameObject is the object that this object collided with.
		// gameobject.tag is the tag of the script's gameobject.		

		if ((collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Enemy") ||
			collision.gameObject.CompareTag("Fragile") || collision.gameObject.CompareTag("Sturdy")) && 
			(gameObject.CompareTag("FragileProjectile") || gameObject.CompareTag("SturdyProjectile")))
		{
			// Apply gravity to the projectile
			GetComponent<Rigidbody2D>().gravityScale = 3;

			// Apply physics to the target, if it's an enemy, fragile or sturdy object
			if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Fragile") || collision.gameObject.CompareTag("Sturdy"))
			{
				// Unfix the collided object's constraints, and apply gravity
				collision.gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
				collision.gameObject.GetComponent<Rigidbody2D>().gravityScale = 3;
			}

			// If the target is an enemy, and it has an EnemyControl script, call the Stun method
			if (collision.gameObject.CompareTag("Enemy") && collision.gameObject.TryGetComponent<EnemyControl>(out EnemyControl hitEnemyControl))
			{
				hitEnemyControl.Stun();
			}

			// If the projectile is fragile and has an EnemyControl script, disable it
			if (gameObject.CompareTag("FragileProjectile") && gameObject.TryGetComponent<EnemyControl>(out EnemyControl projectileEnemyControl))
			{
				projectileEnemyControl.enabled = false;
				// GetComponent<Animator>().enabled = false;
			}

			// If the projectile's tag is FragileProjectile, change the tag to Breaking and the layer to Breaking
			if (gameObject.CompareTag("FragileProjectile"))
			{
				gameObject.tag = "Breaking";
				gameObject.layer = LayerMask.NameToLayer("Breaking");
			}
			else if (gameObject.CompareTag("SturdyProjectile"))
			{
				gameObject.tag = "Sturdy";
				gameObject.layer = LayerMask.NameToLayer("Interactive");
			}
		}
	}
}