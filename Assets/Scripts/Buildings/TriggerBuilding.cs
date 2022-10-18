using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class OnTriggerFunc : UnityEvent<TriggerBuilding, Building> {}

public class TriggerBuilding : MonoBehaviour
{
	public OnTriggerFunc onTrigger;

	public void OnDie()
	{
		onTrigger.Invoke(this, GetComponent<Building>());
	}
}
