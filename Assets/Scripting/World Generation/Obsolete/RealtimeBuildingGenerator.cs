using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RealtimeBuildingGenerator : MonoBehaviour {
	public int cellsPerSide=64,maxPoolCount=64;
	[ReadOnly]public int currentBuildingCount=0;
	public Transform player;
	public Transform buildingParent,roofPoolParent;
	public GameObject[] roofObjects;
	public float[] roofObjectHeights;
	public string arenaLayerName;
	public bool generateRoofs=false;

	int buildingCount,arenaLayer;
	float cellWidth,cellRadius;
	FinalWorldGen worldGen;
	List<int> created,notCreated;
	Mesh[] buildingMeshes;
	GameObject[] buildingObjects;//,roofPoolObjects;
	List<GameObject> roofPoolObjects;
	List<RoofTileID> roofPoolTypes;
	Vector3[] buildingPositions;
	Roof[] buildingRoofs;
	Vector3[,] cellPositions;
	bool[] buildingStatus,buildingRoofStatus;
	List<int> roofPoolObjectStatus;
	public List<int>[,] cells;
	public static RealtimeBuildingGenerator current;

	void Start(){
		if(current==null)
			current=this;
		else{
			Debug.Log("Destroying Building Generator as another already exists");
			this.enabled=false;
			return;
		}
		worldGen=GetComponent<FinalWorldGen>();
		if (FinalWorldGen.buildProgress>=1)
			InitResources();
		arenaLayer=LayerMask.NameToLayer(arenaLayerName);
	}

	void InitResources(){
		buildingCount=worldGen.buildings.Count;
		buildingStatus=new bool[buildingCount];
		//buildingRoofStatus=new bool[buildingCount];
		//buildingRoofs=new Roof[buildingCount];
		//roofPoolObjectStatus=new int[buildingCount];
		buildingPositions=new Vector3[buildingCount];
		//Init cells
		cells=new List<int>[cellsPerSide,cellsPerSide];
		cellPositions=new Vector3[cellsPerSide,cellsPerSide];
		cellWidth=worldGen.worldRadius*2f/cellsPerSide;
		cellRadius=Mathf.Sqrt(cellWidth*cellWidth*2);
		for(int i=0;i<cellsPerSide;i++){
			for(int j=0;j<cellsPerSide;j++){
				cells[i,j]=new List<int>();
				//use i/cps and j/cps as x,y
				//(x-0.5) *2wr
				cellPositions[i,j]=new Vector3((i*1.0f/cellsPerSide - 0.5f)*2*worldGen.worldRadius+cellWidth/2,0,(j*1.0f/cellsPerSide - 0.5f)*2*worldGen.worldRadius+cellWidth/2);
			}
		}
		
		int cellX=0,cellY=0;
		for(int i=0;i<buildingCount;i++){
			buildingStatus[i]=false;
			//buildingRoofStatus[i]=false;
			buildingPositions[i]=WorldGenLib.BuildingToPosition(worldGen.buildings[i]);//+Vector3.up*worldGen.buildings[i].height;
			//Get building cell
			//if bx=0 cx=1/2 cellsPerSide
			//if bx=-wr cx=0
			//if bx=wr cx=cellsPerSide
			//cx=(bx/(2wr)+1/2)*cellsPerSide
			cellX=Mathf.FloorToInt((buildingPositions[i].x/(2*worldGen.worldRadius)+0.5f)*cellsPerSide);
			cellY=Mathf.FloorToInt((buildingPositions[i].z/(2*worldGen.worldRadius)+0.5f)*cellsPerSide);
			cells[cellX,cellY].Add(i);
			Debug.Log("Added building to "+cellX+","+cellY);
		}
		buildingMeshes=new Mesh[buildingCount];
		buildingObjects=new GameObject[buildingCount];

		if(buildingParent==null)
			buildingParent=new GameObject("Generated Buildings").transform;
		//if(roofPoolParent==null)
		//	roofPoolParent=new GameObject("Roof Pool").transform;
		buildingParent.parent=transform;
		//roofPoolParent.position=Vector3.down*50;

		/*roofPoolObjects=new GameObject[maxPoolCount];
		for(int i=0;i<maxPoolCount;i++){
			roofPoolObjectStatus[i]=-1;
			roofPoolObjects[i]=(Instantiate(crate) as GameObject);
			roofPoolObjects[i].transform.parent=roofPoolParent;
			roofPoolObjects[i].transform.localPosition=Vector3.zero;
			roofPoolObjects[i].layer=arenaLayer;
		}*/
		//roofPoolObjects=new List<GameObject>();
		//roofPoolObjectStatus=new List<int>();
		//roofPoolTypes=new List<RoofTileID>();
	}

	void Update(){
		if(player==null){
			if (FinalWorldGen.buildProgress>=1)
				InitResources(); 
			else
				return;
			GameObject player_g=GameObject.FindWithTag("Player");
			if (player_g==null) return;
			player=player_g.transform;
			//Cycle through cells 
			//if dist<buildingGen build building
			//if dist<roofGen build roof
			float dist;
			Debug.Log("Initial building spawn");
			for(int i=0;i<cellsPerSide;i++){
				for(int j=0;j<cellsPerSide;j++){
					dist=(cellPositions[i,j]+Vector3.up*player.position.y-player.position).magnitude;
					//if(dist-cellRadius<WorldData.buildingGenRadius){ 
						Debug.Log("Player in cell radius of "+i+","+j+", has building count "+cells[i,j].Count);
						foreach(int index in cells[i,j])
							SpawnBuilding(index);
					//}
					//if(dist-cellRadius<WorldData.roofGenRadius && generateRoofs){
					//	foreach(int index in cells[i,j])
					//		CreateBuildingRoof(index);
					//}
				}
			}
		}else{
			bool canSpawn=true,canDespawn=true,canCreateRoof=true;
			float dist;
			for(int i=0;i<cellsPerSide&&((canSpawn&&canDespawn)||canCreateRoof);i++){
				for(int j=0;j<cellsPerSide&&((canSpawn&&canDespawn)||canCreateRoof);j++){
					dist=(cellPositions[i,j]+Vector3.up*player.position.y-player.position).magnitude;
					if (dist-cellRadius<WorldData.buildingGenRadius&&dist+cellRadius>WorldData.buildingGenRadius&&canSpawn&&canDespawn) 
						BuildBuildingsInCell(i,j,ref canSpawn,ref canDespawn);
					//if (dist-cellRadius<WorldData.roofGenRadius&&dist+cellRadius>WorldData.roofGenRadius&&generateRoofs)//
					//	BuildRoofsInCell(i,j,ref canCreateRoof);
				}
			}
		}
	}

	void BuildBuildingsInCell(int i,int j,ref bool canSpawn,ref bool canDespawn){
		int buildingChange;
		foreach(int index in cells[i,j]){
			buildingChange=HandleBuildingSpawn(index,canSpawn,canDespawn);
			if(buildingChange<0)
				canDespawn=false;
			if(buildingChange>0)
				canSpawn=false;
			if(!canSpawn&&!canDespawn)
				break;
		}
	}

	/*void BuildRoofsInCell(int i,int j,ref bool canCreateRoof){
		Vector3 deltaVec;
		foreach(int index in cells[i,j]){
			deltaVec=buildingPositions[index]-player.position;
			deltaVec.y=0;
			if (!buildingRoofStatus[index]&&deltaVec.magnitude<WorldData.roofGenRadius&&canCreateRoof){
				CreateBuildingRoof(index);
				canCreateRoof=false;
			}else if (buildingRoofStatus[index]&&deltaVec.magnitude>WorldData.roofGenRadius)
				FreeBuildingRoof(index);
		}
	}*/

	void SpawnBuilding(int index){
		Debug.Log(index);
		if (buildingObjects[index]!=null)
			return;
		Building b=worldGen.buildings[index];
		BuildingTemplate bt=worldGen.buildingTemplate;
		GameObject bObj=new GameObject("Building "+index);
		bObj.AddComponent<MeshFilter>().mesh=BuildingMesh(index);
		bObj.AddComponent<MeshRenderer>().materials=new Material[]{bt.material,bt.roofMaterial};
		if(bt.useBoxCollider)
			bObj.AddComponent<BoxCollider>().size=new Vector3(b.width,b.height,b.depth);
		else
			bObj.AddComponent<MeshCollider>();
		bObj.transform.position=buildingPositions[index];
		bObj.transform.localEulerAngles=Vector3.up*b.yRot;
		bObj.transform.parent=buildingParent;
		bObj.layer=arenaLayer;
		buildingObjects[index]=bObj;
		buildingStatus[index]=true;
		currentBuildingCount++;
	}

	void DestroyBuilding(int index){
		if (buildingObjects[index]==null)
			return;
		Destroy(buildingObjects[index]);
		buildingObjects[index]=null;
		buildingStatus[index]=false;
		currentBuildingCount--;
	}

	Mesh BuildingMesh(int index){
		Mesh m=buildingMeshes[index];
		if(m==null){
			Building b=worldGen.buildings[index];
			BuildingTemplate bt=worldGen.buildingTemplate;
			Vector3 size=new Vector3(b.width,b.height,b.depth);
			m=BuildingMeshGenLib.GenerateBuildingMesh(bt.mesh,size,true,bt.sideUVOffsetPerUnit,bt.roofUVOffsetPerUnit);
			buildingMeshes[index]=m;
		}
		return m;
	}

	int HandleBuildingSpawn(int index,bool canSpawn=true,bool canDespawn=true){
		if (player==null){
			if(!buildingStatus[index]){
				SpawnBuilding(index);
				return 1;
			}
		}else{
			Vector3 deltaVec=buildingPositions[index]-player.position;
			deltaVec.y=0;
			if (buildingStatus[index]&&deltaVec.magnitude>WorldData.buildingGenRadius&&canDespawn){
				DestroyBuilding(index);
				return -1;
			}else if (!buildingStatus[index]&&deltaVec.magnitude<WorldData.buildingGenRadius&&canSpawn){
				SpawnBuilding(index);
				return 1;
			}
		}
		return 0;
	}

	/*void CreateBuildingRoof(int index){
		if (buildingRoofStatus[index]) return;
		Roof r=buildingRoofs[index];
		if (!r.isValid){
			r=BuildingGenLib.RoofFromBuilding(worldGen.buildings[index]);
			buildingRoofs[index]=r;
		}
		/*int objIndex=GetNextFreeRoofObject();
		if (objIndex>=0){
			//roofPoolObjects[objIndex].SetActive(true);
			roofPoolObjects[objIndex].transform.position=buildingPositions[index]+Vector3.up*(worldGen.buildings[index].height+0.5f);
			roofPoolObjectStatus[objIndex]=index;
		}
		for(int i=0;i<r.gridWidth;i++){
			for(int j=0;j<r.gridDepth;j++){
				//if (r.tileTypes[i,j]!=RoofTileID.Box){
					/*int objIndex=GetNextFreeRoofObject();
					if(objIndex>=0){
						roofPoolObjects[objIndex].transform.position=PositionFromRoof(i,j,r)+buildingPositions[index]+Vector3.up*(worldGen.buildings[index].height+1f);
						roofPoolObjectStatus[objIndex]=index;
					}
					PopulateRoofCell(r.tileTypes[i,j],index,i,j);
				//}
			}
		}
		buildingRoofStatus[index]=true;
	}

	Vector3 PositionFromRoof(int i,int j,Roof r){
		//x=i-r.width/2*roofgridcellsize
		return new Vector3((i-r.gridWidth*0.5f)*BuildingGenLib.roofGridCellSize,0,(j-r.gridDepth*0.5f)*BuildingGenLib.roofGridCellSize);
	}

	void PopulateRoofCell(RoofTileID rid, int index, int i, int j){
		if (rid==RoofTileID.None||rid==RoofTileID.Start) return;
		if (!buildingStatus[index]) return;
		int objIndex=GetNextFreeRoofObject(rid);
		if (objIndex<0) return;
		roofPoolObjects[objIndex].transform.parent=buildingObjects[index].transform;
		roofPoolObjects[objIndex].transform.localPosition=PositionFromRoof(i,j,buildingRoofs[index])+Vector3.up*(worldGen.buildings[index].height+roofObjectHeights[((int)rid)-2]);
		roofPoolObjectStatus[objIndex]=index;
	}

	void FreeBuildingRoof(int index){
		for(int i=0;i<roofPoolObjects.Count;i++){
			if(roofPoolObjectStatus[i]==index)
				FreePooledObject(i);
		}
		buildingRoofStatus[index]=false;
	}

	void FreePooledObject(int objIndex){
		roofPoolObjects[objIndex].transform.localPosition=Vector3.zero;//SetActive(false);
		roofPoolObjectStatus[objIndex]=-1;
	}

	int GetNextFreeRoofObject(RoofTileID roofObjType){
		for(int i=0;i<roofPoolObjects.Count;i++)
			if (roofPoolObjectStatus[i]<0 && roofPoolTypes[i]==roofObjType) return i;
		return AddRoofObjectToPool(roofObjType);
	}

	int AddRoofObjectToPool(RoofTileID roofObjType){
		int i=roofPoolObjects.Count;
		roofPoolObjectStatus.Add(-1);
		roofPoolObjects.Add(Instantiate(roofObjects[((int)roofObjType)-2]) as GameObject);
		roofPoolObjects[i].transform.parent=roofPoolParent;
		roofPoolObjects[i].transform.localPosition=Vector3.zero;
		roofPoolObjects[i].layer=arenaLayer;
		roofPoolObjects[i].name+=" "+roofPoolParent.childCount;
		roofPoolTypes.Add(roofObjType);
		return i;
	}*/

	public void Reset(){
		player=null;
		for(int i=0;i<buildingCount;i++){
			DestroyBuilding(i);
			//FreeBuildingRoof(i);
		}
		currentBuildingCount=0;
	}

	public int BuildingFromTransform(Transform t){
		if (t.root!=transform) return -1;
		for(int i=0;i<buildingObjects.Length;i++)
			if (buildingObjects[i].transform==t) return i;
		return -1;
	}
}
