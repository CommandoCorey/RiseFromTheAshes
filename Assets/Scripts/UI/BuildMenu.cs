using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
struct BuildItem
{
	public string name;
	public Building buildingPrefab;
	public Button button;
}

public class BuildMenu : MonoBehaviour
{
	[SerializeField] List<BuildItem> buildItems;

	[HideInInspector] public Ghost ghostBuilding;

	[SerializeField] Button cancelButton;

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
				Building b = Instantiate(buildItem.buildingPrefab, ghostBuilding.transform.position, ghostBuilding.transform.rotation);
				ghostBuilding.gameObject.SetActive(false);
				b.Build();
				gameObject.SetActive(false);
			});
		}

		cancelButton.onClick.AddListener(() => {
			gameObject.SetActive(false);
		});

		gameObject.SetActive(false);
	}
}
