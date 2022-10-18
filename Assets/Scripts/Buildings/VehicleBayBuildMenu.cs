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

					if (GameManager.Instance.UnitCountPlayer < GameManager.Instance.MaxUnitsPlayer)
					{
						/*
						currentVehicleBay.PrepareBuild();
						currentVehicleBay.isBuilding = true;
						ResourceManager.Instance.SpendSteel(ud.unit.Cost);
						currentVehicleBay.buildTimer = 0.0f;*/
						currentVehicleBay.BuildUnit(ud.unit);

						currentVehicleBay.currentUnitDesc = ud;
						Hide();						
					}
					else
					{
                        //Hide();
                        // Display not enough room text
						StartCoroutine(ShowNotification("Not enough space"));
                    }

				}
				else
				{
                    // Display not enough steel text
                    StartCoroutine(ShowNotification("Not enough steel"));
                }

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

	private IEnumerator ShowNotification(string message)
	{
		errorNotification.gameObject.SetActive(true);
		errorNotification.text = message;

        yield return new WaitForSeconds(notificationTimeout);

        errorNotification.gameObject.SetActive(false);
    }
}
