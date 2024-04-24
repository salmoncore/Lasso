using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    // If the bullet collides with an object, destroy the bullet
    void OnCollisionEnter2D(Collision2D col)
    {
        // If the object that the bullet collides with has the "Fragile" tag, break it
        if (col.gameObject.tag == "Fragile")
        {
			// Attach a LassoHandler script to the object, if it doesn't already have one.
			if (col.gameObject.GetComponent<LassoHandler>() == null)
			{
				col.gameObject.AddComponent<LassoHandler>();
			}

			if (col.gameObject.GetComponent<LassoHandler>() != null)
			{
				col.gameObject.GetComponent<LassoHandler>().BreakObject(col.gameObject);
			}
		}

		Destroy(gameObject);
	}
}
