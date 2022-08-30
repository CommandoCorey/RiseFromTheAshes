using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class ProgressBar : MonoBehaviour
{
	public float progress = 0.5f;

	public Image background;
	public Image foreground;

	public void Update()
	{
		progress = Mathf.Clamp(progress, 0.0f, 1.0f);

		RectTransform myRt = GetComponent<RectTransform>();

		RectTransform rt = foreground.GetComponent<RectTransform>();
		rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0.0f, progress * myRt.rect.width);
	}
}
