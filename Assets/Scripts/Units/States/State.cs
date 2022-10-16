using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class State : MonoBehaviour
{
    protected Vector3 target;
    protected Vector3 finalTarget;
    protected Vector3 formationTarget;
    protected Vector3 directionToTarget;

    protected UnitController unit;
   // protected AgentMovement agent;

    public Vector3 Target { get => target; set => target = value; }
    public Vector3 FormationTarget { set => formationTarget = value; }

    protected virtual void Awake()
    {
        unit = GetComponent<UnitController>();
        //agent = GetComponent<AgentMovement>();
    }

    protected virtual void Update()
    {

    }

    #region protected functions
    protected void HandleEnemies()
    {
        var enemiesInRange = GetEnemiesInRange();

        if (enemiesInRange.Count > 0)
        {
            unit.AttackTarget = GetClosestEnemy(enemiesInRange.ToArray());
            HandleEnemyInRange();
        }
    }

    protected List<Collider> GetEnemiesInRange()
    {
        //int unitLayer = (int)Mathf.Log(unit.EnemyUnitLayer.value, 2);
        //int buildingLayer = (int)Mathf.Log(unit.EnemyBuildingLayer.value, 2);

        List<Collider> enemiesFound = new List<Collider>();

        //if (unitLayer != 1)
        //{
        int layerMaskVal = unit.EnemyUnitLayer;
        var unitsInRange = Physics.OverlapSphere(unit.body.position, unit.DetectionRadius, layerMaskVal);
        //var objectsInRange = Physics.OverlapSphere(unit.body.position, unit.DetectionRadius);

        enemiesFound.AddRange(unitsInRange);

        layerMaskVal = unit.EnemyBuildingLayer;
        var buildingsInInRange = Physics.OverlapSphere(unit.body.position, unit.DetectionRadius, layerMaskVal);
        enemiesFound.AddRange(buildingsInInRange);          

        return enemiesFound;
    }

    // Searches through all detected enemies in the overlap sphere and returns the transform
    // of the one that is the closest
    protected Transform GetClosestEnemy(Collider[] enemies, Transform current = null)
    {
        Transform closest = null;
        float shortestDistance = float.MaxValue;

        if (current != null)
        {
            closest = current;
            shortestDistance = Vector3.Distance(unit.body.position, current.position);
        }
        else if (enemies.Length > 0)
        {
            closest = enemies[0].transform;
            shortestDistance = Vector3.Distance(unit.body.position, enemies[0].transform.position);
        }

        foreach (Collider enemy in enemies)
        {
            float distance = Vector3.Distance(unit.body.position, enemy.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closest = enemy.transform;
            }
        }

        return closest;
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
    #endregion

}
