using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
struct BuildItem
{
	public string name;
	public Building buildingPrefab;
}

public class BuildMenu : MonoBehaviour
{
	[SerializeField] Canvas canvas;
	[SerializeField] List<BuildItem> buildItems;

	[Space] [Space]
	[Header("UI")]
	[SerializeField] Button buildButtonPrefab;
	[SerializeField] float spacing;
	[HideInInspector] public Ghost ghostBuilding;

	private void Start()
	{
		float cursorY = 0.0f;

		foreach (var buildItem in buildItems) {
			Button b = Instantiate(buildButtonPrefab, canvas.transform);
			RectTransform bt = b.GetComponent<RectTransform>();

			bt.localPosition = new Vector3(0.0f, cursorY, 0.0f);

			b.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = buildItem.name;

			b.onClick.AddListener(() => {
				Building b = Instantiate(buildItem.buildingPrefab, ghostBuilding.transform.position, ghostBuilding.transform.rotation);
				ghostBuilding.gameObject.SetActive(false);
				b.Build();
				gameObject.SetActive(false);
			});

			RectTransform rt = canvas.GetComponent<RectTransform>();

			Rect r = rt.rect;
			rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
					r.height +
					bt.rect.height +
					spacing);

			cursorY -= bt.rect.height + spacing;
		}

		Button cancelButton = Instantiate(buildButtonPrefab, canvas.transform);
		RectTransform cancelButtonTransform = cancelButton.GetComponent<RectTransform>();
		cancelButtonTransform.localPosition = new Vector3(0.0f, cursorY, 0.0f);

		cancelButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Cancel";

		cancelButton.onClick.AddListener(() => {
			gameObject.SetActive(false);
		});

		{
			RectTransform rt = canvas.GetComponent<RectTransform>();

			Rect r = rt.rect;
			rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
					r.height +
					cancelButtonTransform.rect.height +
					spacing);
		}

		gameObject.SetActive(false);
	}
}
