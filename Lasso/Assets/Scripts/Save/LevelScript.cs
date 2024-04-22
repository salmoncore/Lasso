using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelScript : MonoBehaviour
{
    private GameObject gameManager;
    private GameObject player;

    public void Start()
    {
        gameManager = GameObject.Find("GameManager");

        // Null check
        if (gameManager == null)
        {
			Debug.Log("GameManager not found!!! Saving may fail.");
		}
        else
        {
			Debug.Log("GameManager found: " + gameManager.name);
		}
    }

    public void Update()
    {
        if (player == null && Time.timeScale != 0)
        {
			player = GameObject.FindGameObjectWithTag("Player");
        }

        if (player != null && player.GetComponent<PlayerMovement>() != null)
        {
            if (player.GetComponent<PlayerMovement>().HasWon())
            {
                Debug.Log("epic win");
                Pass();
            }
        }
    }

    public void Pass()
    {
        int currentLevel = SceneManager.GetActiveScene().buildIndex;

        if (currentLevel >= PlayerPrefs.GetInt("levelsUnlocked"))
        {
            PlayerPrefs.SetInt("levelsUnlocked", currentLevel + 1);      
        }
        SceneManager.LoadScene(currentLevel + 1);
        Debug.Log("Level" + PlayerPrefs.GetInt("levelsUnlocked") + "Unlocked");
    }
}
