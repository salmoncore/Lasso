using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    // Note that there are 3 hearts in the UI. Any more will require a UI change.
    [SerializeField] private int startingHealth = 3;
    public int currentHealth { get; private set; }
    private Animator anim;
    private bool dead;
    private bool isInvincible = false;
    private bool render = true;
    [SerializeField] private float invincibilityDurationSeconds;
    [SerializeField] private float invincibilityDeltaTime;
    [SerializeField] private GameObject player;
    private Rigidbody2D body;

    private void Awake()
    {
        currentHealth = startingHealth;
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
        body = GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(int _damage) 
    {
        if (isInvincible) return;

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
                anim.SetTrigger("die");
                GetComponent<PlayerMovement>().enabled = false;
                body.velocity = new Vector2(0, 0);
                dead = true;
                return;
            }
        }

        StartCoroutine(BecomeTemporarilyInvincible());
    }

    public void Heal(int _heal)
    {
        if (currentHealth > 0) // Player healable
        {
            // Clamp the health to be between 0 and startingHealth
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

    private IEnumerator BecomeTemporarilyInvincible() {
        Debug.Log("Player turned invincible!");
        isInvincible = true;

        for (float i = 0; i < invincibilityDurationSeconds; i += invincibilityDeltaTime) {
            if (render) {
                TurnOffSpriteRenderer();
            } else {
                TurnOnSpriteRenderer();
            }
            yield return new WaitForSeconds(invincibilityDeltaTime);
        }

        Debug.Log("Player is no longer invincible!");
        TurnOnSpriteRenderer();
        isInvincible = false;
    }

    void MethodThatTriggersInvulnerability() {
        if (!isInvincible) {
            StartCoroutine(BecomeTemporarilyInvincible());
        }
    }

    private void TurnOffSpriteRenderer() {
        player.GetComponent<SpriteRenderer>().enabled = false;
        render = false;
    }

    private void TurnOnSpriteRenderer() {
        player.GetComponent<SpriteRenderer>().enabled = true;
        render = true;
    }
}
