using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormationState : State
{
    Agent agent;
    StateManager states;

    SeekBehaviour seek;

    private float distanceFromTarget;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<Agent>();
        states = GetComponent<StateManager>();

        seek = gameObject.AddComponent<SeekBehaviour>();
        seek.target = target;
        seek.weight = 100;
        seek.enabled = true;

        Steering steer = seek.GetSteering();
        agent.AddSteering(steer, seek.weight);
    }

    // Update is called once per frame
    void Update()
    {
        distanceFromTarget = Vector3.Distance(target, transform.position);

        if (distanceFromTarget <= agent.MinDistanceFromTarget)
            states.ChangeState(UnitState.Idle);
    }

    private void OnDestroy()
    {
        Destroy(seek);
    }
}