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

	Material m_Material;

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
		Graphics.Blit(src,dest);
		if (fadeStart==-1) return;
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
			if (stopWhenFinished) return;
			
		}

		material.SetInt("_FadeInvert",invertFade?-1:1);
		material.SetInt("_FadeDirection",fadeDirection);
		material.SetFloat("_FadeProgress",fadeProgress);
		Graphics.Blit(src,dest,material);
	}
}
