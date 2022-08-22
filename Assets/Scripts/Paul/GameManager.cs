using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    public Transform marker;
    public GameObject minimap;
    public bool showMinimap = false;
    public LayerMask groundLayer;

    RaycastHit hitInfo;

    [SerializeField]
    List<GameObject> selectedUnits;
    SelectionManager selection;

    [SerializeField]
    List<List<GameObject>> squads;

    [Header("Unit formations")]
    [SerializeField]float offsetLength = 1.5f;

    List<Vector3> positions;

    Vector3[] groupPath;

    private List<Vector3> searchedPositions = new List<Vector3>();

    public GameObject[] GetPlayerUnits()
    {
        return GameObject.FindGameObjectsWithTag("PlayerUnit");
    }

    public List<GameObject> GetNeighbourUnits(GameObject current)
    {
        List<GameObject> neighbours = new List<GameObject> ();
        var units = GameObject.FindGameObjectsWithTag("PlayerUnit");

        foreach (var unit in units)
        {
            if(unit != current)
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

    // Start is called before the first frame update
    void Start()
    {
        selectedUnits = new List<GameObject>();
        //selectedUnits = new Dictionary<int, GameObject>();
        selection = GetComponent<SelectionManager>();

        squads = new List<List<GameObject>>();
        positions = new List<Vector3>();

        if(showMinimap)
            minimap.active = true;
    }

    // Update is called once per frame
    void Update()
    {
        // move units when right mouse buttons is clicked
        if(Input.GetMouseButtonDown(1) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
        {
            if(selection.Units.Count > 0)
                MoveUnits(hitInfo);
        }

    }

    private void MoveUnits(RaycastHit hit)
    {
        positions.Clear();
        Vector3 targetPos;

        marker.transform.position = new Vector3(hit.point.x, 1, hit.point.z);

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

            //targetPos = GetNewPosition(hit.point);

            //if (targetPos == Vector3.zero)
            //return;
        }

        if(selection.Units.Count > 1)
        {
            //groupPath = GetPath(hit.point);
                
            positions = GetFormationPositions(targetPos);            

            /*
            foreach (GameObject unit in selection.Units)
            {
                var states = unit.GetComponent<StateManager>();

                //states.target = hit.point;
                states.ChangeState(UnitState.Flock, hit.point);                    
            }*/

                
            // move all units to their designated target
            for(int i=0; i < selection.Units.Count; i++)
            {
                var unit = selection.Units[i].GetComponent<UnitController>();
                unit.ChangeState(UnitState.Moving, positions[i]);
            }

        }
        else
        {
            GameObject unit = selection.Units[0];
            var controller = unit.GetComponent<UnitController>();

            controller.ChangeState(UnitState.Moving, targetPos);

            //unit.GetComponent<SeekState>().MoveTo(hitInfo.point);
        }

        squads.Add(selection.Units);
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

        // create the formation positions
        for(int i = 0; i < selection.Units.Count; i++)
        {
            Vector3 position = point + (offsetDirection * i * offsetLength);

            // check if the position is on the navigation mesh
            if (!Physics.Raycast(position + Vector3.up * 10, Vector3.down, out rayHit, 10, groundLayer))
            {
                // get a new position
                if (NavMesh.SamplePosition(position, out navHit, 5, NavMesh.AllAreas))
                {
                    position = navHit.position;
                }
            }

            positions.Add(position);

        }

        return positions;
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

    // Redirects the unit to another position near and obstacle that was clicked on
    // Not currently being used
    /*private Vector3 GetNewPosition(Vector3 clickedPos)
    {
        Vector2[] searchDirections = { new Vector2(-1, 1), Vector2.up, new Vector2(1, 1), Vector2.right,
                                       new Vector2(1, -1), Vector2.down, new Vector2(-1, -1), Vector2.left};

        Vector3 direction;// = (selection.CenterPoint - clickedPos).normalized;
        float offset = 1;
        float maxIterations = 10;
        Vector3 castPosition;

        Ray ray;
        RaycastHit hitInfo;
        NavMeshHit navHit;

        //Debug.DrawRay(clickedPos, direction, Color.red, 2.0f);        

        bool foundPos = false;

        searchedPositions.Clear();

        for (int i = 0; i < maxIterations; i++)
        {
            foreach (Vector3 searchDirection in searchDirections)
            {
                // converts 2D space into 3D
                direction = new Vector3(searchDirection.x, 0, searchDirection.y);

                castPosition = clickedPos + (direction * i);
                castPosition.y = 0;

                ray = new Ray(castPosition, direction);

                searchedPositions.Add(castPosition);

                if (Physics.BoxCast(castPosition, Vector3.one, direction, out hitInfo, Quaternion.identity, 10))
                {
                    Debug.Log("Hit Object: " + hitInfo.transform.name);
                    continue;
                }
                else
                {
                    Debug.DrawLine(castPosition + Vector3.up * 10, castPosition, Color.blue, 3.0f);

                    // fire a raycast downward to see if it hits the ground
                    if (Physics.Raycast(castPosition + Vector3.up * 2, Vector3.down, out hitInfo, 3))
                    {
                        Debug.Log("Hit plane");

                        // check the the position that the raycast hit is part of the navigation mesh
                        if (NavMesh.SamplePosition(hitInfo.point, out navHit, 1f, NavMesh.AllAreas))
                        {
                            Debug.Log("Hit Navigation Mesh");

                            return navHit.position;
                        }
                    }

                }
            }

        }     

        return Vector3.zero;

    }*/

    private void OnDrawGizmos()
    {
        if (groupPath != null)
        {
            Gizmos.color = Color.red;

            for (int i = 0; i < groupPath.Length - 1; i++)
                Gizmos.DrawLine(groupPath[i], groupPath[i + 1]);
        }

        if (positions != null)
        {
            foreach (Vector3 position in positions)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(position, 1);
            }
        }

        if (searchedPositions != null)
        {
            Gizmos.color = Color.green;
            foreach (Vector3 position in searchedPositions)
            {                   
                if(position == searchedPositions.Last())
                    Gizmos.color = Color.blue;

                Gizmos.DrawWireCube(position, Vector3.one);
            }
               
        }

    }

}