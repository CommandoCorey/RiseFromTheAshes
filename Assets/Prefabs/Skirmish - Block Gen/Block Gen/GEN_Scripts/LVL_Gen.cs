using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LVL_Gen : MonoBehaviour
{
    public GameObject blockGameObject;
    //spawn single
    //Make the Space

    [Space(10)]

    public int worldsizeX = 3;
    public int worldsizeY = 3;

    private int GridOffset = 40;

    // Start is called before the first frame update
    void Start()
    {
        // generate the Empty Game Objects (called Pieces) to make them in a grid line
        for (int x = 0; x < worldsizeX; x++)
        {
            for (int z = 0; z < worldsizeY; z++)
            {
                Vector3 pos = new Vector3(x * GridOffset, 0, z * GridOffset);

                GameObject block = Instantiate(blockGameObject, pos, Quaternion.identity) as GameObject;

                block.transform.SetParent(this.transform);
            }
        }
    }
}
