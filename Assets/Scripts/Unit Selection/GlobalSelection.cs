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

    private float boxHeight = 10;

    // Start is called before the first frame update
    void Start()
    {
        selectedTable = GetComponent<SelectedDictionary>();
        dragSelect = false;
    }

    // Update is called once per frame
    void Update()
    {
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

                if (Physics.Raycast(ray, out hit, 50000.0f, selectionLayer))
                {
                    if (Input.GetKey(KeyCode.LeftShift)) //inclusive select
                    {
                        selectedTable.AddSelected(hit.transform.gameObject);
                    }
                    else //exclusive selected
                    {
                        selectedTable.DeselectAll();
                        selectedTable.AddSelected(hit.transform.gameObject);
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
                        selectedTable.DeselectAll();
                    }
                }
            }
            else //marquee select
            {
                verts = new Vector3[4];
                vecs = new Vector3[4];
                int i = 0;
                p2 = Input.mousePosition;

                corners = GetBoundingBox(p1, p2);

                foreach (Vector2 corner in corners)
                {
                    Ray ray = Camera.main.ScreenPointToRay(corner);

                    if (Physics.Raycast(ray, out hit, 50000.0f))// (1 << 8)))
                    {
                        verts[i] = new Vector3(hit.point.x, hit.point.y, hit.point.z);
                        vecs[i] = ray.origin - hit.point;
                        Debug.DrawLine(Camera.main.ScreenToWorldPoint(corner), hit.point, Color.red, 1.0f);
                    }
                    i++;
                }

                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    selectedTable.DeselectAll();
                }

                Vector3 centerPoint = GetBoxCenter();
                Vector3 halfExtents = GetHalfExtents();

                DrawOverlapBox(centerPoint, halfExtents);

                var collisions = Physics.OverlapBox(centerPoint, halfExtents, Quaternion.identity, selectionLayer);

                foreach (Collider collision in collisions)
                {
                    //Debug.Log("Selected: " + collision.gameObject.name);
                    selectedTable.AddSelected(collision.gameObject);
                }                

                Destroy(selectionBox, 0.02f);

            }//end marquee select

            dragSelect = false;
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

    private void OnTriggerEnter(Collider other)
    {
        selectedTable.AddSelected(other.gameObject);
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
