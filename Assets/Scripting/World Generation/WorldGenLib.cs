using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public struct Sector{
	public float endAngle;
	public int[] possibleBuildings,possibleSections;
	public bool floatingBuildings;
	public float startHeight,endHeight;
	public float minBuildingRadius,maxBuildingRadius;
	public float minPathWidth,maxPathWidth;
	public int numberOfPaths;
}

[System.Serializable]
public struct Building{
	public float distance,angle,height,width,depth,yRot,radius; //Radius included for compatibility, don't use for actual generation
	public int typeID,materialID,seed;
	public bool inited;
	public int[] inFront;
}

[System.Serializable]
public struct BuildingTemplate{
	public Material material,lowLODMaterial;
	public Material roofMaterial;
	public Mesh mesh,lowLODMesh;
	public bool useBoxCollider;
	public Vector2 sideUVOffsetPerUnit,roofUVOffsetPerUnit;
	public GameObject[] roofObjects;
	public float objectDensity; //objects per square meter
}

public static class WorldGenLib {
	public static Dictionary<Building,Vector4> buildingAABBs;
	
	//Generate all paths that combine to create the branching path
	//Returns a list of path segments [ [startAngle,startDistance],[endAngle,endDistance] ]
	public static List<float[][]> GenerateLines (int startNodeAmount=1,int maxNodes=0,float startAngle=0,float endAngle=360,float angleIncrement=10,float minDistance=0,float maxDistance=1000,float maxDistanceDeviation=0,System.Random randomGen=null) {
		//Init variables
		float angle=startAngle;
		angleIncrement=Mathf.Max(angleIncrement,1f);
		int np=startNodeAmount,n;
		List<float[]> startPoints=new List<float[]>(),endPoints;
		for(int i=0;i<np;i++)
			startPoints.Add(new float[]{angle,SuperMaths.RandomRange(randomGen,minDistance,maxDistance)}); //PLACEHOLDER DISTANCE
		List<float[][]> lines=new List<float[][]>();
		int[][] connections;
		List<int>[] endPointConnectors;

		while(angle<endAngle){
			//Setup variables
			angle+=angleIncrement;
			//n=np+1; //PLACEHOLDER
			n=Mathf.RoundToInt(SuperMaths.RandomRange(randomGen,np,maxNodes));
			if(maxNodes>0)
				n=Mathf.Min(n,maxNodes);
			endPoints=new List<float[]>();
			for(int i=0;i<n;i++)
				endPoints.Add(new float[]{angle,SuperMaths.RandomRange(randomGen,minDistance,maxDistance)}); //PLACEHOLDER DISTANCE

			//Add node connections to lines 
			connections=GenerateConnectedIndices(np,n,startPoints,endPoints,maxDistanceDeviation,randomGen);
			endPointConnectors=new List<int>[endPoints.Count];
			for(int i=0;i<endPoints.Count;i++)
				endPointConnectors[i]=new List<int>();
			for(int i=0;i<connections.Length;i++){
				//Debug.Log(connections[i][0]+","+connections[i][1]);
				endPointConnectors[connections[i][1]].Add(i);
			}
			if (maxDistanceDeviation>0){
				for(int i=0;i<endPoints.Count;i++){
					if (endPointConnectors[i].Count>1){
						float avgDist=startPoints[ connections[ endPointConnectors[i][0] ][0] ][1];
						foreach(int endPointIndex in endPointConnectors[i])
							avgDist=(startPoints[connections[endPointIndex][0]][1]+avgDist)/2;
						endPoints[i][1]=avgDist;
					}else if (endPointConnectors[i].Count>0){
						float currentDist=endPoints[i][1],starterDist=startPoints[connections[endPointConnectors[i][0]][0]][1];
						if (Mathf.Abs(currentDist-starterDist)>maxDistanceDeviation){
							//Debug.Log(currentDist+"-"+starterDist+">"+maxDistanceDeviation);
							currentDist=Mathf.Sign(currentDist-starterDist)*maxDistanceDeviation+starterDist;
						}
						endPoints[i][1]=currentDist;
					}
				}	
			}
			for(int i=0;i<connections.Length;i++){
				lines.Add(new float[][]{startPoints[connections[i][0]],endPoints[connections[i][1]]});
				//LogLine(lines[lines.Count-1]);
			}
			

			//Make sure to only carry over used nodes into the next phase
			if(angle<endAngle){
				startPoints=new List<float[]>();
				for(int i=0;i<endPoints.Count;i++){
					if (endPointConnectors[i].Count>0){
						startPoints.Add(endPoints[i]);
					}
				}
				np=startPoints.Count;
			}
		}

		bool[] isDuplicate=new bool[lines.Count];
		for(int i=0;i<lines.Count;i++){
			if (isDuplicate[i]) continue;
			for(int j=0;j<lines.Count;j++){
				if (j==i) continue;
				if (isDuplicate[j]) continue;
				if (LinesEqual(lines[i],lines[j])) isDuplicate[j]=true;
			}
		}

		List<float[][]> linesNoDuplicates=new List<float[][]>();
		for(int i=0;i<lines.Count;i++)
			if (!isDuplicate[i]) linesNoDuplicates.Add(lines[i]);
		
			//Debug.Log(lines.Count-linesNoDuplicates.Count);
		return linesNoDuplicates;
	}

	

	static bool LinesEqual(float[][] line1,float[][] line2){
		if (line1[0][0]!=line2[0][0]) return false;
		if (line1[0][1]!=line2[0][1]) return false;
		if (line1[1][0]!=line2[1][0]) return false;
		if (line1[1][1]!=line2[1][1]) return false;
		return true;
	}

	//Generate the connections between node sets
	static int[][] GenerateConnectedIndices(int np, int n,List<float[]> startPoints,List<float[]> endPoints,float maxDistanceDeviation,System.Random randomGen,int connectionAmount=-1){ //np=amount of starter nodes n=amount of end nodes
		if (connectionAmount<n)
			connectionAmount=n;
		int[][] connections=new int[connectionAmount][];
		int s,e;
		for	(int i=0;i<connectionAmount;i++){
			s=SuperMaths.RandomRange(randomGen,0,np); //PLACEHOLDER
			if (Mathf.Abs(endPoints[i][1]-startPoints[s][1])>maxDistanceDeviation)
				s=SuperMaths.RandomRange(randomGen,0,np); //PLACEHOLDER
			e=SuperMaths.RandomRange(randomGen,0,n); //For the Random.Range max is exclusive so we go from 0 to n
			connections[i]=new int[]{s,e};
		}
		return connections;
	}

	//Generate a string that represents a line and print
	public static string LogLine(float[][] line,bool print=true){
		string toLog="("+line[0][0]+","+line[0][1]+") to ("+line[1][0]+","+line[1][1]+")";
		if (print) Debug.Log(toLog);
		return toLog;
	}

	public static Vector3 BuildingToPosition(Building building){
		Vector3 pos=Vector3.zero;
		float a=building.distance*Mathf.Sin(Mathf.Deg2Rad*building.angle),b=building.distance*Mathf.Cos(Mathf.Deg2Rad*building.angle);
		pos.x=a;
		pos.z=b;
		return pos;
	}

	public static Vector2 WorldToPolar(Vector3 world){
		Vector2 polar=Vector2.zero;
		//dsin(angle)=world.x
		//dcos(angle)=world.z
		//x/z=tan(angle)
		//if world z is zero d is zero or theta is 90
		if (Mathf.Equals(world.z,0) || world.z==0){
			if(Mathf.Equals(world.x,0) || world.x==0)
				return polar;
			polar.y=90f*Mathf.Sign(world.x);
		}else polar.y=Mathf.Atan(world.x/world.z)*Mathf.Rad2Deg;
		
		polar.y%=360;
		if (polar.y<0) polar.y+=360;
		/*if (world.x*world.z<0) polar.y+=180f;
		if (world.z<0) polar.y+=180f;*/
		if (world.z<0) polar.y+=180f;
		else polar.y+=360f;
		polar.y%=360;
		//polar.y=Mathf.Atan2(world.z,world.x)*Mathf.Rad2Deg+360;
		if (Mathf.Equals(polar.y%180,0)||polar.y%180==0)
			polar.x=Mathf.Abs(world.z);
		else polar.x=Mathf.Abs(world.x/Mathf.Sin(polar.y*Mathf.Deg2Rad));
		return polar;
	}

	public static Vector3 PolarToWorld(float distance,float angle){
		return new Vector3(distance*Mathf.Sin(angle*Mathf.Deg2Rad),0,distance*Mathf.Cos(angle*Mathf.Deg2Rad));
	}

	public static Vector3 PolarToWorld(Vector2 polar){
		return PolarToWorld(polar.x,polar.y);
	}

	public static Vector4 BuildingAABB(Building b){
		if (buildingAABBs==null)
			buildingAABBs=new Dictionary<Building,Vector4>();
		else if (buildingAABBs.ContainsKey(b))
			return buildingAABBs[b];
		//Bounding box is Vec4(x,y,w,h) where x,y is top left
		Vector4 bbox=new Vector4(0,0,0,0);
		Quaternion rotation=Quaternion.AngleAxis(b.yRot,Vector3.forward);
		Vector3 worldPos=BuildingToPosition(b);
		//[topLeft,topRight,bottomRight,bottomLeft]
		Vector2[] corners=new Vector2[4]{
			rotation*new Vector2(-b.width/2,-b.depth/2),
			rotation*new Vector2(+b.width/2,-b.depth/2),
			rotation*new Vector2(+b.width/2,+b.depth/2),
			rotation*new Vector2(-b.width/2,+b.depth/2)
		};
		//Vector2 topLeft=new Vector2(worldPos.x-b.width/2,worldPos.z-b.height/2),bottomRight=new Vector2(worldPos.x+b.width/2,worldPos.z+b.height/2);
		bbox.x=Mathf.Min(corners[0].x,corners[1].x,corners[2].x,corners[3].x);
		bbox.z=Mathf.Max(corners[0].x,corners[1].x,corners[2].x,corners[3].x)-bbox.x;
		bbox.y=Mathf.Min(corners[0].y,corners[1].y,corners[2].y,corners[3].y);
		bbox.w=Mathf.Max(corners[0].y,corners[1].y,corners[2].y,corners[3].y)-bbox.y;
		bbox.x+=worldPos.x;
		bbox.y+=worldPos.z;
		//if (bbox.x<0 || bbox.y<0 || bbox.z<0 || bbox.w<0)
		//	Debug.Log(bbox);
		buildingAABBs[b]=bbox;
		return bbox;
	}
}
