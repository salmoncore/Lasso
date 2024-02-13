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

            // Get direction from right stick or IJKL
            Vector2 attackDirection = GetAttackDirectionFromInput();
            lassoObj.transform.position = firePoint.position;

            // Set the direction of the projectile
            lassoObj.GetComponent<Projectile>().SetDirection(attackDirection);
        }
    }

    private Vector2 GetAttackDirectionFromInput()
    {
        // Right Stick Input
        float horizontal = Input.GetAxis("RightStickHorizontal");
        float vertical = Input.GetAxis("RightStickVertical");

        // IJKL Input (Checking RS Deadzone)
        if (Mathf.Approximately(horizontal, 0f) && Mathf.Approximately(vertical, 0f))
        { 
            horizontal = (Input.GetKey(KeyCode.L) ? 1 : 0) - (Input.GetKey(KeyCode.J) ? 1 : 0);
            vertical = (Input.GetKey(KeyCode.I) ? 1 : 0) - (Input.GetKey(KeyCode.K) ? 1 : 0);
        }

        return new Vector2(horizontal, vertical);
    }

    private bool LassoReady() 
    { 
        return (lassoObj.activeInHierarchy) ? false : true;
    }
}
