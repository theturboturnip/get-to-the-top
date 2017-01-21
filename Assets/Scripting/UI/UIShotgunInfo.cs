using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIShotgunInfo : MonoBehaviour {
	Component[] images;
	bool activated=false;
	Shotgun s;

	// Use this for initialization
	void Start () {
		//Disable children
		images=(gameObject.GetComponentsInChildren(typeof(Image)));
		foreach(Image i in images){
			i.enabled=false;
		}
		s=LevelHandler.shotgun;
	}
	
	// Update is called once per frame
	void Update () {
		//if shotgun exists, enable all children 
		if (activated) return;
		//s=(Shotgun)Object.FindObjectOfType(typeof(Shotgun));
		if (!activated &&s.gameObject.active){
			activated=true;
			foreach(Image i in images){
				i.enabled=true;
			}
			Component[] shellHandlers=gameObject.GetComponentsInChildren(typeof(UIShellHandler));
			foreach(UIShellHandler sh in shellHandlers)
				sh.shotgun=s;
		}
	}
}
