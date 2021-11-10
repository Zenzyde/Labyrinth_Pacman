using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	[SerializeField] private Transform followTarget;
	[SerializeField] private float followDistance;

	// Update is called once per frame
	void LateUpdate()
	{
		transform.position = followTarget.position - transform.forward * followDistance;
	}
}
