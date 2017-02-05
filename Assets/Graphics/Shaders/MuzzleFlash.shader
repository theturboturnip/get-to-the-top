Shader "Unlit/MuzzleFlash"
{
	Properties
	{
		_StartColor ("StartColor", Color) = (1,1,1,1) 
		_EndColor ("End Color",Color) = (1,.4,0,1)
		_ScaleDist ("Color Scaling Distance",Float) = 0.1 
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Geometry"}
		LOD 100
		Cull Front
		ZWrite On
		Offset 0,-1

		Pass
		{

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 color : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			float4 _StartColor,_EndColor;
			float _ScaleDist;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				//o.vertex.z=1;
				o.color=lerp(_StartColor,_EndColor,length(v.vertex)/_ScaleDist);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = i.color;
				// apply fog
				return col;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
