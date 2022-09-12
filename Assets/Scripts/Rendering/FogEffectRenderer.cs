using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
public class FogEffectRenderer : ScriptableRendererFeature
{
	FogEffectPass pass;

	public int sampleCount = 32;
	public float fogDepth = 10.0f;
	public float threshold = 0.1f;
	public float stepSize = 0.1f;
	public Vector3 scrollDirection = Vector3.right;
	public Color colour;
	public Color exploredColour;
	public float scale = 0.5f;

	public override void Create()
	{
		pass = new FogEffectPass(sampleCount, fogDepth, threshold, stepSize, scrollDirection, colour, exploredColour, scale);
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		renderer.EnqueuePass(pass);
	}
}

class FogEffectPass : ScriptableRenderPass
{
	RenderTargetIdentifier src;
	RenderTargetIdentifier positions;
	RenderTargetIdentifier dst;

	readonly int temporaryRTId = Shader.PropertyToID("_TempRT");

	Material fogMaterial;
	Material impermMaterial;

	int sampleCount;
	float fogDepth;
	float threshold;
	float stepSize;
	Vector3 scrollDirection;
	Color colour;
	Color exploredColour;
	float scale;

	public FogEffectPass(int _sampleCount, float _fogDepth, float _threshold, float _stepSize, Vector3 _scrollDirection, Color _colour, Color _exploredColour, float _scale)
	{
		renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

		fogMaterial = new Material(Shader.Find("Hidden/Fog"));
		impermMaterial = new Material(Shader.Find("Hidden/FogImperm"));

		sampleCount = _sampleCount;
		fogDepth = _fogDepth;
		threshold = _threshold;
		stepSize = _stepSize;
		scrollDirection = _scrollDirection;
		colour = _colour;
		scale = _scale;
		exploredColour = _exploredColour;
	}

	public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
		RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;
		desc.depthBufferBits = 0;

		ScriptableRenderer renderer = renderingData.cameraData.renderer;
		src = renderer.cameraColorTarget;

		cmd.GetTemporaryRT(temporaryRTId, desc, FilterMode.Point);
		dst = new RenderTargetIdentifier(temporaryRTId);
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (SceneView.currentDrawingSceneView != null || !FOWManager.Instance || !FOWManager.Instance.imperm || !FOWManager.Instance.perm) {
				return;
		}

		FOWCamera FOWCamera = Camera.main.GetComponent<FOWCamera>();
		if (!FOWCamera) {
			Debug.LogWarning("There isn't a FOWCamera attached to the main camera. Fix this.");
			return;
		}

		fogMaterial.SetTexture("_MaskTex", FOWManager.Instance.perm.MaskToTexture());
		fogMaterial.SetVector("_FogTopCorner", FOWManager.Instance.perm.transform.position);
		fogMaterial.SetFloat("_Threshold", threshold);
		fogMaterial.SetFloat("_FogDepth", fogDepth);
		fogMaterial.SetInt("_Samples", sampleCount);
		fogMaterial.SetFloat("_StepSize", stepSize);
		fogMaterial.SetVector("_ScrollDirection", scrollDirection);
		fogMaterial.SetFloat("_Height", FOWManager.Instance.perm.transform.position.y);
		fogMaterial.SetColor("_FogColour", colour);
		fogMaterial.SetVector("_FogMaskSize", FOWManager.Instance.perm.GetMaskExtentf());
		fogMaterial.SetTexture("_NoiseTexture", FOWManager.Instance.perm.noiseTexture);
		fogMaterial.SetFloat("_CloudScale", scale);

		Texture mask = FOWManager.Instance.imperm.MaskToTexture();

		impermMaterial.SetTexture("_MaskTex", mask);
		impermMaterial.SetVector("_FogTopCorner", FOWManager.Instance.imperm.transform.position);
		impermMaterial.SetFloat("_Height", FOWManager.Instance.imperm.transform.position.y);
		impermMaterial.SetVector("_FogMaskSize", FOWManager.Instance.imperm.GetMaskExtentf());
		impermMaterial.SetVector("_FogColour", exploredColour);
		impermMaterial.SetTexture("_AffectedObjects", FOWCamera.FOWAffectedRenderTexture, RenderTextureSubElement.Color);
		impermMaterial.SetTexture("_AffectedDepth", FOWCamera.FOWAffectedRenderTexture, RenderTextureSubElement.Depth);
		impermMaterial.SetTexture("_MainDepth", Camera.main.activeTexture, RenderTextureSubElement.Depth);

		Shader.SetGlobalTexture("G_FOWOccludeMaskTexture", mask);
		Shader.SetGlobalVector("G_FogTopCorner", FOWManager.Instance.imperm.transform.position);
		Shader.SetGlobalVector("G_FogMaskSize", FOWManager.Instance.imperm.GetMaskExtentf());

		CommandBuffer cmd = CommandBufferPool.Get("fogEffectCmdBuffer");
		cmd.Clear();

		Blit(cmd, src, dst, impermMaterial);
		Blit(cmd, dst, src);

		Blit(cmd, src, dst, fogMaterial);
		Blit(cmd, dst, src);

		context.ExecuteCommandBuffer(cmd);
		CommandBufferPool.Release(cmd);
	}
}
