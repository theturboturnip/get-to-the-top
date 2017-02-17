using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GTTTCompleteImageEffect : MonoBehaviour {
	public Shader shader;

	[Header("Fade Options")]
	public bool invertFade=false;
	public bool stopWhenFinished=true,invertWhenFinished=false,startOnStart=false;
	public int fadeDirection=0;
	//public bool topToBottomFade=true;
	[Range(0,1)]
	public float fadeProgress=0;
	public float fadeTime; 
	public bool permaFade=false;
	float fadeStart=-1;

	[Header("Back Tex")]
	public Texture backColor;
	public Texture backDepth;
	public float secondaryNear,secondaryFar;
	public bool shouldMix=true;

	Material m_Material;
	Camera myCamera;

	void Start(){
			// Disable if we don't support image effects
		if (!SystemInfo.supportsImageEffects){
			enabled = false;
			return;
		}

		// Disable the image effect if the shader can't
		// run on the users graphics card
		if (!shader || !shader.isSupported)
			enabled = false;

		if (startOnStart)
			StartFading();

		myCamera=GetComponent<Camera>();
		myCamera.depthTextureMode=DepthTextureMode.Depth;
	}

	protected Material material{
		get{
			if (m_Material == null){
				m_Material = new Material(shader);
				m_Material.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_Material;
		}
	}

	protected virtual void OnDisable(){
		if (m_Material){
			DestroyImmediate(m_Material);
		}
	}

	public void StartFading(){
		fadeStart=Time.time;
	}

	public void StopFading(){
		fadeStart=-1;
	}

	void OnRenderImage(RenderTexture src,RenderTexture dest){
		//RenderTexture intermediate=RenderTexture.GetTemporary(Screen.width,Screen.height);
		/*material.SetPass(1);
		material.SetTexture("_BackTex",backColor);
		material.SetTexture("_BackDepth",backDepth);
		Vector4 depthParams=new Vector4(myCamera.nearClipPlane,myCamera.farClipPlane,secondaryNear,secondaryFar);
		material.SetVector("_DepthParams",depthParams);
		Graphics.Blit(src,intermediate,material);*/
		//Graphics.Blit(src,intermediate);
		//Graphics.Blit(src,dest);
		bool fade=true;
		if (fadeStart==-1) fade=false;
		/*if (fadeProgress==1 && invertWhenFinished&&!stopWhenFinished){
				invertFade=invertFade;
				fadeStart=Time.time;
				fadeDirection=-fadeDirection;
				fadeProgress=0;
				stopWhenFinished=true;
			}*/
		if (!permaFade)
			fadeProgress=(Time.time-fadeStart)/fadeTime;
		
		if (fadeProgress>1){
			//fadeProgress=1;
			if (stopWhenFinished) fade=false;
			
		}

		material.SetInt("_ShouldMix",(shouldMix&&backColor!=null)?1:0);
		material.SetTexture("_BackTex",backColor);
		material.SetTexture("_BackDepth",backDepth);
		Vector4 depthParams=new Vector4(myCamera.nearClipPlane,myCamera.farClipPlane,secondaryNear,secondaryFar);
		material.SetVector("_DepthParams",depthParams);
		material.SetInt("_FadeInvert",invertFade?-1:1);
		material.SetInt("_FadeDirection",fadeDirection);
		material.SetFloat("_FadeProgress",fadeProgress);
		material.SetInt("_ApplyFade",fade?1:0);
		//material.SetPass(0);
		Graphics.Blit(src,dest,material);
	}
}
