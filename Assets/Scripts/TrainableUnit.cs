using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Unit", menuName = "GuiButton/TrainableUnit", order = 1)]
public class TrainableUnit : ScriptableObject
{
    public Image buttonIcon;
    public string name;
    public GameObject unitToTrain;
    public int steelCost = 0;
    [Range(0, 300)]
    public float secondsToTrain = 0;

}
