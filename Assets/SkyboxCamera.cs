using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxCamera : MonoBehaviour {
	Camera c,targetC;
	public GTTTCompleteImageEffect target;
	RenderTexture tempColor,tempDepth;
	// Use this for initialization
	void Start(){
		targetC=target.gameObject.GetComponent<Camera>();
		c=GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void TargetPreRender(Camera rendering){
		
		if (rendering!=targetC) return;
		if (tempColor!=null){
			RenderTexture.ReleaseTemporary(tempColor);
			RenderTexture.ReleaseTemporary(tempDepth);
		}
		tempColor=RenderTexture.GetTemporary(Screen.width,Screen.height,0,RenderTextureFormat.Default,RenderTextureReadWrite.Default);
		tempDepth=RenderTexture.GetTemporary(Screen.width,Screen.height,24,RenderTextureFormat.Depth);

		c.targetTexture=tempColor;
		c.Render();
		c.targetTexture=tempDepth;
		c.Render();

		target.backColor=tempColor;
		target.backDepth=tempDepth;
		target.secondaryNear=c.nearClipPlane;
		target.secondaryFar=c.farClipPlane;
	}

	void TargetPostRender(Camera rendering){
		if (rendering!=targetC) return;
		
	}

	void OnEnable(){
		Camera.onPreRender+=TargetPreRender;
		Camera.onPostRender+=TargetPostRender;
	}

	void OnDisable(){
		Camera.onPreRender-=TargetPreRender;
		Camera.onPostRender-=TargetPostRender;
	}
}
