using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Train Task", menuName = "Ai Task/Train Unit", order = 0)]
public class TrainUnitTask : AiTask
{
    public UnitController unitToTrain;

    public bool UnitTrained { get; set; }

    public override string TaskDescription
    {
        get => "Train " + unitToTrain.Name;
    }

    public override string ActiveTaskDescription
    {
        get => "Training " + unitToTrain.Name + "... ";
    }

    public override int GetSteelCost()
    {
        return unitToTrain.Cost;
    }

    public override bool PerformTask()
    {
        if (FindObjectOfType<AiPlayer>())
        {
            var ai = FindObjectOfType<AiPlayer>();

            if (ai.TrainUnit(unitToTrain, this))
            {
                taskStatus = "Training unit";
                return true;
            }

        }
        else if(FindObjectOfType<SimpleAiPlayer>())
        {
            var ai = FindObjectOfType<SimpleAiPlayer>();

            if (ai.TrainUnit(unitToTrain, this))
            {
                taskStatus = "Training unit";
                return true;
            }

        }
        
        return false;
    }

    public override bool IsComplete()
    {
        return UnitTrained;
    }
}
