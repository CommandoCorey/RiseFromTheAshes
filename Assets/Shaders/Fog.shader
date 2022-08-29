Shader "Hidden/Fog"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_MaskTex("Mask Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
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
			sampler2D _MaskTex;

			uniform float _Threshold;
			uniform float _FogDepth;
			uniform float _StepSize;
			uniform int _Samples;
			uniform float3 _ScrollDirection;

			uniform float4x4 _FogTransform;

			#define MAX_RAY_STEPS 256
			#define MAX_RAY_DIST  256.0
			#define MIN_RAY_DIST  0.001

			/* TODO: Better noise function.
			 * This one is from: https://gist.github.com/patriciogonzalezvivo/670c22f3966e662d2f83 */
			float mod289(float x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
			float4 mod289(float4 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
			float4 perm(float4 x) { return mod289(((x * 34.0) + 1.0) * x); }

			float noise(float3 p) {
				float3 a = floor(p);
				float3 d = p - a;
				d = d * d * (3.0 - 2.0 * d);

				float4 b = a.xxyy + float4(0.0, 1.0, 0.0, 1.0);
				float4 k1 = perm(b.xyxy);
				float4 k2 = perm(k1.xyxy + b.zzww);
				
				float4 c = k2 + a.zzzz;
				float4 k3 = perm(c);
				float4 k4 = perm(c + 1.0);
				
				float4 o1 = 1.0 - floor(k3 * (1.0 / 41.0));
				float4 o2 = 1.0 - floor(k4 * (1.0 / 41.0));
			
				float4 o3 = o2 * d.z + o1 * (1.0 - d.z);
				float2 o4 = o3.yw * d.x + o3.xz * (1.0 - d.x);

				return o4.y * d.y + o4.x * (1.0 - d.y);
			}

			float map(float3 p) {
				/* Infinite plane: dot(p, n) + h */
				return dot(p, float3(0.0, 1.0, 0.0));
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

			float4 frag(v2f i) : SV_Target
			{
				float3 rayOrigin = _WorldSpaceCameraPos;
				float3 rayDirection = normalize(i.viewVector);

				float dist = rayMarch(rayOrigin, rayDirection);

				float4 col = tex2D(_MainTex, i.uv);

				if (dist > 0.0 && dist < MAX_RAY_DIST) {
					/* Ray trace from the hit point returned by the raymarch and
					 * take samples from the 3-D noise to determine the density of the
					 * fog at this pixel. */

					float3 hitPoint = rayOrigin + rayDirection * dist;

					float density = 0.0;
					for (int i = 0; i < _Samples; i++) {
						float3 p = hitPoint + rayDirection * (float(i) * _StepSize);

						float n = noise(p + _ScrollDirection * _Time.y);

						density += max(0.0, n - _Threshold);
					}

					col = float4(density, density, density, 1.0);
				}

				return col;
			}
			ENDCG
		}
	}
}
