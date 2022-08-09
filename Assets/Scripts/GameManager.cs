using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    RaycastHit hitInfo;

    List<GameObject> selectedUnits;
    SelectionManager selection;

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

    // Start is called before the first frame update
    void Start()
    {
        selectedUnits = new List<GameObject>();
        //selectedUnits = new Dictionary<int, GameObject>();
        selection = GetComponent<SelectionManager>();
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                //Debug.Log("Clicked on " + hitInfo.transform.name);
                
                if(hitInfo.transform.gameObject.layer == 6) // Unit
                {
                    //Debug.Log("Selected Unit");
                    selectedUnits.Add(hitInfo.transform.GetComponent<UnitController>());
                    selectedUnits.Last().SetSelected(true);
                }
                else if(selectedUnits.Count > 0)
                {
                    foreach (UnitController unit in selectedUnits)
                        unit.SetSelected(false);

                    selectedUnits.Clear();
                }
            }

        }*/

        // move units when right mouse buttons is clicked
        if(Input.GetMouseButtonDown(1))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                if(hitInfo.transform.gameObject.layer == 3) // Ground
                {
                    foreach (GameObject unit in selection.Units)
                    {
                        unit.GetComponent<StateManager>().ChangeState(UnitState.Moving);
                        unit.GetComponent<MoveState>().MoveTo(hitInfo.point);
                    }
                }

            }

        }

    }

}