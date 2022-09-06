using UnityEngine;

public class FOWManager : MonoBehaviour
{
	static public FOWManager Instance { get; set; }

	[Tooltip("The cloudy part.")]
	[SerializeField] public FOW perm;
	[Tooltip("The cloudless part that doesn't occlude environment.")]
	[SerializeField] public FOW imperm;

	[Tooltip("Objects on this layer will be occluded by the impermanent fog of war.")]
	[SerializeField] public LayerMask affectedLayer;

	void Awake() {
		if (Instance != null && Instance != this) {
			Destroy(this);
		} else {
			Instance = this;
		}
	}

	public static void DrawCircle(Vector3 position, int radius) {
		Instance.perm.MaskDrawCircle(Instance.perm.WorldPosToMaskPos(position), radius);
		Instance.imperm.MaskDrawCircle(Instance.imperm.WorldPosToMaskPos(position), radius);
	}
}
