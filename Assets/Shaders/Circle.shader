Shader "Unlit/Circle"
{
    Properties
    {
        _Colour("Colour", Color) = (1, 0, 0, 1)
        _Thiccness("Thiccness", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };


            uniform float4 _Colour;
            uniform float _Thiccness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float m = length(i.uv - float2(0.5, 0.5));

                if (m < 0.5 - _Thiccness / 100) {
                    discard;
                }

                if (m > 0.5) {
                    discard;
                }

                return float4(_Colour.rgb, 1);
            }
            ENDCG
        }
    }
}
