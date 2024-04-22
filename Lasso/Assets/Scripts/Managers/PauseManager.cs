using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI, optionsMenuUI;
    public GameObject resumeButton, optionsFirstButton, optionsClosedButton;
    public bool isPaused = false;
    public GameManagerScript gameManager;
    public GameObject inputManager;

    // Update is called once per frame
    void Update()
    {
        // Now handled in playermovement using new input system

        //if (Input.GetKeyDown(KeyCode.Escape) && !gameManager.isDead) {
        //    if (isPaused) {
        //        resume();
        //    } else {
        //        pause();
        //    }
        //}
    }

    public void resume() {
        inputManager.SetActive(false);
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        Debug.Log("Resume");
    }

    public void pause() {
        Debug.Log("Paused");
        inputManager.SetActive(true);
        // clear selected object
        EventSystem.current.SetSelectedGameObject(null);

        // set a new selected object
        EventSystem.current.SetSelectedGameObject(resumeButton);
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void openOptions() {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(optionsFirstButton);
        Debug.Log("Options Open");
    }

    public void closeOptions() {
        optionsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(optionsClosedButton);
        Debug.Log("Options Close");
    }

    public void mainMenu() {
        Debug.Log("Main Menu");
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene("MainMenu");
    }

    public void quit() {
        Debug.Log("Quit");
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        Application.Quit();
    }
}
