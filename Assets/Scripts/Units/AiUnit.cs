using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitMode
{
    Offensive, Intermediate, Defensive
}

public class AiUnit : MonoBehaviour
{
    [Header("Flash Highlight")]
    public GameObject AttackedHighlight;
    [SerializeField] float fadeSpeed = 8;
    [SerializeField] int iterations = 3;

    public UnitMode Mode { get; set; } = UnitMode.Intermediate;

    public bool FlashSelection = false;

    AiPlayer ai;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
