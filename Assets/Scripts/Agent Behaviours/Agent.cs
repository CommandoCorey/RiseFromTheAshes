using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Used to move unit with steering behaviours
public class Agent : MonoBehaviour
{
    public GameObject highlight;

    [Header("Unit Stats")]
    [SerializeField] float maxHealth = 100;
    [SerializeField]
    private float health;

    [Header("Physics & Steering Behaviours")]
    [SerializeField] float maxSpeed = 10.0f; // the maximum velocity the agent can reach
    [SerializeField] float trueMaxSpeed; // used for group formations
    [SerializeField] float acceleration = 3.0f; // increase in velocity each frame
    [SerializeField] float deceleration = 3.0f; // decrease in vecloity each frame
    //[SerializeField] float maxAccel = 30.0f; // maximum increase in speed each frame

    [SerializeField] float orientation; // angle on the y axis
    [SerializeField] float rotation; // the amount of rotation to be applied each frame
    [SerializeField] Vector3 velocity; // distance to travel per frame

    [Header("Turning")]
    [SerializeField] float maxRotation = 45.0f; // maximum angularVelocity per frame
    [SerializeField] float maxAnagulerAccel = 45.0f; // maximum angular acceleration per frame

    [Header("Stopping")]
    [SerializeField] float minDistanceFromTarget = 1.0f; // distance from target before stopping
    [SerializeField] float maxDistanceFromTarget = 3.0f; // distance from target before stopping
    [SerializeField] float distanceFromNeighbour = 1.0f; // distance from stationary unit before stopping
    [SerializeField] float minSpeedWhenStopping = 1.6f;

    protected Steering steer;

    private StateManager states;

    // Properties
    public float MaxSpeed { get => maxSpeed; }
    public float CurrentSpeed { get => velocity.magnitude; }
    public float Acceleration { get => acceleration; }
    public float Deceleration { get => deceleration; }

    //public float MaxAccel { get => maxAccel; }

    public float MaxRotation { get => maxRotation; }
    public float MaxAnagulerAccel { get => maxAnagulerAccel; }
    public Vector3 Vecloity { get => velocity; }
    public float MinDistanceFromTarget { get => minDistanceFromTarget; }
    public float MaxDistanceFromTarget { get => maxDistanceFromTarget; }
    public float MinDistanceFromNeighbour { get => distanceFromNeighbour; }
    public float MinSpeedWhenStopping { get => minSpeedWhenStopping; }

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

    void Start()
    {
        health = maxHealth;

        velocity = Vector3.zero;
        steer = new Steering();
        trueMaxSpeed = maxSpeed;
    }

    // change the transform based off the last frame's steering
    public virtual void Update()
    {       
        // moves the agent on the x and z axis only
        Vector3 displacement = velocity * Time.deltaTime;
        displacement.y = 0;

        orientation = rotation;// * Time.deltaTime; // update the orientation by the rotation speed

        //limit orientation between 0 and 360
        if(orientation < 0.0f)
        {
            orientation += 360.0f;
        }
        else if(orientation  > 360.0f)
        {
            orientation -= 360.0f;
        }

        // update the transform prperties
        transform.Translate(displacement, Space.World);
        //transform.rotation = new Quaternion();
        //transform.Rotate(Vector3.up, orientation);

        transform.LookAt(transform.position + displacement.normalized, Vector3.up);

        if (health <= 0)
        {
            GameObject.Destroy(this.gameObject);
        }
    }    

    // update movement for the next frame
    public virtual void LateUpdate()
    {
        velocity += steer.linearVelocity * Time.deltaTime;

        rotation += steer.angularVelocity * Time.deltaTime;

        // cap the velocity to the max speed
        if(velocity.magnitude > maxSpeed)
        {
            velocity.Normalize();
        }

        // if the steering behaviour is not moving set the velocity to zero
        //if(steer.linearVelocity.magnitude == 0.0f)
        //{
           //velocity = Vector3.zero;
        //}

        steer = new Steering();
    }

    /// <summary>
    /// Resets the maximum speed to the true maximum when traveling in groups
    /// </summary>
    public void SpeedReset()
    {
        maxSpeed = trueMaxSpeed;
    }

    public void SetSelected(bool selected)
    {
        highlight.SetActive(selected);
    }

    public void SubtractHealth(float amount)
    {
        health -= amount;
    }

    public void StopMoving()
    {
        velocity = Vector3.zero;
        steer.linearVelocity = Vector3.zero;
        steer.angularVelocity = 0;
    }

    public float GetDecelerateDistance()
    {
        return (velocity.magnitude * velocity.magnitude) / (2 * deceleration);
    }

}