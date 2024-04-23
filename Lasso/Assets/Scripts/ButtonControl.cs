using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonControl : MonoBehaviour
{
	[SerializeField] private GameObject Box; // The thing that the button will drop
	[SerializeField] private bool boxSquishes = true;

	private void Start()
	{
		if (Box == null)
		{
			Debug.LogError("Box is null");
			return;
		}

		Rigidbody2D rb = Box.GetComponent<Rigidbody2D>();
		rb.constraints = RigidbodyConstraints2D.FreezeAll;
	}

	public void Press()
	{
		if (Box == null)
		{
			Debug.LogError("Box is null");
			return;
		}

		Rigidbody2D rb = Box.GetComponent<Rigidbody2D>();
		rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
		rb.WakeUp(); // Ensure Rigidbody is awake after changing constraints

		// If the box "squishes" things under it when it lands, add the SquishHandler script to the box
		if (boxSquishes)
		{
			Box.AddComponent<SquishHandler>();
		}

		// Call Break from LassoHandler.cs
		GetComponent<LassoHandler>().BreakObject(this.gameObject);
	}
}
