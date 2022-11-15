using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GainSteel : MonoBehaviour
{
    [SerializeField] LayerMask unitLayerMask;

    BoxCollider bc;

    // Start is called before the first frame update
    void MyOnTrigger(Collider other)
    {
        ResourceManager.Instance.AddResource(ResourceType.Steel, 10);

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
