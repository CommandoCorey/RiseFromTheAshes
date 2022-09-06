using UnityEngine;

public class FOWRevealer : MonoBehaviour
{
	[SerializeField] int permRadius = 10;
	[SerializeField] int impermRadius = 5;

	void Update()
	{
		FOWManager.Instance.perm.MaskDrawCircle(FOWManager.Instance.perm.WorldPosToMaskPos(transform.position), permRadius);
		FOWManager.Instance.imperm.MaskDrawCircle(FOWManager.Instance.imperm.WorldPosToMaskPos(transform.position), impermRadius);
	}
}
