using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using System;

#endif

#if UNITY_EDITOR
public class WorldGenerator : MonoBehaviour {
	public int seed;
	public bool createDynamicSkyboxCopy=false;
	[Header("Path Generation")]
	float worldRadius;
	public float minDistance;
	public float sectorStart,sectorEnd;
	public float minimumPathWidth,maximumPathWidth;
	public int numberOfPaths=0;
	[Header("Building Placement")]
	public float endDifficulty;
	public float minimumBuildingRadius;
	public float maximumBuildingRadius;
	public float ringRadiusRatio=0.1f;
	public float roundPositionsTo=10;
	public float roundDistancesTo=10;
	public float[] distanceRanges;
	public float[] angleRanges;
	public Texture2D buildingMask;
	PathSectionGenerator[] sectionGenerators;

	[Header("Building Creation")]
	public BuildingTemplate[] buildingTemplates;
	//public Mesh[] buildingShapes;
	//public Material[] buildingMaterials;
	public float startHeight,endHeight;
	public float heightJitter;
	public float maxHeightDeviation;
	//	GameObject basicBuilding;

	[Header("Debug")]

	public bool generatePaths=true;
	public bool generateBuildings=true;
	public bool buildBuildings=true;
	public bool displayGenerateTime=false;

	List<float[]> paths= new List<float[]>();
	List<Building> buildingsToBuild=new List<Building>();
	List<Building>[] rings;
	PathEndComparer pathComp;
	//Debug
	DateTime generateStart,generateEnd;

	void Start(){
		worldRadius=WorldData.worldRadius;
		UnityEngine.Random.seed=seed;
		createDynamicSkyboxCopy=createDynamicSkyboxCopy&&(DynamicSkybox.currentSkybox!=null);
		//transform.localScale=new Vector3(worldRadius*2,1f,worldRadius*2);
		GenerateWorld();
	}

	void GenerateWorld(){
		if (displayGenerateTime)
			generateStart=DateTime.UtcNow;
		if (!generatePaths) return;
		Debug.Log("Generating Paths");
		GeneratePaths();
		if (displayGenerateTime){
			Debug.Log("Time to generate paths: "+(DateTime.UtcNow-generateStart).TotalMilliseconds+"ms.");
			generateStart=DateTime.UtcNow;
		}
		if(!generateBuildings) return;
		Debug.Log("Generating Buildings");
		GenerateBuildingPositions();
		if (displayGenerateTime){
			Debug.Log("Time to generate buildings: "+(DateTime.UtcNow-generateStart).TotalMilliseconds+"ms.");
			generateStart=DateTime.UtcNow;
		}
		if(!buildBuildings) return;
		Debug.Log("Building Buildings");
		BuildBuildings();
		if (displayGenerateTime){
			Debug.Log("Time to build buildings: "+(DateTime.UtcNow-generateStart).TotalMilliseconds+"ms.");
			generateStart=DateTime.UtcNow;
		}
	}

	//				   //
	// PATH GENERATION //
	//                 //

	void GeneratePaths(){
		float start,end;
		while (paths.Count<numberOfPaths || numberOfPaths<=0){
			start=GenerateStart();
			end=GenerateEnd();
			if (start==-1 || end==-1){
				//Impossible to create more paths
				Debug.Log("Path Generation Complete!");
				break;
			}
			paths.Add(new float[]{start,end});
		}
	}

	float GenerateStart(){
		float prospectiveStart=minDistance;
		if (paths.Count>0) prospectiveStart=paths[paths.Count-1][0];
		if (prospectiveStart<worldRadius)
			return Mathf.Min(prospectiveStart+RandomRange(minimumPathWidth,maximumPathWidth)); 
		return -1; //No possible start points
	}

	float GenerateEnd(){
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
			if (Mathf.Abs(sortedPaths[i+1][1]-sortedPaths[i][1])>2*minimumPathWidth)
				availableRangeIndices.Add(i);
		}
		if (availableRangeIndices.Count==0) return -1;
		int rangeIndex=RandomChoice(availableRangeIndices);
		return RandomRange(sortedPaths[rangeIndex][1],sortedPaths[rangeIndex+1][1]);
	}

	//                              //
	// BUILDING POSITION GENERATION //
	//                              //

	void GenerateBuildingPositions(){
		if(ringRadiusRatio!=0){
			rings=new List<Building>[(int)(1/ringRadiusRatio)];
			for (int r=0;r<rings.Length;r++)
				rings[r]=new List<Building>();
		}
		sectionGenerators=GetComponents<PathSectionGenerator>();
		FixPathSectionWeights();
		float progress,pathLength,currentRadius,radius,height,newHeight,cornerAngle,difficulty,targetDifficulty;
		//DateTime pathStart;
		generateStart=DateTime.UtcNow;
		PathSectionGenerator pathGen;
		//Building oldBuilding=new Building();
		foreach(float[] path in paths){
			progress=0;
			pathLength=Mathf.PI*(path[0]+path[1])*(sectorEnd-sectorStart)/360f;
			height=startHeight;
			difficulty=0;
			//pathStart=DateTime.UtcNow;
			//int buildingCount=0;
			while (progress<pathLength){
				currentRadius=Mathf.Lerp(path[0],path[1],progress/pathLength);
				Building toPlace=new Building();
				toPlace=InitializeBuilding(toPlace);
				toPlace.distance=currentRadius;
				toPlace.angle=Mathf.Lerp(sectorStart,sectorEnd,progress/pathLength);
				targetDifficulty=endDifficulty*progress/pathLength;//toPlace.angle/(360*toPlace.distance/worldRadius);
				if (progress==0)
					pathGen=sectionGenerators[0];
				else 
					pathGen=RandomSection(targetDifficulty,difficulty);
				//toPlace.yRot=toPlace.angle;
					//toPlace=pathGen.Apply(toPlace,this,path,pathLength,progress,oldBuilding);
				toPlace.distance=RoundToNearest(toPlace.distance,roundDistancesTo);
				radius=UnityEngine.Random.Range(minimumBuildingRadius,maximumBuildingRadius);
				cornerAngle=RandomRange(25f,65f);
			//	Debug.Log(cornerAngle);
				toPlace.width=2*radius*Mathf.Sin(Mathf.Deg2Rad*cornerAngle);
				toPlace.depth=2*radius*Mathf.Cos(Mathf.Deg2Rad*cornerAngle);
				newHeight=Mathf.Lerp(startHeight,endHeight,progress/pathLength)+toPlace.height+RandomRange(-heightJitter,heightJitter);
				if (maxHeightDeviation>0){
					height+=Mathf.Clamp(newHeight-height,-maxHeightDeviation,maxHeightDeviation);
				}else height=newHeight;
				toPlace.height=height;

				//COMMENT THIS LINE OUT!
				toPlace.radius=radius;
				//COMMENT THIS LINE OUT!
				
				AddProspectiveBuilding(toPlace);
				progress=Mathf.Min(progress+radius,pathLength);
				if(progress>0)
					difficulty+=pathGen.difficulty;//*BuildingDistance(oldBuilding,toPlace);
				//oldBuilding=toPlace;
			}
		}
	}

	float FixAngle(float toFix){
		toFix%=360;
		while (toFix<0) toFix+=360;
		return toFix;
	}

	PathSectionGenerator RandomSection(float targetDifficulty=0,float currentDifficulty=0){
		if(targetDifficulty==0){
			//Assume weights add to 1
			float randomVal=UnityEngine.Random.value;
			float totalWeight=0;
			foreach(PathSectionGenerator psg in sectionGenerators){
				totalWeight+=psg.weight;
				if (randomVal<=totalWeight) return psg;
			}
		}else{
			int closestIndex=0;
			float d,targetDelta=targetDifficulty-currentDifficulty;
			if (numberOfPaths==1)
				Debug.Log(targetDelta+","+currentDifficulty+","+targetDifficulty);
			for(int i=0;i<sectionGenerators.Length;i++){
				d=sectionGenerators[i].difficulty;
				if (Mathf.Abs(d-targetDelta)<Mathf.Abs(sectionGenerators[closestIndex].difficulty-targetDelta))
					closestIndex=i;
			}
			return sectionGenerators[closestIndex];
		}
		return null;
	}

	void FixPathSectionWeights(){
		float totalWeight=0;
		foreach(PathSectionGenerator psg in sectionGenerators)
			totalWeight+=psg.weight;
		foreach(PathSectionGenerator psg in sectionGenerators)
			psg.weight/=totalWeight;
	}

	Vector3 BuildingToPosition(Building building){
		Vector3 pos=Vector3.zero;
		float a=building.distance*Mathf.Sin(Mathf.Deg2Rad*building.angle),b=building.distance*Mathf.Cos(Mathf.Deg2Rad*building.angle);
		pos.x=RoundToNearest(a,roundPositionsTo);
		pos.z=RoundToNearest(b,roundPositionsTo);

		return pos;
	}

	Vector2 BuildingToTexcoord(Building building){
		Vector2 texCoord=Vector2.one*0.5f;
		float a=building.distance*Mathf.Sin(Mathf.Deg2Rad*building.angle),b=building.distance*Mathf.Cos(Mathf.Deg2Rad*building.angle);
		texCoord.x+=a/(2*worldRadius);
		texCoord.y+=b/(2*worldRadius);
		return texCoord;
	}

	bool AddProspectiveBuilding(Building toBuild){
		//A building is {distance,angle,radius,height}
		if (!ApplyRanges(toBuild.distance,distanceRanges))
			return false;
		if (!ApplyRanges(toBuild.angle,angleRanges))
			return false;
		if(buildingMask!=null){
			if (SampleTextureAtBuilding(buildingMask,toBuild).grayscale<0.5f)
				return false;
		}
		if (toBuild.distance+toBuild.radius>worldRadius || toBuild.distance-toBuild.radius<minDistance)
			return false;
		//Compare with buildings in rings within 2 max radius
		//ring width=ringRadiusRatio*worldRadius
		//minimum ring=currentRing-(2*maxRadius)/ringWidth
		int currentRing=(int)Mathf.Clamp(Mathf.FloorToInt(toBuild.distance/(ringRadiusRatio*worldRadius)),0,rings.Length-1);
		int ringOffset=Mathf.CeilToInt((2*maximumBuildingRadius)/(ringRadiusRatio*worldRadius));
		for(int r=(int)Mathf.Max(0,currentRing-ringOffset);(r<=currentRing+ringOffset)&&(r<rings.Length);r++){
			for (int b=0;b<rings[r].Count;b++){
				Building building = rings[r][b];
				float squaredMinDistance=(toBuild.radius+building.radius)*(toBuild.radius+building.radius);
				if (Mathf.Abs(toBuild.distance-building.distance)>toBuild.radius+building.radius) continue;
				//float rDTheta=(Mathf.Min(toBuild.distance,building.distance)*(toBuild.angle-building.angle)*Mathf.Deg2Rad);
				//if (rDTheta*rDTheta>squaredMinDistance) continue;
				if (CosineRuleForASquared(toBuild.distance,building.distance,toBuild.angle-building.angle)<squaredMinDistance){ //More efficient to compare squared as avoids having to sqrt
					building.distance=(toBuild.distance+building.distance)/2;
					building.angle=(toBuild.angle+building.angle)/2;
					building.height=(building.height+toBuild.height)/2;
					return false;
				}
			}
		}
		buildingsToBuild.Add(toBuild);
		rings[currentRing].Add(toBuild);
		return true;
	}

	//                  //
	// BUILDING BUILDER //
	//                  //

	/*void OldBuildBuildings(){
		foreach(Building building in buildingsToBuild){
			Vector3 pos=BuildingToPosition(building);
			Vector3 scale=new Vector3(building.width,building.height,building.depth);//*2;
			Transform b=(Instantiate(basicBuilding,pos,Quaternion.identity) as GameObject).transform;
			b.localScale=scale;
			b.parent=transform;
		}
	}*/

	void BuildBuildings(){
		Transform dsBuildingParent=transform;
		DynamicSkybox ds=DynamicSkybox.currentSkybox;
		if(createDynamicSkyboxCopy){
			dsBuildingParent=new GameObject("Dynamic Skybox Buildings").transform;
			dsBuildingParent.parent=DynamicSkybox.currentSkybox.transform;
		}
		foreach(Building building in buildingsToBuild){
			UnityEngine.Random.seed=building.seed;
			Vector3 pos=BuildingToPosition(building);
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
	}

	//				    //
	// HELPER FUNCTIONS //
	//				    //

	public float BuildingDistance(Building one,Building two){
		return CosineRule(one.distance,two.distance,(one.angle-two.angle),true);
	}

	Color SampleTextureAtBuilding(Texture2D texture,Building building){
		Vector2 texPoint=BuildingToTexcoord(building);
		return texture.GetPixelBilinear(texPoint.x,texPoint.y);
	}

	public float RoundToNearest(float target,float roundTo){
		if (roundTo==0) return target;
		return Mathf.Floor(target/roundTo)*roundTo;
	}

	bool ApplyRanges(float value, float[] ranges){
		if (ranges.Length==0) return true;
		for (int i=0;i<ranges.Length;i+=2){
			if (value<ranges[i+1]&&value>ranges[i]) 
				return true;
		}
		return false;
	}

	public float CosineRule(float b,float c,float theta,bool degrees=true){
		return Mathf.Sqrt(CosineRuleForASquared(b,c,theta,degrees));
	}

	public float CosineRuleForASquared(float b,float c,float theta,bool degrees=true){
		theta=degrees?(theta*Mathf.Deg2Rad):theta;
		return (b*b+c*c-2*b*c*Mathf.Cos(theta));
	}

	public float ReverseCosineRule(float a,float b,float c,bool degrees=true){
		return (degrees?Mathf.Rad2Deg:1)*Mathf.Acos((a*a+b*b-c*c)/(2*a*b));
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
#endif
