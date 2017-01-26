Shader "Cel Shader/Transparent" {
	Properties {
		_DiffuseColor ("Diffuse Color", Color) = (1,1,1,1)
		_MainTex ("Diffuse Tex", 2D) = "white" {}
		_SpecularColor("Specular Color",Color)=(1,1,1,1)
		_SpecularTex ("Specular Tex",2D)="white" {}
		_Reflectivity ("Reflectivity",Range(0,1))=0
		_BaseLightLevel ("Base Light Level",Range(0,1)) = 0.5
		_TransCutoff("Alpha Cutoff",Range(0,1))=0
		_TransAfterCut("Alpha After Cutoff",Range(0,1))=0
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent+200" }
		LOD 200
		Cull Off
		ZWrite Off
		//Blend SrcAlpha OneMinusSrcAlpha
		//ZTest Always
		
		CGPROGRAM
		#include "UnityCG.cginc"
		#include "CelShaderLib.cginc"

		#pragma surface TransCelSurf CelShader nodynlightmap noambient alpha// vertex:myvert finalcolor:mycolor

		#pragma target 3.0
		//#pragma multi_compile_fog

		half _TransCutoff,_TransAfterCut;

		void TransCelSurf(Input IN,inout SurfaceOutput o){
			CelSurf(IN,o);
			//If the alphas below the cutoff, go away
			clip(o.Alpha-_TransCutoff);
			o.Alpha=lerp(o.Alpha,_TransAfterCut,saturate(_TransCutoff*100000));
		}
		ENDCG
	}
	FallBack "Diffuse"
}
