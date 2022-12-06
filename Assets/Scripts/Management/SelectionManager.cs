using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionManager : MonoBehaviour
{
    [Header("Layer Masks")]
    public LayerMask unitSelection;
    public LayerMask buildingSelectionLayer;
    public LayerMask enemyUnitSelection;
    public LayerMask enemyBuildingSelection;

    public bool drawDebugBox = true;
    public UnitGui gui; // used to update unit icons

    [Header("GUI Items")]
    public TextMeshProUGUI tooltipText;

    // contains all of the selected units
    private Dictionary<int, GameObject> selectedTable = new Dictionary<int, GameObject>();    
    RaycastHit hit;

    bool dragSelect; // defines whether or not to show a box on screen
    bool overSelectableObject = false;

    Building selectedBuilding;

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

    private GameManager gameManager;
    private UnitManager unitManager;

    // properties
    public static SelectionManager Instance { get; private set; }
    public Building SelectedBuilding { get => selectedBuilding; }

    /// <summary>
    /// Returns a list of unit gameobjects in the selection
    /// </summary>
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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //selectedTable = GetComponent<SelectedDictionary>();
        dragSelect = false;
        unitManager = GetComponent<UnitManager>();
        gameManager = GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gui.ButtonClicked != UnitGui.ActionChosen.Null)
            return;

        if(gameManager.enableCursorChanges)        
            HoverOverObject();        

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
            if (dragSelect == false)
            {
                SingleSelect();
            }
            else 
            {
                MarqueeSelect();
            }

            dragSelect = false;

            unitManager.SetSelectedUnits(Units);

            if (Units.Count > 0)
                tooltipText.gameObject.SetActive(false);

            // if only one unit is selected then display the unit stats/info
            if (Units.Count == 1)
                gui.SelectSingleUnit(0);            

        }

    }


    #region public functions
    // Not currently used
    /// <summary>
    /// Removes a specific unit from the selection specified by an id
    /// </summary>
    /// <param name="id">The instance id of the game object to be deselected</param>
    public void Deselect(int id)
    {
        var unit = selectedTable[id].GetComponent<UnitController>();

        unit.SetSelected(false);
        unit.SingleSelected = false;

        selectedTable.Remove(id);

        //unitManager.SetTargetHighlight(unit, false);       
    }

    public void SetSelectedBuilding(Building building)
    {
        ClearBuildingSelection();
        selectedBuilding = building;
        selectedBuilding.selectionHighlight.SetActive(true);

        var sprites = selectedBuilding.GetComponent<SelectionSprites>();
        sprites.SetSelectedSprite(true);
    }

    public void SetPanelTooltip(bool visible)
    {
        tooltipText.gameObject.SetActive(visible);
    }
    #endregion

    #region private functions
    private void HoverOverObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 50000.0f))
        {
            int layer = hit.transform.gameObject.layer;

            if ((layer >= 6 && layer <= 9) || layer == 22)
            {                
                var events = EventSystem.current;                

                if (!overSelectableObject)
                {
                    gameManager.SetCursor(gameManager.selectableCursor);
                    overSelectableObject = true;
                }
            }
            else if (overSelectableObject)
            {
                gameManager.SetCursor(gameManager.defaultCursor);
                overSelectableObject = false;
            }

        }

    }

    private void SingleSelect()
    {
        Ray ray = Camera.main.ScreenPointToRay(p1);

        /*
        if(Physics.Raycast(ray, out hit, 50000.0f))
        {
            Debug.Log("Hit: " + hit.transform.name + " at " + hit.point);
        }*/

        unitManager.DeselectEnemyUnit();

        // clear the building selection if the player clicked on something other than the building
        if (Physics.Raycast(ray, out hit, 50000.0f))
        {
            if (1 << hit.transform.gameObject.layer != buildingSelectionLayer.value &&
                hit.transform.gameObject.layer != 5) // UI layer
                ClearBuildingSelection();
        }

        if (Physics.Raycast(ray, out hit, 50000.0f, unitSelection))
        {
            if (Input.GetKey(KeyCode.LeftShift)) //inclusive select
            {
                AddSelected(hit.transform.gameObject);

                var lastUnit = selectedTable.Values.Last().GetComponent<UnitController>();
                lastUnit.SingleSelected = false;

                gui.GenerateUnitIcons(Units);
            }
            else //exclusive selected
            {
                var unit = hit.transform.GetComponent<UnitController>();

                DeselectAll();
                AddSelected(hit.transform.gameObject);
                unit.SingleSelected = true;

                gui.GenerateUnitIcons(Units);
            }

        }
        else if (Physics.Raycast(ray, out hit, 50000.0f, enemyUnitSelection))
        {
            DeselectAll(); // removes all player untis from selection

            var unit = hit.transform.GetComponent<UnitController>();
            unit.SingleSelected = true;

            unitManager.SelectEnemyUnit(unit);
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

    private void MarqueeSelect()
    {
        verts = new Vector3[4];
        int i = 0;
        p2 = Input.mousePosition;

        corners = GetBoundingBox(p1, p2);

        foreach (Vector2 corner in corners)
        {
            Ray ray = Camera.main.ScreenPointToRay(corner);

            if (Physics.Raycast(ray, out hit, 1000000.0f))// (1 << 8)))
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
        if (drawDebugBox)
            DrawOverlapBox(centerPoint, halfExtents);

        var collisions = Physics.OverlapBox(centerPoint, halfExtents, Quaternion.identity, unitSelection);

        if(collisions.Length == 1)
        {
            collisions[0].GetComponent<UnitController>().SingleSelected = true;
        }

        foreach (Collider collision in collisions)
        {
            //Debug.Log("Selected: " + collision.gameObject.name);
            AddSelected(collision.transform.gameObject);
        }

        gui.GenerateUnitIcons(Units);

        Destroy(selectionBox, 0.02f);
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
    private void AddSelected(GameObject go)
    {
        int id = go.GetInstanceID();

        // check if the game object is already in the dictionary
        if (!(selectedTable.ContainsKey(id)))
        {
            selectedTable.Add(id, go);

            var unit = go.GetComponent<UnitController>();

            if (unit != null)
            {
                unit.SetSelected(true);

                //Debug.Log("Added " + id + " to selected dict");
            }
        }

        gui.EnableActionButtons(); // turns on move, attack and halt
    }

    /// <summary>
    /// Remove all units from the selection
    /// </summary>
    private void DeselectAll()
    {
        // looks trhough every value in the dictionary and if it is
        // not null, destroys it
        foreach (KeyValuePair<int, GameObject> pair in selectedTable)
        {
            if (pair.Value != null)
            {
                var unit = selectedTable[pair.Key].GetComponent<UnitController>();

                //Destroy(selectedTable[pair.Key].GetComponent<SelectedDictionary>());
                unit.SingleSelected=false;
                unit.SetSelected(false);

                //if(unit.AttackTarget != null)
                    //unitManager.SetTargetHighlight(unit, false);
            }
        }
        selectedTable.Clear(); // clears the whole dictionary

        gui.ClearUnitSelection();
        gui.DisableActionButtons();

        unitManager.DeselectEnemyUnit();

        //tooltipText.gameObject.SetActive(true);
    }   
    

    private void ClearBuildingSelection()
    {
        if (selectedBuilding != null)
        {
            selectedBuilding.selectionHighlight.SetActive(false);
            selectedBuilding.GetComponent<SelectionSprites>().SetSelectedSprite(false);

            var vehiclebay = selectedBuilding.GetComponent<VehicleBay>();
            if (vehiclebay != null)
                vehiclebay.HideMenu();

            selectedBuilding = null;
        }
    }

    // create a bounding vox (4 corners in order) from the start and end mouse position
    private Vector2[] GetBoundingBox(Vector2 p1, Vector2 p2)
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
    #endregion
}
