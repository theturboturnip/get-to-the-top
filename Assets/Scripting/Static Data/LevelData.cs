using UnityEngine;
using System.Collections;

public class Level{
	public int sceneIndex;
	public string name;
	public float threeStarTime,twoStarTime,oneStarTime;

	public Level(int scene,string n, float threeStar,float twoStar,float oneStar){
		sceneIndex=scene;
		name=n;

		threeStarTime=threeStar;
		twoStarTime=twoStar;
		oneStarTime=oneStar;
	}
}

public static class LevelData{
	static Level[] levels;

	static bool inited=false;

	public static void Initialize(){
		levels=new Level[4];
		levels[0]=new Level(1,"Get Going",30f,60f,120f);
		levels[1]=new Level(2,"Hardcore Parkour",50f,120f,180f);
		levels[2]=new Level(3,"For Every Action...",25f,60f,120f);
		levels[3]=new Level(4,"Get To The Top",60*10f,60*20f,60*30f);
		//levels[2]=new Level(3,"")
		inited=true;
	}

	public static Level GetLevel(int index){
		if (!inited) Initialize();
		if (index>=levels.Length) return null;
		return levels[index];
	}

	public static int GetLevelCount(){
		if (!inited) Initialize();
		return levels.Length;
	}

}