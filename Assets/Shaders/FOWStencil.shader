Shader "Unlit/FOWStencil" {
	Properties {
	}

	SubShader {
		Tags {
			"RenderType"="Opaque"
		}

		Pass {
			ZWrite Off
		}
	}
}
