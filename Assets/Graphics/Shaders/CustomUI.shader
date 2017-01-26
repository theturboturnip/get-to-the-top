Shader "UI/Custom"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_ScanColorOne("Scanline Primary Color", Color) = (1,1,1,1)
		_ScanColorTwo("Scanline Secondary Color", Color) = (0.5,0.5,0.5,1)
		_ScanHeight("Scanline Height",Range(0.00001,1)) = 0.001
		
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask ("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass
		{
			Name "Default"
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile __ UNITY_UI_ALPHACLIP
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
			};
			
			fixed4 _Color,_OutlineColor,_ScanColorOne,_ScanColorTwo;
			fixed _ScanHeight;
			fixed4 _TextureSampleAdd;
			float4 _ClipRect;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

				OUT.texcoord = IN.texcoord;
				
				#ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.xy += (_ScreenParams.zw-1.0) * float2(-1,1) * OUT.vertex.w;
				#endif
				
				OUT.color = IN.color * _Color;
				return OUT;
			}

			sampler2D _MainTex;

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 startColor = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd);
				
				startColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
				
				#ifdef UNITY_UI_ALPHACLIP
				clip (startColor.a - 0.01);
				#endif

				//If we're not the outline i.e. magnitude > 0.8ish
					//Draw scanline 1 if we're on a scanline
					//Draw scanline 2 otherwise
				//Take y pos, divide by scanline height, take frac() and use as lerp
				fixed scanConst=abs(frac(IN.vertex.y/_ScanHeight));
				scanConst=pow((scanConst-0.5)*2,4);
				//if (scanConst>0.5)
				//	scanConst=1;
				//else
				//	scanConst=0;

				fixed4 nonOutline=lerp(_ScanColorOne,_ScanColorTwo,scanConst)*startColor * IN.color;
				//Else draw outline color

				fixed4 color=lerp(_OutlineColor,nonOutline,startColor.r*startColor.r);
				color.a=startColor.a*lerp(_OutlineColor.a,IN.color.a,startColor.r);
				return color;
			}
		ENDCG
		}
	}
}



/*
fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 startColor = tex2D(_MainTex, IN.texcoord);
				//clip (startColor.a - 0.01);

				//If we're not the outline i.e. magnitude > 0.8ish
					//Draw scanline 1 if we're on a scanline
					//Draw scanline 2 otherwise
				//Take y pos, divide by scanline height, take frac() and use as lerp
				fixed scanConst=abs(frac(IN.vertex.y/_ScanHeight));
				scanConst=pow((scanConst-0.5)*2,4);
				//if (scanConst>0.5)
				//	scanConst=1;
				//else
				//	scanConst=0;

				fixed4 nonOutline=lerp(_ScanColorOne,_ScanColorTwo,scanConst)*startColor * IN.color;
				//Else draw outline color

				fixed4 color=lerp(_OutlineColor,nonOutline,startColor.r*startColor.r);
				color.a=startColor.a*IN.color.a+0.1;

				return color;
			}
			*/