using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class GameManager : MonoBehaviour
{
    public GameObject minimap;
    public bool showMinimap = false;
    public Transform marker;

    // Start is called before the first frame update
    void Start()
    {
        if (showMinimap)
            minimap.SetActive(true);

        marker.GetComponent<MeshRenderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Moves the marker gameobject to a specified location
    /// </summary>
    /// <param name="position">The position on the map that the user clicked on</param>
    public void SetMarkerLocation(Vector3 position)
    {
        marker.transform.position = position;
        marker.GetComponent<MeshRenderer>().enabled = true;
    }

    public bool IsLayerInMask(int layer, LayerMask layerMask)
    {
        return layerMask == (layerMask | (1 << layer));
    }

}
