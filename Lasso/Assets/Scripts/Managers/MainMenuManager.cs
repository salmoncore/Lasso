using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
//using UnityEditor.Rendering.Fullscreen.ShaderGraph;
using UnityEngine.UI;
using Unity.VisualScripting;

public class MainMenuManager : MonoBehaviour
{
    public GameObject optionsMenuUI, gameControlsUI, mainMenuUI;
    public GameObject playButton, optionsFirstButton, optionsClosedButton, controlsFirstButton, controlsClosedButton;
    public GameObject inputManager;
    public GameObject toggle;
    public Text fullscreenB, fullscreenW, borderlessB, borderlessW;
    
    Toggle m_toggle;
    // Start is called before the first frame update
    void Start()
    {
        inputManager.SetActive(true);
        optionsMenuUI.SetActive(false);
        gameControlsUI.SetActive(false);

        // clear selected object
        EventSystem.current.SetSelectedGameObject(null);

        // set a new selected object
        EventSystem.current.SetSelectedGameObject(playButton);
    }

    // Update is called once per frame
    void Update()
    {
        // value to store our toggle component
        m_toggle = toggle.GetComponent<Toggle>();

        // if the screen is on fullscreenwindow we want the checkmark on
        // else, we want it off. this works when opening the game multiple times
        if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow) {
            m_toggle.isOn = true;
            windowToggle(true);
        } else if (Screen.fullScreenMode == FullScreenMode.Windowed) {
            m_toggle.isOn = false;
            windowToggle(false);
        }
    }

    public void play() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        Debug.Log("Play");
    }

    public void openControls() {
        mainMenuUI.SetActive(false);
        gameControlsUI.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(controlsFirstButton);
        Debug.Log("Open Controls");
    }

    public void closeControls() {
        gameControlsUI.SetActive(false);
        mainMenuUI.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(controlsClosedButton);
        Debug.Log("Close Controls");
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

    public void windowToggle(bool tog) {
        
        // if the player hits the toggle, we change the screen to windowed
        // else, we make it fullscreen and change the wordings.
        if (!tog) {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            fullscreenB.gameObject.SetActive(false);
            fullscreenW.gameObject.SetActive(false);
            borderlessB.gameObject.SetActive(true);
            borderlessW.gameObject.SetActive(true);
            Debug.Log("Borderless on");
        } else {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            borderlessB.gameObject.SetActive(false);
            borderlessW.gameObject.SetActive(false);
            fullscreenB.gameObject.SetActive(true);
            fullscreenW.gameObject.SetActive(true);
            Debug.Log("Fullscreen on");
        }

        Debug.Log("Screen Changed");
    }
}
