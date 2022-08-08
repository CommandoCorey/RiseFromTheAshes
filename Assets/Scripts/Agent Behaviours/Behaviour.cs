using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Behaviour : MonoBehaviour
{
    [SerializeField] int team;
    public GameObject target;

    [HideInInspector]
    public SeekBehaviour seek;
    //[HideInInspector]
    //public FleeBehaviour flee;

    FlockState seekState;

    // Flocking behaviours
    [HideInInspector]
    public BoidCohesion cohesion;
    [HideInInspector]
    public BoidSepearation sepearation;

    // intelligent movement script
    Agent agent;

    UnitState state;

    public enum UnitState
    {
        Idle,
        Flock,
        Attack
    }
    

    // Start is called before the first frame update
    void Start()
    {
        //agent = gameObject.AddComponent<Agent>(); // add agent component to game object
        //seek = gameObject.GetComponent<SeekBehaviour>();        

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

    private void ChangeState(UnitState newState)
    {
        state = newState;

        switch(newState)
        {
            case UnitState.Idle:
                DestroyImmediate(seekState);
                //DestroyImmediate(attackState);
            break;

            case UnitState.Flock:
                if(GetComponent<FlockState>() == null)
                {
                    seekState = gameObject.AddComponent<FlockState>();
                }

                //DestroyImmediate(attackState);
            break;

            case UnitState.Attack:
                DestroyImmediate(seekState);
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
    }

}
