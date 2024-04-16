using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquishHandler : MonoBehaviour
{
	private Dictionary<GameObject, Coroutine> squishingCoroutines = new Dictionary<GameObject, Coroutine>();
	private bool isSquishing = true;

	private void OnCollisionEnter2D(Collision2D collision)
	{
		Debug.Log("Collided with: " + collision.gameObject.name);

		if (collision.gameObject.tag == "Player" && isSquishing)
		{
			collision.gameObject.GetComponent<Health>().TakeDamage(1000);
		}
		else if (collision.gameObject.tag == "Ground" && isSquishing)
		{
			isSquishing = false;
		}
		else if (collision.gameObject.tag != "Button" && isSquishing) // Squish and destroy all other objects enemies, good riddence
		{
			if (!squishingCoroutines.ContainsKey(collision.gameObject))
			{
				squishingCoroutines[collision.gameObject] = StartCoroutine(Squish(collision.gameObject));
			}
		}
	}

	private IEnumerator Squish(GameObject obj)
	{
		// Shrink the object on the Y axis
		while (obj != null && obj.transform.localScale.y > 0f)
		{
			obj.transform.localScale = new Vector3(obj.transform.localScale.x, obj.transform.localScale.y - 1f, obj.transform.localScale.z);
			yield return new WaitForSeconds(0.01f);
		}

		// Destroy the object if it hasn't been destroyed yet
		if (obj != null)
		{
			Destroy(obj);
		}

		if (squishingCoroutines.ContainsKey(obj))
		{
			squishingCoroutines.Remove(obj);
		}
	}

	private void OnDestroy()
	{
		foreach (var coroutine in squishingCoroutines.Values)
		{
			StopCoroutine(coroutine);
		}
		squishingCoroutines.Clear();
	}
}
