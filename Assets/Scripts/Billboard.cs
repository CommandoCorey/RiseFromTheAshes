using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
	[SerializeField] Camera mainCamera;

	private void Update()
	{
		Vector3 oldRotation = transform.rotation.eulerAngles;
		transform.LookAt(mainCamera.transform.position);
		Vector3 oldRotation2 = transform.rotation.eulerAngles;
		transform.rotation = Quaternion.Euler(new Vector3(oldRotation2.x, oldRotation.y, oldRotation.z));
	}
}
