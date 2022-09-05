using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCameraMovement : MonoBehaviour
{
    public float panSpeed = 10.0f;
    public float zoomSpeed = 10.0f;

    private Vector3 velocity;
    Vector3 xVelocity, yVelocity,zVelocity;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        xVelocity = Vector3.right * Input.GetAxisRaw("Horizontal") * panSpeed;
        yVelocity = Vector3.down * Input.mouseScrollDelta.y * zoomSpeed;
        zVelocity = Vector3.forward * Input.GetAxisRaw("Vertical") * panSpeed;

        velocity = xVelocity + yVelocity + zVelocity;

        transform.position += velocity * Time.deltaTime;

    }
}
