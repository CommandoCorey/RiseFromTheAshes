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
 * see if that's actually worth the extra effort.
 * 
 * It might be more worth trying to do the mesh generation
 * in a compute shader, now that I think about it. */

[RequireComponent(typeof(MeshFilter))]
public class FOW : MonoBehaviour {
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

	Texture2D genMaskTexture;

	[SerializeField] ComputeShader noiseGenShader;
	[SerializeField]
	[Tooltip("Don't go crazy with this. Video memory is not infinite and 3D textures such as this use incredible amounts of it.")]
	Vector3Int noiseResolution = new Vector3Int(256, 256, 256);
	[HideInInspector] public RenderTexture noiseTexture;
	[SerializeField] int noisePointCount = 32;

	Mesh mesh;

	byte[] FOWMask;
	byte[] blurredMask;

	bool wantMaskUpdate = true;

	List<Vector3> vertices;
	List<int> tris;

	static float[] ShiftedPoints(float[] points, Vector3 amount)
	{
		float[] r = new float[points.Length];

		for (int i = 0; i < points.Length; i += 3) {
			r[i + 0] = points[i + 0] + amount.x;
			r[i + 1] = points[i + 1] + amount.y;
			r[i + 2] = points[i + 2] + amount.z;
		}

		return r;
	}

	void Start() {
		FOWMask = new byte[maskExtent.x * maskExtent.y];
		blurredMask = new byte[maskExtent.x * maskExtent.y];

		SetMaskFromTexture(maskTexture);

		mesh = new Mesh();
		mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
		GetComponent<MeshFilter>().mesh = mesh;

		vertices = new List<Vector3>();
		tris = new List<int>();

		genMaskTexture = new Texture2D(maskExtent.x, maskExtent.y, TextureFormat.R8, false);

		noiseTexture = new RenderTexture(noiseResolution.x, noiseResolution.y, 0, RenderTextureFormat.RG16);
		noiseTexture.enableRandomWrite = true;
		noiseTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
		noiseTexture.volumeDepth = noiseResolution.z;
		noiseTexture.wrapModeU = TextureWrapMode.Repeat;
		noiseTexture.wrapModeV = TextureWrapMode.Repeat;
		noiseTexture.wrapModeW = TextureWrapMode.Repeat;
		noiseTexture.Create();

		int computePointCount = noisePointCount * 7;

		ComputeBuffer points = new ComputeBuffer(computePointCount, sizeof(float) * 3);
		float[] CPUPoints = new float[noisePointCount * 3];
		for (int i = 0; i < noisePointCount * 3; i += 3)
		{
			CPUPoints[i + 0] = Random.Range(0, noiseResolution.x - 1);
			CPUPoints[i + 1] = Random.Range(0, noiseResolution.y - 1);
			CPUPoints[i + 2] = Random.Range(0, noiseResolution.z - 1);
		}

		/* Tile the noise by copying the points to each of the faces of the boundry. */
		points.SetData(CPUPoints, 0, 0, noisePointCount * 3);
		points.SetData(ShiftedPoints(CPUPoints, new Vector3( noiseResolution.x, 0.0f, 0.0f)), 0, noisePointCount * 3 * 1, noisePointCount * 3);
		points.SetData(ShiftedPoints(CPUPoints, new Vector3(-noiseResolution.x, 0.0f, 0.0f)), 0, noisePointCount * 3 * 2, noisePointCount * 3);
		points.SetData(ShiftedPoints(CPUPoints, new Vector3(0.0f,  noiseResolution.y, 0.0f)), 0, noisePointCount * 3 * 3, noisePointCount * 3);
		points.SetData(ShiftedPoints(CPUPoints, new Vector3(0.0f, -noiseResolution.y, 0.0f)), 0, noisePointCount * 3 * 4, noisePointCount * 3);
		points.SetData(ShiftedPoints(CPUPoints, new Vector3(0.0f, 0.0f,  noiseResolution.z)), 0, noisePointCount * 3 * 5, noisePointCount * 3);
		points.SetData(ShiftedPoints(CPUPoints, new Vector3(0.0f, 0.0f, -noiseResolution.z)), 0, noisePointCount * 3 * 6, noisePointCount * 3);

		noiseGenShader.SetTexture(0, "Result", noiseTexture);
		noiseGenShader.SetBuffer(0, "Points", points);
		noiseGenShader.SetInt("PointCount", computePointCount);
		noiseGenShader.SetFloat("ValueDivisor", Vector3Int.Distance(Vector3Int.zero, noiseResolution));
		noiseGenShader.Dispatch(0, noiseResolution.x / 8, noiseResolution.y / 8, noiseResolution.z / 8);

		points.Release();
	}

	private void Update()
	{
		if (wantMaskUpdate) {
			GenerateMesh();
			wantMaskUpdate = false;
		}

		if (!permanent) { ClearMask(); }
	}

	public void RequestMaskUpdate(byte value, Vector2Int position) {
		wantMaskUpdate = true;
		FOWMask[position.x + position.y * maskExtent.x] = value;
	}

	public Texture2D MaskToTexture()
	{
		/* TODO (George): Actually blur the mask. Maybe. It
		 * might not be necessary. */

		for (int i = 0; i < maskExtent.x * maskExtent.y; i++) {
			blurredMask[i] = (byte)(FOWMask[i] == 0 ? 0x0 : 255);
		}

		genMaskTexture.LoadRawTextureData(blurredMask);
		genMaskTexture.Apply();

		return genMaskTexture;
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

	public Vector2 GetMaskExtentf()
	{
		return new Vector2((float)maskExtent.x, (float)maskExtent.y);
	}

	public Vector2Int GetMaskExtent()
	{
		return maskExtent;
	}
}
