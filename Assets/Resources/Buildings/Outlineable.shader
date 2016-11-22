Shader "Outlined/Silhouette" {
	Properties{
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_Outline("Outline width", Range(0.0, 0.03)) = .005
	}

	CGINCLUDE
	#include "UnityCG.cginc"

	struct appdata {
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		float2 uv : TEXCOORD0;
	};

	struct v2f {
		float4 pos : SV_POSITION;
		float4 color : COLOR;
		float2 uv : TEXCOORD0;
	};

	uniform float _Outline;
	uniform float4 _OutlineColor;

	v2f rVert(appdata v) {
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.color.r = 0;
		o.color.g = 0;
		o.color.b = 0;
		o.color.a = 0;
		o.uv = v.uv;
		return o;
	}

	v2f vert(appdata v) {
		// just make a copy of incoming vertex data but scaled according to normal direction
		v2f o;

		v.vertex.xyz += v.normal.xyz * _Outline;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		//o.pos.xyz += v.normal.xyz * _Outline;

		/*float3 norm = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
		float2 offset = TransformViewToProjection(norm.xy);

		o.pos.xy += offset * o.pos.z * _Outline;*/
		o.color = _OutlineColor;
		return o;
	}
	ENDCG

	SubShader{
		Tags{ "Queue" = "Transparent" }

		//Pass{
		//	Name "BASE"
		//	Cull Back
		//	Blend Zero One

		//	// uncomment this to hide inner details:
		//	//Offset -8, -8

		//	SetTexture[_OutlineColor]{
		//		ConstantColor(0,0,0,0)
		//		Combine constant
		//	}
		//}

		// note that a vertex shader is specified here but its using the one above
		Pass{
			Name "OUTLINE"
			Tags{ "LightMode" = "Always" }
			Cull Front

			// you can choose what kind of blending mode you want for the outline
			//Blend SrcAlpha OneMinusSrcAlpha // Normal
			//Blend One One // Additive
			Blend One OneMinusDstColor // Soft Additive
									   //Blend DstColor Zero // Multiplicative
									   //Blend DstColor SrcColor // 2x Multiplicative

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			half4 frag(v2f i) :COLOR{
				return i.color;
			}
			ENDCG
		}

		// note that a vertex shader is specified here but its using the one above
		//Pass{
		//	Name "OUTLINE"
		//	Tags{ "LightMode" = "Always" }
		//	Cull Front

		//	// you can choose what kind of blending mode you want for the outline
		//	//Blend SrcAlpha OneMinusSrcAlpha // Normal
		//	//Blend One One // Additive
		//	Blend One OneMinusDstColor // Soft Additive
		//								//Blend DstColor Zero // Multiplicative
		//								//Blend DstColor SrcColor // 2x Multiplicative

		//	CGPROGRAM
		//	#pragma vertex rVert
		//	#pragma fragment frag

		//	half4 frag(v2f i) :COLOR{
		//		half4 c;
		//		return c;
		//	}
		//	ENDCG
		//}
	}

	Fallback "Diffuse"
}