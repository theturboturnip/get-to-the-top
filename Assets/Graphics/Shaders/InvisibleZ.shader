Shader "Unlit/InvisibleZ"
{
	Properties
	{
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Geometry" }
		LOD 100

		Pass
		{
			Blend Zero One
			ZWrite On
			//ZTest Always
		}
	}
	Fallback "Diffuse"
}
