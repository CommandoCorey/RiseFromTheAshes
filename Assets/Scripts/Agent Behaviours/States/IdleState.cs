using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : MonoBehaviour
{
    StateManager behaviours;

    // Start is called before the first frame update
    void Awake()
    {
        behaviours = GetComponent<StateManager>();
    }

    // Update is called once per frame
    void Update()
    {
        var unitsInRange = Physics.OverlapSphere(transform.position, behaviours.DetectionRadius, behaviours.DetectionLayer);

        if (unitsInRange.Length > 0)
        {
            behaviours.AttackTarget = unitsInRange[0].gameObject;
            behaviours.ChangeState(UnitState.Attack);
        }
    }

}
