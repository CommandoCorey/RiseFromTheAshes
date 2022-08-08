using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadParent : MonoBehaviour
{
    public GameObject target;
    public GameObject childPrefab;
    public List<GameObject> children;

    public int childrenToSpawn = 12;
    public int rows = 4;
    public float distanceMultiplier = 6.0f;
    public float relativeYPos = 0.33f;

    // Start is called before the first frame update
    void Start()
    {
        /*
        children = new List<GameObject>();

        // spawn squard children
        for(int i = 0; i < childrenToSpawn; i++)
        {
            Vector3 relativeSpawn = new Vector3(i% rows, relativeYPos, i/ rows);

            GameObject temp = Instantiate(childPrefab, transform.position + relativeSpawn * distanceMultiplier, transform.rotation);
            temp.GetComponent<Behaviour>().target = gameObject; // set squad leader as the object to follow

            children.Add(temp);
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        // moves gameobject towards target in a straight line
        transform.position += (target.transform.position - transform.position).normalized * Time.deltaTime * 5.0f;
        transform.LookAt(target.transform, Vector3.up);
        //transform.rotation = Quaternion.Euler(0, transform.rotation.y, 0);
    }
}
