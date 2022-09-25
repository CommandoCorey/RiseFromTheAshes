using UnityEngine;
using System.Collections.Generic;

public class CameraController : MonoBehaviour {
	[SerializeField] float mouseSensitivity = 0.01f;
	[SerializeField] float maxZoom = 15.0f;
	[SerializeField] float minZoom = 1.0f;

	Vector2 mousePos;
	Vector2 oldMousePos;
	Vector2 mouseDelta;

	Vector2 velocity;
	Queue<Vector2> velocities = new Queue<Vector2>();
	[SerializeField] float friction = 100.0f;

	bool firstMove;
	bool moving;

	private void Start()
	{
		firstMove = true;
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(2))
		{
			firstMove = true;
			moving = true;
		}

		transform.position += new Vector3(0.0f, -Input.mouseScrollDelta.y, 0.0f);
		transform.position = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y, minZoom, maxZoom), transform.position.z);
		float zoom = transform.position.y;

		if (moving)
		{
			mousePos = Input.mousePosition;

			if (firstMove) {
				oldMousePos = mousePos;
				firstMove = false;
				velocities.Clear();
			}

			mouseDelta = oldMousePos - mousePos;

			transform.transform.position += new Vector3(mouseDelta.x, 0.0f, mouseDelta.y) * mouseSensitivity * zoom;

			oldMousePos = mousePos;

			velocities.Enqueue(mouseDelta);

			if (velocities.Count > 64) {
				velocities.Dequeue();
			}

			if (Input.GetMouseButtonUp(2))
			{
				velocity = mouseDelta;
				moving = false;

				velocity = new Vector2(0.0f, 0.0f);

				foreach (var v in velocities)
				{
					velocity += v;
				}

				velocity /= (float)velocities.Count;
			}
		}
		else {
			transform.transform.position += new Vector3(velocity.x, 0.0f, velocity.y) * Time.deltaTime;

			if (velocity.x > 0.0001f && velocity.y > 0.0001 || velocity.x < -0.0001 || velocity.y < -0.0001)
			{
				velocity -= new Vector2(Time.deltaTime * friction, Time.deltaTime * friction) * velocity;
			} else
			{
				velocity = new Vector2(0.0f, 0.0f);
			}
		}
	}
}
