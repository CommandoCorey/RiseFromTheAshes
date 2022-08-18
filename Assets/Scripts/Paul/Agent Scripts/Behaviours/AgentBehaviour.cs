using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentBehaviour : MonoBehaviour
{    
    public float weight { get; set; } = 1.0f;
    public Vector3 target { get; set; }

    protected float angularVelocity = 0;
    protected float angularAccel = 0;
    protected AgentMovement agent;

    protected virtual void Start()
    {
        agent = GetComponent<AgentMovement>();
    }

    public virtual void Update()
    {
        try
        {
            if(agent == null)
                agent = GetComponent<AgentMovement>();

            agent.AddSteering(GetSteering(), weight);
        }
        catch(NullReferenceException e)
        {
            Debug.LogError(e.Message);
        }
        
    }

    /// <summary>
    /// Clamps the rotation to a range
    /// </summary>
    /// <param name="rotation">The current rotation</param>
    /// <returns>Updated rotation amount</returns>
    public float MapToRange(float rotation)
    {
        rotation %= 360.0f;

        if(Mathf.Abs(rotation) > 100.0f)
        {
            if(rotation < 0.0f)            
                rotation += 360.0f;            
            else
                rotation -= 360.0f;
        }

        return rotation;
    }

    /// <summary>
    /// Creates a new steering behaviour and returns it. Can be overrident by subclasses.
    /// </summary>
    /// <returns>Object of the Steering struct</returns>
    public virtual Steering GetSteering()
    {
        return new Steering();
    }

}

public class Steering
{
    public float angularVelocity; // rotation 0->360
    public Vector3 linearVelocity; //instantaneous velocity

    public Steering(float angularVelocity = 0.0f, Vector3 linearVelocity = new Vector3())
    {
        this.angularVelocity = angularVelocity;
        this.linearVelocity = linearVelocity;
    }

}
