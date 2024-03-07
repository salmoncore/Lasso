using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
 
// If there is no selected item, set the selected item to the event system's first selected item
public class InputBehaviour : MonoBehaviour
{
    GameObject lastselect;
 
    void Start()
    {
        lastselect = new GameObject();
    }
 
    // Update is called once per frame
    void Update ()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(lastselect);
        }
        else
        {
            lastselect = EventSystem.current.currentSelectedGameObject;
        }
    }
 
}
