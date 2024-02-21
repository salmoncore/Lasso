using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	[SerializeField] private float speed;
	[SerializeField] private float lassoDistance;
	[SerializeField] private Rigidbody2D player;
	[SerializeField] private float enemyTravelTime = 1f;
	private GameObject capturedEnemy = null;
	private float direction;
	private float verticalDirection;
	private bool hit;
	private Vector2 playerVelocity;
	private Vector2 playerPosition;

	private BoxCollider2D boxCollider;
	private Animator anim;

	private void Awake()
	{
		anim = GetComponent<Animator>();
		boxCollider = GetComponent<BoxCollider2D>();
	}

	private void Update()
	{
		if (hit) return;

		playerPosition = player.position;

		float horizontalMovementSpeed = (speed + playerVelocity.x) * Time.deltaTime * direction;
		float verticalMovementSpeed = (speed + playerVelocity.y) * Time.deltaTime * verticalDirection;

		transform.Translate(horizontalMovementSpeed, verticalMovementSpeed, 0);

		float distanceFromPlayer = Vector2.Distance(player.position, transform.position);

		if (distanceFromPlayer > lassoDistance)
		{
			Deactivate();
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
			capturedEnemy.tag = "EnemyProjectile";
			capturedEnemy.GetComponent<SpriteRenderer>().enabled = true;
			capturedEnemy.SetActive(true);
			capturedEnemy.transform.position = player.position;
			capturedEnemy.GetComponent<Collider2D>().enabled = true;

			Rigidbody2D enemyRigidbody = capturedEnemy.GetComponent<Rigidbody2D>();
			enemyRigidbody.velocity = newDirection.normalized * speed;

			// TODO: Attach LassoHandler to enemies
			LassoHandler handler = capturedEnemy.AddComponent<LassoHandler>();
			handler.Initialize(speed);

			// Set the enemy's layer to "ThrownEnemies" so it can't collide with the player and other enemies
			capturedEnemy.layer = LayerMask.NameToLayer("ThrownEnemies");
			capturedEnemy.GetComponent<BoxCollider2D>().isTrigger = false;

			capturedEnemy = null;
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.tag == "Ground")
		{
			hit = true;
			boxCollider.enabled = false;
			anim.SetTrigger("Hit");
		}
		else if (collision.tag == "Enemy")
		{
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
			
			playerPosition = player.position;
			enemy.transform.position = Vector2.Lerp(startPosition, playerPosition, t / enemyTravelTime);
			yield return null;
		}

		enemy.transform.position = player.position;

		enemy.GetComponent<SpriteRenderer>().enabled = false;
		enemy.SetActive(false);
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

		this.direction = direction.x;
		this.verticalDirection = direction.y;

		this.playerVelocity.x = Mathf.Abs(player.velocity.x);
		this.playerVelocity.y = Mathf.Abs(player.velocity.y);
	}

	private void Deactivate()
	{
		gameObject.SetActive(false);
	}
}
