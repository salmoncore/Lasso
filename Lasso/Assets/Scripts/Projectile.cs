using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	[SerializeField] private float speed;
	private float direction;
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
		if (hit) return;
		float movementSpeed = speed * Time.deltaTime * direction;
		transform.Translate(movementSpeed, 0, 0);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		hit = true;
		boxCollider.enabled = false;
		anim.SetTrigger("Thrown");
	}

	public void SetDirection(float _direction)
	{
		direction = _direction;
		gameObject.SetActive(true);
		hit = false;
		boxCollider.enabled = true;
	}
}
