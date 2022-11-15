using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class AiTaskScheduler : MonoBehaviour
{
    [Header("Ai Tasks")]
    [SerializeField] AiStrategy playerStrategy;
    public TaskSet[] tasksSchedule;

    [Header("Info Panel")]
    [SerializeField] bool showInfoPanel;
    [SerializeField] GameObject infoPanel;
    [SerializeField] TextMeshProUGUI steelCurrentAmount;
    [SerializeField] TextMeshProUGUI steelMaxAmount;
    [SerializeField] TextMeshProUGUI totalUnitAmount;
    [SerializeField] TextMeshProUGUI maxUnitAmount;
    [SerializeField] Transform taskListPanel;
    [SerializeField] TaskSetDisplay taskSetPanelPrefab;
    [SerializeField] TextMeshProUGUI activeTaskList;

    private List<AiTask> activeTasks;
    private List<TaskSetDisplay> taskDisplays;

    private GameManager gameManager;
    private ResourceManager resources;

    // private variables form AiPlayer
    private int steel = 0;
    private int maxSteel;
    private List<Building> baysInConstruction;

    private AiPlayer aiPlayer;

    public static AiTaskScheduler Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        resources = ResourceManager.Instance;        
        gameManager = GameManager.Instance;
        aiPlayer = GetComponent<AiPlayer>();

        activeTasks = new List<AiTask>();

        baysInConstruction = aiPlayer.BaysInConstruction;

        if (playerStrategy != null)
            tasksSchedule = (TaskSet[])playerStrategy.Clone();

        //SortTasks();

        foreach (TaskSet set in tasksSchedule)
        {
            set.TaskNum = 0;
            set.ReadyToPerform = true;
        }

        // instantiate task sets on info panel
        taskDisplays = new List<TaskSetDisplay>();
        for (int i = 0; i < tasksSchedule.Length; i++)
        {
            taskDisplays.Add(Instantiate(taskSetPanelPrefab, taskListPanel));
            taskDisplays[i].taskSetNumber.text = "Task Set " + i + ":";
        }        
        
    }

    // Update is called once per frame
    void Update()
    {
        steel = resources.GetResource(ResourceType.Steel, true);
        maxSteel = resources.GetResourceMax(ResourceType.Steel, true);

        // toggle info panel
        if (showInfoPanel)
        {
            infoPanel.SetActive(true);
            UpdateInfoPanel();
        }
        else
        {
            infoPanel.SetActive(false);
        }

        // check status of current tasks
        foreach (TaskSet set in tasksSchedule)
        {
            // if not looping move to the next set
            if (set.TaskNum >= set.tasks.Count && !set.loopTaskSet)
                continue;

            if (steel >= set.tasks[set.TaskNum].GetSteelCost() && set.ReadyToPerform)
            {
                StartCoroutine(PerformNextTask(set));
                set.ReadyToPerform = false;
            }
        }

        // check if vehicle bays in construction have finished building
        foreach (Building v in baysInConstruction)
        {
            if (v.IsBuilt)
            {
                aiPlayer.AddVehicleBay(v.GetComponent<VehicleBay>());
                baysInConstruction.Remove(v);
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.Tilde))
        {
            infoPanel.SetActive(!infoPanel.activeSelf);
        }
    }

    // sorts tasks in a specific task set by highest to lowst priority score
    private void SortTaskSet(TaskSet set)
    {
        RemoveNullTasks(set.tasks);

        var sortedTasks = from t in set.tasks
                          orderby t.priorityScore descending
                          select t;

        set.tasks = sortedTasks.ToList();
    }



    // checks if any taks in a set is null and if they are, removes them
    private void RemoveNullTasks(List<AiTask> tasks)
    {
        foreach (AiTask task in tasks)
        {
            if (task == null)
            {
                tasks.Remove(task);
                RemoveNullTasks(tasks);
                return;
            }

        }
    }

    private IEnumerator PerformNextTask(TaskSet set)
    {
        yield return new WaitForSeconds(set.tasks[set.TaskNum].timeDelay);

        if (set.tasks[set.TaskNum].PerformTask()) // attempt to perform the task
        {
            activeTasks.Add(set.tasks[set.TaskNum]);
            set.TaskNum++;

            if (set.TaskNum >= set.tasks.Count && set.loopTaskSet)
            {
                set.TaskNum = 0;
            }
        }

        set.ReadyToPerform = true;
    }

    private void UpdateInfoPanel()
    {
        steelCurrentAmount.text = steel.ToString();
        steelMaxAmount.text = maxSteel.ToString();
        totalUnitAmount.text = gameManager.UnitCountAi.ToString();
        maxUnitAmount.text = gameManager.MaxUnitsAi.ToString();

        for (int i = 0; i < tasksSchedule.Length; i++)
        {
            if (tasksSchedule[i].TaskNum >= tasksSchedule[i].tasks.Count)
            {
                taskDisplays[i].taskDescription.text = "None";
                taskDisplays[i].taskStatus.text = "Task Set complete";
            }
            else
            {
                //try
                //{
                    AiTask task = tasksSchedule[i].tasks[tasksSchedule[i].TaskNum];

                    taskDisplays[i].taskDescription.text = task.TaskDescription;

                    if (steel < task.GetSteelCost())
                        taskDisplays[i].taskStatus.text = "Not enough Steel";
                    else
                        taskDisplays[i].taskStatus.text = task.TaskStatus;
                //}
                //catch (Exception e)
                //{
                //    Debug.LogException(e);
                //}
            }
        }

        UpdateActiveTasks();

        // display active tasks
        activeTaskList.text = "";
        foreach (var task in activeTasks)
        {
            //if (task.IsComplete())
            //activeTaskList.text += "DONE\n";            
            //else
            activeTaskList.text += task.ActiveTaskDescription + "\n";
        }
    }

    private void UpdateActiveTasks()
    {
        foreach (var task in activeTasks)
        {
            if (task.IsComplete())
            {
                activeTasks.Remove(task);
                UpdateActiveTasks();

                return;
            }
        }
    }

    private void SortTasks()
    {
        foreach (TaskSet set in tasksSchedule)
        {
            RemoveNullTasks(set.tasks);

            var sortedTasks = from t in set.tasks
                              orderby t.priorityScore descending
                              select t;

            set.tasks = sortedTasks.ToList();
        }
    }

    /// <summary>
    /// Adds a new build task to the rebuild set at runtime
    /// </summary>
    /// <param name="BuildingTag">The tag of the game object to be instantiated</param>
    /// <param name="placholder">Game Object with the Ghost script to build on</param>
    public void AddRebuildTask(string buildingTag, Transform placholder)
    {
        BuildTask rebuildTask = ScriptableObject.CreateInstance<BuildTask>();
        Building buildingToConstruct = null;

        foreach (Transform building in aiPlayer.BuildingPrefabs)
        {
            if (building.tag == buildingTag)
            {
                buildingToConstruct = building.GetComponent<Building>();
                break;
            }
        }

        if (buildingToConstruct == null)
        {
            Debug.LogError("No prefab found in list with tag '" + buildingTag + "'");
            return;
        }

        rebuildTask.name = "Build " + buildingToConstruct.buildingName;
        rebuildTask.buildingToConstruct = buildingToConstruct;
        rebuildTask.placeholderNumber = aiPlayer.BuildingPlaceholders.IndexOf(placholder);
        rebuildTask.timeDelay = 0;

        TaskSet taskSet = tasksSchedule[0];
        foreach (TaskSet set in tasksSchedule)
        {
            if (set.addRebuildTasks)
            {
                taskSet = set;
                break;
            }
        }

        taskSet.tasks.Add(rebuildTask);
        SortTaskSet(taskSet);
    }

    private void ClearRebuildTasks()
    {       
        foreach (TaskSet set in tasksSchedule)
        {
            if (set.addRebuildTasks)
            {
                set.tasks.Clear();
            }
        }
    }

    private void OnDestroy()
    {      
        ClearRebuildTasks();
    }
}
