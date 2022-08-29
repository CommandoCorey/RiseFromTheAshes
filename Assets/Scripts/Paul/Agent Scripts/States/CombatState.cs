using System.Collections;
using UnityEngine;

public class CombatState : MonoBehaviour
{
    UnitController unit;

    public LayerMask detectionLayer;

    private Transform target;
    private Quaternion lookRotation;    
    private Vector3 direction;
    private Vector3 offsetRotation;

    private bool attacking = false;

    void Awake()
    {
        unit = GetComponent<UnitController>();
        offsetRotation.x = unit.turret.localRotation.eulerAngles.x;
        offsetRotation.y = unit.turret.localRotation.eulerAngles.y;
        offsetRotation.z = unit.turret.localRotation.eulerAngles.z;
    }

    void Start()
    {
        target = unit.AttackTarget.transform;

        direction = (target.position - unit.turret.position).normalized;
        lookRotation = Quaternion.LookRotation(direction);
    }

    // Update is called once per frame
    void Update()
    {
        var unitsInRange = Physics.OverlapSphere(transform.position, unit.DetectionRadius, unit.DetectionLayer);

        //rotate us over time according to speed until we are in the required rotation
        unit.turret.rotation = Quaternion.Slerp(unit.turret.rotation, lookRotation, Time.deltaTime * unit.TurretRotationSpeed);

        if (unitsInRange.Length > 0)
        {
            //Debug.Log("Enemy target detected");
            target = unitsInRange[0].transform;
            RaycastHit hit;

            Debug.DrawLine(unit.firingPoint.position, unit.firingPoint.position + (unit.turret.forward * 5), Color.red);

            // check if turret is pointing at target
            if (Physics.Raycast(unit.firingPoint.position, unit.turret.forward, out hit) && hit.transform == target)            
            {
                //Debug.DrawLine(unit.turret.position, target.position, Color.yellow);
                if (!attacking)
                {
                    Invoke("DealDamage", unit.AttackRate);
                    attacking = true;
                }
            }
            else
            {
                // find the vector pointing from our position to the target
                direction = (target.position - unit.turret.position).normalized;                
            }
        }
        else
        {
            target = null;
            //direction = Quaternion.identity.eulerAngles;
            lookRotation = Quaternion.identity;

            if (unit.turret.rotation == Quaternion.identity)
                unit.ChangeState(UnitState.Idle);
        }

        //create the rotation we need to be in to look at the target
        if(lookRotation != Quaternion.identity)
            lookRotation = Quaternion.LookRotation(direction);

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
            attacking = false;
        }
    }

    private void OnDrawGizmos()
    {
        if(target != null)
            Gizmos.DrawLine(unit.firingPoint.position, target.position);
    }

}
