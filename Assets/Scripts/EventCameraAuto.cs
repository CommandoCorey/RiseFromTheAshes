using UnityEngine;

public class EventCameraAuto : MonoBehaviour
{
	private void OnEnable()
	{
		Canvas c;
		if (TryGetComponent(out c))
		{
			c.worldCamera = Camera.main;
		}
	}
}
