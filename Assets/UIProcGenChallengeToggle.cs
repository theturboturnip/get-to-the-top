using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIProcGenChallengeToggle : MonoBehaviour {
	public bool toggleNightMode,toggleAutoFire;
	// Use this for initialization
	void OnEnable () {
		GetComponent<Toggle>().isOn=false;

		GetComponent<Toggle>().onValueChanged.AddListener(DoToggle);
	}
	
	// Update is called once per frame
	void DoToggle (bool isOn) {
		if (toggleNightMode) FinalWorldGen.nightMode=isOn;
		if (toggleAutoFire) FinalWorldGen.autoFire=isOn;
	}
}
