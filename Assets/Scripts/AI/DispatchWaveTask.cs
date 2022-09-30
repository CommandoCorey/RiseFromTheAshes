using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static DispatchWaveTask;

[CreateAssetMenu(fileName = "Dispatch Wave Task", menuName = "Ai Task/Dispatch Unit Wave", order = 2)]
public class DispatchWaveTask : AiTask
{
    public enum UnitType
    {
        AFV, APHT, MBT, RCV
    }

    [System.Serializable]
    public struct DispatchableUnit
    {
        public UnitType type;
        public int quantity;
    }

    public DispatchableUnit[] enemyWave;

    public override string TaskDescription
    {
        get => " Send " + this.name + " to player's base";
    }

    public override string ActiveTaskDescription
    {
        get => "Sending wave to player's base";
    }

    public override int GetSteelCost() { return 0; }

    public override bool PerformTask()
    {
        ai = FindObjectOfType<AiPlayer>();

        var afvs = GameObject.FindGameObjectsWithTag("ai AFV");
        var aphts = GameObject.FindGameObjectsWithTag("ai APHT");
        var mbts = GameObject.FindGameObjectsWithTag("ai MBT");
        var rcvs = GameObject.FindGameObjectsWithTag("ai RCV");

        List<Transform> unitWave = new List<Transform>();

        // ensure that there are enough units of each type in the scene
        foreach (var unitType in enemyWave)
        {
            switch(unitType.type)
            {
                case UnitType.AFV:
                    if (afvs.Length < unitType.quantity)
                    {
                        taskStatus = "Not enough units";
                        return false;
                    }

                    for(int i=0; i< unitType.quantity; i++)
                    {
                        unitWave.Add(afvs[i].transform);
                    }
                break;

                case UnitType.APHT:
                    if (aphts.Length < unitType.quantity)
                    {
                        taskStatus = "Not enough units";
                        return false;                        
                    }                        

                    for (int i = 0; i < unitType.quantity; i++)
                    {
                        unitWave.Add(aphts[i].transform);
                    }

                break;

                case UnitType.MBT:
                    if (mbts.Length < unitType.quantity)
                    {
                        taskStatus = "Not enough units";
                        return false;
                    }

                    for (int i = 0; i < unitType.quantity; i++)
                    {
                        unitWave.Add(mbts[i].transform);
                    }
                break;

                case UnitType.RCV:
                    if (rcvs.Length < unitType.quantity)
                    {
                        taskStatus = "Not enough units";
                        return false;
                    }

                    for (int i = 0; i < unitType.quantity; i++)
                    {
                        unitWave.Add(rcvs[i].transform);
                    }
               break;
            }
        }

        ai.DispatchUnits(unitWave);

        return true;
    }

    public override bool IsComplete()
    {
        return false;
    }
}