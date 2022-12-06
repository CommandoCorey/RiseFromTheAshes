using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.CanvasScaler;

public class IdleState : State
{    
    protected override void Awake()
    {
        base.Awake();

        //if(gameObject.tag == "PlayerUnit")
           // agent = GetComponent<AgentMovement>();
    }

    private void Start()
    {
        unit.body.GetComponent<NavMeshAgent>().isStopped = true;

        // turns off targeted sprite while in halt mode
        if(unit.AttackTarget != null && unit.UnitHalt)
        {
            var um = UnitManager.Instance;

            var prevTarget = unit.AttackTarget;
            unit.AttackTarget = null;

            if (!um.TargetInSelection(prevTarget))
            {
                prevTarget.GetComponent<SelectionSprites>().ShowTargetedSprite = false;
            }
        }

        AudioSource audio = GetComponentInParent<AudioSource>();

        if (audio != null && audio.isPlaying)
            audio.Stop();

        unit.statusText.text = "Idle";
    }

    protected override void Update()
    {
        if (!unit.autoAttack)
            return;

        var enemiesInRange = GetEnemiesInRange();
            //Physics.OverlapSphere(unit.body.position, unit.DetectionRadius, unit.DetectionLayer);

        // if there are any enemies in range change to the combat state
        if (enemiesInRange.Count > 0)
        {
            //Debug.Log("Detected Enemy: " + enemiesInRange[0].gameObject.name);

            if (unit.seeThroughWalls)
            {
                unit.AttackTarget = enemiesInRange[0].gameObject.transform;

                //if (gameObject.tag == "PlayerUnit")
                    //unit.ChangeState(UnitState.Attack);
                //else if (gameObject.tag == "NavMesh Agent")

                if(!unit.UnitHalt)
                    unit.ChangeState(UnitState.Follow);
            }
            else
            {
                Vector3 directionToTarget = (enemiesInRange[0].gameObject.transform.position - transform.position).normalized;

                if(!ObstacleInWay(directionToTarget))
                {
                    unit.AttackTarget = enemiesInRange[0].gameObject.transform;
                    
                    if(!unit.UnitHalt)
                        unit.ChangeState(UnitState.Follow);
                    else if(Vector3.Distance(transform.position, unit.AttackTarget.position) <= unit.AttackRange)
                        unit.ChangeState((UnitState)UnitState.Attack);
                }
            }
        }
        else if(unit.turret.rotation != unit.body.rotation)
        {
            unit.turret.rotation = Quaternion.RotateTowards(unit.turret.rotation, unit.body.rotation, 
                Time.deltaTime * unit.TurretRotationSpeed);
        }


    }

    private void ResetTurret()
    {
        unit.turret.rotation = Quaternion.RotateTowards(unit.turret.rotation, Quaternion.identity, Time.deltaTime * unit.TurretRotationSpeed);

    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if(unit != null)
            UnityEditor.Handles.Label(unit.body.position + Vector3.up * 1, "Idle");
#endif
    }

}
