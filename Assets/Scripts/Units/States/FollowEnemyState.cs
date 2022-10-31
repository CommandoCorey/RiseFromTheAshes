using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FollowEnemyState : State
{
    private NavMeshAgent agent;
    private new Vector3 directionToTarget;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponentInChildren<NavMeshAgent>();        

        // start the navmesh agent again
        if (agent.isStopped)
            agent.isStopped = false;
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (target == null || unit.UnitHalt)        
            return;

        // check if there is a closer target if auto attacking
        if (!unit.AttackOrderGiven)
        {            
            var unitsInRange = GetEnemiesInRange();
            var closest = GetClosestEnemy(unitsInRange.ToArray(), unit.AttackTarget);

            if (closest && closest != unit.AttackTarget) // closest enemy was found
            {
                unit.AttackTarget = closest;

                // update path
                agent.SetDestination(unit.AttackTarget.position);

                HandleEnemyInRange();
            }
        }

        if (unit.AttackTarget != null)
        {
            directionToTarget = (unit.AttackTarget.position - unit.body.position).normalized;

            agent.SetDestination(unit.AttackTarget.position + directionToTarget * unit.AttackRange);

            // check that the line of sight is vacant and we are in attack range
            if (!ObstacleInWay(directionToTarget) &&
                Vector3.Distance(unit.body.position, unit.AttackTarget.position) <= unit.AttackRange)
            {
                agent.isStopped = true;
                unit.ChangeState(UnitState.Attack);
            }
        }
    }

    private void OnDestroy()
    {
        unit.AttackOrderGiven = false;
    }

    private void OnDrawGizmos()
    {
        // Draw line to enemy
        Gizmos.color = Color.red;
        Gizmos.DrawLine(unit.body.position, unit.body.position + directionToTarget * unit.DetectionRadius);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(unit.body.position + Vector3.up * 5, "Moving To Enemy");
#endif
    }

}
