using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIVersionControl : MonoBehaviour {
	int isValid=0;
	Text t;
	public string currentVersion;
	public GameObject updateButton;
	string internalText="";
	bool versionBehind;

	// Use this for initialization
	void Start(){
		t=GetComponentInChildren<Text>();
		t.text="Current Version: "+currentVersion;
		//Get valid
		GTTTNetwork.RequestValidate(Random.value,ReceiveValid);
		//Get version
		GTTTNetwork.GetCurrentVersion(ReceiveVersion);
		
	}

	void Update(){
		if (internalText!=""){
			t.text=internalText;
			internalText="";
		}
		if (versionBehind){
			updateButton.SetActive(true);
			versionBehind=false;
		}
	}

	// Update is called once per frame
	void ReceiveValid(string response){
		if (response[0]=='y') isValid=1;
		else{
			isValid=-1;
			internalText="SERVER AUTH FAILED";
			versionBehind=true;
		}
	}

	void ReceiveVersion(string response){
		if (isValid<0) return;
		internalText="Current Version: "+currentVersion+"\nLatest version: "+response.Trim();
		if (currentVersion.Trim()!=response.Trim()){
			versionBehind=true;
		}else
			internalText+="\nUp To Date";

	}
}
