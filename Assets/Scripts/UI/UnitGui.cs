using System;
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
        Null, Move, Attack, Halt, MoveRallyPoint
    }

    public GameManager gameManager;

    [Header("Panels and prefabs")]
    public GameObject buttonPanel;
    public Transform unitPanal;
    public GameObject unitInfoPanel;
    public GameObject unitIconPrefab;
    public TextMeshProUGUI alertMessage;

    [Header("Action Buttons")]
    public Button moveButton;
    public Button attackButton;
    public Button haltButton;
    public Button setRallyPointButton;

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
    private UnitController unitOnPanel = null;

    // properties
    public ActionChosen ButtonClicked { get; set; } = ActionChosen.Null;

    // Start is called before the first frame update
    void Start()
    {
        unitIcons = new List<GameObject>();
        selectedUnits = new List<UnitController>();
        unitManager = UnitManager.Instance;
        selectionManager = GameObject.FindObjectOfType<SelectionManager>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateUnitHealth();

        // check if mouse clicks on environment while an action is chosen
        if(Input.GetMouseButtonUp(0) && ButtonClicked != ActionChosen.Null && 
            !EventSystem.current.IsPointerOverGameObject())
        {
            RaycastHit hitInfo;

            if(ButtonClicked == ActionChosen.Move && 
                Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                unitManager.MoveUnits(hitInfo);

                // reset cursor and button
                gameManager.ResetCursor();
                moveButton.interactable = true;

                ButtonClicked = ActionChosen.Null;
            }
            else if(ButtonClicked == ActionChosen.Attack && 
                Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                bool attackSuccess = unitManager.AttackTarget(hitInfo.transform);

                if(!attackSuccess)
                {
                    StartCoroutine(ShowAlert("That's not a valid attack target", 2));
                }

                // reset cursor and button
                gameManager.ResetCursor();
                attackButton.interactable = true;

                ButtonClicked = ActionChosen.Null;
            }
            else if(ButtonClicked == ActionChosen.Halt)
            {
                unitManager.HaltUnitSelection();
            }
            if (ButtonClicked == ActionChosen.MoveRallyPoint && 
                Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                int layer = hitInfo.transform.gameObject.layer;

                if (layer == 3 || layer == 6)
                {
                    unitManager.SetPlayerRallyPointPosition(hitInfo.point);
                }
                else
                {
                    // display error message
                }

                // reset cursor and button
                gameManager.ResetCursor();
                setRallyPointButton.interactable = true;

                ButtonClicked = ActionChosen.Null; 
            }

            selectionManager.enabled = true;            
        }

        if (Input.GetMouseButtonUp(1) && ButtonClicked != ActionChosen.Null)
        {
            gameManager.ResetCursor();
            EnableActionButtons();
            ButtonClicked = ActionChosen.Null;

            selectionManager.enabled = true;            
        }
    }

    #region private functions
    private void UpdateUnitHealth()
    {
        try { 

            if (selectedUnits != null && selectedUnits.Count > 0)
            {
                for (int i = 0; i < selectedUnits.Count; i++)
                {
                    if (selectedUnits[i] == null)
                    { 
                        if(unitIcons[i] != null)
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
        catch(Exception ex)
        {
            Debug.LogError(ex.StackTrace);
        }

    }

    private void RemoveUnitIcon(int index)
    {
        // check if the object has already been destroyed
        if (index == -1)
            return;

        try
        {
            GameObject.Destroy(unitIcons[index]);
            unitIcons.RemoveAt(index);
        }
        catch(Exception e)
        {
            Debug.LogError(e.StackTrace);
        }
    }

    private IEnumerator ShowAlert(string message, int seconds)
    {
        alertMessage.text = message;
        alertMessage.gameObject.SetActive(true);

        yield return new WaitForSeconds(seconds);

        alertMessage.gameObject.SetActive(false);
    }

    // Makes all the buttons on the action button panel interactable
    private void EnableActionButtons()
    {
        moveButton.interactable = true;
        attackButton.interactable = true;
        haltButton.interactable = true;
        setRallyPointButton.interactable = true;
    }

    #endregion

    #region public functions

    /// <summary>
    /// Removes all information form gui to do with one particular unit
    /// </summary>
    /// <param name="unit">The instance of the UnitController script on the object about to be destroyed</param>
    public void RemoveUnitFromSelection(UnitController unit)
    {
        if (unitIcons.Count > 0)
        {
            RemoveUnitIcon(selectedUnits.IndexOf(unit));
        }
        else if (unitInfoPanel.activeInHierarchy && unit == unitOnPanel)
        {
            unitInfoPanel.SetActive(false);
            buttonPanel.SetActive(false);
        }

        selectedUnits.Remove(unit);

        // turn off the buttons panel is all selected untis are destroted
        if (selectedUnits.Count < 1)
        {
            buttonPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Handles the clicking of the move button
    /// </summary>
    public void SetMoveMode() 
    {
        ButtonClicked = ActionChosen.Move;
        selectionManager.enabled = false;

        moveButton.interactable = false;
        attackButton.interactable = true;
        haltButton.interactable = true;
        setRallyPointButton.interactable = true;

        gameManager.SetCursor(gameManager.moveCursor);
    }

    /// <summary>
    /// Handles the clicking of the attack button
    /// </summary>
    public void SetAttackMode() {
        ButtonClicked = ActionChosen.Attack;
        selectionManager.enabled = false;

        attackButton.interactable = false;
        moveButton.interactable = true;
        haltButton.interactable = true;
        setRallyPointButton.interactable = true;

        gameManager.SetCursor(gameManager.attackCursor);
    }

    /// <summary>
    /// Handles the clicking of the halt button
    /// </summary>
    public void SetHaltClicked()
    {
        ButtonClicked = ActionChosen.Halt;
        selectionManager.enabled = false;

        moveButton.interactable = true;
        attackButton.interactable = true;
        setRallyPointButton.interactable = true;

        gameManager.SetCursor(gameManager.defaultCursor);
    }

    /// <summary>
    /// Handles the clicking of the set rally point button
    /// </summary>
    public void SetMoveRallyPointMode()
    {
        ButtonClicked = ActionChosen.MoveRallyPoint;

        selectionManager.enabled = false;

        setRallyPointButton.interactable = false;
        attackButton.interactable = true;
        moveButton.interactable = true;
        haltButton.interactable = true;

        gameManager.SetCursor(gameManager.moveCursor);
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
            //return;
        }

        if(selection.Count > 0)
            buttonPanel.SetActive(true);

        // generate new icons
        for (int i=0; i < selection.Count; i++)
        {
            unitIcons.Add(Instantiate(unitIconPrefab, unitPanal));

            //if (selection[i].tag == "PlayerUnit")
                selectedUnits.Add(selection[i].GetComponent<UnitController>());
            //else if (selection[i].tag == "Navmesh Agent")
               // selectedUnits.Add(selection[i].GetComponent<NavMeshUnitController>());

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
        unitOnPanel = unit;

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