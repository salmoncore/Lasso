using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public GameObject resumeButton, optionsButton, mainMenuButton, quitButton;
    public static bool isPaused = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (isPaused) {
                resume();
            } else {
                pause();
            }
        }
    }

    public void resume() {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        Debug.Log("Resume");
    }

    public void pause() {
        Debug.Log("Paused");
        // clear selected object
        EventSystem.current.SetSelectedGameObject(null);

        // set a new selected object
        EventSystem.current.SetSelectedGameObject(resumeButton);
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void options() {
        Debug.Log("Options");
    }

    public void mainMenu() {
        Debug.Log("Main Menu");
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene("MainMenu");
    }

    public void quit() {
        Debug.Log("Quit");
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        Application.Quit();
    }
}
