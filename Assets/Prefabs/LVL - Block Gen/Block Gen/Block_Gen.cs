using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block_Gen : MonoBehaviour
{
    //Random Spawn - Gameobjects;
    public List<GameObject> ObjectsToSpawn = new List<GameObject>();
    public bool isRandom;

    [Space(10)]
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
        // (Bug) don't add using the inspector just drag and drop it into the inspector


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

    //         CHECKS FOR QA
    // when running spawns the object
    // spawns it in the empty game object
    // no issues occurred 
    // stays inside the game object

    //        EXTRA FEATURES
    // spawn an object when the game start and spawn in a location (done)
    // spawn a random object type of the building (done)
    // minecraft village or Dungeon Connect Gen block connection (In Progress)
    // spawn by type Variation (In Progress)
    // Random Images for the splash art (50% - spawn image type and not Double up need to swap)
    // makes a navmesh (WIP)
    // applys the layers (WIP)
    // Can be used to Spawn Enemies from Alleyways (WIP GUI BETA)
    // Generate Level (50% - need to make 2 scripts for it)
    // Custom Map Size using one scene (WIP - Need GUI - BETA)
    // spawn furniture using the same prosess (WIP - needs to be child - Need Props - BETA)

    //              ASK (BETA)
    // ask paul about a box collider away from the tank for triggers
    // ask paul if he needs this for Enemy Spawning
    // ask jay if the random gen particle effects would work
}
