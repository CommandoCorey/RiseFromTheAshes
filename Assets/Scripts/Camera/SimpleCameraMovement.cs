using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCameraMovement : MonoBehaviour
{
    public float panSpeed = 10.0f;
    public float zoomSpeed = 5.0f;

    private Vector3 velocity;
    Vector3 xVelocity, zVelocity;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        xVelocity = Vector3.right * Input.GetAxisRaw("Horizontal") * panSpeed;
        zVelocity = Vector3.forward * Input.GetAxisRaw("Vertical") * panSpeed;

        velocity = xVelocity + zVelocity;

        transform.position += velocity * Time.deltaTime;
    }
}
