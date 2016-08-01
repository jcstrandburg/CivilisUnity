Shader "Unlit/Blueprint" {
	Properties{
		_Color("Main Color", Color) = (1,1,1,1)
		_GhostColor("Ghost Color", Color) = (1,1,1,1)
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
	}

	SubShader{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 200

		CGPROGRAM
#pragma surface surf Lambert alpha

			sampler2D _MainTex;
			fixed4 _GhostColor;

			struct Input {
				float2 uv_MainTex;
			};

			void surf(Input IN, inout SurfaceOutput o) {
				//fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				//o.Albedo = c.rgb;
				o.Albedo = _GhostColor.rgb;
				o.Alpha = 0.4f;
			}
		ENDCG
	}
	Fallback "Transparent/VertexLit"
}
