
Shader "Hidden/FogImperm"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
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

			TEXTURE2D(_CameraDepthTexture);
			SAMPLER(sampler_CameraDepthTexture);

			uniform float _Height;

			uniform float4 _FogColour;

			uniform float4 _FogTopCorner;
			uniform float2 _FogMaskSize;

			#define MAX_RAY_STEPS 256
			#define MAX_RAY_DIST  256.0
			#define MIN_RAY_DIST  0.001

			float map(float3 p) {
				/* Infinite plane: dot(p, n) + h */
				return dot(p, float3(0.0, 1.0, 0.0)) - _Height;
			}

			float rayVSPlane(float3 centre, float3 normal, float3 origin, float3 dir) {
				float denom = dot(normal, dir);
				if (abs(denom) > 0.0001) {
					return dot((centre - origin), normal) / denom;
				}
				return 0.0;
			}

			float2 worldPosToFogMaskPos(float3 worldPos) {
				float3 p = worldPos - _FogTopCorner;
				return p.xz;
			}

			float4 frag(v2f i) : SV_Target
			{
				float3 rayOrigin = _WorldSpaceCameraPos;
				float3 rayDirection = normalize(i.viewVector);

				float dist = rayVSPlane(float3(0.0, _Height, 0.0), float3(0.0, 1.0, 0.0), rayOrigin, rayDirection);

				float3 hitPoint = rayOrigin + rayDirection * dist;

				float2 hitPointMaskSpace = worldPosToFogMaskPos(hitPoint);
				hitPointMaskSpace /= _FogMaskSize;

				hitPointMaskSpace = clamp(hitPointMaskSpace, 0.0, 1.0);

				float maskVal = tex2D(_MaskTex, hitPointMaskSpace).r;

				float4 col = tex2D(_MainTex, i.uv);

				float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv);
				depth = ComputeWorldSpacePosition(i.uv, depth, UNITY_MATRIX_I_VP).y;

				if (hitPoint.y < depth) {
					return col;
				}
				else {
					float3 m = (1.0 - (maskVal - _FogColour.rgb * _FogColour.a));
					col.rgb *= m;
				}

				return float4(col.rgb, 1.0);
			}
			ENDHLSL
		}
	}
}

