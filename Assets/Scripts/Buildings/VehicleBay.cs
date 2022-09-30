using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
class UnitDesc
{
	public string name;
	public GameObject prefab;
	public float timeToBuild;
	public Button buildButton;
	public int steelCost;
	[HideInInspector] public int index;
}

public class VehicleBay : MonoBehaviour {
	[SerializeField] List<UnitDesc> units = new List<UnitDesc>();
	[SerializeField] Transform spawnLocation;
	[SerializeField] ProgressBar buildProgress;

	[SerializeField] GameObject buildMenu;

	[SerializeField] Button cancelButton;
	Building building;

	[Space]

	[SerializeField] float healUnitRadius;
	[SerializeField] float healUnitPerSecond;
	[SerializeField] LayerMask unitLayer;

	UnitDesc currentUnitDesc;

	float buildTimer = 0.0f;
	int buildingIndex;
	bool isBuilding;

	// properties
	public bool IsTraining { get => isBuilding; set => isBuilding = value; }

	private void OnEnable()
	{	
		building = GetComponent<Building>();
		if (building == null) {
			Debug.LogError("Game objects with the VehicleBay component must  also have a Building component.");
		}

		if (building.aiBuilding)
			return;

		buildTimer = 100.0f;
		buildProgress.progress = 0.0f;
		buildProgress.gameObject.SetActive(false);
		buildMenu.SetActive(false);

		foreach (UnitDesc ud in units)
		{
			ud.buildButton.onClick.AddListener(() => {
				PrepareBuild();
				isBuilding = true;
				ResourceManager.Instance.SpendSteel(ud.steelCost);
				buildTimer = 0.0f;
				currentUnitDesc = ud;
			});
		}

		cancelButton.onClick.AddListener(() => {
			buildMenu.SetActive(false);
		});
	}
	void PrepareBuild()
	{
		buildProgress.gameObject.SetActive(true);
		buildMenu.SetActive(false);
	}

	public void Interact()
	{
		if (building != null && building.IsBuilt) {
			buildMenu.SetActive(true);
		}
	}

	public void Update()
	{
		if (building.IsBuilt)
		{
			if (isBuilding && !building.aiBuilding) {
				UnitDesc desc = currentUnitDesc;

				float timeToBuild = desc.prefab.GetComponent<UnitController>().TimeToTrain;

				buildTimer += Time.deltaTime / timeToBuild;

				buildProgress.progress = buildTimer;

				if (buildTimer >= 1.0f)
				{
					Instantiate(desc.prefab, spawnLocation.position, Quaternion.identity);
					buildProgress.gameObject.SetActive(false);

					isBuilding = false;
				}
			}

			Collider[] overlapping = Physics.OverlapSphere(transform.position, healUnitRadius, unitLayer);
			foreach (Collider o in overlapping)
			{
				UnitController uc;
				if (!TryGetComponent(out uc)) { continue; }

				uc.Heal(Time.deltaTime * healUnitPerSecond);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.DrawWireSphere(transform.position, healUnitRadius);
	}
}
