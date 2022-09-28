using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Build Task", menuName = "Ai Task/Construct Building", order = 1)]
public class BuildTask : AiTask
{
    public Building buildingToConstruct;

    [Range(0, 7)]
    public int placeholderNumber;

    public override int GetSteelCost()
    {
        return buildingToConstruct.steelCost;
    }

    public override bool PerformTask()
    {
        ai = FindObjectOfType<AiPlayer>();

        if (!ai.PlaceHoldersLeft)
        {
            Debug.LogError("There are no more placeholders to construct the next building");
            return false;
        }

        Ghost ghostBuilding = ai.GetPlaceholder(placeholderNumber);
        ai.ConstructBuilding(ghostBuilding, buildingToConstruct);

        return true;
    }
}
