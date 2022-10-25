using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        //agent.StopMoving();

        AudioSource audio = GetComponentInParent<AudioSource>();

        if (audio != null && audio.isPlaying)
            audio.Stop();
    }

    void Update()
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

                if (gameObject.tag == "PlayerUnit")
                    unit.ChangeState(UnitState.Attack);
                else if (gameObject.tag == "NavMesh Agent")
                    unit.ChangeState(UnitState.Follow);
            }
            else
            {
                Vector3 directionToTarget = (enemiesInRange[0].gameObject.transform.position - transform.position).normalized;

                if(!ObstacleInWay(directionToTarget))
                {
                    if (gameObject.tag == "PlayerUnit")
                        unit.ChangeState(UnitState.Attack);
                    else
                        unit.ChangeState(UnitState.Follow);
                }
            }
        }
        else if(unit.turret.localRotation != unit.transform.rotation )
        {
            unit.turret.rotation = Quaternion.RotateTowards(unit.turret.rotation, unit.transform.rotation, 
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
