using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

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

    [Header("Gizmos")]
    public bool showFormationPosition = false;

    List<List<GameObject>> squads;

    List<Vector3> formationPositions;
    Vector3[] groupPath;

    private List<Vector3> searchedPositions = new List<Vector3>();   
    private Vector3 point;
    new AudioSource audio;

    // external scripts
    private GameManager gameManager;
    #endregion

    #region start and update
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GetComponent<GameManager>();        

        selectedUnits = new List<GameObject>();
        selection = GetComponent<SelectionManager>();

        squads = new List<List<GameObject>>();
        formationPositions = new List<Vector3>();
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

    }
    #endregion

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
        formationPositions.Clear();
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
            formationPositions = GetFormationPositions(targetPos);

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

        for (int row = 0; unitsPlaced < selection.Units.Count; row++)
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
                    break;

            }

            unitsOnLeft = 0;
            unitsOnRight = 0;
        }

        return formationPositions;
    }

    /// <summary>
    /// Cretes formation positions for a group of units moving towards a specified point
    /// </summary>
    /// <param name="location"></param>
    /// <param name="unitList"></param>
    /// <returns></returns>
    public List<Vector3> GetFormationPositions(Vector3 destination, List<Transform> unitList)
    {
        Vector3 unitCenter = new Vector3();
        RaycastHit rayHit;
        NavMeshHit navHit;
        List<Vector3> formationPositions = new List<Vector3>();

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

        for (int row = 0; unitsPlaced < unitList.Count; row++)
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
                    Debug.Log("Raycast did not hit anything at position " + position);
                    break;
                }

                // exit loop if all units are placed
                if (unitsPlaced == selection.Units.Count)
                    break;
            }

            unitsOnLeft = 0;
            unitsOnRight = 0;
        }

        return formationPositions;
    }

    #endregion

    #region private functions 
    bool IsOutOfBounds(Vector3 position)
    {
        if(Physics.Raycast(position + Vector3.up * 10, Vector3.down, 10))
            return false;
        
        return true;
    }

    private Vector3 GetRightAngle(Vector3 current)
    {
        Vector3 newVector;
        newVector.x = -current.z;
        newVector.y = 0;
        newVector.z = current.x;

        return newVector;
    }

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

    // Not 
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
        if (formationPositions != null && showFormationPosition)
        {
            foreach (Vector3 position in formationPositions)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(position, 1);
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
