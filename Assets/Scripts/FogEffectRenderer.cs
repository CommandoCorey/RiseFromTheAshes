using System;
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
	public float height = 5.0f;
	public Color colour;

	public override void Create()
	{
		pass = new FogEffectPass(sampleCount, fogDepth, threshold, stepSize, scrollDirection, height, colour);
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

	Material postMaterial;

	int sampleCount;
	float fogDepth;
	float threshold;
	float stepSize;
	Vector3 scrollDirection;
	float height;
	Color colour;

	public FogEffectPass(int _sampleCount, float _fogDepth, float _threshold, float _stepSize, Vector3 _scrollDirection, float _height, Color _colour)
	{
		renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

		postMaterial = new Material(Shader.Find("Hidden/Fog"));

		sampleCount = _sampleCount;
		fogDepth = _fogDepth;
		threshold = _threshold;
		stepSize = _stepSize;
		scrollDirection = _scrollDirection;
		height = _height;
		colour = _colour;
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
		if (!(Application.isPlaying && Application.isEditor) || !FOWManager.Instance || !FOWManager.Instance.imperm || !FOWManager.Instance.perm) {
				return;
		}

		postMaterial.SetTexture("_MaskTex", FOWManager.Instance.perm.MaskToTexture());
		postMaterial.SetVector("_FogTopCorner", FOWManager.Instance.perm.transform.position);
		postMaterial.SetFloat("_Threshold", threshold);
		postMaterial.SetFloat("_FogDepth", fogDepth);
		postMaterial.SetInt("_Samples", sampleCount);
		postMaterial.SetFloat("_StepSize", stepSize);
		postMaterial.SetVector("_ScrollDirection", scrollDirection);
		postMaterial.SetFloat("_Height", height);
		postMaterial.SetColor("_FogColour", colour);

		CommandBuffer cmd = CommandBufferPool.Get("fogEffectCmdBuffer");
		cmd.Clear();

		Blit(cmd, src, dst, postMaterial);
		Blit(cmd, dst, src);

		context.ExecuteCommandBuffer(cmd);
		CommandBufferPool.Release(cmd);
	}
}
