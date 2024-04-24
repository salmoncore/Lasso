using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackCooldown;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject lassoObj;
	[SerializeField] private GameObject sackObj;
	private Animator anim;
    private PlayerMovement playerMovement;
    private float cooldownTimer = Mathf.Infinity;
	private bool fireFlag = false;
	private bool buttonReleased = true;
	private Vector2 aimDirection;
	private Vector2 movementDirection;
	private PlayerInput playerInput;
	public PauseManager pause;
	private bool flicked;
	public GameObject musicPlayer;
    public AudioManager audioManagerScript;
	public bool levelEntry = true;

	private void Awake()
    {
		anim = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();

		playerInput = GetComponent<PlayerInput>();
		playerInput.onActionTriggered += PlayerInput_onActionTriggered;

		if (GameObject.Find("Entrance") == null)
		{
			levelEntry = false;
		}

		// Disable the sack object by default
		sackObj.SetActive(false);
	}

	private void PlayerInput_onActionTriggered(InputAction.CallbackContext context)
	{
		if (context.action.name == playerInput.actions["Attack"].name)
		{
			if (context.performed && buttonReleased)
			{
				fireFlag = true;
				buttonReleased = false;
			}
			else if (context.canceled)
			{
				buttonReleased = true;
			}
		}

		if (context.action.name == playerInput.actions["Aim"].name)
		{
			aimDirection = context.ReadValue<Vector2>();

			// Flick stick logic, pray
			if (context.performed)
			{
				flicked = true;
			}
			else if (context.canceled)
			{
				flicked = false;
				fireFlag = false;
			}

			if (flicked)
			{
				fireFlag = true;
			}
		}

        if (context.action.name == playerInput.actions["Movement"].name)
		{
			movementDirection = context.ReadValue<Vector2>();
		}
    }

	private void Update()
	{
		if (pause.isPaused) {
			fireFlag = false;
		} else {
			if (fireFlag && (cooldownTimer > attackCooldown) && playerMovement.canAttack() && !levelEntry)
			{
				Attack();
				fireFlag = false;
			}

			cooldownTimer += Time.deltaTime;
		}

		// If the player has a captured enemy, enable the sack object
		if (lassoObj.GetComponent<Projectile>().HasCapturedEnemy())
		{
			sackObj.SetActive(true);
		}
		else
		{
			sackObj.SetActive(false);
		}
    }

	private void Attack()
	{
		cooldownTimer = 0;

		Vector2 attackDirection = GetAttackDirection();
		musicPlayer = GameObject.Find("MusicPlayer");

		if (musicPlayer != null)
		{
			audioManagerScript = musicPlayer.GetComponent<AudioManager>();

			if (audioManagerScript != null)
			{
				audioManagerScript.SFX("lasso");
			}
			else
			{
				Debug.Log("AudioManager script not found");
			}
		}

		if (lassoObj.GetComponent<Projectile>().HasCapturedEnemy())
		{
			anim.SetTrigger("attack");
			anim.SetTrigger("isAttacking");

			lassoObj.GetComponent<Projectile>().ThrowCapturedEnemy(attackDirection);
		}
		else if (LassoReady())
		{
			anim.SetTrigger("attack");
			anim.SetTrigger("isAttacking");

			lassoObj.transform.position = firePoint.position;

			lassoObj.GetComponent<Projectile>().SetDirection(attackDirection);
		}
	}

	private Vector2 GetAttackDirection()
	{
		Vector2 direction = aimDirection; // Left Stick/IJKL takes priority

		if (direction == Vector2.zero) // If no left stick input, use right stick/dpad/WASD
		{
			direction = movementDirection; 
		}

		if (direction == Vector2.zero) // If neither, use the direction the player is facing
		{
			direction = new Vector2(Mathf.Sign(transform.localScale.z), 0); 
		}

		if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y)) // Determine attack direction
		{
			direction.y = 0;
		}
		else
		{
			direction.x = 0;
		}

		return direction;
	}

	private bool LassoReady() 
    { 
        return (lassoObj.activeInHierarchy) ? false : true;
    }
}
