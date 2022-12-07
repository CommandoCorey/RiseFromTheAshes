using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[System.Serializable]
public class TaskSet
{
    public string description;
    public bool loopTaskSet;
    public bool addRebuildTasks;
    //public bool addOutpostBuildTasks;
    public bool waitForPreviousTaskSet;
    public List<AiTask> tasks;

    [HideInInspector]
    public String status = "";

    public int TaskNum { get; set; } = 0;
    public bool ReadyToPerform { get; set; } = true;

    public bool Completed { get => TaskNum >= tasks.Count; }
}

public class AiTaskScheduler : MonoBehaviour
{
    public float delayBetweenTasks = 10;
    public bool useIndividualTaskDelay = false;

    [Header("Ai Tasks")]
    [SerializeField] AiStrategy playerStrategyEasy;
    [SerializeField] AiStrategy playerStrategyNormal;
    [SerializeField] AiStrategy playerStrategyHard;
    [SerializeField] AiStrategy playerStrategyVeryHard;

    public TaskSet[] tasksSchedule;

    [Header("Info Panel")]
    [SerializeField] bool showInfoPanel;
    [SerializeField] KeyCode togglePanelKey;
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

    private TaskSet previousSet;

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

        AiStrategy playerStrategy = null;

        switch(AiPlayer.Difficulty)
        {
            case AiDifficulty.Easy: playerStrategy = playerStrategyEasy;
                break;

            case AiDifficulty.Normal: playerStrategy = playerStrategyNormal;
                break;

            case AiDifficulty.Hard: playerStrategy = playerStrategyHard;
                break;

            case AiDifficulty.VeryHard: playerStrategy = playerStrategyVeryHard;
                break;
        }

        SetTimeDelay();

        tasksSchedule = (TaskSet[])playerStrategy.Clone();
        //delayBetweenTasks = playerStrategy.delayBetweenTasks;

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
            taskDisplays[i].taskSetNumber.text = tasksSchedule[i].description +":";
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        previousSet = null;
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

            // Check if the task set requires completion of previous set
            if (set.waitForPreviousTaskSet && previousSet != null && !previousSet.Completed)
            {
                if (set.tasks.Count > 0)
                {
                    set.status = "Waiting for previous set to finish";
                }

                continue;
            }

            if (set.tasks[set.TaskNum].CanPerform() && set.ReadyToPerform)
            {
                StartCoroutine(PerformNextTask(set));
                set.ReadyToPerform = false;                
            }
            else if (set.ReadyToPerform)
            {
                set.status = set.tasks[set.TaskNum].TaskStatus;
            }

            previousSet = set;
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

        if (Input.GetKeyDown(togglePanelKey) || Input.GetKeyDown(KeyCode.Tilde))
        {            
            showInfoPanel = !showInfoPanel;
        }        
    }

    private void SetTimeDelay()
    {
        //Debug.Log("Difficulty: " + AiPlayer.Difficulty);

        switch (AiPlayer.Difficulty)
        {
            case AiDifficulty.Easy:
                delayBetweenTasks = aiPlayer.EASY_TIME_DELAY;
                gameManager.SetDifficultyText("Easy");
            break;

            case AiDifficulty.Normal:
                delayBetweenTasks = aiPlayer.NORMAL_TIME_DELAY;
                gameManager.SetDifficultyText("Normal");
            break;

            case AiDifficulty.Hard:
                delayBetweenTasks = aiPlayer.HARD_TIME_DELAY;
                gameManager.SetDifficultyText("Hard");
            break;

            case AiDifficulty.VeryHard:
                delayBetweenTasks = aiPlayer.VERY_HARD_TIME_DELAY;
                gameManager.SetDifficultyText("Very Hard");
            break;
        }

       //Debug.Log("Delay between tasks: " + delayBetweenTasks + " seconds");
    }

    // sorts tasks in a specific task set by highest to lowst priority score
    private void SortTaskSet(TaskSet set)
    {
        RemoveNullTasks(set.tasks);

        var sortedTasks = from t in set.tasks
                          orderby ((BuildTask) t).priorityScore descending
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
        float delay = useIndividualTaskDelay 
            ? set.tasks[set.TaskNum].timeDelay : delayBetweenTasks;

        // update the status message for the set               
        set.status = "Performing in " + delay + " seconds";

        yield return new WaitForSeconds(delay);

        if (set.tasks[set.TaskNum].PerformTask()) // attempt to perform the task
        {
            set.status = "Task performed";

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
                AiTask task = tasksSchedule[i].tasks[tasksSchedule[i].TaskNum];

                taskDisplays[i].taskDescription.text = task.TaskDescription;
                taskDisplays[i].taskStatus.text = tasksSchedule[i].status;
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

    /* No Longer used
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
    }*/

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