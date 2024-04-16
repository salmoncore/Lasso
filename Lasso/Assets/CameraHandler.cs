using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    // CameraHandler's purpose is to follow the player's position, either vertically or horizontally, as set in the Unity inspector.

    [SerializeField] private bool followPlayerVertically;
    [SerializeField] private bool followPlayerHorizontally;
    [SerializeField] private Vector2 offsetVertical;
    [SerializeField] private Vector2 offsetHorizontal;
    [SerializeField] private float boundaryVertical;
    [SerializeField] private float boundaryHorizontal;
    private GameObject player;
	private Vector3 cameraStartPosition;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
		cameraStartPosition = transform.position;

		if (player == null)
        {
            Debug.LogError("Player object not found in the scene. Make sure the player object has the tag 'Player'.");
        }
    }

	void Update()
	{
		if (player == null) return;

		Vector3 desiredPosition = cameraStartPosition;
		Vector3 playerPosition = player.transform.position;

		// Handle horizontal movement
		if (followPlayerHorizontally)
		{
			if (Mathf.Abs(playerPosition.x + offsetHorizontal.x - transform.position.x) > boundaryHorizontal)
			{
				float offsetX = playerPosition.x + offsetHorizontal.x - transform.position.x - Mathf.Sign(playerPosition.x + offsetHorizontal.x - transform.position.x) * boundaryHorizontal;
				desiredPosition.x += offsetX;
			}
		}

		// Handle vertical movement
		if (followPlayerVertically)
		{
			if (Mathf.Abs(playerPosition.y + offsetVertical.y - transform.position.y) > boundaryVertical)
			{
				float offsetY = playerPosition.y + offsetVertical.y - transform.position.y - Mathf.Sign(playerPosition.y + offsetVertical.y - transform.position.y) * boundaryVertical;
				desiredPosition.y += offsetY;
			}
		}

		transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 3);
	}
}
