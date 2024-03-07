using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenuManager : MonoBehaviour
{
    public GameObject optionsMenuUI, mainMenuUI;
    public GameObject playButton, optionsFirstButton, optionsClosedButton;
    public GameObject inputManager;
    // Start is called before the first frame update
    void Start()
    {
        inputManager.SetActive(true);
        optionsMenuUI.SetActive(false);
        // clear selected object
        EventSystem.current.SetSelectedGameObject(null);

        // set a new selected object
        EventSystem.current.SetSelectedGameObject(playButton);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void play() {
        SceneManager.LoadScene("klevel");
        Debug.Log("Play");
    }

    public void openOptions() {
        mainMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(optionsFirstButton);
        Debug.Log("Options open");
    }

    public void closeOptions() {
        optionsMenuUI.SetActive(false);
        mainMenuUI.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(optionsClosedButton);
        Debug.Log("Options closed");
    }

    public void quit() {
        Application.Quit();
        Debug.Log("Quit");
    }

    public void credits() {
        SceneManager.LoadScene("Credits");
        Debug.Log("Credits");
    }
}
