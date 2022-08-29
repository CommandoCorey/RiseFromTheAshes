using UnityEngine;

public class FOWManager : MonoBehaviour
{
	static public FOWManager Instance { get; set; }

	[SerializeField] public FOW perm;
	[SerializeField] public FOW imperm;

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
