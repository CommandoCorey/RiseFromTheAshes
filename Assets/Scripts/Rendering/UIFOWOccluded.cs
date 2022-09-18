using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/* This script disables the rendering of the game object
 * if it is occluded by the fog of war. For UI objects
 * only, such as health bars. */
public class UIFOWOccluded : MonoBehaviour
{
	Canvas[] childCanvases;
	Canvas myCanvas;

	void EnableRendering(bool enable)
	{
		if (childCanvases != null)
		{
			foreach (var m in childCanvases)
			{
				m.enabled = enable;
			}
		}

		if (myCanvas != null)
		{
			myCanvas.enabled = enable;
		}
	}
	void PopulateCanvasRefs()
	{
		myCanvas = GetComponent<Canvas>();
		childCanvases = GetComponentsInChildren<Canvas>();
	}

	private void Start()
	{
		PopulateCanvasRefs();
	}

	private void Update()
	{
		var f = FOWManager.Instance.imperm;
		if (!f) { return; }
		if (f.GetMask(f.WorldPosToMaskPos(transform.position)))
		{
			EnableRendering(true);
		} else
		{
			EnableRendering(false);
		}
	}
}
