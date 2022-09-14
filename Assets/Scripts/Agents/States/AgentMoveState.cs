using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class AgentMoveState : MonoBehaviour
{
    //[SerializeField] float movementSpeed = 100;

    private NavMeshAgent agent;
    private Vector3 targetPos;

    private UnitController unit;
    private UnitState state;    

    // Start is called before the first frame update
    void Awake()
    {
        unit = GetComponent<UnitController>();
        agent = GetComponent<NavMeshAgent>();
        targetPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {

        if (unit.State == UnitState.Moving)
        {
            agent.destination = targetPos;

            if (Vector3.Distance(transform.position, agent.destination) < 0.2f)
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
        AudioSource audio = GetComponentInParent<AudioSource>();
        int randomPick = Random.Range(0, unit.moveSounds.Length - 1);
        audio.PlayOneShot(unit.moveSounds[randomPick], 0.5f);
    }

    private void OnDrawGizmos()
    {        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(targetPos, 1);        

        UnityEditor.Handles.Label(transform.position + Vector3.up * 1, "Moving");
    }

}
