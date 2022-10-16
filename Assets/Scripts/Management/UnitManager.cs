using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class UnitManager : MonoBehaviour
{
    #region variable declaration
    public UnitGui gui;

    [Header("Layer Masks")]
    public LayerMask groundLayer;
    public LayerMask enemyLayers;
    RaycastHit hitInfo;

    //[SerializeField]
    List<GameObject> selectedUnits;
    SelectionManager selection;

    [Header("Group Movement")]
    [SerializeField] bool flockWhileMoving;

    [Header("Unit Formations")]
    [SerializeField][Range(1, 10)]
    float spaceBetweenUnits = 1.5f;
    [SerializeField][Range(1, 20)]
    int maxUnitsPerRow = 5;
    [SerializeField]
    int maxRows = 1000;

    [Header("Gizmos")]
    public bool showFormationPositions = false;
    public bool showAiRallyPositions = false;
    public bool showSearchedPositions = false;

    public float unitInCombatTimeout = 30.0f;

    public static UnitManager Instance { get; private set; }

    List<List<GameObject>> squads;

    List<Vector3> formationPositions;
    List<Vector3> playerRallyFormation;
    List<Vector3> aiRallyFormation;
    List<Vector3> sarchedPositions;

    Vector3[] groupPath;
      
    private Vector3 point;
    new AudioSource audio;

    bool anythingInCombat;

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
        formationPositions = new List<Vector3>();
        playerRallyFormation = new List<Vector3>();
        aiRallyFormation = new List<Vector3>();
        sarchedPositions = new List<Vector3>();

        formations = FormationManager.Instance;

        anythingInCombat = false;
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
            // check if the cursor is over the a UI element
            if (EventSystem.current.IsPointerOverGameObject())                
            {
                //Debug.Log("Clicked on GUI");
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(point);

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo) && selection.Units.Count > 0)
                MoveUnits(hitInfo);
        }
        DoTheMusic();
    }
    #endregion

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
    /// 
    /// </summary>
    /// <returns></returns>
    public List<UnitController> GetSelectedUnits()
    {
        List<UnitController> selected = new List<UnitController>();

        foreach(GameObject unit in selection.Units)
        {
            selected.Add(unit.GetComponent<UnitController>());
        }

        return selected;
    }

    /// <summary>
    /// Set an indivudal unit as the selection
    /// </summary>
    /// <param name="unit">The gameobject being selected as the unit</param>
    public void SetSelectedUnit(GameObject unit)
    {
        // turn off selection highlights
        foreach(var selected in selectedUnits)
        {
            selected.GetComponent<UnitController>().SetSelected(false);
        }

        selectedUnits.Clear();
        selectedUnits.Add(unit);

        unit.GetComponent<UnitController>().SetSelected(true);
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
    public void RemoveFromSelection(GameObject unit)
    {
        selectedUnits.Remove(unit);
    }
    
    public GameObject[] GetPlayerUnits()
    {
        return GameObject.FindGameObjectsWithTag("PlayerUnit");
    }

    public List<GameObject> GetNeighbourUnits(GameObject current, int squad)
    {
        List<GameObject> neighbours = new List<GameObject> ();
        //var units = GameObject.FindGameObjectsWithTag("PlayerUnit");

        foreach (var unit in squads[squad])
        {
            UnitState state = unit.GetComponent<UnitController>().State;

            if (unit != current && state == UnitState.Flock)
                neighbours.Add(unit);
        }

        return neighbours;
    }

    public List<GameObject> GetMovingUnits(GameObject current)
    {
        List<GameObject> movingUnits = new List<GameObject>();
        var units = GameObject.FindGameObjectsWithTag("PlayerUnit");

        foreach(var unit in units)
        {
            UnitState state = unit.GetComponent<UnitController>().State;

            if(state == UnitState.Moving || state == UnitState.Flock)
                movingUnits.Add(unit);
        }

        return movingUnits;
    }

    public List<GameObject> GetUnitsInSquad(int squadNum)
    {
        return squads[squadNum];
    }

    /// <summary>
    /// Causes all units in selection to enter their attack a target
    /// </summary>
    /// <param name="target">The game object that the use clicked on</param>
    /// <returns>True or false value based on whether the object is valid</returns>
    public bool AttackTarget(Transform target)
    {
        // check if the target's layer is one of the enemy layers
        if(enemyLayers == (enemyLayers | (1 << target.gameObject.layer)))
        {
            foreach(var unit in selectedUnits)
            {
                var controller = unit.GetComponent<UnitController>();
                controller.AttackTarget = target;
                controller.ChangeState(UnitState.Attack, target.position);
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

                if (flockWhileMoving)
                    unit.ChangeState(UnitState.Flock, formationPositions[i]);
                else
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
    
    /// <summary>
    /// Create a formations for all the selected units based on the position the player clicked
    /// </summary>
    /// <param name="point">The target position on the map to created a formation around</param>
    /// <returns>A list of coordinates for all positions in the formation</returns>
    public List<Vector3> GetFormationPositions(Vector3 point)
    {        
        Vector3 unitCenter = new Vector3();    
        RaycastHit rayHit;
        NavMeshHit navHit;

        searchedPositions.Clear();

        foreach (GameObject unit in selection.Units)
        {
            unitCenter += unit.transform.position;
        }
        unitCenter /= selection.Units.Count;

        Vector3 moveDirection = (point - unitCenter).normalized;
        Vector3 offsetDirection = GetRightAngle(moveDirection);
        Vector3 position;
        int unitsOnLeft = 0;
        int unitsOnRight = 0;
        int unitsPlaced = 0;

        // make sure an object with the ground tag exists
        /*if (!GameObject.FindWithTag("Ground"))
        {
            Debug.LogError("The ground object has not been tagged");
            return formationPositions;
        }*/

        for (int row = 0; row < maxRows; row++)
        {            
            // create the formation positions
            for (int column = 0; column < maxUnitsPerRow; column++)
            {                
                if (column <= maxUnitsPerRow / 2)
                {  
                    position = point + (offsetDirection * unitsOnRight * spaceBetweenUnits);
                    unitsOnRight++;
                }
                else
                {
                    unitsOnLeft++;
                    position = point - (offsetDirection * unitsOnLeft * spaceBetweenUnits);
                }

                //Debug.Log("Old position: " + position);
                position -= moveDirection * spaceBetweenUnits * row;
                //Debug.Log("New position: " + position);                

                searchedPositions.Add(position);

                // check that the position is not out of bounds
                if (Physics.Raycast(position + Vector3.up, Vector3.down, out rayHit))
                {                    
                    // check if the position is on the ground
                    if (rayHit.transform.gameObject.layer == 3 || rayHit.transform.tag == "Ground")
                    {
                        // Make sure point is on navmesh
                        // Note: maxDistance must be above agent radius else program will get stuck in loop forever
                        if (NavMesh.SamplePosition(rayHit.point, out navHit, 1.0f, NavMesh.AllAreas))
                        {
                            formationPositions.Add(navHit.position);
                            unitsPlaced++;
                        }
                    }

                }

                // exit loop if all units are placed
                if (unitsPlaced == selection.Units.Count)
                    return formationPositions;
            }

            unitsOnLeft = 0;
            unitsOnRight = 0;
        }

        return formationPositions;
    }

    /// <summary>
    /// Cretes formation positions for a group of units moving towards a specified point
    /// </summary>
    /// <param name="destination">The position on the map to move all of the units to</param>
    /// <param name="unitList">The list of transforms being moved</param>
    /// <returns></returns>
    public List<Vector3> GetFormationPositions(Vector3 destination, List<Transform> unitList)
    {
        Vector3 unitCenter = new Vector3();
        RaycastHit rayHit;
        NavMeshHit navHit;
        List<Vector3> formationPositions = new List<Vector3>();

        searchedPositions.Clear();

        foreach (Transform unit in unitList)
        {
            unitCenter += unit.position;
        }
        unitCenter /= unitList.Count;

        Vector3 moveDirection = (destination - unitCenter).normalized;
        Vector3 offsetDirection = GetRightAngle(moveDirection);
        Vector3 position;
        int unitsOnLeft = 0;
        int unitsOnRight = 0;
        int unitsPlaced = 0;        

        for (int row = 0; row < maxRows; row++)
        {
            // create the formation positions
            for (int column = 0; column < maxUnitsPerRow; column++)
            {
                if (column <= maxUnitsPerRow / 2)
                {
                    position = destination + (offsetDirection * unitsOnRight * spaceBetweenUnits);
                    unitsOnRight++;
                }
                else
                {
                    unitsOnLeft++;
                    position = destination - (offsetDirection * unitsOnLeft * spaceBetweenUnits);
                }

                //Debug.Log("Old position: " + position);
                position -= moveDirection * spaceBetweenUnits * row;
                //Debug.Log("New position: " + position);               
                searchedPositions.Add(position);

                // check that the position is not out of bounds
                if (Physics.Raycast(position + Vector3.up, Vector3.down, out rayHit))
                {
                    // check if the position is on the ground
                    if (rayHit.transform.gameObject.layer == 3 || rayHit.transform.tag == "Ground")
                    {
                        // Make sure point is on navmesh
                        // Note: maxDistance must be above agent radius else program will get stuck in loop forever
                        if (NavMesh.SamplePosition(rayHit.point, out navHit, 1.0f, NavMesh.AllAreas))
                        {
                            formationPositions.Add(navHit.position);
                            unitsPlaced++;
                        }
                    }

                }
                else
                {
                    //Debug.Log("Raycast did not hit anything at position " + position);
                    break;
                }

                // exit loop if all units are placed
                if (unitsPlaced == unitList.Count)
                    return formationPositions;
            }

            unitsOnLeft = 0;
            unitsOnRight = 0;
        }

        if (unitsPlaced < selection.Units.Count)
        {
            Debug.LogError("Max Rows in formation exceeded");
        }

        return formationPositions;
    }

    public void AddRallyFormationPoint(Vector3 point, int player = 0)
    {
        if(player == 0)
            playerRallyFormation.Add(point);
        else if(player == 1)
            aiRallyFormation.Add(point);
    }

    public List<Vector3> GetRallyFormation(int player = 0)
    {
        if (player == 0)
            return playerRallyFormation;
        else
            return aiRallyFormation;
    }

    public void ClearRallyFormation(int player = 0)
    {
        if(player == 0)
            playerRallyFormation.Clear();
        else
            aiRallyFormation.Clear();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rallyPoint"></param>
    /// <param name="player"></param>
    /// <returns></returns>
    public Vector3 GetRallyPosition(Vector3 rallyPoint, int player)
    {
        if (player == 0) // Human player
            return GetNextFormationPoint(playerRallyFormation, rallyPoint);

        else if (player == 1) // Ai player
            return GetNextFormationPoint(aiRallyFormation, rallyPoint);

        else
            return rallyPoint;
    }

    // used for agent priorities
    public int GetCurrentRallySize(int player)
    {
        if (player == 0)
            return playerRallyFormation.Count;

        else if (player == 1) // Ai player
            return aiRallyFormation.Count;

        return 0;
    }
    #endregion

    #region private functions 
    // Not currently be using
    private bool UnitsNotMoving(List<GameObject> units)
    {
        foreach(GameObject unit in units)
        {
            if (unit.GetComponent<UnitController>().State == UnitState.Flock)
                return false;
        }

        return true;
    }

    // Not currently be using
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
    }
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

        // draws the fromation positions that each unit will finish at
        if (formationPositions != null && showFormationPositions)
        {
            foreach (Vector3 position in formationPositions)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(position, 1);
            }
        }

        if(searchedPositions != null && showSearchedPositions)
        {
            foreach (Vector3 position in searchedPositions)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(position, 1);
            }
        }                

        // 
        if (aiRallyFormation != null && showAiRallyPositions)
        {
            foreach (Vector3 position in aiRallyFormation)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(position + Vector3.up * 0.5f, 1);
            }
        }

        /*
        if (searchedPositions != null)
        {
            Gizmos.color = Color.green;
            foreach (Vector3 position in searchedPositions)
            {                   
                if(position == searchedPositions.Last())
                    Gizmos.color = Color.blue;

                Gizmos.DrawWireCube(position, Vector3.one);
            }               
        }*/

    }

}
