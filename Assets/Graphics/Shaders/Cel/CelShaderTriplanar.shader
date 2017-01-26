Shader "Cel Shader/Triplanar" {
	Properties {
		_Scale("Texture Scaling",Float) = 1
		_DiffuseColorX ("Diffuse Color X", Color) = (1,1,1,1)
		_DiffuseColorY ("Diffuse Color Y", Color) = (1,1,1,1)
		_DiffuseColorZ ("Diffuse Color Z", Color) = (1,1,1,1)
		_XTex ("Diffuse Tex X", 2D) = "white" {}
		_YTex ("Diffuse Tex Y", 2D) = "white" {}
		_ZTex ("Diffuse Tex Z", 2D) = "white" {}
		_SpecularColorX ("Specular Color X", Color) = (1,1,1,1)
		_SpecularColorY ("Specular Color Y", Color) = (1,1,1,1)
		_SpecularColorZ ("Specular Color Z", Color) = (1,1,1,1)
		_XSpecularTex ("Specular Tex X",2D)="white" {}
		_YSpecularTex ("Specular Tex Y`",2D)="white" {}
		_ZSpecularTex ("Specular Tex Z",2D)="white" {}
		_Reflectivity ("Reflectivity",Range(0,1))=0
		_BaseLightLevel ("Base Light Level",Range(0,1)) = 0.5
		[MaterialToggle] _GlossDot("Super Tint", Range(0,1)) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#include "UnityCG.cginc"

		#define NO_SURF_FUNC
		#include "CelShaderLib.cginc"

		#pragma surface TriSurf CelShader nodynlightmap noambient

		#pragma target 3.0
		#pragma multi_compile_fog

		sampler2D _XTex,_YTex,_ZTex;
		sampler2D _XSpecularTex,_YSpecularTex,_ZSpecularTex;
		half4 _DiffuseColorX,_DiffuseColorY,_DiffuseColorZ;
		half4 _SpecularColorX,_SpecularColorY,_SpecularColorZ;
		half _Scale,_Reflectivity;

		struct Input{
			float3 worldRefl;
			float3 worldPos;
			half fog; 
		};

		void TriSurf(Input IN,inout SurfaceOutput o){
			half3 blending=abs(o.Normal);
			blending=normalize(max(blending,0.0001));//Force sum to 1
			blending/=dot(blending,1);

			fixed4 diffuse=
				tex2D(_XTex,IN.worldPos.yz*_Scale)*blending.x*_DiffuseColorX+
				tex2D(_YTex,IN.worldPos.xz*_Scale)*blending.y*_DiffuseColorY+
				tex2D(_ZTex,IN.worldPos.xy*_Scale)*blending.z*_DiffuseColorZ;
			//diffuse*=_DiffuseColor;
			fixed3 gloss= DecodeHDR(UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, IN.worldRefl),unity_SpecCube0_HDR);
			//Gloss processing: turn the reflection into k*_SpecularColor;
			//gloss=dot(gloss,_SpecularColor)*_SpecularColor;
			//alt: realistic (just multiply gloss by _SpecularColor)
			//gloss=lerp(gloss,dot(gloss,_SpecularColor)/3,saturate(_GlossDot));
			gloss*=
				blending.x*_SpecularColorX+
				blending.y*_SpecularColorY+
				blending.z*_SpecularColorZ;

			half lightRatio=
				tex2D(_XSpecularTex,IN.worldPos.yz)*blending.x*_SpecularColorX+
				tex2D(_YSpecularTex,IN.worldPos.xz)*blending.y*_SpecularColorY+
				tex2D(_ZSpecularTex,IN.worldPos.xy)*blending.z*_SpecularColorZ;
			lightRatio*=_Reflectivity;

			o.Albedo=diffuse*(1-lightRatio);
			o.Emission=gloss*lightRatio;
			o.Alpha=1;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
