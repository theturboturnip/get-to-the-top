Shader "Custom/DecalShader" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BaseLightLevel("Base Light Level", Range(0,1)) = 0.5
	}
	SubShader {
		Tags { "RenderType"="Opaque" "Queue"="AlphaTest" "ForceNoShadowCasting"="False" }
    	LOD 200
		Offset -1, -1
		Cull Off
		
		CGPROGRAM
		#define NO_SURF_FUNC
		#include "CelShaderLib.cginc"
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf CelShader noambient addshadow//decal:blend //alpha

		sampler2D _MainTex;
		//half _BaseLightLevel;

		struct Input {
			float2 uv_MainTex;
		};

		/*half4 LightingDecal(SurfaceOutput s, half3 lightDir,half atten){
			half NdotL=dot(s.Normal,lightDir);
			atten*=smoothstep(0,0.025f,NdotL);
			atten=max(atten,_BaseLightLevel);
			half4 c;
			c.rgb=s.Albedo*atten;
			c.a=s.Alpha;
			return c;
		}*/

		void surf (Input IN, inout SurfaceOutput o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			clip(c.a-0.5);
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
