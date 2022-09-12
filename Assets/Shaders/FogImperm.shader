
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
			sampler2D _LastCameraDepthTexture;
			sampler2D _MaskTex;
			sampler2D _AffectedObjects;
			Texture2D<float> _AffectedDepth;

			SamplerState sampler_AffectedDepth;

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

				float density = 0.0;
				float3 hitPoint = float3(0.0, 0.0, 0.0);
				if (dist > 0.0 && dist < MAX_RAY_DIST) {
					hitPoint = rayOrigin + rayDirection * dist;
				}

				float2 hitPointMaskSpace = worldPosToFogMaskPos(hitPoint);
				hitPointMaskSpace /= _FogMaskSize;

				hitPointMaskSpace = clamp(hitPointMaskSpace, 0.0, 1.0);

				float maskVal = tex2D(_MaskTex, hitPointMaskSpace).r;

				//float affectedDepth = _AffectedDepth.SampleLevel(sampler_AffectedDepth, i.uv, 0).r;
				//affectedDepth = Linear01Depth(affectedDepth);

				//float sceneDepth = SAMPLE_DEPTH_TEXTURE(_LastCameraDepthTexture, i.uv);
				//sceneDepth = Linear01Depth(sceneDepth);

				//float4 affectedObjectsColour = tex2D(_AffectedObjects, i.uv);

				//affectedObjectsColour.a *= 1.0 - maskVal;

				/* TODO (George): Depth test the affected objects */
				//if (affectedDepth > sceneDepth) {
				//	affectedObjectsColour = float4(0.0, 0.0, 0.0, 0.0);
				//}

				float4 col = tex2D(_MainTex, i.uv);

				col.rgb *= (1.0 - (maskVal - _FogColour.rgb * _FogColour.a));

				//float3 sceneColour = affectedObjectsColour.rgb * affectedObjectsColour.a + col.rgb * (1.0 - affectedObjectsColour.a);
				float3 sceneColour = col.rgb;

				return float4(sceneColour, 1.0);
			}
			ENDCG
		}
	}
}

