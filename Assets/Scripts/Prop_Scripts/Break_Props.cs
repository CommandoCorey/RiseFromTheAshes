using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Break_Props : MonoBehaviour
{
    BoxCollider bc;
    [SerializeField] LayerMask unitLayerMask;

    void MyOnTrigger(Collider other)
    {
        var p = Instantiate(GameManager.Instance.destroyPropEffect, transform.position, Quaternion.identity);
        Destroy(p, 1.0f);

        Destroy(gameObject);
    }

    private void OnEnable()
    {
        bc = GetComponent<BoxCollider>();
    }

    private void FixedUpdate()
    {
        var hits = Physics.OverlapBox(
            bc.bounds.min + (bc.bounds.max - bc.bounds.min) * 0.5f,
            (bc.bounds.max - bc.bounds.min) * 0.5f, Quaternion.identity, unitLayerMask);
        foreach (var hit in hits) { MyOnTrigger(hit); }
    }
}
