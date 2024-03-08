using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class CreditManager : MonoBehaviour
{
    public GameObject backButton;
    public GameObject inputManager;

    // Start is called before the first frame update
    void Start()
    {
        inputManager.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void back() {
        SceneManager.LoadScene("MainMenu");
        Debug.Log("Main Menu");
    }
}
