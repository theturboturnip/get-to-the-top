using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIProcGenSeedEditor : MonoBehaviour {
	InputField input;
	// Use this for initialization
	void Start () {
		input=GetComponent<InputField>();
		input.onEndEdit.AddListener(ChangeSeed);
	}
	
	// Update is called once per frame
	void ChangeSeed (string newValue) {
		FinalWorldGen.seedString=newValue;
	}
}
