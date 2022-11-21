using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Collections;

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
	[SerializeField] TextMeshProUGUI insufficientResourcesText;
	[SerializeField] float notificationTimeout = 1;

	[Header("Build Stats Text")]
	[SerializeField] GameObject statsPanel;
	[SerializeField] TextMeshProUGUI buildingName;
	[SerializeField] TextMeshProUGUI resourceCost;
	[SerializeField] TextMeshProUGUI buildTime;
	[SerializeField] TextMeshProUGUI description;

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

		if (cancelButton)
		{
			cancelButton.onClick.AddListener(() =>
			{
				Hide();
			});
		}

		Hide();
	}

	public void Hide()
	{
		gameObject.SetActive(false);
	}

	// added by Paul
	/// <summary>
	/// Updates the value on the info panel to show the stats based on the button hovered over
	/// </summary>
	/// <param name="itemNumber">the index of the item in the buildItems array</param>
	public void ShowBuildInfo(int itemNumber)
    {
		//Debug.Log("pointer entered button number" + itemNumber);
		
		statsPanel.SetActive(true);

		var building = buildItems[itemNumber].buildingPrefab;

		buildingName.text = buildItems[itemNumber].name;
		resourceCost.text = building.steelCost.ToString();
		buildTime.text = building.timeToBuild.ToString();
		description.text = building.buildingDescription;
	}
	
	// Added by Paul
	public void HideBuildInfo()
    {
		//Debug.Log("Pointer exited button");
		statsPanel.SetActive(false);
	}

	public void Build(in BuildItem item)
	{
		// check the resource cost of the building
		ResourceManager rm = ResourceManager.Instance;

		if (rm.SpendResource(ResourceType.Steel, item.buildingPrefab.steelCost))
		{
			Building b = Instantiate(item.buildingPrefab, ghostBuilding.transform.position, ghostBuilding.transform.rotation);
			ghostBuilding.child = b;
			b.ghost = ghostBuilding;
			ghostBuilding.gameObject.SetActive(false);
			b.Build();
			insufficientResourcesText.gameObject.SetActive(false);

			Hide();
		}
		else
        {
			Notify.Queue("You don't have enough steel to construct this building", notificationTimeout);
		}
	}
}
