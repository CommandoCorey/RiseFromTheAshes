using UnityEngine;

public class FOWManager : MonoBehaviour
{
	static FOWManager Instance { get; set; }

	[SerializeField] FOW perm;
	[SerializeField] FOW imperm;

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
