using System.Collections;
using UnityEngine;

public class CombatState : MonoBehaviour
{
    enum CombatBehaviour
    {
        Search, Aim, Fire, Follow
    }

    UnitController unit;

    public LayerMask detectionLayer;

    private Transform target;
    private Quaternion lookRotation;    
    private Vector3 direction;
    private Vector3 offsetRotation;

    private AgentMovement agent;
    private SeekBehaviour seek;
    private CombatBehaviour state = CombatBehaviour.Search;

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
            state = CombatBehaviour.Search;
        else if (Vector3.Distance(transform.position, target.transform.position) <= unit.AttackRange)
        {
            state = CombatBehaviour.Aim;
        }
        else
        {
            state = CombatBehaviour.Follow;
        }
    }

    // Update is called once per frame
    void Update()
    {      
        switch(state)
        {
            case CombatBehaviour.Search:
                LookForTargets();
                break;

            case CombatBehaviour.Follow:
                FollowTarget();
                break;

            case CombatBehaviour.Aim:
                Aim();
                break;

            case CombatBehaviour.Fire:
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
            state = CombatBehaviour.Search;
        }
    }

    private void LookForTargets()
    {
        // check for other units in vision radius
        var unitsInRange = Physics.OverlapSphere(transform.position, unit.DetectionRadius, unit.DetectionLayer);

        if (unitsInRange.Length > 0)
        {
            foreach (var enemy in unitsInRange)
            {
                if (Vector3.Distance(transform.position, enemy.transform.position) <= unit.AttackRange)
                {
                    target = enemy.transform;
                    state = CombatBehaviour.Aim;
                    return;
                }

            }

            // no units were in attack range so follow the first one
            target = unitsInRange[0].transform;
            state = CombatBehaviour.Follow;
        }
        else
        {
            // rotates turret back to statrting position
            target = null;
            lookRotation = Quaternion.identity;

            if (lookRotation != Quaternion.identity)
                lookRotation = Quaternion.LookRotation(transform.forward);

            unit.turret.rotation = Quaternion.Slerp(unit.turret.rotation, lookRotation, Time.deltaTime * unit.TurretRotationSpeed);

            if (unit.turret.rotation.y <= transform.rotation.y)
            {
                unit.turret.rotation = transform.rotation;
                unit.ChangeState(UnitState.Idle);
            }
        }
    }

    private void Aim()
    {
        if (target == null)
        {
            state = CombatBehaviour.Search;
            return;
        }

        //rotate us over time according to speed until we are in the required rotation  
        unit.turret.rotation = Quaternion.Slerp(unit.turret.rotation, lookRotation, Time.deltaTime * unit.TurretRotationSpeed);

        //Debug.Log("Enemy target detected");
        RaycastHit hit;

        Debug.DrawLine(unit.firingPoint.position, unit.firingPoint.position + (unit.turret.forward * 5), Color.red);

        // check if turret is pointing at target
        if (Physics.Raycast(unit.firingPoint.position, unit.turret.forward, out hit) && hit.transform == target)
        {
            //Debug.DrawLine(unit.turret.position, target.position, Color.yellow);

            Invoke("DealDamage", unit.AttackRate);
            state = CombatBehaviour.Fire;
            
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
            state = CombatBehaviour.Search;
            return;
        }

        if (Vector3.Distance(transform.position, target.transform.position) <= unit.AttackRange)
        {
            state = CombatBehaviour.Aim;

            if(seek != null)
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

    private void OnDrawGizmos()
    {
        if (target != null)
            Gizmos.DrawLine(unit.firingPoint.position, target.position);
    }

    private void OnDestroy()
    {
        if (seek != null)
            Destroy(seek);
    }

}
