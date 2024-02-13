using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	[SerializeField] private float speed;
	[SerializeField] private float lassoDistance;
	[SerializeField] private Rigidbody2D player;
	private float direction;
	private float verticalDirection;
	private bool hit;
	private Vector2 playerVelocity;

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

		float horizontalMovementSpeed = (speed + playerVelocity.x) * Time.deltaTime * direction;
		float verticalMovementSpeed = (speed + playerVelocity.y) * Time.deltaTime * verticalDirection;

		transform.Translate(horizontalMovementSpeed, verticalMovementSpeed, 0);

		float distanceFromPlayer = Vector2.Distance(player.position, transform.position);

		if (distanceFromPlayer > lassoDistance)
		{
			Deactivate();
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.tag == "Enemy" || collision.tag == "Ground")
		{
			hit = true;
			boxCollider.enabled = false;
			anim.SetTrigger("Hit");
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
