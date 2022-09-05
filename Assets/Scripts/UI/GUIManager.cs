using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour
{
    public GameObject buttonPanel;
    public Transform unitPanal;
    public Transform statsPanel;

    public GameObject unitIconPrefab;

    private List<UnitController> selectedUnits;
    private List<GameObject> unitIcons;

    private bool moveMode = false;
    private bool attackMode = false;

    // Start is called before the first frame update
    void Start()
    {
        unitIcons = new List<GameObject>();
        selectedUnits = new List<UnitController>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateUnitHealth();

        if(Input.GetMouseButton(0))
        {
            if(moveMode)
            {

            }
            else if(attackMode)
            {

            }
        }
    }

    #region private functions
    private void UpdateUnitHealth()
    {
        if (selectedUnits != null)
        {
            for (int i = 0; i < selectedUnits.Count; i++)
            {
                if (selectedUnits[i] == null)
                {
                    GameObject.Destroy(unitIcons[i]);

                    selectedUnits.RemoveAt(i);
                    unitIcons.RemoveAt(i);
                    return;
                }

                TextMeshProUGUI[] healthText = unitIcons[i].GetComponentsInChildren<TextMeshProUGUI>();
                healthText[0].text = selectedUnits[i].CurrentHealth.ToString();

                var healthBar = unitIcons[i].GetComponentInChildren<ProgressBar>();
                healthBar.progress = selectedUnits[i].CurrentHealth / selectedUnits[i].MaxHealth;
            }

        }
    }

    // Switches to a single selection of a unit after an icon is clicked on

    #endregion

    #region public functions
    public void SetMoveMode()  {  moveMode = true;  }

    public void SetAttackMode()  {   attackMode = true; }

    /// <summary>
    /// populates the unit panel with unit icons alongs with their health
    /// </summary>
    /// <param name="selection">The list of unit game objects to use when populating the panel</param>
    public void GenerateUnitIcons(List<GameObject> selection)
    {
        ClearUnitSelection();

        // generate new icons
        for (int i=0; i < selection.Count; i++)
        {
            unitIcons.Add(Instantiate(unitIconPrefab, unitPanal));
            selectedUnits.Add(selection[i].GetComponent<UnitController>());
            
            TextMeshProUGUI[] healthText = unitIcons[i].GetComponentsInChildren<TextMeshProUGUI>();

            healthText[2].text = selectedUnits[i].MaxHealth.ToString();
            healthText[0].text = selectedUnits[i].CurrentHealth.ToString();

            unitIcons[i].GetComponentInChildren<Image>().sprite = selectedUnits[i].GuiIcon;

            var healthBar = unitIcons[i].GetComponentInChildren<ProgressBar>();
            healthBar.progress = selectedUnits[i].CurrentHealth / selectedUnits[i].MaxHealth;

            unitIcons[i].GetComponent<UnitIconButton>().IconIndex = i;
        }

        buttonPanel.SetActive(true);
    }

    /// <summary>
    /// Updates the GUI to only show one selected unit
    /// </summary>
    /// <param name="index">The number of the unit button being clicked on</param>
    public void SelectSingleUnit(int index)
    {
        var unit = selectedUnits[index];

        ClearUnitSelection();

        // select the matching unit
        selectedUnits.Add(unit);
        unit.SetSelected(true);

        // intantiate a new unit icon
        var unitIcon = Instantiate(unitIconPrefab, unitPanal);
        unitIcons.Add(unitIcon);

        // set the icon and health
        unitIcon.GetComponentInChildren<Image>().sprite = unit.GuiIcon;

        TextMeshProUGUI[] healthText = unitIcon.GetComponentsInChildren<TextMeshProUGUI>();
        healthText[2].text = unit.MaxHealth.ToString();
        healthText[0].text = unit.CurrentHealth.ToString();
        
        var healthBar = unit.GetComponentInChildren<ProgressBar>();
        healthBar.progress = unit.CurrentHealth / unit.MaxHealth;

        // update the unit manager
        var unitManager = GameObject.FindObjectOfType<UnitManager>();
        unitManager.SetSelectedUnit(unit.gameObject);
    }

    public void DisplayUnitStats()
    {

    }


    /// <summary>
    /// Removes all unit information from the GUI and clears the lists
    /// </summary>
    public void ClearUnitSelection()
    {
        // Destroy existing icons on GUI
        foreach (var icon in unitIcons)
        {
            GameObject.Destroy(icon);
        }

        // clear the lists
        unitIcons.Clear();
        selectedUnits.Clear();

        buttonPanel.SetActive(true);
    }
    #endregion

}