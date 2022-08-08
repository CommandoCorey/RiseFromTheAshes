using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlapBoxCheck : MonoBehaviour
{
    public float boxSize = 100;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 center = new Vector3(0, 0, 0);
        Vector3 halfExtents = new Vector3(boxSize / 2, boxSize / 2, boxSize / 2);

        var collisions = Physics.OverlapBox(center, halfExtents);

        foreach (var collision in collisions)
        {
            Debug.Log(collision.gameObject.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
