using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	[SerializeField] private float speed;
	[SerializeField] private float lassoDistance;
	[SerializeField] private Rigidbody2D player;
	private float direction;
	private bool hit;

	private BoxCollider2D boxCollider;
	private Animator anim;

	private void Awake()
	{
		anim = GetComponent<Animator>();
		boxCollider = GetComponent<BoxCollider2D>();
		player = GetComponent<Rigidbody2D>();
	}

	private void Update()
	{
		// Print the player's position for debugging purposes
		if (player != null)
		{
			Debug.Log("Player Position: " + player.position);
		}
		else
		{
			Debug.Log("Player Rigidbody2D is not assigned!");
		}

		if (hit) { return; }

		float movementSpeed = speed * Time.deltaTime * direction;
		transform.Translate(movementSpeed, 0, 0);

		// If player Rigidbody2D is correctly assigned and not null, check the distance
		if (player != null)
		{
			float distanceFromPlayer = Vector2.Distance(player.position, transform.position);
			if (distanceFromPlayer > lassoDistance)
			{
				Deactivate();
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		hit = true;
		boxCollider.enabled = false;
		anim.SetTrigger("Hit");
	}

	public void SetDirection(float _direction)
	{
		direction = _direction;
		gameObject.SetActive(true);
		hit = false;
		boxCollider.enabled = true;

		float localScaleX = transform.localScale.x;

		if (Mathf.Sign(localScaleX) != _direction)
		{
			localScaleX = -localScaleX;
		}

		transform.localScale = new Vector3(localScaleX, transform.localScale.y, transform.localScale.z);
	}

	private void Deactivate()
	{
		boxCollider.enabled = false;
		anim.SetTrigger("Hit");
		gameObject.SetActive(false);
	}
}
