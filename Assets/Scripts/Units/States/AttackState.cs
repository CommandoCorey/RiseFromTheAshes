using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : State
{    
    bool pointingAtTarget = false;

    private Quaternion lookRotation;
    private Vector3 direction;
    private Vector3 initialRotation;

    // Start is called before the first frame update
    void Start()
    {
        unit.PlayAimSound();
    }

    // Update is called once per frame
    void Update()
    {
        if(!pointingAtTarget)
            Aim();
    }

    private void Aim()
    {
        if (unit.AttackTarget == null)
        {
            unit.ChangeState(UnitState.Idle);
            return;
        }

        if (ObstacleInWay(directionToTarget))
        {
            //Debug.Log("Obstacle is in way");

            if (unit.UnitHalt)
            {
                unit.ChangeState(UnitState.Idle);                
            }
            else
            {
                unit.ChangeState(UnitState.Follow);
            }

            return;
        }

        // rotate us over time according to speed until we are in the required rotation  
        //unit.turret.rotation = Quaternion.Slerp(unit.turret.rotation, lookRotation, Time.deltaTime * unit.TurretRotationSpeed);
        unit.turret.rotation = Quaternion.RotateTowards(unit.turret.rotation, lookRotation, Time.deltaTime * unit.TurretRotationSpeed);
        // revert x and z rotation back to there original value
        unit.turret.localRotation = Quaternion.Euler(initialRotation.x, unit.turret.localRotation.eulerAngles.y, initialRotation.z);

        //Debug.Log("Enemy target detected");

        // check if turret is pointing at target
        if (Quaternion.Angle(unit.turret.rotation, lookRotation) < unit.MinAngle)        
        {
            unit.turret.rotation = lookRotation;

            Debug.DrawLine(unit.turret.position, unit.AttackTarget.position, Color.yellow);

            Invoke("DealDamage", unit.AttackRate);
            pointingAtTarget = true;

        }
        else
        {
            // find the vector pointing from our position to the target
            direction = (unit.AttackTarget.position - unit.body.position).normalized;
        }

        //create the rotation we need to be in to look at the target
        if (lookRotation != Quaternion.identity)
            lookRotation = Quaternion.LookRotation(direction);
    }

    // deals damage to enemy once every x amount of seconds
    private void DealDamage()
    {
        if (unit.AttackTarget != null)
        {
            Vector3 hitPosition = new Vector3();

            ParticleSystem fireParticles = unit.fireEffects[Random.Range(0, unit.fireEffects.Length)];

            //unit.PlayParticles(unit.fireEffect);
            unit.InstantiateParticles(fireParticles, unit.firingPoint.position);

            unit.PlayFireSound();

            // generate damage particles at hit position
            if(Physics.Raycast(unit.firingPoint.position, unit.turret.forward, out RaycastHit hit))
            {
                hitPosition = hit.point;
            }

            // check if target is still in attack range
            if (Vector3.Distance(unit.body.position, unit.AttackTarget.position) > unit.AttackRange + 0.01f)
            {
                unit.ChangeState(UnitState.Follow);
            }            

            // check if the target is a unit
            else if (unit.AttackTarget.gameObject.layer == 6 || unit.AttackTarget.gameObject.layer == 7)
            {
                unit.AttackTarget.GetComponentInParent<UnitController>().TakeDamage(unit.DamagePerHit, hitPosition);
                Invoke("DealDamage", unit.AttackRate);
            }
            // check if the target is a building
            else if (unit.AttackTarget.gameObject.layer == 8 || unit.AttackTarget.gameObject.layer == 9)
            {
                //Debug.Log("Dealing " + unit.DamagePerHit + " damage to " + unit.AttackTarget.name);
                unit.AttackTarget.GetComponent<Building>().TakeDamage(hitPosition, unit.DamagePerHit);
                Invoke("DealDamage", unit.AttackRate);
            }

        }
        else if (unit.MovingToBase && unit.body.gameObject.layer == 7)
        {
            var ai = FindObjectOfType<AiPlayer>();           
            unit.ChangeState(UnitState.Moving, ai.playerBase.position);
        }
        else
        {            
            unit.ChangeState(UnitState.Idle);
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(unit.firingPoint.position, unit.firingPoint.position + (unit.turret.forward * unit.AttackRange));

#if UNITY_EDITOR
        if(!pointingAtTarget)
            UnityEditor.Handles.Label(unit.body.position + Vector3.up * 5, "Aiming");
        else
            UnityEditor.Handles.Label(unit.body.position + Vector3.up * 5, "Firing");
#endif
    }

}
