using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResourceBuilding : MonoBehaviour
{
    [SerializeField] ResourceType resourceToAdd;
    [SerializeField] int maxQuantityIncrease = 0;
    [SerializeField] public int quantityToAdd = 10;
    [SerializeField] public float timePerIncerement = 1;
    [SerializeField] public bool giveToAIPlayer = false;

    [Header("Floating Text Label")]
    public GameObject floatingLabel;

    private ResourceManager resources;
    private GameManager gameManager;
    private Building building;

    private bool wasPaused = false;
    private bool generating = false;

    private int currentAmount, maxAmount;

    // Start is called before the first frame update
    void Start()
    {
        resources = FindObjectOfType<ResourceManager>();
        gameManager = FindObjectOfType<GameManager>();
        building = GetComponent<Building>();

        //if (gameObject.CompareTag("Headquarters"))
        //building.IsBuilding = false;

        if (gameManager.State == GameState.Running && !building.IsBuilding)
        {
            Invoke("IncrementResource", timePerIncerement);
            generating = true;
        }

        resources.IncreaseResourceMax(resourceToAdd, maxQuantityIncrease, giveToAIPlayer);
    }

    // Update is called once per frame
    void Update()
    {
        currentAmount = resources.GetResource(resourceToAdd, giveToAIPlayer);
        maxAmount = resources.GetResourceMax(resourceToAdd, giveToAIPlayer);

        if (currentAmount >= maxAmount)
            return;

        if(!wasPaused && gameManager.State == GameState.Paused)
            wasPaused = true;

        if(gameManager.State == GameState.Running && !generating)
        {
            if (building.IsBuilt || gameObject.CompareTag("Headquarters"))
            {
                generating = true;
                Invoke("IncrementResource", timePerIncerement);
            }
        }

        if(wasPaused && gameManager.State == GameState.Running)
        {
            if (building.IsBuilt || gameObject.CompareTag("Headquarters"))
            {
                Invoke("IncrementResource", timePerIncerement);
                wasPaused = false;
            }
        }
    }

    private void IncrementResource()
    {
        if (giveToAIPlayer)
            resources.AddResourceToAI(resourceToAdd, quantityToAdd);        
        else
            resources.AddResource(resourceToAdd, quantityToAdd);

        if (floatingLabel && gameObject.layer == 8) // player building layer
        {
            floatingLabel.SetActive(true);
            floatingLabel.GetComponent<FloatingResourceLabel>().Begin(quantityToAdd, timePerIncerement);
        }

        currentAmount = resources.GetResource(resourceToAdd, giveToAIPlayer);
        maxAmount = resources.GetResourceMax(resourceToAdd, giveToAIPlayer);

        // if reosources is not at maximum generate again
        if (gameManager.State == GameState.Running && currentAmount < maxAmount)
        {
            Invoke("IncrementResource", timePerIncerement);
        }
        else
        {
            generating = false;
        }
    }

    private void HideLabel()
    {
        floatingLabel.SetActive(false);
    }

}
