using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoomIn : MonoBehaviour
{
	CameraHandler CameraHandler;

	private void Awake()
	{
		CameraHandler = Camera.main.GetComponent<CameraHandler>();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.tag == "Player")
		{
			StartCoroutine(ZoomIn());
		}
	}

	IEnumerator ZoomIn()
	{
		while (Camera.main.orthographicSize > 5)
		{
			Camera.main.orthographicSize -= 0.1f;
			CameraHandler.boundariesY = new Vector2(-8.26f, CameraHandler.boundariesY.y);
			yield return new WaitForSeconds(0.01f);
		}
	}
}
