using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockState : State
{
    GameManager gameManager;

    StateManager states;
    Agent agent;
    Steering steer;
    BehaviourManager behaviours;
    
    [SerializeField]
    float distanceFromTarget;

    // Behaviours
    SeekBehaviour seekAccel;
    SeekDecelerateBehaviour seekDecel;
    BoidCohesion cohesion;
    BoidAlignment alignment;
    BoidSepearation sepearation;

    private float decelerationDistance;

    Transform leader;

    // Start is called before the first frame update
    void Start()
    {
        states = GetComponent<StateManager>();
        agent = GetComponent<Agent>();
        behaviours = GetComponent<BehaviourManager>();

        gameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();

        // add seek acceleration
        seekAccel = gameObject.AddComponent<SeekBehaviour>();
        seekAccel.target = target;
        seekAccel.weight = behaviours.SeekWeight;
        seekAccel.enabled = true;

        steer = seekAccel.GetSteering();
        agent.AddSteering(steer, seekAccel.weight);

        // add seek deceleration
        seekDecel = gameObject.AddComponent<SeekDecelerateBehaviour>();
        seekDecel.target = target;
        seekDecel.weight = behaviours.SeekWeight;
        seekDecel.enabled = false;

        // Add the boid cohesion behaviour
        cohesion = gameObject.AddComponent<BoidCohesion>();
        cohesion.targets = //gameManager.GetNeighbourUnits(this.gameObject);
            gameManager.GetUnitsInSquad(0);
        cohesion.weight = behaviours.CohesionWeight;
        cohesion.enabled = true;

        steer = cohesion.GetSteering();
        agent.AddSteering(steer, cohesion.weight);

        // add alignment behaviour
        alignment = gameObject.AddComponent<BoidAlignment>();
        alignment.targets = //gameManager.GetNeighbourUnits(this.gameObject);
            gameManager.GetUnitsInSquad(0);
        alignment.weight = behaviours.AlignmentWeight;
        alignment.enabled = true;

        steer = alignment.GetSteering();
        agent.AddSteering(steer, cohesion.weight);

        // Add the boid seperation behaviour
        sepearation = gameObject.AddComponent<BoidSepearation>();
        sepearation.targets = //gameManager.GetNeighbourUnits(this.gameObject);
            gameManager.GetUnitsInSquad(0);
        sepearation.weight = behaviours.SeparationWeight;
        sepearation.enabled = true;

        steer = sepearation.GetSteering();
        agent.AddSteering(steer, sepearation.weight);       

    }

    public void Init()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //decelerationDistance = GetDecelerateDistance();

        distanceFromTarget = Vector3.Distance(target, transform.position);

        // if the distance to start deccelerating has bene reached then change behaviours
        if (distanceFromTarget <= agent.MinDistanceFromTarget)
        {
            //seekDecel.enabled = true;
            //seekAccel.enabled = false;
            //cohesion.enabled = false;
            //alignment.enabled = false;
            //sepearation.enabled = false;

            states.ChangeState(UnitState.Idle);
        }

        // if the distance from a stationary target has been reached and the unit
        // is close enough to the target then start decelearting
        /*if (distanceFromTarget <= agent.MaxDistanceFromTarget && StationaryUnitInRange())
        {            
            seekDecel.enabled = true;
            seekAccel.enabled = false;
        }

        // check if the agent has reached minimum speed
        if (seekDecel.enabled && agent.Vecloity.magnitude <= agent.MinSpeedWhenStopping)
        {
            //agent.StopMoving();

            seekDecel.enabled = false;
            states.ChangeState(UnitState.Idle);
        }*/

        //gameManager.StopGroupMoving();
    }

    private void OnDestroy()
    {
        agent.StopMoving();

        Destroy(seekAccel);
        Destroy(seekDecel);
        Destroy(cohesion);
        Destroy(alignment);
        Destroy(sepearation);
    }

    private void OnDrawGizmos()
    {
        //UnityEditor.Handles.Label(transform.position + Vector3.up * 3, "Seek");
    }

    private bool StationaryUnitInRange()
    {
        var neighbours = gameManager.GetNeighbourUnits(this.gameObject);

        foreach (GameObject neighbour in neighbours)
        {
            // check distance from neighbout
            if (Vector3.Distance(neighbour.transform.position, transform.position) <= agent.MinDistanceFromNeighbour)
                return true;
        }

        return false;
    }

    private float GetDecelerateDistance()
    {
        return (agent.CurrentSpeed * agent.CurrentSpeed) / (2 * agent.Deceleration);
    }
}
