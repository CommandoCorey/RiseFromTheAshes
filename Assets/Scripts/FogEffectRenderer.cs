using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
public class FogEffectRenderer : ScriptableRendererFeature
{
	FogEffectPass pass;

	public override void Create()
	{
		pass = new FogEffectPass();
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

	public FogEffectPass()
	{
		renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

		postMaterial = new Material(Shader.Find("Hidden/Fog"));
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

		postMaterial.SetTexture("_MaskTex", FOWManager.Instance.imperm.MaskToTexture());

		CommandBuffer cmd = CommandBufferPool.Get("fogEffectCmdBuffer");
		cmd.Clear();

		Blit(cmd, src, dst, postMaterial);
		Blit(cmd, dst, src);

		context.ExecuteCommandBuffer(cmd);
		CommandBufferPool.Release(cmd);
	}
}
