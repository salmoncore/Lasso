using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class PlayerCombat : MonoBehaviour
{

    public Animator animator;

    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) {
            Attack();
        }
    }

    void Attack() {
        // Play an attack animation

        // Detect all the enemies in range of attack
        Collider2D hitEnemies = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayers);

        // Apply Damage
        // This will have to be changed for the first one that was hit
        if (hitEnemies != null) {
            Debug.Log("We hit " + hitEnemies.name);
        } 
    }

    void OnDrawGizmosSelected() {
        if (attackPoint == null)
            return;

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
