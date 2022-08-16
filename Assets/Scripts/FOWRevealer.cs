using UnityEngine;

public class FOWRevealer : MonoBehaviour
{
	[SerializeField] int radius = 5;

	void Update()
	{
		FOWManager.DrawCircle(transform.position, radius);
	}
}
