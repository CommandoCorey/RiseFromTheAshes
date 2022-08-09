using UnityEngine;

public class FOWRevealer : MonoBehaviour
{
	[SerializeField] FOW FOWObject;
	[SerializeField] int radius = 5;

	void Update()
	{
		FOWObject.MaskDrawCircle(FOWObject.WorldPosToMaskPos(transform.position), radius);
	}
}
