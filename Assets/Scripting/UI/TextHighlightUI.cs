using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class TextHighlightUI : MouseOverReceiver {
	Vector2 targetAnchorMin,targetAnchorMax;
	Vector2 startAnchorMin,startAnchorMax;
	Vector2 oldAnchorMin,oldAnchorMax;
	Image img;
	RectTransform rt;
	public float lerpTime=0.3f;
	float timeTaken=0;

	void Start(){
		rt=GetComponent<RectTransform>();
		targetAnchorMin=rt.anchorMin;
		targetAnchorMax=rt.anchorMax;
		startAnchorMin=rt.anchorMin;
		startAnchorMax=rt.anchorMax;
	}

	void Update(){
		timeTaken=Mathf.Clamp(timeTaken+Time.deltaTime,0,lerpTime);
		rt.anchorMin=Vector2.Lerp(oldAnchorMin,targetAnchorMin,timeTaken/lerpTime);
		rt.anchorMax=Vector2.Lerp(oldAnchorMax,targetAnchorMax,timeTaken/lerpTime);
	}

	public void BeginHighlight(){
		targetAnchorMin=Vector2.zero;
		targetAnchorMax=Vector2.one;
		oldAnchorMin=startAnchorMin;
		oldAnchorMax=startAnchorMax;
		timeTaken=0;
	}

	public override void OnMouseEnter(){
		BeginHighlight();
	}

	public override void OnMouseExit(){
		EndHighlight();
	}

	public void EndHighlight(){
		targetAnchorMin=startAnchorMin;
		targetAnchorMax=startAnchorMax;
		oldAnchorMin=Vector2.zero;
		oldAnchorMax=Vector2.one;
		timeTaken=0;
	}
	
}
