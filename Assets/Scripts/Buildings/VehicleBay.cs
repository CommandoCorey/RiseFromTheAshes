using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class VehicleBay : MonoBehaviour {
	public Transform spawnLocation;
	[SerializeField] ProgressBar buildProgress;

	[SerializeField] Vector3 buildMenuOffset;

	Building building;

	[Space]

	[SerializeField] float healUnitRadius;
	[SerializeField] float healUnitPerSecond;
	[SerializeField] LayerMask unitLayer;

	[HideInInspector] public UnitDesc currentUnitDesc;

	[HideInInspector] public float buildTimer = 0.0f;
	[HideInInspector] public int buildingIndex;
	[HideInInspector] public bool isBuilding;

	[SerializeField] public TMPro.TextMeshProUGUI errorText;

	float healPulseTimer = 0.0f;

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
	}
	public void PrepareBuild()
	{
		buildProgress.gameObject.SetActive(true);
	}

	public bool Interact()
	{
		if (building != null && building.IsBuilt) {
			if (isBuilding)
			{
                //Notify.Queue("This vehicle bay is busy.", 1.5f);
                BuildingInfo.Instance.ShowUnitBeingBuilt(this, currentUnitDesc.unit);
                return false;
			}

			//VehicleBayBuildMenu.Instance.transform.position = transform.position + buildMenuOffset;
			VehicleBayBuildMenu.Instance.currentVehicleBay = this;
			VehicleBayBuildMenu.Instance.gameObject.SetActive(true);

			return true;
		}

		return false;
	}

	public void HideMenu()
	{
        VehicleBayBuildMenu.Instance.gameObject.SetActive(false);
    }

	public void Update()
	{
		if (building.IsBuilt)
		{
			if (isBuilding && !building.aiBuilding) {
				UnitDesc desc = currentUnitDesc;

				float timeToBuild = desc.unit.TimeToTrain;

				buildTimer += Time.deltaTime / timeToBuild;

				buildProgress.progress = buildTimer;

				if (buildTimer >= 1.0f)
				{
					var unit = Instantiate(desc.unit, spawnLocation.position, spawnLocation.rotation);

					//Added by Paul
					unit.MoveToRallyPoint(UnitManager.Instance.playerRallyPoint.position);

					buildProgress.gameObject.SetActive(false);

					var buildingInfo = BuildingInfo.Instance;

					// Added By Paul
					if (buildingInfo.BuildingUnit)
					{
						BuildingInfo.Instance.HidePanel();
						VehicleBayBuildMenu.Instance.gameObject.SetActive(true);
					}

                    isBuilding = false;
				}
			}

			healPulseTimer += Time.deltaTime;
			if (healPulseTimer >= 1.0f)
			{
				healPulseTimer = 0.0f;
				Collider[] overlapping = Physics.OverlapSphere(transform.position, healUnitRadius, unitLayer);
				foreach (Collider o in overlapping)
				{
					UnitController uc;
					if (!o.gameObject.GetComponent<Transform>().TryGetComponent(out uc)) { continue; }

					uc.Heal(healUnitPerSecond);
				}
			}
		}
	}

	public void BuildUnit(UnitController unit)
	{
        ResourceManager.Instance.SpendSteel(unit.Cost);

        PrepareBuild();
        isBuilding = true;        
        buildTimer = 0.0f;       
    }

	private void OnDrawGizmosSelected()
	{
		Gizmos.DrawWireSphere(transform.position, healUnitRadius);
	}

	// Added by Paul
	private void OnDestroy()
	{
		// check if this vehicle bay is selected and if it is hide the menu
		if(SelectionManager.Instance.SelectedBuilding == building)
			HideMenu();
    }
}
