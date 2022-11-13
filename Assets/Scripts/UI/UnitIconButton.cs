using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitIconButton : MonoBehaviour
{
    public int IconIndex { get; set; }

    UnitGui gui;

    public void Start()
    {
        gui = UnitGui.Instance;
        
    }

    public void SelectSingleUnit()
    {
        SelectionManager.Instance.enabled = false;
        gui.SelectSingleUnit(IconIndex);
        SelectionManager.Instance.enabled = true;
    }

}
