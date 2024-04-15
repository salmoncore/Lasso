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
	[SerializeField] private float patrolSpeed = 2; // Speed of the enemy when patrolling
	[SerializeField] private float chargeSpeed = 5; // Speed of the enemy when charging. Used primarily for Charger class.
	[SerializeField] private float acceleration = 0.5f; // TODO: Implement acceleration 
	[SerializeField] private float attackDuration = 1.5f; // Duration of the attack for the charger class.
	[SerializeField] private float aggroTimeDivision = 2f; // How much longer the enemy attacks the player the longer the player is in sight. Used for Charger class.
	[SerializeField] private float gunnerSightRange = 10f; // Range at which the Gunner can see the player
	[SerializeField] private float gunnerFleeRange = 5f; // Range at which the Gunner will flee from the player
	[SerializeField] private float gunnerDelayToFire = 2f; // TODO: Fix this. Delay before the gunner can shoot at the player.
	[SerializeField] private float gunnerCooldown = 5f;	// Cooldown between shots for the Gunner
	[SerializeField] private float gunnerFleeTime = 4f; // The amount of time the Gunner will flee from the player
	[SerializeField] private bool gunnerFaceLeft = true; // Determines if the Gunner faces left or right on spawn
	[SerializeField] private bool isStunned = false; // Determines if the enemy is stunned
	[SerializeField] private bool ledgeCautious = true; // Determines if the enemy will stop at ledges
	[SerializeField] private bool seeThroughObjects = false; // Determines if the enemy can see through objects on the "Interactive" layer
	[SerializeField] private bool noStunDuringAttack = false; // Determines if the enemy can be stunned during an attack
	[SerializeField] private bool breaksSturdyProjectiles = false; // Determines if the Charger can break sturdy projectiles while attacking
	private Rigidbody2D rb;
    private Animator anim;
    private bool isCrumpled = false; // TODO: Remove or revise this.
    private bool waitFlag = false;
	private bool gunnerOnCooldown = false;
    private float patrolDirection = 1; // Determines the direction the enemy is moving. -1 is left, 1 is right.
	private float attackTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

		// If it's the Charger or Balloonist, choose a random direction to patrol in. The gunner will have the starting direction defined in the flags.
		if (Class == "Charger" || Class == "Balloonist")
		{ 
			patrolDirection = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
		}
		else
		{
			patrolDirection = -1;
		}

		attackTimer = attackDuration;

		// Flips the enemy so they always face the direction they're moving in.
		if (patrolDirection > 0)
		{
			transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, -Mathf.Abs(transform.localScale.z));
		}
		else
		{
			transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, Mathf.Abs(transform.localScale.z));
		}

		// Check if the class is valid. If not, default to Charger.
		if (Class != "Charger" && Class != "Gunner" && Class != "Balloonist")
		{
			Debug.LogError("Please choose a valid class for the enemy.");
			Debug.Log("Valid classes are: Charger, Gunner, Balloonist");
			Class = "Charger";
			return;
		}

		// Initializing variables based on the class of the enemy.
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
			// Flip the Gunner if they're supposed to face left
			if (!gunnerFaceLeft)
			{
				transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
			}
			Cooldown(gunnerDelayToFire);

			boxCastSize = new Vector2(1f, 1.169f);
			boxCastOffset = new Vector2(0.14f, 0.04f);
			ledgeCheckSize = new Vector2(0.1f, 0.51f);
			ledgeCheckOffset = new Vector2(0.28f, -0.69f);
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
		// // For Determining box sizes lmao
		//}
		//return;

		// If the enemy is stunned or crumpled, don't update the enemy logic until they recover.
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
				case "TakeAim":
					TakeAim();
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
		if (waitFlag) return;

		// Check if the player is in the Flee or Sight range, and switch to either Flee or TakeAim states, respectively.
		if (lookoutPlayer(gunnerFleeRange))
		{
			//Debug.Log("Fleeing!");
			currentState = "Flee";
			return;
		} 
		else if (lookoutPlayer(gunnerSightRange))
		{
			//Debug.Log("Shooting!");
			Cooldown(gunnerDelayToFire);
			currentState = "TakeAim";
		}
	}

	// For Gunner. Enemy moves away from the player for a set amount of time, or until the enemy hits a wall or object.
	// If the gunner gets cornered, they will shoot at the player.
	private void Flee()
	{
		if (waitFlag) return;

		// Check if the player is still in sight
		if (lookoutPlayer(gunnerFleeRange))
		{
			// If the player is still in sight, continue fleeing
			rb.velocity = new Vector2(-patrolDirection * chargeSpeed, rb.velocity.y);
		}
		else
		{
			// If the player is no longer in sight, stop fleeing and return to the lookout state
			rb.velocity = new Vector2(0, rb.velocity.y);
			currentState = "Lookout";
		}

		// Get the direction of the player
		Vector3 playerPosition = GameObject.Find("Player").GetComponent<CapsuleCollider2D>().bounds.center;

		// Determine the direction to move in to flee from the player
		if (playerPosition.x < transform.position.x)
		{
			patrolDirection = 1;
			transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
		}
		else
		{
			patrolDirection = -1;
			transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
		}

		// If there is no wall or object in the way, continue moving away from the player
		if (!hitWall() && !hitObject() && !hitLedge())
		{
			rb.velocity = new Vector2(patrolDirection * chargeSpeed, rb.velocity.y);
		}
		else
		{
			// If there is a wall or object in the way, shoot at the player
			rb.velocity = new Vector2(0, rb.velocity.y);
			Shoot();
		}
	}

	// For Gunner. Enemy shoots at the player if the player is in sight. Transitions from Lookout state.
	private void TakeAim()
	{
		if (lookoutPlayer(gunnerFleeRange))
		{
			//Debug.Log("Fleeing!");
			currentState = "Flee";
			return;
		}

		// Flip to face the direction of the player as the gunner aims.
		if (GameObject.Find("Player").transform.position.x < transform.position.x)
		{
			transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
		}
		else
		{
			transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
		}

		Shoot();

		// If the player is no longer in sight, return to the Lookout state.
		if (!lookoutPlayer(gunnerSightRange))
		{
			currentState = "Lookout";
		}
	}

	// For Gunner. Handles firing and cooldowns for the gunner. Called in the TakeAim state.
	private void Shoot()
	{
		if (gunnerOnCooldown || waitFlag) return;

		// Instantiate the bullet prefab at the firePoint's position
		Transform firePoint = transform.Find("firePoint");

		// Error checking, else instantiate the bullet and set its velocity in the direction of the player.
		if (firePoint == null)
		{
			Debug.Log("Failed to get firepoint. Is enemyControl set to gunner?");
			return;
		}
		else
		{
			// Set bullet to velocity of 10 in the direction of the center of the player's collider
			GameObject bulletInstance = Instantiate(bullet, firePoint.position, Quaternion.identity);
			Vector3 playerPosition = GameObject.Find("Player").GetComponent<CapsuleCollider2D>().bounds.center;
			Vector3 direction = playerPosition - firePoint.position;
			bulletInstance.GetComponent<Rigidbody2D>().velocity = direction.normalized * 10;
		}

		// Start the cooldown for the gunner
		StartCoroutine(Cooldown(gunnerCooldown));
	}

	IEnumerator Cooldown(float time)
	{
		gunnerOnCooldown = true;
		yield return new WaitForSeconds(time);
		gunnerOnCooldown = false;
	}

	// For Gunner. Checks if the player is in sight of the Gunner. Uses playerSightSize/Offset for seeing player through floors & walls.
	private bool lookoutPlayer(float viewDistance) // Uses playerSightSize/Offset for seeing player through floors & walls
	{
		// Get the position of the enemy's child object, firePoint, to use as the origin of the raycast.
		Transform firePoint = transform.Find("firePoint");

		// Get the location of the player to use as the direction of the raycast. Use the center of the player's collider.
		Vector3 playerPosition = GameObject.Find("Player").GetComponent<CapsuleCollider2D>().bounds.center;

		// Error checks
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

			// If seeThroughObjects is true, ignore objects in the way, else use raycast to check for interactive.
			if (!seeThroughObjects)
			{
				hitObject = Physics2D.Raycast(firePoint.position, playerPosition - firePoint.position, distance, LayerMask.GetMask("Interactive"));
			}

			// If there is a wall in the way, return false.
			if (hitWall.collider != null)
			{
				//Debug.Log("Wall detected!");
				return false;
			}

			// If there is an object in the way, but seeThroughObjects is true, return true.
			if (hitObject.collider != null && seeThroughObjects)
			{
				//Debug.Log("Object detected, but seeThroughObjects is true!");
				return true;
			}

			// If there is an object in the way, but seeThroughObjects is false, return false.
			if (hitObject.collider != null && !seeThroughObjects)
			{
				//Debug.Log("Object detected!");
				return false;
			}

			// If there is no wall or object in the way, return true.
			//Debug.Log("Player detected!");
			return true;
		}
		
		// If the raycast does not hit the player or the player is out of range, return false.
		return false;
	}

	// For debugging, supposed to be blank. Lol.
	private void Oopsie()
	{
		// Current state isn't set up in editor?
		Debug.Log("Make sure to set the Current State in the editor!");
	}

	// For Charger. Brief transition before rushing towards the player.
	IEnumerator RushTransition() 
	{
		// TODO: add some telegraph effect here so gamers know they're about to get rushed

		yield return new WaitForSeconds(.75f);

		currentState = "Rush";
		//Debug.Log("Moving to " + currentState + "state.");
	}

	// For Charger. Brief transition before attacking the player.
	IEnumerator AttackTransition()
	{
		// TODO: also some effects here lmao

		// Jump back a bit before attacking
		rb.velocity = new Vector2(-patrolDirection * patrolSpeed, rb.velocity.y);

		yield return new WaitForSeconds(.75f);
		currentState = "Attack";
		//Debug.Log("Moving to " + currentState + "state.");
	}

    // Charger moves forward until raycast collision with a wall or a ledge. If collision with a wall/ledge, pause, turn around, and continue.
    private void Patrol()
    {
		anim.SetBool("isWalking", true);
		anim.SetBool("isRunning", false);

		// If the player is in sight, transition to the Rush state.
		if (hitPlayer())
		{
			currentState = "Rush";
			//Debug.Log("Moving to " + currentState + "state.");
		}
		
		if (isStunned || isCrumpled || waitFlag) return;
		
		// If the enemy hits a wall, object, or ledge, turn around.
		if (hitWall() || hitObject() || hitLedge())
		{
		    //Debug.Log("Hit wall/object!");

			anim.SetBool("isWalking", false); // stop the walking animation for the turn, resume it after.
			anim.SetBool("isRunning", false);
			anim.SetBool("isIdle", true);

		    patrolDirection *= -1;
		    StartCoroutine(WaitToTurn(1f));

			anim.SetBool("isIdle", false); // these and the ones above it don't seem to work for turning.
			anim.SetBool("isWalking", true);
		}
		else // Otherwise, continue walking.
		{
		    rb.velocity = new Vector2(patrolSpeed * patrolDirection, rb.velocity.y);
		}
	}

    // Charger pauses for a moment, and then accelerates towards the player's last known position. If the player is in sight, the enemy will rush towards the player.
    private void Rush()
    {
		// Checks if the player is in the attack range, and transitions to the Attack state if true.
        if (attackPlayer())
        { 
			currentState = "TransitionToAttack";
            //Debug.Log("Moving to " + currentState + "state.");
        }

        if (isStunned || isCrumpled || waitFlag) return;

		// Ledge checking during the rush, dependant on if the enemy is ledge cautious.
        if (hitLedge() && ledgeCautious)
        {
			//Debug.Log("Hit ledge!");

			rb.velocity = new Vector2(0, rb.velocity.y);
			StartCoroutine(WaitToTurn(1f));

			currentState = "Patrol";
			//Debug.Log("Moving to " + currentState + "state.");
		}
		else if (hitWall()) // Always checks for walls, will switch to patrol if true.
		{
			//Debug.Log("Hit wall/object!");

			rb.velocity = new Vector2(0, rb.velocity.y);
			currentState = "Patrol";
			//Debug.Log("Moving to " + currentState + "state.");
		}
		else // Otherwise, continue rushing.
        {
			rb.velocity = new Vector2(chargeSpeed * patrolDirection, rb.velocity.y);
		}
    }

    // Charger attacks the player. If the player remains in the attack range, the enemy will attack for a longer time.
	private void Attack()
	{
		if (isStunned || isCrumpled || waitFlag) return;

		//Debug.Log("Time: " + attackTimer);

		// while attacking, the brute should sprint.
		anim.SetBool("isWalking", false);
		anim.SetBool("isRunning", true);

		// If the charger hasn't hit anything and the attack timer hasn't run out, continue attacking.
		if ((!hitObject() || !hitWall() || !hitLedge()) && attackTimer > 0)
		{
			// Get the player's position constantly and figure out what direction to "patrol" in while attacking, really just the direction to attack in here.
			float playerDirection = GameObject.Find("Player").transform.position.x - transform.position.x;

			if (playerDirection > 0)
			{
				patrolDirection = 1;
			}
			else
			{
				patrolDirection = -1;
			}

			// This is the check where it sees if the player is still in the attack range, in which case the attack timer is extended.
			if (attackPlayer())
			{
				attackTimer -= Time.deltaTime / aggroTimeDivision; // Enemy attacks longer the longer the player is in sight
			}
			else // If the player is out of the attack range, the attack timer is reduced.
			{
				attackTimer -= Time.deltaTime;
			}
			
			// Charger moves in the direction of the player while attacking, at half patrol speed.
			rb.velocity = new Vector2(patrolDirection * patrolSpeed / 2, rb.velocity.y);

			// TODO: Enable hurtbox here
			// TODO: Trigger attack animation here
			// unsure if this is actually where it should go. it works a little wonky.
			// arthur note: hm, noted. um I suppose it could be done in the check for the attackPlayer() function? 
			// anim.SetTrigger("isAttacking");
			anim.SetTrigger("isAttacking");
		}
		else
		{
			currentState = "Patrol";
			attackTimer = attackDuration;
		}
	}

	// For Charger. Flips the enemy after a set amount of time. Used after hitting a wall or object.
    IEnumerator WaitToTurn(float time)
    {
        waitFlag = true;
        yield return new WaitForSeconds(time);
		flip();
        waitFlag = false;
	}

	// For charger. Determines if the player is in the attack range, using beginAttackSize/Offset values for range. Note that these may be initialized in code.
	private bool attackPlayer()
	{

		if (waitFlag || isStunned || isCrumpled) return false;

		// Produces the actual boxcast for checking if the player is in range for an attack.
		CapsuleCollider2D capsuleCollider = GetComponent<CapsuleCollider2D>();
		Vector3 boxcastOrigin = capsuleCollider.bounds.center + new Vector3(patrolDirection * (beginAttackSize.x / 2 + beginAttackOffset.x), beginAttackOffset.y, 0);

		RaycastHit2D hitPlayer = Physics2D.BoxCast(boxcastOrigin, beginAttackSize, 0, new Vector2(patrolDirection, 0), 0, LayerMask.GetMask("Player"));

		// If the player is within the attack range, check for walls and objects in the way.
		if (hitPlayer.collider != null)
		{

			RaycastHit2D hitWall = Physics2D.BoxCast(boxcastOrigin, beginAttackSize, 0, new Vector2(patrolDirection, 0), 0, LayerMask.GetMask("Ground"));
			RaycastHit2D hitObject = new RaycastHit2D();

			if (!seeThroughObjects) // Checks, depending on the flag, if the enemy's view is obstructed by interactive objects.
			{
				hitObject = Physics2D.BoxCast(boxcastOrigin, beginAttackSize, 0, new Vector2(patrolDirection, 0), 0, LayerMask.GetMask("Interactive"));
			}


			anim.SetTrigger("isAttacking");

			// If no wall is detected or the player is closer than the wall, and if the player is closer than the object or no object is detected, begin attack.
			if ((hitWall.collider == null || hitWall.distance > hitPlayer.distance) && (hitObject.collider == null || hitObject.distance > hitPlayer.distance))
			{
				Debug.Log("Player detected! (in AttackPlayer)");
				return true;
			}
		}
		
		// Debug lines for the boxcast, that's all.
		Vector2 topRight = boxcastOrigin + new Vector3(beginAttackSize.x / 2, beginAttackSize.y / 2, 0);
		Vector2 topLeft = boxcastOrigin + new Vector3(-beginAttackSize.x / 2, beginAttackSize.y / 2, 0);
		Vector2 bottomRight = boxcastOrigin + new Vector3(beginAttackSize.x / 2, -beginAttackSize.y / 2, 0);
		Vector2 bottomLeft = boxcastOrigin + new Vector3(-beginAttackSize.x / 2, -beginAttackSize.y / 2, 0);

		Debug.DrawLine(topLeft, topRight, Color.yellow); // Top edge
		Debug.DrawLine(topRight, bottomRight, Color.yellow); // Right edge
		Debug.DrawLine(bottomRight, bottomLeft, Color.yellow); // Bottom edge
		Debug.DrawLine(bottomLeft, topLeft, Color.yellow); // Left edge

		// Some variety of conditions weren't met, so not attacking.
		return false;
	}
	
	// For charger. This name is really misleading, it's just checking if the boxcast is "hitting" the player at a distance for determining when to rush.
	private bool hitPlayer()
	{
		if (waitFlag || isStunned || isCrumpled) return false;

		// Produces the actual boxcast for checking if the player is in range for an attack.
		CapsuleCollider2D capsuleCollider = GetComponent<CapsuleCollider2D>();
		Vector3 boxcastOrigin = capsuleCollider.bounds.center + new Vector3(patrolDirection * (playerSightSize.x / 2 + playerSightOffset.x), playerSightOffset.y, 0);

		RaycastHit2D hitPlayer = Physics2D.BoxCast(boxcastOrigin, playerSightSize, 0, new Vector2(patrolDirection, 0), 0, LayerMask.GetMask("Player"));

		// If the player is within the sight range, check for walls and objects in the way.
		if (hitPlayer.collider != null)
		{ 
			RaycastHit2D hitWall = Physics2D.BoxCast(boxcastOrigin, playerSightSize, 0, new Vector2(patrolDirection, 0), 0, LayerMask.GetMask("Ground"));
			RaycastHit2D hitObject = new RaycastHit2D();

			if (!seeThroughObjects) // Checks, depending on the flag, if the enemy's view is obstructed by interactive objects.
			{
				hitObject = Physics2D.BoxCast(boxcastOrigin, playerSightSize, 0, new Vector2(patrolDirection, 0), 0, LayerMask.GetMask("Interactive"));
			}

			// If no wall is detected or the player is closer than the wall, and if the player is closer than the object or no object is detected, begin rushing.
			if ((hitWall.collider == null || hitWall.distance > hitPlayer.distance) && (hitObject.collider == null || hitObject.distance > hitPlayer.distance))
			{
				Debug.Log("Player detected! (in hitPlayer)");
				anim.SetBool("isRunning", true);
				anim.SetBool("isWalking", false);
				return true;
			}
		}

		// More debug lines for the boxcast.
		Vector2 topRight = boxcastOrigin + new Vector3(playerSightSize.x / 2, playerSightSize.y / 2, 0);
		Vector2 topLeft = boxcastOrigin + new Vector3(-playerSightSize.x / 2, playerSightSize.y / 2, 0);
		Vector2 bottomRight = boxcastOrigin + new Vector3(playerSightSize.x / 2, -playerSightSize.y / 2, 0);
		Vector2 bottomLeft = boxcastOrigin + new Vector3(-playerSightSize.x / 2, -playerSightSize.y / 2, 0);

		Debug.DrawLine(topLeft, topRight, Color.green); // Top edge
		Debug.DrawLine(topRight, bottomRight, Color.green); // Right edge
		Debug.DrawLine(bottomRight, bottomLeft, Color.green); // Bottom edge
		Debug.DrawLine(bottomLeft, topLeft, Color.green); // Left edge

		// Some variety of conditions weren't met, so not rushing.
		return false;
	}

	// For both the charger and the gunner, just checks to see if there is a wall within the boxcast. Returns true if there is.
	private bool hitWall()
	{
		CapsuleCollider2D capsuleCollider = GetComponent<CapsuleCollider2D>();
		Vector3 boxcastOrigin = capsuleCollider.bounds.center + new Vector3(patrolDirection * (boxCastSize.x / 2 + boxCastOffset.x), boxCastOffset.y, 0);

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
			//Debug.Log("Hit wall!");
		}

		return hit.collider != null;
	}

	// For both the charger and the gunner, just checks to see if there is an object within the boxcast. Returns true if there is.
	private bool hitObject()
    {
		CapsuleCollider2D capsuleCollider = GetComponent<CapsuleCollider2D>();
		Vector3 boxcastOrigin = capsuleCollider.bounds.center + new Vector3(patrolDirection * (boxCastSize.x / 2 + boxCastOffset.x), boxCastOffset.y, 0);

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
			//Debug.Log("Hit object!");
		}

		return hit.collider != null;
	}

	// For both the charger and the gunner, just checks to see if there is a ledge within the boxcast. Returns true if there is.
	private bool hitLedge()
	{
		CapsuleCollider2D capsuleCollider = GetComponent<CapsuleCollider2D>();
		Vector3 boxcastOrigin = capsuleCollider.bounds.center + new Vector3(patrolDirection * (ledgeCheckSize.x / 2 + ledgeCheckOffset.x), ledgeCheckOffset.y, 0);

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

	// Public method for all enemies, called from the LassoHandler script's update method under certain conditions. 
	public void Stun()
	{
		// For the Charger, if the enemy is attacking and the noStun flag is true, they cannot be stunned.
		if (currentState == "Attack" && noStunDuringAttack) return;

		// Sets the isStunned flag to true for the duration of the StunTimer, currently set to 2 seconds.
        isStunned = true;
        gameObject.tag = "StunnedEnemy";

		// play the stunned animation so long as they're stunned
		Debug.Log("right before isStunned anim tag");

        StartCoroutine(StunTimer());

		// After coming out of being stunned, the charger returns to patrolling, the gunner to lookout, and the balloonist to whatever state they'll end up in by default lol.
		if (Class == "Charger")
		{
			currentState = "Patrol";
		}
		else if (Class == "Gunner")
		{
			currentState = "Lookout";
		}
		else if (Class == "Balloon")
		{
			// Set up Balloonist-specific variables here
		}
	}

	// The time should really be enum'd or something, but for now it's just a hardcoded 2 seconds.
    IEnumerator StunTimer()
    {
		anim.SetBool("isStunned", true);
		yield return new WaitForSeconds(2);
		anim.SetBool("isStunned", false);

        isStunned = false;
        gameObject.tag = "Enemy";
		Debug.Log("Enemy no longer stunned!");
	}

	// Handles collisions with the enemy.
	private void OnCollisionEnter2D(Collision2D collision)
	{	
		// For the charger. If breaksSturdyProjectiles is true, the charger can break sturdy projectiles while attacking.
		if (breaksSturdyProjectiles && collision.gameObject.tag == "SturdyProjectile")
        {
            collision.gameObject.tag = "FragileProjectile";
        }

		// If colliding with an enemy, just flip the patrol direction. Should probably be using a boxcast for this since they get so close, but that's for later.
		if (collision.gameObject.tag == "Enemy")
		{
			patrolDirection *= -1;
			flip();
		}

		// If the charger is in the rush state and collides with a fragile object, break it from the LassoHandler script.
		if (currentState == "Rush" && collision.gameObject.tag == "Fragile")
		{
			if (collision.gameObject.GetComponent<LassoHandler>() != null)
			{
				collision.gameObject.GetComponent<LassoHandler>().BreakObject();
			}
		} // If the charger is in the rush state and collides with a sturdy object, change it to a sturdy projectile.
		else if (currentState == "Rush" && collision.gameObject.tag == "Sturdy")
		{
			if (collision.gameObject.GetComponent<LassoHandler>() != null)
			{
				collision.gameObject.tag = "SturdyProjectile";
				collision.gameObject.layer = LayerMask.NameToLayer("Projectiles");
			}
		}

		// If the charger is in the attack state and collides with a fragile object, break it.
		// If it's a sturdy object, break it if breaksSturdyProjectiles is true.
		if (currentState == "Attack")
		{ 
			float collisionDirection = collision.transform.position.x - transform.position.x;

			if (collisionDirection * patrolDirection > 0)
			{
				if (collision.gameObject.tag == "Fragile")
				{
					if (collision.gameObject.GetComponent<LassoHandler>() != null)
					{
						collision.gameObject.GetComponent<LassoHandler>().BreakObject(); // ps, breaking occurs in LassoHandler script.
					}
				}
				else if (breaksSturdyProjectiles && collision.gameObject.tag == "SturdyProjectile")
				{
					if (collision.gameObject.GetComponent<LassoHandler>() != null)
					{
						collision.gameObject.GetComponent<LassoHandler>().BreakObject(); // Breaking still occurs in LassoHandler script.
					}
				}
			}
		}
	}

	// Just flips the enemy by setting the scale.z to the negative of itself.
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
