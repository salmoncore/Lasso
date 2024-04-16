using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquishHandler : MonoBehaviour
{
	private bool isSquishing = true;

	private void OnCollisionEnter2D(Collision2D collision)
	{
		Debug.Log("Collided with: " + collision.gameObject.name);

		if (collision.gameObject.GetComponent<LassoHandler>() && isSquishing)
		{
			Debug.Log("Object: " + collision.gameObject.name + " has a LassoHandler script attached!");
			collision.gameObject.GetComponent<LassoHandler>().BreakObject(collision.gameObject);
		}
		if (collision.gameObject.tag == "Player" && isSquishing)
		{
			collision.gameObject.GetComponent<Health>().TakeDamage(1000);
		}
		if (collision.gameObject.tag == "Ground" && isSquishing)
		{
			isSquishing = false;
		}
	}
}
