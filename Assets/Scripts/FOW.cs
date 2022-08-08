using UnityEngine;
using System.Collections.Generic;


/* NOTE (George):
 *
 * A problem I can see with this script is performance.
 *
 * The code to re-generate the mesh using marching squares
 * isn't exactly cheap, and it will need to happen once per
 * frame in a likely worse-case scenario.
 *
 * Maybe the mesh regeneration code should be implemented
 * in C++ and loaded from a DLL? We shall see if the C#
 * version performs well enough, and maybe run tests to
 * see if that's actually worth the extra effort. */

[RequireComponent(typeof(MeshFilter))]
class FOW : MonoBehaviour {
	struct MaskRect {
		Vector2Int position;
		Vector2Int extent;
	}

	[SerializeField] Vector2Int maskExtent;
	[SerializeField] Vector2 cellSize = new Vector2(1.0f, 1.0f);
	[SerializeField] Vector3 offset;
	[SerializeField] Texture2D maskTexture;

	[SerializeField] GameObject cubePrefab;

	Mesh mesh;

	Vector3[] vertices;
	int[] tris;

	byte[] FOWMask;

	void Start() {
		FOWMask = new byte[maskExtent.x * maskExtent.y];

		SetMaskFromTexture(maskTexture);

		mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;

		GenerateMesh();
	}

	void Update() {
	}

	public void RequestMaskUpdate(byte value, Vector2Int position) {
		FOWMask[position.x + position.y * maskExtent.x] = value;
	}

	public void SetMaskFromTexture(Texture2D texture) {
		/* Texture.GetPixel is incredibly slow.
		 *
		 * This function should be used *very* sparingly, if at
		 * all in the final game. */

		int width = Mathf.Min(maskExtent.x, texture.width);
		int height = Mathf.Min(maskExtent.y, texture.height);

		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				Color pixel = texture.GetPixel(x, y);

				if (pixel == Color.black) {
					FOWMask[x + y * width] = 0x1;
				} else {
					FOWMask[x + y * width] = 0x0;
				}
			}
		}
	}

	public void GenerateMesh() {
		List<Vector3> vertices = new List<Vector3>();
		List<int> tris = new List<int>();

		int index_offset = 0;
		for (int y = 0; y < maskExtent.y; y++) {
			for (int x = 0; x < maskExtent.x; x++) {
				int idx = x + y * maskExtent.x;

				if (FOWMask[idx] == 0x0) { continue; }

			//	Debug.Log(x.ToString() + ", " + y.ToString());

				vertices.Add(new Vector3((float)x,        0.0f, (float)y));
				vertices.Add(new Vector3((float)x,        0.0f, (float)y + 1.0f));
				vertices.Add(new Vector3((float)x + 1.0f, 0.0f, (float)y));
				vertices.Add(new Vector3((float)x + 1.0f, 0.0f, (float)y + 1.0f));

				tris.Add(index_offset + 0);
				tris.Add(index_offset + 1);
				tris.Add(index_offset + 2);
				tris.Add(index_offset + 1);
				tris.Add(index_offset + 3);
				tris.Add(index_offset + 2);

				index_offset += 4;
			}
		}

		mesh.Clear();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = tris.ToArray();
		mesh.RecalculateNormals();
	}
}
