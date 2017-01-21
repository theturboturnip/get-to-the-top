using UnityEngine;
using System.Collections;
using System;

public class MenuHandler : MonoBehaviour {
	public GameObject startMenu;
	GameObject currentMenu,previousMenu;

	public Transform toOpen,toClose;
	float animateStart=-1;
	public float animateTime,xStretch=1.1f;
	public bool animating=false;

	public GameObject[] notMenus;

	public static MenuHandler current;

	// Use this for initialization
	public virtual void Start () {
		current=this;
		ChangeMenu(startMenu);
	}
	
	// Update is called once per frame
	public virtual void Update () {
		float p;
		if (toOpen!=null && animateStart>-1 && toClose==null){
			toOpen.gameObject.SetActive(true);
			p=Mathf.Clamp01((Time.unscaledTime-animateStart)/animateTime);

			//Fancay anims
			Vector3 newScale=Vector3.up*p;
			if (p<0.5){
				newScale.x=Mathf.SmoothStep(0,xStretch,p*2);
				newScale.z=Mathf.SmoothStep(0,0.1f,p*2);
			}else{
				newScale.x=Mathf.SmoothStep(xStretch,1,p*2-1);
				newScale.z=Mathf.SmoothStep(0.1f,1,p*2-1);
			}

			toOpen.localScale=newScale;

			if (p>=1){
				currentMenu=toOpen.gameObject;
				animateStart=-1;
				toOpen=null;
			}
		}

		if (toClose!=null && animateStart>-1){
			p=Mathf.Clamp01((Time.unscaledTime-animateStart)/animateTime);

			//Fancay anims
			Vector3 newScale=Vector3.up*(1-p);
			if (p<0.5){
				newScale.x=Mathf.SmoothStep(1,xStretch,p*2);
				newScale.z=Mathf.SmoothStep(1,0.1f,p*2);
			}else{
				newScale.x=Mathf.SmoothStep(xStretch,0,p*2-1);
				newScale.z=Mathf.SmoothStep(0.1f,0,p*2-1);
			}

			toClose.localScale=newScale;

			if (p>=1){
				toClose.gameObject.SetActive(false);
				if (toOpen!=null)
					animateStart=Time.unscaledTime;
				toClose=null;
			}
		}

		animating=(animateStart>-1);
	}

	public void ChangeMenu(GameObject newMenu){
		if (newMenu==null){
			if (previousMenu==null) return;
			newMenu=previousMenu;
		}
		foreach (Transform t in transform){
			if (t.gameObject==currentMenu) continue;
			if (Array.IndexOf(notMenus,t.gameObject)!=-1) continue;
			t.gameObject.SetActive(false);
		}

		toOpen=newMenu.transform;
		if (currentMenu!=null)
			toClose=currentMenu.transform;
		animateStart=Time.unscaledTime;
		animating=true;
		previousMenu=currentMenu;
	}
}
