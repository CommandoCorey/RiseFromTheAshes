using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainUnitButton : MonoBehaviour
{
    public string textLabel;
    public GameObject unitToTrain;
    public int steelCost;
    [Range(0, 300)]
    public float timeToTrain;

    public Transform building;
    public Vector3 spawnOffset = Vector3.forward * 3;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetBuilding(Transform building)
    {
        this.building = building;
    }

    public void TrainUnit()
    {
        StartCoroutine(BeginTraining());
    }

    private IEnumerator BeginTraining()
    {
        // update GUI to show progress bar


        yield return new WaitForSeconds(timeToTrain);

        Instantiate(unitToTrain, building.position + spawnOffset, Quaternion.identity);
    }
}
