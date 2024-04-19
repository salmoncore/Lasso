using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalArrow : MonoBehaviour
{
	private GameObject player;
	private GameObject goal;
	private LineRenderer lineRenderer;

	private void Awake()
	{
		goal = GameObject.FindGameObjectWithTag("Goal");

		if (goal == null)
		{
			Debug.Log("Goal not found?");
			return;
		}

		lineRenderer = gameObject.AddComponent<LineRenderer>();
		lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

		lineRenderer.startColor = new Color(0.0f, 1.0f, 0.0f, 1.0f);
		lineRenderer.endColor = new Color(0.0f, 1.0f, 0.0f, 0.0f);

		lineRenderer.startWidth = 0.06f;
		lineRenderer.endWidth = 0.14f;
	}

	private void Update()
	{
		if (player == null && Time.timeScale != 0)
		{
			player = GameObject.FindGameObjectWithTag("Player");
		}

		DrawLine();
	}

	private void DrawLine()
	{
		if (player != null && player.GetComponent<PlayerMovement>().LevelCleared())
		{
			Vector2 playerCenter = GetPlayerCenter();
			Vector2 goalCenter = GetGoalCenter();
			lineRenderer.SetPosition(0, playerCenter);
			lineRenderer.SetPosition(1, goalCenter);

			lineRenderer.enabled = true;
		}
		else
		{
			lineRenderer.enabled = false;
		}
	}

	private Vector2 GetPlayerCenter()
	{
		if (player != null && player.GetComponent<Collider2D>() != null)
		{
			return player.GetComponent<Collider2D>().bounds.center;
		}
		else
		{
			Debug.LogError("Player object or BoxCollider2D component not found.");
			return Vector2.zero;
		}
	}

	private Vector2 GetGoalCenter()
	{
		if (goal != null && goal.GetComponent<Collider2D>() != null)
		{
			return goal.GetComponent<Collider2D>().bounds.center;
		}
		else
		{
			Debug.LogError("Goal object or BoxCollider2D component not found.");
			return Vector2.zero;
		}
	}
}
