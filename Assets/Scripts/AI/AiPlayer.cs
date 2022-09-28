using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiPlayer : MonoBehaviour
{
    public Transform playerBase;
    [SerializeField] List<Ghost> placeholders;

    [SerializeField] bool loopTasks;
    public AiTask[] tasksSchedule;

    public List<VehicleBay> vehicleBays;    

    private ResourceManager resources;
    [SerializeField]
    private int steel = 0;
    private int taskNum = 0;
    private UnitController unit;

    private bool available = true;
    private bool waveFinished = false;
    private int unitsTrained = 0;
    private bool readyToPerform = true;

    private float zOffset = 6;
    private float unitsPerRow = 5;
    private float spaceBetween = 4;    

    // unit management
    private List<UnitController> aiUnits;
    private List<Transform> unitGroup;
    private List<Vector3> formationPositions;

    private GameManager gameManager;

    public bool PlaceHoldersLeft { get => placeholders.Count > 0; }

    // Start is called before the first frame update
    void Start()
    {
        resources = ResourceManager.Instance;
        aiUnits = new List<UnitController>();
        unitGroup = new List<Transform>();

        gameManager = GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.State != GameState.Running)
            return;
        
        if (!loopTasks && taskNum >= tasksSchedule.Length)
            return;

        steel = resources.aiResources[0].currentAmount;

        if(steel >= tasksSchedule[taskNum].GetSteelCost() && readyToPerform)
        {
            Invoke("PerformNextTask", tasksSchedule[taskNum].timeDelay);
            readyToPerform = false;
        }
        
    }

    private void PerformNextTask()
    {
        if (tasksSchedule[taskNum].PerformTask()) // attempt to perform the task
            taskNum++;

        readyToPerform = true;
    }

    public bool TrainUnit(UnitController unit)
    {
        // check for available vehicle bays
        foreach(VehicleBay bay in vehicleBays)
        {
            if(!bay.IsBuilding)
            {
                StartCoroutine(StartTraining(bay, unit));
                Debug.Log("Training Unit");

                return true;
            }
        }

        return false;
    }

    public void ConstructBuilding(Ghost ghostBuilding, Building buildItem)
    {
        Building building = Instantiate(buildItem, ghostBuilding.transform.position, ghostBuilding.transform.rotation);
        ghostBuilding.gameObject.SetActive(false);
        building.Build();
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

        waveFinished = true;

        //Invoke("StartNextWave", timeBetweenWave);
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

    public Ghost GetPlaceholder(int number)
    {
        if(number >-1 && number < placeholders.Count)
            return placeholders[number];

        return placeholders[0];
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

    private IEnumerator StartTraining(VehicleBay vehicleBay, UnitController unit)
    {
        //Debug.Log("Training Unit");
        resources.AiSpendSteel(unit.Cost);
        steel = resources.aiResources[0].currentAmount;

        yield return new WaitForSeconds(unit.TimeToTrain);
 
        var unitInstance = Instantiate(unit, GetSpawnPosition(vehicleBay.transform), Quaternion.identity);
        unitInstance.body.forward = vehicleBay.transform.forward;

        //Debug.Log("Training Complete");
        aiUnits.Add(unitInstance);
        unitGroup.Add(unitInstance.transform);

        unitsTrained++;
        available = true;

        if(unitsTrained % 5 == 0)
        {
            zOffset += 6;
        }

    }

    /*
    private void NextUnit()
    {
        if (unitNum < trainOdrer.Length - 1)
            unitNum++;
        else
            unitNum = 0;

        unit = trainOdrer[unitNum];
    }*/

    private Vector3 GetSpawnPosition(Transform vehicleBay)
    {
        float x = vehicleBay.position.x + (unitsTrained % unitsPerRow * spaceBetween);
        float z = vehicleBay.position.z + vehicleBay.forward.z * zOffset;

        return new Vector3(x, 0, z);
    }    

    /*
    private void StartNextWave()
    {
        if (waveNumber < enemyWaves.Length - 1)
            waveNumber++;
        else
        {
            waveNumber = 0;

            if (!loopEnemyWaves)
                return;
        }

        waveFinished = false;

        trainOdrer = enemyWaves[waveNumber].units;
        unit = trainOdrer[unitNum];
    }    */

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