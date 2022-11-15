using UnityEngine;

public class FOWRevealer : MonoBehaviour
{
	[SerializeField] int permRadius = 10;
	[SerializeField] int impermRadius = 5;

	void LateUpdate()
	{
		if (FOWManager.Instance) {
			FOWManager.Instance.imperm.MaskDrawCircle(FOWManager.Instance.imperm.WorldPosToMaskPos(transform.position), impermRadius);
			FOWManager.Instance.perm.MaskDrawCircle(FOWManager.Instance.perm.WorldPosToMaskPos(transform.position), permRadius);
		}
	}
}
