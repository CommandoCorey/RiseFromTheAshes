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
		if (FOWMask == null) { FOWMask = new byte[maskExtent.x * maskExtent.y]; }

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

	bool GetMask(int x, int y)
	{
		return FOWMask[x + y * maskExtent.x] == 0x0 ? true : false;
	}

	public void GenerateMesh() {
	}

	private void OnDrawGizmos()
	{
		/*		List<Vector3> vertices = new List<Vector3>();
				List<int> tris = new List<int>();

				int index_offset = 0;
				for (int y = 0; y < maskExtent.y; y++) {
					for (int x = 0; x < maskExtent.x; x++) {
						int idx = x + y * maskExtent.x;

						if (FOWMask[idx] == 0x0) { continue; }

						vertices.Add(new Vector3((float)x - cellSize.x * 0.5f, 0.0f, (float)y - cellSize.y * 0.5f));
						vertices.Add(new Vector3((float)x - cellSize.x * 0.5f, 0.0f, (float)y - cellSize.y * 0.5f));
						vertices.Add(new Vector3((float)x + cellSize.x * 0.5f, 0.0f, (float)y + cellSize.y * 0.5f));
						vertices.Add(new Vector3((float)x + cellSize.x * 0.5f, 0.0f, (float)y + cellSize.y * 0.5f));

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
				mesh.RecalculateNormals(); */

		/* TODO (George): Get rid of this. */
		if (!Application.isPlaying) { return; }

		int endY = maskExtent.y - 1;
		int endX = maskExtent.x - 1;
		for (int y = 0; y < endY; y++)
		{
			for (int x = 0; x < endX; x++)
			{
				bool topLeft  = GetMask(x, y);
				bool topRight = GetMask(x + 1, y);
				bool botLeft  = GetMask(x, y + 1);
				bool botRight = GetMask(x + 1, y + 1);

				Vector2 pos = new Vector2((float)x * cellSize.x, (float)y * cellSize.y);
				Vector2 hcs = cellSize / 2.0f;

				/* I hate this.
				 *
				 * TODO (George): Make an ID from the combination of topLeft, topRight, botLeft, botRight
				 * and use it as an index into an array to get the state. Be faster, hopefully.
				 *
				 * TODO (George): Blur the mask and lerp between the values for better smoothing. */
				int state = 0;

				Gizmos.color = Color.white;

				if (!topLeft && !topRight && !botLeft && !botRight)
				{
					state = 0;
				} else if (!topLeft && !topRight && botLeft && !botRight)
				{
					Gizmos.DrawLine(new Vector3(pos.x, 0.0f, pos.y + hcs.y), new Vector3(pos.x + hcs.x, 0.0f, pos.y + cellSize.y));
					state = 1;
				} else if (!topLeft && !topRight && !botLeft && botRight)
				{
					Gizmos.DrawLine(new Vector3(pos.x + cellSize.x, 0.0f, pos.y + hcs.y), new Vector3(pos.x + hcs.x, 0.0f, pos.y + cellSize.y));
					state = 2;
				} else if (!topLeft && !topRight && botLeft && botRight)
				{
					Gizmos.DrawLine(new Vector3(pos.x, 0.0f, pos.y + hcs.y), new Vector3(pos.x + cellSize.x, 0.0f, pos.y + hcs.y));
					state = 3;
				} else if (!topLeft && topRight && !botLeft && !botRight)
				{
					Gizmos.DrawLine(new Vector3(pos.x + hcs.x, 0.0f, pos.y), new Vector3(pos.x + cellSize.x, 0.0f, pos.y + hcs.y));
					state = 4;
				} else if (!topLeft && topRight && botLeft && !botRight)
				{
					state = 5;
				} else if (!topLeft && topRight && !botLeft && botRight)
				{
					Gizmos.DrawLine(new Vector3(pos.x + hcs.x, 0.0f, pos.y), new Vector3(pos.x + hcs.x, 0.0f, pos.y + cellSize.y));
					state = 6;
				} else if (!topLeft && topRight && botLeft && botRight)
				{
					Gizmos.DrawLine(new Vector3(pos.x + hcs.x, 0.0f, pos.y), new Vector3(pos.x, 0.0f, pos.y + hcs.x));
					state = 7;
				} else if (topLeft && !topRight && !botLeft && !botRight)
				{
					Gizmos.DrawLine(new Vector3(pos.x + hcs.x, 0.0f, pos.y), new Vector3(pos.x, 0.0f, pos.y + hcs.x));
					state = 8;
				} else if (topLeft && !topRight && botLeft && !botRight)
				{
					Gizmos.DrawLine(new Vector3(pos.x + hcs.x, 0.0f, pos.y), new Vector3(pos.x + hcs.x, 0.0f, pos.y + cellSize.y));
					state = 9;
				} else if (topLeft && !topRight && !botLeft && botRight)
				{
					state = 10;
				} else if (topLeft && !topRight && botLeft && botRight)
				{
					Gizmos.DrawLine(new Vector3(pos.x + hcs.x, 0.0f, pos.y), new Vector3(pos.x + cellSize.x, 0.0f, pos.y + hcs.y));
					state = 11;
				} else if (topLeft && topRight && !botLeft && !botRight)
				{
					Gizmos.DrawLine(new Vector3(pos.x, 0.0f, pos.y + hcs.y), new Vector3(pos.x + cellSize.x, 0.0f, pos.y + hcs.y));
					state = 12;
				} else if (topLeft && topRight && botLeft && !botRight)
				{

					Gizmos.DrawLine(new Vector3(pos.x + cellSize.x, 0.0f, pos.y + hcs.y), new Vector3(pos.x + hcs.x, 0.0f, pos.y + cellSize.y));
					state = 13;
				} else if (topLeft && topRight && !botLeft && botRight)
				{
					Gizmos.DrawLine(new Vector3(pos.x, 0.0f, pos.y + hcs.y), new Vector3(pos.x + hcs.x, 0.0f, pos.y + cellSize.y));
					state = 14;
				} else if (topLeft && topRight && botLeft && botRight)
				{
					state = 15;
				}
			}
		}
	}
}
