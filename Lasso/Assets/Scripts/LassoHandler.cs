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
		if (collision.gameObject.CompareTag("Ground"))
		{
			Vector2 normal = collision.GetContact(0).normal;
			if (Mathf.Abs(normal.y) <= 0.1f)
			{
				Rigidbody2D rb = GetComponent<Rigidbody2D>();
				rb.velocity = new Vector2(0, rb.velocity.y);

				rb.gravityScale = 1;
			}
		}
	}

	void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.CompareTag("Enemy"))
		{
			if (collision.gameObject.TryGetComponent<EnemyControl>(out EnemyControl enemyControl))
			{
				enemyControl.Stun();
			}			

			gameObject.tag = "CrumpledEnemy";
			gameObject.layer = LayerMask.NameToLayer("CrumpledEnemies");
			Rigidbody2D rb = GetComponent<Rigidbody2D>();
			rb.velocity = Vector2.zero;
			rb.gravityScale = 1;
		}
	}

}