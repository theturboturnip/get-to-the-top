Shader "Cel Shader/Double Sided Color" {
	Properties {
		_DiffuseColor ("Diffuse Color", Color) = (1,1,1,1)
		_MainTex ("Diffuse Tex", 2D) = "white" {}
		_SpecularColor("Specular Color",Color)=(1,1,1,1)
		_SpecularTex ("Specular Tex",2D)="white" {}
		_Reflectivity ("Reflectivity",Range(0,1))=0
		_BaseLightLevel ("Base Light Level",Range(0,1)) = 0.5
		_BackFaceColor ("Back Face", Color) = (0,0,0,1)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		Cull Back
		
		CGPROGRAM
		#include "UnityCG.cginc"
		#include "CelShaderLib.cginc"

		#pragma surface CelSurf CelShader nodynlightmap noambient// vertex:myvert finalcolor:mycolor

		#pragma target 3.0
		#pragma multi_compile_fog
		ENDCG

		Pass{
			Cull Front
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag

			struct v2f{
				float4 vertex : SV_POSITION;
			};

			fixed4 _BackFaceColor;

			v2f vert (appdata_base v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return _BackFaceColor;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
