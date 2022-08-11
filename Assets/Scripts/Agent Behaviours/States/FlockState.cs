using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockState : MonoBehaviour
{
    GameManager gameManager;

    StateManager behaviours;
    Vector3 target;
    Agent agent;
    Steering steer;
    
    [SerializeField]
    float distanceFromTarget;

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

        //if(behaviours.seek == null)
        //{
            behaviours.seek = gameObject.GetComponent<SeekBehaviour>();
            behaviours.seek.target = target;
            //behaviours.seek.weight = 0.7f;
            behaviours.seek.enabled = true;

            steer = behaviours.seek.GetSteering();
            agent.AddSteering(steer, behaviours.seek.weight);

            //behaviours.flee = gameObject.AddComponent<FleeBehaviour>();
            //behaviours.flee.target = target;
            //behaviours.enabled = true;

            // Add the boid cohesion behaviour
            behaviours.cohesion = gameObject.GetComponent<BoidCohesion>();
            behaviours.cohesion.targets = gameManager.GetNeighbourUnits(this.gameObject);                
            behaviours.cohesion.enabled = true;

            steer = behaviours.cohesion.GetSteering();
            agent.AddSteering(steer, behaviours.cohesion.weight);

            // add alignment behaviour
            behaviours.alignment = gameObject.GetComponent<BoidAlignment>();
            behaviours.alignment.targets = gameManager.GetNeighbourUnits(this.gameObject);               
            behaviours.alignment.enabled = true;

            steer = behaviours.alignment.GetSteering();
            agent.AddSteering(steer, behaviours.cohesion.weight);

            // Add the boid seperation behaviour
            behaviours.sepearation = gameObject.GetComponent<BoidSepearation>();
            behaviours.sepearation.targets = gameManager.GetNeighbourUnits(this.gameObject);                
            behaviours.sepearation.enabled = true;

            steer = behaviours.sepearation.GetSteering();
            agent.AddSteering(steer, behaviours.sepearation.weight);
         //}
    }

    // Update is called once per frame
    void Update()
    {
        distanceFromTarget = Vector3.Distance(target, transform.position);

        if (distanceFromTarget <= agent.MinDistanceFromTarget)
            behaviours.ChangeState(UnitState.Idle);

        if(distanceFromTarget <= agent.MaxDistanceFromTarget && StationaryUnitInRange())
            behaviours.ChangeState(UnitState.Idle);

        //gameManager.StopGroupMoving();
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
        behaviours.cohesion.enabled = false;
        behaviours.alignment.enabled = false;
        behaviours.sepearation.enabled = false;
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
