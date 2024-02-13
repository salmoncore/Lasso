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
		float horizontal = Input.GetAxis("RightStickHorizontal");
		float vertical = Input.GetAxis("RightStickVertical");

		bool isRightStickInDeadzone = Mathf.Approximately(horizontal, 0f) && Mathf.Approximately(vertical, 0f);
		bool noKeyboardInput = !Input.GetKey(KeyCode.I) && !Input.GetKey(KeyCode.J) && !Input.GetKey(KeyCode.K) && !Input.GetKey(KeyCode.L);

		Vector2 direction = Vector2.zero;

		if (!isRightStickInDeadzone)
		{
			// Determine the direction of the joystick input
			if (Mathf.Abs(horizontal) > Mathf.Abs(vertical))
			{
				direction = new Vector2(Mathf.Sign(horizontal), 0);
			}
			else if (Mathf.Abs(vertical) > Mathf.Abs(horizontal))
			{
				direction = new Vector2(0, Mathf.Sign(vertical));
			}
			else if (!Mathf.Approximately(horizontal, 0f))
			{
				direction = new Vector2(Mathf.Sign(horizontal), 0);
			}
		}
		else if (noKeyboardInput)
		{
			direction = new Vector2(Mathf.Sign(transform.localScale.x), 0);
		}
		else
		{
			horizontal = (Input.GetKey(KeyCode.L) ? 1 : 0) - (Input.GetKey(KeyCode.J) ? 1 : 0);
			vertical = (Input.GetKey(KeyCode.I) ? 1 : 0) - (Input.GetKey(KeyCode.K) ? 1 : 0);
			direction = new Vector2(horizontal, vertical);
		}

		if (direction != Vector2.zero)
		{
			direction.Normalize();
		}
		return direction;
	}



	private bool LassoReady() 
    { 
        return (lassoObj.activeInHierarchy) ? false : true;
    }
}
