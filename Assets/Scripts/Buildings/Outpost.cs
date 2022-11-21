using UnityEngine;

public class Outpost : MonoBehaviour
{
	Building building;
	[SerializeField] Ghost[] ghosts;

	[SerializeField] int maxUnitIncrease = 5;

	private void Start()
	{
		building = GetComponent<Building>();

		if (!building)
		{
			Debug.LogError("Outpost needs a building on the same object.");
		}

		ghosts = GetComponentsInChildren<Ghost>();
		foreach (var ghost in ghosts)
		{
			ghost.gameObject.SetActive(false);
		}
	}

	public void OnBuilt()
	{
		foreach (var ghost in ghosts)
		{
			ghost.gameObject.SetActive(true);
		}

		GameManager.Instance.IncreaseMaxUnits(maxUnitIncrease, gameObject.layer == 9);
	}
}
