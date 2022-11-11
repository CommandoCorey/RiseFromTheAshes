using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateHighlight : MonoBehaviour
{
    [SerializeField][Range(10, 200)]
    float rotationSpeed = 100;

    private GameObject targetedHighlight;
    private float highlightAngle = 0;

    // Start is called before the first frame update
    void Start()
    {
        if(gameObject.layer == 7)
            targetedHighlight = GetComponent<UnitController>().targetedHighlight;
        else if(gameObject.layer == 9)
            targetedHighlight = GetComponent<Building>().targetedHighlight;
    }

    // Update is called once per frame
    void Update()
    {
        if (targetedHighlight != null)
        {
            highlightAngle += rotationSpeed * Time.deltaTime;

            if (highlightAngle >= 360)
                highlightAngle = 0;

            targetedHighlight.transform.rotation = Quaternion.Euler(90, highlightAngle, 0);
        }
    }
}
