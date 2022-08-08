using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSelection : MonoBehaviour
{
    public LayerMask selectionLayer;
    [SerializeField]
    float minBoxSize = 40;

    //id_dictionary idTable;
    SelectedDictionary selectedTable; // contains the dictionary of selected units
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
    Vector3[] vecs;

    // Start is called before the first frame update
    void Start()
    {
        selectedTable = GetComponent<SelectedDictionary>();
        dragSelect = false;
    }

    // Update is called once per frame
    void Update()
    {
        //1. when left mous button clicked
        if (Input.GetMouseButtonDown(0))
        {
            p1 = Input.mousePosition;
        }

        //2. while left mouse button held
        if (Input.GetMouseButton(0))
        {
            // check if cursor is a minimum
            if ((p1 - Input.mousePosition).magnitude > minBoxSize)
            {
                dragSelect = true;
            }
        }

        //3. when mous button comes up
        if (Input.GetMouseButtonUp(0))
        {
            if (dragSelect == false)
            {
                // cast ray from camera at cursor position
                Ray ray = Camera.main.ScreenPointToRay(p1);

                if (Physics.Raycast(ray, out hit, 50000.0f, selectionLayer))
                {
                    // if the left key is held down then perform and inclusive select
                    // where new selection is added to the current selection                    
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        selectedTable.AddSelected(hit.transform.gameObject);
                    }
                    else // exclusive select: Deslect the current selection before adding
                    {
                        selectedTable.DeselectAll();
                        selectedTable.AddSelected(hit.transform.gameObject);
                    }
                }
                else // if we didn't hit something
                {
                    // do nothing if shift is held down otherwise deselect all units
                    if (Input.GetKey(KeyCode.LeftShift))
                    {

                    }
                    else
                    {
                        selectedTable.DeselectAll();
                    }
                }

            }
        }
        else // marquee select
        {
            verts = new Vector3[4];
            vecs = new Vector3[4];
            int i = 0;
            p2 = Input.mousePosition;

            corners = GetBoundingBox(p1, p2);

            foreach (Vector2 corner in corners)
            {
                Ray ray = Camera.main.ScreenPointToRay(corner);

                if (Physics.Raycast(ray, out hit, 50000.0f, (1 << 8)))
                {
                    verts[i] = new Vector3(hit.point.x, 0, hit.point.z);
                    vecs[i] = ray.origin - hit.point;
                    Debug.DrawLine(Camera.main.ScreenToWorldPoint(corner), hit.point, Color.red, 1.0f);
                }
                i++;
            }

            // generate the mesh
            selectionMesh = generateSelectionMesh(verts);

            selectionBox = gameObject.AddComponent<MeshCollider>();
            selectionBox.sharedMesh = selectionMesh;
            selectionBox.convex = true;
            selectionBox.isTrigger = true;

            // if the shift key is not held down deselect all of the units
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                selectedTable.DeselectAll();
            }

            // removes the selection box from the screen before the next frame
            Destroy(selectionBox, 0.02f);

        } // end marquee select

        dragSelect = false;
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

    // create a bounding vox (4 corners in order) from the start and end mouse position
    Vector2[] GetBoundingBox(Vector2 p1, Vector2 p2)
    {
        Vector2 newP1;
        Vector2 newP2;
        Vector2 newP3;
        Vector2 newP4;

        if (p1.x > p2.x) // if p1 is to the left of p2
        {
            if (p1.y > p2.y) // if p1 is above p2
            {
                newP1 = p1;
                newP2 = new Vector2(p2.x, p1.y);
                newP3 = new Vector2(p1.x, p2.y);
                newP4 = p2;
            }
            else // if p1 is below p2
            {
                newP1 = new Vector2(p1.x, p2.y);
                newP2 = p2;
                newP3 = p1;
                newP4 = new Vector2(p2.x, p1.y);
            }
        }
        else
        {
            if (p1.y > p2.y) // if p1 is above p2
            {
                newP1 = new Vector2(p2.x, p1.y);
                newP2 = p1;
                newP3 = p2;
                newP4 = new Vector2(p1.x, p2.y);
            }
            else // if p1 is below p2
            {
                newP1 = p2;
                newP2 = new Vector2(p1.x, p2.y);
                newP3 = new Vector2(p2.x, p1.y);
                newP4 = p1;
            }
        }

        // create the corner positions for the bounding box and return them
        Vector2[] corners = { newP1, newP2, newP3, newP4 };
        return corners;
    }

    // generate a meshh from the 4 bottom points
    Mesh generateSelectionMesh(Vector3[] corners)
    {
        Vector3[] verts = new Vector3[8]; // the 8 vertices on a cube
        //map the triangles of our cube
        int[] tris = { 0, 1, 2, 2, 1, 3, 4, 6, 0, // top triangle 1
                       0, 6, 2, 6, 7, 2, 2, 7, 3, // top triangle 2
                       7, 5, 3, 3, 5, 1, 5, 0, 1, // bottom triangle 1
                       1, 4, 0, 4, 5, 6, 6, 5, 7 }; // bottom triangle 2

        // create vertices for bottom rectanlge
        for (int i = 0; i < 4; i++)
        {
            verts[i] = corners[i];
        }

        // create vertices for top rectangle
        for (int j = 4; j < 8; j++)
        {
            verts[j] = corners[j - 4] + Vector3.up * 100.0f;
        }

        // create mesh using the verticie and triangle data
        Mesh selectionMesh = new Mesh();
        selectionMesh.vertices = verts;
        selectionMesh.triangles = tris;

        return selectionMesh;
    }

    private void OnTriggerEnter(Collider other)
    {
        selectedTable.AddSelected(other.gameObject);
    }

}
