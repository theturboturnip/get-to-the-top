using UnityEngine.UI;
using UnityEngine;
using System.Collections;

public class ResetDataButton : ButtonCallback {
	public bool resetSettings,resetSave;
	public override void BeenClicked(){
		if (MenuHandler.current.animating) return;
		if (resetSave)
			SaveData.Reset();
		if(resetSettings){
			SettingsData.Reset();
			if (UIOptionsMenu.current!=null)
				UIOptionsMenu.current.Reload();
		}
		Debug.Log("Reset something");
	}
}
