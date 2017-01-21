using UnityEngine;
#if UNITY_EDITOR
using System;
#endif
using System.Collections;
using System.Collections.Generic;


class PathStartComparer : IComparer<float[]>{
	public int Compare(float[] x,float[] y){
		if(x[0]>y[0]) return 1;
		if(x[0]<y[0]) return -1;
		return 0;
	}
}

class PathEndComparer : IComparer<float[]>{
	public int Compare(float[] x,float[] y){
		if(x[1]>y[1]) return 1;
		if(x[1]<y[1]) return -1;
		return 0;
	}
}

public class WorldGenv2 : MonoBehaviour {
	public int seed;
	public float startAngle,buildingHeightVar;
	public Sector[] sectors;
//	public float startDifficulty,endDifficulty;	
	public BuildingTemplate[] buildingTemplates;
	public float minDistance;
	public GameObject basicBuilding;
	public int ringCount=10;
	public bool createDynamicSkyboxCopy,buildBuildings=false,verbose;
	public GameObject island;
	PathSectionGenerator[] generators;
	public List<Building> buildings;
	List<int>[] rings;
	float worldRadius;

	void Start(){
		#if !UNITY_EDITOR
		verbose=false;
		#endif
		NuGenerate();
	}

	void Generate(){
		worldRadius=WorldData.worldRadius;
		if(island!=null)
			island.GetComponent<CircleMeshGen>().radius=worldRadius;
		generators=gameObject.GetComponents<PathSectionGenerator>();
		float bGenStartAngle=startAngle;
		List<float[]> paths;
		buildings=new List<Building>();
		if(ringCount>0){
			rings=new List<int>[(int)(ringCount)];
			for (int r=0;r<rings.Length;r++)
				rings[r]=new List<int>();
		}
		foreach (Sector s in sectors){
			paths=GeneratePaths(s);
			GenerateBuildings(s,paths,bGenStartAngle);
			bGenStartAngle=s.endAngle;
		}
		if(buildBuildings)
			BuildBuildings();
	}

	void NuGenerate(){
		worldRadius=WorldData.worldRadius;
		//if(island!=null)
		//	island.GetComponent<CircleMeshGen>().radius=worldRadius;
		UnityEngine.Random.seed=seed;
		
		generators=gameObject.GetComponents<PathSectionGenerator>();
		float bGenStartAngle=startAngle;
		List<float[][]> paths;

		buildings=new List<Building>();
		if(ringCount>0){
			rings=new List<int>[(int)(ringCount)];
			for (int r=0;r<rings.Length;r++)
				rings[r]=new List<int>();
		}

		foreach(Sector s in sectors){
			paths=WorldGenLib.GenerateLines(startAngle:bGenStartAngle,endAngle:s.endAngle,angleIncrement:40f,minDistance:minDistance,maxDistance:worldRadius);
			foreach(float[][] p in paths)
				GenerateBuildingsForPath(s,p,bGenStartAngle);
			bGenStartAngle=s.endAngle;
		}

		if(buildBuildings)
			BuildBuildings();
	}

	List<float[]> GeneratePaths(Sector s){
		#if UNITY_EDITOR
		Timer t=new Timer(false);
		if(verbose) t.Start();
		#endif
		UnityEngine.Random.seed=seed;
		List<float[]> paths=new List<float[]>();
		float start,end;
		while (paths.Count<s.numberOfPaths || s.numberOfPaths<=0){
			start=AltGenerateStart(s.minPathWidth,s.maxPathWidth,paths);
			end=GenerateEnd(s.minPathWidth,s.maxPathWidth,paths);
			if (start==-1 || end==-1){
				//Impossible to create more paths
				#if UNITY_EDITOR
				Debug.Log("Generated "+paths.Count+" paths for sector "+Array.IndexOf(sectors,s)+" in "+t.Stop()+" ms.");
				#endif
				break;
			}
			paths.Add(new float[]{start,end});
		}
		return paths;
	}

	float GenerateStart(float minimumPathWidth,float maximumPathWidth,List<float[]> paths){
		float prospectiveStart=minDistance;
		if (paths.Count>0) prospectiveStart=paths[paths.Count-1][0];
		if (prospectiveStart<worldRadius)
			return Mathf.Min(prospectiveStart+RandomRange(minimumPathWidth,maximumPathWidth)); 
		return -1; //No possible start points
	}

	float AltGenerateStart(float minimumPathWidth,float maximumPathWidth,List<float[]> paths){
		if (paths.Count==0)
			return RandomRange(minDistance,worldRadius);
		List<int> availableRangeIndices=new List<int>();
		paths.Sort(new PathStartComparer());
		List<float[]> sortedPaths=(paths);
		if (sortedPaths[sortedPaths.Count-1][1]<worldRadius)
			sortedPaths.Add(new float[]{0,worldRadius});
		if (sortedPaths[0][1]>0)
			sortedPaths.Insert(0,new float[]{0,0});
		for(int i=0;i<sortedPaths.Count-1;i++){
			if (Mathf.Abs(sortedPaths[i+1][1]-sortedPaths[i][1])>minimumPathWidth+maximumPathWidth)
				availableRangeIndices.Add(i);
		}
		if (availableRangeIndices.Count==0) return -1;
		int rangeIndex=RandomChoice(availableRangeIndices);
		return RandomRange(sortedPaths[rangeIndex][1],sortedPaths[rangeIndex+1][1]);
	}

	float GenerateEnd(float minimumPathWidth,float maximumPathWidth,List<float[]> paths){
		if (paths.Count==0)
			return RandomRange(minDistance,worldRadius);
		List<int> availableRangeIndices=new List<int>();
		List<float[]> sortedPaths=new List<float[]>(paths);
		sortedPaths.Sort(new PathEndComparer());
		if (sortedPaths[sortedPaths.Count-1][1]<worldRadius)
			sortedPaths.Add(new float[]{0,worldRadius});
		if (sortedPaths[0][1]>0)
			sortedPaths.Insert(0,new float[]{0,0});
		for(int i=0;i<sortedPaths.Count-1;i++){
			if (Mathf.Abs(sortedPaths[i+1][1]-sortedPaths[i][1])>minimumPathWidth+maximumPathWidth)
				availableRangeIndices.Add(i);
		}
		if (availableRangeIndices.Count==0) return -1;
		int rangeIndex=RandomChoice(availableRangeIndices);
		return RandomRange(sortedPaths[rangeIndex][1],sortedPaths[rangeIndex+1][1]);
	}

	void GenerateBuildings(Sector s, List<float[]> paths,float startAngle){
		#if UNITY_EDITOR
		Timer timer=new Timer(false);
		if (verbose)
			timer.Start();
		#endif
		int extraBuildings=0;
		foreach (float[] path in paths){
			float start=path[0],end=path[1];
			float endAngle=s.endAngle;
			float progress=startAngle;
			Building previous=new Building();
			while(progress<endAngle){
				//Gen building pos
				float t=(progress-startAngle)/(endAngle-startAngle);
				float currentDistance=Mathf.Lerp(start,end,t);
				float buildingRadius=RandomRange(s.minBuildingRadius,s.maxBuildingRadius);
				Building toPlace=new Building();
				toPlace.angle=progress;
				toPlace.radius=buildingRadius;
				toPlace.distance=currentDistance;
				toPlace.typeID=RandomChoice(s.possibleBuildings);
				RandomChoice(generators).Apply(ref toPlace,this,path,t,previous);
				//At the beginning (t=0) height=startHeight
				//At the end (t=1) if distance/worldRadius > 0.5 height=end
				//else as distance/worldRadius goes from 0.5 to 0 height goes from end to start
				//t=1, r=0 h=0
				//t=1, r=1 h=2
				//t=1, r=0.5 h=1
				//h=r*2*t
				float h=t+(toPlace.distance-minDistance)/(worldRadius-minDistance);
				toPlace.height=Mathf.Lerp(s.startHeight,s.endHeight,h*t)+RandomRange(-10f,20f);
				float cornerAngle=RandomRange(45f,55f);
				toPlace.width=2*toPlace.radius*Mathf.Sin(Mathf.Deg2Rad*cornerAngle);
				toPlace.depth=2*toPlace.radius*Mathf.Cos(Mathf.Deg2Rad*cornerAngle);
				toPlace.yRot=progress;
				progress=toPlace.angle+Mathf.Atan(toPlace.radius/currentDistance)*Mathf.Rad2Deg;
				if (AddPlannedBuilding(toPlace,s))
					extraBuildings++;
				previous=toPlace;
			}
		}
		#if UNITY_EDITOR
		if(verbose)
			Debug.Log("Generated "+extraBuildings+" buildings for sector "+Array.IndexOf(sectors,s)+" in "+ timer.Stop()+" ms.");
		#endif
	}

	void GenerateBuildingsForPath(Sector s, float[][] path,float sectorStartAngle){
		int extraBuildings=0;
			float start=path[0][1],end=path[1][1];
			float startAngle=path[0][0],endAngle=path[1][0];
			//float endAngle=s.endAngle;
			float progress=startAngle;
			Building previous=new Building();
			while(progress<endAngle){
				//Gen building pos
				float t=(progress-startAngle)/(endAngle-startAngle),ht=(progress-sectorStartAngle)/(s.endAngle-sectorStartAngle);
				float currentDistance=Mathf.Lerp(start,end,t);
				float buildingRadius=RandomRange(s.minBuildingRadius,s.maxBuildingRadius);
				Building toPlace=new Building();
				toPlace.angle=progress;
				toPlace.radius=buildingRadius;
				toPlace.distance=currentDistance;
				toPlace.typeID=RandomChoice(s.possibleBuildings);
				RandomChoice(generators).Apply(ref toPlace,this,new float[]{start,end},t,previous);
				//At the beginning (t=0) height=startHeight
				//At the end (t=1) if distance/worldRadius > 0.5 height=end
				//else as distance/worldRadius goes from 0.5 to 0 height goes from end to start
				//t=1, r=0 h=0
				//t=1, r=1 h=2
				//t=1, r=0.5 h=1
				//h=r*2*t
				float h=ht+(toPlace.distance-minDistance)/(worldRadius-minDistance);
				toPlace.height=Mathf.Lerp(s.startHeight,s.endHeight,h*ht)+RandomRange(-buildingHeightVar,buildingHeightVar);
				float cornerAngle=RandomRange(45f,55f);
				toPlace.width=2*toPlace.radius*Mathf.Sin(Mathf.Deg2Rad*cornerAngle);
				toPlace.depth=2*toPlace.radius*Mathf.Cos(Mathf.Deg2Rad*cornerAngle);
				toPlace.yRot=progress;
				progress=toPlace.angle+Mathf.Atan(toPlace.radius/currentDistance)*Mathf.Rad2Deg/4;
				if (AddPlannedBuilding(toPlace,s))
					extraBuildings++;
				previous=toPlace;
			}
	}

	bool AddPlannedBuilding(Building toBuild,Sector s){
		if (toBuild.distance+toBuild.radius>worldRadius || toBuild.distance-toBuild.radius<minDistance)
			return false;
		int currentRing=(int)Mathf.Clamp(Mathf.FloorToInt(toBuild.distance*ringCount/worldRadius),0,rings.Length-1);
		int ringOffset=Mathf.CeilToInt((2*s.maxBuildingRadius*ringCount)/worldRadius);
		for(int r=(int)Mathf.Max(0,currentRing-ringOffset);(r<=currentRing+ringOffset)&&(r<rings.Length);r++){
			for (int b=0;b<rings[r].Count;b++){
				Building building = buildings[rings[r][b]];
				float squaredMinDistance=(toBuild.radius+building.radius)*(toBuild.radius+building.radius);
				if (Mathf.Abs(toBuild.distance-building.distance)>toBuild.radius+building.radius) continue;
				//float rDTheta=(Mathf.Min(toBuild.distance,building.distance)*(toBuild.angle-building.angle)*Mathf.Deg2Rad);
				//if (rDTheta*rDTheta>squaredMinDistance) continue;
				if (SuperMaths.CosineRuleForASquared(toBuild.distance,building.distance,toBuild.angle-building.angle)<squaredMinDistance){ //More efficient to compare squared as avoids having to sqrt
					building.distance=(toBuild.distance+building.distance)/2;
					building.angle=(toBuild.angle+building.angle)/2;
					building.height=(building.height+toBuild.height)/2;
					return false;
				}
			}
		}
		buildings.Add(toBuild);
		rings[currentRing].Add(buildings.Count-1);

		return true;
	}

	void OldBuildBuildings(){

		foreach(Building building in buildings){
			Vector3 pos=WorldGenLib.BuildingToPosition(building);
			Vector3 scale=new Vector3(building.width,building.height*2,building.depth);//*2;
			Transform b=(Instantiate(basicBuilding,pos,Quaternion.Euler(Vector3.up*building.yRot)) as GameObject).transform;
			b.localScale=scale;
			b.parent=transform;
		}
	}
	void BuildBuildings(){
		#if UNITY_EDITOR
		Timer t=new Timer(verbose);
		#endif

		Transform dsBuildingParent=transform;
		DynamicSkybox ds=DynamicSkybox.currentSkybox;
		if(createDynamicSkyboxCopy){
			dsBuildingParent=new GameObject("Dynamic Skybox Buildings").transform;
			dsBuildingParent.parent=DynamicSkybox.currentSkybox.transform;
		}
		foreach(Building building in buildings){
			UnityEngine.Random.seed=building.seed;
			Vector3 pos=WorldGenLib.BuildingToPosition(building);
			Vector3 scale=new Vector3(building.width,building.height,building.depth);

			BuildingTemplate bt=buildingTemplates[building.typeID];
			GameObject building_g=new GameObject("Building "+transform.childCount);
			building_g.transform.parent=transform;
			building_g.transform.position=pos;
			building_g.transform.rotation=Quaternion.Euler(Vector3.up*building.yRot);
			building_g.AddComponent<MeshFilter>();
			building_g.AddComponent<MeshRenderer>().materials=new Material[]{bt.material,bt.roofMaterial};
			//building_g.AddComponent<CameraCuller>();
			//if (createDynamicSkyboxCopy)
			//	building_g.AddComponent<DynamicSkyboxDuplicator>();
			
			BuildingGeneratorv2 buildingGen=building_g.AddComponent<BuildingGeneratorv2>();
			buildingGen.bounds=scale;
			buildingGen.buildingShape=bt.mesh;
			buildingGen.generateUVs=true;
			buildingGen.useBoxCollider=bt.useBoxCollider;
			buildingGen.sideUVOffsetPerUnit=bt.sideUVOffsetPerUnit;
			buildingGen.roofUVOffsetPerUnit=bt.roofUVOffsetPerUnit;

			if(createDynamicSkyboxCopy){
				GameObject dsBuildingCopy=new GameObject("Building "+dsBuildingParent.childCount);
				//dsBuildingCopy.AddComponent<CameraCuller>();
				dsBuildingCopy.AddComponent<MeshFilter>().mesh=bt.lowLODMesh;
				dsBuildingCopy.AddComponent<MeshRenderer>().material=bt.lowLODMaterial;
				dsBuildingCopy.transform.localScale=ds.playingAreaScaleToSkybox.MultiplyVector(scale);
				dsBuildingCopy.transform.position=ds.playingAreaToSkybox.MultiplyPoint3x4(pos)+Vector3.up*dsBuildingCopy.transform.localScale.y/2;
				dsBuildingCopy.transform.rotation=building_g.transform.rotation;
				dsBuildingCopy.transform.parent=dsBuildingParent;
				dsBuildingCopy.layer=LayerMask.NameToLayer(ds.skyboxLayer);
			}

			//Populate building roof with roof objects
			/*int objectCount=Mathf.FloorToInt(bt.objectDensity*building.width*building.depth);
			Vector3 objectPos=Vector3.zero;
			GameObject toSpawn;
			float spawnAngle=0;*/
			/*for(int i=0;i<2;i++){
				toSpawn=RandomChoice(bt.roofObjects);
				objectPos.x=RandomRange(0f,building.width)-building.width/2;
				objectPos.y=building.height+toSpawn.transform.localScale.y/2;
				objectPos.z=RandomRange(0f,building.depth)-building.depth/2;
				spawnAngle=RandomRange(0,360);
				Transform object_t=(Instantiate(toSpawn,objectPos+building_g.transform.position,Quaternion.Euler(Vector3.up*spawnAngle)) as GameObject).transform;//.parent=building_g.transform;
				//object_t.parent=building_g.transform;
			}*/
		}
		#if UNITY_EDITOR
		if(verbose)
			Debug.Log("Built "+buildings.Count+" buildings in "+t.Stop()+" ms.");
		#endif
	}

	//				    //
	// HELPER FUNCTIONS //
	//				    //

	Vector2 BuildingToTexcoord(Building building){
		Vector2 texCoord=Vector2.one*0.5f;
		float a=building.distance*Mathf.Sin(Mathf.Deg2Rad*building.angle),b=building.distance*Mathf.Cos(Mathf.Deg2Rad*building.angle);
		texCoord.x+=a/(2*worldRadius);
		texCoord.y+=b/(2*worldRadius);
		return texCoord;
	}

	public float BuildingDistance(Building one,Building two){
		return SuperMaths.CosineRule(one.distance,two.distance,(one.angle-two.angle),true);
	}

	Color SampleTextureAtBuilding(Texture2D texture,Building building){
		Vector2 texPoint=BuildingToTexcoord(building);
		return texture.GetPixelBilinear(texPoint.x,texPoint.y);
	}

	bool ApplyRanges(float value, float[] ranges){
		if (ranges.Length==0) return true;
		for (int i=0;i<ranges.Length;i+=2){
			if (value<ranges[i+1]&&value>ranges[i]) 
				return true;
		}
		return false;
	}

	public float RandomRange(float start,float end){
		return Mathf.Lerp(start,end,UnityEngine.Random.value);
	}

	public int RandomRange(int start, int end){
		return UnityEngine.Random.Range(start,end);
	}

	public T RandomChoice<T>(T[] pickFrom){
		return pickFrom[UnityEngine.Random.Range(0,pickFrom.Length)];
	}

	public T RandomChoice<T>(List<T> pickFrom){
		return pickFrom[UnityEngine.Random.Range(0,pickFrom.Count)];
	}

	public Building InitializeBuilding(Building toInit){
		/*public float distance,angle,height,width,depth,yRot;
		public int typeID,materialID,roofSeed;*/
		toInit.distance=-1;
		toInit.angle=-1;
		toInit.height=-1;
		toInit.width=-1;
		toInit.depth=-1;
		toInit.yRot=0;
		toInit.typeID=RandomRange(0,buildingTemplates.Length);
		toInit.materialID=0;
		toInit.seed=RandomRange(0,1001);
		toInit.inited=true;
		return toInit;
	}
}
