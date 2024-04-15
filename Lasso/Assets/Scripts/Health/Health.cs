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
    private bool isInvincible = false;
    private bool render = true;
    [SerializeField] private float invincibilityDurationSeconds;
    [SerializeField] private float invincibilityDeltaTime;
    [SerializeField] private GameObject player;
    [SerializeField] private float kickback = 30;
    private Rigidbody2D body;
    private bool isEnemyToTheRight;
    public GameManagerScript gameManager;
    public GameObject musicPlayer;
    public AudioManager audioManagerScript;

	private void Awake()
    {
        currentHealth = startingHealth;
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
        body = GetComponent<Rigidbody2D>();
    }

	private void OnCollisionEnter2D(Collision2D collision)
	{
		// check the tag of the object we collided with
        if (collision.gameObject.tag == "Enemy")
        {
			TakeDamage(damage);
            isEnemyToTheRight = collision.transform.position.x > transform.position.x;
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.tag == "Health")
		{
			Heal(heal);
			Destroy(collision.gameObject);
		}
	}

	public void TakeDamage(int _damage) // TODO: Does this need to be public anymore?
    {
        if (isInvincible) return;

        // Clamp the health to be between 0 and startingHealth
        currentHealth = Mathf.Clamp(currentHealth - _damage, 0, startingHealth);

        if (currentHealth > 0) // Player hurt
        {
            // plays hurt music
            musicPlayer = GameObject.Find("MusicPlayer");
            audioManagerScript = musicPlayer.GetComponent<AudioManager>();
            audioManagerScript.SFX("hurt");
            
            anim.SetTrigger("isHit");
            anim.SetTrigger("hurt");

            // When the player is hurt, knock them back to the left or the right in the opposite direction of the enemy
            if (isEnemyToTheRight)
            {
				body.velocity = new Vector2(kickback, kickback);
			}
			else
            {
				body.velocity = new Vector2(-kickback, kickback);
			}

        }
        else // Player dead lol
        {
            if (!dead)
            { 
                gameManager.gameOver();
                anim.SetBool("dead", true);
                GetComponent<PlayerMovement>().enabled = false;
                GetComponent<PlayerAttack>().enabled = false;
                body.velocity = new Vector2(0, 0);
                body.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
                dead = true;
                return;
            }
		}

        // TODO: Fix this later please god

        StartCoroutine(BecomeTemporarilyInvincible());
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

    private IEnumerator BecomeTemporarilyInvincible() {
        // Debug.Log("Player turned invincible!");
        isInvincible = true;

        for (float i = 0; i < invincibilityDurationSeconds; i += invincibilityDeltaTime) {
            if (render) {
                
                // Determine if there's a sprite renderer or a mesh renderer
                if (player.GetComponent<SpriteRenderer>() != null)
                {
					TurnOffSpriteRenderer();
				} else
                {
					TurnOffMeshRenderer();
				}

            } else {

				if (player.GetComponent<SpriteRenderer>() != null)
				{
					TurnOnSpriteRenderer();
				}
				else
				{
					TurnOnMeshRenderer();
				}
			}
            yield return new WaitForSeconds(invincibilityDeltaTime);
        }

		// Debug.Log("Player is no longer invincible!");
		if (player.GetComponent<SpriteRenderer>() != null)
		{
			TurnOnSpriteRenderer();
		}
		else
		{
			TurnOnMeshRenderer();
		}
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

    private void TurnOffMeshRenderer()
    {
        player.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        render = false;
    }

    private void TurnOnMeshRenderer()
    {
		player.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        render = true;
	}
}
