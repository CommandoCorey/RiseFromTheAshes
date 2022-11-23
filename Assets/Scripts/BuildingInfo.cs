using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingInfo : MonoBehaviour
{
    public GameObject InfoPanel;

    [Header("Individual Components")]
    public Image imageThunbnail;
    public TextMeshProUGUI buildingName;
    public TextMeshProUGUI buildingDescription;
    public ProgressBar buildProgressBar;
    public ProgressBar healthbar;    

    private Building building;

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

    // Add singlton
    public static BuildingInfo Instance { get; private set; }

    public void ShowPanel(Building building)
    {
        this.building = building;

        InfoPanel.SetActive(true);

        imageThunbnail.sprite = building.thumbnailImage;
        buildingName.text = building.buildingName;

        if (building.IsBuilt || building.StartAtMaxHP)
        {
            healthbar.gameObject.SetActive(true);
            buildProgressBar.gameObject.SetActive(false);
            buildingDescription.text = building.buildingDescription;
        }
        else
        {
            buildProgressBar.gameObject.SetActive(true);
            healthbar.gameObject.SetActive(false);            
            buildingDescription.text = "Under Construction";
        }
    }

    public void HidePanel()
    {
        InfoPanel.SetActive(false);
        building = null;
    }

    private void Update()
    {
        if (building == null || !InfoPanel.activeInHierarchy)
            return;

        if (building.IsBuilt)
        {
            buildingDescription.text = building.buildingDescription;
            healthbar.progress = building.HP;
            healthbar.maxValue = building.maxHP;

            if(buildProgressBar.gameObject.activeInHierarchy)
            {
                buildProgressBar.gameObject.SetActive(false);
                healthbar.gameObject.SetActive(true);

                if(building.tag == "VehicleBay")
                {
                    if (building.TryVehicleBayInteract())
                        HidePanel();
                }
            }
        }
        else
        {            
            buildProgressBar.progress = building.BuiltPerc;

            int percentage = (int) (building.BuiltPerc * 100);
            buildProgressBar.textString = percentage + " %";
        }

    }

}
