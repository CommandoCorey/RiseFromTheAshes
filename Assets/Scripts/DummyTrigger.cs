using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyTrigger : MonoBehaviour
{
	public void OnTrigger(TriggerBuilding self, Building building)
	{
		Destroy(self.gameObject);
	}
}
