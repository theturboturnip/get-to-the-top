using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FPSCount : MonoBehaviour {
	Text fpsText;
	//Timer t;
	public float frameTime=0;
	// Use this for initialization
	void Start () {
		fpsText=GetComponent<Text>();
	}

	public void Update(){
		if (Time.timeScale==0) return;
		//t.Start();
		frameTime+=(Time.smoothDeltaTime-frameTime)*0.1f;
		int fps=(int)(1f/Time.smoothDeltaTime);
		fpsText.text=fps+" FPS";
		/*if(DynamicSkybox.currentSkybox!=null)
			fpsText.text+="\nDSRT "+DynamicSkybox.currentSkybox.renderTime.ToString("F2")+"ms";*/
	}
	
	// Update is called once per frame
	public void CustomPostRender (Camera c) {
		
	}

	void OnEnable(){
		//Camera.onPreRender+=CustomPreRender;
		//Camera.onPostRender+=CustomPostRender;
	}
}
