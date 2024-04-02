using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class LassoHandler : MonoBehaviour
{
	[SerializeField] public bool canGrabWithoutStunning = false;
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

			float distance = Projectile.GetComponent<BoxCollider2D>().bounds.extents.y;

			// debug raycast
			Debug.DrawRay(Projectile.transform.position, Vector2.down * (0.5f + distance), Color.red);

			if (Physics2D.Raycast(Projectile.transform.position, Vector2.down, 0.5f + distance, LayerMask.GetMask("Ground")).collider != null)
			{ isOnGround = true; }
			else { isOnGround = false; }

			if (Physics2D.Raycast(Projectile.transform.position, Vector2.down, 0.5f + distance, LayerMask.GetMask("Interactive")).collider != null)
			{ isOnInteractive = true; }
			else { isOnInteractive = false;	}

			if (Physics2D.Raycast(Projectile.transform.position, Vector2.down, 0.5f + distance, LayerMask.GetMask("Enemies")).collider != null)
			{ isOnEnemy = true;	}
			else { isOnEnemy = false; }

			if ((Projectile.CompareTag("FragileProjectile") || Projectile.CompareTag("SturdyProjectile")) && (Collided.CompareTag("Enemy") || Collided.CompareTag("StunnedEnemy")))
			{
				// These shouldn't be needed anymore?
				//Collided.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
				//Collided.GetComponent<Rigidbody2D>().gravityScale = 2.5f;
				//Collided.GetComponent<BoxCollider2D>().isTrigger = false;

				// Check if the collided object has an EnemyControl script, call the Stun method
				if (Collided.GetComponent<EnemyControl>() != null)
				{
					Collided.GetComponent<EnemyControl>().Stun();
				}
			}

			// If the projectile has stopped sliding
			if (Projectile.GetComponent<Rigidbody2D>().velocity.magnitude < 5f && (isOnGround || isOnInteractive || isOnEnemy))
			{
				if (Projectile.CompareTag("FragileProjectile") || Projectile.CompareTag("StunnedEnemy") || Projectile.CompareTag("Enemy"))
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
		}
	}

	private IEnumerator Break()
	{
		Projectile.tag = "Breaking";
		Projectile.layer = LayerMask.NameToLayer("Breaking");
		Projectile.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 2.5f);

		if (Projectile.TryGetComponent<SpriteRenderer>(out SpriteRenderer spriteRenderer))
		{
			spriteRenderer.color = new Color(1, 1, 1, 0.5f);
		}
		else
		{
			// Changing the alpha doesn't seem to work here so I'm gonna do this silly thing
			StartCoroutine(SpinProjectile());
		}

		Projectile.GetComponent<Collider2D>().enabled = false;

		yield return new WaitForSeconds(5f);
		Destroy(Projectile);
	}

	private IEnumerator SpinProjectile()
	{
		// pick a random rotation speed for each axis to spin the object

		Vector3 RotateSpecs = new Vector3(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(-3f, 3f));
		if (RotateSpecs.x < 0.5f) { RotateSpecs.x = 0.5f; }
		if (RotateSpecs.y < 0.5f) { RotateSpecs.y = 0.5f; }
		if (RotateSpecs.z < 0.5f) { RotateSpecs.z = 0.5f; }

		while (true)
		{
			Projectile.transform.Rotate(RotateSpecs);
			yield return new WaitForSeconds(0.01f);
		}
	}

	public void BreakObject()
	{
		Projectile = gameObject;
		StartCoroutine(Break());
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
		//else if (gameObject.CompareTag("Enemy"))
        //{
        //    
        //}
        else
        {
            Reset();
        }
    }
}