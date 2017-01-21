using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct RoofHeightZone{
	public int x,y,w,d;
	public float height;
}

public struct Roof{
	public Building target;
	public RoofTileID[,] tileTypes;
	public RoofHeightZone[] heightZones;
	public bool isValid;
	public int gridWidth,gridDepth;

	public Roof(Building b,RoofTileID[,] tileTypes_,RoofHeightZone[] heightZones_){
		target=b;
		tileTypes=tileTypes_;
		heightZones=heightZones_;
		gridWidth=0;
		gridDepth=0;
		isValid=true;
	}
}

public enum RoofTileID{
	None=0,
	Start=1,
	Box=2,
	SolarPanel=3
}

public static class BuildingGenLib{
	public static float roofGridCellSize=5;

	public static Roof RoofFromBuilding(Building b){
		int oldSeed=Random.seed;
		Random.seed=b.seed;

		//Generate roof
		//Roof r=new Roof();
		int gridWidth=Mathf.FloorToInt(b.width/roofGridCellSize),gridDepth=Mathf.FloorToInt(b.depth/roofGridCellSize);
		RoofTileID[,] tileTypes=new RoofTileID[gridWidth,gridDepth];
		for(int i=0;i<gridWidth;i++){
			for(int j=0;j<gridDepth;j++){
				tileTypes[i,j]=RoofTileID.None;
				float randVal=Random.value;
				if (randVal>0.95f)
					tileTypes[i,j]=RoofTileID.Box;
				else if (randVal>0.9f)
					tileTypes[i,j]=RoofTileID.SolarPanel;
			}
		}
		tileTypes[Random.Range(0,gridWidth),Random.Range(0,gridDepth)]=RoofTileID.Box;
		tileTypes[0,0]=RoofTileID.Start;

		RoofHeightZone[] heightZones=new RoofHeightZone[0];

		Random.seed=oldSeed;
		Roof r= new Roof(b,tileTypes,heightZones);
		r.gridDepth=gridDepth;
		r.gridWidth=gridWidth;
		return r;
	}
}