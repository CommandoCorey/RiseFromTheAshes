using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResourceBuilding : MonoBehaviour
{
    //public int ResourceToGain;
    public ResourceType resourceToAdd;
    public int quantityToAdd = 10;
    public float timePerIncerement = 1;
    public bool giveToAIPlayer = false;

    [Header("Floating Text Label")]
    public GameObject floatingLabel;

    private ResourceManager resources;
    private GameManager gameManager;
    private Building building;

    private bool wasPaused = false;
    private bool generating = false;

    // Start is called before the first frame update
    void Start()
    {
        resources = FindObjectOfType<ResourceManager>();
        gameManager = FindObjectOfType<GameManager>();
        building = GetComponent<Building>();

        if (gameObject.CompareTag("Headquarters"))
            building.IsBuilding = false;

        if(gameManager.State == GameState.Running && !building.IsBuilding)
            Invoke("IncrementResource", timePerIncerement);
    }

    // Update is called once per frame
    void Update()
    {        
        if(!wasPaused && gameManager.State == GameState.Paused)
            wasPaused = true;

        if(gameManager.State == GameState.Running && building.IsBuilt && !generating)
        {
            generating = true;
            Invoke("IncrementResource", timePerIncerement);
        }

        if(wasPaused && gameManager.State == GameState.Running && building.IsBuilt)
        {
            Invoke("IncrementResource", timePerIncerement);
            wasPaused = false;
        }
    }

    private void IncrementResource()
    {
        if (giveToAIPlayer)
            resources.AddResourceToAI(resourceToAdd, quantityToAdd);        
        else
            resources.AddResource(resourceToAdd, quantityToAdd);

        if (floatingLabel)
        {
            floatingLabel.SetActive(true);
            floatingLabel.GetComponent<FloatingResourceLabel>().Begin(quantityToAdd);
        }

        if (gameManager.State == GameState.Running)
            Invoke("IncrementResource", timePerIncerement);
    }

    private void HideLabel()
    {
        floatingLabel.SetActive(false);
    }

}
