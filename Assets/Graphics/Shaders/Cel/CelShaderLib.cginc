#ifndef NO_SURF_FUNC
//Surf-specific vars
sampler2D _MainTex,_SpecularTex;
half _Reflectivity;
fixed4 _DiffuseColor,_SpecularColor;
half _GlossDot;

//Input
struct Input {
	float2 uv_MainTex;
	float2 uv_SpecularTex;
	float3 worldRefl;
	half fog;
	float4 color : COLOR; 
};

void CelSurf(Input IN, inout SurfaceOutput o){
	//Calculate glossy and diffuse colors
	fixed4 diffuse= tex2D(_MainTex, IN.uv_MainTex)*_DiffuseColor;
	fixed3 gloss= DecodeHDR(UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, IN.worldRefl),unity_SpecCube0_HDR);

	//Gloss processing: turn the reflection into k*_SpecularColor;
	//gloss=dot(gloss,_SpecularColor)*_SpecularColor;
	//alt: realistic (just multiply gloss by _SpecularColor)
	gloss=lerp(gloss,dot(gloss,_SpecularColor),_GlossDot);
	gloss*=_SpecularColor;

	//Calculate diffuse:glossy ratio
	half lightRatio=_Reflectivity*tex2D(_SpecularTex,IN.uv_SpecularTex);
	
	//Setup output
	o.Albedo=diffuse.rgb*(1-lightRatio)*IN.color.rgb;
	o.Emission=gloss*lightRatio*IN.color.rgb;
	o.Alpha=diffuse.a;
}
#endif

half _BaseLightLevel;

half4 LightingCelShader(SurfaceOutput s, half3 lightDir, half3 viewDir,half atten){
	//Calculate light level
	half NdotL=dot(s.Normal,lightDir);
	NdotL=smoothstep(0,0.025f,NdotL);
	atten=smoothstep(0,0.025f,atten); //Get smoother shadowmaps

	//If this is the base pass, add ambient term
	half lightLevel=NdotL*atten;
	#ifndef UNITY_PASS_FORWARDADD
	lightLevel=max(lightLevel,_BaseLightLevel);
	#else
	lightLevel=smoothstep(0,0.025f,lightLevel);
	#endif

	//Calculate color
	half4 c;
	c.rgb=s.Albedo*lightLevel*_LightColor0.rgb;
	//c.rgb=lightLevel;
	c.a=s.Alpha;
	return c;
}