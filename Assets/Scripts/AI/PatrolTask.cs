using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Patrol Task", menuName = "Ai Task/Send To Waypoint", order = 2)]
public class SendToWaypointTask : DispatchWaveTask
{
    public int routeNumber = 0;

    private bool completed = false;

    public override string TaskDescription { get => name; }
    public override string ActiveTaskDescription { get => name; }

    public override object Clone()
    {
        var clonedTask = (SendToWaypointTask) base.Clone();

        clonedTask.routeNumber = routeNumber;
        return clonedTask;
    }

    public override int GetSteelCost()
    {
        return 0;
    }

    public override bool IsComplete()
    {
        return completed;
    }

    public override bool PerformTask()
    {
        var afvs = GameObject.FindGameObjectsWithTag("ai AFV");
        var aphts = GameObject.FindGameObjectsWithTag("ai APHT");
        var mbts = GameObject.FindGameObjectsWithTag("ai MBT");
        var rcvs = GameObject.FindGameObjectsWithTag("ai RCV");

        List<Transform> unitWave = new List<Transform>();

        // ensure that there are enough units of each type in the scene
        foreach (var unitType in enemyWave)
        {
            switch (unitType.type)
            {
                case UnitType.AFV:
                    if (afvs.Length < unitType.quantity)
                    {
                        taskStatus = "Not enough units";
                        return false;
                    }

                    for (int i = 0; i < unitType.quantity; i++)
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

        if (FindObjectOfType<AiPlayer>())
        {
            var ai = FindObjectOfType<AiPlayer>();
            ai.SendOnPatrol(unitWave);

            completed = true;

            return true;
        }
        else if (FindObjectOfType<SimpleAiPlayer>())
        {
            var ai = FindObjectOfType<SimpleAiPlayer>();
            //ai.SendOnPatrol(unitWave, waypoints);

            completed = true;
            return true;
        }

        return false;
    }
}
