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
		[MaterialToggle] _ApplyFade("Should Apply Fade",Int) = 0

		[MaterialToggle] _ShouldMix("Should Mix", Int)=1
		_BackTex("Back Texture", 2D) = "white" {}
		_BackDepth("Back Depth Texture", 2D) = "white" {}
		_DepthParams("Depth Params",Vector)= (0,100,0,100)
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
				float2 uv_depth : TEXCOORD2;
				float4 vertex : SV_POSITION;
				float fadeData : TEXCOORD1;
				//float3 fogData : TEXCOORD2;
			};

			float _FadeProgress;
			uniform float4 _MainTex_TexelSize;

			int _FadeInvert,_FadeDirection;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv.xy;
				o.uv_depth = v.uv.xy;
		
				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					o.uv.y = 1-o.uv.y;
				#endif		
				//fadeData is the height of the fade line
				o.fadeData=_FadeProgress; 

				if (_FadeDirection==1)
					o.fadeData=1-o.fadeData;

				/*#if UNITY_UV_STARTS_AT_TOP
				if (_FadeDirection!=0)
					o.fadeData=1-o.fadeData;
				#endif*/
				
				//if _FadeDirection==-1 then do 1-fadeData i.e. 

				//fogData.xyz is the view direction

				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _BackTex,_CameraDepthTexture,_BackDepth;
			float4 _DepthParams;
			fixed4 _FadeColor;
			int _ShouldMix;

			fixed4 HandleFade(v2f i, fixed4 col){
				//if (i.fadeData<0) return col;
				fixed fragFade=0; //0 for original color, 1 for fade color
				if (_FadeDirection==0)
					fragFade=i.fadeData;
				else 
					//1 if uv.y-fadeData>0 & _FadeInvert=1
					fragFade=sign(i.uv_depth.y-i.fadeData)*_FadeInvert;
				fragFade=saturate(fragFade);
				return lerp(col,_FadeColor,fragFade);			
			}

			float LinearBackDepth(float rawBackDpth){
				//x is documented as 1-(far/near), y is documented as (far/near). The values in this struct are used to linearize depth in this exact way.
				float zBuffX=_ZBufferParams.x,zBuffY=_ZBufferParams.y;//This y value was found to be 1. AAAAAAAAAAAAAAA

				//After some reverse engineering...
				zBuffX=-(-_DepthParams.w/_DepthParams.z);
				zBuffY=1-_DepthParams.z/_DepthParams.w;//-_DepthParams.y/_DepthParams.x;
				//return (_ZBufferParams.y-zBuffY);
				#if defined(UNITY_REVERSED_Z)
				return 1.0 / ( zBuffX * rawBackDpth + zBuffY); //This works 
				#else
				return -1.0 / ( zBuffX * rawBackDpth + zBuffY); //This works 
				#endif
			} 

			fixed4 TexBlend(v2f i){
				//Get Depths
				float rawMainDpth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv_depth);
				float main01Dpth = Linear01Depth(rawMainDpth);
				/*float mainDpth = lerp(_DepthParams.x,_DepthParams.y,main01Dpth);
				float rawBackDpth = SAMPLE_DEPTH_TEXTURE(_BackDepth,i.uv_depth);
				float back01Dpth = LinearBackDepth(rawBackDpth);//Linear01Depth(rawBackDpth); //THIUS IS WRONG
				float backDpth = lerp(_DepthParams.z,_DepthParams.w,back01Dpth);*/

				//return _ZBufferParams;
				//return back01Dpth;//(rawBackDpth*10+10)/20;
				//return main01Dpth;
				//Get Colors
				fixed4 main=tex2D(_MainTex,i.uv),back=tex2D(_BackTex,i.uv);
				//return back01Dpth;
				//return main01Dpth;
				if (_ShouldMix==1&&(main01Dpth==1)){
					//if (length(main)<0.5)
					//	return 2*main*back;
					return back+main*(1-back);
					return 1-(1-main)*(1-back);//back/1+main;//*(1-back.a);//+main*0.5;
				}

				return main;
			}

			int _ApplyFade;
			fixed4 frag (v2f i) : SV_Target
			{
				//return 0;
				fixed4 col = TexBlend(i);
				//col=HandleFog(i,col);
				if (_ApplyFade==1)
					col=HandleFade(i,col);
				return col;
			}
			ENDCG
		}
		/*Pass
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
				float2 uv_depth : TEXCOORD2;
				float4 vertex : SV_POSITION;
			};

			uniform float4 _MainTex_TexelSize;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv.xy;
				o.uv_depth = v.uv.xy;
		
				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					o.uv.y = 1-o.uv.y;
				#endif		
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _BackTex,_CameraDepthTexture,_BackDepth;
			float4 _DepthParams;

			float LinearBackDepth(float rawBackDpth){
				//x is documented as 1-(far/near), y is documented as (far/near). The values in this struct are used to linearize depth in this exact way.
				float zBuffX=_ZBufferParams.x,zBuffY=_ZBufferParams.y;//This y value was found to be 1. AAAAAAAAAAAAAAA

				//After some reverse engineering...
				zBuffX=-(-_DepthParams.w/_DepthParams.z);
				zBuffY=1-_DepthParams.z/_DepthParams.w;//-_DepthParams.y/_DepthParams.x;
				//return (_ZBufferParams.y-zBuffY);
				return 1.0 / ( zBuffX * rawBackDpth + zBuffY); //This works 
			} 

			fixed4 TexBlend(v2f i){
				//Get Depths
				float rawMainDpth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv_depth);
				float main01Dpth = Linear01Depth(rawMainDpth);
				float mainDpth = lerp(_DepthParams.x,_DepthParams.y,main01Dpth);
				float rawBackDpth = SAMPLE_DEPTH_TEXTURE(_BackDepth,i.uv_depth);
				float back01Dpth = LinearBackDepth(rawBackDpth);//Linear01Depth(rawBackDpth); //THIUS IS WRONG
				float backDpth = lerp(_DepthParams.z,_DepthParams.w,back01Dpth);

				//return _ZBufferParams;
				//return back01Dpth;//(rawBackDpth*10+10)/20;
				//return main01Dpth;
				//Get Colors
				fixed4 main=tex2D(_MainTex,i.uv),back=tex2D(_BackTex,i.uv);

				if (backDpth<mainDpth || main01Dpth==1){
					return back+main*(1-back);//+main*0.5;
				}

				return main;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = TexBlend(i);//tex2D(_MainTex, i.uv);
				return col;
			}
			ENDCG
		}*/
	}
}
