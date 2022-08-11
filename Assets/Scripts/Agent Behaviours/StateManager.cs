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
   
    public GameObject AttackTarget {  get; set; }

    public Vector3 target;

    //[HideInInspector]
    
    //[HideInInspector]
    //public FleeBehaviour flee;

    IdleState idleState;
    //MoveState moveState;
    SeekState moveState;
    FlockState flockState;
    AttackState attackState;

    // Flocking behaviours
    [HideInInspector]
    public SeekBehaviour seek;
    [HideInInspector]
    public SeekDecelerateBehaviour decelerate;
    [HideInInspector]
    public BoidCohesion cohesion;
    [HideInInspector]
    public BoidSepearation sepearation;
    [HideInInspector]
    public BoidAlignment alignment;

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
        
        idleState = GetComponent<IdleState>();
        attackState = GetComponent<AttackState>();
        flockState = GetComponent<FlockState>();
        moveState = GetComponent<SeekState>();
        decelerate = GetComponent<SeekDecelerateBehaviour>();

        ChangeState(UnitState.Idle);
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

                if(flockState != null && flockState.enabled)
                {
                    flockState.EndState();
                    //flockState.enabled = false;
                    //Destroy(flockState);
                }

                if(moveState != null && moveState.enabled)
                    moveState.EndState();

                moveState.enabled = false;

                attackState.enabled = false;
                idleState.enabled = true;                

                //Destroy(flockState);
                //Destroy(moveState);
                //DestroyImmediate(attackState);                
            break;

            case UnitState.Moving:
                if(GetComponent<SeekState>() == null)
                {
                    moveState = gameObject.AddComponent<SeekState>();
                }

                idleState.enabled = false;
                //DestroyImmediate(idleState);
                //DestroyImmediate(flockState);
                //DestroyImmediate(attackState);
                attackState.enabled = false;
                moveState.enabled = true;

                moveState.Init();
            break;

            case UnitState.Flock:
                if(GetComponent<FlockState>() == null)
                {
                    flockState = gameObject.AddComponent<FlockState>();
                }

                //DestroyImmediate(idleState);
                //DestroyImmediate(moveState);
                //DestroyImmediate(attackState);

                idleState.enabled = false;
                attackState.enabled = false;
                flockState.enabled = true;

                flockState.Init();
            break;

            case UnitState.Attack:
                if(GetComponent<AttackState>() == null)
                {
                    attackState = gameObject.AddComponent<AttackState>();
                }

                //DestroyImmediate(idleState);
                //DestroyImmediate(moveState);
                //DestroyImmediate(flockState);

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
