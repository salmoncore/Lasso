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

	private BoxCollider2D boxCollider;
	private Animator anim;

	private void Awake()
	{
		anim = GetComponent<Animator>();
		boxCollider = GetComponent<BoxCollider2D>();
	}

	private void Update()
	{
		if (hit) { return; }

		float movementSpeed = (speed + Mathf.Abs(player.velocity.x)) * Time.deltaTime * direction;
		transform.Translate(movementSpeed, 0, 0);

		Debug.Log("Player Position: " + player.position);
		Debug.Log("Projectile Position: " + transform.position);

		float distanceFromPlayer = Vector2.Distance(player.position, transform.position);

		Debug.Log("Distance: " + distanceFromPlayer);

		if (distanceFromPlayer > lassoDistance)
		{
			Debug.Log("YUP");
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
	}

	private void Deactivate()
	{

		gameObject.SetActive(false);
	}
}
