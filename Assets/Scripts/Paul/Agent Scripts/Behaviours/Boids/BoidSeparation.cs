using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidSeparation : FleeBehaviour
{
    [SerializeField] float desiredSepearation = 15.0f; // how much space we want between each unit
    public List<GameObject> targets;
    private int count = 0;

    protected override void Start()
    {
        desiredSepearation = GetComponent<BehaviourManager>().DesiredSeparation;
    }

    public override Steering GetSteering()
    {       
        Steering steer = new Steering();
        count = 0;

        // for each boid in the system, check if it is too close
        foreach(GameObject other in targets)
        {
            if(other != null)
            {
                // repulsion force
                float distance = (transform.position - other.transform.position).magnitude;

                // check that the distance is betwwen 0 and the deisired distance
                if((distance > 0) && (distance < desiredSepearation))
                {
                    // calculate vector pointint away from neighbout
                    Vector3 difference = transform.position - other.transform.position;
                    difference.Normalize();
                    difference /= distance;

                    steer.linearVelocity += difference;
                    count++;
                }
            }
        } // end foreach loop

        if(count > 0)
        {
            // requires alignment to work properly
            //steer.linearVelocity /= (float) count;
        }

        return steer;

    }

    private void OnDrawGizmos()
    {
        //UnityEditor.Handles.Label(transform.position + Vector3.up * 2, "Separation: " + count.ToString());
        //Gizmos.DrawWireSphere(transform.position, desiredSepearation);
    }

}
