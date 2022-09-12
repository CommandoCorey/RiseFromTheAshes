using UnityEngine;

public class Easings {
	public static float EaseInOutCirc(float x) {
		return x < 0.5f
			? (1.0f - Mathf.Sqrt(1.0f - Mathf.Pow(2.0f * x, 2.0f))) / 2.0f :
			(Mathf.Sqrt(1.0f - Mathf.Pow(-2.0f * x + 2.0f, 2.0f)) + 1.0f) / 2.0f;
	}
}
