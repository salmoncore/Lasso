using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    // Note that there are 3 hearts in the UI. Any more will require a UI change.
    [SerializeField] private int startingHealth = 3;
    public int currentHealth { get; private set; }

    private void Awake()
    {
        currentHealth = startingHealth;
    }

    private void TakeDamage(int _damage) 
    {
        // Clamp the health to be between 0 and startingHealth
        currentHealth = Mathf.Clamp(currentHealth - _damage, 0, startingHealth);

        if (currentHealth > 0) // Player hurt
        {
            
        }
        else // Player dead lol
        {
            
        }
    }

    private void Heal(int _heal)
    {
        // Clamp the health to be between 0 and startingHealth
        currentHealth = Mathf.Clamp(currentHealth + _heal, 0, startingHealth);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TakeDamage(1);
        }
    }
}
