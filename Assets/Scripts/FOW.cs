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
	[SerializeField] float height = 1.0f;

	[SerializeField] bool permanent;

	Texture2D genMaskTexture;

	[SerializeField] ComputeShader noiseGenShader;
	[SerializeField]
	[Tooltip("Don't go crazy with this. Video memory is not infinite and 3D textures such as this use incredible amounts of it.")]
	Vector3Int noiseResolution = new Vector3Int(256, 256, 256);
	[HideInInspector] public RenderTexture noiseTexture;
	[SerializeField] int noisePointCount = 32;

	byte[] FOWMask;
	byte[] blurredMask;

	bool wantMaskUpdate = true;

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

		genMaskTexture = new Texture2D(maskExtent.x, maskExtent.y, TextureFormat.R8, false);
		if (permanent) {
			ClearMask();

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
	}

	private void Update()
	{
		if (wantMaskUpdate) {
			//GenerateMesh();
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

	bool GetMask(int x, int y)
	{
		return FOWMask[x + y * maskExtent.x] == 0x0 ? true : false;
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

	void OnDrawGizmosSelected() {
		Gizmos.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y, transform.position.z + maskExtent.y));
		Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + maskExtent.x, transform.position.y, transform.position.z));
		Gizmos.DrawLine(
			new Vector3(transform.position.x + maskExtent.x, transform.position.y, transform.position.z),
			new Vector3(transform.position.x + maskExtent.x, transform.position.y, transform.position.z + maskExtent.y));
		Gizmos.DrawLine(
			new Vector3(transform.position.x, transform.position.y, transform.position.z + maskExtent.y),
			new Vector3(transform.position.x + maskExtent.x, transform.position.y, transform.position.z + maskExtent.y));
	}
}
