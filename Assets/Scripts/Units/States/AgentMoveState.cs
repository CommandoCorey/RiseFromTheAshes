using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class AgentMoveState : MonoBehaviour
{
    //[SerializeField] float movementSpeed = 100;
    [SerializeField] float stoppingDistance = 0.5f;

    private NavMeshAgent agent;
    private Vector3 targetPos;

    private UnitController unit;

    private GameManager gameManager;

    // Start is called before the first frame update
    void Awake()
    {
        unit = GetComponent<UnitController>();
        agent = unit.body.GetComponent<NavMeshAgent>();
        targetPos = transform.position;

        gameManager = FindObjectOfType<GameManager>();

        agent.speed = unit.Speed;
    }

    // Update is called once per frame
    void Update()
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
            }
        }
    }    

    public void MoveTo(Vector3 position)
    {
        targetPos = position;
        agent.SetDestination(targetPos);
        agent.isStopped = false;

        // plays random move sound
        AudioSource audio = unit.body.GetComponent<AudioSource>();
        unit.PlayMoveSound();
    }

    private void OnDrawGizmos()
    {        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(targetPos, 1);
#if UNITY_EDITOR
        UnityEditor.Handles.Label(unit.body.position + Vector3.up * 1, "Moving");
#endif
    }

}
