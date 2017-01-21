using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class GuideHandler : MonoBehaviour {
	public Transform guideAnchor;
	public Vector3 guideLocalPos;

	public RectTransform onscreenGuide,offscreenGuide,offscreenGuideChild;

	CanvasScaler cs;

	public float offscreenChildY;

	public float onscreenMag;

	// Use this for initialization
	void Start () {
		cs=(CanvasScaler)Object.FindObjectOfType(typeof(CanvasScaler));

		onscreenGuide.gameObject.SetActive(true);
		offscreenGuide.gameObject.SetActive(false);

		offscreenChildY=cs.referenceResolution.y/2;
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 guidePos=guideAnchor.TransformPoint(guideLocalPos);
		Vector3 screenGuidePos=Camera.main.WorldToViewportPoint(guidePos);
		Vector2 viewportXY=new Vector2(screenGuidePos.x,screenGuidePos.y);
		onscreenGuide.anchorMax=onscreenGuide.anchorMin=viewportXY;
		//Debug.Log(onscreenGuide.localPosition);
		viewportXY-=Vector2.one*0.5f;
		onscreenMag=Vector2.Scale(viewportXY,cs.referenceResolution*cs.scaleFactor).magnitude;

		offscreenGuide.eulerAngles=Vector3.forward*(Mathf.Atan2(viewportXY.y,viewportXY.x*Camera.main.aspect)*Mathf.Rad2Deg);
		if ((Mathf.Abs(viewportXY.y)<Mathf.Abs(viewportXY.x)) || (viewportXY.y<0)) {
			offscreenGuide.eulerAngles=Vector3.forward*(Mathf.Atan2(1.0f,Mathf.Sign(viewportXY.x)*Mathf.Sign(viewportXY.y)*1.0f*Camera.main.aspect)*Mathf.Rad2Deg);	
		}
		//if (screenGuidePos.z<0)
		//	offscreenGuide.eulerAngles=-Vector3.forward*offscreenGuide.eulerAngles.z;
		offscreenGuideChild.anchoredPosition=Vector2.right*(offscreenChildY*cs.scaleFactor-27*2.7f);

		//localOnscreenGuide.anchorRelativePosition=new Vector2(screenGuidePos.x*800+screenGuidePos.y*600;
		if (onscreenMag>offscreenChildY*cs.scaleFactor-27*2||screenGuidePos.z<0){
			//Move + rotate the offscreen version
			//Set the pivot of the offscreen version to the point closest to the edge
			//if z==0 this works
			//
			//offscreenGuide.anchorMax=offscreenGuide.anchorMin=new Vector2(Mathf.Clamp01(viewportXY.x),Mathf.Clamp01(viewportXY.y))*0.9f;//viewportXY.normalized*0.5f+Vector2.one*0.5f;
			
			if (onscreenGuide.gameObject.active){
				onscreenGuide.gameObject.SetActive(false);
				offscreenGuide.gameObject.SetActive(true);
			}
		}else if (!onscreenGuide.gameObject.active){
			onscreenGuide.gameObject.SetActive(true);
			offscreenGuide.gameObject.SetActive(false);
		}
	}
}
