﻿Shader "Custom/MyTest" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;

			float4 frag(v2f_img i) : COLOR {
				return tex2D(_MainTex, i.uv);
			}
			ENDCG
		}
	}
}
