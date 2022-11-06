using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class kill : MonoBehaviour
{
    [SerializeField] LayerMask unitLayerMask;

    [Space(10)]

    BoxCollider bc;

    public List<GameObject> KillObjects = new List<GameObject>();
    public GameObject Remove;

    // Start is called before the first frame update
    void MyOnTrigger(Collider other)
    {
        //entering the trigger
        Debug.Log("Object Died");
        foreach (var g in KillObjects)
        {
            Destroy(gameObject);
        }

        Remove.SetActive(false);

    }
    private void OnEnable()
    {
        bc = GetComponent<BoxCollider>();
    }

    private void FixedUpdate()
    {
        var hits = Physics.OverlapBox(bc.bounds.min, (bc.bounds.max - bc.bounds.min) * 0.5f, Quaternion.identity, unitLayerMask);
        foreach (var hit in hits) { MyOnTrigger(hit); }
    }
}
