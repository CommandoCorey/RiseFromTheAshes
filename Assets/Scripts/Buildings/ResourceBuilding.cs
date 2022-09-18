using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum ResourceNumber
{
    Steel
}


public class ResourceBuilding : Building
{
    //public int ResourceToGain;
    public ResourceNumber resourceToAdd;
    public int quantityToAdd = 10;
    public float timePerIncerement = 1;

    [Header("Floating Text Label")]
    public GameObject floatingLabel;

    ResourceManager resources;

    // Start is called before the first frame update
    void Start()
    {
        resources = FindObjectOfType<ResourceManager>();        

        Invoke("IncrementResource", timePerIncerement);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void IncrementResource()
    {
        resources.AddResource((int)resourceToAdd, quantityToAdd);
        floatingLabel.SetActive(true);
        floatingLabel.GetComponent<FloatingResourceLabel>().Begin(quantityToAdd);
        Invoke("IncrementResource", timePerIncerement);
    }

    private void HideLabel()
    {
        floatingLabel.SetActive(false);
    }

}
