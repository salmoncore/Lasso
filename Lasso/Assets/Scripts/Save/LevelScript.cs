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
        player = GameObject.FindGameObjectWithTag("Player");
        gameManager = GameObject.Find("GameManager");
    }

    public void Update()
    {
        if (player == null)
        {
            Debug.Log("Player object not found in the scene. Make sure the player object has the tag 'Player'.");
            return;
        }

       if (player.GetComponent<PlayerMovement>() != null)
        {
            if (player.GetComponent<PlayerMovement>().HasWon())
            {
                Pass();
                Debug.Log("epic win");
            }
            else
            {
                gameManager.GetComponent<GameManagerScript>().gameOver();
                Debug.Log("Dead");
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
