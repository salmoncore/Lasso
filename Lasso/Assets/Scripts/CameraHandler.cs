using System.Collections;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
	[SerializeField] private Transform background;
	[SerializeField] private float parallaxEffectMultiplier = 0.5f;
	[SerializeField] private bool followPlayerVertically;
	[SerializeField] private bool followPlayerHorizontally;
	[SerializeField] public Vector2 boundariesX; // X-axis boundaries
	[SerializeField] public Vector2 boundariesY; // Y-axis boundaries
	[SerializeField] private float smoothSpeed = 0.125f; // Speed of the camera's follow
	private GameObject player;
	private Vector3 targetPosition;
	private Vector3 lastCameraPosition;

	void Start()
	{
		lastCameraPosition = transform.position;
	}

	void Update()
	{
		if (player == null && Time.timeScale != 0f)
		{
			player = GameObject.FindGameObjectWithTag("Player");
			Debug.Log("Player found: " + player);
		}
		else if (player == null)
		{
			return;
		}

		float posX = followPlayerHorizontally ? Mathf.Clamp(player.transform.position.x, boundariesX.x, boundariesX.y) : transform.position.x;
		float posY = followPlayerVertically ? Mathf.Clamp(player.transform.position.y, boundariesY.x, boundariesY.y) : transform.position.y;

		// Smoothly lerp to the target position
		targetPosition = new Vector3(posX, posY, transform.position.z);
		transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
	}

	void LateUpdate()
	{
		if (background != null)
		{
			Vector3 deltaMovement = transform.position - lastCameraPosition;
			background.position += new Vector3(deltaMovement.x * parallaxEffectMultiplier, deltaMovement.y * parallaxEffectMultiplier, 0);
		}
		lastCameraPosition = transform.position;
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