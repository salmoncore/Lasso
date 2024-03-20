using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class EnemyControl : MonoBehaviour
{
	[SerializeField] private float patrolSpeed;
    private Rigidbody2D rb;
    private Animator anim;
    private Transform currentPoint;
    private bool isStunned = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {

    }

	public void Stun()
	{
        Debug.Log("Stunned!");
        isStunned = true;
        gameObject.tag = "StunnedEnemy";
        StartCoroutine(StunTimer());
	}

    IEnumerator StunTimer()
    {
		yield return new WaitForSeconds(2);
        isStunned = false;
        gameObject.tag = "Enemy";
        Debug.Log("Unstunned!");
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag == "SturdyProjectile")
        {
            collision.gameObject.tag = "FragileProjectile";
        }
	}

	private void flip()
    {
        if (TryGetComponent<SpriteRenderer>(out SpriteRenderer spriteRenderer))
        {
			spriteRenderer.flipX = !spriteRenderer.flipX;
		}
		else if (TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer))
        {
			Vector3 localScale = transform.localScale;
			localScale.x *= -1;
			transform.localScale = localScale;
		}
    }
}
