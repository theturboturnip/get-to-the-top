using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIOptionsMenu : MonoBehaviour {
	public RectTransform scrollParent;
	public UIScroller uiScrollbar;
	RectTransform video,audioMenu,control,rt;
	//UIControlSet[] controlSets;
	public static UIOptionsMenu current;

	// Use this for initialization
	void Start () {
		foreach(Transform t in scrollParent){
			rt=((RectTransform)t);
			rt.pivot=Vector2.right*0.5f+Vector2.up;
			Rect rect=rt.rect;
		 	rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,OrganiseChildren(rt));
		}
		scrollParent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,OrganiseChildren(scrollParent));
		video=(RectTransform)scrollParent.GetChild(0);
		audioMenu=(RectTransform)scrollParent.GetChild(1);
		control=(RectTransform)scrollParent.GetChild(2);
		uiScrollbar.maxY=scrollParent.rect.height-((RectTransform)scrollParent.parent).rect.height;
		//OrganiseChildren(control);
		//controlSets=scrollParent.gameObject.GetComponentsInChildren<UIControlSet>();
		current=this;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	float OrganiseChildren(RectTransform r){
		float height=0;
		if (r.parent==scrollParent)
			height-=r.rect.height;
		RectTransform childR;
		foreach(Transform t in r){
			childR=(RectTransform)t;
			childR.pivot=Vector2.right*0.5f+Vector2.up;
			childR.localPosition=childR.localPosition.x*Vector2.right+Vector2.up*height;
			height-=childR.rect.height;
		}
		return -height;
	}

	public void ApplySettings(){
		//Send values to SettingsData, tell it to save
		video.gameObject.GetComponent<UIVideoOptions>().ApplyVideoSettings();
		audioMenu.gameObject.GetComponent<UIAudioOptions>().ApplyAudioSettings();
		/*UIControlSet[] controlSets=(UIControlSet[])Object.FindObjectsOfType(typeof(UIControlSet));
		foreach(UIControlSet c in controlSets)
			c.Apply();
		((UIControlAxis)Object.FindObjectOfType(typeof(UIControlAxis))).Apply();*/
		control.gameObject.GetComponent<UIControlOptions>().ApplyControlSettings();

		SettingsData.Save();
		if (SettingsApplier.current!=null)
			SettingsApplier.current.UpdateSettings();

	}

	public void Reload(){
		video.gameObject.SetActive(false);
		audioMenu.gameObject.SetActive(false);
		control.gameObject.SetActive(false);
		video.gameObject.SetActive(true);
		audioMenu.gameObject.SetActive(true);
		control.gameObject.SetActive(true);
	}


}
