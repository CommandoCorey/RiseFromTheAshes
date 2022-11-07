using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block_Gen : MonoBehaviour
{
    //Random Spawn - Gameobjects;
    public List<GameObject> ObjectsToSpawn = new List<GameObject>();
    public bool isRandom;

    [Space(10)]
    // if its hard
    //[TextArea] 
    //public string Notes = "Test.";

    //Single Spawn - Gameobjects(WIP);
    public float timer;
    private float currentTimer;
    public GameObject objectToSpawn;
    public bool isTimer;

    //-------------------------------------------------------------------------------------------------
    // Spawning the Object / Random Spawn
    void Start()
    {
        // for random Blocks Click the is random Buttom in the inspector 
        // for single use unclick it and make the thing you want to spawn the first or only one
        int index = isRandom ? Random.Range(0, ObjectsToSpawn.Count) : 0;

        if (ObjectsToSpawn.Count > 0)
        {
            Instantiate(ObjectsToSpawn[index], transform.position, transform.rotation);
        }
        // (Bug/Issues)
        // - don't add using the inspector just drag and drop it into the inspector
        // - make sure the prefabs parent is set to x-0 y-0 z-0


        {
            currentTimer = timer;
        }
    }


    //-------------------------------------------------------------------------------------------------
    // Timer (for UI) (needs to spawn 2D image instead of Block)
    void Update()
    {
        if (isTimer)
        {
            UpdateTimer();
        }
    }

    void UpdateTimer()
    {
        if(currentTimer > 0)
        {
            currentTimer -= Time.deltaTime;
        }
        else
        {
            SpawnObject();
            currentTimer = timer;
        }
    }

    //-------------------------------------------------------------------------------------------------
    //spawn single
    public void SpawnObject()
    {
        Instantiate(objectToSpawn, transform.position, transform.rotation);
    }
    //-------------------------------------------------------------------------------------------------
}
