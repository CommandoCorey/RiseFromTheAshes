using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

[System.Serializable]
public struct BuildItem
{
	public string name;
	public Building buildingPrefab;
	public Button button;

	BuildItem(string n, Building pre, Button but = null) {
		name = n;
		buildingPrefab = pre;
		button = but;
	}
}

public class BuildMenu : MonoBehaviour
{
	[SerializeField] List<BuildItem> buildItems;

	[HideInInspector] public Ghost ghostBuilding;

	[SerializeField] Button cancelButton;

	[SerializeField] GraphicRaycaster raycaster;
	[SerializeField] EventSystem eventSystem;
	[SerializeField] GameObject insufficientResourcesText;

	static public BuildMenu Instance { get; private set; }

	void Awake() {
		if (Instance != null && Instance != this) {
			Destroy(this);
		} else {
			Instance = this;
		}
	}

	private void Start()
	{
		foreach (var buildItem in buildItems) {
			buildItem.button.onClick.AddListener(() => {
				Build(buildItem);
			});
		}

		cancelButton.onClick.AddListener(() => {
			Hide();
		});

		Hide();
	}

	public void Hide()
	{
		gameObject.SetActive(false);
	}

	private void Update()
	{
		if (Input.GetMouseButtonUp(0))
		{
			RaycastHit hit;
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
			{
				if (!hit.collider.gameObject.CompareTag("BuildMenu"))
				{
					Hide();
				}
			}
		}
	}

	public void Build(in BuildItem item)
	{
		// check the resource cost of the building
		ResourceManager rm = ResourceManager.Instance;
		int totalSteel = rm.GetResource(ResourceType.Steel);

		if (totalSteel >= item.buildingPrefab.steelCost)
		{
			rm.SpendResource(ResourceType.Steel, item.buildingPrefab.steelCost);

			Building b = Instantiate(item.buildingPrefab, ghostBuilding.transform.position, ghostBuilding.transform.rotation);
			ghostBuilding.gameObject.SetActive(false);
			b.Build();
			insufficientResourcesText.SetActive(false);
			gameObject.SetActive(false);
		}
		else
        {
			insufficientResourcesText.SetActive(true);
		}
	}
}
