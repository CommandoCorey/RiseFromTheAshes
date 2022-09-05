using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Camera))]
public class FOWCamera : MonoBehaviour {
	Camera myCamera;

	Camera affectedCamera;

	public LayerMask FOWAffectedLayerMask;

	[HideInInspector] public RenderTexture FOWAffectedRenderTexture;

	void Start() {
		myCamera = GetComponent<Camera>();

		GameObject affectedCameraGO = new GameObject();
		affectedCameraGO.transform.parent = this.gameObject.transform;

		/* Because Unity tried to be clever :( */
		affectedCameraGO.transform.localPosition = Vector3.zero;
		affectedCameraGO.transform.localRotation = Quaternion.identity;
		affectedCameraGO.transform.localScale = Vector3.zero;

		affectedCameraGO.AddComponent<Camera>();

		affectedCamera = affectedCameraGO.GetComponent<Camera>();
		affectedCamera.cullingMask = FOWAffectedLayerMask;
		affectedCamera.fieldOfView = myCamera.fieldOfView;

		UniversalAdditionalCameraData uacd = affectedCamera.GetUniversalAdditionalCameraData();
		uacd.renderPostProcessing = false;
		uacd.SetRenderer(1);

		FOWAffectedRenderTexture = new RenderTexture(Screen.width, Screen.height, 32, RenderTextureFormat.Default);

		affectedCamera.targetTexture = FOWAffectedRenderTexture;
		affectedCamera.depthTextureMode = DepthTextureMode.Depth;
		affectedCamera.clearFlags = CameraClearFlags.SolidColor;
		affectedCamera.backgroundColor = new Color(0, 0, 0, 0);
	}

	void Update() {
		if (Screen.width != FOWAffectedRenderTexture.width || Screen.height != FOWAffectedRenderTexture.height) {
			FOWAffectedRenderTexture.Release();
			FOWAffectedRenderTexture.width  = Screen.width;
			FOWAffectedRenderTexture.height = Screen.height;
			FOWAffectedRenderTexture.Create();
		}
	}
}
