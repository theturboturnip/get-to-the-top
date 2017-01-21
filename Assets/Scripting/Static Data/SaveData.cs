using UnityEngine;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public struct LevelProgress{
	public float timeTaken;
	public int starRanking;
	public bool unlocked;
}

[System.Serializable]
public struct ProcGenProgress{
	public LevelProgress prog;
	public int seed;
}

[System.Serializable]
public struct GameProgress{
	public LevelProgress[] levelSaveData;
	public List<ProcGenProgress> procGenSaveData;
}

public static class SaveData{

	private static GameProgress progress;
	private static bool isDirty=false,hasLoaded=false;

	public static string GetSaveLocation(){
		return Path.Combine(Application.persistentDataPath , "savedata.sv");
	}

	public static void Save(){
		Debug.Log("Saving... ");
		if (!hasLoaded) Load();
		FileInfo fileInfo = new FileInfo(GetSaveLocation());

		if (!fileInfo.Exists){
			Directory.CreateDirectory(fileInfo.Directory.FullName);
		}
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create(GetSaveLocation());
		bf.Serialize(file, SaveData.progress);
		file.Close();
		isDirty=false;
		Debug.Log("Done!");
	}
	public static void Load(){
		//if(!File.Exists(GetSaveLocation()))
		//	Reset();
		FileInfo fileInfo = new FileInfo(GetSaveLocation());

		if (!fileInfo.Exists){
			//Directory.CreateDirectory(fileInfo.Directory.FullName);
			Reset();
			return;
		}else{
			FileStream file=File.Open(GetSaveLocation(), FileMode.Open);
			try{
				BinaryFormatter bf = new BinaryFormatter();
				SaveData.progress = (GameProgress)bf.Deserialize(file);
				file.Close();
			}catch{
				file.Close();
				Reset();
			}
		}
		if (progress.levelSaveData.Length!=LevelData.GetLevelCount())
			Reset();
		hasLoaded=true;
		isDirty=false;

	}
	public static void Reset(){
		int levelCount=LevelData.GetLevelCount();
		progress.levelSaveData=new LevelProgress[levelCount];
		for (int i=0;i<levelCount;i++)
			progress.levelSaveData[i]=new LevelProgress();
		progress.procGenSaveData=new List<ProcGenProgress>();
		hasLoaded=true;
		isDirty=false;
		LevelProgress firstLevel=progress.levelSaveData[0];
		firstLevel.unlocked=true;
		SetLevelProgress(0,firstLevel);
		Save();
	}

	public static LevelProgress GetLevelProgress(int index){
		if (!hasLoaded) Load();
		return progress.levelSaveData[index];
	}
	public static void SetLevelProgress(int index,LevelProgress lp){
		if (!hasLoaded) Load();
		progress.levelSaveData[index]=lp;
		isDirty=true;
	}

	public static void AddProcGenProgress(int seed,LevelProgress lp){
		if (!hasLoaded) Load();
		ProcGenProgress p= new ProcGenProgress();
		p.seed=seed;
		int saveDataIndex=-1;
		for(int i=0;i<progress.procGenSaveData.Count;i++){
			if (progress.procGenSaveData[i].seed==seed){
				p=progress.procGenSaveData[i];
				saveDataIndex=i;
				break;
			}
		}
		p.prog=lp;
		if (saveDataIndex>-1)
			progress.procGenSaveData[saveDataIndex]=p;
		else
			progress.procGenSaveData.Add(p);
		isDirty=true;
	}

	public static LevelProgress GetProcGenProgress(int seed){
		if (!hasLoaded) Load();
		foreach(ProcGenProgress p in progress.procGenSaveData){
			if (p.seed==seed)
				return p.prog;
		}
		ProcGenProgress toReturn=new ProcGenProgress();
		toReturn.seed=seed;
		toReturn.prog=new LevelProgress();
		progress.procGenSaveData.Add(toReturn);
		return toReturn.prog;
	}

}