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
	private Vector2 playerPosition;
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
			capturedEnemy.tag = "EnemyProjectile";
			capturedEnemy.GetComponent<SpriteRenderer>().enabled = true;
			capturedEnemy.SetActive(true);
			capturedEnemy.transform.position = player.position;
			capturedEnemy.GetComponent<Collider2D>().enabled = true;

			Rigidbody2D enemyRigidbody = capturedEnemy.GetComponent<Rigidbody2D>();

			enemyRigidbody.velocity = newDirection.normalized * speed + player.velocity;

			// Make sure to attach the LassoHandler script to the enemy!
			LassoHandler handler = capturedEnemy.AddComponent<LassoHandler>();
			handler.Initialize(speed);

			capturedEnemy.layer = LayerMask.NameToLayer("ThrownEnemies");
			capturedEnemy.GetComponent<BoxCollider2D>().isTrigger = false;

			capturedEnemy = null;

			player.velocity = -newDirection.normalized * kickback;
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.tag == "Ground")
		{
			lassoTimer = lassoFlightTime;
			hit = true;
			boxCollider.enabled = false;
			anim.SetTrigger("Hit");
		}
		else if (collision.tag == "Enemy")
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
