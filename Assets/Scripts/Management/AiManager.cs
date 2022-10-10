using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiManager : MonoBehaviour
{
    public Transform vehicleBay;
    public UnitController[] trainOdrer;
    public Transform playerBase;
    [SerializeField] float timeBetweenWave = 3;
    [SerializeField] bool loopEnemyWaves;

    public EnemyWave[] enemyWaves;

    [Header("Buildings")]
    [SerializeField] Ghost[] placeholders;
    [SerializeField] List<BuildItem> buildItems;

    private ResourceManager resources;
    [SerializeField]
    private int steel = 0;
    private int unitNum = 0;
    private int waveNumber = 0;
    private UnitController unit;

    private bool available = true;
    private bool waveFinished = false;
    private int unitsTrained = 0;
   
    private float zOffset = 6;
    private float unitsPerRow = 5;
    private float spaceBetween = 4;

    // unit management
    private List<Transform> unitGroup;
    private List<Vector3> formationPositions;

    private GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        resources = ResourceManager.Instance;
        trainOdrer = enemyWaves[waveNumber].units;

        unit = trainOdrer[unitNum];
        unitGroup = new List<Transform>();

        gameManager = GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.State != GameState.Running)
            return;

        if (waveFinished)
            return;

        steel = resources.aiResources[0].currentAmount;

        if (unitsTrained < enemyWaves[waveNumber].units.Length)
        {
            if (available && steel >= unit.Cost)
            {
                //Debug.Log("Enough Steel has been gatherd");
                StartCoroutine(TrainUnit(unit));
                available = false;

                NextUnit();
            }
        }
        else
        {
            //Debug.Log("Enough units have been trained");
            DispatchUnits();                       
            
            unitsTrained = 0;
            unitNum = 0;
            zOffset = 6;            
        }
    }

    public void StartTrainTask(UnitController unit)
    {
        StartCoroutine(TrainUnit(unit));
    }

    public Ghost GetPlaceholder(int number)
    {
        if(number >-1 && number < placeholders.Length)
            return placeholders[number];

        return placeholders[0];
    }

    public void ConstructBuilding(Ghost ghostBuilding, Building buildItem)
    {
        Building building = Instantiate(buildItem, ghostBuilding.transform.position, ghostBuilding.transform.rotation);
        ghostBuilding.gameObject.SetActive(false);
        building.Build();
    }

    private IEnumerator TrainUnit(UnitController unit)
    {
        //Debug.Log("Training Unit");
        resources.AiSpendSteel(unit.Cost);
        steel = resources.aiResources[0].currentAmount;

        yield return new WaitForSeconds(unit.TimeToTrain);
 
        var unitInstance = Instantiate(unit.gameObject, GetSpawnPosition(), Quaternion.identity);
        unitInstance.GetComponent<UnitController>().body.forward = vehicleBay.forward;

        //Debug.Log("Training Complete");
        unitGroup.Add(unitInstance.transform);

        unitsTrained++;
        available = true;

        if(unitsTrained % 5 == 0)
        {
            zOffset += 6;
        }

    }

    private void NextUnit()
    {
        if (unitNum < trainOdrer.Length - 1)
            unitNum++;
        else
            unitNum = 0;

        unit = trainOdrer[unitNum];
    }

    private Vector3 GetSpawnPosition()
    {
        float x = vehicleBay.position.x + (unitsTrained % unitsPerRow * spaceBetween);
        float z = vehicleBay.position.z + vehicleBay.forward.z * zOffset;

        return new Vector3(x, 0, z);
    }

    private void DispatchUnits()
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

        Invoke("StartNextWave", timeBetweenWave);
    }

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

        if(vehicleBay != null)
            Gizmos.DrawLine(vehicleBay.position, vehicleBay.position + vehicleBay.forward * zOffset);
    }

}

[System.Serializable]
public struct EnemyWave
{
    public UnitController[] units;
}