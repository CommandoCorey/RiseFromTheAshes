Shader "Hidden/MinimapFog"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_MaskTex("Mask Texture", 2D) = "white" {}
	}
	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest Always

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			struct Attributes
			{
				float4 positionOS       : POSITION;
				float2 uv               : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 viewVector : TEXCOORD1;
			};

			v2f vert (Attributes input)
			{
				v2f o;
				o.vertex = GetVertexPositionInputs(input.positionOS.xyz).positionCS;
				o.uv = input.uv;

				float3 viewVector = mul(unity_CameraInvProjection, float4(input.uv * 2 - 1, 0, -1));
				o.viewVector = mul(unity_CameraToWorld, float4(viewVector, 0));

				return o;
			}

			sampler2D _MainTex;
			sampler2D _MaskTex;

			float4 frag(v2f i) : SV_Target
			{
				return float4(1.0 - tex2D(_MainTex, i.uv).rgb, 1.0);

			}
			ENDHLSL
		}
	}
}
