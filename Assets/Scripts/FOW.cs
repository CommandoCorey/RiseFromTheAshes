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
	[SerializeField] float height = 1.0f;

	[SerializeField] bool permanent;

	Mesh mesh;

	byte[] FOWMask;

	bool wantMaskUpdate = true;

	List<Vector3> vertices;
	List<int> tris;

	void Start() {
		FOWMask = new byte[maskExtent.x * maskExtent.y];

		SetMaskFromTexture(maskTexture);

		mesh = new Mesh();
		mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
		GetComponent<MeshFilter>().mesh = mesh;

		vertices = new List<Vector3>();
		tris = new List<int>();
	}

	private void Update()
	{
		if (wantMaskUpdate) {
			GenerateMesh();
			wantMaskUpdate = false;
		}

		if (permanent) { ClearMask(); }
	}

	public void RequestMaskUpdate(byte value, Vector2Int position) {
		wantMaskUpdate = true;
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

	void EmitWall(Vector2 a, Vector2 b) {

		int indexOffset = vertices.Count;

		vertices.Add(new Vector3(a.x, 0.0f, a.y));
		vertices.Add(new Vector3(b.x, 0.0f, b.y));
		vertices.Add(new Vector3(a.x, -height, a.y));
		vertices.Add(new Vector3(b.x, -height, b.y));

		tris.Add(indexOffset + 3);
		tris.Add(indexOffset + 1);
		tris.Add(indexOffset + 0);
		tris.Add(indexOffset + 0);
		tris.Add(indexOffset + 2);
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
				 * TODO (George): Blur the mask and lerp between the values for better smoothing. This also
				 * might not be necessary if it's just used to spawn particles.
				 * 
				 * This generates quite an ineffecient mesh; It doesn't care much about reusing vertices.
				 *
				 * There's currently unhandled cases, such as the two ambigous ones that exist in this
				 * algorithm. I think it's fine to leave them unhandled, though we will see once we
				 * get some actual gameplay happening. I doubt anybody will notice - it's not like there's
				 * going to be gaps in an outline or something since this mesh will be used to spawn
				 * particles - It doesn't have to be perfect. If that doesn't work out, then the edge
				 * cases will need to be handled. */
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
					Vector2 start = new Vector2(pos.x,         pos.y + hcs.y);
					Vector2 end   = new Vector2(pos.x + hcs.x, pos.y + cellSize.y);
					//Debug.DrawLine(new Vector3(start.x, 0.0f, start.y), new Vector3(end.x, 0.0f, end.y));
					EmitQuad(new Vector2(pos.x + hcs.x, pos.y), new Vector2(hcs.x, cellSize.y));
					EmitQuad(pos, hcs);
					EmitTriangle(
						end,
						pos + hcs,
						start
						);
					EmitWall(start, end);
					state = 1;
				} else if (!topLeft && !topRight && !botLeft && botRight)
				{
					Vector2 start = new Vector2(pos.x + cellSize.x, pos.y + hcs.y);
					Vector2 end   = new Vector2(pos.x + hcs.x,      pos.y + cellSize.y);
					//Debug.DrawLine(new Vector3(start.x, 0.0f, start.y), new Vector3(end.x, 0.0f, end.y));
					EmitQuad(pos, new Vector2(hcs.x, cellSize.y));
					EmitQuad(new Vector2(pos.x + hcs.x, pos.y), hcs);
					EmitTriangle(
						start,
						pos + hcs,
						end
						);
					EmitWall(end, start);
					state = 2;
				} else if (!topLeft && !topRight && botLeft && botRight)
				{
					Vector2 start = new Vector2(pos.x,              pos.y + hcs.y);
					Vector2 end   = new Vector2(pos.x + cellSize.x, pos.y + hcs.y);
					//Debug.DrawLine(new Vector3(pos.x, 0.0f, pos.y + hcs.y), new Vector3(pos.x + cellSize.x, 0.0f, pos.y + hcs.y));
					EmitQuad(pos, new Vector2(cellSize.x, hcs.y));
					EmitWall(start, end);
					state = 3;
				} else if (!topLeft && topRight && !botLeft && !botRight)
				{
					Vector2 start = new Vector2(pos.x + hcs.x,      pos.y);
					Vector2 end   = new Vector2(pos.x + cellSize.x, pos.y + hcs.y);
					//Debug.DrawLine(new Vector3(start.x, 0.0f, start.y), new Vector3(end.x, 0.0f, end.y));
					EmitQuad(pos, new Vector2(hcs.x, cellSize.y));
					EmitQuad(pos + hcs, hcs);
					EmitTriangle(
						start,
						pos + hcs,
						end
						);
					EmitWall(end, start);
					state = 4;
				} else if (!topLeft && topRight && botLeft && !botRight)
				{
					state = 5;
				} else if (!topLeft && topRight && !botLeft && botRight)
				{
					Vector2 start = new Vector2(pos.x + hcs.x, pos.y);
					Vector2 end   = new Vector2(pos.x + hcs.x, pos.y + cellSize.y);
					//Debug.DrawLine(new Vector3(start.x, 0.0f, start.y), new Vector3(end.x, 0.0f, end.y));
					EmitQuad(pos, new Vector2(hcs.x, cellSize.y));
					EmitWall(end, start); 
					state = 6;
				} else if (!topLeft && topRight && botLeft && botRight)
				{
					Vector2 start = new Vector2(pos.x + hcs.x, pos.y);
					Vector2 end   = new Vector2(pos.x,         pos.y + hcs.y);
					//Debug.DrawLine(new Vector3(pos.x + hcs.x, 0.0f, pos.y), new Vector3(pos.x, 0.0f, pos.y + hcs.y));
					EmitTriangle(
						start,
						pos,
						end
						);
					EmitWall(end, start);
					state = 7;
				} else if (topLeft && !topRight && !botLeft && !botRight)
				{
					Vector2 start = new Vector2(pos.x + hcs.x, pos.y);
					Vector2 end   = new Vector2(pos.x, pos.y + hcs.y);
					//Debug.DrawLine(new Vector3(start.x, 0.0f, start.y), new Vector3(end.x, 0.0f, end.y));
					EmitQuad(new Vector2(pos.x, pos.y + hcs.y), hcs);
					EmitQuad(new Vector2(pos.x + hcs.x, pos.y), new Vector2(hcs.x, cellSize.y));
					EmitTriangle(
						end,
						pos + hcs,
						start
						);
					EmitWall(start, end);
					state = 8;
				} else if (topLeft && !topRight && botLeft && !botRight)
				{
					Vector2 start = new Vector2(pos.x + hcs.x, pos.y);
					Vector2 end   = new Vector2(pos.x + hcs.x, pos.y + cellSize.y);
					//Debug.DrawLine(new Vector3(start.x, 0.0f, start.y), new Vector3(end.x, 0.0f, end.y));
					EmitQuad(new Vector2(pos.x + hcs.x, pos.y), new Vector2(hcs.x, cellSize.y));
					EmitWall(start, end);
					state = 9;
				} else if (topLeft && !topRight && !botLeft && botRight)
				{
					state = 10;
				} else if (topLeft && !topRight && botLeft && botRight)
				{
					Vector2 start = new Vector2(pos.x + hcs.x,      pos.y);
					Vector2 end   = new Vector2(pos.x + cellSize.x, pos.y + hcs.y);
					//Debug.DrawLine(new Vector3(start.x, 0.0f, start.y), new Vector3(end.x, 0.0f, end.y));
					EmitTriangle(
						end,
						new Vector2(pos.x + cellSize.x, pos.y),
						start);
					EmitWall(start, end);
					state = 11;
				} else if (topLeft && topRight && !botLeft && !botRight)
				{
					Vector2 start = new Vector2(pos.x,              pos.y + hcs.y);
					Vector2 end   = new Vector2(pos.x + cellSize.x, pos.y + hcs.y);
					//Debug.DrawLine(new Vector3(start.x, 0.0f, start.y), new Vector3(end.x, 0.0f, end.y));
					EmitQuad(new Vector2(pos.x, pos.y + hcs.y), new Vector2(cellSize.x, hcs.y));
					EmitWall(end, start);
					state = 12;
				} else if (topLeft && topRight && botLeft && !botRight)
				{
					Vector2 start = new Vector2(pos.x + cellSize.x, pos.y + hcs.y);
					Vector2 end   = new Vector2(pos.x + hcs.x,      pos.y + cellSize.y);
					//Debug.DrawLine(new Vector3(start.x, 0.0f, start.y), new Vector3(end.x, 0.0f, end.y));
					EmitTriangle(
						end,
						pos + cellSize,
						start
						);
					EmitWall(start, end);
					state = 13;
				} else if (topLeft && topRight && !botLeft && botRight)
				{
					Vector2 start = new Vector2(pos.x, pos.y + hcs.y);
					Vector2 end = new Vector2(pos.x + hcs.x, pos.y + cellSize.y);
					//Debug.DrawLine(new Vector3(start.x, 0.0f, start.y), new Vector3(end.x, 0.0f, end.y));
					EmitTriangle(
						start,
						new Vector2(pos.x, pos.y + cellSize.y),
						end
						);
					EmitWall(end, start);
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

	public void ClearMask()
	{
		wantMaskUpdate = true;

		for (int i = 0; i < maskExtent.x * maskExtent.y; i++)
		{
			FOWMask[i] = 0x1;
		}
	}

	public void MaskDrawCircle(Vector2Int position, int radius)
	{
		int diametre = radius * 2;

		int start_x = Mathf.Min(Mathf.Max(position.x - radius, 0), maskExtent.x);
		int start_y = Mathf.Min(Mathf.Max(position.y - radius, 0), maskExtent.y);
		int end_x   = Mathf.Min(position.x + diametre, maskExtent.x);
		int end_y   = Mathf.Min(position.y + diametre, maskExtent.y);

		var centre = position;

		for (int y = start_y; y < end_y; y++)
		{
			for (int x = start_x; x < end_x; x++)
			{
				if (Vector2Int.Distance(new Vector2Int(x, y), centre) <= radius) {
					int pos = x + y * maskExtent.x;
					if (FOWMask[pos] != 0x0) {
						FOWMask[pos] = 0x0;
						wantMaskUpdate = true;
					}
				}
			}
		}
	}

	public Vector2Int WorldPosToMaskPos(Vector3 worldPos)
	{
		Vector3 topCorner = transform.position;
		Vector3 pos = worldPos - topCorner;
		pos = new Vector3(pos.x, pos.y, pos.z);
		return new Vector2Int((int)pos.x, (int)pos.z);
	}
}
