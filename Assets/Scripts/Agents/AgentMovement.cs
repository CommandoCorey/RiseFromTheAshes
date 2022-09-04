using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Used to move unit with steering behaviours
public class AgentMovement : MonoBehaviour
{
    #region variable declaration
    public Transform unitBody;

    [Header("Physics & Steering Behaviours")]
    
    [SerializeField][Range(1, 60)] 
    float maxSpeed = 10.0f; // the maximum velocity the agent can reach
    //[SerializeField] float trueMaxSpeed; // used for group formations
    [SerializeField][Range(1, 10)]
    float acceleration = 3.0f; // increase in velocity each frame
    [SerializeField][Range(1, 10)]
    float deceleration = 3.0f; // decrease in vecloity each frame
    //[SerializeField] float maxAccel = 30.0f; // maximum increase in speed each frame

    //[SerializeField] float orientation; // angle on the y axis
    //[SerializeField] float rotation; // the amount of rotation to be applied each frame
    Vector3 velocity; // distance to travel per frame
    
    [Header("Turning")]
    [SerializeField][Range(1, 100)]
    float maxRotation = 45.0f; // maximum angularVelocity per frame
    //[SerializeField] float maxAnagulerAccel = 45.0f; // maximum angular acceleration per frame

    [Header("Slowing and Stopping")]
    [SerializeField][Range(0, 10)]
    float minDistanceFromTarget = 1.0f; // distance from target before stopping
    [SerializeField][Range(0, 10)]
    float maxDistanceFromTarget = 3.0f; // distance from target before stopping
    [SerializeField][Range(0, 10)]
    float minDistanceFromWaypoint = 3.0f; // minimum distance from a waypoint before changing to the next one
    [SerializeField][Range(0, 10)]
    float distanceFromNeighbour = 1.0f; // distance from stationary unit before stopping
    [SerializeField][Range(0, 10)]
    float minSpeedWhenStopping = 1.6f; // 

    private Steering steer;
    private NavMeshPath path;
    private UnitManager unitManager;
    #endregion

    #region properties
    public int SquadNum { get; set; }
    public float MaxSpeed { get => maxSpeed; }
    public float CurrentSpeed { get => velocity.magnitude; }
    public float Acceleration { get => acceleration; }
    public float Deceleration { get => deceleration; }
    public float MaxRotation { get => maxRotation; }
    //public float MaxAnagulerAccel { get => maxAnagulerAccel; }
    public Vector3 Velocity { get => velocity; }
    public float MinDistanceFromTarget { get => minDistanceFromTarget; }
    public float MaxDistanceFromTarget { get => maxDistanceFromTarget; }
    public float MinDistanceFromWaypoint {  get => minDistanceFromWaypoint; }
    public float MinDistanceFromNeighbour { get => distanceFromNeighbour; }
    public float MinSpeedWhenStopping { get => minSpeedWhenStopping; }
    public Vector3[] Path { get => path.corners; } // returns all waypoints in the path

    /// <summary>
    /// get a list of squad neighbours form the game manager
    /// </summary>
    public List<GameObject> Neighbours
    {
        get => unitManager.GetNeighbourUnits(gameObject, SquadNum);
    }
    #endregion

    #region Start And Update
    void Start()
    {
        velocity = Vector3.zero;
        steer = new Steering();
        //trueMaxSpeed = maxSpeed;

        path = new NavMeshPath();

        unitManager = GameObject.FindObjectOfType<UnitManager>();
    }

    // change the transform based off the last frame's steering
    protected virtual void Update()
    {       
        // moves the agent on the x and z axis only
        Vector3 displacement = velocity * Time.deltaTime;
        displacement.y = 0;

        /*
        orientation = rotation;// * Time.deltaTime; // update the orientation by the rotation speed

        //limit orientation between 0 and 360
        if(orientation < 0.0f)
        {
            orientation += 360.0f;
        }
        else if(orientation  > 360.0f)
        {
            orientation -= 360.0f;
        }*/

        // update the transform prperties
        transform.Translate(displacement, Space.World);
        //transform.rotation = new Quaternion();
        //transform.Rotate(Vector3.up, orientation);

        unitBody.LookAt(unitBody.position + displacement.normalized, Vector3.up);    
    }

    // update movement for the next frame
    protected virtual void LateUpdate()
    {
        velocity += steer.linearVelocity * Time.deltaTime;
        //rotation += steer.angularVelocity * Time.deltaTime;

        // cap the velocity to the max speed
        if(velocity.magnitude > maxSpeed)
        {
            velocity.Normalize();
        }

        steer = new Steering();
    }
    #endregion

    #region public functions
    /// <summary>
    /// Sets internal steering and applies a weight to it
    /// </summary>
    /// <param name="steer">Sets the steering behaviour to use</param>
    /// <param name="weight">Sets the weight of the steering behaviour when combining different ones</param>
    /// e.g. avoiding a wall is more important than avoiding another unit
    public void AddSteering(Steering steer, float weight)
    {
        this.steer.linearVelocity += (weight * steer.linearVelocity);
        this.steer.angularVelocity += (weight * steer.angularVelocity);
    }

    /// <summary>
    /// Resets the maximum speed to the true maximum when traveling in groups
    /// </summary>
    /*public void SpeedReset()
    {
        maxSpeed = trueMaxSpeed;
    }*/    

    /// <summary>
    /// Causes the unit velocity values to instantly be set to zero
    /// </summary>
    public void StopMoving()
    {
        velocity = Vector3.zero;
        GetComponent<Rigidbody>().velocity = velocity;
        GetComponent<Rigidbody>().angularVelocity = velocity;
        steer.linearVelocity = Vector3.zero;
        steer.angularVelocity = 0;

        var rigidBody = GetComponent<Rigidbody>();
        if (rigidBody != null)
            rigidBody.velocity = Vector3.zero;
    }

    /// <summary>
    /// Calculates required to stop moving from current velocity
    /// </summary>
    /// <returns>floating point value based on the magnitude of the distance vector</returns>
    public float GetDecelerateDistance()
    {
        return (velocity.magnitude * velocity.magnitude) / (2 * deceleration);
    }

    /// <summary>
    /// Sets path along navigation mesh from current position to the destination
    /// </summary>
    public Vector3[] CreatePath(Vector3 targetPos)
    {
        path.ClearCorners();
        NavMesh.CalculatePath(transform.position, targetPos, NavMesh.AllAreas, path);

        return path.corners;
    }
    #endregion
}