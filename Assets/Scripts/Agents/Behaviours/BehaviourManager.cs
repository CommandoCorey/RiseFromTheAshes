using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourManager : MonoBehaviour
{
    [Tooltip("The distance from the final waypoint before changing the weight values")]
    [SerializeField][Range(0, 10)]
    float distanceBeforeReduction = 6.0f;

    [Header("Initial Steering Behaviour Weights")]
    [SerializeField]
    float seek = 1.0f;
    [SerializeField]
    float cohesion = 0.4f;
    [SerializeField]
    float alignment = 100.0f;
    [SerializeField]
    float separation = 10.0f;
    [SerializeField]
    float obstacleAvoidance = 200.0f;

    [Header("Weight Changes when slowing")]
    [SerializeField]
    float cohesionReduction = 0.001f;
    [SerializeField]
    float alignmentReduction = 1f;
    [SerializeField]
    float separationReduction = 0.1f;
    [SerializeField]
    float obstacleAvoidanceReeduction = 1f;

    [Header("Initial Boid Distances")]
    [SerializeField]
    float cohesionDistance = 15.0f;
    [SerializeField]
    float desiredSeparation = 6.0f;

    [Header("Boid distance changes")]
    [SerializeField]
    float cohesionDistanceChange = 0;
    [SerializeField]
    float separationDistanceChange = 0;

    [Header("Obstacle Avoidance")]
    public Transform frontPoint;
    [SerializeField] float aheadDistance = 1;
    [SerializeField] Vector3 boxSize = Vector3.one;
    [SerializeField] Vector3 boxOffset = Vector3.forward;
    [SerializeField] LayerMask obstacleLayers = 10;

    [Header("Gizmos Enabled")]
    public bool flockPath = false;
    public bool neighbourDistance = false;
    public bool obstacleDetection = false;
    public bool behaviourText = false;

    // properties
    public float MaxDistanceFromTarget { get => distanceBeforeReduction;}
    public float SeekWeight { get => seek; }
    public float CohesionWeight { get => cohesion; }
    public float SeparationWeight { get => separation; }   
    public float AlignmentWeight { get => alignment; }  
    public float CohesionDistance {  get => cohesionDistance; }
    public float DesiredSeparation { get=> desiredSeparation; }
    public float CohesionReduction { get => cohesionReduction; }
    public float AlignmentReduction { get => alignmentReduction; }
    public float SeparationReduction { get => separationReduction; }
    public float CohesionDistanceChange { get => cohesionDistanceChange; }
    public float SeparationDistanceChange { get => separationDistanceChange; }
    public float ObstacleAvoidanceReduction { get => obstacleAvoidanceReeduction; }

    // obstacle avoidance
    public Vector3 FrontPoint { get => frontPoint.position; }
    public float AheadDistance { get => aheadDistance; }
    public LayerMask ObstacleLayers { get => obstacleLayers; }
    public float AvoidWeight { get => obstacleAvoidance; }
    public Vector3 BoxSize { get => boxSize; }
    public Vector3 BoxOffset { get => boxOffset; }


    // properties with setters
    public bool FrontObstacle { get; set; } = false;
    public bool UpLeftObstacle { get; set; } = false;
    public bool UpRightObstacle { get; set; } = false;

    private void OnDrawGizmos()
    {
        if (behaviourText)
        {
            Gizmos.color = Color.white;
            if (GetComponent<SeekBehaviour>() != null && GetComponent<SeekBehaviour>().enabled)
                UnityEditor.Handles.Label(transform.position + Vector3.up * 5, "Seeking");
            else if (GetComponent<SeekDecelerateBehaviour>() != null && GetComponent<SeekDecelerateBehaviour>().enabled)
                UnityEditor.Handles.Label(transform.position + Vector3.up * 5, "Decelerating");

            /*
            if (GetComponent<BoidCohesion>() != null && GetComponent<BoidAlignment>() != null && 
                GetComponent<BoidSeparation>() != null)
            {
                Gizmos.color = Color.blue;
                UnityEditor.Handles.Label(transform.position + Vector3.up * 3, "Flocking");
            }*/

            /*
            if (GetComponent<BoidCohesion>() != null)
                UnityEditor.Handles.Label(transform.position + Vector3.up * 3, "Cohesion");

            if (GetComponent<BoidAlignment>() != null)
                UnityEditor.Handles.Label(transform.position + Vector3.up * 2, "Cohesion");

            if (GetComponent<BoidSeparation>() != null)
                UnityEditor.Handles.Label(transform.position + Vector3.up * 1, "Separation");
            */

            // check state
            if (GetComponent<FormationState>() != null && GetComponent<FormationState>().enabled)
                UnityEditor.Handles.Label(transform.position + Vector3.up * 3, "Entering Formation");

        }
        // obstacle avoidance lines
        if (obstacleDetection && frontPoint != null)
        {
            /*
            Vector3 upLeft = (transform.forward - transform.right).normalized;
            Vector3 upRight = (transform.forward + transform.right).normalized;
            //float aheadLength = (frontPoint.position - (frontPoint.position + transform.forward * aheadDistance)).magnitude;            

            if (FrontObstacle)
                Gizmos.color = Color.red;
            else
                Gizmos.color = Color.green;
            Gizmos.DrawLine(frontPoint.position, frontPoint.position + transform.forward * aheadDistance);

            if (UpLeftObstacle)
                Gizmos.color = Color.red;
            else
                Gizmos.color = Color.green;
            Gizmos.DrawLine(frontPoint.position, frontPoint.position + upLeft * aheadDistance);

            if (UpRightObstacle)
                Gizmos.color = Color.red;
            else
                Gizmos.color = Color.green;
            Gizmos.DrawLine(frontPoint.position, frontPoint.position + upRight * aheadDistance);
            */

            Gizmos.DrawWireCube(transform.position + transform.TransformDirection(boxOffset), boxSize);            
        }
    }

}
