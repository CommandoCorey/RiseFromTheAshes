Shader "Unlit/FOW"
{
	SubShader
	{
		Tags {
			"RenderType"="Transparent"
			"RenderPipeline" = "UniversalRenderPipeline"
		}
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			#define RAY_STEPS 10
			#define RAY_STEP 1.0f

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			}; 

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float _Density;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				return float4(0.0f, 0.0f, 0.0f, 0.5);
			}
			ENDCG
		}
	}
}
