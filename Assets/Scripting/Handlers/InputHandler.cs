using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct InputButton{
	public KeyCode keycode;
	public int mouseButton;
	public bool isKey,isActive;

	public InputButton(KeyCode kc){
		isKey=true;
		isActive=true;
		keycode=kc;
		mouseButton=-1;
	}

	public InputButton(int mb){
		isKey=false;
		isActive=true;
		mouseButton=mb;
		keycode=KeyCode.Space;
	}

	/*public InputButton(){
		isActive=false;
		isKey=false;
		mouseButton=-1;
		keycode=KeyCode.Space;
	}*/
}

[System.Serializable]
public struct InputAxis{
	public InputButton xPButton,xNButton,yPButton,yNButton;
	public InputAxis(InputButton xp,InputButton xn,InputButton yp,InputButton yn){
		xPButton=xp;
		xNButton=xn;
		yPButton=yp;
		yNButton=yn;
	}
}

public static class InputHandler {

	static InputButton[] jump,lookback,fire,reload;
	static InputAxis run;
	static Vector2 mouseSens;

	static bool hasKeybinds=false;

	public static void LoadKeybinds(){
		//Load keybinds (these can be updated midgame, so we can't just load on startup)

		Settings settings=SettingsData.GetSettingsStruct();
		jump=settings.jump;
		lookback=settings.lookback;
		fire=settings.fire;
		reload=settings.reload;
		run=settings.run;

		mouseSens=Vector2.one*SettingsData.GetMouseSens();
		if (settings.mouseInvertY)
			mouseSens.y=-mouseSens.y;
		hasKeybinds=true;
	}

	public static bool GetButton(string name){
		if (!hasKeybinds) LoadKeybinds();
		//select button array with name
		InputButton[] buttons=jump;
		switch (name){
			case "Lookback":
				buttons=lookback;
				break;
			case "Fire":
				buttons=fire;
				break;
			case "Reload":
				buttons=reload;
				break;
		} 
		//foreach button in array
		foreach(InputButton b in buttons){
			//check if Input function returns true, if so, return true
			if (EvalButton(b)) return true;
		}
		
		return false;
	}

	public static bool GetButtonDown(string name){
		if (!hasKeybinds) LoadKeybinds();
		//select button array with name
		InputButton[] buttons=jump;
		switch (name){
			case "Lookback":
				buttons=lookback;
				break;
			case "Fire":
				buttons=fire;
				break;
			case "Reload":
				buttons=reload;
				break;
		} 
		//foreach button in array
		foreach(InputButton b in buttons){
			//check if Input function returns true, if so, return true
			if (EvalButtonDown(b)) return true;
		}
		
		return false;
	}

	public static Vector3 GetMovementAxis(){
		if (!hasKeybinds) LoadKeybinds();
		//take 1st of each axis as positive, 2nd as negative
		Vector3 runVec=Vector3.zero;
		//evaluate x buttons into the vector x
		if (EvalButton(run.xPButton)) runVec.x++;
		if (EvalButton(run.xNButton)) runVec.x--;
		//evaluate y buttons into the vector z
		if (EvalButton(run.yPButton)) runVec.z++;
		if (EvalButton(run.yNButton)) runVec.z--;
		//return vector
		return runVec.normalized;
	}

	public static Vector2 GetMouseDelta(){
		if (!hasKeybinds) LoadKeybinds();
		//use mouse x and y and sensitivity data in settings to return a mouse delta
		Vector2 mouseDelta=new Vector2(Input.GetAxisRaw("Mouse X"),Input.GetAxisRaw("Mouse Y"));
		mouseDelta=Vector2.Scale(mouseDelta,mouseSens);
		return mouseDelta;
	}

	static bool EvalButton(InputButton b){
		if (!b.isActive) return false;
		//true if button down, otherwise false
		if (b.isKey)
			return Input.GetKey(b.keycode);
		return Input.GetMouseButton(b.mouseButton);
	}

	static bool EvalButtonDown(InputButton b){
		if (!b.isActive) return false;
		//true if button just pressed, otherwise false
		if (b.isKey)
			return Input.GetKeyDown(b.keycode);
		return Input.GetMouseButtonDown(b.mouseButton);
	}

}
