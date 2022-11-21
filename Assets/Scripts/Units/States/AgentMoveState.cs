using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class AgentMoveState : State
{
    //[SerializeField] float movementSpeed = 100;
    [SerializeField] float stoppingDistance = 0.5f;

    private NavMeshAgent agent;
    private Vector3 targetPos;

    //private UnitController unit;
    //private UnitManager um;

    private GameManager gameManager;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();

        //unit = GetComponent<UnitController>();
        agent = unit.body.GetComponent<NavMeshAgent>();
        targetPos = transform.position;

        gameManager = FindObjectOfType<GameManager>();
        //um = FindObjectOfType<UnitManager>();

        agent.speed = unit.Speed;

        unit.statusText.text = "Moving to target";
    }

    // Update is called once per frame
    protected override void Update()
    {
        agent.enabled = (gameManager.State == GameState.Running);

        if (!agent.enabled)
            return;

        if (unit.State == UnitState.Moving)
        {
            agent.destination = targetPos;

            if (agent.isStopped || Vector3.Distance(unit.body.position, agent.destination) < stoppingDistance)
            {
                agent.isStopped = true;
                unit.ChangeState(UnitState.Idle);

                if(!unit.ReachedRallyPoint)
                    unit.ReachedRallyPoint = true;
            }
        }

        if(unit.body.gameObject.layer == 7 && unit.MovingToBase)
        {
            CheckForeEnemies();
        }
    }

    private void CheckForeEnemies()
    {
        var enemiesInRange = GetEnemiesInRange();

        if (enemiesInRange.Count > 0)
        {
            unit.AttackTarget = GetClosestEnemy(enemiesInRange.ToArray());
            HandleEnemyInRange();
        }
    }

    public void MoveTo(Vector3 position)
    {
        targetPos = position;
        agent.SetDestination(targetPos);
        agent.isStopped = false;

        bool isAi = gameObject.layer == 7;

        if (unit.ReachedRallyPoint)
        {
            FormationManager.Instance.RemovePositionFromRally(unit.RallyId, isAi);
            unit.ReachedRallyPoint = false;
        }

        // plays random move sound
        AudioSource audio = unit.body.GetComponent<AudioSource>();
        unit.PlayMoveSound();
    }

    private void OnDrawGizmos()
    {        
        //Gizmos.color = Color.green;
        //Gizmos.DrawWireSphere(targetPos, 1);
#if UNITY_EDITOR

        UnityEditor.Handles.Label(unit.body.position + Vector3.up * 1, "Moving");
#endif
    }

}