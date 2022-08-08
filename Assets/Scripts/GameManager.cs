using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    RaycastHit hitInfo;

    private List<UnitController> selectedUnits;

    // Start is called before the first frame update
    void Start()
    {
        selectedUnits = new List<UnitController>();
    }

    // Update is called once per frame
    void Update()
    {
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

        }

        if(Input.GetMouseButtonDown(1))
        {
            if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                if(hitInfo.transform.gameObject.layer == 3) // Ground
                {
                    foreach (UnitController unit in selectedUnits)
                        unit.MoveTo(hitInfo.point);
                }

            }

        }

    }

}