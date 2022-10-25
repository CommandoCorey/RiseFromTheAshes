using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class FOW : MonoBehaviour {
	struct MaskRect {
		Vector2Int position;
		Vector2Int extent;
	}

	[SerializeField] Vector2Int maskExtent;

	[SerializeField] bool permanent;

	Texture2D genMaskTexture;
	RenderTexture genBlurredMaskTexture;

	[SerializeField] ComputeShader noiseGenShader;
	[SerializeField] ComputeShader maskBlurShader;
	[SerializeField]
	[Tooltip("Don't go crazy with this. Video memory is not infinite and 3D textures such as this use incredible amounts of it.")]
	Vector3Int noiseResolution = new Vector3Int(256, 256, 256);
	[HideInInspector] public RenderTexture noiseTexture;
	[SerializeField] int mainNoisePointCount = 32;
	[SerializeField] int detailNoisePointCount = 256;
	[SerializeField] int noiseSeed = 0;

	byte[] FOWMask;
	byte[] maskTextureData;

	float[] ShiftedPoints(float[] points, Vector3 amount)
	{
		float[] r = new float[points.Length];

		for (int i = 0; i < points.Length; i += 3) {
			r[i + 0] = points[i + 0] + amount.x * noiseResolution.x;
			r[i + 1] = points[i + 1] + amount.y * noiseResolution.y;
			r[i + 2] = points[i + 2] + amount.z * noiseResolution.z;
		}

		return r;
	}

	void GenPoints(float[] CPUPoints, ComputeBuffer points, ref int offset, Vector3 shift)
	{
		points.SetData(ShiftedPoints(CPUPoints, shift), 0, offset, CPUPoints.Length);
		offset += CPUPoints.Length;
	}

	ComputeBuffer GenPointComputeBuffer(int count, out int resultCount) {
		int computePointCount = count * 23;

		ComputeBuffer points = new ComputeBuffer(computePointCount * 3, sizeof(float));
		float[] CPUPoints = new float[count * 3];
		for (int i = 0; i < count * 3; i += 3)
		{
			CPUPoints[i + 0] = Random.Range(0, noiseResolution.x - 1);
			CPUPoints[i + 1] = Random.Range(0, noiseResolution.y - 1);
			CPUPoints[i + 2] = Random.Range(0, noiseResolution.z - 1);
		}

		int offset = 0;

		/* Tile the noise by copying the points to each of the faces of the boundry. */
		GenPoints(CPUPoints, points, ref offset, new Vector3(0.0f, 0.0f, 0.0f));

		GenPoints(CPUPoints, points, ref offset, new Vector3(-1.0f,  0.0f,  0.0f));
		GenPoints(CPUPoints, points, ref offset, new Vector3( 1.0f,  0.0f,  0.0f));
		GenPoints(CPUPoints, points, ref offset, new Vector3( 0.0f, -1.0f,  0.0f));
		GenPoints(CPUPoints, points, ref offset, new Vector3( 0.0f,  1.0f,  0.0f));
		GenPoints(CPUPoints, points, ref offset, new Vector3( 0.0f,  0.0f, -1.0f));
		GenPoints(CPUPoints, points, ref offset, new Vector3( 0.0f,  0.0f,  1.0f));

		GenPoints(CPUPoints, points, ref offset, new Vector3( 0.0f, -1.0f, -1.0f));
		GenPoints(CPUPoints, points, ref offset, new Vector3( 0.0f, -1.0f,  1.0f));
		GenPoints(CPUPoints, points, ref offset, new Vector3( 1.0f, -1.0f,  0.0f));
		GenPoints(CPUPoints, points, ref offset, new Vector3(-1.0f, -1.0f,  0.0f));

		GenPoints(CPUPoints, points, ref offset, new Vector3( 0.0f,  1.0f, -1.0f));
		GenPoints(CPUPoints, points, ref offset, new Vector3( 0.0f,  1.0f,  1.0f));
		GenPoints(CPUPoints, points, ref offset, new Vector3( 1.0f,  1.0f,  0.0f));
		GenPoints(CPUPoints, points, ref offset, new Vector3(-1.0f,  1.0f,  0.0f));

		GenPoints(CPUPoints, points, ref offset, new Vector3( 1.0f, -1.0f, -1.0f));
		GenPoints(CPUPoints, points, ref offset, new Vector3( 1.0f, -1.0f,  1.0f));
		GenPoints(CPUPoints, points, ref offset, new Vector3(-1.0f, -1.0f, -1.0f));
		GenPoints(CPUPoints, points, ref offset, new Vector3(-1.0f, -1.0f,  1.0f));

		GenPoints(CPUPoints, points, ref offset, new Vector3( 1.0f,  1.0f, -1.0f));
		GenPoints(CPUPoints, points, ref offset, new Vector3( 1.0f,  1.0f,  1.0f));
		GenPoints(CPUPoints, points, ref offset, new Vector3(-1.0f,  1.0f, -1.0f));
		GenPoints(CPUPoints, points, ref offset, new Vector3(-1.0f,  1.0f,  1.0f));

		resultCount = computePointCount;

		return points;
	}

	void Start() {
		FOWMask = new byte[maskExtent.x * maskExtent.y];
		maskTextureData = new byte[maskExtent.x * maskExtent.y];

		genMaskTexture        = new Texture2D(maskExtent.x, maskExtent.y, TextureFormat.R8, false);

		genBlurredMaskTexture = new RenderTexture(maskExtent.x, maskExtent.y, 0, RenderTextureFormat.R16);
		genBlurredMaskTexture.enableRandomWrite = true;
		genBlurredMaskTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
		genBlurredMaskTexture.wrapModeU = TextureWrapMode.Repeat;
		genBlurredMaskTexture.wrapModeV = TextureWrapMode.Repeat;
		genBlurredMaskTexture.Create();

		if (permanent) {
			ClearMask();

			var oldState = Random.state;
			Random.InitState(noiseSeed);

			noiseTexture = new RenderTexture(noiseResolution.x, noiseResolution.y, 0, RenderTextureFormat.RG16);
			noiseTexture.enableRandomWrite = true;
			noiseTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
			noiseTexture.volumeDepth = noiseResolution.z;
			noiseTexture.wrapModeU = TextureWrapMode.Repeat;
			noiseTexture.wrapModeV = TextureWrapMode.Repeat;
			noiseTexture.wrapModeW = TextureWrapMode.Repeat;
			noiseTexture.Create();

			int mainPointCount;
			var mainPoints = GenPointComputeBuffer(mainNoisePointCount, out mainPointCount);
			int detailPointCount;
			var detailPoints = GenPointComputeBuffer(detailNoisePointCount, out detailPointCount);

			noiseGenShader.SetTexture(0, "Result", noiseTexture);
			noiseGenShader.SetBuffer(0, "MainPoints", mainPoints);
			noiseGenShader.SetInt("MainPointCount", mainPointCount);

			noiseGenShader.SetBuffer(0, "DetailPoints", detailPoints);
			noiseGenShader.SetInt("DetailPointCount", detailPointCount);

			noiseGenShader.SetFloat("ValueDivisor", noiseResolution.magnitude);
			noiseGenShader.Dispatch(0, noiseResolution.x / 8, noiseResolution.y / 8, noiseResolution.z / 8);

			mainPoints.Release();
			detailPoints.Release();

			Random.state = oldState;
		}
	}

	private void Update()
	{
		if (!permanent) { ClearMask(); }
	}

	public void RequestMaskUpdate(byte value, Vector2Int position) {
		FOWMask[position.x + position.y * maskExtent.x] = value;
	}

	/* Returns a blurred version of the texture suitable for rendering */
	public RenderTexture MaskToTexture()
	{
		for (int i = 0; i < maskExtent.x * maskExtent.y; i++) {
			maskTextureData[i] = (byte)(FOWMask[i] == 0 ? 0 : 255);
		}

		genMaskTexture.LoadRawTextureData(maskTextureData);
		genMaskTexture.Apply();

		maskBlurShader.SetTexture(0, "Result",   genBlurredMaskTexture);
		maskBlurShader.SetTexture(0, "Original", genMaskTexture);
		maskBlurShader.SetVector("TextureSize", GetMaskExtentf());
		maskBlurShader.Dispatch(0,
			(int)((float)maskExtent.x / 8.0f + 1.0f),
			(int)((float)maskExtent.y / 8.0f + 1.0f), 1);

		return genBlurredMaskTexture;
	}

	public bool GetMask(int x, int y)
	{
		return FOWMask[x + y * maskExtent.x] == 0x0 ? true : false;
	}

	public bool GetMask(Vector2 xy)
	{
		return GetMask((int)xy.x, (int)xy.y);
	}

	public void ClearMask()
	{
		for (int i = 0; i < maskExtent.x * maskExtent.y; i++)
		{
			FOWMask[i] = 1;
		}
	}

	public void MaskDrawCircle(Vector2 position, int radius)
	{
		int radius2 = radius * radius;

		int start_x = Mathf.Min(Mathf.Max(Mathf.FloorToInt(position.x) - radius, 0), maskExtent.x);
		int start_y = Mathf.Min(Mathf.Max(Mathf.FloorToInt(position.y) - radius, 0), maskExtent.y);
		int end_x = Mathf.Min(Mathf.CeilToInt(position.x) + radius, maskExtent.x);
		int end_y = Mathf.Min(Mathf.CeilToInt(position.y) + radius, maskExtent.y);

		for (int y = start_y; y < end_y; y++)
		{
			for (int x = start_x; x < end_x; x++)
			{
				float a = x - position.x;
				float b = y - position.y;

				if (a * a + b * b <= radius2)
				{
					int pos = x + y * maskExtent.x;
					FOWMask[pos] = 0;
				}
			}
		}
	}

	public Vector2 WorldPosToMaskPos(Vector3 worldPos)
	{
		Vector3 topCorner = transform.position;
		Vector3 pos = worldPos - topCorner;
		pos = new Vector3(pos.x, pos.y, pos.z);
		return new Vector2(pos.x, pos.z);
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
