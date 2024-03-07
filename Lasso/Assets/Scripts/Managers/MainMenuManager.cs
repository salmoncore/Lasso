using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenuManager : MonoBehaviour
{
    public GameObject playButton, optionsButton, quitButton, creditsButton;
    // Start is called before the first frame update
    void Start()
    {
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
        SceneManager.LoadScene("Demo");
        Debug.Log("Play");
    }

    public void options() {
        //SceneManager.LoadScene("MainMenu");
        Debug.Log("Options");
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
