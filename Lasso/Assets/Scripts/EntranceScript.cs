using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntranceScript : MonoBehaviour
{
	private Vector2 startPosition;
	private bool doneMoving = false;
	private bool spawnPlayer = false;
	private bool fallAway = false;
	GameObject player;
	GameObject mainCamera;

	void Start()
	{
		Time.timeScale = 0f;
		player = GameObject.FindGameObjectWithTag("Player");

		mainCamera = GameObject.Find("Main Camera");

		player.SetActive(false);
		startPosition = transform.position;
	}

	void Update()
	{
		if (!doneMoving)
		{
			transform.position += new Vector3(0, 0.01f, 0);

			if (mainCamera != null)
			{
				CameraHandler cameraHandler = mainCamera.GetComponent<CameraHandler>();
				cameraHandler.ScreenShake(0.5f, 0.1f);
			}

			if (transform.position.y >= startPosition.y + 3.35f)
			{
				doneMoving = true;
				//Time.timeScale = 1f;
				spawnPlayer = true;
			}
		}

		if (spawnPlayer)
		{
			// Wait .5 seconds and then spawn the player
			StartCoroutine(SpawnPlayer());
		}

		if (fallAway)
		{
			// Move the entrance back down and enable time
			transform.position += new Vector3(0, -0.01f, 0);

			if (mainCamera != null)
			{
				CameraHandler cameraHandler = mainCamera.GetComponent<CameraHandler>();
				cameraHandler.ScreenShake(0.5f, 0.1f);
			}

			if (transform.position.y <= startPosition.y)
			{
				Time.timeScale = 1f;
				Destroy(gameObject);
			}
		}
	}

	IEnumerator SpawnPlayer()
	{
		yield return new WaitForSecondsRealtime(0.5f);
		player.transform.position = transform.GetChild(0).position;
		player.SetActive(true);
		player = null;
		fallAway = true;
	}
}