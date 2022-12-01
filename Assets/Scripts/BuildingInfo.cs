using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingInfo : MonoBehaviour
{
    public GameObject infoPanel;

    [Header("Individual Components")]
    public Image imageThunbnail;
    public TextMeshProUGUI buildingName;
    public TextMeshProUGUI buildingDescription;
    public ProgressBar buildProgressBar;
    public ProgressBar healthbar;    

    private Building building;
    private VehicleBay bayUsed;

    private bool buildingUnit = false;

    public bool BuildingUnit { get => buildingUnit; }

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

    public void ShowBuildingPanel(Building building)
    {
        this.building = building;

        SelectionManager.Instance.SetPanelTooltip(false);
        BuildMenu.Instance.gameObject.SetActive(false);

        infoPanel.SetActive(true);

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

        buildingUnit = false;
    }

    public void ShowUnitBeingBuilt(VehicleBay bay, UnitController unit)
    {
        bayUsed = bay;

        infoPanel.SetActive(true);

        imageThunbnail.sprite = unit.guiIcon;
        buildingName.text = unit.Name;
        buildingDescription.text = "Under Construction";

        buildingUnit = true;

        buildProgressBar.gameObject.SetActive(true);
        healthbar.gameObject.SetActive(false);
    }

    public void HidePanel()
    {
        infoPanel.SetActive(false);
        building = null;
        buildingUnit = false;
    }
    private void Update()
    {
        if (!infoPanel.activeInHierarchy)
            return;

        // used fo vehicle bays
        if(buildingUnit)
        {
            buildProgressBar.progress = bayUsed.buildTimer;

            int percentage = (int)(bayUsed.buildTimer * 100);
            buildProgressBar.textString = percentage + " %";

        }
        else if (building == null)
            return;

        else if (building.IsBuilt || building.StartAtMaxHP) // construction is finished
        {
            buildingDescription.text = building.buildingDescription;

            // update healthbar
            healthbar.maxValue = building.maxHP;
            healthbar.progress = building.HPPerc;

            //healthbar.SetProgress(building.HP, building.maxHP);

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
        else // building under construction
        {            
            buildProgressBar.progress = building.BuiltPerc;

            int percentage = (int) (building.BuiltPerc * 100);
            buildProgressBar.textString = percentage + " %";
        }

    }

}
