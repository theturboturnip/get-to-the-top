using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct CellData{
	public Vector3 center;
	public List<int> buildings;
}

struct BuildingData{
	public Vector3 pos;
	public bool isSpawned,hasSpawned,shouldBeSpawned; //If it's currently spawned, don't spawn it again. If it hasn't spawned, don't try to access the mesh data
	public Mesh cachedMesh;
	public int cellX,cellY;
	public Bounds bounds;
	public Transform t;
}

public class RealtimeBuildingGeneratorv2 : MonoBehaviour {

	public int cellsPerSide=128; //x squared cells
	public int constantMeshBuildingCount=500; //allow x buildings in this mesh
	public Mesh buildingBaseMesh;
	public Material roofMat,windowMat;
	[ReadOnly] public int activeBuildings=0,cellsSampled=0,activeVertexCount=0,spawnListLen,despawnListLen;
	[ReadOnly] public float maxCellDist;
	public float cellSize,cellRadius;
	public bool useCullingGroupAPI=false;
	//public CullingDistanceBehaviour cullingDistBehaviour;

	bool inited=false;
	public GameObject buildingPlaceholder;
	Building b;
	FinalWorldGen worldGen;
	Transform buildingParent;
	int i,j;
	bool canSpawn,canDespawn;
	Mesh buildingMesh;
	bool[] meshSpotAvailable;
	List<int> spawnQueue,despawnQueue;

	CellData[,] cells;
	BuildingData[] buildingData;
	List<int> spawnedBuildings;

	float playerAngle;

	Camera previousCamera=null;
	Vector3 prevCPos=Vector3.zero;

	CullingGroup buildingCullingGroup;
	BoundingSphere[] buildingSpheres;


	/* 
		EVENT FUNCTIONS
						*/

	void Start(){
		worldGen=(FinalWorldGen)Object.FindObjectOfType(typeof(FinalWorldGen));
		buildingParent=new GameObject("Building Holder").transform;
		buildingParent.parent=transform;
		spawnQueue=new List<int>();
		despawnQueue=new List<int>();
		spawnedBuildings=new List<int>();
		if (constantMeshBuildingCount>2500) constantMeshBuildingCount=2500; //Otherwise we have too many vertices
	}

	void Init(){
		if (!useCullingGroupAPI){
			cellSize=2*worldGen.worldRadius/cellsPerSide; //width
			cellRadius=Mathf.Sqrt(2)*cellSize/2; //Smallest radius that encompasses the whole of a cell
			cells=new CellData[cellsPerSide,cellsPerSide];
			for (i=0;i<cellsPerSide;i++){
				for (j=0;j<cellsPerSide;j++){
					cells[i,j]=new CellData();
					cells[i,j].buildings=new List<int>();
					cells[i,j].center=Vector3.right*(cellSize*(i+0.5f)-worldGen.worldRadius)+Vector3.forward*(cellSize*(j+0.5f)-worldGen.worldRadius);//(1100-1000,0,200 0-1000) (-450,)
				}
			}
		}else{
			buildingCullingGroup=new CullingGroup();
			buildingCullingGroup.targetCamera=Camera.main;
			buildingSpheres=new BoundingSphere[worldGen.buildings.Count];
			buildingCullingGroup.SetBoundingSpheres(buildingSpheres);
			buildingCullingGroup.SetBoundingSphereCount(worldGen.buildings.Count);
			buildingCullingGroup.onStateChanged=BuildingSphereStateChanged;
			//buildingCullingGroup.SetDistanceReferencePoint(Camera.main.transform);
			//buildingCullingGroup.SetBoundingDistances(new float[]{10,20,30});
		}

		buildingData=new BuildingData[worldGen.buildings.Count];
		for(i=0;i<worldGen.buildings.Count;i++){
			buildingData[i]=new BuildingData();
			buildingData[i].pos=WorldGenLib.BuildingToPosition(worldGen.buildings[i]);
			if (!useCullingGroupAPI){
				buildingData[i].cellX=Mathf.FloorToInt((buildingData[i].pos.x/(2*worldGen.worldRadius)+0.5f)*cellsPerSide); //5
				buildingData[i].cellY=Mathf.FloorToInt((buildingData[i].pos.z/(2*worldGen.worldRadius)+0.5f)*cellsPerSide); //7
				cells[buildingData[i].cellX,buildingData[i].cellY].buildings.Add(i);
				if(Vector3.Distance(buildingData[i].pos,cells[buildingData[i].cellX,buildingData[i].cellY].center)-cellRadius>0.01f){
					Debug.Log(buildingData[i].pos+","+buildingData[i].cellX+","+buildingData[i].cellY+","+cells[buildingData[i].cellX,buildingData[i].cellY].center);
				}
			}else
				buildingSpheres[i]=new BoundingSphere(buildingData[i].pos+Vector3.up*worldGen.buildings[i].height/2f,worldGen.buildings[i].height/2);
			buildingData[i].isSpawned=false;
			buildingData[i].hasSpawned=false;
			buildingData[i].shouldBeSpawned=false;
			GenBuildingBounds(i);
		}

		/*if (constantMeshBuildingCount>worldGen.buildings.Length) constantMeshBuildingCount=worldGen.buildings.Length;
		Mesh buildingMesh=new Mesh();
		buildingMesh.vertices=new Vector3[constantMeshBuildingCount*22];
		buildingMesh.triangles=new int[]{};
		buildingMesh.uvs=new Vector2[constantMeshBuildingCount*22];*/                            


		//CreateBuildings();
		inited=true;
	}

	void FixedUpdate(){
		if (!inited) return;

		//With a shared mesh, the way this works is: 
		//Despawn buildings we want to despawn

		int creationsPerTick=10;
		int i=0,despawnIndex=0;
		spawnListLen=spawnQueue.Count;
		despawnListLen=despawnQueue.Count;
		playerAngle=WorldGenLib.WorldToPolar(Camera.main.transform.position).y;	
		float buildingAngle;
		int spawnDespawnDiscrepancy=spawnQueue.Count-despawnQueue.Count;// we need to find x buildings to despawn
		/*if (spawnDespawnDiscrepancy>0){
			buildingAngle=worldGen.buildings[spawnQueue[0]].angle;
			if (AngleDist(playerAngle,buildingAngle)<0){
				//Take from the end of the list
				for (i=spawnedBuildings.Count-1;i>spawnedBuildings.Count-1-spawnDespawnDiscrepancy;i--)
					despawnQueue.Add(spawnedBuildings[i]);
			}else{
				//Take from the start of the list
				Debug.Log(spawnedBuildings.Count+"<"+spawnDespawnDiscrepancy);
				for (i=0;i<spawnDespawnDiscrepancy;i++)
					despawnQueue.Add(spawnedBuildings[i]);
			}
		}*/
		for (i=Mathf.Min(spawnQueue.Count-1,creationsPerTick);i>0;i--){
			//Debug.Log(i);
			//find a building to despawn
				//despawnIndex=despawnQueue[0];
				//despawnQueue.RemoveAt(0);
			
			SpawnBuilding(spawnQueue[i]);
			spawnQueue.RemoveAt(i);
			//DespawnBuilding(despawnQueue[0]);
		}
		/*if (spawnListLen>spawnQueue.Count){
			SortSpawnedBuildingsByAngle(playerAngle,true); //The ones at the front will be behind the player.
		}*/
		for (i=Mathf.Min(creationsPerTick,despawnQueue.Count-1);i>0;i--){
			//Debug.Log(i);
			DespawnBuilding(despawnQueue[i]);
			despawnQueue.RemoveAt(i);
		}
		/*i=0;
		foreach(int index in despawnQueue){
			DespawnBuilding(index);
			i++;
			if (i>=destructionsPerTick) break;
		}*/
	}

	/*void SortSpawnedBuildingsByAngle(float playerAngle,bool behindFirst){
		float dangle1,dangle2=AngleDist(playerAngle,worldGen.buildings[spawnedBuildings[0]].angle);
		for (int i=1;i<spawnedBuildings.Count;i++){
			dangle1=dangle2;
			dangle2=AngleDist(playerAngle,worldGen.buildings[spawnedBuildings[i]].angle);
			if ((dangle2<dangle1&&behindFirst)||(dangle2>dangle1&&!behindFirst)){
				int stor=spawnedBuildings[i];
				spawnedBuildings[i]=spawnedBuildings[i-1];
				spawnedBuildings[i-1]=stor;
				SortSpawnedBuildingsByAngle(playerAngle,behindFirst);
				return;
			}
		}
		return;
	}*/

	bool BuildingBehindPlayer(float buildingAngle){
		if (AngleDist(playerAngle,buildingAngle)>0) return false;
		return true;
	}

	float AngleDist(float angle1,float angle2){
		/*if (angle1<=90) angle2-=360;
		else if (angle2<=90) angle1-=360;
		if (angle1>=270) angle1-=360;
		if (angle2>=270) angle2-=360;
		return angle2-angle1;*/
		float dist=angle2-angle1;
		if (dist>180) dist-=360;
		else if (dist<-180) dist+=360;
		return dist;
	}

	/* 
		BUILDING CREATION
							*/

	void CreateBuildings(){
		for(int i=0;i<worldGen.buildings.Count;i++)
			SpawnBuilding(i);
	}

	void SpawnBuilding(int index){
		if (buildingData[index].isSpawned) return;
		SpawnBuildingObject(index);
		if (spawnedBuildings.Count==0){
			spawnedBuildings.Add(index);
			return;
		}
		//Spawned buildings is sorted by distance from player, starting with buildings behind the player
		/*float buildingAngleDist=AngleDist(playerAngle,worldGen.buildings[index].angle),previousAngleDist,nextAngleDist=AngleDist(playerAngle,worldGen.buildings[spawnedBuildings[0]].angle);
		for(int i=1;i<spawnedBuildings.Count;i++){
			previousAngleDist=nextAngleDist;
			nextAngleDist=AngleDist(playerAngle,worldGen.buildings[spawnedBuildings[i]].angle);
			if (previousAngleDist<=buildingAngleDist&&nextAngleDist>=buildingAngleDist){
				spawnedBuildings.Insert(i,index);
				return;
			}
		}*/
		spawnedBuildings.Add(index);
	}

	void SpawnBuildingInConstantMesh(int index){

	}

	void AddToSpawnQueue(int index){
		buildingData[index].shouldBeSpawned=true;
		spawnQueue.Add(index);
	}

	void SpawnBuildingObject(int index){
		//Debug.Log("Spawning building "+index);
		b=worldGen.buildings[index];
		GenBuildingBounds(index);
		//Transform bt=(Instantiate(buildingPlaceholder,buildingData[index].bounds.center,Quaternion.Euler(Vector3.up*b.yRot),buildingParent)).transform;
		//bt.localScale=buildingData[index].bounds.size;
		GameObject bg=new GameObject("Building "+index);
		bg.transform.parent=buildingParent;
		bg.transform.position=buildingData[index].bounds.center;
		bg.transform.eulerAngles=Vector3.up*b.yRot;
		BuildingGeneratorv2 gen=bg.AddComponent<BuildingGeneratorv2>();
		gen.bounds=buildingData[index].bounds.size;
		gen.sideUVOffsetPerUnit=new Vector2(0.6f,0.4f);
		gen.roofUVOffsetPerUnit=Vector2.one*0.5f;
		gen.generateUVs=true;
		gen.topRelative=false;
		gen.buildingShape=buildingBaseMesh;
		if (buildingData[index].cachedMesh!=null)
			gen.overrideMesh=buildingData[index].cachedMesh;
		MeshRenderer mr=bg.GetComponent<MeshRenderer>();
		mr.sharedMaterials=new Material[]{windowMat,//(Material)AssetDatabase.LoadAssetAtPath("Assets/Graphics/Materials/Buildings/ShinyWindow.mat",typeof(Material)),
									roofMat/*(Material)AssetDatabase.LoadAssetAtPath("Assets/Graphics/Materials/Buildings/TiledRoof.mat",typeof(Material))*/};

		buildingData[index].isSpawned=true;
		buildingData[index].hasSpawned=true;
		buildingData[index].shouldBeSpawned=true;
		buildingData[index].t=bg.transform;
		activeBuildings++;
		activeVertexCount+=22;
	}

	void AddToDespawnQueue(int index){
		buildingData[index].shouldBeSpawned=false;
		despawnQueue.Add(index);
	}

	void DespawnBuilding(int index){
		if (!buildingData[index].isSpawned) return;
		DespawnBuildingObject(index);
		spawnedBuildings.RemoveAt(spawnedBuildings.IndexOf(index));
	}

	void DespawnBuildingObject(int index){
		//Debug.Log("Despawning "+index);
		buildingData[index].cachedMesh=buildingData[index].t.gameObject.GetComponent<MeshFilter>().mesh;
		Destroy(buildingData[index].t.gameObject);
		buildingData[index].t=null;
		buildingData[index].isSpawned=false;
		activeBuildings--;
		activeVertexCount-=22;
		buildingData[index].shouldBeSpawned=false;
	}

	/*
		BUILDING DECISIONS
							*/

	void BuildingSphereStateChanged(CullingGroupEvent evt){
		if (evt.hasBecomeVisible){
			SpawnBuilding(evt.index);
		}
		Vector3 cPos=Camera.main.transform.position;
		cPos.y=0;
		float buildingDist=Vector3.Distance(buildingData[evt.index].pos,cPos);
		if (evt.hasBecomeInvisible&&buildingDist>50){
			//Debug.Log(buildingCullingGroup.GetDistance(evt.index));
			DespawnBuilding(evt.index);
		}
	}

	void Update(){
		if (!inited&&FinalWorldGen.buildProgress>=1.1f)
			Init();

		if (!inited) return;
		Camera c= Camera.main;
		if (c==null) return;
		if (useCullingGroupAPI) return;
		//if (c!=Camera.main) return;
		//Debug.Log(c);
		Vector3 cPos=c.transform.position;
		cPos.y=0;
		canSpawn=true;
		canDespawn=true;
		float cellCentreDist;
		maxCellDist=c.farClipPlane/Mathf.Cos(c.fov*Mathf.Deg2Rad/2);
		bool creationOverride=(c!=previousCamera)||(Vector3.Distance(prevCPos,cPos)>cellRadius);
		if (creationOverride)
			Debug.Log("Complete creation "+maxCellDist);
		cellsSampled=0;
		//playerAngle=
				//Debug.Log(cells[0,0].center);
		for (i=0;i<cellsPerSide;i++){
			for (j=0;j<cellsPerSide;j++){
				cellCentreDist=Vector3.Distance(cells[i,j].center,cPos);
				//if (creationOverride)
				//	Debug.Log(cellCentreDist);
				if (cellCentreDist-cellRadius<maxCellDist&&(cellCentreDist+cellRadius>maxCellDist||creationOverride)){
					HandleCell(i,j,cPos,maxCellDist,creationOverride,cellCentreDist);
				}/*else if (cellCentreDist>700){
					Debug.Log(cellCentreDist+cellRadius+","+maxCellDist);
				}
				//if (!canSpawn&&!canDespawn&&!creationOverride) break;*/
			}
		}
		previousCamera=c;
		prevCPos=cPos;
	}

	/*void OnDrawGizmos(){
		Gizmos.color=Color.red;
		Gizmos.DrawWireSphere(Camera.main.transform.position,maxCellDist+worldGen.maxBuildingDim);
	}*/

	void HandleCell(int cellX,int cellY,Vector3 cPos,float maxSightRadius,bool creationOverride,float cellCentreDist){
		float buildingDist,maxBuildingDist=maxSightRadius+worldGen.maxBuildingDim;
		//Debug.Log(cellX+","+cellY);
		//Debug.Log(maxBuildingDist);
		foreach(int index in cells[cellX,cellY].buildings){
			buildingDist=Vector3.Distance(buildingData[index].pos,cPos);
			//if (Vector3.Distance(buildingData[index].pos,cells[cellX,cellY].center)>cellRadius){
			//	Debug.Log("WOT");
			//}
			//if (!buildingData[index].shouldBeSpawned)
			//	Debug.Log(buildingDist);
			//if (buildingDist>maxSightRadius+)
			if (buildingDist<maxBuildingDist&&((!buildingData[index].shouldBeSpawned&&!buildingData[index].isSpawned)||creationOverride)){
				if (creationOverride)
					SpawnBuilding(index);
				else
					AddToSpawnQueue(index);
				//SpawnBuilding(index);
				//canSpawn=false;
				//if (!canDespawn&&!creationOverride) break;
			}else if (buildingDist>maxBuildingDist&&(creationOverride||(buildingData[index].isSpawned && buildingData[index].shouldBeSpawned))){
				//Debug.Log("Despawned: "+buildingDist);
				if (creationOverride)
					DespawnBuilding(index);
				else
					AddToDespawnQueue(index);
				//DespawnBuilding(index);
				//canDespawn=false;
				//if (!canSpawn&&!creationOverride) break;
			}
				//Debug.Log(buildingDist);
			
		}
		cellsSampled++;

	}

	void GenBuildingBounds(int index){
		if (buildingData[index].bounds.size!=Vector3.zero) return;
		buildingData[index].bounds=new Bounds(buildingData[index].pos,new Vector3(b.width,b.height,b.depth));
	}

	void OnDisable(){
		if (useCullingGroupAPI)
			buildingCullingGroup.Dispose();
	}

	/*void OnEnable(){
		Camera.onPreCull+=HandleBuildings;
	}
	void OnDisable(){
		Camera.onPreCull-=HandleBuildings;
	}*/
}
