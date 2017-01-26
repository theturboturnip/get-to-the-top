Shader "Hidden/FadeOut"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_FadeColor("Fade Color", Color) = (0,0,0,1)
		//_FadeType("Fade Type", Int) = 0
		_FadeProgress("Fade Progress",Range(0,1)) = 0
		_FadeReverse("Fade Reverse", Int) = 0
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

			fixed4 _FadeColor;
			int _FadeType,_FadeReverse;
			half _FadeProgress;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float fadeState : TEXCOORD1;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				o.fadeState=_FadeProgress;
				/*half yProg=(o.vertex.y+1)/2;
				if (_FadeType==0) //Top-to-Bottom (y=1, fade=0) (_FadeProgress=0.5, y=1, fade=1) (_FadeProgress>0.5, y=0, fade=2*_FadeProgress)  if y=1 return 2*_FadeProgress else if y=0 return 2*_FadeProgress-1
					o.fadeState=lerp(2*_FadeProgress-1,2*_FadeProgress,yProg);
				else if (_FadeType==1) //Bottom-to-top
					o.fadeState=lerp(2*_FadeProgress-1,2*_FadeProgress,1-yProg);
				o.fadeState=saturate(o.fadeState);
				if (_FadeReverse==1)
					o.fadeState=1-o.fadeState;*/
				return o;
			}
			
			sampler2D _MainTex;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				col=lerp(col,_FadeColor,saturate(_FadeProgress));
				return col;
			}
			ENDCG
		}
	}
}
