using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public struct RaidPath
{
    public Transform[] route;
}

public enum AiDifficulty
{
    Easy, Normal, Hard, VeryHard
}

public class AiPlayer : MonoBehaviour
{
    [Header("Difficulty Time Delays")]
    public static AiDifficulty Difficulty = AiDifficulty.Normal;
    [SerializeField] float easyDelay = 15;
    [SerializeField] float normalDelay = 10;
    [SerializeField] float hardDelay = 5;
    [SerializeField] float veryHardDelay = 0;

    public Transform AiHeadquarters;
    public Transform playerBase;
    public Transform rallyPoint;
    [SerializeField] [Range(0, 200)]
    float HQDefenseRadius = 80;

    [SerializeField] List<Transform> buildingPlaceholders;
    [SerializeField] Transform[] buildingPrefabs;

    public List<VehicleBay> vehicleBays;
    public RaidPath[] raidPaths;
    public RaidPath[] patrolRoutes;

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
    [HideInInspector]
    public List<UnitController> unitsAttackingHQ;
    private List<Transform> unitGroup;
    private List<Vector3> formationPositions;

    private List<Building> baysInConstruction = new List<Building>();

    private GameManager gameManager;
    private FormationManager formations;
    private AiTaskScheduler taskScheduler;

    private List<AiTask> activeTasks;
    private List<TaskSetDisplay> taskDisplays;

    // properties
    public Transform[] BuildingPrefabs { get => buildingPrefabs; }
    public List<Transform> BuildingPlaceholders { get => buildingPlaceholders; }
    public List<Building> BaysInConstruction { get => baysInConstruction; }
    public List<UnitController> UnitsAttackingHQ { get => unitsAttackingHQ; } 

    public float EASY_TIME_DELAY { get => easyDelay; }
    public float NORMAL_TIME_DELAY { get => normalDelay; }
    public float HARD_TIME_DELAY { get => hardDelay; }
    public float VERY_HARD_TIME_DELAY { get => veryHardDelay; }

    public bool PlaceHoldersLeft
    {
        get
        {
            if (GetNextPlaceholder() != null)
                return true;

            return false;
        }

        //get => buildingPlaceholders.Count > 0; 
    }

    //public bool OutpostPlaceholdersLeft { get => outpostPlaceholders.Count > 0; }

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
        // seed the unit random number generator by using the time
        Random.InitState((int)DateTime.Now.Ticks);

        resources = ResourceManager.Instance;
        formations = FormationManager.Instance;
        gameManager = GameManager.Instance;
        taskScheduler = GetComponent<AiTaskScheduler>();

        aiUnits = new List<UnitController>();
        unitGroup = new List<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.State != GameState.Running)
            return;

        UpdateUnitsAttackingHQ();
    }

    #region private functions
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
        //unitInstance.body.forward = vehicleBay.transform.forward; // look at this
        unitInstance.body.rotation = Quaternion.Euler(0, 180, 0);

        unitInstance.GetComponent<AiUnit>().Mode = task.mode; // sets the unit mode (Intermediate by default)

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

    private void SendUnitsToHQ()
    {
        var colliders = Physics.OverlapSphere(AiHeadquarters.position, HQDefenseRadius);

        foreach(var collider in colliders)
        {
            if(collider.gameObject.layer == 7)
            {
                var aiUnit = collider.GetComponent<AiUnit>();
                aiUnit.AttackClosestHQEnemy();
            }
        }
    }

    private void UpdateUnitsAttackingHQ()
    {
        foreach(UnitController unit in unitsAttackingHQ)
        {
            if(unit == null || // unit has been destroyed
                !unit.GetComponent<AttackState>()) // unit is not in attack state
            {
                unitsAttackingHQ.Remove(unit);
                UpdateUnitsAttackingHQ();
                return;
            }

        }
    }
    #endregion

    #region public functions
    public void AddVehicleBay(VehicleBay bay)
    {
        vehicleBays.Add(bay);
    }

    public void AddUnitAttackingHQ(UnitController unit)
    {
        if (!unitsAttackingHQ.Contains(unit))
        {
            unitsAttackingHQ.Add(unit);
            SendUnitsToHQ();
        }
    }

    /*
    /// <summary>
    /// Sends a group of units to a specified outpost
    /// </summary>
    /// <param name="units"></param>
    /// <param name="number">The index in the list of outposts to use</param>
    public void SendToOutpost(List<Transform> units, int number)
    {
        foreach (Transform t in units)
        {
            var unit = t.GetComponent<UnitController>();
            unit.ChangeState(UnitState.Moving, outpostPlaceholders[number].position);
        }
    }*/

    /// <summary>
    /// Sends all units in a specified list along a random patrol route 
    /// </summary>
    /// <param name="units">list of transforms containing the UnitController</param>
    public void SendOnPatrol(List<Transform> units)
    {
        foreach (Transform t in units)
        {
            var unit = t.GetComponent<UnitController>();
            unit.ChangeState(UnitState.Patrol);

            // select random route            
            int random = Random.Range(0, patrolRoutes.Length - 1);

            var patrolState = unit.GetComponent<FollowPathState>();
            patrolState.SetRoute(patrolRoutes[random].route);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="units"></param>
    /// <param name="pathNumber"></param>
    public void SendOnPatrol(List<Transform> units, int pathNumber)
    {
        foreach (Transform t in units)
        {
            var unit = t.GetComponent<UnitController>();
            unit.ChangeState(UnitState.Patrol);

            var patrolState = unit.GetComponent<FollowPathState>();
            patrolState.LoopPath = true;
            patrolState.SetRoute(patrolRoutes[pathNumber].route);
        }
    }

    public void SendAlongPath(List<Transform> units, bool patrolling)
    {
        // select random route
        Random.InitState((int)DateTime.Now.Ticks);
        int random = Random.Range(0, raidPaths.Length - 1);

        foreach (Transform t in units)
        {
            var unit = t.GetComponent<UnitController>();
            unit.ChangeState(UnitState.Patrol);

            // mark the unit as being in a wave
            var aiUnit = t.GetComponent<AiUnit>();
            aiUnit.IsInWave = true;

            var followPathState = unit.GetComponent<FollowPathState>();
            followPathState.LoopPath = patrolling;
            followPathState.SetRoute(raidPaths[random].route);
        }
    }

    /*
    /// <summary>
    /// Add outpost tests
    /// </summary>
    /// <param name="outpostPlaceholder"></param>
    public void AddOutpost(Transform outpostPlaceholder)
    {

    }*/

    /// <summary>
    /// Trains a specified unit from a vehicle bay
    /// </summary>
    /// <param name="unit">The type of unit to be trained</param>
    /// <param name="task">The TrainUnitTask scriptable object</param>
    /// <returns></returns>
    public bool TrainUnit(UnitController unit, TrainUnitTask task = null)
    {
        // check for available vehicle bays
        if (vehicleBays.Count == 0)
        {
            task.TaskStatus = "No vehicle bays in base";
            return false;
        }

        if (gameManager.UnitCountAi >= gameManager.MaxUnitsAi)
        {
            task.TaskStatus = "Not enough space for more units";
            return false;
        }

        foreach (VehicleBay bay in vehicleBays)
        {
            if (!bay.IsTraining)
            {
                StartCoroutine(StartTraining(bay, unit, task));
                //Debug.Log("Training Unit");

                return true;
            }

        }

        task.TaskStatus = "All vehicle bays are busy";
        return false;
    }

    public void ConstructBuilding(Transform ghostBuilding, Building buildItem, BuildTask task = null, TaskSet set = null)
    {
        resources.AiSpendSteel(buildItem.steelCost);

        // create the building
        Building building = Instantiate(buildItem, ghostBuilding.position, ghostBuilding.rotation);
        building.ghostTransform = ghostBuilding;
        building.Build();

        ghostBuilding.gameObject.SetActive(false);

        if (building.gameObject.CompareTag("VehicleBay") && building.GetComponent<VehicleBay>())
            baysInConstruction.Add(building);

        ghostBuilding.gameObject.SetActive(false);

        //buildingPlaceholders.Remove(ghostBuilding);

        task.SetBuildingInstance(building);

        if(set != null && set.addRebuildTasks)
        {
            set.tasks.Remove(task);
        }
    }

    /// <summary>
    /// Finds an AI ghost building that is currently set to active
    /// </summary>
    /// <returns>The first transform found is active</returns>
    public Transform GetNextPlaceholder()
    {
        foreach (Transform placeholder in buildingPlaceholders)
        {
            if (placeholder.gameObject.activeInHierarchy)
                return placeholder;
        }

        return null;
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
        if (number > -1 && number < buildingPlaceholders.Count)
            return buildingPlaceholders[number];

        return buildingPlaceholders[0];
    }

    /*
    public Transform GetOutpostPlaceholder(int number)
    {
        if (number > -1 && number < outpostPlaceholders.Count)
            return buildingPlaceholders[number];

        return buildingPlaceholders[0];
    }

    public bool HasOutpostGhost(Transform transform)
    {
        return outpostPlaceholders.Contains(transform);
    }*/

    public Transform[] GetIdleUnits()
    {
        List<Transform> idleUnits = new List<Transform>();

        foreach (var unit in aiUnits)
        {
            if (unit.State == UnitState.Idle)
                idleUnits.Add(unit.transform);
        }

        return idleUnits.ToArray();
    }
    #endregion

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        // draws the fromation positions that each unit will finish at
        if (formationPositions != null)
        {
            foreach (Vector3 position in formationPositions)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(position, 1);
            }
        }

        if (AiHeadquarters != null)
        {
            UnityEditor.Handles.color = Color.green;
            UnityEditor.Handles.DrawWireDisc(AiHeadquarters.position, Vector3.up, HQDefenseRadius);
        }

        //if(vehicleBay != null)
        //Gizmos.DrawLine(vehicleBay.position, vehicleBay.position + vehicleBay.forward * zOffset);

#endif
    }

}