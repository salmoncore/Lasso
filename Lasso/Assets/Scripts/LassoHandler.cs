using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LassoHandler : MonoBehaviour
{
	private GameObject Projectile;
	private GameObject Collided;
	private bool isOnGround;
	private bool isOnInteractive;
	private bool isOnEnemy;

	public void Initialize(float speed)
	{
		Projectile = null;
		Collided = null;
		isOnGround = false;
	}

	private void Update()
	{
		if (!(Projectile == null) && !(Collided == null))
		{
			// PS the raycast lines always face down so don't worry if rotation is unlocked

			if (Physics2D.Raycast(Projectile.transform.position, Vector2.down, 0.5f, LayerMask.GetMask("Ground")).collider != null)
			{ isOnGround = true; }
			else { isOnGround = false; }

			if (Physics2D.Raycast(Projectile.transform.position, Vector2.down, 0.5f, LayerMask.GetMask("Interactive")).collider != null)
			{ isOnInteractive = true; }
			else { isOnInteractive = false;	}

			if (Physics2D.Raycast(Projectile.transform.position, Vector2.down, 0.5f, LayerMask.GetMask("Enemy")).collider != null)
			{ isOnEnemy = true;	}
			else { isOnEnemy = false; }

			// If the projectile has stopped sliding
			if (Projectile.GetComponent<Rigidbody2D>().velocity.magnitude < 0.1f && (isOnGround || isOnInteractive || isOnEnemy))
			{
				if (Projectile.CompareTag("FragileProjectile"))
				{
					StartCoroutine(Break());
				}
				else if (Projectile.CompareTag("SturdyProjectile"))
				{
					Projectile.tag = "Sturdy";
					Projectile.layer = LayerMask.NameToLayer("Interactive");
					Reset();
				}
			}
			else if ((Projectile.CompareTag("FragileProjectile") || Projectile.CompareTag("SturdyProjectile")) && Collided.CompareTag("Enemy"))
			{
				Collided.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
				Collided.GetComponent<Rigidbody2D>().gravityScale = 1.5f;
				Collided.GetComponent<BoxCollider2D>().isTrigger = false;

				// Check if the collided object has an EnemyControl script, call the Stun method
				if (Collided.GetComponent<EnemyControl>() != null)
				{
					Collided.GetComponent<EnemyControl>().Stun();
				}
				Reset();
			}
		}
	}

	private IEnumerator Break()
	{
		Projectile.tag = "Breaking";
		Projectile.layer = LayerMask.NameToLayer("Breaking");
		Projectile.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 2.5f);
		Projectile.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
		Projectile.GetComponent<Collider2D>().enabled = false;

		yield return new WaitForSeconds(5f);
		Destroy(Projectile);
	}

	public void Reset()
	{
		Projectile = null;
		Collided = null;
		isOnGround = false;
		isOnInteractive = false;
		isOnEnemy = false;
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		// collision.gameObject is the object that this object collided with.
		// gameobject.tag is the tag of the script's gameobject.		

		if (gameObject.CompareTag("FragileProjectile") || gameObject.CompareTag("SturdyProjectile"))
		{
			Projectile = gameObject;
			Collided = collision.gameObject;
		}
		else
		{
			Reset();
		}
	}
}