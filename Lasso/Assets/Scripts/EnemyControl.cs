using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class EnemyControl : MonoBehaviour
{
	[SerializeField] private String Class = "CHOOSE Charger/Gunner/Balloonist";
	[SerializeField] private String currentState = "Patrol";
	[SerializeField] private Vector2 boxCastSize = new Vector2 (0.65f, 0.65f); // For use in detecting walls/objects
	[SerializeField] private Vector2 boxCastOffset = new Vector2 (0f, 0f);
	[SerializeField] private Vector2 ledgeCheckSize = new Vector2(0f, 0f); // For use in detecting ledges
	[SerializeField] private Vector2 ledgeCheckOffset = new Vector2(0f, 0f);
	[SerializeField] private Vector2 playerSightSize = new Vector2(0f, 0f); // Detecting player at a distance
	[SerializeField] private Vector2 playerSightOffset = new Vector2(0f, 0f);
	[SerializeField] private Vector2 beginAttackSize = new Vector2(0f, 0f); // When the player is in range for an attack
	[SerializeField] private Vector2 beginAttackOffset = new Vector2(0f, 0f);
	[SerializeField] private float patrolSpeed = 2;
	[SerializeField] private float chargeSpeed = 5;
	[SerializeField] private float acceleration = 0.5f;
	[SerializeField] private float attackDuration = 1.5f;
	[SerializeField] private float aggroTimeDivision = 2f;
	[SerializeField] private bool isStunned = false;
	[SerializeField] private bool ledgeCautious = true;
	[SerializeField] private bool seeThroughObjects = false;
	[SerializeField] private bool noStunDuringAttack = false;
	[SerializeField] private bool breaksSturdyProjectiles = false;
	private Rigidbody2D rb;
    private Animator anim;
    private bool isCrumpled = false;
    private bool waitFlag = false;
    private float patrolDirection = 1;
	private float attackTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        patrolDirection = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
		attackTimer = attackDuration;

		if (patrolDirection > 0)
		{
			transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, -Mathf.Abs(transform.localScale.z));
		}
		else
		{
			transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, Mathf.Abs(transform.localScale.z));
		}
	}

	void Update()
    {
		if (isStunned || isCrumpled) return;

		if (Class != "Charger" && Class != "Gunner" && Class != "Balloonist")
		{
			Debug.LogError("Please choose a valid class for the enemy.");
			Debug.Log("Valid classes are: Charger, Gunner, Balloonist");
			Class = "Charger";
			return;
		}

		if (Class == "Charger")
		{
			switch (currentState)
			{
				case "Patrol":
					Patrol();
					break;
				case "TransitionToRush":
					StartCoroutine(RushTransition());
					break;
				case "Rush":
					Rush();
					break;
				case "TransitionToAttack":
					StartCoroutine(AttackTransition());
					break;
				case "Attack":
					Attack();
					break;
			}
		}
		else if (Class == "Gunner")
		{
			switch (currentState)
			{
				case "Patrol":
					Patrol();
					break;
				case "TransitionToRush":
					StartCoroutine(RushTransition());
					break;
				case "Rush":
					Rush();
					break;
				case "TransitionToAttack":
					StartCoroutine(AttackTransition());
					break;
				case "Attack":
					Attack();
					break;
			}
		}
		else if (Class == "Balloonist")
		{
			switch (currentState)
			{
				case "Patrol":
					Patrol();
					break;
				case "TransitionToRush":
					StartCoroutine(RushTransition());
					break;
				case "Rush":
					Rush();
					break;
				case "TransitionToAttack":
					StartCoroutine(AttackTransition());
					break;
				case "Attack":
					Attack();
					break;
			}
		}
    }

	IEnumerator RushTransition() 
	{
		// TODO: add some telegraph effect here so gamers know they're about to get rushed

		yield return new WaitForSeconds(.75f);

		currentState = "Rush";
		//Debug.Log("Moving to " + currentState + "state.");
	}

	IEnumerator AttackTransition()
	{
		// TODO: also some effects here lmao

		// Jump back a bit before attacking
		rb.velocity = new Vector2(-patrolDirection * patrolSpeed, rb.velocity.y);

		yield return new WaitForSeconds(.75f);
		currentState = "Attack";
		//Debug.Log("Moving to " + currentState + "state.");
	}

    // Patrol: The enemy moves forward until raycast collision with a wall or a ledge. If collision with a wall/ledge, pause, turn around, and continue.
    private void Patrol()
    {
		if (hitPlayer())
		{
			//currentState = "Rush";
			//Debug.Log("Moving to " + currentState + "state.");
		}

		//if (isStunned || isCrumpled || waitFlag) return;
		//
		//if (hitWall() || hitObject() || hitLedge())
		//{
		//    Debug.Log("Hit wall/object!");
		//    patrolDirection *= -1;
		//    StartCoroutine(WaitToTurn(1f));
		//}
		//else
		//{
		//    rb.velocity = new Vector2(patrolSpeed * patrolDirection, rb.velocity.y);
		//}
	}

    // Rush: The enemy pauses for a moment, and then accelerates towards the player's last known position. If the player is in sight, the enemy will rush towards the player.
    private void Rush()
    {
        if (hitPlayer())
        { 
			currentState = "TransitionToAttack";
            //Debug.Log("Moving to " + currentState + "state.");
        }

        if (isStunned || isCrumpled || waitFlag) return;

        if (hitLedge() && ledgeCautious)
        {
			//Debug.Log("Hit ledge!");

			rb.velocity = new Vector2(0, rb.velocity.y);
			StartCoroutine(WaitToTurn(1f));

			currentState = "Patrol";
			//Debug.Log("Moving to " + currentState + "state.");
		}
		else if (hitWall())
		{
			//Debug.Log("Hit wall/object!");

			rb.velocity = new Vector2(0, rb.velocity.y);
			currentState = "Patrol";
			//Debug.Log("Moving to " + currentState + "state.");
		}
		else
        {
			rb.velocity = new Vector2(chargeSpeed * patrolDirection, rb.velocity.y);
		}
    }

    // Attack: The enemy pauses for a moment, and then attacks the player. If the player is in sight, the enemy will attack the player.
	private void Attack()
	{
		if (isStunned || isCrumpled || waitFlag) return;

		Debug.Log("Time: " + attackTimer);

		if ((!hitObject() || !hitWall() || !hitLedge()) && attackTimer > 0)
		{
			float playerDirection = GameObject.Find("Player").transform.position.x - transform.position.x;

			if (playerDirection > 0)
			{
				patrolDirection = 1;
			}
			else
			{
				patrolDirection = -1;
			}

			//if (hitPlayer(sightDistance))
			//{
			//	attackTimer -= Time.deltaTime / aggroTimeDivision; // Enemy attacks longer the longer the player is in sight
			//}
			//else
			//{
			//	attackTimer -= Time.deltaTime;
			//}

			rb.velocity = new Vector2(patrolDirection * patrolSpeed / 2, rb.velocity.y);

			// TODO: Enable hurtbox here
			// TODO: Trigger attack animation here
		}
		else
		{
			currentState = "Patrol";
			attackTimer = attackDuration;
			// TODO: Disable Hurtbox here
			// TODO: Disable animation here
			//Debug.Log("Moving to " + currentState + "state.");
		}
	}

    IEnumerator WaitToTurn(float time)
    {
        waitFlag = true;
        yield return new WaitForSeconds(time);
		flip();
        waitFlag = false;
	}

	private bool hitPlayer() // Uses playerSightSize/Offset for seeing player
	{
		BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
		Vector3 boxcastOrigin = boxCollider.bounds.center + new Vector3(patrolDirection * (playerSightSize.x / 2 + playerSightOffset.x), playerSightOffset.y, 0);

		RaycastHit2D hitPlayer = Physics2D.BoxCast(boxcastOrigin, playerSightSize, 0, new Vector2(patrolDirection, 0), 0, LayerMask.GetMask("Player"));

		if (hitPlayer.collider != null)
		{ 
			RaycastHit2D hitWall = Physics2D.BoxCast(boxcastOrigin, playerSightSize, 0, new Vector2(patrolDirection, 0), 0, LayerMask.GetMask("Ground"));
			RaycastHit2D hitObject = new RaycastHit2D();

			if (!seeThroughObjects)
			{
				hitObject = Physics2D.BoxCast(boxcastOrigin, playerSightSize, 0, new Vector2(patrolDirection, 0), 0, LayerMask.GetMask("Interactive"));
			}

			if ((hitWall.collider == null || hitWall.distance > hitPlayer.distance) && (hitObject.collider == null || hitObject.distance > hitPlayer.distance))
			{
				Debug.Log("Player detected!");
				return true;
			}
		}

		Vector2 topRight = boxcastOrigin + new Vector3(playerSightSize.x / 2, playerSightSize.y / 2, 0);
		Vector2 topLeft = boxcastOrigin + new Vector3(-playerSightSize.x / 2, playerSightSize.y / 2, 0);
		Vector2 bottomRight = boxcastOrigin + new Vector3(playerSightSize.x / 2, -playerSightSize.y / 2, 0);
		Vector2 bottomLeft = boxcastOrigin + new Vector3(-playerSightSize.x / 2, -playerSightSize.y / 2, 0);

		Debug.DrawLine(topLeft, topRight, Color.green); // Top edge
		Debug.DrawLine(topRight, bottomRight, Color.green); // Right edge
		Debug.DrawLine(bottomRight, bottomLeft, Color.green); // Bottom edge
		Debug.DrawLine(bottomLeft, topLeft, Color.green); // Left edge

		return false;
	}

	private bool hitWall()
	{
		BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
		Vector3 boxcastOrigin = boxCollider.bounds.center + new Vector3(patrolDirection * (boxCastSize.x / 2 + boxCastOffset.x), boxCastOffset.y, 0);

		RaycastHit2D hit = Physics2D.BoxCast(boxcastOrigin, boxCastSize, 0, new Vector2(patrolDirection, 0), 0, LayerMask.GetMask("Ground"));

		Vector3 topRight = boxcastOrigin + new Vector3(boxCastSize.x / 2, boxCastSize.y / 2, 0);
		Vector3 topLeft = boxcastOrigin + new Vector3(-boxCastSize.x / 2, boxCastSize.y / 2, 0);
		Vector3 bottomRight = boxcastOrigin + new Vector3(boxCastSize.x / 2, -boxCastSize.y / 2, 0);
		Vector3 bottomLeft = boxcastOrigin + new Vector3(-boxCastSize.x / 2, -boxCastSize.y / 2, 0);

		Debug.DrawLine(topLeft, topRight, Color.red); // Top edge
		Debug.DrawLine(topRight, bottomRight, Color.red); // Right edge
		Debug.DrawLine(bottomRight, bottomLeft, Color.red); // Bottom edge
		Debug.DrawLine(bottomLeft, topLeft, Color.red); // Left edge

		if (hit.collider != null)
		{
			Debug.Log("Hit wall!");
		}

		return hit.collider != null;
	}

	private bool hitObject()
    {
		BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
		Vector3 boxcastOrigin = boxCollider.bounds.center + new Vector3(patrolDirection * (boxCastSize.x / 2 + boxCastOffset.x), boxCastOffset.y, 0);

		RaycastHit2D hit = Physics2D.BoxCast(boxcastOrigin, boxCastSize, 0, new Vector2(patrolDirection, 0), 0, LayerMask.GetMask("Interactive"));

		Vector3 topRight = boxcastOrigin + new Vector3(boxCastSize.x / 2, boxCastSize.y / 2, 0);
		Vector3 topLeft = boxcastOrigin + new Vector3(-boxCastSize.x / 2, boxCastSize.y / 2, 0);
		Vector3 bottomRight = boxcastOrigin + new Vector3(boxCastSize.x / 2, -boxCastSize.y / 2, 0);
		Vector3 bottomLeft = boxcastOrigin + new Vector3(-boxCastSize.x / 2, -boxCastSize.y / 2, 0);

		Debug.DrawLine(topLeft, topRight, Color.red); // Top edge
		Debug.DrawLine(topRight, bottomRight, Color.red); // Right edge
		Debug.DrawLine(bottomRight, bottomLeft, Color.red); // Bottom edge
		Debug.DrawLine(bottomLeft, topLeft, Color.red); // Left edge

		if (hit.collider != null)
		{
			Debug.Log("Hit object!");
		}

		return hit.collider != null;
	}

	private bool hitLedge()
	{
		BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
		Vector3 boxcastOrigin = boxCollider.bounds.center + new Vector3(patrolDirection * (ledgeCheckSize.x / 2 + ledgeCheckOffset.x), ledgeCheckOffset.y, 0);

		RaycastHit2D hit = Physics2D.BoxCast(boxcastOrigin, ledgeCheckSize, 0, new Vector2(patrolDirection, 0), 0, LayerMask.GetMask("Ground"));

		Vector3 topRight = boxcastOrigin + new Vector3(ledgeCheckSize.x / 2, ledgeCheckSize.y / 2, 0);
		Vector3 topLeft = boxcastOrigin + new Vector3(-ledgeCheckSize.x / 2, ledgeCheckSize.y / 2, 0);
		Vector3 bottomRight = boxcastOrigin + new Vector3(ledgeCheckSize.x / 2, -ledgeCheckSize.y / 2, 0);
		Vector3 bottomLeft = boxcastOrigin + new Vector3(-ledgeCheckSize.x / 2, -ledgeCheckSize.y / 2, 0);

		Debug.DrawLine(topLeft, topRight, Color.blue); // Top edge
		Debug.DrawLine(topRight, bottomRight, Color.blue); // Right edge
		Debug.DrawLine(bottomRight, bottomLeft, Color.blue); // Bottom edge
		Debug.DrawLine(bottomLeft, topLeft, Color.blue); // Left edge

		if (hit.collider == null)
		{
			Debug.Log("Ledge detected!");
			return true;
		}

		return false;
	}

	public void Stun()
	{
		if (currentState == "Attack" && noStunDuringAttack) return;

        //Debug.Log("Stunned!");
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
        //Debug.Log("Unstunned!");
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{		
		if (breaksSturdyProjectiles && collision.gameObject.tag == "SturdyProjectile")
        {
            collision.gameObject.tag = "FragileProjectile";
        }

		if (collision.gameObject.tag == "Enemy")
		{
			patrolDirection *= -1;
			flip();
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
				collision.gameObject.tag = "SturdyProjectile";
				collision.gameObject.layer = LayerMask.NameToLayer("Projectiles");
			}
		}

		if (currentState == "Attack")
		{ 
			float collisionDirection = collision.transform.position.x - transform.position.x;

			if (collisionDirection * patrolDirection > 0)
			{
				if (collision.gameObject.tag == "Fragile")
				{
					if (collision.gameObject.GetComponent<LassoHandler>() != null)
					{
						collision.gameObject.GetComponent<LassoHandler>().BreakObject();
					}
				}
				else if (breaksSturdyProjectiles && collision.gameObject.tag == "SturdyProjectile")
				{
					if (collision.gameObject.GetComponent<LassoHandler>() != null)
					{
						collision.gameObject.GetComponent<LassoHandler>().BreakObject();
					}
				}
			}
		}
	}

	private void flip()
    {
		if (TryGetComponent<SpriteRenderer>(out SpriteRenderer spriteRenderer))
		{
			spriteRenderer.flipX = !spriteRenderer.flipX;
		}
		else
		{
			// Flip the transform.localScale.z depending on the patrolDirection

			if (patrolDirection > 0)
			{
				transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, -Mathf.Abs(transform.localScale.z));
			}
			else
			{
				transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, Mathf.Abs(transform.localScale.z));
			}

			//Vector3 localScale = transform.localScale;
			//localScale.z *= -1;
			//transform.localScale = localScale;
		}


		//else if (TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer))
        //{
		//	Vector3 localScale = transform.localScale;
		//	localScale.x *= -1;
		//	transform.localScale = localScale;
		//}

    }
}
