using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ai Player", menuName = "Ai Player Strategy", order = 1)]
public class AiStrategy : ScriptableObject
{
    public TaskSet[] tasksSchedule;
}
