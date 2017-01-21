using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAudioOptions : MonoBehaviour {
	public Slider masterSlider,ambientSlider,sfxSlider;
	public InputField masterInput,ambientInput,sfxInput;

	float master,ambient,sfx;
	// Use this for initialization
	void OnEnable () {
		master=SettingsData.GetMasterAudio();
		masterSlider.value=master;
		masterInput.text=master.ToString("N2");
		masterSlider.onValueChanged.AddListener(MasterSliderChanged);
		masterInput.onValueChanged.AddListener(MasterInputChanged);

		ambient=SettingsData.GetAmbientAudio();
		ambientSlider.value=ambient;
		ambientInput.text=ambient.ToString("N2");
		ambientSlider.onValueChanged.AddListener(AmbientSliderChanged);
		ambientInput.onValueChanged.AddListener(AmbientInputChanged);

		sfx=SettingsData.GetSFXAudio();
		sfxSlider.value=sfx;		
		sfxInput.text=sfx.ToString("N2");
		sfxSlider.onValueChanged.AddListener(SFXSliderChanged);
		sfxInput.onValueChanged.AddListener(SFXInputChanged);
	}

	void MasterSliderChanged(float value){
		master=value;
		masterInput.text=value.ToString("N2");
	}

	void MasterInputChanged(string text){
		float newMaster=1;
		try{
			newMaster=Mathf.Clamp01(float.Parse(text));
		}catch(Exception e){
			return;
		}
		master=newMaster;
		masterSlider.value=master;
	}

	void AmbientSliderChanged(float value){
		ambient=value;
		ambientInput.text=value.ToString("N2");
	}

	void AmbientInputChanged(string text){
		float newAmbient=1;
		try{
			newAmbient=Mathf.Clamp01(float.Parse(text));
		}catch(Exception e){
			return;
		}
		ambient=newAmbient;
		ambientSlider.value=ambient;
	}

	void SFXSliderChanged(float value){
		sfx=value;
		sfxInput.text=value.ToString("N2");
	}

	void SFXInputChanged(string text){
		float newSFX=1;
		try{
			newSFX=Mathf.Clamp01(float.Parse(text));
		}catch(Exception e){
			return;
		}
		sfx=newSFX;
		sfxSlider.value=sfx;
	}
	
	// Update is called once per frame
	public void ApplyAudioSettings () {
		SettingsData.SetMasterAudio(master);
		SettingsData.SetAmbientAudio(ambient);
		SettingsData.SetSFXAudio(sfx);
	}
}
