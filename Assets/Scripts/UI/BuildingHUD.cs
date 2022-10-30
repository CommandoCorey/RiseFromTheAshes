using UnityEngine;

public class BuildingHUD : MonoBehaviour
{
	[SerializeField] ProgressBar healthBar;

	Building building;

	private void Start()
	{
		building = transform.root.GetComponent<Building>();

		if (building == null)
		{
			Debug.LogError("BuildingHUD must have a parent that has a Building script attached to it.");
		}

		healthBar.maxValue = building.maxHP;
	}

	private void Update()
	{
		healthBar.progress = building.HPPerc;
	}
}
