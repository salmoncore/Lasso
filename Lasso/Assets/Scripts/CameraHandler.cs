using System.Collections;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
	[SerializeField] private GameObject background;
	[SerializeField] private bool followPlayerVertically;
	[SerializeField] private bool followPlayerHorizontally;
	[SerializeField] private Vector2 offsetVertical;
	[SerializeField] private Vector2 boundariesX;
	[SerializeField] private Vector2 boundariesY;
	[SerializeField] private float smoothSpeed = 0.125f; // Speed of the camera's follow
	private GameObject player;
	private Vector3 targetPosition;

	void Start()
	{
		player = GameObject.FindGameObjectWithTag("Player");

		if (player == null)
		{
			Debug.LogError("Player object not found in the scene. Make sure the player object has the tag 'Player'.");
		}
	}

	void Update()
	{
		if (player == null) return;

		float posX = followPlayerHorizontally ? player.transform.position.x : transform.position.x;
		float posY = followPlayerVertically ? player.transform.position.y : transform.position.y;

		posX += followPlayerHorizontally ? offsetVertical.x : 0;
		posY += followPlayerVertically ? offsetVertical.y : 0;

		// Apply boundaries
		posX = Mathf.Clamp(posX, boundariesX.x, boundariesX.y);
		posY = Mathf.Clamp(posY, boundariesY.x, boundariesY.y);

		// Smoothly lerp to the target position
		targetPosition = new Vector3(posX, posY, transform.position.z);
		transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
	}

	public void ScreenShake(float duration, float magnitude)
	{
		StartCoroutine(DoScreenShake(duration, magnitude));
	}

	private IEnumerator DoScreenShake(float duration, float magnitude)
	{
		Vector3 originalPosition = transform.position;
		float elapsed = 0.0f;

		while (elapsed < duration)
		{
			float x = Random.Range(-1f, 1f) * magnitude;
			float y = Random.Range(-1f, 1f) * magnitude;

			transform.position = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);
			transform.position = new Vector3(Mathf.Clamp(transform.position.x, boundariesX.x, boundariesX.y),
											 Mathf.Clamp(transform.position.y, boundariesY.x, boundariesY.y),
											 transform.position.z);

			elapsed += Time.deltaTime;
			yield return null;
		}

		transform.position = originalPosition;
	}
}
