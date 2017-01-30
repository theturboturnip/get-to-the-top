/*
NOTES:
	This image effect has to do:
		Fog (height only)
		Fade In/Out
		Blur?
*/
Shader "Hidden/GTTTCompleteShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_FadeProgress("Fade Progress", Range(0,1)) = 0
		_FadeInvert("Fade Invert", Int) = 1 //-1 for inversion i.e. fading from fadecolor to texture
		_FadeDir("Fade Direction", Int) = 1 //-1 for inversion i.e. fading from bottom instead of top
		_FadeColor("Fade Color", Color) = (0,0,0,1)

		_FogHeight("Fog Height", Float) = -10
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float fadeData : TEXCOORD1;
				//float3 fogData : TEXCOORD2;
			};

			float _FadeProgress;
			int _FadeInvert,_FadeDirection;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				//fadeData is the height of the fade line
				o.fadeData=_FadeProgress; 
				if (_FadeDirection==-1)
					o.fadeData=1-o.fadeData;
				#if UNITY_UV_STARTS_AT_TOP
				if (_FadeDirection!=0)
					o.fadeData=1-o.fadeData;
				#endif
				//if _FadeDirection==-1 then do 1-fadeData i.e. 

				//fogData.xyz is the view direction

				return o;
			}
			
			sampler2D _MainTex;
			fixed4 _FadeColor;

			fixed4 HandleFade(v2f i, fixed4 col){
				fixed fragFade=0; //0 for original color, 1 for fade color
				if (_FadeDirection==0)
					fragFade=i.fadeData;
				else 
					//1 if uv.y-fadeData>0 & _FadeInvert=1
					fragFade=sign(i.uv.y-i.fadeData)*_FadeInvert;
				fragFade=saturate(fragFade);
				return lerp(col,_FadeColor,fragFade);			
			}

			/*fixed4 HandleFog(v2f i, fixed4 col){
				//if the camera ray goes down to the target height, then fog.
				//take the depth buffer value
				//multiply it by projection to world space matrix;
				//take y 
				//output white if y<target
				return col;
			}*/

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				//col=HandleFog(i,col);
				col=HandleFade(i,col);
				return col;
			}
			ENDCG
		}
	}
}
