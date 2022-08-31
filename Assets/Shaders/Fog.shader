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
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 viewVector : TEXCOORD1;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;

				float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv * 2 - 1, 0, -1));
				o.viewVector = mul(unity_CameraToWorld, float4(viewVector, 0));

				return o;
			}

			sampler2D _MainTex;
			sampler2D _CameraDepthTexture;
			sampler2D _MaskTex;
			Texture3D<float> _NoiseTexture;

			SamplerState sampler_NoiseTexture;

			uniform float _Threshold;
			uniform float _FogDepth;
			uniform float _StepSize;
			uniform int _Samples;
			uniform float3 _ScrollDirection;
			uniform float _Height;
			uniform float4 _FogColour;
			uniform float _CloudScale;

			uniform float4 _FogTopCorner;
			uniform float2 _FogMaskSize;

			#define MAX_RAY_STEPS 256
			#define MAX_RAY_DIST  256.0
			#define MIN_RAY_DIST  0.001

			float map(float3 p) {
				/* Infinite plane: dot(p, n) + h */
				return dot(p, float3(0.0, 1.0, 0.0)) - _Height;
			}

			float rayMarch(float3 origin, float3 direction) {
				float dist = 0.0;
				for (int i = 0; i < MAX_RAY_STEPS; i++) {
					float3 p = origin + dist * direction;
					float hit = map(p);
					dist += hit;
					if (abs(hit) < MIN_RAY_DIST || dist > MAX_RAY_DIST) { break; }
				}
				return dist;
			}

			float2 worldPosToFogMaskPos(float3 worldPos) {
				float3 p = worldPos - _FogTopCorner;
				return p.xz;
			}

			float4 frag(v2f i) : SV_Target
			{
				float3 rayOrigin = _WorldSpaceCameraPos;
				float3 rayDirection = normalize(i.viewVector);

				float dist = rayMarch(rayOrigin, rayDirection);

				float4 col = tex2D(_MainTex, i.uv);

				float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);

				int maxSamples = min(_Samples, 32);

				float density = 0.0;
				float3 hitPoint = float3(0.0, 0.0, 0.0);
				if (dist > 0.0 && dist < MAX_RAY_DIST) {
					/* Ray trace from the hit point returned by the raymarch and
					 * take samples from the 3-D noise to determine the density of the
					 * fog at this pixel. */

					hitPoint = rayOrigin + rayDirection * dist;

					for (int i = 0; i < maxSamples; i++) {
						float3 p = hitPoint + rayDirection * (float(i) * _StepSize);

						float2 n = _NoiseTexture.SampleLevel(sampler_NoiseTexture, (p * _CloudScale) + _ScrollDirection * _Time.y, 0);

						float mainNoise = n.r;
						float detailNoise = n.g;

						float noise = mainNoise + detailNoise;

						/* TODO (George): Lighting. */

						density += max(0.0, noise - _Threshold);
					}
				}

				float2 hitPointMaskSpace = worldPosToFogMaskPos(hitPoint);
				hitPointMaskSpace /= _FogMaskSize;

				hitPointMaskSpace = clamp(hitPointMaskSpace, 0.0, 1.0);

				float maskVal = tex2D(_MaskTex, hitPointMaskSpace).r;

				density *= maskVal;

				density = exp(-density);
				float4 fogColour = (1.0 - density) * _FogColour;

				return col * density + fogColour;
			}
			ENDCG
		}
	}
}
