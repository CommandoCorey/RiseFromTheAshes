using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBillboard : MonoBehaviour
{
	[SerializeField] bool invert = false;

	private void Update()
	{
		var mainCamera = Camera.main;
 
		Vector3 oldRotation = transform.rotation.eulerAngles;
		transform.LookAt(mainCamera.transform.position);
		Vector3 oldRotation2 = transform.rotation.eulerAngles;
		transform.rotation = Quaternion.Euler(new Vector3(invert ? -oldRotation2.x : oldRotation2.x, oldRotation.y, oldRotation.z));
	}
}
