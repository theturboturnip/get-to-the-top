using UnityEngine;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

[System.Serializable]
public struct Settings{
	//Audio
	public float masterAudio,ambientAudio,sfxAudio;

	//Video
	public float FOV,shadowDist,drawDist;
	public int vSyncState,msaa,fpsLimit;
	public ShadowResolution shadowResolution;

	//Control
	public InputButton[] jump,lookback,fire,reload;
	public InputAxis run;
	public float mouseSens;
	public bool mouseInvertY;
}

public static class SettingsData{

	private static Settings settings;
	private static bool isDirty=false,hasLoaded=false;
   
	public static string GetSaveLocation(){
		return Path.Combine(Application.persistentDataPath , "settings.sv");
	}

	public static void Save(){
		Debug.Log("Saving Settings... ");
		if (!hasLoaded) Load();
		FileInfo fileInfo = new FileInfo(GetSaveLocation());

		if (!fileInfo.Exists){
			Directory.CreateDirectory(fileInfo.Directory.FullName);
		}
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create(GetSaveLocation());
		bf.Serialize(file, SettingsData.settings);
		file.Close();
		isDirty=false;
		Debug.Log("Done!");
	}
	public static void Load(){
		FileInfo fileInfo = new FileInfo(GetSaveLocation());

		if (!fileInfo.Exists){
			Reset();
			return;
		}else{
			FileStream file=File.Open(GetSaveLocation(), FileMode.Open);
			try{
				BinaryFormatter bf = new BinaryFormatter();
				SettingsData.settings = (Settings)bf.Deserialize(file);
				file.Close();
			}catch{
				file.Close();
				Reset();
			}
		}
		hasLoaded=true;
		isDirty=false;

	}
	public static void Reset(){
		SettingsData.settings=new Settings();

		//Reset Audio
		settings.masterAudio=1;
		settings.ambientAudio=1;
		settings.sfxAudio=1;

		//Reset Video
		settings.FOV=60;
		settings.shadowDist=250;
		settings.fpsLimit=120;
		settings.drawDist=500;
		settings.vSyncState=0;
		settings.msaa=1;
		settings.shadowResolution=ShadowResolution.Medium;

		//Reset Controls
		settings.jump=new InputButton[]{new InputButton(KeyCode.Space),new InputButton()};
		settings.lookback=new InputButton[]{new InputButton(KeyCode.LeftShift),new InputButton(1)};
		settings.fire=new InputButton[]{new InputButton(0),new InputButton()};
		settings.reload=new InputButton[]{new InputButton(KeyCode.R), new InputButton()};
		settings.run=new InputAxis(new InputButton(KeyCode.D),new InputButton(KeyCode.A),new InputButton(KeyCode.W),new InputButton(KeyCode.S));
		settings.mouseSens=2;
		settings.mouseInvertY=false;

		hasLoaded=true;
		isDirty=false;
		Save();
	}

	public static float GetMasterAudio(){
		if (!hasLoaded) Load();
		return settings.masterAudio;
	}
	public static void SetMasterAudio(float newValue){
		if (!hasLoaded) Load();
		settings.masterAudio=newValue;
		isDirty=true;
	}

	public static float GetSFXAudio(){
		if (!hasLoaded) Load();
		return settings.sfxAudio;
	}
	public static void SetSFXAudio(float newValue){
		if (!hasLoaded) Load();
		settings.sfxAudio=newValue;
		isDirty=true;
	}

	public static float GetAmbientAudio(){
		if (!hasLoaded) Load();
		return settings.ambientAudio;
	}
	public static void SetAmbientAudio(float newValue){
		if (!hasLoaded) Load();
		settings.ambientAudio=newValue;
		isDirty=true;
	}

	public static Settings GetSettingsStruct(){
		if (!hasLoaded) Load();
		return settings;
	}

	public static InputAxis GetRunAxis(){
		if (!hasLoaded) Load();
		return settings.run;
	}

	public static void SetRunAxis(InputAxis newRunAxis){
		if (!hasLoaded) Load();
		settings.run=newRunAxis;
		isDirty=true;
	}

	public static InputButton[] GetInputButton(string name){
		if (!hasLoaded) Load();
		InputButton[] buttons=settings.jump;
		switch (name){
			case "Lookback":
				buttons=settings.lookback;
				break;
			case "Fire":
				buttons=settings.fire;
				break;
			case "Reload":
				buttons=settings.reload;
				break;
		}
		return buttons;
	}

	public static void SetInputButton(string name,InputButton[] buttons){
		if (!hasLoaded) Load();
		switch (name){
			case "Lookback":
				settings.lookback=buttons;
				break;
			case "Fire":
				settings.fire=buttons;
				break;
			case "Reload":
				settings.reload=buttons;
				break;
			case "Jump":
				settings.jump=buttons;
				break;
		}
		isDirty=true;
	}

	public static float GetFOV(){
		if (!hasLoaded) Load();
		return settings.FOV;
	}

	public static void SetFOV(float newValue){
		settings.FOV=newValue;
		isDirty=true;
	}

	public static float GetShadowDistance(){
		if (!hasLoaded) Load();
		return settings.shadowDist;
	}

	public static void SetShadowDistance(float newValue){
		settings.shadowDist=newValue;
		isDirty=true;
	}

	public static float GetDrawDistance(){
		if (!hasLoaded) Load();
		return settings.drawDist;
	}

	public static void SetDrawDistance(float newValue){
		settings.drawDist=newValue;
		isDirty=true;
	}

	public static int GetFPSLimit(){
		if (!hasLoaded) Load();
		return settings.fpsLimit;
	}

	public static void SetFPSLimit(int newValue){
		settings.fpsLimit=newValue;
		isDirty=true;
	}

	public static int GetVSyncState(){
		if (!hasLoaded) Load();
		return settings.vSyncState;
	}

	public static void SetVSyncState(int newValue){
		settings.vSyncState=newValue;
		isDirty=true;
	}
	
	public static int GetMSAA(){
		if (!hasLoaded) Load();
		return settings.msaa;
	}

	public static void SetMSAA(int newValue){
		settings.msaa=newValue;
		isDirty=true;
	}

	public static ShadowResolution GetShadowRes(){
		if (!hasLoaded) Load();
		return settings.shadowResolution;
	}

	public static void SetShadowRes(ShadowResolution newValue){
		settings.shadowResolution=newValue;
		isDirty=true;
	}

	public static void SetMouseSens(float newValue){
		settings.mouseSens=newValue;
		isDirty=true;
	}

	public static float GetMouseSens(){
		if (!hasLoaded) Load();
		return settings.mouseSens;
	}

	public static void SetInvY(bool newValue){
		settings.mouseInvertY=newValue;
		isDirty=true;
	}

	public static bool GetInvY(){
		if (!hasLoaded) Load();
		return settings.mouseInvertY;
	}

}