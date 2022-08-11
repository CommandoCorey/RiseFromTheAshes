using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Transform marker;

    RaycastHit hitInfo;

    List<GameObject> selectedUnits;
    SelectionManager selection;

    List<List<GameObject>> squads;

    public GameObject[] GetPlayerUnits()
    {
        return GameObject.FindGameObjectsWithTag("Boid");
    }

    public List<GameObject> GetNeighbourUnits(GameObject current)
    {
        List<GameObject> neighbours = new List<GameObject> ();
        var units = GameObject.FindGameObjectsWithTag("Boid");

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
        var units = GameObject.FindGameObjectsWithTag("Boid");

        foreach(var unit in units)
        {
            UnitState state = unit.GetComponent<StateManager>().State;

            if(state == UnitState.Moving || state == UnitState.Flock)
                movingUnits.Add(unit);
        }

        return movingUnits;
    }


    public void StopGroupMoving()
    {
        var squad = GameObject.FindGameObjectsWithTag("Boid");

        foreach(GameObject unit in squad)
        {
            unit.GetComponent<StateManager>().ChangeState(UnitState.Idle);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        selectedUnits = new List<GameObject>();
        //selectedUnits = new Dictionary<int, GameObject>();
        selection = GetComponent<SelectionManager>();

        squads = new List<List<GameObject>>();
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
        if (hit.transform.gameObject.layer == 3) // Ground
        {
            marker.transform.position = new Vector3(hit.point.x, 1, hit.point.z);

            if(selection.Units.Count > 1)
            {
                foreach (GameObject unit in selection.Units)
                {
                    var states = unit.GetComponent<StateManager>();

                    states.target = hit.point;
                    states.ChangeState(UnitState.Flock);                    
                }
            }
            else
            {
                GameObject unit = selection.Units[0];
                var states = unit.GetComponent<StateManager>();

                states.target = hit.point;
                states.ChangeState(UnitState.Moving);
                //unit.GetComponent<SeekState>().MoveTo(hitInfo.point);
            }

            squads.Add(selection.Units);
        }
    }

}