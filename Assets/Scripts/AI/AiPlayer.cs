using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class TaskSet
{
    public string description;
    public bool loopTaskSet;
    public bool addRebuildTasks;
    public bool addOutpostBuildTasks;
    public List<AiTask> tasks;

    public int TaskNum { get; set; } = 0;
    public bool ReadyToPerform { get; set; } = true;
}

public class AiPlayer : MonoBehaviour
{
    public Transform playerBase;    
    public Transform rallyPoint;
    [SerializeField] List<Transform> buildingPlaceholders;
    [SerializeField] List<Transform> outpostPlaceholders;

    public List<VehicleBay> vehicleBays;
    public Transform[] waypoints;
    public Transform[] patrolRoute;

    [Header("Ai Tasks")]
    [SerializeField] AiStrategy playerStrategy;
    [SerializeField] int rebuiltSetNumber = 0;
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

    //[SerializeField] TextMeshProUGUI taskDescription;
    //[SerializeField] TextMeshProUGUI taskStatus;
    [SerializeField] TextMeshProUGUI activeTaskList;

    // private variables
    private ResourceManager resources;
    private int steel = 0;
    private int maxSteel;
    private UnitController unit;

    private int unitsTrained = 0;

    private float zOffset = 6;
    private float unitsPerRow = 5;
    private float spaceBetween = 4;    

    // unit management
    private List<UnitController> aiUnits;
    private List<Transform> unitGroup;
    private List<Vector3> formationPositions;

    private List<Building> baysInConstruction;

    private GameManager gameManager;
    private FormationManager formations;

    private List<AiTask> activeTasks;
    private List<TaskSetDisplay> taskDisplays;

    public bool PlaceHoldersLeft { get => buildingPlaceholders.Count > 0; }
    public bool OutpostPlaceholdersLeft { get=> outpostPlaceholders.Count > 0; }

    public static AiPlayer Instance { get; private set; }

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
        formations = FormationManager.Instance;
        gameManager = GameManager.Instance;

        if (playerStrategy != null)
            tasksSchedule = (TaskSet[]) playerStrategy.Clone();

        aiUnits = new List<UnitController>();
        unitGroup = new List<Transform>();
        baysInConstruction = new List<Building>();        

        activeTasks = new List<AiTask>();

        SortTasks();

        foreach (TaskSet set in tasksSchedule)
        {
            set.TaskNum = 0;
            set.ReadyToPerform = true;
        }

        // instantiate task sets on info panel
        taskDisplays = new List<TaskSetDisplay>();
        for (int i= 0; i < tasksSchedule.Length; i++)
        {
            taskDisplays.Add(Instantiate(taskSetPanelPrefab, taskListPanel));
            taskDisplays[i].taskSetNumber.text = "Task Set " + i + ":";
        }

        resources.AddResourceToAI(ResourceType.Steel, 300);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.State != GameState.Running)
            return;

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
                vehicleBays.Add(v.GetComponent<VehicleBay>());
                baysInConstruction.Remove(v);
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.Tilde))
		{
            infoPanel.SetActive(!infoPanel.activeSelf);
		}
    }

    #region private functions
    // sorts tasks in every set by highest to lowst priority score
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
        foreach(AiTask task in tasks)
        {
            if(task == null)
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
                AiTask task = tasksSchedule[i].tasks[tasksSchedule[i].TaskNum];

                taskDisplays[i].taskDescription.text = task.TaskDescription;

                if (steel < task.GetSteelCost())
                    taskDisplays[i].taskStatus.text = "Not enough Steel";
                else
                    taskDisplays[i].taskStatus.text = task.TaskStatus;
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
                //UpdateActiveTasks();

                return;
            }
        }
    }

    private IEnumerator StartTraining(VehicleBay vehicleBay, UnitController unit, TrainUnitTask task = null)
    {
        //Debug.Log("Training Unit");
        resources.AiSpendSteel(unit.Cost);
        steel = resources.aiResources[0].currentAmount;
        vehicleBay.IsTraining = true;
        task.UnitTrained = false;

        yield return new WaitForSeconds(unit.TimeToTrain);

        vehicleBay.IsTraining = false;

        var unitInstance = Instantiate(unit, vehicleBay.spawnLocation.position, Quaternion.identity);
        unitInstance.body.forward = vehicleBay.transform.forward;

        unitInstance.MoveToRallyPoint(rallyPoint.position);

        //Debug.Log("Training Complete");
        aiUnits.Add(unitInstance);
        unitGroup.Add(unitInstance.transform);

        unitsTrained++;

        if (unitsTrained % 5 == 0)
        {
            zOffset += 6;
        }

        task.UnitTrained = true;
    }

    private Vector3 GetSpawnPosition(Transform vehicleBay)
    {
        float x = vehicleBay.position.x + (unitsTrained % unitsPerRow * spaceBetween);
        float z = vehicleBay.position.z + vehicleBay.forward.z * zOffset;

        return new Vector3(x, 0, z);
    }
    #endregion

    #region public functions
    /// <summary>
    /// Adds a new build task to the rebuild set at runtime
    /// </summary>
    /// <param name="building">The building to be rebuilt</param>
    public void AddRebuildTask(Building building)
    {
        BuildTask rebuildTask = ScriptableObject.CreateInstance<BuildTask>();

        rebuildTask.name = "Build " + building.buildingName;
        rebuildTask.buildingToConstruct = building;
        rebuildTask.autoSelectPlaceholder = true;
        rebuildTask.timeDelay = 0;

        TaskSet taskSet = tasksSchedule[0];
        foreach(TaskSet set in tasksSchedule)
        {
            if(set.addRebuildTasks)
            {
                taskSet = set;
                break;
            }
        }

        tasksSchedule[rebuiltSetNumber].tasks.Add(rebuildTask);
        SortTaskSet(tasksSchedule[rebuiltSetNumber]);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="units"></param>
    /// <param name="number">The index in the list of outposts to use</param>
    public void SendToOutpost(List<Transform> units, int number)
    {
        foreach (Transform t in units)
        {
            var unit = t.GetComponent<UnitController>();
            unit.ChangeState(UnitState.Moving, waypoints[number].position);
        }
    }

    /// <summary>
    /// Sends all units in a specified list along the patrol route 
    /// </summary>
    /// <param name="units">list of transforms containing the UnitController</param>
    public void SendOnPatrol(List<Transform> units)
    {
        foreach (Transform t in units)
        {
            var unit = t.GetComponent<UnitController>();
            unit.ChangeState(UnitState.Patrol);

            var patrolState = unit.GetComponent<PatrolState>();
            patrolState.SetPatrolRoute(patrolRoute);
        }
    }

    /// <summary>
    /// Add outpost tests
    /// </summary>
    /// <param name="outpostPlaceholder"></param>
    public void AddOutpost(Transform outpostPlaceholder)
    {

    }

    public bool TrainUnit(UnitController unit, TrainUnitTask task = null)
    {
        // check for available vehicle bays
        if(vehicleBays.Count == 0)
        {
            task.TaskStatus = "No vehicle bays in base";
            return false;
        }

        if(gameManager.UnitCountAi >= gameManager.MaxUnitsAi)
        {
            task.TaskStatus = "Not enough space for more units";
            return false;
        }

        foreach(VehicleBay bay in vehicleBays)
        {
            if(!bay.IsTraining)
            {
                StartCoroutine(StartTraining(bay, unit, task));
                //Debug.Log("Training Unit");

                return true;
            }
            
        }

        task.TaskStatus = "All vehicle bays are busy";
        return false;
    }

    public void ConstructBuilding(Transform ghostBuilding, Building buildItem, BuildTask task = null)
    {
        resources.AiSpendSteel(buildItem.steelCost);

        // create the building
        Building building = Instantiate(buildItem, ghostBuilding.position, ghostBuilding.rotation);
        ghostBuilding.gameObject.SetActive(false);
        building.Build();

        if (building.gameObject.CompareTag("VehicleBay") && building.GetComponent<VehicleBay>())
            baysInConstruction.Add(building);

        buildingPlaceholders.Remove(ghostBuilding);

        task.SetBuildingInstance(building);
    }

    public void DispatchAllUnits()
    {
        formationPositions = formations.GetFormationPositions(playerBase.position, unitGroup);

        if (formationPositions.Count < unitGroup.Count)
        {
            Debug.LogError("Not enough formations positions were created for the selected units");
            return;
        }

        // move all units to their designated target positions
        for (int i = 0; i < unitGroup.Count; i++)
        {
            var unit = unitGroup[i].GetComponent<UnitController>();

            unit.ChangeState(UnitState.Moving, formationPositions[i]);
            unit.MovingToBase = true;
        }
        
    }

    public bool DispatchUnits(List<Transform> unitGroup)
    {
        formationPositions = formations.GetFormationPositions(playerBase.position, unitGroup);

        if (formationPositions.Count < unitGroup.Count)
        {
            Debug.LogError("Not enough formations positions were created for the selected units");
            return false;
        }

        // move all units to their designated target positions
        for (int i = 0; i < unitGroup.Count; i++)
        {
            var unit = unitGroup[i].GetComponent<UnitController>();

            unit.ChangeState(UnitState.Moving, formationPositions[i]);
            unit.MovingToBase = true;
        }

        Debug.Log("Dispatching Units");
        return true;
    } 

    public Transform GetPlaceholder(int number)
    {
        if(number >-1 && number < buildingPlaceholders.Count)
            return buildingPlaceholders[number];

        return buildingPlaceholders[0];
    }

    public Transform GetOutpostPlaceholder(int number)
    {
        if (number > -1 && number < outpostPlaceholders.Count)
            return buildingPlaceholders[number];

        return buildingPlaceholders[0];
    }

    public Transform[] GetIdleUnits()
    {
        List<Transform> idleUnits = new List<Transform>();

        foreach(var unit in aiUnits)
        {
            if (unit.State == UnitState.Idle)
                idleUnits.Add(unit.transform);
        }

        return idleUnits.ToArray();
    }
    #endregion

    private void OnDrawGizmos()
    {
        // draws the fromation positions that each unit will finish at
        if (formationPositions != null)
        {
            foreach (Vector3 position in formationPositions)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(position, 1);
            }
        }

        //if(vehicleBay != null)
            //Gizmos.DrawLine(vehicleBay.position, vehicleBay.position + vehicleBay.forward * zOffset);
    }
    
}