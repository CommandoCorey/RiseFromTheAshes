using UnityEngine;

public class Ghost : MonoBehaviour
{
	[SerializeField] Vector3 buildMenuOffset;
	[SerializeField] LayerMask ghostBlockers;

	[HideInInspector] public Building child;

	public void ShowBuildMenu()
	{
		//BuildMenu.Instance.transform.position = transform.position + buildMenuOffset;
		BuildMenu.Instance.ghostBuilding = this;
		BuildMenu.Instance.gameObject.SetActive(true);

		SelectionManager.Instance.SetPanelTooltip(false);
    }

	public void OnDrawGizmosSelected()
	{
		Gizmos.DrawSphere(transform.position + buildMenuOffset, 0.1f);
	}

	public void OnEnable()
	{
		var b = GetComponent<BoxCollider>().bounds;

		var cols = Physics.OverlapBox(b.center, b.size, Quaternion.identity, ghostBlockers);
		if (cols.Length > 0)
		{
			gameObject.SetActive(false);
		}
	}

	public void OnDestroy()
	{
		if (child) { Destroy(child.gameObject); }
	}
}
