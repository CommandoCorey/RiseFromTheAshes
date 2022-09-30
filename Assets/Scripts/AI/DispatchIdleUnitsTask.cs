using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Dispatch Task", menuName = "Ai Task/Dispatch Idle Units", order = 2)]
public class DispatchIdleUnitsTask : AiTask
{
    [Header("Unit types to dispatch")]
    public bool armoredFightingVehciels = true;
    public bool halftracks = true;
    public bool mainBattleTanks = true;
    public bool recconaissanceVehicles = true;

    public override string TaskDescription
    {
        get => "Send idle units to player's base";
    }

    public override string ActiveTaskDescription
    {
        get => "Sending units to player's base";
    }

    public override int GetSteelCost() { return 0; }

    public override bool PerformTask()
    {
        ai = FindObjectOfType<AiPlayer>();

        var idleUnits = ai.GetIdleUnits();

        if (armoredFightingVehciels && halftracks && 
            mainBattleTanks && recconaissanceVehicles)
        {
            ai.DispatchUnits(idleUnits.ToList());
        }

        List<Transform> dispatchGroup = new List<Transform>();

        foreach (var unit in idleUnits)
        {
            if (armoredFightingVehciels && unit.gameObject.tag == "AFV")
                dispatchGroup.Add(unit.transform);

            else if (halftracks && unit.gameObject.tag == "APHT")
                dispatchGroup.Add(unit.transform);

            else if (mainBattleTanks && unit.gameObject.tag == "MBT")
                dispatchGroup.Add(unit.transform);

            else if (recconaissanceVehicles && unit.gameObject.tag == "RCV")
                dispatchGroup.Add(unit.transform);
        }

        taskStatus = "Units have been dispatched";
        return true;
    }

    public override bool IsComplete()
    {
        return false;
    }
}