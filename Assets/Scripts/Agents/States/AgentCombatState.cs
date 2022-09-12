using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AgentCombatState : MonoBehaviour
{
    #region variables
    private UnitController unit;

    private Transform target;
    private Quaternion lookRotation;
    private Vector3 direction;
    private Vector3 initialRotation;

    private NavMeshAgent agent;
    private CombatMode state = CombatMode.Search;

    private Vector3[] pathToTarget;
    private int waypointNum = 1;

    private float distanceFromWaypoint = 1;
    private Vector3 directionToTarget;

    Vector3 castingOffset = Vector3.up;    
    #endregion

    void Awake()
    {
        unit = GetComponent<UnitController>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        target = unit.AttackTarget;

        initialRotation = unit.turret.localRotation.eulerAngles;

        direction = (target.position - unit.turret.position).normalized;
        lookRotation = Quaternion.LookRotation(direction);

        // set the initial state
        if (target == null)
        {
            state = CombatMode.Search;
        }
        else
        {
            HandleEnemyInRange();
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
            directionToTarget = (target.position - transform.position).normalized;

        switch (state)
        {
            case CombatMode.Search:
                LookForTargets();
                break;

            case CombatMode.MoveTowards:
                MoveTowardsTarget();
                break;

            case CombatMode.Follow:
               
                break;

            case CombatMode.Aim: Aim();
                break;

            case CombatMode.Fire:
                break;
        }

    }

    // deals damage to enemy once every x amount of seconds
    private void DealDamage()
    {
        if (target != null)
        {
            // check if target is still in attack range
            if (Vector3.Distance(transform.position, target.position) > unit.AttackRange)
            {
                if (unit.UnitHalt)
                    state = CombatMode.Search;
                else
                    state = CombatMode.MoveTowards;
            }

            // check if the target is a unit
            else if (target.gameObject.layer == 6 || target.gameObject.layer == 7)
            {
                target.GetComponent<UnitController>().TakeDamage(unit.DamagePerHit);
                Invoke("DealDamage", unit.AttackRate);
            }
            // check if the target is a building
            else if (target.gameObject.layer == 8 || target.gameObject.layer == 9)
            {
                Debug.Log("Dealing " + unit.DamagePerHit + " damage to " + target.name);
                Invoke("DealDamage", unit.AttackRate);
            }

        }
        else
        {
            state = CombatMode.Search;
        }
    }

    private void LookForTargets()
    {
        // check for other units in vision radius
        var unitsInRange = Physics.OverlapSphere(transform.position, unit.DetectionRadius, unit.DetectionLayer);

        if (unitsInRange.Length > 0)
        {
            target = GetClosestEnemy(unitsInRange);

            HandleEnemyInRange();
        }
        else
        {
            // rotates turret back to statrting position
            target = null;
            lookRotation = Quaternion.identity;

            if (lookRotation != Quaternion.identity)
                lookRotation = Quaternion.LookRotation(transform.forward);

            unit.turret.rotation = Quaternion.RotateTowards(unit.turret.rotation, lookRotation, Time.deltaTime * unit.TurretRotationSpeed);

            if ((unit.turret.eulerAngles.y <= (initialRotation.y + 1)) ||
                (unit.turret.eulerAngles.y >= (initialRotation.y - 1)))
            {
                unit.turret.rotation = transform.rotation;
                unit.ChangeState(UnitState.Idle);
            }
        }
    }

    // Switches state base on enemy position from current unit
    private void HandleEnemyInRange()
    {
        if (ObstacleInWay(directionToTarget) && !unit.UnitHalt)
        {
            //pathToTarget = agent.CreatePath(target.position);
            state = CombatMode.MoveTowards;
        }
        else if (Vector3.Distance(transform.position, target.transform.position) <= unit.AttackRange)
        {
            state = CombatMode.Aim;
            pathToTarget = null;
        }
        else if (!unit.UnitHalt)
        {
            state = CombatMode.MoveTowards;
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


    private void MoveTowardsTarget()
    {
        if (target == null || unit.UnitHalt)
        {
            state = CombatMode.Search;
            return;
        }

        agent.SetDestination(target.position);

        // check if there is a closer target
        var unitsInRange = Physics.OverlapSphere(transform.position, unit.DetectionRadius, unit.DetectionLayer);
        var closest = GetClosestEnemy(unitsInRange, target);

        if (closest != target) // closest enemy was found
        {
            target = closest;

            // update path
            agent.SetDestination(target.position);

            HandleEnemyInRange();
        }

        // check that the line of sight is vacant and we are in attack range
        if (Vector3.Distance(transform.position, target.transform.position) <= unit.AttackRange)
        {
            state = CombatMode.Aim;
            pathToTarget = null;
        }

    }


    private void Aim()
    {
        if (target == null)
        {
            if (unit.UnitHalt)
                state = CombatMode.Search;
            else
                state = CombatMode.MoveTowards;
            return;
        }

        if (ObstacleInWay(directionToTarget))
        {
            //Debug.Log("Obstacle is in way");

            if (unit.UnitHalt)
            {
                state = CombatMode.Search;
            }
            else
            {
                agent.SetDestination(target.position);
                state = CombatMode.MoveTowards;
            }

            return;
        }

        // rotate us over time according to speed until we are in the required rotation  
        //unit.turret.rotation = Quaternion.Slerp(unit.turret.rotation, lookRotation, Time.deltaTime * unit.TurretRotationSpeed);
        unit.turret.rotation = Quaternion.RotateTowards(unit.turret.rotation, lookRotation, Time.deltaTime * unit.TurretRotationSpeed);
        // revert x and z rotation back to there original value
        unit.turret.localRotation = Quaternion.Euler(initialRotation.x, unit.turret.localRotation.eulerAngles.y, initialRotation.z);

        //Debug.Log("Enemy target detected");
        RaycastHit hit;

        // check if turret is pointing at target
        if (Physics.Raycast(unit.firingPoint.position, unit.turret.forward, out hit) && hit.transform == target)
        {
            Debug.DrawLine(unit.turret.position, target.position, Color.yellow);

            Invoke("DealDamage", unit.AttackRate);
            state = CombatMode.Fire;

        }
        else
        {
            // find the vector pointing from our position to the target
            direction = (target.position - unit.turret.position).normalized;
        }

        //create the rotation we need to be in to look at the target
        if (lookRotation != Quaternion.identity)
            lookRotation = Quaternion.LookRotation(direction);
    }

    private bool ObstacleInWay(Vector3 direction)
    {
        //Debug.DrawLine(unit.firingPoint.position, transform.position + direction * unit.DetectionRadius, Color.red, 1.0f);

        if (Physics.Raycast(unit.castingPoint.position, direction, out RaycastHit hit, unit.DetectionRadius))
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

    private void OnDrawGizmos()
    {

        if (gameObject.layer == 7)
        {
            Gizmos.color = Color.red;        
            Gizmos.DrawLine(unit.castingPoint.position, unit.castingPoint.position + directionToTarget * unit.DetectionRadius);
        }

        Debug.DrawLine(unit.firingPoint.position, unit.firingPoint.position + (unit.turret.forward * unit.AttackRange), Color.yellow);
        //Gizmos.DrawLine(transform.position, transform.position + (directionToTarget * unit.DetectionRadius));

        string combatMode = "";
        switch (state)
        {
            case CombatMode.Search:
                combatMode = "Searching";
                break;

            case CombatMode.MoveTowards:
                combatMode = "Following Path";
                break; ;

            case CombatMode.Follow:
                combatMode = "Following Enemy";
                break;

            case CombatMode.Aim:
                combatMode = "Aiming";
                break;

            case CombatMode.Fire:
                combatMode = "Firing";
                break;
        }

        UnityEditor.Handles.Label(transform.position + Vector3.up * 5, combatMode);

        if (pathToTarget != null && pathToTarget.Length > 0)
        {
            for (int i = 0; i < pathToTarget.Length - 1; i++)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(pathToTarget[i], pathToTarget[i + 1]);
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(pathToTarget[i + 1], distanceFromWaypoint);
            }
        }
    }

}
