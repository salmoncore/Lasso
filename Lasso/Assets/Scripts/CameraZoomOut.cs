using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoomOut : MonoBehaviour
{
	CameraHandler CameraHandler;

	private void Awake()
	{
		CameraHandler = Camera.main.GetComponent<CameraHandler>();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		StartCoroutine(ZoomOut());
	}

	IEnumerator ZoomOut()
	{
		while (Camera.main.orthographicSize < 7.5)
		{
			Camera.main.orthographicSize += 0.1f;
			CameraHandler.boundariesY = new Vector2(-2.56f, CameraHandler.boundariesY.y);
			yield return new WaitForSeconds(0.01f);
		}
	}
}
