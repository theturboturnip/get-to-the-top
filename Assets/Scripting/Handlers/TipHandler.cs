using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipHandler : MonoBehaviour {
	public static TipHandler current;

	public float animTime;

	Sprite tipSprite;
	public string tipMessage="This is a tip.\nThis is a second tip";
	bool tipOpen=false;
	float tipAnimStart=-1;
	float currentTipLife,currentTipStartTime;
	int currentTipImportance=-1;

	RectTransform rt;
	public Image tipSpriteHolder;
	public Text tipMessageHolder;
	public Vector2 openScale=	new Vector2(200,150);
	public Vector2 closedScale=	new Vector2(0,0);
	//public Sprite debugSprite;

	void OnEnable(){
		current=this;
	}

	// Use this for initialization
	void Start () {
		rt=(RectTransform)transform;
		transform.localScale=Vector3.zero;//=closedScale;
		openScale=Vector2.one;
		//tipSpriteHolder.enabled=false;
		//tipMessageHolder.enabled=false;
		//OpenTip(debugSprite,"DEBUGDEBUGDEBUG");
	}
	
	// Update is called once per frame
	void Update () {
		if (tipAnimStart!=-1){
			//We're opening the UI
			float p=Mathf.Clamp01((Time.time-tipAnimStart)/animTime);
			//From 0 to 0.5 go from 0 to x*1.1,0 to 0.1z
			//From 0.5 to 1 go to x,z
			//y is straight lerp

			Vector3 newScale=new Vector3(0,0,1);
			if (tipOpen){
				if (p<0.5){
					newScale.x=Mathf.SmoothStep(closedScale.x,openScale.x*1.5f,p*2);
					newScale.y=Mathf.SmoothStep(closedScale.y,openScale.y*0.1f,p*2);
				}else{
					newScale.x=Mathf.SmoothStep(openScale.x*1.5f,openScale.x,p*2-1);
					newScale.y=Mathf.SmoothStep(openScale.y*0.1f,openScale.y,p*2-1);
				}
			}else{
				if (p<0.5){
					newScale.x=Mathf.SmoothStep(openScale.x,openScale.x*1.5f,p*2);
					newScale.y=Mathf.SmoothStep(openScale.y,openScale.y*0.1f,p*2);
				}else{
					newScale.x=Mathf.SmoothStep(openScale.x*1.5f,closedScale.x,p*2-1);
					newScale.y=Mathf.SmoothStep(openScale.y*0.1f,closedScale.y,p*2-1);
				}
			}

			transform.localScale=newScale;
			//rt.sizeDelta=newScale;
			if (p==1){
				tipAnimStart=-1;
				currentTipStartTime=Time.time;
				tipSpriteHolder.enabled=tipOpen;
				tipMessageHolder.enabled=tipOpen;
			}
		}
		if (Time.time-currentTipStartTime>=currentTipLife&&tipOpen&&currentTipLife>0&&tipAnimStart==-1)
			CloseTip(tipMessage);
	}

	public void OpenTip(Sprite ts,string tm,int importance=0,float tipLifeTime=0){
		if (currentTipImportance>importance && tipOpen) return;
		tipSprite=ts;
		//tipSpriteHolder.sprite=tipSprite;
		//tipSpriteHolder.preserveAspect=true;
		tipMessage=tm;
		tipMessageHolder.text=tipMessage;
		tipAnimStart=Time.time;
		currentTipLife=tipLifeTime;
		currentTipImportance=importance;
		currentTipStartTime=Time.time;
		tipOpen=true;
	}

	public void CloseTip(string tm){
		if (tm!=tipMessage) return;
		tipAnimStart=Time.time;
		tipOpen=false;
		//currentTipImportance=-1;
	}
}
