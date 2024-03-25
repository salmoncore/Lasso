using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class EnemyControl : MonoBehaviour
{
    [SerializeField] private float boxCastSize = 0.65f;
    [SerializeField] private float ledgeCheckDistance = 0.5f;
	[SerializeField] private float patrolSpeed;
    private Rigidbody2D rb;
    private Animator anim;
    private bool isStunned = false;
    private bool isCrumpled = false;
    private float patrolDirection = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        patrolDirection = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
    }

    void Update()
    {
        Patrol();
    }

    // Patrol: The enemy moves forward until raycast collision with a wall or a ledge. If collision with a wall/ledge, pause, turn around, and continue.
    private void Patrol()
    {
		if (isStunned || isCrumpled) return;
		
		if (hitWall() || hitObject() || hitLedge())
        {
            Debug.Log("Hit wall/object!");
            patrolDirection *= -1;
			flip();
		}
        else
        {
            rb.velocity = new Vector2(patrolSpeed * patrolDirection, rb.velocity.y);
        }
	}

    // Rush: The enemy pauses for a moment, and then accelerates towards the player's last known position. If the player is in sight, the enemy will rush towards the player.
    // Attack: The enemy pauses for a moment, and then attacks the player. If the player is in sight, the enemy will attack the player.

    private bool hitWall()
    {
        // idk why this doesn't just work so i'm going to enumerate the size instead, good luck lol
        //Vector2 boxcastSize = GetComponent<BoxCollider2D>().size;

        Vector2 boxcastSize = new Vector2(boxCastSize, boxCastSize);

        RaycastHit2D hit = Physics2D.BoxCast(transform.position, boxcastSize, 0, new Vector2(patrolDirection, 0), 1f, LayerMask.GetMask("Ground"));

        // For testing the boxcast fit lol
        Debug.DrawRay(transform.position, new Vector2(patrolDirection, 0) * 1f, Color.red);
        Debug.DrawRay(transform.position + new Vector3(0, boxcastSize.y, 0), new Vector2(patrolDirection, 0) * 1f, Color.red);
        Debug.DrawRay(transform.position - new Vector3(0, boxcastSize.y, 0), new Vector2(patrolDirection, 0) * 1f, Color.red);
        return hit.collider != null;
	}

    private bool hitObject()
    {
		Vector2 boxcastSize = new Vector2(boxCastSize, boxCastSize);

		RaycastHit2D hit = Physics2D.BoxCast(transform.position, boxcastSize, 0, new Vector2(patrolDirection, 0), 1f, LayerMask.GetMask("Interactive"));

		// For testing the boxcast fit lol
		Debug.DrawRay(transform.position, new Vector2(patrolDirection, 0) * 1f, Color.red);
		Debug.DrawRay(transform.position + new Vector3(0, boxcastSize.y, 0), new Vector2(patrolDirection, 0) * 1f, Color.red);
		Debug.DrawRay(transform.position - new Vector3(0, boxcastSize.y, 0), new Vector2(patrolDirection, 0) * 1f, Color.red);
		return hit.collider != null;
	}

	private bool hitLedge()
	{
		float horizontalOffset = patrolDirection * (boxCastSize / 2 + 0.1f);
		float verticalOffset = -boxCastSize / 2; // tweak later

		Vector2 startPosition = (Vector2)transform.position + new Vector2(horizontalOffset, verticalOffset);

		Vector2 direction = Vector2.down;
		RaycastHit2D hit = Physics2D.Raycast(startPosition, direction, ledgeCheckDistance, LayerMask.GetMask("Ground"));

		Debug.DrawRay(startPosition, direction * ledgeCheckDistance, Color.blue);

		return hit.collider == null;
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
