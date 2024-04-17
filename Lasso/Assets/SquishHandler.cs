using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*public class SquishHandler : MonoBehaviour
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

			GameObject mainCamera = GameObject.Find("Main Camera");
			if (mainCamera != null)
			{
				CameraHandler cameraHandler = mainCamera.GetComponent<CameraHandler>();
				cameraHandler.ScreenShake(0.5f, 0.1f);
			}
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
		while (obj != null && (obj.transform.localScale.y > 0f || obj.transform.localScale.x > 0f))
		{
			// Recalculate whether the object is more vertical or horizontal on each iteration
			bool Xisvertical = (Vector3.Dot(obj.transform.up, Vector3.up) > Vector3.Dot(obj.transform.right, Vector3.up)) && (obj.transform.localScale.y > obj.transform.localScale.x);

			if (Xisvertical)
			{
				// Squish vertically
				obj.transform.localScale = new Vector3(obj.transform.localScale.x, Mathf.Max(0, obj.transform.localScale.y - 3f), obj.transform.localScale.z);
			}
			else
			{
				// Squish horizontally
				obj.transform.localScale = new Vector3(Mathf.Max(0, obj.transform.localScale.x - 1.5f), obj.transform.localScale.y, obj.transform.localScale.z);
			}

			// Wait for a frame
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
}*/
