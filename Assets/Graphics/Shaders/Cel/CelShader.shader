Shader "Cel Shader/Normal" {
	Properties {
		_DiffuseColor ("Diffuse Color", Color) = (1,1,1,1)
		_MainTex ("Diffuse Tex", 2D) = "white" {}
		_SpecularColor("Specular Color",Color)=(1,1,1,1)
		_SpecularTex ("Specular Tex",2D)="white" {}
		_Reflectivity ("Reflectivity",Range(0,1))=0
		_BaseLightLevel ("Base Light Level",Range(0,1)) = 0.5
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#include "UnityCG.cginc"
		#include "CelShaderLib.cginc"
 		#pragma multi_compile_instancing
		#pragma surface CelSurf CelShader nodynlightmap noambient addshadow// vertex:myvert finalcolor:mycolor

		#pragma target 3.0
		#pragma multi_compile_fog
		ENDCG
	}
	FallBack "Diffuse"
}
