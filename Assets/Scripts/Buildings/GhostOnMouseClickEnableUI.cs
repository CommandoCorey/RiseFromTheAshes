using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostOnMouseClickEnableUI : MonoBehaviour
{
    public GameObject GhostGUI;
    public GameObject BuildMenu;
    public GameObject VehicleBay;
    public GameObject Outpost;

    public void EnableBuildMenu()
    {
        BuildMenu.SetActive(true);
    }
    public void EnableVehicleBay()
    {
        VehicleBay.SetActive(true);
        GhostGUI.SetActive(false);
    }
    public void EnableOutpost()
    {
        Outpost.SetActive(true);
        GhostGUI.SetActive(false);
    }
    public void CloseBuildMenu()
    {
        BuildMenu.SetActive(false);
    }
}
