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
    public LayerMask enemyLayers;
    public TextMeshProUGUI alertMessage;

    private List<UnitController> selectedUnits;
    private List<GameObject> unitIcons;

    private UnitManager unitManager;
    private SelectionManager selectionManager;

    // properties
    public bool MoveClicked { get; set; } = false;
    public bool AttackClicked { get; set; } = false;
    public bool HaltClicked { get; set; } = false;

    // Start is called before the first frame update
    void Start()
    {
        unitIcons = new List<GameObject>();
        selectedUnits = new List<UnitController>();
        unitManager = GameObject.FindObjectOfType<UnitManager>();
        selectionManager = GameObject.FindObjectOfType<SelectionManager>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateUnitHealth();

        if(Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            RaycastHit hitInfo;

            if(MoveClicked && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                //Debug.Log("Clicked");
                unitManager.MoveUnits(hitInfo);

                MoveClicked = false;
            }
            else if(AttackClicked && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                // check if the layer is one of the layers in the layermask
                if (enemyLayers == (enemyLayers | (1 << hitInfo.transform.gameObject.layer)))
                {
                    //Debug.Log("You chose to attack " + hitInfo.transform.name);
                    StartCoroutine(ShowAlert("You chose to attack " + hitInfo.transform.name, 2));
                }
                else
                {
                    StartCoroutine(ShowAlert("That's not a valid attack target", 2));
                    //Debug.Log("That's not a valid attack target");
                }

                AttackClicked = false;
            }
            else
            {
                selectionManager.enabled = true;
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

    private IEnumerator ShowAlert(string message, int seconds)
    {
        alertMessage.text = message;
        alertMessage.gameObject.SetActive(true);

        yield return new WaitForSeconds(seconds);

        alertMessage.gameObject.SetActive(false);
    }

    #endregion

    #region public functions
    public void SetMoveMode() 
    { 
        MoveClicked = true;
        selectionManager.enabled = false;
    }

    public void SetAttackMode() { 
        AttackClicked = true;
        selectionManager.enabled = false;
    }

    public void SetHaltClicked()
    {
        HaltClicked = true;
    }

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

        buttonPanel.SetActive(true);

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

        buttonPanel.SetActive(false);
    }
    #endregion

}