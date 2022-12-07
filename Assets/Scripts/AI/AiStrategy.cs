using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ai Player", menuName = "Ai Player Strategy", order = 1)]
public class AiStrategy : ScriptableObject, ICloneable
{
    public float delayBetweenTasks = 0;
    public TaskSet[] tasksSchedule;

    public object Clone()
    {
        List<TaskSet> copiedTaskSchedule = new List<TaskSet>();

        foreach(TaskSet set in tasksSchedule)
        {
            copiedTaskSchedule.Add(set);
        }

        return copiedTaskSchedule.ToArray();
    }
}
