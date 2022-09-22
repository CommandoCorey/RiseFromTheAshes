using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiManager : MonoBehaviour
{
    public Transform vehicleBay;
    public int unitsBeforeAttacking = 10;
    public GameObject[] trainOdrer;
    public Transform playerBase;

    private ResourceManager resources;
    [SerializeField]
    private int steel = 0;
    private int unitNum = 0;
    private UnitController unit;

    private bool available = true;
    private bool dispatchedUnits = false;
    private int unitsTrained = 0;

    private float zOffset = 12;

    // unit management
    private List<Transform> unitGroup;
    private List<Vector3> formationPositions;

    // Start is called before the first frame update
    void Start()
    {
        resources = ResourceManager.Instance;
        unit = trainOdrer[unitNum].GetComponent<UnitController>();
        unitGroup = new List<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        steel = resources.aiResources[0].currentAmount;

        if (available && steel >= unit.Cost && unitsTrained < unitsBeforeAttacking)
        {
            //Debug.Log("Enough Steel has been gatherd");
            StartCoroutine(TrainUnit(unit));
            available = false;

            NextUnit();            
        }

        if (unitsTrained == unitsBeforeAttacking && !dispatchedUnits)
        {
            //Debug.Log("Enough units have been trained");
            DispatchUnits();
        }
    }


    private IEnumerator TrainUnit(UnitController unit)
    {
        //Debug.Log("Training Unit");
        resources.AiSpendSteel(unit.Cost);
        steel = resources.aiResources[0].currentAmount;

        yield return new WaitForSeconds(unit.TimeToTrain);
 
        var unitInstance = Instantiate(unit.gameObject, vehicleBay.position + GetSpawnOffset(), Quaternion.identity);

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

        unit = trainOdrer[unitNum].GetComponent<UnitController>();
    }

    private Vector3 GetSpawnOffset()
    {
        return new Vector3((unitsTrained % 5) * 4, 0, zOffset);
    }

    private void DispatchUnits()
    {
        Debug.Log("Dispatching Units");
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

        dispatchedUnits = true;
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
    }

}
