using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class EnemyControl : MonoBehaviour
{
    public GameObject pointA;
    public GameObject pointB;
    private Rigidbody2D rb;
    private Animator anim;
    private Transform currentPoint;
    public float speed;
    private bool isStunned = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentPoint = pointB.transform;
        anim.SetBool("isRunning", true);
    }

    void Update()
    {
        Vector2 point = currentPoint.position - transform.position;

        if (isStunned)
        {
			return;
		}

        if(currentPoint == pointB.transform)
        {
            rb.velocity = new Vector2(speed, 0);
        }
        else
        {
            rb.velocity = new Vector2(-speed, 0);
        }

        if ((Vector2.Distance(transform.position, currentPoint.position) < 1f) && (currentPoint == pointB.transform))
        {
            flip();
            currentPoint = pointA.transform;
        }

        if ((Vector2.Distance(transform.position, currentPoint.position) < 1f) && (currentPoint == pointA.transform))
        {
            flip();
            currentPoint = pointB.transform;
        }
    }

	public void Stun()
	{
        Debug.Log("Stunned");
        isStunned = true;
        rb.velocity = new Vector2(0, 0);
        anim.SetBool("isRunning", false);
        StartCoroutine(StunTimer());
	}

    IEnumerator StunTimer()
    {
		yield return new WaitForSeconds(2);
		anim.SetBool("isRunning", true);
        isStunned = false;
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {

    }

    private void flip()
    {
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(pointA.transform.position, 0.5f);
        Gizmos.DrawWireSphere(pointB.transform.position, 0.5f);
        Gizmos.DrawLine(pointA.transform.position, pointB.transform.position);  
    }
}
