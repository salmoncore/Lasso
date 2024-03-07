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
	private Animator anim;
    private PlayerMovement playerMovement;
    private float cooldownTimer = Mathf.Infinity;
	private bool fireFlag = false;
	private bool buttonReleased = true;

	private PlayerInput playerInput;

	private void Awake()
    {
		anim = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();

		playerInput = GetComponent<PlayerInput>();
		playerInput.onActionTriggered += PlayerInput_onActionTriggered;
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
	}

	private void Update()
	{
		if (fireFlag && (cooldownTimer > attackCooldown) && playerMovement.canAttack())
        {
            Attack();
			fireFlag = false;
        }

        cooldownTimer += Time.deltaTime;
    }

	private void Attack()
	{
		cooldownTimer = 0;

		Vector2 attackDirection = GetAttackDirection();

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
		Vector2 direction = new Vector2(Input.GetAxis("RightStickHorizontal"), Input.GetAxis("RightStickVertical"));

		if (direction == Vector2.zero)
		{
			direction = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
		}

		if (direction == Vector2.zero)
		{
			direction = new Vector2(Mathf.Sign(transform.localScale.z), 0);
		}

		if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
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
