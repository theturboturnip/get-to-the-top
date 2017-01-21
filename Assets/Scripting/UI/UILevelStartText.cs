using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILevelStartText : MonoBehaviour {
	Text nameText;
	float nameShowStart;
	float fadeTime=0.5f;
	// Use this for initialization
	void Start () {
		nameText=GetComponentInChildren<Text>();
		nameText.text=LevelHandler.currentLevel.levelName;
		nameShowStart=Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		float timeFromStart=Time.time-nameShowStart,timeFromEnd=NuPlayer.startLevelTime-timeFromStart,p=1;
		if (timeFromEnd<0){
			this.enabled=false;
			PauseMenuHandler.current.ChangeMenu(PauseMenuHandler.current.hudObject);
		}
		if (timeFromStart<fadeTime)
			p=timeFromStart/fadeTime;
		else if (timeFromEnd<fadeTime)
			p=timeFromEnd/fadeTime;
		nameText.color=new Color(p,p,p,p);
	}
}
