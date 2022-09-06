using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitGui : MonoBehaviour
{
    public enum ActionChosen
    {
        Null, Move, Attack, Halt
    }

    public GameObject buttonPanel;
    public Transform unitPanal;
    public GameObject unitInfoPanel;
    public GameObject unitIconPrefab;
    public LayerMask enemyLayers;
    public TextMeshProUGUI alertMessage;

    [Header("Unit Stats")]    
    public Image thumbnail;
    public TextMeshProUGUI unitName;
    public TextMeshProUGUI currentHealth;
    public TextMeshProUGUI maxHealth;
    public TextMeshProUGUI range;
    public TextMeshProUGUI damagePerSecond;
    public TextMeshProUGUI movementSpeed;    

    // private variables
    private List<UnitController> selectedUnits;
    private List<GameObject> unitIcons;

    private UnitManager unitManager;
    private SelectionManager selectionManager;

    // properties
    public ActionChosen ButtonClicked { get; set; } = ActionChosen.Null;

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

        // check if mouse clicks on environment while an action is chosen
        if(Input.GetMouseButtonUp(0) && ButtonClicked!= ActionChosen.Null && !EventSystem.current.IsPointerOverGameObject())
        {
            RaycastHit hitInfo;

            if(ButtonClicked == ActionChosen.Move && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                //Debug.Log("Clicked");
                unitManager.MoveUnits(hitInfo);

                ButtonClicked = ActionChosen.Null;
            }
            else if(ButtonClicked == ActionChosen.Attack && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                bool attackSuccess = unitManager.AttackTarget(hitInfo.transform);

                if(!attackSuccess)
                {
                    StartCoroutine(ShowAlert("That's not a valid attack target", 2));
                }

                ButtonClicked = ActionChosen.Null;
            }
            else if(ButtonClicked == ActionChosen.Halt)
            {
                unitManager.HaltUnitSelection();
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

                if (unitIcons.Count > 0)
                {
                    TextMeshProUGUI[] healthText = unitIcons[i].GetComponentsInChildren<TextMeshProUGUI>();
                    healthText[0].text = selectedUnits[i].CurrentHealth.ToString();

                    var healthBar = unitIcons[i].GetComponentInChildren<ProgressBar>();
                    healthBar.progress = selectedUnits[i].CurrentHealth / selectedUnits[i].MaxHealth;
                }
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
        ButtonClicked = ActionChosen.Move;
        selectionManager.enabled = false;
    }

    public void SetAttackMode() {
        ButtonClicked = ActionChosen.Attack;
        selectionManager.enabled = false;
    }

    public void SetHaltClicked()
    {
        ButtonClicked = ActionChosen.Halt;
        selectionManager.enabled = false;
    }

    /// <summary>
    /// populates the unit panel with unit icons alongs with their health
    /// </summary>
    /// <param name="selection">The list of unit game objects to use when populating the panel</param>
    public void GenerateUnitIcons(List<GameObject> selection)
    {
        ClearUnitSelection();

        // if only one unit is selcted display the unit stats/info instead
        if(selection.Count == 1)
        {
            selectedUnits.Add(selection[0].GetComponent<UnitController>());
            SelectSingleUnit(0);
            return;
        }

        if(selection.Count > 0)
            buttonPanel.SetActive(true);

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

        /*
        // intantiate a new unit icon
        var unitIcon = Instantiate(unitIconPrefab, unitPanal);
        unitIcons.Add(unitIcon);

        // set the icon and health
        unitIcon.GetComponentInChildren<Image>().sprite = unit.GuiIcon;

        TextMeshProUGUI[] healthText = unitIcon.GetComponentsInChildren<TextMeshProUGUI>();
        healthText[2].text = unit.MaxHealth.ToString();
        healthText[0].text = unit.CurrentHealth.ToString();
        
        var healthBar = unit.GetComponentInChildren<ProgressBar>();
        healthBar.progress = unit.CurrentHealth / unit.MaxHealth;*/

        DisplayUnitStats(unit);

        buttonPanel.SetActive(true);

        // update the unit manager
        var unitManager = GameObject.FindObjectOfType<UnitManager>();
        unitManager.SetSelectedUnit(unit.gameObject);
    }

    /// <summary>
    /// Displays the name and stats for one particular type of unit on the GUI
    /// </summary>
    /// <param name="unit">The UnitController Script on the slected unit</param>
    public void DisplayUnitStats(UnitController unit)
    {
        unitInfoPanel.SetActive(true);

        thumbnail.sprite = unit.GuiIcon;
        unitName.text = unit.Name;
        currentHealth.text = unit.CurrentHealth.ToString();
        maxHealth.text = unit.MaxHealth.ToString();
        range.text = unit.AttackRange.ToString();
        damagePerSecond.text = (unit.DamagePerHit / unit.AttackRate).ToString();
        movementSpeed.text = unit.Speed.ToString();
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