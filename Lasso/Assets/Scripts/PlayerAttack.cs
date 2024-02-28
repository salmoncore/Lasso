using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackCooldown;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject lassoObj;
	private Animator anim;
    private PlayerMovement playerMovement;
    private float cooldownTimer = Mathf.Infinity;

    private void Awake()
    {
		anim = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
	}

	private void Update()
	{
		if (Input.GetButtonDown("Fire") && (cooldownTimer > attackCooldown) && playerMovement.canAttack())
        {
            Attack();
        }

        cooldownTimer += Time.deltaTime;
    }

	private void Attack()
	{
		cooldownTimer = 0;

		Vector2 attackDirection = GetAttackDirection();

		if (lassoObj.GetComponent<Projectile>().HasCapturedEnemy())
		{
			lassoObj.GetComponent<Projectile>().ThrowCapturedEnemy(attackDirection);
		}
		else if (LassoReady())
		{
			anim.SetTrigger("attack");
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
			direction = new Vector2(Mathf.Sign(transform.localScale.x), 0);
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
