using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionManager : MonoBehaviour
{
    public LayerMask selectionLayer;
    [SerializeField][Range(4, 50)]
    public bool drawDebugBox = true;
    public UnitGui gui; // used to update unit icons

    // contains all of the selected units
    private Dictionary<int, GameObject> selectedTable = new Dictionary<int, GameObject>();    
    RaycastHit hit;

    bool dragSelect; // defines whether or not to show a box on screen

    //Collider variables
    //==================
    MeshCollider selectionBox;
    Mesh selectionMesh;

    Vector3 p1, p2;

    // the corners of our 2d selection box
    Vector2[] corners;

    // the vertices of our meshcollider
    Vector3[] verts;

    private float boxHeight = 10;

    private UnitManager unitManager;

    // properties
    public List<GameObject> Units
    {
        get {
            List<GameObject> units = new List<GameObject>();

            foreach (KeyValuePair<int, GameObject> unit in selectedTable)
                units.Add(unit.Value);     
            
            return units;
        }

    }

    /// <summary>
    /// Get the centerpoint of all selected units
    /// </summary>
    public Vector3 CenterPoint
    {
        get
        {
            Vector3 unitCenter = new Vector3();

            foreach (KeyValuePair<int, GameObject> unit in selectedTable)
            {
                unitCenter += unit.Value.transform.position;
            }
            unitCenter /= selectedTable.Count;

            return unitCenter;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        //selectedTable = GetComponent<SelectedDictionary>();
        dragSelect = false;
        unitManager = GetComponent<UnitManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gui.ButtonClicked != UnitGui.ActionChosen.Null)
            return;

        //1. when left mouse button clicked (but not released)
        if (Input.GetMouseButtonDown(0))
        {
            p1 = Input.mousePosition;
        }

        //2. while left mouse button held
        if (Input.GetMouseButton(0))
        {
            if ((p1 - Input.mousePosition).magnitude > 40)
            {
                dragSelect = true;
            }
        }

        //3. when mouse button comes up
        if (Input.GetMouseButtonUp(0))
        {
            if (dragSelect == false) //single select
            {
                Ray ray = Camera.main.ScreenPointToRay(p1);

                // dont select anything if the GUI is clicked
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    //Debug.Log("Clicked on GUI");
                    return;
                }

                if (Physics.Raycast(ray, out hit, 50000.0f, selectionLayer))
                {
                    if (Input.GetKey(KeyCode.LeftShift)) //inclusive select
                    {
                        AddSelected(hit.transform.gameObject);
                    }
                    else //exclusive selected
                    {
                        DeselectAll();
                        AddSelected(hit.transform.gameObject);

                        gui.GenerateUnitIcons(Units);
                    }
                }
                else //if we didnt hit something
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        //do nothing
                    }
                    else
                    {
                        DeselectAll();
                    }

                    gui.unitInfoPanel.SetActive(false);
                }
            }
            else //marquee select
            {
                verts = new Vector3[4];
                int i = 0;
                p2 = Input.mousePosition;

                corners = GetBoundingBox(p1, p2);

                foreach (Vector2 corner in corners)
                {
                    Ray ray = Camera.main.ScreenPointToRay(corner);

                    if (Physics.Raycast(ray, out hit, 50000.0f))// (1 << 8)))
                    {
                        verts[i] = new Vector3(hit.point.x, hit.point.y, hit.point.z);                        
                        Debug.DrawLine(Camera.main.ScreenToWorldPoint(corner), hit.point, Color.red, 1.0f);
                    }
                    i++;
                }

                if (!Input.GetKey(KeyCode.LeftShift) && gui.ButtonClicked == UnitGui.ActionChosen.Null)
                {
                    DeselectAll();
                }

                Vector3 centerPoint = GetBoxCenter();
                Vector3 halfExtents = GetHalfExtents();

                // shows the overlap box on the scene view
                if(drawDebugBox)
                    DrawOverlapBox(centerPoint, halfExtents);

                var collisions = Physics.OverlapBox(centerPoint, halfExtents, Quaternion.identity, selectionLayer);

                foreach (Collider collision in collisions)
                {
                    //Debug.Log("Selected: " + collision.gameObject.name);
                    AddSelected(collision.gameObject);
                }

                gui.GenerateUnitIcons(Units);

                Destroy(selectionBox, 0.02f);

            }//end marquee select

            dragSelect = false;

            unitManager.SetSelectedUnits(Units);

            // if only one unit is selected then display the unit stats/info
            //if (Units.Count == 1)
                //gui.SelectSingleUnit(Units[0].GetComponent<>);
        }

    }

    private void OnGUI()
    {
        if (dragSelect == true)
        {
            var rect = Utils.GetScreenRect(p1, Input.mousePosition);
            Utils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
            Utils.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
        }
    }

    /// <summary>
    /// Gets an instant ID from game object and addes it to the unit selection
    /// if it is not already in there
    /// </summary>
    /// <param name="go">The unit to add to the selection</param>
    public void AddSelected(GameObject go)
    {
        int id = go.GetInstanceID();

        // check if the game object is already in the dictionary
        if (!(selectedTable.ContainsKey(id)))
        {
            selectedTable.Add(id, go);

            if (go.GetComponent<UnitController>() != null)
            {
                go.GetComponent<UnitController>().SetSelected(true);
                //Debug.Log("Added " + id + " to selected dict");
            }
        }
    }

    /// <summary>
    /// Removes a specific unit from the selection specified by an id
    /// </summary>
    /// <param name="id">The instance id of the game object to be deselected</param>
    public void Deselect(int id)
    {
        //Destroy(selectedTable[id].GetComponent<SelectionComponent>());
        selectedTable[id].GetComponent<UnitController>().SetSelected(false);
        selectedTable.Remove(id);
    }

    /// <summary>
    /// Remove all units from the selection
    /// </summary>
    public void DeselectAll()
    {
        // looks trhough every value in the dictionary and if it is
        // not null, destroys it
        foreach (KeyValuePair<int, GameObject> pair in selectedTable)
        {
            if (pair.Value != null)
            {
                //Destroy(selectedTable[pair.Key].GetComponent<SelectedDictionary>());
                selectedTable[pair.Key].GetComponent<UnitController>().SetSelected(false);
            }
        }
        selectedTable.Clear(); // clears the whole dictionary

        gui.ClearUnitSelection();
    }

    // create a bounding vox (4 corners in order) from the start and end mouse position
    Vector2[] GetBoundingBox(Vector2 p1, Vector2 p2)
    {
        // Min and Max to get 2 corners of rectangle regardless of drag direction.
        var bottomLeft = Vector3.Min(p1, p2);
        var topRight = Vector3.Max(p1, p2);

        // 0 = top left; 1 = top right; 2 = bottom left; 3 = bottom right;
        Vector2[] corners =
        {
            new Vector2(bottomLeft.x, topRight.y),
            new Vector2(topRight.x, topRight.y),
            new Vector2(bottomLeft.x, bottomLeft.y),
            new Vector2(topRight.x, bottomLeft.y)
        };
        return corners;

    }

    private Vector3 GetBoxCenter()
    {
        // 0 = top left; 1 = top right; 2 = bottom left; 3 = bottom right;
        float width = verts[1].x - verts[0].x;
        float height = verts[0].z - verts[2].z;

        Vector3 centerPoint;
        centerPoint.x = verts[0].x + (width / 2);
        centerPoint.y = boxHeight / 2;
        centerPoint.z = verts[2].z + (height / 2);     

        return centerPoint;
    }

    private Vector3 GetHalfExtents()
    {
        Vector3 halfExtents = new Vector3();

        halfExtents.x = Mathf.Abs(verts[1].x - verts[0].x) / 2;
        halfExtents.y = boxHeight / 2;
        halfExtents.z = Mathf.Abs(verts[2].z - verts[0].z) / 2;

        return halfExtents;
    }

    private void DrawOverlapBox(Vector3 centerPoint, Vector3 HalfExtents)
    {
        Vector3 p1, p2, p3, p4, p5, p6, p7, p8;

        // Top
        p1.x = centerPoint.x - HalfExtents.x;
        p1.y = centerPoint.y + HalfExtents.y;
        p1.z = centerPoint.z + HalfExtents.z;

        p2.x = centerPoint.x + HalfExtents.x;
        p2.y = centerPoint.y + HalfExtents.y;
        p2.z = centerPoint.z + HalfExtents.z;

        p3.x = centerPoint.x - HalfExtents.x;
        p3.y = centerPoint.y + HalfExtents.y;
        p3.z = centerPoint.z - HalfExtents.z;

        p4.x = centerPoint.x + HalfExtents.x;
        p4.y = centerPoint.y + HalfExtents.y;
        p4.z = centerPoint.z - HalfExtents.z;

        // bottom
        p5.x = centerPoint.x - HalfExtents.x;
        p5.y = centerPoint.y - HalfExtents.y;
        p5.z = centerPoint.z + HalfExtents.z;

        p6.x = centerPoint.x + HalfExtents.x;
        p6.y = centerPoint.y - HalfExtents.y;
        p6.z = centerPoint.z + HalfExtents.z;

        p7.x = centerPoint.x - HalfExtents.x;
        p7.y = centerPoint.y - HalfExtents.y;
        p7.z = centerPoint.z - HalfExtents.z;

        p8.x = centerPoint.x + HalfExtents.x;
        p8.y = centerPoint.y - HalfExtents.y;
        p8.z = centerPoint.z - HalfExtents.z;

        // Draw lines
        // top
        Debug.DrawLine(p1, p2, Color.yellow, 1.0f);
        Debug.DrawLine(p2, p4, Color.yellow, 1.0f);
        Debug.DrawLine(p4, p3, Color.yellow, 1.0f);
        Debug.DrawLine(p3, p1, Color.yellow, 1.0f);

        // bottom
        Debug.DrawLine(p5, p6, Color.yellow, 1.0f);
        Debug.DrawLine(p6, p8, Color.yellow, 1.0f);
        Debug.DrawLine(p8, p7, Color.yellow, 1.0f);
        Debug.DrawLine(p7, p5, Color.yellow, 1.0f);

        // verticles
        Debug.DrawLine(p1, p5, Color.yellow, 1.0f);
        Debug.DrawLine(p2, p6, Color.yellow, 1.0f);
        Debug.DrawLine(p3, p7, Color.yellow, 1.0f);
        Debug.DrawLine(p4, p8, Color.yellow, 1.0f);
    }

}
