using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[System.Serializable]
enum ProgressBarTextType {
	Value,
	Text
}

[ExecuteAlways]
public class ProgressBar : MonoBehaviour
{
	public float progress = 0.5f;

	[SerializeField] Image background;
	[SerializeField] Image foreground;

	[Header("Text")]
	[SerializeField] ProgressBarTextType textType = ProgressBarTextType.Value;
	[SerializeField] string textString;
	[SerializeField] bool showText;
	[SerializeField] TMPro.TMP_Text text;
	[SerializeField] public float maxValue;

	public void Update()
	{
		progress = Mathf.Clamp(progress, 0.0f, 1.0f);

		RectTransform myRt = GetComponent<RectTransform>();

		RectTransform rt = foreground.GetComponent<RectTransform>();
		rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0.0f, progress * myRt.rect.width);

		if (showText)
		{
			if (textType == ProgressBarTextType.Value)
			{
				text.gameObject.SetActive(true);
				text.text = Mathf.Round((progress * maxValue)).ToString() + "/" + maxValue.ToString();
			} else
			{
				text.gameObject.SetActive(true);
				text.text = textString;
			}
		} else
		{
			text.gameObject.SetActive(false);
		}
	}
}
