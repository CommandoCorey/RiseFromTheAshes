using System;
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
    public Sprite icon;
    public int startingAmount;
    public int maxAmount;

    [HideInInspector]
    public int currentAmount;
}

public class ResourceManager : MonoBehaviour
{
    public Resource[] playerResources;
    public Resource[] aiResources;

    [Header("GUI Text")]
    public TextMeshProUGUI steelAmount;
    public TextMeshProUGUI maxSteelAmount;

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
        for (int i = 0; i < playerResources.Length; i++)
            playerResources[i].currentAmount = playerResources[i].startingAmount;
    }

    // Update is called once per frame
    void Update()
    {
        if(steelAmount && playerResources.Length > 0)
            steelAmount.text = playerResources[0].currentAmount.ToString();

        if(maxSteelAmount && playerResources.Length > 0)
            maxSteelAmount.text = playerResources[0].maxAmount.ToString();
    }   

    /// <summary>
    /// Rets the amount of resources from a specified resource type
    /// </summary>
    /// <param name="index">The location in the resources array</param>
    /// <returns>The resource quantity</returns>
    public int GetResource(ResourceType type, bool aiPlayer = false)
    {
        if (aiPlayer)
            return aiResources[(int)type].currentAmount;
        else
            return playerResources[(int)type].currentAmount;
    }

    /// <summary>
    /// Returns the current maximum amount of a resource player has
    /// </summary>
    /// <param name="type">The type of resource being used (Enumerator)</param>
    /// <param name="aiPlayer">Determins it it is the human player's or ai player's resources</param>
    /// <returns>The maximum resource qauntity</returns>
    public int GetResourceMax(ResourceType type, bool aiPlayer = false)
    {
        if(aiPlayer)
            return aiResources[(int)type].maxAmount;
        else
            return playerResources[(int)type].maxAmount;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="amount"></param>
    /// <param name="aiPlayer"></param>
    public void IncreaseResourceMaximum(ResourceType type, int amount, bool aiPlayer = false)
    {
        if (aiPlayer)
            aiResources[(int)type].maxAmount += amount;
        else
            playerResources[(int)type].maxAmount += amount;
    }


    /// <summary>
    /// Increases the resource quantity of given type by a specified amount
    /// </summary>
    /// <param name="type">the position in the resources array for the resource type</param>
    /// <param name="amount">The quantity to add to the given resource</param>
    public void AddResource(ResourceType type, int amount)
    {
        playerResources[(int)type].currentAmount += amount;

        if (playerResources[(int)type].currentAmount > playerResources[(int)type].maxAmount)
            playerResources[(int)type].currentAmount = playerResources[(int)type].maxAmount;
    }

    /// <summary>
    /// Increases the A.I. player's resource quantity of given type by a specified amount
    /// </summary>
    /// <param name="type">the position in the resources array for the resource type</param>
    /// <param name="amount">The quantity to add to the given resource</param>
    public void AddResourceToAI(ResourceType type, int amount)
    {
        aiResources[(int)type].currentAmount += amount;

        if (aiResources[(int)type].currentAmount > aiResources[(int)type].maxAmount)
            aiResources[(int)type].currentAmount = aiResources[(int)type].maxAmount;
    }

    /// <summary>
    /// Subtracts resources of specified type by a specified amount if the player currently has enough
    /// </summary>
    /// <param name="index">The location in the resources array</param>
    /// <param name="amount">The quantity of given resource type to spend</param>
    /// <returns>boolean value based on whether the player has enough of that resource type</returns>
    public bool SpendResource(ResourceType type, int amount, bool aiPlayer = false)
    {
        if (aiPlayer)
        {
            if (aiResources[(int)type].currentAmount >= amount)
            {
                aiResources[(int)type].currentAmount -= amount;
                return true;
            }
        }
        else
        {
            if (playerResources[(int)type].currentAmount >= amount)
            {
                playerResources[(int)type].currentAmount -= amount;
                return true;
            }
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
    /// Increases the maximim capacity of a specified resource
    /// </summary>
    /// <param name="type">steel amount</param>
    /// <param name="amount">increase in the resource quantity</param>
    /// <param name="aiPlayer"></param>
    public void IncreaseResourceMax(ResourceType type, int amount, bool aiPlayer = false)
    {
        if(aiPlayer)
            aiResources[(int)type].maxAmount += amount;
        else
            playerResources[(int) type].maxAmount += amount;
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
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
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