using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	[SerializeField] private float speed;
	[SerializeField] private float kickback;
	[SerializeField] private float lassoFlightTime;
	[SerializeField] private Rigidbody2D player;
	[SerializeField] private float enemyTravelTime = 1f;
	private GameObject capturedEnemy = null;
	private float lateralDirection;
	private float verticalDirection;
	private bool hit;
	private Vector2 playerVelocity;
	private float lassoTimer;

	private BoxCollider2D boxCollider;
	private Animator anim;

	private void Awake()
	{
		anim = GetComponent<Animator>();
		boxCollider = GetComponent<BoxCollider2D>();
		lassoTimer = lassoFlightTime;
	}

	private void Update()
	{
		if (hit) return;

		Vector2 movement = new Vector2(speed * lateralDirection, speed * verticalDirection);
		movement += player.velocity;
		transform.Translate(movement * Time.deltaTime);

		lassoTimer -= Time.deltaTime;
		if (lassoTimer <= 0)
		{
			Deactivate();
			lassoTimer = lassoFlightTime;
		}
	}

	public bool HasCapturedEnemy()
	{
		return capturedEnemy != null;
	}

	public void ThrowCapturedEnemy(Vector2 newDirection)
	{
		if (capturedEnemy != null)
		{
			if (capturedEnemy.tag == "Fragile" || capturedEnemy.tag == "Enemy")
			{
				capturedEnemy.tag = "FragileProjectile";
			}
			else if (capturedEnemy.tag == "Sturdy")
			{
				capturedEnemy.tag = "SturdyProjectile";
			}
			capturedEnemy.layer = LayerMask.NameToLayer("Projectiles");

			if (capturedEnemy.GetComponent<SpriteRenderer>() != null)
			{
				capturedEnemy.GetComponent<SpriteRenderer>().enabled = true;
			}
			else if (capturedEnemy.GetComponent<MeshRenderer>() != null)
			{
				capturedEnemy.GetComponent<MeshRenderer>().enabled = true;
			}

			capturedEnemy.SetActive(true);
			capturedEnemy.transform.position = GetFirePoint();

			capturedEnemy.GetComponent<Collider2D>().enabled = true;

			capturedEnemy.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;

			Rigidbody2D enemyRigidbody = capturedEnemy.GetComponent<Rigidbody2D>();

			enemyRigidbody.velocity = newDirection.normalized * speed + player.velocity;

			if (!capturedEnemy.TryGetComponent<LassoHandler>(out LassoHandler handler))
			{
				handler = capturedEnemy.AddComponent<LassoHandler>();
				handler.Initialize(speed);
			}

			// If it exists, disable the EnemyControl script
			if (capturedEnemy.TryGetComponent<EnemyControl>(out EnemyControl enemyControl))
			{
				enemyControl.enabled = false;
			}

			// May not be necessary anymore, just in case
			capturedEnemy.GetComponent<BoxCollider2D>().isTrigger = false; 

			// Apply gravity scale 1 to thrown enemy, may not be necessary anymore
			enemyRigidbody.gravityScale = 1.5f; // Make this enumerable

			capturedEnemy = null;

			// Projectile Kickback
			player.velocity = -newDirection.normalized * kickback;
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		Debug.Log("Projectile hit " + collision.tag);
		// PS the lasso projectile is the trigger here lmao
		if (collision.tag == "Ground")
		{
			lassoTimer = lassoFlightTime;
			hit = true;
			boxCollider.enabled = false;
			anim.SetTrigger("Hit");
		}
		else if ((collision.tag == "Enemy" || collision.tag == "Fragile" || collision.tag == "Sturdy" ||
				 collision.tag == "FragileProjectile" || collision.tag == "SturdyProjectile") && capturedEnemy == null)
		{
			lassoTimer = lassoFlightTime;
			hit = true;
			boxCollider.enabled = false;
			anim.SetTrigger("Hit");

			capturedEnemy = collision.gameObject;
			StartCoroutine(CaptureEnemy(capturedEnemy));
		}
	}

	private IEnumerator CaptureEnemy(GameObject enemy)
	{
		enemy.GetComponent<Collider2D>().enabled = false;

		// Hitstun?

		Vector2 startPosition = enemy.transform.position;

		float t = 0f;
		while (t < enemyTravelTime)
		{
			t += Time.deltaTime;
			
			enemy.transform.position = Vector2.Lerp(startPosition, GetFirePoint(), t / enemyTravelTime);
			yield return null;
		}

		enemy.transform.position = player.position;

		if (enemy.GetComponent<SpriteRenderer>() != null)
		{
			enemy.GetComponent<SpriteRenderer>().enabled = false;
		}
		else if (enemy.GetComponent<MeshRenderer>() != null)
		{
			enemy.GetComponent<MeshRenderer>().enabled = false;
		}

		enemy.SetActive(false);
	}

	public Vector2 GetFirePoint()
	{
		Transform firePoint = player.transform.Find("firePoint");
		if (firePoint != null)
		{
			return firePoint.position;
		}
		else
		{
			Debug.LogError("firePoint not found. Check Unity inspector.");
			return player.position;
		}
	}

	public void SetDirection(Vector2 direction)
	{
		direction.Normalize();

		gameObject.SetActive(true);
		hit = false;
		boxCollider.enabled = true;

		float localScaleX = transform.localScale.x;
		if (Mathf.Sign(localScaleX) != direction.x)
		{
			localScaleX = -localScaleX;
		}
		transform.localScale = new Vector3(localScaleX, transform.localScale.y, transform.localScale.z);

		this.lateralDirection = direction.x;
		this.verticalDirection = direction.y;

		this.playerVelocity.x = Mathf.Abs(player.velocity.x);
		this.playerVelocity.y = Mathf.Abs(player.velocity.y);
	}

	private void Deactivate()
	{
		gameObject.SetActive(false);
	}
}
