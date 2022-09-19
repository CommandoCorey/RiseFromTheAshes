using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : State
{
    AgentMovement agent;
    
    protected override void Awake()
    {
        base.Awake();

        if(gameObject.tag == "PlayerUnit")
            agent = GetComponent<AgentMovement>();
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

        var enemiesInRange = Physics.OverlapSphere(transform.position, unit.DetectionRadius, unit.DetectionLayer);

        // if there are any enemies in range change to the combat state
        if (enemiesInRange.Length > 0)
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
                    else if (gameObject.tag == "NavMesh Agent")
                        unit.ChangeState(UnitState.Follow);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1, "Idle");
#endif
    }

}
