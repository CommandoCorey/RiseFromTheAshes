using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("GUI Text")]
    public TextMeshProUGUI steelAmount;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < resources.Length; i++)
            resources[i].currentAmount = resources[i].startingAmount;
    }

    // Update is called once per frame
    void Update()
    {
        steelAmount.text = resources[0].currentAmount.ToString();
    }

    /// <summary>
    /// Gets the amount of resources from a specified resource type
    /// </summary>
    /// <param name="index">The location in the resources array</param>
    /// <returns>Integer value for the resource quantity</returns>
    public int GetResource(int index)
    {
        return resources[index].currentAmount;
    }

    /// <summary>
    /// Increases the resource quantity of given type by a specified amount
    /// </summary>
    /// <param name="type">the position in the resources array for the resource type</param>
    /// <param name="amount">The quantity to add to the given resource</param>
    public void AddResource(int type, int amount)
    {
        resources[type].currentAmount += amount;
    }

    /// <summary>
    /// Subtracts resources of specified type by a specified amount if the player currently has enough
    /// </summary>
    /// <param name="index">The location in the resources array</param>
    /// <param name="amount">The quantity of given resource type to spend</param>
    /// <returns>boolean value based on whether the player has enough of that resource type</returns>
    public bool SpendResource(int index, int amount)
    {
        if (resources[index].currentAmount <= amount)
        {
            resources[index].currentAmount -= amount;
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
        return resources[0].currentAmount;
    }

    /// <summary>
    /// Increases the quantity of units from the steel resource by a specified amount
    /// </summary>
    /// <param name="amount">The amount of units to add to the steel resource</param>
    public void AddSteel(int amount)
    {
        resources[0].currentAmount += amount;
    }

    /// <summary>
    /// Reduces the quantity of units from the steel resource by a specified amount
    /// </summary>
    /// <param name="amount">The amount of units to remove from the steel resource</param>
    /// <returns>boolean value based on whether the player has enough steel to spend</returns>
    public bool SpendSteel(int amount)
    {
        if (amount <= resources[0].currentAmount)
            resources[0].currentAmount -= amount;

        return false;
    }
    
}