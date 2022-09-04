using UnityEngine;

public class Ghost : MonoBehaviour
{
	[SerializeField] BuildMenu buildMenu;
	[SerializeField] Vector3 buildMenuOffset;

	public void ShowBuildMenu()
	{
		buildMenu.transform.position = transform.position + buildMenuOffset;
		buildMenu.ghostBuilding = this;
		buildMenu.gameObject.SetActive(true);
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.DrawSphere(transform.position + buildMenuOffset, 0.1f);
	}
}
