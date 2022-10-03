using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Build Task", menuName = "Ai Task/Construct Building", order = 1)]
public class BuildTask : AiTask
{
    public Building buildingToConstruct;

    public bool autoSelectPlaeholder = true;
    [Range(0, 7)]
    public int placeholderNumber;

    private Building instance;

    public override string TaskDescription { 
        get => "Build " + buildingToConstruct.buildingName;
    }

    public override string ActiveTaskDescription
    {
        get => "Constructing " + buildingToConstruct.buildingName + "... ";
    }

    public override int GetSteelCost()
    {
        return buildingToConstruct.steelCost;
    }

    public override bool PerformTask()
    {
        if (FindObjectOfType<AiPlayer>())
        {
            var ai = FindObjectOfType<AiPlayer>();

            if (!ai.PlaceHoldersLeft)
            {
                taskStatus = "No placeholders left";
                Debug.LogError("There are no more placeholders to construct the next building");
                return false;
            }

            Transform ghostBuilding;
            if (autoSelectPlaeholder)
                ghostBuilding = ai.GetPlaceholder(0);
            else
                ghostBuilding = ai.GetPlaceholder(placeholderNumber);

            ai.ConstructBuilding(ghostBuilding, buildingToConstruct, this);

            taskStatus = "Constructing building";
            return true;

        }
        else if (FindObjectOfType<SimpleAiPlayer>())
        {
            var ai = FindObjectOfType<SimpleAiPlayer>();

            if (!ai.PlaceHoldersLeft)
            {
                taskStatus = "No placeholders left";
                Debug.LogError("There are no more placeholders to construct the next building");
                return false;
            }

            Transform ghostBuilding;
            if (autoSelectPlaeholder)
                ghostBuilding = ai.GetPlaceholder(0);
            else
                ghostBuilding = ai.GetPlaceholder(placeholderNumber);

            ai.ConstructBuilding(ghostBuilding, buildingToConstruct, this);

            taskStatus = "Constructing building";
            return true;

        }

        return false;
    }

    public void SetBuildingInstance(Building building)
    {
        instance = building;
    }

    public override bool IsComplete()
    {
        return instance.IsBuilding == false;
    }
}
