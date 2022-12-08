using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static DispatchWaveTask;

[CreateAssetMenu(fileName = "Dispatch Wave Task", menuName = "Ai Task/Dispatch Unit Wave", order = 2)]
public class DispatchWaveTask : AiTask
{
    protected List<Transform> unitWave;

    private AiUnit unit;    

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
        get => "Send " + this.name + " to player's base";
    }

    public override string ActiveTaskDescription
    {
        get => "Sending wave to player's base";
    }

    public override object Clone()
    {
        taskStatus = "Not enough units";
        var clonedTask = new DispatchWaveTask();
        clonedTask.enemyWave = enemyWave;
        clonedTask.taskStatus = taskStatus;
        return clonedTask;
    }

    /*
    public override int GetSteelCost() { return 0; }
    */

    public override bool CanPerform()
    {
        if (AddUnitsToWave())
        {
            unitWave.Clear();
            return true;
        }

        unitWave.Clear();
        return false;

    }

    public override bool PerformTask()
    {
        unitWave = new List<Transform>();

        if (AddUnitsToWave())
        {
            if (FindObjectOfType<AiPlayer>())
            {
                var ai = FindObjectOfType<AiPlayer>();
                ai.DispatchUnits(unitWave);

                unitWave.Clear();

                return true;
            }
            else if (FindObjectOfType<SimpleAiPlayer>())
            {
                var ai = FindObjectOfType<SimpleAiPlayer>();
                ai.DispatchUnits(unitWave);

                unitWave.Clear();

                return true;
            }
        }

        unitWave.Clear();

        return false;
    }

    public override bool IsComplete()
    {
        return true;
    }

    protected bool AddUnitsToWave()
    {
        var afvs = GetFreeUnits("ai AFV");            
        var aphts = GetFreeUnits("ai APHT");
        var mbts = GetFreeUnits("ai MBT");
        var rcvs = GetFreeUnits("ai RCV");

        // ensure that there are enough units of each type in the scene
        foreach (var unitType in enemyWave)
        {
            switch (unitType.type)
            {
                case UnitType.AFV:

                    // Todo: check if unit is in defensive mode

                    if (afvs.Count < unitType.quantity)
                    {
                        taskStatus = "Not enough units";
                        return false;
                    }

                    for (int i = 0; i < unitType.quantity; i++)
                    {
                        if(!afvs[i].IsInWave)
                        {
                            unitWave.Add(afvs[i].transform);
                        }
                                              
                    }
                    break;

                case UnitType.APHT:

                    // Todo: check if unit is in defensive mode

                    if (aphts.Count < unitType.quantity)
                    {
                        taskStatus = "Not enough units";
                        return false;
                    }

                    for (int i = 0; i < unitType.quantity; i++)
                    {
                        if(!aphts[i].IsInWave)
                        {
                            unitWave.Add(aphts[i].transform);                            
                        }

                    }

                    break;

                case UnitType.MBT:

                    // Todo: check if unit is in defensive mode

                    if (mbts.Count < unitType.quantity)
                    {
                        taskStatus = "Not enough units";
                        return false;
                    }

                    for (int i = 0; i < unitType.quantity; i++)
                    {
                        if (!mbts[i].IsInWave)
                        {
                            unitWave.Add(mbts[i].transform);                            
                        }
                    }
                    break;

                case UnitType.RCV:

                    // Todo: check if unit is in defensive mode

                    if (rcvs.Count < unitType.quantity)
                    {
                        taskStatus = "Not enough units";
                        return false;
                    }

                    for (int i = 0; i < unitType.quantity; i++)
                    {
                        if (!rcvs[i].IsInWave)
                        {
                            unitWave.Add(rcvs[i].transform);                            
                        }
                    }
                    break;
            }
        }

        return true;
    }

    /*
    private bool IsInWave(GameObject obj)
    {
        unit = obj.GetComponent<AiUnit>();
        return unit.IsInWave;
    }*/

    // returns list of units matching a tag that are
    // not already in an attack wave
    private List<AiUnit> GetFreeUnits(string tag)
    {
        var unitTransforms = GameObject.FindGameObjectsWithTag(tag);

        List<AiUnit> freeUnits = new List<AiUnit>();

        foreach (var t in unitTransforms)
        {
            var unit = t.GetComponent<AiUnit>();
            
            if(!unit.IsInWave)
            {
                freeUnits.Add(unit);
            }
        }

        return freeUnits;       
    }    

    /*
    private void AddToWave(Transform uniTransform)
    { 
        unitWave.Add(uniTransform);
    }*/

}