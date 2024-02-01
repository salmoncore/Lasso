using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    // Note that there are 3 hearts in the UI. Any more will require a UI change.
    [SerializeField] private int startingHealth = 3;
    public int currentHealth { get; private set; }
    private Animator anim;

    private void Awake()
    {
        currentHealth = startingHealth;
        anim = GetComponent<Animator>();
    }

    private void TakeDamage(int _damage) 
    {
        // Clamp the health to be between 0 and startingHealth
        currentHealth = Mathf.Clamp(currentHealth - _damage, 0, startingHealth);

        if (currentHealth > 0) // Player hurt
        {
            anim.SetTrigger("hurt");
        }
        else // Player dead lol
        {
            anim.SetTrigger("die");
        }
    }

    private void Heal(int _heal)
    {
        // Clamp the health to be between 0 and startingHealth
        currentHealth = Mathf.Clamp(currentHealth + _heal, 0, startingHealth);
    }
}
