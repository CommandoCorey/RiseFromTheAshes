using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class UnitManager : MonoBehaviour
{
    #region variable declaration
    public LayerMask groundLayer;
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

    List<List<GameObject>> squads;

    List<Vector3> formationPositions;
    Vector3[] groupPath;

    private List<Vector3> searchedPositions = new List<Vector3>();

    private GameManager gameManager;
    #endregion

    #region start and update
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GetComponent<GameManager>();

        selectedUnits = new List<GameObject>();
        //selectedUnits = new Dictionary<int, GameObject>();
        selection = GetComponent<SelectionManager>();

        squads = new List<List<GameObject>>();
        formationPositions = new List<Vector3>();
    }

    // Update is called once per frame
    void Update()
    {
        // move units when right mouse buttons is clicked
        if (Input.GetMouseButtonDown(1) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
        {
            if (selection.Units.Count > 0)
                MoveUnits(hitInfo);
        }

    }
    #endregion

    #region public functions
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

    public void StopGroupMoving()
    {
        var squad = GameObject.FindGameObjectsWithTag("PlayerUnit");

        foreach(GameObject unit in squad)
        {
            unit.GetComponent<UnitController>().ChangeState(UnitState.Idle);
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
    #endregion

    #region private functions
    private void MoveUnits(RaycastHit hit)
    {
        formationPositions.Clear();
        Vector3 targetPos;

        gameManager.SetMarkerLocation(new Vector3(hit.point.x, 1, hit.point.z));        

        // check that the player clicked on the ground.
        // If they did not find a new position
        if (hit.transform.gameObject.layer == groundLayer)
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

        squads.Add(selection.Units);

        if (selection.Units.Count > 1)
        {                
            formationPositions = GetFormationPositions(targetPos);          

            if(formationPositions.Count < selection.Units.Count)
            {
                Debug.LogError("Not enough formations positions were created for the selected units");
                return;
            }
                
            // move all units to their designated targets
            for(int i=0; i < selection.Units.Count; i++)
            {
                var agent = selection.Units[i].GetComponent<AgentMovement>();
                agent.SquadNum = squads.Count - 1;

                var unit = selection.Units[i].GetComponent<UnitController>();

                if(flockWhileMoving)
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
    private List<Vector3> GetFormationPositions(Vector3 point)
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
        if (!GameObject.FindWithTag("Ground"))
        {
            Debug.LogError("The ground object has not been tagged");
            return formationPositions;
        }

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
                    if (rayHit.transform.gameObject.layer == groundLayer || 
                        rayHit.transform.tag == "Ground")
                    {
                        // Make sure point is on navmesh
                        // Note: maxDistance must be abov zero else program will get stuck in loop forever
                        if (NavMesh.SamplePosition(rayHit.point, out navHit, 0.1f, NavMesh.AllAreas))
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

    bool IsOutOfBounds(Vector3 position)
    {
        if(Physics.Raycast(position + Vector3.up * 10, Vector3.down, 10))
            return false;
        
        return true;
    }

    private List<Vector3> GetBasicFormations(Vector3 point)
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
        Vector3 position;
        int xOffset;
        int unitsInRow = 0;
        int row = 0;

        // create the formation positions
        for (int i = 0; i < selection.Units.Count; i++)
        {
            if (i <= selection.Units.Count / 2)            
                xOffset = i;               
            else            
                xOffset = i - (selection.Units.Count / 2);            

            position.x = point.x + xOffset * spaceBetweenUnits;
            position.y = point.y;
            position.z = point.z + row * spaceBetweenUnits;

            // move units to next row
            if (unitsInRow == maxUnitsPerRow)
            {
                row++;
                unitsInRow = 0;
            }

            // check if the position is on the navigation mesh
            if (!Physics.Raycast(position + Vector3.up * 10, Vector3.down, out rayHit, 10, groundLayer))
            {
                // get a new position
                if (NavMesh.SamplePosition(position, out navHit, 5, NavMesh.AllAreas))
                {
                    position = navHit.position;
                }
            }

            formationPositions.Add(position);
            unitsInRow++;
        }

        return formationPositions;
    }

    private Vector3 GetRightAngle(Vector3 current)
    {
        Vector3 newVector;
        newVector.x = current.z;
        newVector.y = 0;
        newVector.z = current.x;

        return newVector;
    }

    private bool UnitsNotMoving(List<GameObject> units)
    {
        foreach(GameObject unit in units)
        {
            if (unit.GetComponent<UnitController>().State == UnitState.Flock)
                return false;
        }

        return true;
    } 

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
        if (formationPositions != null)
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
