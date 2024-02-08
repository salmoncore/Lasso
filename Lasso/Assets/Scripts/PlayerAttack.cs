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
		if (Input.GetKeyDown(";") && (cooldownTimer > attackCooldown) && playerMovement.canAttack())
        {
            Attack();
        }

        cooldownTimer += Time.deltaTime;
    }

    private void Attack()
    {
        cooldownTimer = 0;

        if (LassoReady())
        {
			anim.SetTrigger("attack");
			lassoObj.transform.position = firePoint.position;
            lassoObj.GetComponent<Projectile>().SetDirection(Mathf.Sign(transform.localScale.x));
        }
    }

    private bool LassoReady() 
    { 
        return (lassoObj.activeInHierarchy) ? false : true;
    }
}
