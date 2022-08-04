using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Behaviour : MonoBehaviour
{
    [SerializeField] int team;
    public GameObject target;

    [HideInInspector]
    public SeekBehaviour seek;
    [HideInInspector]
    public FleeBehaviour flee;

    // intelligent movement script
    Agent agent;

    public enum UnitState
    {
        Idle,
        Seek,
        Attack
    }

    // Start is called before the first frame update
    void Start()
    {
        agent = gameObject.AddComponent<Agent>(); // add agent component to game object
        seek = gameObject.GetComponent<SeekBehaviour>();


        if (seek == null)
        {
            seek = gameObject.AddComponent<SeekBehaviour>();
            seek.target = target;
            seek.enabled = true;

            flee = gameObject.AddComponent<FleeBehaviour>();
            flee.target = target;
            enabled = true;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        Destroy(seek);
        Destroy(flee);
    }

    private void OnDrawGizmos()
    {
        UnityEditor.Handles.Label(transform.position + Vector3.up * 3, "Seek");
    }

}
