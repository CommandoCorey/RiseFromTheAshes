using UnityEngine;

public class Ghost : MonoBehaviour
{
	[SerializeField] Vector3 buildMenuOffset;

	public void ShowBuildMenu()
	{
		BuildMenu.Instance.transform.position = transform.position + buildMenuOffset;
		BuildMenu.Instance.ghostBuilding = this;
		BuildMenu.Instance.gameObject.SetActive(true);
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.DrawSphere(transform.position + buildMenuOffset, 0.1f);
	}
}
