using UnityEngine;
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

	public void Interact()
	{
		if (building != null && building.IsBuilt) {
			VehicleBayBuildMenu.Instance.transform.position = transform.position + buildMenuOffset;
			VehicleBayBuildMenu.Instance.currentVehicleBay = this;
			VehicleBayBuildMenu.Instance.gameObject.SetActive(true);
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
					Instantiate(desc.prefab, spawnLocation.position, spawnLocation.rotation);
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
