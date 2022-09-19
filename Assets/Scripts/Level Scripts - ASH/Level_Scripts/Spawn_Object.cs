using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn_Object : MonoBehaviour
{
    [SerializeField] LayerMask unitLayerMask;

    [Space(10)]

    public List<GameObject> SpawnObjects = new List<GameObject> ();
    public GameObject Remove;

    // Start is called before the first frame update
    void OnTriggerEnter(Collider other)
    {
        //entering the trigger
        Debug.Log("Spawning Object");
        if (((1 << other.gameObject.layer) & unitLayerMask) == 0)
        {
            foreach (var g in SpawnObjects) {
                g.SetActive(true);
            }

            Remove.SetActive(false);
        }

    }
}