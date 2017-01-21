using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIControlAxis : MonoBehaviour {
	public Button xP,xN,yP,yN;
	Text xPText,xNText,yNText,yPText;
	InputAxis run;
	Button currentFocus;

	void OnEnable(){
		if (xPText==null){
			xPText=xP.gameObject.GetComponentInChildren<Text>();
			yPText=yP.gameObject.GetComponentInChildren<Text>();
			xNText=xN.gameObject.GetComponentInChildren<Text>();
			yNText=yN.gameObject.GetComponentInChildren<Text>();
		}
		run=SettingsData.GetRunAxis();
		xPText.text=UIControlSet.TranslateInputButtonToString(run.xPButton);
		xNText.text=UIControlSet.TranslateInputButtonToString(run.xNButton);
		yPText.text=UIControlSet.TranslateInputButtonToString(run.yPButton);
		yNText.text=UIControlSet.TranslateInputButtonToString(run.yNButton);

		xP.onClick.AddListener(FocusXP);
		xN.onClick.AddListener(FocusXN);
		yP.onClick.AddListener(FocusYP);
		yN.onClick.AddListener(FocusYN);
	}

	void FocusXP(){
		currentFocus=xP;
		UIKeyChangeMenu.currentGO.SetActive(true);
		UIKeyChangeMenu.current.Activate(ReceiveNewButton);
	}

	void FocusYP(){
		currentFocus=yP;
		UIKeyChangeMenu.currentGO.SetActive(true);
		UIKeyChangeMenu.current.Activate(ReceiveNewButton);
	}

	void FocusYN(){
		currentFocus=yN;
		UIKeyChangeMenu.currentGO.SetActive(true);
		UIKeyChangeMenu.current.Activate(ReceiveNewButton);
	}

	void FocusXN(){
		currentFocus=xN;
		UIKeyChangeMenu.currentGO.SetActive(true);
		UIKeyChangeMenu.current.Activate(ReceiveNewButton);
	}

	public void ReceiveNewButton(bool timedOut,InputButton b){
		if (timedOut) return;
		if(currentFocus==xP)
			run.xPButton=b;
		else if (currentFocus==xN)
			run.xNButton=b;
		else if(currentFocus==yP)
			run.yPButton=b;
		else if (currentFocus==yN)
			run.yNButton=b;
		xPText.text=UIControlSet.TranslateInputButtonToString(run.xPButton);
		xNText.text=UIControlSet.TranslateInputButtonToString(run.xNButton);
		yPText.text=UIControlSet.TranslateInputButtonToString(run.yPButton);
		yNText.text=UIControlSet.TranslateInputButtonToString(run.yNButton);
	}

	public void Apply(){
		SettingsData.SetRunAxis(run);
	}
}
