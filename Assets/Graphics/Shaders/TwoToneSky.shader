// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Skybox/TwoTone"
{
	Properties
	{
		_SunTex("Sun Texture", 2D) = "white" {}

		_Color ("Color", Color)=(1,1,1,1)
		_HorizonColor ("Horizon Color", Color)=(1,1,1,1)
		_HorizonOffset ("Horizon Offset", Range(0,1))=0
		_HorizonColorBlend ("Horizon Blend Amount", Range(0,1)) = 0.05
		_SunScale("Sun Size", Float) = 0.04
		_SunBrightness("Sun Brightness", Float) = 20
		_LayerCube("Dynamic Layer", Cube) = "" {}
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

			float _SunScale,_SunBrightness,_HorizonOffset,_HorizonColorBlend;
			float4 _Color,_HorizonColor;
			samplerCUBE _LayerCube;
			sampler2D _SunTex;

			half4 calcSunSpot(half3 vec1, half3 vec2){
				half3 delta = vec1 - vec2;
				half2 delta2=delta.xy;
				
				//return half4(saturate(delta2),0,1);
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

				/*half3 delta = _WorldSpaceLightPos0.xyz + i.rayDir.xyz;
				//length of the ray = sqrt(length of the light pos squared + sun size squared)
				//return half4(saturate(delta2),0,1);
				half dist = length(delta);
				if (dist<_SunScale){
					delta=normalize(delta)*sqrt(dot(_WorldSpaceLightPos0,_WorldSpaceLightPos0)+_SunScale*_SunScale);
					
					return half4(delta.xy/_SunScale,0,1);
					return tex2D(_SunTex,delta/_SunScale-0.5f);//half4(1,0,0,1);
				}*/
				half4 mieColor = calcSunSpot(_WorldSpaceLightPos0.xyz, -i.rayDir.xyz);
				col+=mieColor;

				//float4 cubeColor=texCUBE(_LayerCube,-i.rayDir.xyz);
				//col=lerp(col,cubeColor,cubeColor.a);//fixed4(cubeColor.rgb*cubeColor.a,0);

				return col;
			}
			ENDCG
		}
	}
}
