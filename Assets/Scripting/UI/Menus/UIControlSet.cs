using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIControlSet : MonoBehaviour {
	public string buttonName,niceButtonName;

	Text primaryText,secondaryText;
	Button primaryButton,secondaryButton;

	InputButton[] currentButtons;

	//UIKeyChangeMenu kcm;

	bool changingPrimary;

	// Use this for initialization
	void Start () {
		transform.GetChild(0).gameObject.GetComponent<Text>().text=niceButtonName;

		primaryButton=transform.GetChild(1).gameObject.GetComponent<Button>();
		primaryButton.onClick.AddListener(FocusPrimary);
		primaryText=primaryButton.gameObject.GetComponentInChildren<Text>();

		secondaryButton=transform.GetChild(2).gameObject.GetComponent<Button>();
		secondaryButton.onClick.AddListener(FocusSecondary);
		secondaryText=secondaryButton.gameObject.GetComponentInChildren<Text>();

		//kcm=(UIKeyChangeMenu)Object.FindObjectOfType(typeof(UIKeyChangeMenu));
	}

	void OnEnable(){
		if (primaryButton==null) Start();
		currentButtons=SettingsData.GetInputButton(buttonName);
		primaryText.text=TranslateInputButtonToString(currentButtons[0]);
		secondaryText.text=TranslateInputButtonToString(currentButtons[1]);
	}

	void FocusPrimary(){
		changingPrimary=true;
		UIKeyChangeMenu.currentGO.SetActive(true);
		UIKeyChangeMenu.current.Activate(ReceiveNewButton);
	}

	void FocusSecondary(){
		changingPrimary=false;
		UIKeyChangeMenu.currentGO.SetActive(true);
		UIKeyChangeMenu.current.Activate(ReceiveNewButton);
	}

	public void ReceiveNewButton(bool timedOut,InputButton b){
		if (timedOut) return;
		if (changingPrimary){
			currentButtons[0]=b;
			primaryText.text=TranslateInputButtonToString(b);
		}else{
			currentButtons[1]=b;
			secondaryText.text=TranslateInputButtonToString(b);
		}
	}

	public void Apply(){
		SettingsData.SetInputButton(buttonName,currentButtons);
	}

	public static string TranslateInputButtonToString(InputButton b){
		if (!b.isActive) return "NONE";
		if (b.isKey){
			if (b.keycode==KeyCode.LeftShift)
				return "LSHIFT";
			else if (b.keycode==KeyCode.RightShift)
				return "RSHIFT";
			else if (b.keycode==KeyCode.UpArrow)
				return "UP";
			else if (b.keycode==KeyCode.DownArrow)
				return "DOWN";
			else if (b.keycode==KeyCode.LeftArrow)
				return "LEFT";
			else if (b.keycode==KeyCode.RightArrow)
				return "RIGHT";
			return b.keycode.ToString();
		}else{
			switch (b.mouseButton){
				case 0:
					return "LMB";
				case 1:
					return "RMB";
				case 2:
					return "MMB";
			}
		}
		return "NONE";
	}
}
