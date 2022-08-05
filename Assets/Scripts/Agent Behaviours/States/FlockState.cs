using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockState : MonoBehaviour
{
    Behaviour behaviours;
    GameObject target;

    // Start is called before the first frame update
    void Start()
    {
        behaviours = GetComponent<Behaviour>();
        target = behaviours.target;

        if(behaviours.seek == null)
        {
            behaviours.seek = gameObject.GetComponent<SeekBehaviour>();
            behaviours.seek.target = target;
            behaviours.seek.weight = 0.7f;
            behaviours.seek.enabled = true;

            //behaviours.flee = gameObject.AddComponent<FleeBehaviour>();
            //behaviours.flee.target = target;
            //behaviours.enabled = true;

            // Add the boid cohesion behaviour
            behaviours.cohesion = gameObject.GetComponent<BoidCohesion>();
            behaviours.cohesion.targets = behaviours.target.GetComponent<SquadParent>().children;
            behaviours.cohesion.weight = 0.4f;
            behaviours.cohesion.enabled = true;

            // Add the boid seperation behaviour
            behaviours.sepearation = gameObject.GetComponent<BoidSepearation>();
            behaviours.sepearation.targets = behaviours.target.GetComponent<SquadParent>().children;
            behaviours.sepearation.weight = 10.0f;
            behaviours.sepearation.enabled = true;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        Destroy(behaviours.seek);
    }

    private void OnDrawGizmos()
    {
        //UnityEditor.Handles.Label(transform.position + Vector3.up * 3, "Seek");
    }
}
