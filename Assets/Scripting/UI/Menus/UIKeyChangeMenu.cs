using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIKeyChangeMenu : MonoBehaviour {
	public int life=16;
	float startTime=-1;
	Text t;
	public static UIKeyChangeMenu current;
	public static GameObject currentGO;
	public delegate void KeyDownCallback(bool timedOut,InputButton b);
	KeyDownCallback currentCallback;
	// Use this for initialization

	void Awake(){
		current=this;
		t=GetComponentInChildren<Text>();
		currentGO=this.gameObject;
		currentGO.SetActive(false);
	}

	public void Activate(KeyDownCallback requestedCallback){
		if (startTime>=0) return;
		startTime=Time.unscaledTime;
		currentCallback=requestedCallback;
		PauseMenuHandler.canTogglePause=false;
	}

	void Deactivate(){
		startTime=-1;
		gameObject.SetActive(false);
		PauseMenuHandler.canTogglePause=true;
	}

	void Update(){
		if (startTime==-1) return;

		int secondsLeft=life-Mathf.FloorToInt(Time.unscaledTime-startTime);
		t.text="Press a key, or hit escape to set to none.\n\n"+secondsLeft;

		if (secondsLeft<0){
			Deactivate();
			currentCallback(true,new InputButton());
		}

		int pressedMouseButton=-1;
		if (Input.GetMouseButtonDown(0)) pressedMouseButton=0;
		if (Input.GetMouseButtonDown(1)) pressedMouseButton=1;
		if (Input.GetMouseButtonDown(2)) pressedMouseButton=2;

		if (pressedMouseButton!=-1){
			Deactivate();
			currentCallback(false,new InputButton(pressedMouseButton));
			return;
		}

		if (!Input.anyKeyDown) return;
		foreach(KeyCode vKey in System.Enum.GetValues(typeof(KeyCode))){
			if(Input.GetKeyDown(vKey)){
				if (vKey==KeyCode.Escape)
					currentCallback(false, new InputButton());
				else
					currentCallback(false,new InputButton(vKey));
				Deactivate();
			}
		}
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		
	}
}
