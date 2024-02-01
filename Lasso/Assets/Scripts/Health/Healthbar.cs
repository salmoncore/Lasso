using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    // Note that there are 3 hearts in the UI. Any more will require a UI change.
    [SerializeField] private Health playerHealth;
    [SerializeField] private Image Heart1;
    [SerializeField] private Image Heart2;
    [SerializeField] private Image Heart3;

    private void Start()
    {
        playerHealth = GameObject.Find("Player").GetComponent<Health>();
    }

    private void Update()
    {
        if (playerHealth.currentHealth == 3)
        {
            Heart1.enabled = true;
            Heart2.enabled = true;
            Heart3.enabled = true;
        }
        else if (playerHealth.currentHealth == 2)
        {
            Heart1.enabled = true;
            Heart2.enabled = true;
            Heart3.enabled = false;
        }
        else if (playerHealth.currentHealth == 1)
        {
            Heart1.enabled = true;
            Heart2.enabled = false;
            Heart3.enabled = false;
        }
        else if (playerHealth.currentHealth == 0)
        {
            Heart1.enabled = false;
            Heart2.enabled = false;
            Heart3.enabled = false;
        }
        else // error
        {
            Heart1.enabled = false;
            Heart2.enabled = true;
            Heart3.enabled = false;
        }
    }
}
