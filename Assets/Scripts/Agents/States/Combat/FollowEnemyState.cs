using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FollowEnemyState : State
{
    private NavMeshAgent agent;
    private Vector3 directionToTarget;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();        

        // start the navmesh agent again
        if (agent.isStopped)
            agent.isStopped = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null || unit.UnitHalt)        
            return;

        directionToTarget = (unit.AttackTarget.position - transform.position).normalized;

        agent.SetDestination(target);

        // check if there is a closer target
        var unitsInRange = Physics.OverlapSphere(transform.position, unit.DetectionRadius, unit.DetectionLayer);
        var closest = GetClosestEnemy(unitsInRange, unit.AttackTarget);

        if (closest != unit.AttackTarget) // closest enemy was found
        {
            unit.AttackTarget = closest;

            // update path
            agent.SetDestination(unit.AttackTarget.position);

            HandleEnemyInRange();
        }

        // check that the line of sight is vacant and we are in attack range
        if (!ObstacleInWay(directionToTarget) &&
            Vector3.Distance(transform.position, unit.AttackTarget.position) <= unit.AttackRange)
        {            
            agent.isStopped = true;
            unit.ChangeState(UnitState.Attack);
        }
    }

    // Searches through all detected enemies in the overlap sphere and returns the transform
    // of the one that is the closest
    private Transform GetClosestEnemy(Collider[] enemies, Transform current = null)
    {
        Transform closest;
        float shortestDistance;

        if (current != null)
        {
            closest = current;
            shortestDistance = Vector3.Distance(transform.position, current.position);
        }
        else
        {
            closest = enemies[0].transform;
            shortestDistance = Vector3.Distance(transform.position, enemies[0].transform.position);
        }

        foreach (Collider enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closest = enemy.transform;
            }
        }

        return closest;
    }


    private void OnDrawGizmos()
    {
        // Draw line to enemy
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + directionToTarget * unit.DetectionRadius);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 5, "Moving To Enemy");
#endif
    }

}
