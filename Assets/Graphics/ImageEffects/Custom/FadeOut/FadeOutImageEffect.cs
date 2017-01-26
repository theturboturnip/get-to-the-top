using UnityEngine;
using System.Collections;

public enum FadeType{
	TopToBottom=0,
	BottomToTop=1,
	AllCorners=2
};

public class FadeOutImageEffect : MonoBehaviour {
	public Shader shader;

	public Color fadeColor;
	//public FadeType fadeType;
	public bool reverseFade;
	public float fadeTime;
	public bool autoStart=false;
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

		if (autoStart)
			StartFade();
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

	public void StartFade(){
		fadeStart=Time.unscaledTime;
		material.SetColor("_FadeColor",fadeColor);
		//material.SetInt("_FadeType",(int)fadeType);
		if (reverseFade)
			material.SetInt("_FadeReverse",1);
		else
			material.SetInt("_FadeReverse",0);

	}

	public void ResetFade(){
		fadeStart=-1;
	}

	protected virtual void OnDisable(){
		if (m_Material){
			DestroyImmediate(m_Material);
		}
	}

	void OnRenderImage(RenderTexture src,RenderTexture dest){
		if (fadeStart<0) return;
		material.SetFloat("_FadeProgress", (Time.unscaledTime-fadeStart)/fadeTime);
		//material.SetFloat("_DepthCutoff",depthCutoff);
		Graphics.Blit(src,dest,material);
	}
}
