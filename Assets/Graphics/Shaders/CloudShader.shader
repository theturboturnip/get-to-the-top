Shader "Unlit/CloudShader"
{
	Properties
	{
		_MainTex ("Noise Texture", 2D) = "white" {}
		_MainTexSize("Noise Texture Resolution", Vector) = (128,128,0,0)
		_BaseColor ("Base Color", Color)=(1,1,1,0)
		_CloudColor("Second Color", Color)=(1,1,1,1)
		_ClampToBlack("Clamp outside to black", Int) = 0
		_NoNoiseProcessing("Disable filtering", Int) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent"}
		LOD 200
		ZWrite Off
    	Blend SrcAlpha OneMinusSrcAlpha
    	Cull Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			//#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 uv : TEXCOORD0;
				//UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				//float2 texelOffset : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			half4 _BaseColor,_CloudColor;
			half4 _MainTexSize;
			int _ClampToBlack,_NoNoiseProcessing;
			//float2 texelOffset=float2(_MainTex_ST.x/_MainTexSize,_MainTex_ST.y/_MainTexSize);
			//float texelSqrRadius=texelOffset.x*texelOffset.x/4;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv=float4(0,0,0,0);
				o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv.zw = float2(1.0/_MainTexSize.x,1.0/_MainTexSize.y);
				//UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed SendToOne(fixed val){
				return (sign(val-0.5));
			}

			fixed4 Fixed4(fixed val){
				return fixed4(val,val,val,val);
			}

			fixed GetRawNoiseTexVal(float2 uv){
				if ((uv.x<0||uv.y<0||uv.x>1||uv.y>1)&&_ClampToBlack==1)
					return 0;
				return tex2D(_MainTex,uv);
			}

			fixed SampleNoise(float4 uv){
				//determine the offset from centre of texel, in terms of 0.5 texels
				float2 centreOffset=frac(uv.xy/uv.zw)*2-1;
				float2 texelCentre=0.5*uv.zw+uv.xy-frac(uv.xy/uv.zw)*uv.zw;
				float centreSqrDist=dot(centreOffset,centreOffset);
				fixed mainVal=GetRawNoiseTexVal(uv.xy);
				mainVal=SendToOne(mainVal);
				if (centreSqrDist<=1.01 || _NoNoiseProcessing==1){
					return (mainVal);
				}

				//find the horiz+vert texels around us 
				//sample from the centre of the texels to remove inconsistencies
				fixed horizVal=GetRawNoiseTexVal(float2(texelCentre.x+uv.z*sign(centreOffset.x),texelCentre.y));
				fixed vertVal =GetRawNoiseTexVal(float2(texelCentre.x,texelCentre.y+uv.w*sign(centreOffset.y)));
				fixed diagVal =GetRawNoiseTexVal(float2(texelCentre.x+uv.z*sign(centreOffset.x),texelCentre.y+uv.w*sign(centreOffset.y)));
				horizVal=SendToOne(horizVal);
				vertVal =SendToOne(vertVal);
				diagVal =SendToOne(diagVal);
				
				if (horizVal!=mainVal&&vertVal!=mainVal&&diagVal!=mainVal)
					return -mainVal;
				if (horizVal!=mainVal&&vertVal!=mainVal&&diagVal==mainVal)
					return 1;
				return mainVal;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed a=saturate(SampleNoise(i.uv));
				if (_NoNoiseProcessing&&false){
					float2 centreOffset=frac(i.uv.xy/i.uv.zw)*2-1;
					fixed lineWidth=0.05;
					if (centreOffset.y>0&&centreOffset.y<lineWidth)
						return fixed4(0,1,0,1);
					else if (centreOffset.x>0&&centreOffset.x<lineWidth)
						return fixed4(0,1,0,1);
					else if (centreOffset.x>1-lineWidth||centreOffset.y>1-lineWidth)
						return fixed4(1,0,0,1);
				}

				// apply fog
				//UNITY_APPLY_FOG(i.fogCoord, col);
				return lerp(_BaseColor,_CloudColor,a);
			}
			ENDCG
		}
	}
}
