using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move_Object : MonoBehaviour
{
    [SerializeField] LayerMask unitLayerMask;

    [Space(10)]

    BoxCollider bc;

    void MyOnTrigger(Collider other)
    {
        //entering the trigger
        Debug.Log("Moving Object");
        

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
