using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LassoHandler : MonoBehaviour
{
	private GameObject Projectile;
	private GameObject Collided;
	private bool isOnGround = false;
	private bool isDestroyed = false;

	public void Initialize(float speed)
	{
		Projectile = null;
		Collided = null;
	}

	private void Update()
	{
		// Null check for the projectile and collided variables
		if (!(Projectile == null) && !(Collided == null) && !isDestroyed)
		{
			// PS this line always faces down so don't worry if rotation is unlocked
			RaycastHit2D hit = Physics2D.Raycast(Projectile.transform.position, Vector2.down, 0.5f);

			if (hit.collider != null)
			{
				isOnGround = true;
			}
			else
			{
				isOnGround = false;
			}

			// If the projectile has stopped sliding
			if (Projectile.GetComponent<Rigidbody2D>().velocity.magnitude < 0.1f && isOnGround)
			{
				// If the projectile is a fragile projectile
				if (Projectile.CompareTag("FragileProjectile"))
				{
					// Begin break coroutine
					StartCoroutine(Break());
					isDestroyed = true;
					return;
				}
				// If the projectile is a sturdy projectile
				else if (Projectile.CompareTag("SturdyProjectile"))
				{
					// Set the tag to "Sturdy" and the layer to "Interactive"
					Projectile.tag = "Sturdy";
					Projectile.layer = LayerMask.NameToLayer("Interactive");
				}
			}
			else if ((Projectile.CompareTag("FragileProjectile") || Projectile.CompareTag("SturdyProjectile")) && Collided.CompareTag("Enemy"))
			{ 
				// Check if the collided object has an EnemyControl script, call the Stun method
				if (Collided.GetComponent<EnemyControl>() != null)
				{
					Collided.GetComponent<EnemyControl>().Stun();
				}
			}
		}
	}

	private IEnumerator Break()
	{
		Debug.Log("Breaking");
		Projectile.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 2.5f);
		Projectile.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
		Projectile.GetComponent<Collider2D>().enabled = false;

		yield return new WaitForSeconds(5f);
		Destroy(Projectile);
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		// collision.gameObject is the object that this object collided with.
		// gameobject.tag is the tag of the script's gameobject.		

		// Check and see if this object is a projectile
		if (gameObject.CompareTag("FragileProjectile") || gameObject.CompareTag("SturdyProjectile"))
		{
			// If it is, set the projectile to the collided object
			Projectile = gameObject;
			// Set the collided object to the object that this object collided with
			Collided = collision.gameObject;
		}
	}
}