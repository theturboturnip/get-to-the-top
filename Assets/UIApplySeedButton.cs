using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIApplySeedButton : MonoBehaviour {
	public string seedToApply;
	public InputField applyTo;
	// Use this for initialization
	void Start () {
		Button b=GetComponent<Button>();
		b.onClick.AddListener(ApplySeed);
	}
	
	// Update is called once per frame
	void ApplySeed () {
		applyTo.text=seedToApply;
		FinalWorldGen.seedString=seedToApply;	
	}
}
