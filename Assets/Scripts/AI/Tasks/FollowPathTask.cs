using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dispatch Task", menuName = "Ai Task/Send Along Path", order = 2)]
public class FollowPathTask : DispatchWaveTask
{
    public int routeNumber = 0;
    public bool randomlySelectRoute;
    public bool patrolRoute = false;

    private bool completed = false;

    public override string TaskDescription { get => name; }
    public override string ActiveTaskDescription { get => name; }

    public override object Clone()
    {
        var clonedTask = (FollowPathTask) base.Clone();

        clonedTask.routeNumber = routeNumber;
        return clonedTask;
    }

    /*
    public override int GetSteelCost()
    {
        return 0;
    }*/

    public override bool IsComplete()
    {
        return completed;
    }

    public override bool PerformTask()
    {
        unitWave = new List<Transform>();

        if (AddUnitsToWave()) // invoke from parent class
        {
            if (FindObjectOfType<AiPlayer>())
            {
                var ai = FindObjectOfType<AiPlayer>();

                if (patrolRoute)
                    ai.SendOnPatrol(unitWave);
                else
                    ai.SendAlongPath(unitWave, false);

                unitWave.Clear();

                completed = true;

                return true;
            }
            /*
            else if (FindObjectOfType<SimpleAiPlayer>())
            {
                var ai = FindObjectOfType<SimpleAiPlayer>();
                ai.SendOnPatrol(unitWave, waypoints);

                unitWave.Clear();

                completed = true;
                return true;
            }*/
        }

        unitWave.Clear();
        return false;
    }
}
