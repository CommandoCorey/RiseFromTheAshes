using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Dispatch Task", menuName = "Ai Task/Dispatch Units", order = 2)]
public class DispatchUnitsTask : AiTask
{
    public bool dispatchAllUnits = false;
    //DispatchableUnit[] units;

    public int armedFightingVehicles;
    public int halfTracks;
    public int mainBattleTanks;
    public int reconnaissaneVehicles;

    public override int GetSteelCost() { return 0; }

    public override bool PerformTask()
    {
        if (dispatchAllUnits)
        {
            ai.DispatchAllUnits();
            return true;
        }

        List<Transform> UnitsToDispatch = new List<Transform>();

        var units = GameObject.FindObjectsOfType<UnitController>();

        foreach(var unit in units)
        {
            if(unit.gameObject.layer == LayerMask.NameToLayer("AiUnit"))
            {
                UnitsToDispatch.Add(unit.transform);
            }
        }

        return ai.DispatchUnits(UnitsToDispatch);
    }

}