using System;
using UnityEngine;
using UnityEngine.UI;

public class BuildingManager : MonoBehaviour
{
	public LayerMask buildableLayer;
	public LayerMask buildingLayer;
	[SerializeField] Camera mainCamera;
	[SerializeField] Color selectedGhostColor = Color.black;

	private Ghost selectedGhost = null;

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

			bool buildMenuShown = false;

			if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity))
			{
				if(selectedGhost != null)
				{
					selectedGhost.GetComponentInChildren<Image>().color = Color.white;					
				}

				Ghost building = hit.collider.gameObject.GetComponent<Ghost>();

				if (building && !GameManager.Instance.PointerOverUI())
				{
					building.GetComponentInChildren<Image>().color = selectedGhostColor;
					selectedGhost = building;

					building.ShowBuildMenu();
					UnitGui.Instance.Hide();
					VehicleBayBuildMenu.Instance.Hide();
					buildMenuShown = true;
				}
				
			} 
			
			if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, buildingLayer) 
				&& !GameManager.Instance.PointerOverUI())
			{
				if (selectedGhost != null)
				{
					selectedGhost.GetComponentInChildren<Image>().color = Color.white;
				}

				//Debug.Log(hit.collider.gameObject.name);
				Building building = hit.collider.gameObject.GetComponent<Building>();
				if (building)
				{
					building.Interact();
					UnitGui.Instance.Hide();
					BuildMenu.Instance.Hide();
					buildMenuShown = true;
				}
				/*else
				{
                    BuildingInfo.Instance.HidePanel();
                }*/
			}
			else if(!GameManager.Instance.PointerOverUI()) // if the cursor is not over the UI
			{
				BuildingInfo.Instance.HidePanel();

			}

			if (!buildMenuShown)
			{
				BuildMenu.Instance.Hide();
				VehicleBayBuildMenu.Instance.Hide();
			}
		}
	}
}
