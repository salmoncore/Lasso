using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    // Note that there are 3 hearts in the UI. Any more will require a UI change.
    [SerializeField] private int startingHealth = 3;
	[SerializeField] private int damage = 1;
	[SerializeField] private int heal = 1;
	public int currentHealth { get; private set; }
    private Animator anim;
    private bool dead;

	private void Awake()
    {
        currentHealth = startingHealth;
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
		if (collision.tag == "Enemy")
        {
			TakeDamage(damage);
		}
        else if (collision.tag == "Health")
        {
			Heal(heal);
			Destroy(collision.gameObject);
		}
	}

    public void TakeDamage(int _damage) // TODO: Does this need to be public anymore?
    {
        // Clamp the health to be between 0 and startingHealth
        currentHealth = Mathf.Clamp(currentHealth - _damage, 0, startingHealth);

        if (currentHealth > 0) // Player hurt
        {
            anim.SetTrigger("hurt");
            // TODO: Needs iframes
        }
        else // Player dead lol
        {
            if (!dead)
            { 
                anim.SetBool("dead", true);
                GetComponent<PlayerMovement>().enabled = false;
                dead = true;
            }
        }
    }

    public void Heal(int _heal) // TODO: Does this need to be public anymore?
	{
        if (currentHealth > 0)
        {
            currentHealth = Mathf.Clamp(currentHealth + _heal, 1, startingHealth);

            // TODO: Set heal animation
 
        }
    }

    private void Update()
    {
        // TODO: Remove these for release
        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            TakeDamage(1);
        }
        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            Heal(1);
        }
    }
}
