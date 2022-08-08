using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitController : MonoBehaviour
{
    public GameObject highlight;  

    [SerializeField] float maxHealth = 100;

    [SerializeField]
    private float health;

    [SerializeField] float movementSpeed = 100;

    private NavMeshPath path;
    private Rigidbody body;
    private int waypointNum;
    private Vector3 waypoint;

    private bool moving = false;

    // Start is called before the first frame update
    void Start()
    {
        path = new NavMeshPath();
        body = GetComponent<Rigidbody>();

        health = maxHealth;
        waypointNum = 1;
    }

    // Update is called once per frame
    void Update()
    {        
        if(health <= 0)
        {
            GameObject.Destroy(this.gameObject);
        }

        if (moving)
        {
            for (int i = 0; i < path.corners.Length - 1; i++)
                Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);

            MoveTank();
        }

    }


    public void SubtractHealth(float amount)
    {
        health -= amount;
    }

    public void MoveTo(Vector3 position)
    {
        NavMesh.CalculatePath(transform.position, position, NavMesh.AllAreas, path);
        waypoint = path.corners[waypointNum];

        moving = true;

        //foreach (Vector3 coord in path.corners)
            //Debug.Log("Position: " + coord);    
    }

    public void SetSelected(bool selected)
    {
        highlight.SetActive(selected);
    }

    private void MoveTank()
    {
        Vector3 direction = (waypoint - transform.position).normalized;

        transform.forward = direction; // face moving direction
        body.velocity = direction * movementSpeed * Time.deltaTime; // moves the rigid body

        highlight.transform.transform.position = transform.position;

        // check if at waypoint
        if ((waypoint - transform.position).magnitude < 0.2f)
        {
            waypointNum++;

            if (waypointNum < path.corners.Length)
                waypoint = path.corners[waypointNum];
            else
            {
                moving = false;
                waypointNum = 1;
                body.velocity = Vector3.zero;
            }
        }

    }

}
