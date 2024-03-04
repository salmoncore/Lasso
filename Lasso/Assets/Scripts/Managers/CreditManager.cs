using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class CreditManager : MonoBehaviour
{
    public GameObject backButton;

    // Start is called before the first frame update
    void Start()
    {
        
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
