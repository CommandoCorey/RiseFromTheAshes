using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourManager : MonoBehaviour
{
    [Header("Steering Behaviour Weights")]
    [SerializeField] float seek = 1.0f;
    [SerializeField] float cohesion = 0.4f;
    [SerializeField] float alignment = 100.0f;
    [SerializeField] float separation = 10.0f;

    [Header("Boid Distances")]
    [SerializeField] float cohesionDistance = 15.0f;
    [SerializeField] float desiredSeparation = 6.0f;

    [Header("Obstacle Avoidance")]
    [SerializeField] float aheadDistance = 10;

    // properties for steering behaviour classes
    public SeekBehaviour Seek { get; set; }
    public SeekDecelerateBehaviour Decelerate { get; set; }
    public BoidCohesion Cohesion { get; set; }
    public BoidSeparation Separation { get; set; }
    public BoidAlignment Alignment { get; set; }

    // other properties
    public float SeekWeight { get => seek; }
    public float CohesionWeight { get => cohesion; }
    public float SeparationWeight { get => separation; }   
    public float AlignmentWeight { get => alignment; }  
    public float CohesionDistance {  get => cohesionDistance; }
    public float DesiredSeparation { get=> desiredSeparation; }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        if (GetComponent<SeekBehaviour>() != null && GetComponent<SeekBehaviour>().enabled)
            UnityEditor.Handles.Label(transform.position + Vector3.up * 5, "Seeking");
        else if (GetComponent<SeekDecelerateBehaviour>() != null && GetComponent<SeekDecelerateBehaviour>().enabled)
            UnityEditor.Handles.Label(transform.position + Vector3.up * 5, "Decelerating");
        
        if (GetComponent<BoidCohesion>() != null && GetComponent<BoidAlignment>() != null && 
            GetComponent<BoidSeparation>() != null)
        {
            Gizmos.color = Color.blue;
            UnityEditor.Handles.Label(transform.position + Vector3.up * 3, "Flocking");
        }

        /*
        if (GetComponent<BoidCohesion>() != null)
            UnityEditor.Handles.Label(transform.position + Vector3.up * 3, "Cohesion");

        if (GetComponent<BoidAlignment>() != null)
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2, "Cohesion");

        if (GetComponent<BoidSeparation>() != null)
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1, "Separation");
        */

    }

}
