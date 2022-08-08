using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : MonoBehaviour
{
    Behaviour behaviours;

    // Start is called before the first frame update
    void Awake()
    {
        behaviours = GetComponent<Behaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        var unitsInRange = Physics.OverlapSphere(transform.position, behaviours.DetectionRadius, behaviours.DetectionLayer);

        if (unitsInRange.Length > 0)
        {
            behaviours.target = unitsInRange[0].gameObject;
            behaviours.ChangeState(UnitState.Attack);
        }
    }

}
