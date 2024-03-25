using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class EnemyControl : MonoBehaviour
{
    [SerializeField] private float boxCastSize = 0.65f;
    [SerializeField] private float ledgeCheckDistance = 0.5f;
	[SerializeField] private float patrolSpeed = 2;
    [SerializeField] private float sightDistance = 15f;
    [SerializeField] private float chargeSpeed = 5;
	[SerializeField] private float acceleration = 0.5f;
	[SerializeField] private bool ledgeCautious = true;
	private String currentState = "Patrol";
	private Rigidbody2D rb;
    private Animator anim;
    private bool isStunned = false;
    private bool isCrumpled = false;
    private bool waitFlag = false;
    private float patrolDirection = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        patrolDirection = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
    }

    void Update()
    {
        // Make a state machine for the enemy. By default, the enemy will patrol. If the player is in sight, the enemy will rush towards the player. 
		// If the player is in attack range, the enemy will attack the player. If the enemy is stunned, the enemy will not move. If the enemy is crumpled, the enemy will not move.

		if (isStunned || isCrumpled) return;

		if (currentState == "Patrol")
		{
			Patrol();
		}
		else if (currentState == "TransitionToRush")
		{
			StartCoroutine(RushTransition());
		}
		else if (currentState == "Rush")
		{
			Rush();
		}
    }

	IEnumerator RushTransition() 
	{
		yield return new WaitForSeconds(.75f);
		currentState = "Rush";
	}

    // Patrol: The enemy moves forward until raycast collision with a wall or a ledge. If collision with a wall/ledge, pause, turn around, and continue.
    private void Patrol()
    {
        if (hitPlayer())
        {
			Debug.Log("Player in sight!");
			currentState = "Rush";
		}

		if (isStunned || isCrumpled || waitFlag) return;
		
		if (hitWall() || hitObject() || hitLedge())
        {
            Debug.Log("Hit wall/object!");
            patrolDirection *= -1;
            StartCoroutine(WaitToTurn(1f));
		}
        else
        {
            rb.velocity = new Vector2(patrolSpeed * patrolDirection, rb.velocity.y);
        }
	}

    // Rush: The enemy pauses for a moment, and then accelerates towards the player's last known position. If the player is in sight, the enemy will rush towards the player.
    private void Rush()
    {
        //if (inAttackRange())
        //{ 
        //    Debug.Log("Attack player!");
        //    // Transition to Attack State 
        //}

        if (isStunned || isCrumpled || waitFlag) return;

        if (hitLedge() && ledgeCautious)
        {
			Debug.Log("Hit ledge!");

			rb.velocity = new Vector2(0, rb.velocity.y);
			StartCoroutine(WaitToTurn(1f));

			currentState = "Patrol";
		}
		else if (hitWall() )
		{
			Debug.Log("Hit wall/object!");

			rb.velocity = new Vector2(0, rb.velocity.y);
			currentState = "Patrol";
		}
		else
        {
			rb.velocity = new Vector2(chargeSpeed * patrolDirection, rb.velocity.y);
		}
    }

    // Attack: The enemy pauses for a moment, and then attacks the player. If the player is in sight, the enemy will attack the player.

    IEnumerator WaitToTurn(float time)
    {
        waitFlag = true;
        yield return new WaitForSeconds(time);
		flip();
        waitFlag = false;
	}

	private bool hitPlayer()
	{
		Vector2 boxcastSize = new Vector2(boxCastSize, boxCastSize);

		RaycastHit2D hitPlayer = Physics2D.BoxCast(transform.position, boxcastSize, 0, new Vector2(patrolDirection, 0), sightDistance, LayerMask.GetMask("Player"));

		if (hitPlayer.collider != null)
		{
			RaycastHit2D hitWall = Physics2D.Raycast(transform.position, hitPlayer.point - (Vector2)transform.position, Vector2.Distance(transform.position, hitPlayer.point), LayerMask.GetMask("Ground"));
			RaycastHit2D hitObject = Physics2D.Raycast(transform.position, hitPlayer.point - (Vector2)transform.position, Vector2.Distance(transform.position, hitPlayer.point), LayerMask.GetMask("Interactive"));

			if ((hitWall.collider == null || hitWall.distance > hitPlayer.distance) && (hitObject.collider == null || hitObject.distance > hitPlayer.distance))
			{
				Debug.DrawRay(transform.position, new Vector2(patrolDirection, 0) * hitPlayer.distance, Color.green);
				return true;
			}
		}

		Debug.DrawRay(transform.position, new Vector2(patrolDirection, 0) * sightDistance, Color.red);
		return false;
	}


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

	private bool hitSturdyObject()
	{
		// cast a raycast. if it hits something with the tag "Fragile", return true
		Vector2 boxcastSize = new Vector2(boxCastSize, boxCastSize);

		RaycastHit2D hit = Physics2D.BoxCast(transform.position, boxcastSize, 0, new Vector2(patrolDirection, 0), 1f, LayerMask.GetMask("Interactive"));

		// For testing the boxcast fit lol
		Debug.DrawRay(transform.position, new Vector2(patrolDirection, 0) * 1f, Color.red);
		Debug.DrawRay(transform.position + new Vector3(0, boxcastSize.y, 0), new Vector2(patrolDirection, 0) * 1f, Color.red);
		Debug.DrawRay(transform.position - new Vector3(0, boxcastSize.y, 0), new Vector2(patrolDirection, 0) * 1f, Color.red);
		return hit.transform.CompareTag("Sturdy");
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
		currentState = "Patrol";
	}

    IEnumerator StunTimer()
    {
		yield return new WaitForSeconds(2);
        isStunned = false;
        gameObject.tag = "Enemy";
        Debug.Log("Unstunned!");
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{		
		if (collision.gameObject.tag == "SturdyProjectile")
        {
            collision.gameObject.tag = "FragileProjectile";
        }

		if (currentState == "Rush" && collision.gameObject.tag == "Fragile")
		{
			if (collision.gameObject.GetComponent<LassoHandler>() != null)
			{
				collision.gameObject.GetComponent<LassoHandler>().BreakObject();
			}
		}
		else if (currentState == "Rush" && collision.gameObject.tag == "Sturdy")
		{
			if (collision.gameObject.GetComponent<LassoHandler>() != null)
			{
				Stun();
				collision.gameObject.tag = "SturdyProjectile";
				collision.gameObject.layer = LayerMask.NameToLayer("Projectiles");
			}
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
