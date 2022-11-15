using UnityEngine;
using System.Collections.Generic;

public class CameraController : MonoBehaviour {
	[SerializeField] float zoomedInMouseSensitivity = 0.001f;
	[SerializeField] float zoomedOutMouseSensitivity = 0.01f;
	[SerializeField] float zoomedInWASDSensitivity = 15.0f;
	[SerializeField] float zoomedOutWASDSensitivity = 30.0f;
	[SerializeField] float zoomedInWASDSensitivitySprint = 20.0f;
	[SerializeField] float zoomedOutWASDSensitivitySprint = 40.0f;
	[SerializeField] float maxZoom = 15.0f;
	[SerializeField] float minZoom = 1.0f;
	[SerializeField] float zoomInterpSpeed = 10.0f;
	[SerializeField] BoxCollider bounds;

	float zoom;

	Vector2 mousePos;
	Vector2 oldMousePos;
	Vector2 mouseDelta;

	Vector2 velocity;
	Queue<Vector2> velocities = new Queue<Vector2>();
	[SerializeField] float friction = 100.0f;

	[Header("Edge scrolling.")]
	[SerializeField] [Tooltip("The size of the areas that are to trigger scrolling, in pixels.")] float edgeSize = 30.0f;
	[SerializeField] float zoomedInEdgeScrollSpeed = 15.0f;
	[SerializeField] float zoomedOutEdgeScrollSpeed = 30.0f;
	[SerializeField] bool enableEdgeScrolling = true;

	bool firstMove;
	bool moving;

	float startPy;
	float targetPy;
	float zoomTime;

	private void Start()
	{
		firstMove = true;
		zoom = 0.0f;
		zoomTime = 0.0f;
		startPy = minZoom;
		targetPy = maxZoom;
	}

	private void Update()
	{
		float timestep = Time.deltaTime / GameManager.Instance.timeScale;

		Vector2 mp = Input.mousePosition;

		if (Input.GetMouseButtonDown(2))
		{
			firstMove = true;
			moving = true;
			Cursor.visible = false;
		}

		zoom += Input.mouseScrollDelta.y;
		zoom = Mathf.Clamp(zoom, minZoom, maxZoom);

		float zoomPerc = Utils.Map(zoom, minZoom, maxZoom, 0.0f, 1.0f);

		if (Input.mouseScrollDelta.y != 0.0f) {
			zoomTime = 0.0f;
			startPy = transform.position.y;
			targetPy -= Input.mouseScrollDelta.y;
			targetPy = Mathf.Clamp(targetPy, minZoom, maxZoom);
		}

		if (zoomTime < 1.0)
		{
			zoomTime += timestep * zoomInterpSpeed;
		}

		zoom = Mathf.Lerp(startPy, targetPy, zoomTime);

		transform.position = new Vector3(transform.position.x, zoom, transform.position.z);

		if (moving)
		{
			mousePos = Input.mousePosition;

			if (firstMove) {
				oldMousePos = mousePos;
				firstMove = false;
				velocities.Clear();
			}

			mouseDelta = oldMousePos - mousePos;

			Vector3 delta = new Vector3(mouseDelta.x, 0.0f, mouseDelta.y) * Mathf.Lerp(zoomedInMouseSensitivity, zoomedOutMouseSensitivity, zoomPerc);

			transform.transform.position += delta;
			
			oldMousePos = mousePos;

			velocities.Enqueue(mouseDelta);

			if (velocities.Count > 64) {
				velocities.Dequeue();
			}

			if (Input.GetMouseButtonUp(2))
			{
				moving = false;

				velocity = new Vector2(0.0f, 0.0f);

				foreach (var v in velocities)
				{
					velocity += v;
				}

				velocity /= (float)velocities.Count;

				Cursor.visible = true;
			}
		} else {
			transform.transform.position += new Vector3(velocity.x, 0.0f, velocity.y) * timestep;

			if (velocity.x > 0.0001f && velocity.y > 0.0001 || velocity.x < -0.0001 || velocity.y < -0.0001)
			{
				velocity -= new Vector2(timestep * friction, timestep * friction) * velocity;
			} else
			{
				velocity = new Vector2(0.0f, 0.0f);
			}
		}

		transform.position = new Vector3(
				Mathf.Clamp(transform.position.x, bounds.bounds.min.x, bounds.bounds.max.x),
				transform.position.y,
				Mathf.Clamp(transform.position.z, bounds.bounds.min.z, bounds.bounds.max.z)
			);

		float s1 = Input.GetKey(KeyCode.LeftShift) ? zoomedInWASDSensitivitySprint : zoomedInWASDSensitivity;
		float s2 = Input.GetKey(KeyCode.LeftShift) ? zoomedOutWASDSensitivitySprint : zoomedOutWASDSensitivity;
		float s = Mathf.Lerp(s1, s2, zoomPerc);
		Vector2 WASD = new Vector2();
		WASD.x = Input.GetAxis("Horizontal") * s;
		WASD.y = Input.GetAxis("Vertical") * s;

		if (WASD.x != 0.0f) { velocity.x = WASD.x; }
		if (WASD.y != 0.0f) { velocity.y = WASD.y; }

		/* Check the mouse pointer against a 2D box that's the same size as the screen minus
		 * the borderSize for the edge scrolling. */
		Vector2 checkBoxMin = new Vector2(edgeSize, edgeSize);
		Vector2 checkBoxMax = new Vector2(Screen.width - edgeSize, Screen.height - edgeSize);

#if UNITY_EDITOR
		/* Stop the camera from zooming around if someone is trying to do things in the editor,
		 * that is, the mouse is outside the game view. */
		if (mp.x < 0.0f || mp.y < 0.0f || mp.x > Screen.width || mp.y > Screen.height)
		{
			return;
		}
#endif

		if (enableEdgeScrolling && !moving && mp.x < checkBoxMin.x || mp.y < checkBoxMin.y || mp.x > checkBoxMax.x || mp.y > checkBoxMax.y)
		{
			Vector2 screenCentre = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
			Vector2 dir = (mp - screenCentre).normalized;

			velocity = dir * Mathf.Lerp(zoomedInEdgeScrollSpeed, zoomedOutEdgeScrollSpeed, zoomPerc);
		}
	}
}
