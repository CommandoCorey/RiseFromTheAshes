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

    // properties for steering behaviour classes
    public SeekBehaviour Seek { get; set; }
    public SeekDecelerateBehaviour Decelerate { get; set; }
    public BoidCohesion Cohesion { get; set; }
    public BoidSepearation Sepearation { get; set; }
    public BoidAlignment Alignment { get; set; }

    // other properties
    public float SeekWeight { get => seek; }
    public float CohesionWeight { get => cohesion; }
    public float SeparationWeight { get => separation; }   
    public float AlignmentWeight { get => alignment; }  

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
