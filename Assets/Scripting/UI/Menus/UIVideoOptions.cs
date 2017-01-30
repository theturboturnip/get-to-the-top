using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIVideoOptions : MonoBehaviour {
	/*FOV (Default 60, max 110)
	VSync (default to off)
	Texture Filtering?
	Reflection Quality(Low=low,Medium=medium,High=High)
	MSAA(Low=off,Medium=2x,High=4x)
	FPS Limit (Low=60,Medium=120,High=Off) (Disabled if VSync=On)
	Shadow Distance (Min=50,Max=500,Low=100,Medium=250,High=500)*/
	float FOV,shadowDist,drawDist;
	int vSyncState,msaa,fpsLimit;
	ShadowResolution shadowResolution;

	public float minFOV,maxFOV,minFPSLimit,maxFPSLimit,minShadowDistance,maxShadowDistance,minDrawDistance,maxDrawDistance;
	public UISlider fovSlider,fpsSlider,shadowDistSlider,drawDistSlider;
	public UIDropdown vSyncDropdown,msaaDropdown,shadowQualityDropdown;
	public Toggle vSyncToggle;

	// Use this for initialization
	void OnEnable () {
		ApplyQualitySettingsToUI();
	}
	
	void ApplyQualitySettingsToUI(){
		//Check if we're in a default quality setting
		FOV=SettingsData.GetFOV();


		int quality=QualitySettings.GetQualityLevel();
		if (quality==0){ //Custom
			fpsLimit=SettingsData.GetFPSLimit();
			msaa=SettingsData.GetMSAA();
			vSyncState=SettingsData.GetVSyncState();
			shadowDist=SettingsData.GetShadowDistance();
			shadowResolution=SettingsData.GetShadowRes();
			drawDist=SettingsData.GetDrawDistance();
		}else{
			if (quality==1){ //Low
				fpsLimit=60;
				drawDist=500;
			}else if (quality==2){ //Medium
				fpsLimit=120;
				drawDist=1000;
			}else if (quality==3){ //High
				fpsLimit=300;
				drawDist=2000;
			}
			shadowDist=QualitySettings.shadowDistance;
			vSyncState=QualitySettings.vSyncCount;
			msaa=QualitySettings.antiAliasing;
			shadowResolution=QualitySettings.shadowResolution;
		}

		
		fovSlider.min=minFOV;
		fovSlider.max=maxFOV;
		fovSlider.SetValue(FOV);

		fpsSlider.min=minFPSLimit;
		fpsSlider.max=maxFPSLimit;
		fpsSlider.SetValue(fpsLimit);
		fpsSlider.SetValueChangedCallback(FPSValueChanged);

		shadowDistSlider.min=minShadowDistance;
		shadowDistSlider.max=maxShadowDistance;
		shadowDistSlider.SetValue(shadowDist);

		drawDistSlider.min=minDrawDistance;
		drawDistSlider.max=maxDrawDistance;
		drawDistSlider.SetValue(drawDist);

		//vSyncDropdown.value=vSyncState;
		vSyncToggle.isOn=(vSyncState>0);
		vSyncToggle.onValueChanged.AddListener(VSyncDropdownChanged);
		
		//reflQualityDropdown.value=reflQuality;
		//reflQualityDropdown.onValueChanged.AddListener(ReflectionDropdownChanged);
		
		msaaDropdown.value=(int)Mathf.Log(msaa,2);
		//msaaDropdown.onValueChanged.AddListener(MSAADropdownChanged);

		shadowQualityDropdown.value=(int)shadowResolution;
		//shadowQualityDropdown.onValueChanged.AddListener(ShadowQualityDropdownChanged);
	}

	public void FPSValueChanged(float newValue){
		vSyncToggle.isOn=false;
	}

	void MSAADropdownChanged(int value){
		msaa=(int)Mathf.Pow(2,value);
	}

	void VSyncDropdownChanged(bool isOn){
		vSyncState=isOn?1:0;
		if(isOn){
			fpsSlider.SetValue(Screen.currentResolution.refreshRate/vSyncState);
		}
		vSyncToggle.isOn=isOn;
	}

	void ShadowQualityDropdownChanged(int value){
		shadowResolution=(ShadowResolution)value;
	}

	public void ApplyVideoSettings(){
		//Find the video settings
		FOV=fovSlider.value;
		fpsLimit=(int)fpsSlider.value;
		shadowDist=shadowDistSlider.value;
		drawDist=drawDistSlider.value;
		msaa=(int)Mathf.Pow(2,msaaDropdown.value);
		shadowResolution=(ShadowResolution)shadowQualityDropdown.value;
		//Save the video settings
		SettingsData.SetFOV(FOV);
		SettingsData.SetFPSLimit(fpsLimit);
		//SettingsData.SetReflQuality(reflQuality);
		SettingsData.SetMSAA(msaa);
		SettingsData.SetVSyncState(vSyncState);
		SettingsData.SetShadowRes(shadowResolution);
		SettingsData.SetShadowDistance(shadowDist);
		SettingsData.SetDrawDistance(drawDist);

		//Apply them to the game
		QualitySettings.SetQualityLevel(0);
		Application.targetFrameRate=fpsLimit;
		QualitySettings.antiAliasing=msaa;
		QualitySettings.shadowResolution=shadowResolution;
		QualitySettings.shadowDistance=shadowDist;
		QualitySettings.vSyncCount=vSyncState;

	}
}
