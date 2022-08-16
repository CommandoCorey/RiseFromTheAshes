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
        distanceFromTarget = Vector3.Distance(target, transform.position);

        if (distanceFromTarget <= agent.MinDistanceFromTarget)
            states.ChangeState(UnitState.Idle);

        if(distanceFromTarget <= agent.MaxDistanceFromTarget && StationaryUnitInRange())
            states.ChangeState(UnitState.Idle);

        //gameManager.StopGroupMoving();
    }

    private void OnDestroy()
    {
        agent.StopMoving();

        Destroy(seekAccel);
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
}
