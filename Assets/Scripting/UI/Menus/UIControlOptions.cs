using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIControlOptions : MonoBehaviour {
	public float minSens,maxSens;
	UIControlSet[] controlSets;
	UIControlAxis runAxis;
	UISlider sensitivitySlider;
	Toggle invertToggle;

	// Use this for initialization
	void OnEnable () {
		controlSets=GetComponentsInChildren<UIControlSet>();
		runAxis=GetComponentInChildren<UIControlAxis>();
		sensitivitySlider=GetComponentInChildren<UISlider>();
		sensitivitySlider.min=minSens;
		sensitivitySlider.max=maxSens;
		sensitivitySlider.SetValue(SettingsData.GetMouseSens());
		invertToggle=GetComponentInChildren<Toggle>();
		invertToggle.isOn=SettingsData.GetInvY();
	}
	
	// Update is called once per frame
	public void ApplyControlSettings() {
		foreach(UIControlSet c in controlSets)
			c.Apply();
		runAxis.Apply();
		SettingsData.SetMouseSens(sensitivitySlider.value);
		SettingsData.SetInvY(invertToggle.isOn);
	}
}
