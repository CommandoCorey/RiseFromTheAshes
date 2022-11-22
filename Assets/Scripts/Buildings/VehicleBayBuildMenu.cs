using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Resources;
using TMPro;
using System.Collections;

[System.Serializable]
public class UnitDesc
{
	public string name;
	public UnitController unit;
	public Button buildButton;
	[HideInInspector] public int index;
}

public class VehicleBayBuildMenu : MonoBehaviour {
	static public VehicleBayBuildMenu Instance { get; private set; }

	[SerializeField] List<UnitDesc> units = new List<UnitDesc>();
	[SerializeField] Button cancelButton;
	[SerializeField] TextMeshProUGUI errorNotification;
	[SerializeField] float notificationTimeout = 1;
	
	[SerializeField] bool hideOnConstructUnit = false;

	[Header("Unit Info Panel")]
	[SerializeField] GameObject infoPanel;
	[SerializeField] TextMeshProUGUI unitName;
	[SerializeField] TextMeshProUGUI cost;
    [SerializeField] TextMeshProUGUI spaceUsed;
    [SerializeField] TextMeshProUGUI timeToBuild;
	[SerializeField] TextMeshProUGUI maxHealth;
	[SerializeField] TextMeshProUGUI attackRange;
	[SerializeField] TextMeshProUGUI damagePerSecond;
	[SerializeField] TextMeshProUGUI movementSpeed;

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

				if (ResourceManager.Instance.GetResource(ResourceType.Steel) >= ud.unit.Cost)
				{
					int newUnitCount = GameManager.Instance.UnitCountPlayer + ud.unit.SpaceUsed;

                    if (newUnitCount <= GameManager.Instance.MaxUnitsPlayer)
					{
						/*
						currentVehicleBay.PrepareBuild();
						currentVehicleBay.isBuilding = true;
						ResourceManager.Instance.SpendSteel(ud.unit.Cost);
						currentVehicleBay.buildTimer = 0.0f;*/
						currentVehicleBay.BuildUnit(ud.unit);

						currentVehicleBay.currentUnitDesc = ud;
						
						if (hideOnConstructUnit) {
							Hide();
						}
					}
					else
					{
						//Hide();
						// Display not enough room text
						Notify.Queue("Not enough space.", 1.0f);
					}

				}
				else
				{
                    // Display not enough steel text
					Notify.Queue("Not enough steel.", 1.0f);
				}

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

	// Added by Paul
	public void ShowUnitInfo(int number)
    {
		infoPanel.SetActive(true);

		var unit = units[number].unit;

		unitName.text = unit.Name;
		cost.text = unit.Cost.ToString();
		timeToBuild.text = unit.TimeToTrain.ToString();

		spaceUsed.text = Mathf.Round(unit.SpaceUsed).ToString();
		maxHealth.text = Mathf.Round(unit.MaxHealth).ToString();
		attackRange.text = Mathf.Round(unit.AttackRange).ToString();
		damagePerSecond.text = Mathf.Round(unit.DPS).ToString();
		movementSpeed.text = Mathf.Round(unit.Speed).ToString();
	}

	// Added by Paul
	public void HideUnitInfo()
    {
		infoPanel.SetActive(false);
	}
}
