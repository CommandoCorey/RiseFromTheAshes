using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockState : MonoBehaviour
{
    GameManager gameManager;

    StateManager behaviours;
    GameObject target;
    Agent agent;
    Steering steer;

    // Start is called before the first frame update
    void Start()
    {
        behaviours = GetComponent<StateManager>();
        agent = GetComponent<Agent>();

        gameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();

        target = behaviours.target;

        if(behaviours.seek == null)
        {
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
                //behaviours.target.GetComponent<SquadParent>().children;
            //behaviours.cohesion.weight = 0.4f;
            //behaviours.cohesion.enabled = true;

            steer = behaviours.cohesion.GetSteering();
            agent.AddSteering(steer, behaviours.cohesion.weight);

            // add alignment behaviour
            behaviours.alignment = gameObject.GetComponent<BoidAlignment>();
            behaviours.alignment.targets = gameManager.GetNeighbourUnits(this.gameObject);
                //behaviours.target.GetComponent<SquadParent>().children;
            //behaviours.alignment.enabled = true;

            steer = behaviours.alignment.GetSteering();
            agent.AddSteering(steer, behaviours.cohesion.weight);


            // Add the boid seperation behaviour
            behaviours.sepearation = gameObject.GetComponent<BoidSepearation>();
            behaviours.sepearation.targets = gameManager.GetNeighbourUnits(this.gameObject);
                //behaviours.target.GetComponent<SquadParent>().children;
            //behaviours.sepearation.weight = 10.0f;
            //behaviours.sepearation.enabled = true;

            steer = behaviours.sepearation.GetSteering();
            agent.AddSteering(steer, behaviours.sepearation.weight);
        }

    }

    // Update is called once per frame
    void Update()
    {
        

    }

    private void OnDestroy()
    {
        //Destroy(behaviours.seek);
    }

    private void OnDrawGizmos()
    {
        //UnityEditor.Handles.Label(transform.position + Vector3.up * 3, "Seek");
    }
}
