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
			sampler2D G_FOWPermMaskTexture;
			sampler2D G_FOWImpermMaskTexture;

			uniform float4 G_FOWColour;

			uniform float4 _PermTopCorner;
			uniform float4 _ImpermTopCorner;

			uniform float2 _PermMaskSize;
			uniform float2 _ImpermMaskSize;

			uniform float _PermHeight;
			uniform float _ImpermHeight;

			float2 worldPosToFogMaskPosPerm(float3 worldPos) {
				float3 p = worldPos - _PermTopCorner;
				return p.xz;
			}

			float2 worldPosToFogMaskPosImperm(float3 worldPos) {
				float3 p = worldPos - _ImpermTopCorner;
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
				float dist = rayVSPlane(float3(0.0, _PermHeight, 0.0), float3(0.0, 1.0, 0.0), rayOrigin, rayDirection);
				float3 hitPoint = rayOrigin + rayDirection * dist;

				float2 hitPointMaskSpace = worldPosToFogMaskPosPerm(hitPoint);
				hitPointMaskSpace /= _PermMaskSize;

				hitPointMaskSpace = clamp(hitPointMaskSpace, 0.0, 1.0);

				float permMaskVal = tex2D(G_FOWPermMaskTexture, hitPointMaskSpace).r;
				float impermMaskVal = tex2D(G_FOWImpermMaskTexture, hitPointMaskSpace).r;

				float3 sceneColour = tex2D(_MainTex, i.uv).rgb;
				return float4(sceneColour * (1.0 - (impermMaskVal * 0.95)) + (permMaskVal * impermMaskVal * G_FOWColour.rgb), 1.0);

			}
			ENDHLSL
		}
	}
}
