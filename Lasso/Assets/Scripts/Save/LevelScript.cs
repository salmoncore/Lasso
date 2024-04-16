using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelScript : MonoBehaviour
{
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
