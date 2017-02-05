using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsApplier : MonoBehaviour {
	public static SettingsApplier current;
	public bool highRefl=false;

	// Use this for initialization
	void Start () {
		current=this;
		UpdateSettings();
	}

	void Update(){
		//if highRefl, raycast from camera, place reflprobe and render
	}
	
	// Update is called once per frame
	public void UpdateSettings () {
		Application.targetFrameRate=SettingsData.GetFPSLimit();
		QualitySettings.antiAliasing=SettingsData.GetMSAA();
		QualitySettings.shadowResolution=SettingsData.GetShadowRes();
		QualitySettings.shadowDistance=SettingsData.GetShadowDistance();
		QualitySettings.vSyncCount=SettingsData.GetVSyncState();

		AudioListener.volume=SettingsData.GetMasterAudio();
		foreach(Camera c in Camera.allCameras)
			c.fieldOfView=SettingsData.GetFOV();
		Camera.main.farClipPlane=SettingsData.GetDrawDistance();
		InputHandler.LoadKeybinds();
	}
}
