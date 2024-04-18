using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class TextCopy : MonoBehaviour
{
    private GameObject textBackground;
	private bool errorFlag = false;

	private void Awake()
	{
		textBackground = GameObject.Find("TextBackground").gameObject;

		// Check if the TextBackground object was found
		if (textBackground == null)
		{
			Debug.Log("TextBackground object not found in the scene. Make sure the object is named 'TextBackground'.");
			errorFlag = true;
		}
	}

	// Start is called before the first frame update
	void Update()
	{
		if (errorFlag) return;

		GetComponent<Text>().text = textBackground.GetComponent<Text>().text;
	}
}
