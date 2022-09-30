using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class AiPlayer : MonoBehaviour
{
    public Transform playerBase;
    public Transform vantagePoint;
    [SerializeField] List<Transform> buildingPlaceholders;

    public List<VehicleBay> vehicleBays;

    [SerializeField] bool loopTasks;
    public AiTask[] tasksSchedule;    

    [Header("Info Panel")]
    [SerializeField] bool showInfoPanel;
    [SerializeField] GameObject infoPanel;
    [SerializeField] TextMeshProUGUI steelAmount;
    [SerializeField] TextMeshProUGUI taskDescription;
    [SerializeField] TextMeshProUGUI taskStatus;
    [SerializeField] TextMeshProUGUI activeTaskList;

    // private variables
    private ResourceManager resources;
    private int steel = 0;
    private int taskNum = 0;
    private UnitController unit;

    private int unitsTrained = 0;
    private bool readyToPerform = true;

    private float zOffset = 6;
    private float unitsPerRow = 5;
    private float spaceBetween = 4;    

    // unit management
    private List<UnitController> aiUnits;
    private List<Transform> unitGroup;
    private List<Vector3> formationPositions;

    private List<Building> baysInConstruction;

    private GameManager gameManager;

    List<AiTask> activeTasks;

    public bool PlaceHoldersLeft { get => buildingPlaceholders.Count > 0; }

    // Start is called before the first frame update
    void Start()
    {
        resources = ResourceManager.Instance;
        aiUnits = new List<UnitController>();
        unitGroup = new List<Transform>();
        baysInConstruction = new List<Building>();

        gameManager = FindObjectOfType<GameManager>();

        activeTasks = new List<AiTask>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.State != GameState.Running)
            return;
        
        if (!loopTasks && taskNum >= tasksSchedule.Length)
            return;

        steel = resources.aiResources[0].currentAmount;

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

        if (steel >= tasksSchedule[taskNum].GetSteelCost() && readyToPerform)
        {
            Invoke("PerformNextTask", tasksSchedule[taskNum].timeDelay);
            readyToPerform = false;
        }

        // check if vehicle bays in construction have finished building
        foreach(Building v in baysInConstruction)
        {
            if(v.IsBuilt)
            {
                vehicleBays.Add(v.GetComponent<VehicleBay>());
                baysInConstruction.Remove(v);
                return;
            }
        }
        
    }

    private void PerformNextTask()
    {
        if (tasksSchedule[taskNum].PerformTask()) // attempt to perform the task
        {
            activeTasks.Add(tasksSchedule[taskNum]);
            taskNum++;
        }

        readyToPerform = true;
    }

    private void UpdateInfoPanel()
    {
        steelAmount.text = steel.ToString();
        taskDescription.text = tasksSchedule[taskNum].TaskDescription;

        if (steel < tasksSchedule[taskNum].GetSteelCost())
            taskStatus.text = "Not enough Steel";
        else
            taskStatus.text = tasksSchedule[taskNum].TaskStatus;

        UpdateActiveTasks();

        // display active tasks
        activeTaskList.text = ""; 
        foreach(var task in activeTasks)
        {
            //if (task.IsComplete())
                //activeTaskList.text += "DONE\n";            
            //else
                activeTaskList.text += task.ActiveTaskDescription + "\n";
        }
    }

    private void UpdateActiveTasks()
    {
        foreach(var task in activeTasks)
        {
            if(task.IsComplete())
            {
                activeTasks.Remove(task);
                UpdateActiveTasks();

                return;
            }
        }
    }

    public bool TrainUnit(UnitController unit, TrainUnitTask task = null)
    {
        // check for available vehicle bays
        if(vehicleBays.Count == 0)
        {
            task.TaskStatus = "No vehicle bays in base";
            return false;
        }

        foreach(VehicleBay bay in vehicleBays)
        {
            if(!bay.IsTraining)
            {
                StartCoroutine(StartTraining(bay, unit, task));
                Debug.Log("Training Unit");

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
        //Debug.Log("Dispatching Units");
        UnitManager unitManager = GameObject.FindObjectOfType<UnitManager>();

        formationPositions = unitManager.GetFormationPositions(playerBase.position, unitGroup);

        if (formationPositions.Count < unitGroup.Count)
        {
            Debug.LogError("Not enough formations positions were created for the selected units");
            return;
        }

        // move all units to their designated target positions
        for (int i = 0; i < unitGroup.Count; i++)
        {
            unitGroup[i].GetComponent<UnitController>().
                ChangeState(UnitState.Moving, formationPositions[i]);
        }
        
    }

    public bool DispatchUnits(List<Transform> unitGroup)
    {
        UnitManager unitManager = GameObject.FindObjectOfType<UnitManager>();

        formationPositions = unitManager.GetFormationPositions(playerBase.position, unitGroup);

        if (formationPositions.Count < unitGroup.Count)
        {
            Debug.LogError("Not enough formations positions were created for the selected units");
            return false;
        }

        // move all units to their designated target positions
        for (int i = 0; i < unitGroup.Count; i++)
        {
            unitGroup[i].GetComponent<UnitController>().
                ChangeState(UnitState.Moving, formationPositions[i]);
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

    private IEnumerator StartTraining(VehicleBay vehicleBay, UnitController unit, TrainUnitTask task = null)
    {
        //Debug.Log("Training Unit");
        resources.AiSpendSteel(unit.Cost);
        steel = resources.aiResources[0].currentAmount;
        vehicleBay.IsTraining = true;
        task.UnitTrained = false;

        yield return new WaitForSeconds(unit.TimeToTrain);

        vehicleBay.IsTraining = false;        

        var unitInstance = Instantiate(unit, GetSpawnPosition(vehicleBay.transform), Quaternion.identity);
        unitInstance.body.forward = vehicleBay.transform.forward;

        //Debug.Log("Training Complete");
        aiUnits.Add(unitInstance);
        unitGroup.Add(unitInstance.transform);

        unitsTrained++;        

        if(unitsTrained % 5 == 0)
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