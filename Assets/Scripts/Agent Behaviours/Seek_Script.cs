using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seek_Script : MonoBehaviour
{
    Behaviour behaviours;
    GameObject target;

    // Start is called before the first frame update
    void Awake()
    {
        behaviours = GetComponent<Behaviour>();
        target = behaviours.target;

        if(behaviours.seek == null)
        {
            behaviours.seek = gameObject.AddComponent<SeekBehaviour>();
            behaviours.seek.target = target;
            behaviours.seek.enabled = true;

            behaviours.flee = gameObject.AddComponent<FleeBehaviour>();
            behaviours.flee.target = target;
            behaviours.enabled = true;

        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        Destroy(behaviours.seek);
    }

    private void OnDrawGizmos()
    {
        UnityEditor.Handles.Label(transform.position + Vector3.up * 3, "Seek");
    }
}
