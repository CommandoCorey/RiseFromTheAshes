using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitState
{
    Idle,
    Moving,
    Flock,
    Attack
}

public class Behaviour : MonoBehaviour
{
    [SerializeField] int team;

    [Header("Enemy Detection")]
    [SerializeField] float detectionRadius = 1.0f;
    [SerializeField] LayerMask detectionLayer;

    public GameObject target;

    [HideInInspector]
    public SeekBehaviour seek;
    //[HideInInspector]
    //public FleeBehaviour flee;

    IdleState idleState;
    MoveState moveState;
    FlockState seekState;
    TankController attackState;

    // Flocking behaviours
    [HideInInspector]
    public BoidCohesion cohesion;
    [HideInInspector]
    public BoidSepearation sepearation;

    // intelligent movement script
    Agent agent;

    UnitState state;

    public UnitState State { get => state; }

    public float DetectionRadius { get => detectionRadius; }
    public LayerMask DetectionLayer { get => detectionLayer; }

    // Start is called before the first frame update
    void Start()
    {
        //agent = gameObject.AddComponent<Agent>(); // add agent component to game object
        //seek = gameObject.GetComponent<SeekBehaviour>();
        //
        idleState = GetComponent<IdleState>();
        attackState = GetComponent<TankController>();

        ChangeState(UnitState.Idle);

        /*
        if (seek == null)
        {
            //seek = gameObject.AddComponent<SeekBehaviour>();
            //seek.target = target;
            //seek.enabled = true;

            //flee = gameObject.AddComponent<FleeBehaviour>();
            //flee.target = target;
            //enabled = true;
        }*/

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(state == UnitState.Idle)
            {
                ChangeState(UnitState.Flock);
            }
            else
            {
                ChangeState(UnitState.Idle);
            }

        }
    }

    public void ChangeState(UnitState newState)
    {
        state = newState;

        switch(newState)
        {
            case UnitState.Idle:
                if(GetComponent<IdleState>() == null)
                {
                    idleState = gameObject.AddComponent<IdleState>();
                }

                DestroyImmediate(seekState);
                DestroyImmediate(moveState);
                //DestroyImmediate(attackState);
                attackState.enabled = false;
                idleState.enabled = true;
            break;

            case UnitState.Moving:
                if(GetComponent<MoveState>() == null)
                {
                    moveState = gameObject.AddComponent<MoveState>();
                }

                idleState.enabled = false;
                DestroyImmediate(idleState);
                DestroyImmediate(seekState);
                //DestroyImmediate(attackState);
                attackState.enabled = false;

                break;

            case UnitState.Flock:
                if(GetComponent<FlockState>() == null)
                {
                    seekState = gameObject.AddComponent<FlockState>();
                }

                DestroyImmediate(idleState);
                DestroyImmediate(moveState);
                //DestroyImmediate(attackState);
                attackState.enabled = false;

            break;

            case UnitState.Attack:
                if(GetComponent<TankController>() == null)
                {
                    attackState = gameObject.AddComponent<TankController>();
                }

                DestroyImmediate(idleState);
                DestroyImmediate(moveState);
                DestroyImmediate(seekState);

                attackState.enabled = true;
            break;
        }
    }

    private void OnDestroy()
    {
        Destroy(seek);
        //Destroy(flee);
    }

    private void OnDrawGizmos()
    {
        //UnityEditor.Handles.Label(transform.position + Vector3.up * 3, "Seek");
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

}
