using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class UnitDesc
{
	public string name;
	public GameObject prefab;
	public float timeToBuild;
	public Button buildButton;
	public int steelCost;
	[HideInInspector] public int index;
}


public class VehicleBayBuildMenu : MonoBehaviour {
	static public VehicleBayBuildMenu Instance { get; private set; }

	[SerializeField] List<UnitDesc> units = new List<UnitDesc>();
	[SerializeField] Button cancelButton;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this);
		}
		else
		{
			Instance = this;
		}
	}

	[HideInInspector] public VehicleBay currentVehicleBay;

	private void Start()
	{

		foreach (UnitDesc ud in units)
		{
			ud.buildButton.onClick.AddListener(() => {
				currentVehicleBay.PrepareBuild();
				currentVehicleBay.isBuilding = true;
				//ResourceManager.Instance.SpendSteel(ud.steelCost);
				currentVehicleBay.buildTimer = 0.0f;
				currentVehicleBay.currentUnitDesc = ud;
				Hide();
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
}
