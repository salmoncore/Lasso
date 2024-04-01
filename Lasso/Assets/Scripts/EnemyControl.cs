using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class EnemyControl : MonoBehaviour
{
	public GameObject bullet;
	[SerializeField] private String Class = "CHOOSE Charger/Gunner/Balloonist";
	[SerializeField] private String currentState = "Patrol";
	[SerializeField] private Vector2 boxCastSize = new Vector2 (0f, 0f); // For use in detecting walls/objects
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
	[SerializeField] private float gunnerSightRange = 10f;
	[SerializeField] private float gunnerFleeRange = 5f;
	[SerializeField] private float gunnerDelayToFire = 2f;
	[SerializeField] private float gunnerCooldown = 5f;
	[SerializeField] private bool gunnerFaceLeft = true;
	[SerializeField] private bool isStunned = false;
	[SerializeField] private bool ledgeCautious = true;
	[SerializeField] private bool seeThroughObjects = false;
	[SerializeField] private bool noStunDuringAttack = false;
	[SerializeField] private bool breaksSturdyProjectiles = false;
	private Rigidbody2D rb;
    private Animator anim;
    private bool isCrumpled = false;
    private bool waitFlag = false;
	private bool gunnerOnCooldown = false;
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

		if (Class != "Charger" && Class != "Gunner" && Class != "Balloonist")
		{
			Debug.LogError("Please choose a valid class for the enemy.");
			Debug.Log("Valid classes are: Charger, Gunner, Balloonist");
			Class = "Charger";
			return;
		}

		if (Class == "Charger")
		{
			boxCastSize = new Vector2(0.59f, 1.36f);
			boxCastOffset = new Vector2(0.27f, 0.02f);
			ledgeCheckSize = new Vector2(0.05f, 0.5f);
			ledgeCheckOffset = new Vector2(0.34f, -0.75f);
			playerSightSize = new Vector2(5f, 1f);
			playerSightOffset = new Vector2(0.28f, 0f);
			beginAttackSize = new Vector2(1.5f, 2f);
			beginAttackOffset = new Vector2(0f, 0f);
		}
		else if (Class == "Gunner")
		{
			if (!gunnerFaceLeft)
			{
				transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
			}
			Cooldown(gunnerDelayToFire);
		}
		else if (Class == "Balloonist")
		{
			// Set up Balloonist-specific variables here
		}
	}

	void Update()
    {

		//if (hitLedge() || hitWall() || hitObject() || hitPlayer() || attackPlayer())
		//{
		// // For Debugging lmao
		//}
		//return;

		if (isStunned || isCrumpled) return;

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
				case "Lookout":
					Lookout();
					break;
				case "Shoot":
					Shoot();
					break;
				case "Flee":
					Flee();
					break;
			}
		}
		else if (Class == "Balloonist")
		{
			switch (currentState)
			{
				case "":
					Oopsie();
					break;
			}
		}
    }

	// For Gunner. Enemy stays idle until something from the player layer comes into view.
	private void Lookout()
	{
		if (isStunned || isCrumpled || waitFlag) return;

		if (lookoutPlayer(gunnerFleeRange))
		{
			Debug.Log("Fleeing!");
		}
		else if (lookoutPlayer(gunnerSightRange))
		{
			Debug.Log("Shooting!");
			Cooldown(gunnerDelayToFire);
			currentState = "Shoot";
		}
	}

	private void Flee()
	{
		if (isStunned || isCrumpled || waitFlag) return;

		// Flee from the player
	}

	private void Shoot()
	{ 
		if (gunnerOnCooldown || isStunned || isCrumpled || waitFlag) return;

		// Instantiate the bullet prefab at the firePoint's position
		Transform firePoint = transform.Find("firePoint");
		if (firePoint == null)
		{
			Debug.Log("Failed to get firepoint. Is enemyControl set to gunner?");
			return;
		}
		else
		{
			// Set bullet to velocity of 10 in the direction of the center of the player's collider
			GameObject bulletInstance = Instantiate(bullet, firePoint.position, Quaternion.identity);
			Vector3 playerPosition = GameObject.Find("Player").GetComponent<BoxCollider2D>().bounds.center;
			Vector3 direction = playerPosition - firePoint.position;
			bulletInstance.GetComponent<Rigidbody2D>().velocity = direction.normalized * 10;
		}

		StartCoroutine(Cooldown(gunnerCooldown));

		if (!lookoutPlayer(gunnerSightRange))
		{
			currentState = "Lookout";
		}
	}

	IEnumerator Cooldown(float time)
	{
		gunnerOnCooldown = true;
		yield return new WaitForSeconds(time);
		gunnerOnCooldown = false;
	}

	private bool lookoutPlayer(float viewDistance) // Uses playerSightSize/Offset for seeing player through floors & walls
	{
		// Use a raycast to the player's position from the enemy's position to determine if the player is in sight.
		// If there is a wall in the way, return false.
		// If there is an object in the way, but seeThroughObjects is true, return true.
		// If there is an object in the way, but seeThroughObjects is false, return false.
		// If there is no wall or object in the way, return true.

		// Get the position of the enemy's child object, firePoint, to use as the origin of the raycast.
		Transform firePoint = transform.Find("firePoint");

		// Get the location of the player to use as the direction of the raycast. Use the center of the player's collider.
		Vector3 playerPosition = GameObject.Find("Player").GetComponent<BoxCollider2D>().bounds.center;

		if (firePoint == null)
		{
			Debug.Log("Failed to get firepoint. Is enemyControl set to gunner?");
			return false;
		}

		if (playerPosition == null)
		{
			Debug.Log("Failed to get player position. Make sure the player is named \"Player\" lol.");
			return false;
		}

		// Draw debug raycast from the firePoint to the player's position.
		Debug.DrawRay(firePoint.position, playerPosition - firePoint.position, Color.yellow);

		// Get the distance between the firePoint and the player's position.
		float distance = Vector3.Distance(firePoint.position, playerPosition);

		// Cast a ray from the firePoint to the player's position, checking for the player layer.
		RaycastHit2D hitPlayer = Physics2D.Raycast(firePoint.position, playerPosition - firePoint.position, distance, LayerMask.GetMask("Player"));

		// If the raycast hits the player, check for walls and objects in the way. Additionally, check if the player is within the viewDistance.
		if (hitPlayer.collider != null && distance <= viewDistance)
		{
			// Cast a ray from the firePoint to the player's position, checking for the ground layer.
			RaycastHit2D hitWall = Physics2D.Raycast(firePoint.position, playerPosition - firePoint.position, distance, LayerMask.GetMask("Ground"));

			// Cast a ray from the firePoint to the player's position, checking for the interactive layer.
			RaycastHit2D hitObject = new RaycastHit2D();

			if (!seeThroughObjects)
			{
				hitObject = Physics2D.Raycast(firePoint.position, playerPosition - firePoint.position, distance, LayerMask.GetMask("Interactive"));
			}

			// If there is a wall in the way, return false.
			if (hitWall.collider != null)
			{
				Debug.Log("Wall detected!");
				return false;
			}

			// If there is an object in the way, but seeThroughObjects is true, return true.
			if (hitObject.collider != null && seeThroughObjects)
			{
				Debug.Log("Object detected, but seeThroughObjects is true!");
				return true;
			}

			// If there is an object in the way, but seeThroughObjects is false, return false.
			if (hitObject.collider != null && !seeThroughObjects)
			{
				Debug.Log("Object detected!");
				return false;
			}

			// If there is no wall or object in the way, return true.
			Debug.Log("Player detected!");
			return true;
		}
		
		// If the raycast does not hit the player or the player is out of range, return false.
		return false;
	}

	private void Oopsie()
	{
		// Current state isn't set up in editor?
		Debug.Log("Make sure to set the Current State in the editor!");
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
		
		// Animator: trying to define on spawn a walking animation starts. It currently does not work. 
		anim.SetBool("isWalking", true);
		anim.SetBool("isRunning", false);
		anim.SetBool("isIdle", false);

		if (hitPlayer())
		{
			currentState = "Rush";
			//Debug.Log("Moving to " + currentState + "state.");
		}
		
		if (isStunned || isCrumpled || waitFlag) return;
		
		if (hitWall() || hitObject() || hitLedge())
		{
		    Debug.Log("Hit wall/object!");

			anim.SetBool("isWalking", false); // stop the walking animation for the turn, resume it after.
			anim.SetBool("isIdle", true);

		    patrolDirection *= -1;
		    StartCoroutine(WaitToTurn(1f));

			anim.SetBool("isIdle", false); // these and the ones above it don't seem to work for turning.
			anim.SetBool("isWalking", true);
		}
		else
		{
		    rb.velocity = new Vector2(patrolSpeed * patrolDirection, rb.velocity.y);
		}
	}

    // Rush: The enemy pauses for a moment, and then accelerates towards the player's last known position. If the player is in sight, the enemy will rush towards the player.
    private void Rush()
    {
        if (attackPlayer())
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

		// while attacking, the brute should sprint.
		anim.SetBool("isWalking", false);
		anim.SetBool("isRunning", true);

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

			if (attackPlayer())
			{
				attackTimer -= Time.deltaTime / aggroTimeDivision; // Enemy attacks longer the longer the player is in sight
			}
			else
			{
				attackTimer -= Time.deltaTime;
			}

			rb.velocity = new Vector2(patrolDirection * patrolSpeed / 2, rb.velocity.y);

			// TODO: Enable hurtbox here
			// TODO: Trigger attack animation here
				// unsure if this is actually where it should go. it works a little wonky.
			anim.SetTrigger("isAttacking");
		}
		else
		{
			currentState = "Patrol";
			attackTimer = attackDuration;
			// TODO: Disable Hurtbox here
			// TODO: Disable animation here
				// attacking is a trigger, don't need to disable it.
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

	private bool attackPlayer() // Uses beginAttackSize/Offset for determining when to enter the attack state
	{
		if (waitFlag || isStunned || isCrumpled) return false;

		BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
		Vector3 boxcastOrigin = boxCollider.bounds.center + new Vector3(patrolDirection * (beginAttackSize.x / 2 + beginAttackOffset.x), beginAttackOffset.y, 0);

		RaycastHit2D hitPlayer = Physics2D.BoxCast(boxcastOrigin, beginAttackSize, 0, new Vector2(patrolDirection, 0), 0, LayerMask.GetMask("Player"));
		anim.SetTrigger("isAttacking");

		if (hitPlayer.collider != null)
		{
			RaycastHit2D hitWall = Physics2D.BoxCast(boxcastOrigin, beginAttackSize, 0, new Vector2(patrolDirection, 0), 0, LayerMask.GetMask("Ground"));
			RaycastHit2D hitObject = new RaycastHit2D();

			if (!seeThroughObjects)
			{
				hitObject = Physics2D.BoxCast(boxcastOrigin, beginAttackSize, 0, new Vector2(patrolDirection, 0), 0, LayerMask.GetMask("Interactive"));
			}

			if ((hitWall.collider == null || hitWall.distance > hitPlayer.distance) && (hitObject.collider == null || hitObject.distance > hitPlayer.distance))
			{
				Debug.Log("Player detected!");
				return true;
			}
		}

		Vector2 topRight = boxcastOrigin + new Vector3(beginAttackSize.x / 2, beginAttackSize.y / 2, 0);
		Vector2 topLeft = boxcastOrigin + new Vector3(-beginAttackSize.x / 2, beginAttackSize.y / 2, 0);
		Vector2 bottomRight = boxcastOrigin + new Vector3(beginAttackSize.x / 2, -beginAttackSize.y / 2, 0);
		Vector2 bottomLeft = boxcastOrigin + new Vector3(-beginAttackSize.x / 2, -beginAttackSize.y / 2, 0);

		Debug.DrawLine(topLeft, topRight, Color.yellow); // Top edge
		Debug.DrawLine(topRight, bottomRight, Color.yellow); // Right edge
		Debug.DrawLine(bottomRight, bottomLeft, Color.yellow); // Bottom edge
		Debug.DrawLine(bottomLeft, topLeft, Color.yellow); // Left edge

		return false;
	}
	
	private bool hitPlayer() // Uses playerSightSize/Offset for seeing player
	{
		if (waitFlag || isStunned || isCrumpled) return false;

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

				anim.SetBool("isRunning", true);
				anim.SetBool("isWalking", false);

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
		}
    }
}
