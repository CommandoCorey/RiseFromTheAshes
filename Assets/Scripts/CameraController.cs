using UnityEngine;
using System.Collections.Generic;

public class CameraController : MonoBehaviour {
	[SerializeField] [Range(0, 1)] List<float> zoomLevels;
	[SerializeField] int zoomLevelIndex;

	float zoomTime;
	float zoomLevel;
	[SerializeField] float zoomInterpSpeed = 2.0f;

	Vector2 zoomRange;

	[SerializeField] Camera zoomedIn;
	[SerializeField] Camera zoomedOut;

	Transform zoomedInTransform;
	Transform zoomedOutTransform;

	[SerializeField] float WASDMoveSpeed;

	[SerializeField] Transform zoomCurveHandle;

	[SerializeField] BoxCollider bounds;

	static Vector3 QuadBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, float t) {
		return p1 + Mathf.Pow((1.0f - t), 2.0f) * (p0 - p1) + Mathf.Pow(t, 2.0f) * (p2 - p1);
	}

	void Start() {
		zoomedInTransform = zoomedIn.transform;
		zoomedOutTransform = zoomedOut.transform;

		zoomedIn .gameObject.SetActive(false);
		zoomedOut.gameObject.SetActive(false);
	}

	void Update() {
		float scroll = Input.GetAxis("Mouse ScrollWheel");
		int zoomIncrease = scroll > 0.0f ? 1 : scroll < 0.0f ? -1 : 0;
		if (zoomIncrease != 0) {
			zoomLevelIndex += zoomIncrease;

			if (zoomLevelIndex < 0) { zoomLevelIndex = 0; }
			if (zoomLevelIndex > zoomLevels.Count - 1) { zoomLevelIndex = zoomLevels.Count - 1; }

			zoomRange = new Vector2(zoomLevel, zoomLevels[zoomLevelIndex]);
			zoomTime = 0.0f;
		}

		if (zoomTime < 1.0f) {
			zoomTime += Time.deltaTime * zoomInterpSpeed;
			zoomLevel = Mathf.Lerp(zoomRange.x, zoomRange.y, zoomTime);
		}

		Vector2 axis = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
		Vector2 mod = axis * WASDMoveSpeed * Time.deltaTime;
		zoomedInTransform.position += new Vector3(mod.x, 0.0f, mod.y);
		zoomedInTransform.position = new Vector3(
			Mathf.Clamp(zoomedInTransform.position.x, bounds.bounds.min.x, bounds.bounds.max.x),
			zoomedInTransform.position.y,
			Mathf.Clamp(zoomedInTransform.position.z, bounds.bounds.min.z, bounds.bounds.max.z)
		);

		transform.position = QuadBezierCurve(
			zoomedInTransform.position,
			zoomCurveHandle.position,
			zoomedOutTransform.position,
			zoomLevel);
	}

	void OnDrawGizmos() {
		/* For some reason, this causes the editor to freeze if the two cameras
		 * are thousands of units apart. Which is interesting. */
		int steps = 32;
		Vector3 prev = zoomedIn.transform.position;
		for (int i = 0; i < steps; i++) {
			Vector3 pos = QuadBezierCurve(
				zoomedIn.transform.position,
				zoomCurveHandle.position,
				zoomedOut.transform.position,
				(float)i / (float)steps);

			Gizmos.DrawLine(prev, pos);

			prev = pos;
		}

		Gizmos.DrawLine(prev, zoomedOut.transform.position);

		Gizmos.color = Color.grey;

		if (zoomCurveHandle)
		{
			Gizmos.DrawSphere(zoomCurveHandle.transform.position, 0.1f);
		}

		Gizmos.DrawLine(zoomedIn.transform.position, zoomCurveHandle.transform.position);
		Gizmos.DrawLine(zoomCurveHandle.transform.position, zoomedOut.transform.position);
	}
}
