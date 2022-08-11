using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeekState : MonoBehaviour
{
    [SerializeField]
    float minSpeedWhenStopping = 1.6f;

    GameManager gameManager;

    StateManager behaviours;
    Vector3 target;
    Agent agent;
    Steering steer;

    private float distanceFromTarget;
    private float decelerateDistance;

    

    // Start is called before the first frame update
    void Start()
    {
        

    }

    public void Init()
    {
        behaviours = GetComponent<StateManager>();
        agent = GetComponent<Agent>();

        gameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();

        target = behaviours.target;

        behaviours.seek = gameObject.GetComponent<SeekBehaviour>();
        behaviours.seek.target = target;
        behaviours.seek.enabled = true;

        behaviours.decelerate = gameObject.GetComponent<SeekDecelerateBehaviour>();
        behaviours.decelerate.target = target;
        behaviours.decelerate.enabled = false;

        steer = behaviours.seek.GetSteering();
        agent.AddSteering(steer, behaviours.seek.weight);       
    }

    // Update is called once per frame
    void Update()
    {
        distanceFromTarget = Vector3.Distance(target, transform.position);

        //if (distanceFromTarget <= agent.MinDistanceFromTarget)
        //behaviours.ChangeState(UnitState.Idle);

        decelerateDistance = GetDecelerateDistance();      

        // begin deccelerating
        if (behaviours.seek.enabled && distanceFromTarget <= decelerateDistance)
        {
            behaviours.decelerate.enabled = true;
            behaviours.seek.enabled = false;
        }

        // check if the agent has reached minimum speed
        if(behaviours.decelerate.enabled && agent.Vecloity.magnitude <= minSpeedWhenStopping)
        {
            agent.StopMoving();

            behaviours.decelerate.enabled = false;
            behaviours.ChangeState(UnitState.Idle);
        }

        //Debug.Log("Veclocity = " + agent.Vecloity.magnitude);            
    }

    private void OnDestroy()
    {        
        //Destroy(behaviours.seek);
        //Destroy(behaviours.cohesion);
        //Destroy(behaviours.alignment);
        //Destroy(behaviours.sepearation);
    }

    public void EndState()
    {
        behaviours.seek.enabled = false;
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
