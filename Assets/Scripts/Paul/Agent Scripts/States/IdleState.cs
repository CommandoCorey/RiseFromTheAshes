using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : MonoBehaviour
{
    UnitController unit;

    // Start is called before the first frame update
    void Awake()
    {
        
        unit = GetComponent<UnitController>();
    }

    // Update is called once per frame
    void Update()
    {
        var enemiesInRange = Physics.OverlapSphere(transform.position, unit.DetectionRadius, unit.DetectionLayer);

        // if there are any enemies in range change to the combat state
        if (enemiesInRange.Length > 0)
        {
            unit.AttackTarget = enemiesInRange[0].gameObject;
            unit.ChangeState(UnitState.Attack);
        }
    }

}
