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
		int newSeed=0;
		foreach(char c in newValue)
			newSeed+=(int)c;
		Debug.Log(newSeed);
		FinalWorldGen.seed=newSeed;
	}
}
