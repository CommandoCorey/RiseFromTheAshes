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

	byte[] FOWMask;

	List<Vector3> vertices;
	List<int> tris;

	void Start() {
		FOWMask = new byte[maskExtent.x * maskExtent.y];

		SetMaskFromTexture(maskTexture);

		mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;

		vertices = new List<Vector3>();
		tris = new List<int>();
	}

	private void Update()
	{
		GenerateMesh();
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

	void EmitTriangle(Vector2 a, Vector2 b, Vector2 c)
	{
		int indexOffset = vertices.Count;
		vertices.Add(new Vector3(a.x, 0.0f, a.y));
		vertices.Add(new Vector3(b.x, 0.0f, b.y));
		vertices.Add(new Vector3(c.x, 0.0f, c.y));

		tris.Add(indexOffset + 0);
		tris.Add(indexOffset + 1);
		tris.Add(indexOffset + 2);
	}

	void EmitQuad(Vector2 pos, Vector2 size)
	{
		int indexOffset = vertices.Count;

		vertices.Add(new Vector3(pos.x,          0.0f, pos.y + size.y));
		vertices.Add(new Vector3(pos.x + size.x, 0.0f, pos.y + size.y));
		vertices.Add(new Vector3(pos.x + size.x, 0.0f, pos.y));
		vertices.Add(new Vector3(pos.x,          0.0f, pos.y));
	
		tris.Add(indexOffset + 1);
		tris.Add(indexOffset + 2);
		tris.Add(indexOffset + 3);
		tris.Add(indexOffset + 0);
		tris.Add(indexOffset + 1);
		tris.Add(indexOffset + 3);
	}

	private void GenerateMesh()
	{
		vertices.Clear();
		tris.Clear();

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
				Vector2 hcs = cellSize * 0.5f;

				/* I hate this.
				 *
				 * TODO (George): Make an ID from the combination of topLeft, topRight, botLeft, botRight
				 * and use it as an index into an array to get the state. Be faster, hopefully.
				 *
				 * TODO (George): Blur the mask and lerp between the values for better smoothing.
				 * 
				 * This generates quite an ineffecient mesh; It doesn't care much about reusing vertices.
				 *
				 * There's currently unhandled cases, such as the two ambigous ones that exist in this
				 * algorithm. I think it's fine to leave them unhandled, though we will see once we
				 * get some actual gameplay happening. I doubt anybody will notice - it's not like there's
				 * going to be gaps in an outline or something. */
				int state = 0;

				Gizmos.color = Color.white;
				if (topLeft && topRight && botLeft && botRight)
				{
					state = 15;
				} else if (!topLeft && !topRight && !botLeft && !botRight)
				{
					EmitQuad(pos, cellSize);
					state = 0;
				} else if (!topLeft && !topRight && botLeft && !botRight)
				{
					//Debug.DrawLine(new Vector3(pos.x, 0.0f, pos.y + hcs.y), new Vector3(pos.x + hcs.x, 0.0f, pos.y + cellSize.y));
					EmitQuad(new Vector2(pos.x + hcs.x, pos.y), new Vector2(hcs.x, cellSize.y));
					EmitQuad(pos, hcs);
					EmitTriangle(
						new Vector2(pos.x + hcs.x, pos.y + cellSize.y),
						pos + hcs,
						new Vector2(pos.x, pos.y + hcs.y)
						);
					state = 1;
				} else if (!topLeft && !topRight && !botLeft && botRight)
				{
					//Debug.DrawLine(new Vector3(pos.x + cellSize.x, 0.0f, pos.y + hcs.y), new Vector3(pos.x + hcs.x, 0.0f, pos.y + cellSize.y));
					EmitQuad(pos, new Vector2(hcs.x, cellSize.y));
					EmitQuad(new Vector2(pos.x + hcs.x, pos.y), hcs);
					EmitTriangle(
						new Vector2(pos.x + cellSize.x, pos.y + hcs.y),
						pos + hcs,
						new Vector2(pos.x + hcs.x, pos.y + cellSize.y)
						);
					state = 2;
				} else if (!topLeft && !topRight && botLeft && botRight)
				{
					//Debug.DrawLine(new Vector3(pos.x, 0.0f, pos.y + hcs.y), new Vector3(pos.x + cellSize.x, 0.0f, pos.y + hcs.y));
					EmitQuad(pos, new Vector2(cellSize.x, hcs.y));
					state = 3;
				} else if (!topLeft && topRight && !botLeft && !botRight)
				{
					//Debug.DrawLine(new Vector3(pos.x + hcs.x, 0.0f, pos.y), new Vector3(pos.x + cellSize.x, 0.0f, pos.y + hcs.y));
					EmitQuad(pos, new Vector2(hcs.x, cellSize.y));
					EmitQuad(pos + hcs, hcs);
					EmitTriangle(
						new Vector2(pos.x + hcs.x, pos.y),
						pos + hcs,
						new Vector2(pos.x + cellSize.x, pos.y + hcs.y)
						);
					state = 4;
				} else if (!topLeft && topRight && botLeft && !botRight)
				{
					state = 5;
				} else if (!topLeft && topRight && !botLeft && botRight)
				{
					//Debug.DrawLine(new Vector3(pos.x + hcs.x, 0.0f, pos.y), new Vector3(pos.x + hcs.x, 0.0f, pos.y + cellSize.y));
					EmitQuad(pos, new Vector2(hcs.x, cellSize.y));
					state = 6;
				} else if (!topLeft && topRight && botLeft && botRight)
				{
					//Debug.DrawLine(new Vector3(pos.x + hcs.x, 0.0f, pos.y), new Vector3(pos.x, 0.0f, pos.y + hcs.y));
					EmitTriangle(
						new Vector2(pos.x + hcs.x, pos.y),
						pos,
						new Vector2(pos.x, pos.y + hcs.y)
						);
					state = 7;
				} else if (topLeft && !topRight && !botLeft && !botRight)
				{
					//Debug.DrawLine(new Vector3(pos.x + hcs.x, 0.0f, pos.y), new Vector3(pos.x, 0.0f, pos.y + hcs.y));
					EmitQuad(new Vector2(pos.x, pos.y + hcs.y), hcs);
					EmitQuad(new Vector2(pos.x + hcs.x, pos.y), new Vector2(hcs.x, cellSize.y));
					EmitTriangle(
						new Vector2(pos.x, pos.y + hcs.y),
						pos + hcs,
						new Vector2(pos.x + hcs.x, pos.y)
						);
					state = 8;
				} else if (topLeft && !topRight && botLeft && !botRight)
				{
					//Debug.DrawLine(new Vector3(pos.x + hcs.x, 0.0f, pos.y), new Vector3(pos.x + hcs.x, 0.0f, pos.y + cellSize.y));
					EmitQuad(new Vector2(pos.x + hcs.x, pos.y), new Vector2(hcs.x, cellSize.y));
					state = 9;
				} else if (topLeft && !topRight && !botLeft && botRight)
				{
					state = 10;
				} else if (topLeft && !topRight && botLeft && botRight)
				{
					//Debug.DrawLine(new Vector3(pos.x + hcs.x, 0.0f, pos.y), new Vector3(pos.x + cellSize.x, 0.0f, pos.y + hcs.y));
					EmitTriangle(
						new Vector2(pos.x + cellSize.x, pos.y + hcs.y),
						new Vector2(pos.x + cellSize.x, pos.y),
						new Vector2(pos.x + hcs.x, pos.y));
					state = 11;
				} else if (topLeft && topRight && !botLeft && !botRight)
				{
					//Debug.DrawLine(new Vector3(pos.x, 0.0f, pos.y + hcs.y), new Vector3(pos.x + cellSize.x, 0.0f, pos.y + hcs.y));
					EmitQuad(new Vector2(pos.x, pos.y + hcs.y), new Vector2(cellSize.x, hcs.y));
					state = 12;
				} else if (topLeft && topRight && botLeft && !botRight)
				{
					//Debug.DrawLine(new Vector3(pos.x + cellSize.x, 0.0f, pos.y + hcs.y), new Vector3(pos.x + hcs.x, 0.0f, pos.y + cellSize.y));
					EmitTriangle(
						new Vector2(pos.x + hcs.x, pos.y + cellSize.y),
						pos + cellSize,
						new Vector2(pos.x + cellSize.x, pos.y + hcs.y)
						);
					state = 13;
				} else if (topLeft && topRight && !botLeft && botRight)
				{
					//Debug.DrawLine(new Vector3(pos.x, 0.0f, pos.y + hcs.y), new Vector3(pos.x + hcs.x, 0.0f, pos.y + cellSize.y));
					EmitTriangle(
						new Vector2(pos.x, pos.y + hcs.y),
						new Vector2(pos.x, pos.y + cellSize.y),
						new Vector2(pos.x + hcs.x, pos.y + cellSize.y)
						);
					state = 14;
				}
			}
		}

		mesh.Clear();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = tris.ToArray();
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
	}
}
