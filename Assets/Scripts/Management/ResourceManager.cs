using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ResourceType
{
    Steel = 0
}

[System.Serializable]
public struct Resource
{
    public string name;
    public Image icon;
    public int startingAmount;

    [HideInInspector]
    public int currentAmount;
}

public class ResourceManager : MonoBehaviour
{
    public Resource[] resources;
    public Resource[] aiResources;

    [Header("GUI Text")]
    public TextMeshProUGUI steelAmount;

    static public ResourceManager Instance { get; set; }

	private void Awake()
	{
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

	// Start is called before the first frame update
	void Start()
    {
        for (int i = 0; i < resources.Length; i++)
            resources[i].currentAmount = resources[i].startingAmount;
    }

    // Update is called once per frame
    void Update()
    {
      //  steelAmount.text = resources[0].currentAmount.ToString();
    }

    /// <summary>
    /// Gets the amount of resources from a specified resource type
    /// </summary>
    /// <param name="index">The location in the resources array</param>
    /// <returns>Integer value for the resource quantity</returns>
    public int GetResource(ResourceType type)
    {
        return resources[(int)type].currentAmount;
    }

    /// <summary>
    /// Increases the resource quantity of given type by a specified amount
    /// </summary>
    /// <param name="type">the position in the resources array for the resource type</param>
    /// <param name="amount">The quantity to add to the given resource</param>
    public void AddResource(ResourceType type, int amount)
    {
        resources[(int)type].currentAmount += amount;
    }

    public void AddResourceToAI(ResourceType type, int amount)
    {
        aiResources[(int)type].currentAmount += amount;
    }

    /// <summary>
    /// Subtracts resources of specified type by a specified amount if the player currently has enough
    /// </summary>
    /// <param name="index">The location in the resources array</param>
    /// <param name="amount">The quantity of given resource type to spend</param>
    /// <returns>boolean value based on whether the player has enough of that resource type</returns>
    public bool SpendResource(ResourceType type, int amount)
    {
        if (resources[(int)type].currentAmount <= amount)
        {
            resources[(int)type].currentAmount -= amount;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns the current amount of steel the player has
    /// </summary>
    /// <returns>First index of the resources array</returns>
    public int GetSteel()
    {
        return GetResource(ResourceType.Steel);
    }

    /// <summary>
    /// Increases the quantity of units from the steel resource by a specified amount
    /// </summary>
    /// <param name="amount">The amount of units to add to the steel resource</param>
    public void AddSteel(int amount)
    {
        AddResource(ResourceType.Steel, amount);
    }

    /// <summary>
    /// Reduces the quantity of units from the steel resource by a specified amount
    /// </summary>
    /// <param name="amount">The amount of units to remove from the steel resource</param>
    /// <returns>boolean value based on whether the player has enough steel to spend</returns>
    public bool SpendSteel(int amount)
    {
        return SpendResource(ResourceType.Steel, amount);
    }

    public bool AiSpendSteel(int amount)
    {
        if (aiResources[0].currentAmount >= amount)
        {
            aiResources[0].currentAmount -= amount;
            return true;
        }

        return false;
    }
    
}