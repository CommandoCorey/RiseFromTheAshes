using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : MonoBehaviour
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
    }

    void Update()
    {
        var enemiesInRange = Physics.OverlapSphere(transform.position, unit.DetectionRadius, unit.DetectionLayer);

        // if there are any enemies in range change to the combat state
        if (enemiesInRange.Length > 0)
        {
            //Debug.Log("Detected Enemy: " + enemiesInRange[0].gameObject.name);

            unit.AttackTarget = enemiesInRange[0].gameObject.transform;
            unit.ChangeState(UnitState.Attack);
        }
    }

    private void OnDrawGizmos()
    {
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1, "Idle");
    }

}
