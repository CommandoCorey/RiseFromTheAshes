using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Train Task", menuName = "Ai Task/Train Unit", order = 0)]
public class TrainUnitTask : AiTask
{
    public UnitController unitToTrain;

    public override int GetSteelCost()
    {
        return unitToTrain.Cost;
    }

    public override bool PerformTask()
    {
        ai = FindObjectOfType<AiPlayer>();

        return ai.TrainUnit(unitToTrain);
    }
}
