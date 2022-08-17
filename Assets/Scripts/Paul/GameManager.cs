using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    public Transform marker;

    RaycastHit hitInfo;

    [SerializeField]
    List<GameObject> selectedUnits;
    SelectionManager selection;

    [SerializeField]
    List<List<GameObject>> squads;

    [Header("Unit formations")]
    [SerializeField]float offsetLength = 1.0f;

    List<Vector3> positions;

    Vector3[] groupPath;

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
            UnitState state = unit.GetComponent<StateManager>().State;

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
            unit.GetComponent<StateManager>().ChangeState(UnitState.Idle);
        }

    }

    /// <summary>
    /// Creates a path for a group based on the center of mass
    /// </summary>
    /// <param name="destination"></param>
    /// <returns></returns>
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

        if (hit.transform.gameObject.layer == 3) // Ground
        {
            marker.transform.position = new Vector3(hit.point.x, 1, hit.point.z);

            if(selection.Units.Count > 1)
            {
                //groupPath = GetPath(hit.point);
                
                positions = GetFormationPositions(hit.point);

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
                    var unit = selection.Units[i].GetComponent<StateManager>();
                    unit.ChangeState(UnitState.Moving, positions[i]);
                }

            }
            else
            {
                GameObject unit = selection.Units[0];
                var states = unit.GetComponent<StateManager>();                

                //states.target = hit.point;
                states.ChangeState(UnitState.Moving, hit.point);

                //unit.GetComponent<SeekState>().MoveTo(hitInfo.point);
            }

            squads.Add(selection.Units);
        }
    }

    
    /// <summary>
    /// Create a formations for all the selected units based on the position the player clicked
    /// </summary>
    /// <param name="point">The target position on the map to created a formation around</param>
    /// <returns>A list of coordinates for all positions in the formation</returns>
    private List<Vector3> GetFormationPositions(Vector3 point)
    {        
        Vector3 unitCenter = new Vector3();        

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
            if (unit.GetComponent<StateManager>().State == UnitState.Flock)
                return false;
        }

        return true;
    } 

    private void CheckNeigboursMoving(int squadNum)
    {
        bool unitsStillMoving = false;

        foreach (GameObject unit in squads[squadNum])
        {
            if (unit.GetComponent<StateManager>().State == UnitState.Flock)
                unitsStillMoving = true;
        }

        if (!unitsStillMoving)
            squads.RemoveAt(squadNum);
    }

    private void OnDrawGizmos()
    {
        if(groupPath != null)
        {
            Gizmos.color = Color.red;

            for(int i=0; i < groupPath.Length - 1; i++)
                Gizmos.DrawLine(groupPath[i], groupPath[i + 1]);
        }

        if (positions != null)
        {
            foreach (Vector3 position in positions)
            {
                Gizmos.DrawSphere(position, 1);
            }
        }
    }

}