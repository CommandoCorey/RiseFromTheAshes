using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
public class MinimapFogEffectRenderer : ScriptableRendererFeature
{
	MinimapFogEffect pass;

	public override void Create()
	{
		pass = new MinimapFogEffect();
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		renderer.EnqueuePass(pass);
	}
}

class MinimapFogEffect : ScriptableRenderPass
{
	RenderTargetIdentifier src;
	RenderTargetIdentifier positions;
	RenderTargetIdentifier dst;

	readonly int temporaryRTId = Shader.PropertyToID("_TempRT");

	Material fogMaterial;

	public MinimapFogEffect()
	{
		renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

		fogMaterial = new Material(Shader.Find("Hidden/MinimapFog"));
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
#if UNITY_EDITOR
		if (SceneView.currentDrawingSceneView != null || !FOWManager.Instance || !FOWManager.Instance.imperm || !FOWManager.Instance.perm) {
				return;
		}
#else
		if (!FOWManager.Instance || !FOWManager.Instance.imperm || !FOWManager.Instance.perm) {
				return;
		}
#endif

		fogMaterial.SetVector("_PermTopCorner", FOWManager.Instance.perm.transform.position);
		fogMaterial.SetVector("_ImpermTopCorner", FOWManager.Instance.imperm.transform.position);
		fogMaterial.SetVector("_PermMaskSize", FOWManager.Instance.perm.GetMaskExtentf());
		fogMaterial.SetVector("_ImpermMaskSize", FOWManager.Instance.imperm.GetMaskExtentf());
		fogMaterial.SetFloat("_PermHeight", FOWManager.Instance.perm.transform.position.y);
		fogMaterial.SetFloat("_ImpermHeight", FOWManager.Instance.imperm.transform.position.y);

		RenderTexture mask = FOWManager.Instance.imperm.MaskToTexture();

		CommandBuffer cmd = CommandBufferPool.Get("minimapFogEffectCmdBuffer");
		cmd.Clear();

		Blit(cmd, src, dst, fogMaterial);
		Blit(cmd, dst, src);

		context.ExecuteCommandBuffer(cmd);
		CommandBufferPool.Release(cmd);
	}
}
