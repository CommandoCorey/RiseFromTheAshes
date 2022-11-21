using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Break_Props : MonoBehaviour
{
    BoxCollider bc;
    [SerializeField] LayerMask unitLayerMask;

    // Start is called before the first frame update
    void MyOnTrigger(Collider other)
    {
        //entering the trigger
        Debug.Log("Break Prop");

        Destroy(gameObject);
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
