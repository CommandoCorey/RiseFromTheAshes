Shader "Hidden/Fog"
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

			TEXTURE2D(_CameraDepthTexture);
			SAMPLER(sampler_CameraDepthTexture);

			sampler2D _MaskTex;
			Texture3D<float2> _NoiseTexture;

			SamplerState sampler_NoiseTexture;

			uniform float _Threshold;
			uniform float _FogDepth;
			uniform float _StepSize;
			uniform int _Samples;
			uniform float3 _ScrollDirection;
			uniform float _Height;
			uniform float4 _FogColour;
			uniform float _CloudScale;
			uniform float _RenderDistance;

			uniform float4 _FogTopCorner;
			uniform float2 _FogMaskSize;

			#define MAX_RAY_STEPS 256
			#define MIN_RAY_DIST  0.001

			float2 worldPosToFogMaskPos(float3 worldPos) {
				float3 p = worldPos - _FogTopCorner;
				return p.xz;
			}

			float rayVSPlane(float3 centre, float3 normal, float3 origin, float3 dir) {
				float denom = dot(normal, dir);
				if (abs(denom) > 0.0001) {
    				return dot((centre - origin), normal) / denom;
				}
				return 0.0;
			}

			float4 frag(v2f i) : SV_Target
			{
				float3 rayOrigin = _WorldSpaceCameraPos;
				float3 rayDirection = normalize(i.viewVector);

				float dist = rayVSPlane(float3(0.0, _Height, 0.0), float3(0.0, 1.0, 0.0), rayOrigin, rayDirection);

				float3 hitPoint = rayOrigin + rayDirection * dist;

				float4 col = tex2D(_MainTex, i.uv);

				float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv);
				depth = ComputeWorldSpacePosition(i.uv, depth, UNITY_MATRIX_I_VP).y;

				if (hitPoint.y < depth) {
					//return col;
				}

				int maxSamples = min(_Samples, 32);

				float density = 0.0;
				float light = 1.0;
				if (dist > 0.0 && dist < _RenderDistance) {
					/* Ray trace from the hit point returned by the raymarch and
					 * take samples from the 3-D noise to determine the density of the
					 * fog at this pixel. */

					hitPoint = rayOrigin + rayDirection * dist;

					for (int i = 0; i < maxSamples; i++) {
						float3 p = hitPoint + rayDirection * (float(i) * _StepSize);

						float3 samplePosMain   = (p * _CloudScale) + _ScrollDirection * _Time.y;
						float3 samplePosDetail = (p * _CloudScale) - _ScrollDirection * _Time.y * 0.1;
						float mainNoise   = _NoiseTexture.SampleLevel(sampler_NoiseTexture, samplePosMain, 0).r * 1;
						float detailNoise = 0.0;
						if (dist < 100.0) {
							detailNoise = _NoiseTexture.SampleLevel(sampler_NoiseTexture, samplePosDetail, 0).g;
						}

						float noise = mainNoise + detailNoise;

						light -= noise * 0.01 * (_Height - p.y);
						light = max(0.04, light);

						if (p.y > depth) {
							density += max(0.1, noise - _Threshold);
						}
					}
				}

				float2 hitPointMaskSpace = worldPosToFogMaskPos(hitPoint);
				hitPointMaskSpace /= _FogMaskSize;

				hitPointMaskSpace = clamp(hitPointMaskSpace, 0.0, 1.0);

				float maskVal = tex2D(_MaskTex, hitPointMaskSpace).r;

				density *= maskVal;

				density = exp(-density);
				float4 fogColour = (1.0 - density) * _FogColour * light;
				fogColour.a = 1.0;

				return ((maskVal >= 1.0) ? float4(0.0, 0.0, 0.0, 1.0) : col) * density + fogColour;
			}
			ENDHLSL
		}
	}
}
