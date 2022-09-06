using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitIconButton : MonoBehaviour
{
    public int IconIndex { get; set; }

    public void SelectSingleUnit()
    {
        //Debug.Log("You clicked on button " + IconIndex);

        GetComponentInParent<GUIManager>().SelectSingleUnit(IconIndex);
    }


}
