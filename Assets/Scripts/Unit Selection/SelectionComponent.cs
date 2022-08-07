using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionComponent : MonoBehaviour
{
    public GameObject highlight;

    // Start is called before the first frame update
    void Start()
    {
        //GetComponentInChildren<Renderer>().material.color = Color.red;
        //highlight.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        //GetComponentInChildren<Renderer>().material.color = Color.white;
        highlight.SetActive(false);
    }

    public void SetHighlight(bool active)
    {
        highlight.SetActive(active);
    }

}
