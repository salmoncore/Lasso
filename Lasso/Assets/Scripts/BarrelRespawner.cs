using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelRespawner : MonoBehaviour
{
    private GameObject GameObject;
    private Transform Transform;

    private void Start()
    {
		Transform = transform.GetChild(0);
	}

    private void OnTriggerEnter2D(Collider2D other)
    {
		if (other.CompareTag("Sturdy") || other.CompareTag("SturdyProjectile"))
		{
			other.gameObject.transform.position = Transform.position;
			other.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
		}
	}
}
