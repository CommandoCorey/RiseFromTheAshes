using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitState
{
    Idle,
    Moving,
    Flock,
    Formation,
    Attack
}

public class StateManager : MonoBehaviour
{
    [SerializeField] int team;

    [Header("Enemy Detection")]
    [SerializeField] float detectionRadius = 1.0f;
    [SerializeField] LayerMask detectionLayer;
   
    public GameObject AttackTarget {  get; set; }

    IdleState idleState;
    SeekState moveState;
    FlockState flockState;
    AttackState attackState;
    FormationState formationState;

    // intelligent movement script
    Agent agent;
    UnitState state;

    Vector3 formationTarget;

    private Color drawColor = Color.white;

    public UnitState State { get => state; }

    public float DetectionRadius { get => detectionRadius; }
    public LayerMask DetectionLayer { get => detectionLayer; }

    public void SetFormationTarget(Vector3 position)
    {
        formationTarget = position;
    }

    // Start is called before the first frame update
    void Start()
    {
        //agent = gameObject.AddComponent<Agent>(); // add agent component to game object
        //seek = gameObject.GetComponent<SeekBehaviour>();
        
        idleState = GetComponent<IdleState>();
        attackState = GetComponent<AttackState>();
        flockState = GetComponent<FlockState>();
        moveState = GetComponent<SeekState>();
        //decelerate = GetComponent<SeekDecelerateBehaviour>();

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

    public void ChangeState(UnitState newState, Vector3 target = new Vector3())
    {
        
        state = newState;

        switch(newState)
        {
            case UnitState.Idle:
                if(GetComponent<IdleState>() == null)
                {
                    idleState = gameObject.AddComponent<IdleState>();
                }

                /*
                if(flockState != null && flockState.enabled)
                {
                    flockState.EndState();
                    //flockState.enabled = false;
                    //Destroy(flockState);
                }*/

                //if(moveState != null && moveState.enabled)
                   // moveState.EndState();

                //moveState.enabled = false;

                //attackState.enabled = false;
                //idleState.enabled = true;                

                Destroy(flockState);
                Destroy(moveState);
                Destroy(attackState);
                Destroy(formationState);

                drawColor = Color.white;
            break;

            case UnitState.Moving:
                if(GetComponent<SeekState>() == null)
                {
                    moveState = gameObject.AddComponent<SeekState>();
                }

                //idleState.enabled = false;
                Destroy(idleState);
                Destroy(flockState);
                Destroy(attackState);
                //attackState.enabled = false;
                //moveState.enabled = true;

                moveState.Target = target;
                //moveState.Init();

                drawColor = Color.red;
            break;

            case UnitState.Flock:
                if(GetComponent<FlockState>() == null)
                {
                    flockState = gameObject.AddComponent<FlockState>();
                }

                Destroy(idleState);
                Destroy(moveState);
                Destroy(attackState);

                //idleState.enabled = false;
                //attackState.enabled = false;
                //flockState.enabled = true;

                flockState.Target = target;
                flockState.FormationTarget = formationTarget;

                //flockState.Init();
                drawColor = Color.blue;
            break;

            case UnitState.Formation:
                if (GetComponent<FormationState>() == null)
                {
                    formationState = gameObject.AddComponent<FormationState>();
                }

                Destroy(idleState);
                Destroy(moveState);
                Destroy(attackState);
                Destroy(flockState);

                drawColor = Color.yellow;
            break;

            case UnitState.Attack:
                if(GetComponent<AttackState>() == null)
                {
                    attackState = gameObject.AddComponent<AttackState>();
                }

                Destroy(idleState);
                Destroy(moveState);
                Destroy(flockState);

                //attackState.enabled = true;
            break;
        }
    }

    private void OnDrawGizmos()
    {        
        if (Application.isPlaying)
            Gizmos.color = drawColor;       

        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (state == UnitState.Idle)
            UnityEditor.Handles.Label(transform.position + Vector3.up * 3, "Idle");
    }

}