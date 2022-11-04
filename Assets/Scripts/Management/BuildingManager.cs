using System;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
	public LayerMask buildableLayer;
	public LayerMask buildingLayer;
	[SerializeField] Camera mainCamera;

	static public BuildingManager Instance {  get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		} else
		{
			Destroy(this);
		}
	}

	void Update()
	{
		RaycastHit hit;

		if (Input.GetMouseButtonUp(0))
		{

			if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, buildableLayer))
			{
				Ghost building = hit.collider.gameObject.GetComponent<Ghost>();

				if (building)
				{
					building.ShowBuildMenu();
				}
			} else if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, buildingLayer))
			{
				Debug.Log(hit.collider.gameObject.name);
				Building building = hit.collider.gameObject.GetComponent<Building>();
				try
				{
					building.Interact();
				} catch(Exception e)
				{
					Debug.LogException(e);
				}
			}
		}
	}
}
