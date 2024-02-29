using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    // Note that there are 3 hearts in the UI. Any more will require a UI change.
    [SerializeField] private Health playerHealth;
    [SerializeField] private List<Image> hearts;

    private void Start()
    {
        //playerHealth = GameObject.Find("Player").GetComponent<Health>();

        if (playerHealth == null)
        {
			playerHealth = GameObject.Find("BanditPC").GetComponent<Health>();
		}
    }

    private void Update()
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            hearts[i].enabled = (i < playerHealth.currentHealth);
        }
    }
}
