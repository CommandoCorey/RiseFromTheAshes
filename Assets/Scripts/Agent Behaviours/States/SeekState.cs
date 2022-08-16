using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeekState : State
{
    [SerializeField]
    float minSpeedWhenStopping = 1.6f;

    GameManager gameManager;

    StateManager states;
    Agent agent;
    Steering steer;

    BehaviourManager behaviours;

    // Behaviour classes
    SeekBehaviour seek;
    SeekDecelerateBehaviour decelerate;

    private float distanceFromTarget;
    private float decelerateDistance;

    // Start is called before the first frame update
    void Start()
    {
        states = GetComponent<StateManager>();
        agent = GetComponent<Agent>();
        behaviours = GetComponent<BehaviourManager>();

        gameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();

        seek = gameObject.AddComponent<SeekBehaviour>();
        seek.target = target;
        seek.weight = behaviours.SeekWeight;
        seek.enabled = true;

        decelerate = gameObject.AddComponent<SeekDecelerateBehaviour>();
        decelerate.target = target;
        decelerate.weight = behaviours.SeekWeight;
        decelerate.enabled = false;

        steer = seek.GetSteering();
        agent.AddSteering(steer, seek.weight);

    }

    public void Init()
    {
              
    }

    // Update is called once per frame
    void Update()
    {
        distanceFromTarget = Vector3.Distance(target, transform.position);

        //if (distanceFromTarget <= agent.MinDistanceFromTarget)
        //ChangeState(UnitState.Idle);

        decelerateDistance = GetDecelerateDistance();      

        // begin deccelerating
        if (seek.enabled && distanceFromTarget <= decelerateDistance)
        {
            decelerate.enabled = true;
            seek.enabled = false;
        }

        // check if the agent has reached minimum speed
        if(decelerate.enabled && agent.Vecloity.magnitude <= minSpeedWhenStopping)
        {
            agent.StopMoving();

            decelerate.enabled = false;
            states.ChangeState(UnitState.Idle);
        }

        //Debug.Log("Veclocity = " + agent.Vecloity.magnitude);            
    }

    private void OnDestroy()
    {        
        Destroy(seek);
    }

    public void EndState()
    {
        seek.enabled = false;
    }

    private float GeAccelerateDistance()
    {
        float framesUntilStopped = agent.MaxSpeed / agent.Acceleration;
        return agent.MaxSpeed * framesUntilStopped +(0.5f * agent.Acceleration / Mathf.Pow(framesUntilStopped, 2));
    }

    private float GetDecelerateDistance()
    {
        //float framesUntilStopped = agent.CurrentSpeed / agent.Deceleration;
        //float v = agent.Deceleration * framesUntilStopped;

        return (agent.CurrentSpeed * agent.CurrentSpeed) / (2 * agent.Deceleration);
    }

}
