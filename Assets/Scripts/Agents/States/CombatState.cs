using System;
using System.Collections;
using UnityEngine;

public class CombatState : MonoBehaviour
{
    enum CombatMode
    {
        Search, MoveTowards, Aim, Fire, Follow, Halt
    }

    #region variables
    private UnitController unit;

    private Transform target;
    private Quaternion lookRotation;    
    private Vector3 direction;
    private Vector3 offsetRotation;

    private AgentMovement agent;
    private SeekBehaviour seek;
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
        agent = GetComponent<AgentMovement>();
        offsetRotation.x = unit.turret.localRotation.eulerAngles.x;
        offsetRotation.y = unit.turret.localRotation.eulerAngles.y;
        offsetRotation.z = unit.turret.localRotation.eulerAngles.z;
    }

    void Start()
    {
        target = unit.AttackTarget.transform;

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
        if(target != null)
            directionToTarget = (target.position - transform.position).normalized;

        switch (state)
        {
            case CombatMode.Search: LookForTargets();
                break;

            case CombatMode.MoveTowards: MoveTowardsTarget();
                break;

            case CombatMode.Follow: FollowTarget();
                break;

            case CombatMode.Aim: Aim();
                break;

            case CombatMode.Fire:
                break;
        }
        
    }

    private void DealDamage()
    {
        if (target != null)
        {
            target.GetComponent<UnitController>().TakeDamage(unit.DamagePerHit);
            Invoke("DealDamage", unit.AttackRate);
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

            unit.turret.rotation = Quaternion.Slerp(unit.turret.rotation, lookRotation, Time.deltaTime * unit.TurretRotationSpeed);

            if ((unit.turret.rotation.y <= (transform.rotation.y + 1)) ||
                (unit.turret.rotation.y >= (transform.rotation.y - 1)))
            {
                unit.turret.rotation = transform.rotation;
                unit.ChangeState(UnitState.Idle);
            }
        }
    }

    // Switches state base on enemy position from current unit
    private void HandleEnemyInRange()
    {
        if (ObstacleInWay(directionToTarget))
        {
            //pathToTarget = agent.CreatePath(target.position);
            state = CombatMode.MoveTowards;
        }        
        else if (Vector3.Distance(transform.position, target.transform.position) <= unit.AttackRange)
        {

            state = CombatMode.Aim;
            pathToTarget = null;

            if (seek != null)
                Destroy(seek);
        }
        else
        {
            state = CombatMode.Follow;
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

        foreach(Collider enemy in enemies)
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
        if(target == null)
        {
            state = CombatMode.Search;
            return;
        }

        if (seek == null)
        {
            seek = gameObject.AddComponent<SeekBehaviour>();
            seek.weight = 1;

            Steering steer = seek.GetSteering();
            agent.AddSteering(steer, seek.weight);

            SetNewPath();           
        }

        // check if there is a closer target
        var unitsInRange = Physics.OverlapSphere(transform.position, unit.DetectionRadius, unit.DetectionLayer);
        var closest = GetClosestEnemy(unitsInRange, target);

        if (closest != target) // closer enemy was found
        {
            target = closest;

            // update path
            agent.CreatePath(target.position);

            HandleEnemyInRange();
        }

        Vector3 directionToTarget = (target.position - transform.position).normalized;
        // check that the line of sight is vacant and we are in attack range
        if (!ObstacleInWay(directionToTarget) &&
            Vector3.Distance(transform.position, target.transform.position) <= unit.AttackRange)
        {
            state = CombatMode.Aim;
            pathToTarget = null;

            if (seek != null)
                Destroy(seek);
        }
        // check if the next waypoint has been reached
        else
        {
            try
            {
                if(pathToTarget == null)
                {
                    SetNewPath();
                }

                if (Vector3.Distance(transform.position, pathToTarget[waypointNum]) <= distanceFromWaypoint)
                {
                    agent.StopMoving();
                    waypointNum++;

                    // if at the end of the path create a new one
                    if (waypointNum >= pathToTarget.Length)
                    {
                        pathToTarget = agent.CreatePath(target.position);
                        waypointNum = 1;
                    }

                    seek.target = pathToTarget[waypointNum];
                }
            }
            catch(Exception e)
            {
                Debug.LogError(e.StackTrace);
            }
        }
        
    }

    private void Aim()
    {
        if (target == null)
        {
            state = CombatMode.Follow;
            return;
        }

        if (ObstacleInWay(directionToTarget))
        {
            //Debug.Log("Obstacle is in way");
            pathToTarget = agent.CreatePath(target.position);
            seek.target = pathToTarget[waypointNum];
            state = CombatMode.MoveTowards;
            return;
        }

        //rotate us over time according to speed until we are in the required rotation  
        unit.turret.rotation = Quaternion.Slerp(unit.turret.rotation, lookRotation, Time.deltaTime * unit.TurretRotationSpeed);

        //Debug.Log("Enemy target detected");
        RaycastHit hit;

        // check if turret is pointing at target
        if (Physics.Raycast(unit.firingPoint.position, unit.turret.forward, out hit) && hit.transform == target)
        {
            //Debug.DrawLine(unit.turret.position, target.position, Color.yellow);

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

    private void FollowTarget()
    {
        if(target == null)
        {
            state = CombatMode.Search;            
        }

        else if(ObstacleInWay(directionToTarget))
        {
            state = CombatMode.MoveTowards;            
        }

        // check we are in attack range
        else if (Vector3.Distance(transform.position, target.transform.position) <= unit.AttackRange)
        {
            state = CombatMode.Aim;
            pathToTarget = null;

            if (seek != null)
                Destroy(seek);
        }
        // if the target is not in range follow it
        else if (seek == null)
        {
            seek = gameObject.AddComponent<SeekBehaviour>();

            seek.target = target.position;
            seek.weight = 1;

            Steering steer = seek.GetSteering();
            agent.AddSteering(steer, seek.weight);
        }

    }

    private bool ObstacleInWay(Vector3 direction)
    {
        //Debug.DrawLine(unit.firingPoint.position, transform.position + direction * unit.DetectionRadius, Color.red, 1.0f);

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, unit.DetectionRadius))//, ~unit.EnvironmentLayer.value))
        {
            //Debug.Log("Raycast from " + transform.name + " hit " + hit.transform.name);

            int layerNum = (int) Mathf.Log(unit.EnvironmentLayer.value, 2);

            if (hit.transform.gameObject.layer == layerNum)
            {
                //Debug.Log("Hit " + hit.transform.name);
                return true;
            }
        }
        
        return false;
    }

    private void SetNewPath()
    {
        if(target != null)
        {
            pathToTarget = agent.CreatePath(target.position);
            waypointNum = 1;

            if (seek != null)
                seek.target = pathToTarget[waypointNum];
        }
    }

    private void OnDestroy()
    {
        if (seek != null)
            Destroy(seek);
    }

    private void OnDrawGizmos()
    {
        Debug.DrawLine(transform.position + castingOffset, transform.position + castingOffset + 
            directionToTarget * unit.DetectionRadius, Color.red);
        //Gizmos.DrawLine(transform.position, transform.position + (directionToTarget * unit.DetectionRadius));

        string combatMode = "";
        switch(state)
        {
            case CombatMode.Search: combatMode = "Searching";        
                break;

            case CombatMode.MoveTowards: combatMode = "Following Path";          
                break;;

            case CombatMode.Follow: combatMode = "Following Enemy";
                break;

            case CombatMode.Aim: combatMode = "Aiming";            
                break;

            case CombatMode.Fire: combatMode = "Firing";
                break;
        }

        UnityEditor.Handles.Label(transform.position + Vector3.up * 5, combatMode);

        /*
        if (target != null && state != CombatMode.Fire)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(unit.firingPoint.position, target.position);
        }*/

        if(pathToTarget != null && pathToTarget.Length > 0)
        {
            for(int i = 0; i< pathToTarget.Length-1; i++)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(pathToTarget[i], pathToTarget[i+1]);
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(pathToTarget[i + 1], distanceFromWaypoint);
            }
        }
    }

}
