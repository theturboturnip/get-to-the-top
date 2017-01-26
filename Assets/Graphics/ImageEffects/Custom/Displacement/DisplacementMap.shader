Shader "Hidden/DisplacementMap"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {} //This is the image
		_Displacement ("Displacement Map", 2D) = "grey" {} //This is the displacement map
		_DisplacementAmount ("Displacement Magnitude", Float) = 0
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
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex,_Displacement;
			half _DisplacementAmount;

			fixed4 frag (v2f i) : SV_Target
			{
				half4 dis= tex2D(_Displacement, i.uv);
				half2 newUV=i.uv+dis*_DisplacementAmount;
				fixed4 col = tex2D(_MainTex, newUV);
				// just invert the colors
				return col;
			}
			ENDCG
		}
	}
}
