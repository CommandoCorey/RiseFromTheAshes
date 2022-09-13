using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HaltState : MonoBehaviour
{
    UnitController unit;
    AgentMovement agent;
    
    void Awake()
    {        
        unit = GetComponent<UnitController>();
        agent = GetComponent<AgentMovement>();
    }

    private void Start()
    {
        agent.StopMoving();

        //Invoke("SwitchToIdle", unit.HaltTime);
    }

    void Update()
    {
        var enemiesInRange = Physics.OverlapSphere(transform.position, unit.AttackRange, unit.DetectionLayer);

        // if there are any enemies in range change to the combat state
        if (enemiesInRange.Length > 0)
        {
            //Debug.Log("Detected Enemy: " + enemiesInRange[0].gameObject.name);

            unit.AttackTarget = enemiesInRange[0].gameObject.transform;
            unit.ChangeState(UnitState.Attack);
        }
    }

    private void SwitchToIdle()
    {

    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1, "Idle");
#endif
    }

}
