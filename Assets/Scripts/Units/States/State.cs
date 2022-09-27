using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State : MonoBehaviour
{
    protected Vector3 target;
    protected Vector3 finalTarget;
    protected Vector3 formationTarget;
    protected Vector3 directionToTarget;

    protected UnitController unit;
    protected AgentMovement agent;

    public Vector3 Target { get => target; set => target = value; }
    public Vector3 FormationTarget { set => formationTarget = value; }

    protected virtual void Awake()
    {
        unit = GetComponent<UnitController>();
        agent = GetComponent<AgentMovement>();
    }

    protected virtual void Update()
    {

    }

    // functions
    protected List<Collider> GetEnemiesInRange()
    {
        var unitsInRange = Physics.OverlapSphere(unit.body.position, unit.DetectionRadius, unit.EnemyUnitLayer);
        var buildingsInInRange = Physics.OverlapSphere(unit.body.position, unit.DetectionRadius, unit.EnemyBuildingLayer);

        List<Collider> enemiesFound = new List<Collider>();

        enemiesFound.AddRange(unitsInRange);
        enemiesFound.AddRange(buildingsInInRange);

        return enemiesFound;
    }


    protected void HandleEnemyInRange()
    {
        if (ObstacleInWay(directionToTarget) && !unit.UnitHalt)
        {
            //pathToTarget = agent.CreatePath(target.position);
            //state = CombatMode.MoveTowards;
            unit.ChangeState(UnitState.Follow);
        }
        else if (Vector3.Distance(unit.body.position, unit.AttackTarget.position) <= unit.AttackRange)
        {
            unit.ChangeState(UnitState.Idle);
        }
        else if (!unit.UnitHalt)
        {
            
        }
    }

    protected bool ObstacleInWay(Vector3 direction)
    {
        //Debug.DrawLine(unit.firingPoint.position, transform.position + direction * unit.DetectionRadius, Color.red, 1.0f);

        if (Physics.Raycast(unit.body.position, direction, out RaycastHit hit, unit.DetectionRadius))
        {
            //Debug.Log("Raycast from " + transform.name + " hit " + hit.transform.name);

            int layerNum = (int)Mathf.Log(unit.EnvironmentLayer.value, 2);

            if (hit.transform.gameObject.layer == layerNum)
            {
                //Debug.Log("Hit " + hit.transform.name);
                return true;
            }
        }

        return false;
    }

}
