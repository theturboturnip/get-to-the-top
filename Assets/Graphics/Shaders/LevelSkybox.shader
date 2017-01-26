Shader "Skybox/LevelSkybox"
{
	Properties
	{
		/*

		
		//_LayerCube("Dynamic Layer", Cube) = "" {}*/

		//Sun Data
		_SunScale("Sun Size", Float) = 0.04
		_SunBrightness("Sun Brightness", Float) = 20
		//Top Color
		//Horizon Color
		_Color ("Color", Color)=(1,1,1,1)
		_HorizonColor ("Horizon Color", Color)=(1,1,1,1)
		_HorizonOffset ("Horizon Offset", Range(0,1))=0
		_HorizonColorBlend ("Horizon Blend Amount", Range(0,1)) = 0.05
		//Cloud Data
		_CloudColor("Cloud Color", Color)=(1,1,1,1)
		_CloudStart ("Cloud Start", Range(0,1)) =0
		_CloudFade("Cloud Fade", Range(0.0001,1)) =0.025
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			float _SunScale,_SunBrightness,_HorizonOffset,_HorizonColorBlend,_CloudStart,_CloudFade;
			float4 _Color,_HorizonColor,_CloudColor;
			samplerCUBE _LayerCube;

			half calcSunSpot(half3 vec1, half3 vec2){
				half3 delta = vec1 - vec2;
				half dist = length(delta);
				half spot = 1.0 - smoothstep(0.0, _SunScale, dist);
				return _SunBrightness * spot * spot;
			}

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				half4 rayDir : TEXCOORD0;
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				float3 eyeRay = normalize(mul((float3x3)unity_ObjectToWorld, v.vertex.xyz));

				o.rayDir = half4(half3(-eyeRay),(v.vertex.y+1)/2);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// only use horizon color after the cutoff

				//fixed4 col=_HorizonColor;// = lerp(_Color,_HorizonColor,saturate(1-i.rayDir.w-_HorizonOffset));
				//if height> horizonoffset+horizon blend return Color (p=0)
				//if height< horizonoffset-horizon blend return horizon (p=1)
				//else p=(height-(offset+blend))/-2*blend
				float horizonProgress=saturate(((i.rayDir.w)-_HorizonOffset-_HorizonColorBlend)/(-2*_HorizonColorBlend));
				fixed4 col=lerp(_Color,_HorizonColor,horizonProgress);
			
				half mie = calcSunSpot(_WorldSpaceLightPos0.xyz, -i.rayDir.xyz);
				col+=mie*_LightColor0;

				float cloudProgress=1-smoothstep(0,_CloudFade,i.rayDir.w-_CloudStart);
				col+=cloudProgress*_CloudColor;

				//float4 cubeColor=texCUBE(_LayerCube,-i.rayDir.xyz);
				//col=lerp(col,cubeColor,cubeColor.a);//fixed4(cubeColor.rgb*cubeColor.a,0);

				return col;
			}
			ENDCG
		}
	}
}
