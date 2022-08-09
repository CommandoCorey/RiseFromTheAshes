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

public class StateManager : MonoBehaviour
{
    [SerializeField] int team;

    [Header("Enemy Detection")]
    [SerializeField] float detectionRadius = 1.0f;
    [SerializeField] LayerMask detectionLayer;

    public GameObject target;

    //[HideInInspector]
    
    //[HideInInspector]
    //public FleeBehaviour flee;

    IdleState idleState;
    MoveState moveState;
    FlockState flockState;
    AttackState attackState;

    // Flocking behaviours
    [HideInInspector]
    public SeekBehaviour seek;

    [HideInInspector]
    public BoidCohesion cohesion;
    [HideInInspector]
    public BoidSepearation sepearation;
    [HideInInspector]
    public BoidAlignment alignment;

    // intelligent movement script
    //Agent agent;

    UnitState state;

    public UnitState State { get => state; }

    public float DetectionRadius { get => detectionRadius; }
    public LayerMask DetectionLayer { get => detectionLayer; }

    // Start is called before the first frame update
    void Start()
    {
        //agent = gameObject.AddComponent<Agent>(); // add agent component to game object
        //seek = gameObject.GetComponent<SeekBehaviour>();
        
        idleState = GetComponent<IdleState>();
        attackState = GetComponent<AttackState>();

        ChangeState(UnitState.Flock);

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

                DestroyImmediate(flockState);
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
                DestroyImmediate(flockState);
                //DestroyImmediate(attackState);
                attackState.enabled = false;

                break;

            case UnitState.Flock:
                if(GetComponent<FlockState>() == null)
                {
                    flockState = gameObject.AddComponent<FlockState>();
                }

                DestroyImmediate(idleState);
                DestroyImmediate(moveState);
                //DestroyImmediate(attackState);
                attackState.enabled = false;

            break;

            case UnitState.Attack:
                if(GetComponent<AttackState>() == null)
                {
                    attackState = gameObject.AddComponent<AttackState>();
                }

                DestroyImmediate(idleState);
                DestroyImmediate(moveState);
                DestroyImmediate(flockState);

                attackState.enabled = true;
            break;
        }
    }

    private void OnDestroy()
    {
        //Destroy(seek);
        //Destroy(flee);
    }

    private void OnDrawGizmos()
    {
        //UnityEditor.Handles.Label(transform.position + Vector3.up * 3, "Seek");
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

}
