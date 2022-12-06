using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using static UnityEngine.UI.CanvasScaler;

public class UnitManager : MonoBehaviour
{
    #region variable declaration
    public UnitGui gui;
    public Transform playerRallyPoint;

    [Header("Layer Masks")]
    public LayerMask groundLayer;
    public LayerMask enemyLayers;
    RaycastHit hitInfo;

    //[SerializeField]
    List<GameObject> selectedUnits;
    SelectionManager selection;
    private UnitController selectedEnemyUnit;

    //[Header("Group Movement")]
    //[SerializeField] bool flockWhileMoving;

    public float unitInCombatTimeout = 30.0f;

    [Header("Unit Range Material Colors")]
    [SerializeField] Color detectionRangeColor = Color.yellow;
    [SerializeField] [Range(0, 1)]
    float detectionRangeAlpha = 0.1f;
    [SerializeField] Color attackRangeColor = Color.red;
    [SerializeField] [Range(0, 1)]
    float attackRangeAlpha = 0.1f;

    // properties
    public static UnitManager Instance { get; private set; }
    public Color DetectionRangeColor { get => detectionRangeColor; }
    public Color AttackRangeColor { get => attackRangeColor; }
    public UnitController SelectedEnemyUnit { get => selectedEnemyUnit; }

    List<List<GameObject>> squads;
    Vector3[] groupPath;
      
    private Vector3 point;
    new AudioSource audio;

    // external scripts
    private GameManager gameManager;
    private FormationManager formations;

    public LinkedList<UnitController> UCRefs = new LinkedList<UnitController>();
    #endregion

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

    #region start and update
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GetComponent<GameManager>();        

        selectedUnits = new List<GameObject>();
        selection = GetComponent<SelectionManager>();

        squads = new List<List<GameObject>>();

        formations = FormationManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1)) 
        {
            point = Input.mousePosition;
        }

        // move units when right mouse buttons is clicked
        if (Input.GetMouseButtonUp(1) && gui.ButtonClicked == UnitGui.ActionChosen.Null)
        {         
            Ray ray = Camera.main.ScreenPointToRay(point);

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo) && selection.Units.Count > 0)
            {
                // check if the cursor is over the a UI element
                //if (!EventSystem.current.IsPointerOverGameObject())
                int layer = hitInfo.transform.gameObject.layer;

                if (layer == 7 || layer == 9) // enemy layers
                {
                    AttackTarget(hitInfo.transform);
                }
                else
                {
                    MoveUnits(hitInfo);
                }
            }
        }
        DoTheMusic();

        // Update circle materials
        detectionRangeColor.a = detectionRangeAlpha;
        attackRangeColor.a = attackRangeAlpha;

    }
    #endregion

    // Added byt George
    void DoTheMusic() {

        /* Bit slow. I guess. I am tired. */
        if (MusicManager.Instance)
        {
            foreach (var r in UCRefs)
            {
                if (r.IsInCombat)
                {
                    MusicManager.Instance.ChangeState(MusicManager.State.Combat);
                    return;
                }
            }

            MusicManager.Instance.ChangeState(MusicManager.State.Normal);
        }
	}

    #region public functions

    /// <summary>
    /// Turns targeted highlight of object that a unit is fighting on or off
    /// </summary>
    /// <param name="unit">The unit being used</param>
    /// <param name="on">The on/off toggle to the highlight</param>
    public void SetTargetHighlight(UnitController unit, bool on)
    {
        var highlight = unit.AttackTarget.root.GetComponent<SelectionSprites>();
        highlight.ShowTargetedSprite = on;
    }

    /// <summary>
    /// Returns all units that are selected by the player
    /// </summary>
    /// <returns></returns>
    public List<UnitController> GetSelectedUnits()
    {
        List<UnitController> selected = new List<UnitController>();

        foreach(GameObject unit in selectedUnits)
        {
            selected.Add(unit.GetComponent<UnitController>());
        }

        return selected;
    }

    /// <summary>
    /// Set an indivudal unit as the selection
    /// </summary>
    /// <param name="unit">The gameobject being selected as the unit</param>
    public void SetSelectedUnit(GameObject obj)
    {
        // turn off selection highlights
        foreach(var selected in selectedUnits)
        {
            var unit = selected.GetComponent<UnitController>();            
            unit.SetSelected(false);            
        }

        selectedUnits.Clear();
        selectedUnits.Add(obj);

        obj.GetComponent<UnitController>().SetSelected(true);
    }

    /// <summary>
    /// Sets the selected units by passing in a list as a paramater
    /// </summary>
    /// <param name="units">List of gameobjects for the selected units</param>
    public void SetSelectedUnits(List<GameObject> units)
    {
        // turn off selection highlights and healthbars
        foreach (var selectedUnit in selectedUnits)
        {
            selectedUnit.GetComponent<UnitController>().SetSelected(false);
        }

        selectedUnits.Clear();
        selectedUnits = units;

        // turns on new selection
        foreach (var selectedUnit in selectedUnits)
        {
            selectedUnit.GetComponent<UnitController>().SetSelected(true);
        }
    }

    /// <summary>
    /// Removes an individual unit from the selected units list
    /// </summary>
    /// <param name="unit">the unit to be removed from the list</param>
    public void RemoveFromSelection(GameObject obj)
    {
        if (!selectedUnits.Contains(obj))
            return;

        selectedUnits.Remove(obj);

        var unit = obj.GetComponent<UnitController>();

        // Turns off selection sprite if no other enemy is targeting it
        if (unit.AttackTarget != null)
        {
            if(!unit.AttackTarget.GetComponent<UnitController>().IsTargeted())
            {
                unit.AttackTarget.GetComponent<SelectionSprites>().ShowTargetedSprite = false;
            }
            
        }
    }    

    // Not currently used
    /// <summary>
    /// Returns all game objects with the tag "PlayerUnit"
    /// </summary>
    /// <returns>Array of game objects</returns>
    public GameObject[] GetPlayerUnits()
    {
        return GameObject.FindGameObjectsWithTag("PlayerUnit");
    }

    // Not currently Used
    /// <summary>
    /// returns a list of all gem objects within a specified squad number
    /// </summary>
    /// <param name="squadNum"></param>
    /// <returns></returns>
    public List<GameObject> GetUnitsInSquad(int squadNum)
    {
        return squads[squadNum];
    }

    /// <summary>
    /// Causes all units in selection to enter their attack a target.
    /// Used with player units only
    /// </summary>
    /// <param name="target">The game object that the use clicked on</param>
    /// <returns>True or false value based on whether the object is valid</returns>
    public bool AttackTarget(Transform target)
    {
        target.GetComponent<SelectionSprites>().ShowAttackedSprite = true;        

        // check if the target's layer is one of the enemy layers
        if (enemyLayers == (enemyLayers | (1 << target.gameObject.layer)))
        {
            foreach(var unitObject in selectedUnits)
            {
                var unit = unitObject.GetComponent<UnitController>();

                // turn off highlight of previous target
                if (unit.AttackTarget != null)
                    SetTargetHighlight(unit, false);

                unit.AttackTarget = target;
                unit.AttackOrderGiven = true;

                // if the unit is not already in follow enemy state then swith state
                if(unit.State != UnitState.Follow)
                    unit.ChangeState(UnitState.Follow, target.position);
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Stops all units in a selection form moving
    /// </summary>
    public void HaltUnitSelection()
    {
        //Debug.Log("Halt button clicked");

        foreach(GameObject unit in selectedUnits)
        {
            var controller = unit.GetComponent<UnitController>();

            controller.UnitHalt = true;

            /*
            if (controller.State == UnitState.Attack)
                controller.ChangeState(UnitState.Halt);
            else
                controller.ChangeState(UnitState.Idle);*/
        }

    }

    /// <summary>
    /// Moves the player's rally point to a new location
    /// </summary>
    /// <param name="position">The new location of for the rally point</param>
    public void SetPlayerRallyPointPosition(Vector3 position)
    {
        formations.ClearRallyFormation(0);
        playerRallyPoint.position = new Vector3(position.x, 0, position.z);
    }

    //------------------
    // Not used anymore
    //------------------
    /// <summary>
    /// Creates a path for a group based on the center of mass
    /// </summary>
    /// <param name="destination">the desintation to move the group to</param>
    /// <returns>A list of waypoints for the group to move towards</returns>
    public Vector3[] GetPath(Vector3 destination)
    {
        NavMeshPath path = new NavMeshPath();

        // get the average position of all units
        Vector3 unitCenter = new Vector3();

        foreach (GameObject unit in selection.Units)
        {
            unitCenter += unit.transform.position;
        }
        unitCenter /= selection.Units.Count;

        NavMesh.CalculatePath(unitCenter, destination, NavMesh.AllAreas, path);

        return path.corners;
    }

    /// <summary>
    /// Moves all units in the selection to a location and changes their state
    /// </summary>
    /// <param name="hit">The raycast hit object</param>
    public void MoveUnits(RaycastHit hit)
    {
        Vector3 targetPos;
        
        gameManager.SetMarkerLocation(new Vector3(hit.point.x, hit.point.y + 0.5f, hit.point.z));

        // check that the player clicked on the ground.
        // If they did not find a new position
        if (hit.transform.gameObject.layer == 3)
        {
            targetPos = hit.point;
        }
        else
        {
            NavMeshHit hitInfo;

            if (NavMesh.SamplePosition(hit.point, out hitInfo, 10f, NavMesh.AllAreas))
            {
                targetPos = hitInfo.position;
            }
            else
            {
                Debug.Log("No position in range could be found");
                return;
            }

        }

        squads.Add(selectedUnits);

        if (selectedUnits.Count > 1)
        {
            List<Vector3> formationPositions = formations.GetFormationPositions(targetPos, selection.Units);

            if (formationPositions.Count < selectedUnits.Count)
            {
                Debug.LogError("Not enough formations positions were created for the selected units");
                return;
            }

            // move all units to their designated targets
            for (int i = 0; i < selectedUnits.Count; i++)
            {
                //var agent = selectedUnits[i].GetComponent<AgentMovement>();
                //agent.SquadNum = squads.Count - 1;

                var unit = selectedUnits[i].GetComponent<UnitController>();

                //if (flockWhileMoving)
                    //unit.ChangeState(UnitState.Flock, formationPositions[i]);
                //else
                    unit.ChangeState(UnitState.Moving, formationPositions[i]);
            }

        }
        else
        {
            GameObject unit = selection.Units[0];
            var controller = unit.GetComponent<UnitController>();

            controller.ChangeState(UnitState.Moving, targetPos);

            //unit.GetComponent<SeekState>().MoveTo(hitInfo.point);
        }
    }    

    /// <summary>
    /// Removes a specifed unit from a moving group
    /// </summary>
    /// <param name="unit">The unit to be removed</param>
    /// <param name="squadNum">the index of the squad in the squads list</param>
    public void RemoveFromSquad(GameObject unit, int squadNum)
    {
        squads[squadNum].Remove(unit);
    }    
    #endregion

    #region private functions
    // Not currently be using    
    // check if all units within a specified are stationary
    /*private bool UnitsNotMoving(List<GameObject> units)
    {
        foreach(GameObject unit in units)
        {
            if (unit.GetComponent<UnitController>().State == UnitState.Flock)
                return false;
        }

        return true;
    }

    // Not currently be using
    
    // returns all of the units that are currently moving within a specified squad
    private void CheckNeigboursMoving(int squadNum)
    {
        bool unitsStillMoving = false;

        foreach (GameObject unit in squads[squadNum])
        {
            if (unit.GetComponent<UnitController>().State == UnitState.Flock)
                unitsStillMoving = true;
        }

        if (!unitsStillMoving)
            squads.RemoveAt(squadNum);
    }*/
    #endregion

    private void OnDrawGizmos()
    {
        // draws the path that each unit follows
        if (groupPath != null)
        {
            Gizmos.color = Color.red;

            for (int i = 0; i < groupPath.Length - 1; i++)
                Gizmos.DrawLine(groupPath[i], groupPath[i + 1]);
        }        

    }

    /// <summary>
    /// Turns on highlights on enemy unit
    /// </summary>
    /// <param name="unit">The enemy unit being selected</param>
    public void SelectEnemyUnit(UnitController unit)
    {
        selectedEnemyUnit = unit;
        unit.SetSelected(true);

        if (unit.AttackTarget != null)
        {
            unit.AttackTarget.GetComponent<SelectionSprites>().ShowTargetedSprite = true;
        }
    }

    public void DeselectEnemyUnit()
    {
        if(selectedEnemyUnit != null)
        {
            selectedEnemyUnit.SetSelected(false);
            selectedEnemyUnit.SingleSelected = false;
            selectedEnemyUnit = null;
        }
    }
}
