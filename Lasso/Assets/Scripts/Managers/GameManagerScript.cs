using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameManagerScript : MonoBehaviour
{

    public GameObject gameOverUI;
    public GameObject restartButton, mainMenuButton, quitButton;
    public bool isDead = false;
    public GameObject inputManager;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void gameOver() {
        // stop time on death
        isDead = true;
        Time.timeScale = 0f;
        inputManager.SetActive(true);
        gameOverUI.SetActive(true);
        // clear selected object
        EventSystem.current.SetSelectedGameObject(null);

        // set a new selected object
        EventSystem.current.SetSelectedGameObject(restartButton);
    }

    public void restart() {
        // turn time back on 
        isDead = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Debug.Log("Restart");
    }

    public void mainMenu() {
        // turn time back on 
        isDead = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
        Debug.Log("Main Menu");
    }

    public void quit() {
        // turn time back on 
        isDead = false;
        Time.timeScale = 1f;
        Application.Quit();
        Debug.Log("Quit");
    }
}
