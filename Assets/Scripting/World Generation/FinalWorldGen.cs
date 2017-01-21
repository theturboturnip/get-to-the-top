using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;

public class FinalWorldGen : MonoBehaviour {

	/*
		VARIABLES
					*/

	public static int seed=-1;
	public float worldRadius=5000;
	public bool generateOnStart=true,useAABB=true,threaded=true;
	public int ringCount;
	public float buildProgressAtPathGen=0,buildProgressAtPathFill=0.5f,buildProgressAtFrontFind=0.75f;
	[Space(10)]
	public float pathStartAngle=0;
	public float pathEndAngle=360;
	public float pathStartHeight,pathEndHeight;
	public float minPathStartDist,maxPathStartDist;
	public float minBuildingSeperation;
	public float maxDistDeviation,maxHeightDeviation;
	public float insideHeightDelta;
	public int pathNumber;

	[Space(10)]
	public float minBuildingDim;
	public float maxBuildingDim;
	float maxBuildingRadius;
	public int maxChainedWallrunsAtEnd,maxChainedGapsAtEnd;
	
	[Space(10)]
	public List<Building> buildings;
	public Sector[] sectors;
	public BuildingTemplate buildingTemplate;
	List<int>[] rings;
	public int aabbPreventedCollisions=0,collisions;
	public GameObject[] enableOnFinish;

	public static float buildProgress=0f;
	public Thread generationThread;
	public System.Random threadRandom;
	public static FinalWorldGen current;

	/*
		EVENT FUNCTIONS
						*/

	public void Start(){
		current=this;
		buildProgress=0f;
		maxBuildingRadius=maxBuildingDim/Mathf.Sqrt(2);

		if (generateOnStart)
			StartGeneration();
	}

	public void StartGeneration(){
		if (!PreGenerate()) return;

		if (threaded){
			Debug.Log("Beginning Gen");
			generationThread=new Thread(new ThreadStart(Generate));
			generationThread.IsBackground=true;
			generationThread.Start();
			//generationThread.Sleep(5000);
		}else Generate();
	}

	void Update(){
		//Debug.Log(buildProgress);
		if (generationThread!=null&&buildProgress>=1&&threaded){
			Debug.Log("Killing thread");
			generationThread.Join();
			generationThread=null;
			PostGenerate();
		}
	}

	void OnApplicationQuit(){
		if (generationThread!=null&&threaded)
			generationThread.Abort();
	}

	void OnDisable(){
		buildProgress=0f;
	}

	/*
		GENERATION
					*/

	bool PreGenerate(){
		if (pathNumber<=0) return false;
		if (worldRadius<=0) return false;
		if (maxPathStartDist>worldRadius-50) maxPathStartDist=worldRadius-50;

		buildProgress=0;
		buildings=new List<Building>();
		threadRandom=new System.Random(seed);

		if (maxChainedWallrunsAtEnd <1) maxChainedWallrunsAtEnd=1;

		if (ringCount<1) ringCount=1;
		rings=new List<int>[(int)(ringCount)];
		for (int r=0;r<rings.Length;r++)
			rings[r]=new List<int>();

		return true;
	}

	void Generate(){
		int p=0;
		float startDist,endDist;
		startDist=minPathStartDist;//SuperMaths.RandomRange(threadRandom,minPathStartDist,maxPathStartDist);
		bool samePathStart=false;
		for (p=0;p<pathNumber;p++){
			if (!samePathStart)
				startDist=Mathf.Lerp(maxPathStartDist,minPathStartDist,p*1f/pathNumber);//SuperMaths.RandomRange(threadRandom,minPathStartDist,maxPathStartDist);
			endDist=SuperMaths.RandomRange(threadRandom,minPathStartDist,maxPathStartDist);
			FillPath(startDist,endDist,threadRandom);
			buildProgress=(p+1)*1f/pathNumber;
		}
		buildProgress=1;
	}

	void PostGenerate(){
		Debug.Log("Enabling");
		foreach(GameObject g in enableOnFinish){
			g.SetActive(true);
			Debug.Log("Enabled "+g);
		}

		StartLevelTrigger st=(StartLevelTrigger)UnityEngine.Object.FindObjectOfType(typeof(StartLevelTrigger));
		st.transform.position=(buildings[0].height+2f)*Vector3.up+(buildings[0].depth/2+1.75f)*Vector3.forward+WorldGenLib.BuildingToPosition(buildings[0]);
		st.transform.eulerAngles=Vector3.up*180;

		LevelHandler.currentLevel.WorldGenComplete();

		buildProgress=1.1f;
	}

	void FillPath(float startDist,float endDist,System.Random threadRandom){
		float theta=pathStartAngle,dtheta=0;
		float p=(theta-pathStartAngle)/(pathEndAngle-pathStartAngle);
		float w,l,h;
		float w1=RandomDimension(),l1=RandomDimension(),h1=Mathf.Lerp(pathStartHeight,pathEndHeight,p);
		float dist,dist1=startDist;
		float isectDist;
		int buildingType=0;
		int[] previousTypes=new int[]{-1,-1,-1};

		while (theta<pathEndAngle){
			//Apply 'new' values from previous loop to current values
			w=w1;
			l=l1;
			h=h1;
			dist=dist1;
			AddBuilding(w,l,h,theta,dist);

			//Find all building data not based on theta
			w1=RandomDimension();
			
			//Generate next theta
			isectDist=Mathf.Sqrt(0.25f*w*w+(dist-l/2)*(dist-l/2));

			dtheta=( Mathf.Asin(w/(2*isectDist))+Mathf.Asin(w1/(2*isectDist))+Mathf.Acos(1-minBuildingSeperation*minBuildingSeperation/(2*isectDist*isectDist)) )*Mathf.Rad2Deg;
			if (theta+dtheta>pathEndAngle)
				dtheta=pathEndAngle-theta;
			theta+=dtheta;

			//Generate remaining building data
			p=(theta-pathStartAngle)/(pathEndAngle-pathStartAngle);
			buildingType=NextBuildingType(previousTypes,threadRandom,p);
			l1=RandomDimension();
			dist1=Mathf.Lerp(startDist,endDist,p)+SuperMaths.RandomRange(threadRandom,-maxDistDeviation,maxDistDeviation);
			if (buildingType==1){
				float deviation=Mathf.Sign((float)threadRandom.NextDouble()-0.5f);
				if (deviation==0) deviation=1;
				dist1+=w1*deviation;
			}
			h1=Mathf.Lerp(pathStartHeight,pathEndHeight,p)+insideHeightDelta*(1-(dist1-minPathStartDist)/(maxPathStartDist-minPathStartDist))+SuperMaths.RandomRange(threadRandom,-maxHeightDeviation,maxHeightDeviation);
			if (buildingType==1)
				h1+=50; //SHOULD BE RANDOM VAL
			buildProgress+=(dtheta)/(pathEndAngle-pathStartAngle)*1f/pathNumber;
		
			previousTypes[2]=previousTypes[1];
			previousTypes[1]=previousTypes[0];
			previousTypes[0]=buildingType;
		}
	}

	int NextBuildingType(int[] previousTypes,System.Random threadRandom,float p){
		float randVal=(float)threadRandom.NextDouble();
		if (previousTypes[0]==-1)
			return 0;
		if (randVal>0.7f){
			int maxChainedWallruns=Mathf.RoundToInt(Mathf.Lerp(1,maxChainedWallrunsAtEnd,p));
			if (maxChainedWallruns-1>=previousTypes.Length) return 1;
			//if all of the previous between maxChainedWallruns-1 and 0 are 1 then we can't 
			//=>
			for (int i=0;i<maxChainedWallruns;i++){
				if (previousTypes[i]!=-1)
					return 1;
			}
			return 0;
		}
		if (randVal>0.6f){
			int maxChainedGaps=Mathf.RoundToInt(Mathf.Lerp(0,maxChainedGapsAtEnd,p));
			if (maxChainedGaps-1>=previousTypes.Length) return 2;
			if (maxChainedGaps==0) return 0;
			for (int i=0;i<maxChainedGaps;i++){
				if (previousTypes[i]!=2)
					return 2;
			}
			return 0;
		}

		return 0;
	}

	void AddBuilding(float w,float l,float h,float theta,float dist){
		Building b= new Building();
		b.yRot=theta;
		b.distance=dist;
		b.angle=theta;
		b.height=h;
		b.width=w;
		b.depth=l;
		b.radius=Mathf.Max(l/2,w/2)*Mathf.Sqrt(2);
		b.inited=true;

		int currentRing=(int)Mathf.Clamp(Mathf.FloorToInt(b.distance*ringCount/worldRadius),0,rings.Length-1);
		int ringOffset=Mathf.CeilToInt((2*maxBuildingRadius*ringCount)/worldRadius);

		for(int r=(int)Mathf.Max(0,currentRing-ringOffset);(r<=currentRing+ringOffset)&&(r<rings.Length);r++){ //Check each ring within maxBuildingRadius

			for (int bi=0;bi<rings[r].Count;bi++){ //Foreach building in ring
				Building current = buildings[rings[r][bi]];

				float squaredMinDistance=(b.radius+current.radius)*(b.radius+current.radius);
				if (Mathf.Abs(b.distance-current.distance)>b.radius+current.radius) continue;

				if (SuperMaths.CosineRuleForASquared(b.distance,current.distance,b.angle-current.angle)<squaredMinDistance){ //More efficient to compare squared as avoids having to sqrt
					Vector4 currentBBox=WorldGenLib.BuildingAABB(current),newBBox=WorldGenLib.BuildingAABB(b);
					if (!SuperMaths.AABB2D(currentBBox,newBBox)){
						aabbPreventedCollisions++;
						continue;
					}
					return;
					//continue;
					//Merge the buildings
					//float distStart=Mathf.Max(b.distance+b.depth/2,current.distance+current.depth/2),distEnd=Mathf.Min(b.distance-b.depth/2,current.distance-current.depth/2);
					//if (b.distance>current.distance){
					//	current.distance=Mathf.Lerp(distStart,distEnd,0.75f);
					//	b.distance=Mathf.Lerp(distStart,distEnd,0.25f);
					/*}else{
						current.distance=Mathf.Lerp(distStart,distEnd,0.25f);
						b.distance=Mathf.Lerp(distStart,distEnd,0.75f);
					}*/
					//current.depth=b.depth=(distStart-distEnd)/2;

					//current.yRot=b.yRot=(b.yRot+current.yRot)/2;
					//current.angle=(b.angle+current.angle)/2;
					//scurrent.height=Mathf.Max(current.height,b.height);

					//current.distance=(current.distance+b.distance)/2;


					//buildings[rings[r][bi]]=current;
					//break;
					//return;
				}
			}
		}

		buildings.Add(b);
		rings[currentRing].Add(buildings.Count-1);

	}

	/*
		UTILITY
				*/

	float RandomDimension(){
		return SuperMaths.RandomRange(threadRandom,minBuildingDim,maxBuildingDim);
	}
}