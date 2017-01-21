using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;

public class BranchingWorldGen : MonoBehaviour {
	public int seed=-1; //-1 for random seed, any other input will be used as a seed
	public bool generateOnStart=true,useAABB=true,threaded=true;
	public int ringCount;
	public float buildProgressAtPathGen=0,buildProgressAtPathFill=0.5f,buildProgressAtFrontFind=0.75f;
	[Space(10)]
	public int pathBreaks=1;
	public float pathStartAngle=0,pathEndAngle=360;
	public int startingNodeAmount=1,maximumNodeCount=0;
	public float randomHeightDeviation;
	public float minimumDistance=0;
	public float cityGridRes;
	public float inFrontDistance;
	[Space(10)]
	public List<Building> buildings;
	public Sector[] sectors;
	public BuildingTemplate[] buildingTemplates;
	List<int>[] rings;
	public int aabbPreventedCollisions=0,collisions;

	public static float buildProgress=0f;
	public Thread generationThread;
	public System.Random threadRandom;

	void PreGenerate(){
		buildProgress=0f;
		SortSectors();
		if (seed<0) seed=UnityEngine.Random.Range(0,2100);
		buildings=new List<Building>();
		threadRandom=new System.Random(seed);
	}

	void Generate(){
		//Pregeneration prep
		if (sectors.Length<=0) return;

		if(ringCount>0){
			rings=new List<int>[(int)(ringCount)];
			for (int r=0;r<rings.Length;r++)
				rings[r]=new List<int>();
		}

		//return;
		//Get paths segments
		float angleIncrement=pathEndAngle-pathStartAngle;
		if(pathBreaks>0) angleIncrement/=pathBreaks;
		//Debug.Log(angleIncrement);
		List<float[][]> pathSegments=WorldGenLib.GenerateLines(
			startNodeAmount:startingNodeAmount,
			startAngle:pathStartAngle,
			endAngle:pathEndAngle,
			maxNodes:maximumNodeCount,
			angleIncrement:angleIncrement,
			minDistance:minimumDistance,
			maxDistance:WorldData.worldRadius,
			maxDistanceDeviation:1000,
			randomGen:threadRandom
		);
		if(pathSegments.Count<=0) return;
		//return;
		buildProgress=0;
		int pathsFilled=0;
		foreach(float[][] pathSegment in pathSegments){
			//WorldGenLib.LogLine(pathSegment);
			FillPath(pathSegment);
			//return;
			pathsFilled++;
			buildProgress=buildProgressAtPathFill+(pathsFilled*1f/pathSegments.Count)*(buildProgressAtFrontFind-buildProgressAtPathFill);
		}
		FindNextBuildings();
		buildProgress=1;
		//progress=1;
		return;
	}

	void FillPath(float[][] pathSegment){
		float /*startDistance=pathSegment[0][1],endDistance=pathSegment[1][1],*/startAngle=pathSegment[0][0],endAngle=pathSegment[1][0];
		//Debug.Log(startAngle+" to "+endAngle);

		//progress along line represented as angle
		float progress=startAngle;
		Building b,p=new Building();
		Sector s;
		int si;
		//int duration=0;
		while (progress<endAngle){
			//Pregeneration prep
			si=SectorFromAngle(progress);
			s=sectors[si];
			b=new Building();
			b.inited=true;

			//Essential assignment
			b.radius=SuperMaths.RandomRange(threadRandom,s.minBuildingRadius,s.maxBuildingRadius);
			float cornerAngle=SuperMaths.RandomRange(threadRandom,45f,45f);
			b.width=SuperMaths.RoundTo(2*b.radius*Mathf.Sin(Mathf.Deg2Rad*cornerAngle),BuildingGenLib.roofGridCellSize);
			b.depth=SuperMaths.RoundTo(2*b.radius*Mathf.Cos(Mathf.Deg2Rad*cornerAngle),BuildingGenLib.roofGridCellSize);
			//float angleOffset=0;//Mathf.Atan(b.width*0.5f/DistanceAtAngle(pathSegment,progress))*Mathf.Rad2Deg;
			b.angle=progress;//NewProgress(progress,b,pathSegment);
			b.distance=DistanceAtAngle(pathSegment,b.angle);
			progress=b.angle;

			//Round world coords of building
			Vector3 worldPos=WorldGenLib.BuildingToPosition(b);
			worldPos.x=SuperMaths.RoundToNearest(worldPos.x,cityGridRes);
			worldPos.z=SuperMaths.RoundToNearest(worldPos.z,cityGridRes);
			Vector2 polarCoords=WorldGenLib.WorldToPolar(worldPos);
			b.angle=polarCoords.y;
			b.distance=polarCoords.x;

			//Extra data
			Vector3 behind=WorldGenLib.PolarToWorld(DistanceAtAngle(pathSegment,progress-0.1f),progress-0.1f),front=WorldGenLib.PolarToWorld(DistanceAtAngle(pathSegment,progress+0.1f),progress+0.1f),gradient=front-behind;
			b.yRot=-Mathf.Atan2(gradient.z,gradient.x)*Mathf.Rad2Deg;
			
			b.typeID=SuperMaths.RandomChoice(threadRandom,s.possibleBuildings);
			b.height=GetBuildingHeight(b.angle);
			b.seed=SuperMaths.RandomRange(threadRandom,0,10000);

			//Add building
			if (!AddPlannedBuilding(b,s))
				collisions++;
			else{
				p=b;
				p.inFront=new int[]{buildings.Count-1};
			}

			//Increment progress
			//float progressOffset=Mathf.Atan(b.width/b.distance)*Mathf.Rad2Deg;
			float newProgress=NewProgress(progress,b,pathSegment);
			//if (newProgress<=0)
			//	progress+=progressOffset;
			//else
				progress=newProgress;
			//duration++;
			//if(duration>500) break;
			//if(progress<0) Debug.Log(progress);
			//if (progress==endAngle) break;
			//buildProgress+=progressOffset/(endAngle-startAngle);
		}
		//Debug.Log(progress);
	}

	float NewProgress(float oldProgress,Building b,float[][] pathSegment,int maxIterations=10){
		//Add 6 to progress until outside of radius then subdivide
		float newProgress=oldProgress,incrementAmount=6,radiusSquared=(b.width/2)*(b.width/2);
		while (SuperMaths.CosineRuleForASquared(DistanceAtAngle(pathSegment,oldProgress),DistanceAtAngle(pathSegment,newProgress),newProgress-oldProgress)<radiusSquared)
			newProgress+=incrementAmount; //0.1 rads
		//The point where distance=radius is between newProgress and newProgress-6 so subdivide
		int iter=0;
		float start=newProgress-incrementAmount,end=newProgress,mid,dist;
		while(iter<maxIterations){
			mid=(start+end)/2;
			dist=SuperMaths.CosineRuleForASquared(DistanceAtAngle(pathSegment,start),DistanceAtAngle(pathSegment,end),end-start);
			if (dist<radiusSquared)
				start=mid;
			else end=mid;
			iter++;
		} 
		return (start+end)/2;
	}

	bool AddPlannedBuilding(Building b,Sector s){
		if (b.distance+b.radius>WorldData.worldRadius || b.distance-b.radius<minimumDistance)
			return false;

		int currentRing=(int)Mathf.Clamp(Mathf.FloorToInt(b.distance*ringCount/WorldData.worldRadius),0,rings.Length-1);
		int ringOffset=Mathf.CeilToInt((2*s.maxBuildingRadius*ringCount)/WorldData.worldRadius);

		for(int r=(int)Mathf.Max(0,currentRing-ringOffset);(r<=currentRing+ringOffset)&&(r<rings.Length);r++){ //Check each ring within maxBuildingRadius

			for (int bi=0;bi<rings[r].Count;bi++){ //Foreach building in ring
				Building current = buildings[rings[r][bi]];

				float squaredMinDistance=(b.radius+current.radius)*(b.radius+current.radius);
				if (Mathf.Abs(b.distance-current.distance)>b.radius+current.radius) continue;

				if (SuperMaths.CosineRuleForASquared(b.distance,current.distance,b.angle-current.angle)<squaredMinDistance){ //More efficient to compare squared as avoids having to sqrt
					if (useAABB){
						Vector4 currentBBox=WorldGenLib.BuildingAABB(current),newBBox=WorldGenLib.BuildingAABB(b);
						if (!SuperMaths.AABB2D(currentBBox,newBBox)){
							aabbPreventedCollisions++;
							continue;
						}
						
					}
						//Make bounding box encapsulate both
						/*Vector4 combinedBBox=currentBBox;
						combinedBBox.x=Mathf.Min(currentBBox.x,newBBox.x);
						combinedBBox.z=Mathf.Max(currentBBox.x,newBBox.x)-combinedBBox.x;
						combinedBBox.y=Mathf.Min(currentBBox.y,newBBox.y);
						combinedBBox.w=Mathf.Max(currentBBox.y,newBBox.y)-combinedBBox.y;
						//Given that the building maintains it's yRot place it in the centre and scale it up
						Vector2 polarCoords=WorldGenLib.WorldToPolar(new Vector3(combinedBBox.x,0,combinedBBox.y));
						current.distance=polarCoords.x;
						current.angle=polarCoords.y;
						Vector2 scalingFactor=new Vector2(combinedBBox.z/currentBBox.z,combinedBBox.w/currentBBox.w);
						current.radius*=(scalingFactor.x+scalingFactor.y)/2;
						current.width*=(scalingFactor.x+scalingFactor.y)/2;
						current.height*=(scalingFactor.x+scalingFactor.y)/2;
						buildings[rings[r][bi]]=current;*/
					//}
					//current.distance=(b.distance+current.distance)/2;
					//current.angle=(b.angle+current.angle)/2;
					//current.height=(current.height+b.height)/2;
					return false;
				}
			}
		}
		buildings.Add(b);
		rings[currentRing].Add(buildings.Count-1);

		return true;
	}

	void FindNextBuildings(){
		//Foreach building
		//	find buildings in radius r
		//  foreach of those
		//    if angle>original.angle
		//      add to originals next array
		int currentRing,ringOffset=Mathf.CeilToInt((2*inFrontDistance*ringCount)/WorldData.worldRadius);
		//Debug.Log(ringOffset);
		float squaredFrontDistance=inFrontDistance*inFrontDistance;
		List<int> inFront;
		Building b;
		int comparisons=0;
		for(int i=0;i<buildings.Count;i++){
			b=buildings[i];
			if (b.inFront!=null) continue;
			currentRing=(int)Mathf.Clamp(Mathf.FloorToInt(b.distance*ringCount/WorldData.worldRadius),ringOffset,rings.Length-1);
			inFront=new List<int>();
			for(int r=currentRing-ringOffset;(r<=currentRing+ringOffset)&&(r<rings.Length);r++){
				foreach(int bi in rings[r]){
					if(bi==i) continue;
					Building toCompare=buildings[bi];
					//Debug.Log("Comparing");
					if (toCompare.angle<b.angle) continue;
					if (SuperMaths.CosineRuleForASquared(b.distance,toCompare.distance,b.angle-toCompare.angle)<=squaredFrontDistance){
						inFront.Add(bi);
					comparisons++;
					}
				}
			}
			b.inFront=inFront.ToArray();
			buildings[i]=b;
			buildProgress+=(1-buildProgressAtFrontFind)/buildings.Count;
		}
		//ebug.Log(comparisons);
	}

	float DistanceAtAngle(float[][] pathSegment, float angle){
		float t=(angle-pathSegment[0][0])/(pathSegment[1][0]-pathSegment[0][0]);
		return Mathf.Lerp(pathSegment[0][1],pathSegment[1][1],t);
	}

	float AngleAtDistance(float[][] pathSegment, float distance){
		float t=(distance-pathSegment[0][1])/(pathSegment[1][1]-pathSegment[0][1]);
		return Mathf.Lerp(pathSegment[0][0],pathSegment[1][0],t);
	}

	float GetBuildingHeight(float angle,int sectorIndex=-1){
		if (sectorIndex<0)
			sectorIndex=SectorFromAngle(angle);
		if (sectorIndex<0) return -1;

		float sectorStart=pathStartAngle,sectorEnd=sectors[sectorIndex].endAngle;
		if (sectorIndex>0) sectorStart=sectors[sectorIndex-1].endAngle;
		float sectorProgress=(angle-sectorStart)/(sectorEnd-sectorStart);
		float unclampedH=Mathf.Lerp(sectors[sectorIndex].startHeight,sectors[sectorIndex].endHeight,sectorProgress)+SuperMaths.RandomRange(threadRandom,-randomHeightDeviation,randomHeightDeviation);
		return Mathf.Clamp(unclampedH,sectors[sectorIndex].startHeight,sectors[sectorIndex].endHeight);
	}

	void SortSectors(){
		float sectorStart=pathStartAngle;
		Sector s;
		for(int i=0;i<sectors.Length;i++){
			s=sectors[i];
			if (s.endAngle<sectorStart) s.endAngle+=sectorStart;
			if (s.endAngle>pathEndAngle) s.endAngle=pathEndAngle;
			sectorStart=s.endAngle;
		}
		sectors[sectors.Length-1].endAngle=pathEndAngle;
	}

	int SectorFromAngle(float angle){
		float sectorStart=pathStartAngle;
		Sector s;
		for(int i=0;i<sectors.Length;i++){
			s=sectors[i];
			if(s.endAngle>=angle && sectorStart<=angle) return i;
			sectorStart=s.endAngle;
		}
		//Debug.Log(angle);
		return -1;
	}

	public void Start(){
		buildProgress=0f;

		if (generateOnStart)
			StartGeneration();
	}

	public void StartGeneration(){
		PreGenerate();
		if (threaded){
			generationThread=new Thread(new ThreadStart(Generate));
			generationThread.IsBackground=true;
			generationThread.Start();
		}else{
			Generate();
		}
	}

	void Update(){
		if (generationThread!=null&&buildProgress>=1&&threaded){
			if (generationThread.IsAlive){
				generationThread.Join();
				generationThread=null;
			}
		}
	}

	void OnApplicationQuit(){
		if (generationThread!=null&&threaded)
			generationThread.Abort();
	}

	void OnDisable(){
		buildProgress=0f;

	}
}
