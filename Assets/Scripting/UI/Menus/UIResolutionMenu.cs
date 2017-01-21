using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UIResolutionMenu : MonoBehaviour {
	public int newResWaitTime;
	public GameObject settingsMenu,resolutionMenu,cooldownMenu;

	public Dropdown resolutionDropdown;
	Toggle fullscreen;
	Text confirmText;

	Resolution oldResolution;
	bool oldfullscreen;

	float setResolutionTime=-1;

	void OnEnable(){
		resolutionDropdown=resolutionMenu.GetComponentInChildren<Dropdown>();
		resolutionDropdown.ClearOptions();

		List<string> opts=new List<string>();
		//#if UNITY_EDITOR
		//opts.Add("1920x1080");
		//#else
		int i=0;
		int currentResolutionIndex=0;
		Debug.Log(Screen.currentResolution.ToString());
		foreach(Resolution r in Screen.resolutions){
			opts.Add(r.ToString());
			if (ResolutionsEqual(r,Screen.currentResolution)&&Screen.fullScreen)
				currentResolutionIndex=i;
			i++;
		}
		oldfullscreen=Screen.fullScreen;
		oldResolution=Screen.resolutions[currentResolutionIndex];
		//#endif
		resolutionDropdown.AddOptions(opts);
		resolutionDropdown.value=currentResolutionIndex;
		fullscreen=resolutionMenu.GetComponentInChildren<Toggle>();

		confirmText=cooldownMenu.GetComponentInChildren<Text>();
		cooldownMenu.SetActive(false);
		resolutionMenu.SetActive(true);
	}

	bool ResolutionsEqual(Resolution r1, Resolution r2){
		if (r1.width!=r2.width) return false;
		if (r1.height!=r2.height) return false;
		if (r1.refreshRate!=r2.refreshRate) return false;
		return true;
	}

	void Update(){
		if (setResolutionTime<0) return;
		int secondsLeft=newResWaitTime-Mathf.RoundToInt(Time.time-setResolutionTime);
		confirmText.text="Confirm new resolution? switching back in "+secondsLeft+"...";
		if (secondsLeft<0)
			ApplyPreviousSettings();
	}

	public void StopResolutionCooldown(){
		setResolutionTime=-1;
		//resolutionMenu.SetActive(true);
		//cooldownMenu.SetActive(false);
		MenuHandler.current.ChangeMenu(settingsMenu);
	}

	public void ApplyCurrentSettings(){
		Debug.Log(resolutionDropdown.value);
		Resolution r=Screen.resolutions[resolutionDropdown.value];
		Screen.SetResolution(r.width,r.height,fullscreen.isOn,r.refreshRate);
		setResolutionTime=Time.time;
		resolutionMenu.SetActive(false);
		cooldownMenu.SetActive(true);
	}

	public void ApplyPreviousSettings(){
		Screen.SetResolution(oldResolution.width,oldResolution.height,oldfullscreen,oldResolution.refreshRate);
		setResolutionTime=-1;
		resolutionMenu.SetActive(true);
		cooldownMenu.SetActive(false);
	}
}
