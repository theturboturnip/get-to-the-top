using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;


public class LevelHandler : MonoBehaviour {

	public int currentLevelIndex,nextLevelIndex;
	public bool isProcGen=false;
	public static bool shouldSkipIntro=false;

	[ReadOnly]
	public string levelName="BADNAME";

	[Header("Spawn Data")]
	public bool startWithShotgun=true;
	public bool shouldResetLevel=true,isFirstLevel=false;
	public Transform[] spawnObjectWhitelist;

	[Header("Timing Data")]
	public int starRanking;
	public float levelStartTime=-1,levelEndTime,levelCompleteTime;
	[ReadOnly]
	public float oneStarTime,twoStarTime,threeStarTime;

	[Header("Misc")]
	public float maxYHeight;
	public float cloudLevel=-10f;
	public float checkpointActiveTime=3;

	public bool deathTip=false;

	public Vector3 currentCheckpointPosition;
	Vector3 currentCheckpointLookDir;
	public static Transform player;
	public static Shotgun shotgun;

	public static LevelHandler currentLevel;
	public static bool levelComplete=false,sendInEditor=false;
	GTTTCompleteImageEffect fadeEffect;
	bool respawning;
	
	public float loadLevelFadeTime=1;
	float loadLevelFadeStart=-1;

	AsyncOperation levelLoader;

	// Use this for initialization
	void Start () {
		#if UNITY_EDITOR
		Debug.Log(SaveData.GetLevelProgress(currentLevelIndex).timeTaken);
		#endif

		Level l=LevelData.GetLevel(currentLevelIndex);
		if (l==null){
			Debug.LogError("No level data found, commiting seppaku");
			this.enabled=false;
			return;
		}
		levelName=l.name;
		oneStarTime=l.oneStarTime;
		twoStarTime=l.twoStarTime;
		threeStarTime=l.threeStarTime;

		try{
			shotgun=GameObject.FindWithTag("Shotgun").GetComponent<Shotgun>();
			GameObject.FindWithTag("Shotgun").SetActive(false);
			player=GameObject.FindWithTag("Player").transform;
		}catch{
			Debug.Log("Error trying to find player");
		}

		//if (currentCheckpoint==null){
			/*currentCheckpoint=new GameObject("Temp Checkpoint").transform;
			currentCheckpoint.gameObject.AddComponent<LevelCheckpoint>();
			currentCheckpoint.position=player.position;
			currentCheckpoint.rotation=player.rotation;*/
			currentCheckpointPosition=player.position;
			currentCheckpointLookDir=player.forward;
		//}

		/*if (shouldResetLevel){
			LevelObject[] los=GameObject.FindObjectsOfType(typeof(LevelObject)) as LevelObject[];
			foreach(LevelObject lo in los){
				if (Array.IndexOf(spawnObjectWhitelist,lo.transform)>=0) continue;
				lo.gameObject.SetActive(false);
			}
		}*/

		fadeEffect=player.gameObject.GetComponentInChildren<GTTTCompleteImageEffect>();
		fadeEffect.fadeDirection=-1;
		fadeEffect.invertFade=false;
		fadeEffect.stopWhenFinished=true;
		fadeEffect.StartFading();
		if (isProcGen){
			fadeEffect.permaFade=true;
			fadeEffect.fadeProgress=0;
			player.gameObject.GetComponent<NuPlayer>().enabled=false;
		}
		levelComplete=false;
	}

	public void WorldGenComplete(){
		player.gameObject.GetComponent<NuPlayer>().enabled=true;
		//GTTTCompleteImageEffect fadeEffect=player.gameObject.GetComponentInChildren<GTTTCompleteImageEffect>();
		fadeEffect.permaFade=false;
		fadeEffect.StartFading();
	}

	void OnEnable(){
		currentLevel=this;
	}

	void Update(){
		if (player==null){
			GameObject player_g=GameObject.FindWithTag("Player");
			if (player_g!=null)
				player=player_g.transform;
			else return;
		}
		//Check if the player has fallen off
		if (fadeEffect.fadeProgress>2f&&respawning)
			RespawnPlayer();
		if (player.position.y<cloudLevel&&levelStartTime>0&&!respawning)
			BeginRespawnPlayer();
		

		if (loadLevelFadeStart!=-1){
			float loadLevelProgress=Mathf.Clamp01((Time.time-loadLevelFadeStart)/loadLevelFadeTime);
			if (loadLevelProgress==1)
				levelLoader.allowSceneActivation=true;
		}
	}
	
	void LiftObject (Transform t,float moveTime,float delay=0) {
		LevelObject lo=t.gameObject.GetComponent<LevelObject>();
		Collider c=t.gameObject.GetComponent<Collider>();
		if (lo==null) lo=t.gameObject.AddComponent<LevelObject>();
		t.gameObject.SetActive(true); //Do this now so that the collider will give us non-zero bounds

		//startY=y for collider top to be at cloudLevel
		float maxHeight=c.bounds.center.y+c.bounds.size.y;
		lo.startY=cloudLevel-maxHeight;
		//targetY=transform y
		lo.targetY=t.position.y;
		//timeToY=objectMoveTime
		lo.timeToY=moveTime;
		//delay=extraDelay+objectMoveDelay
		lo.timeDelay=delay;

		lo.StartMoving();
		t.position=new Vector3(t.position.x,lo.startY,t.position.z);
	}

	public void LiftObjectSet(Transform[] objects,bool cumulateDelay=true){
		if (objects==null) return;
		//Accumulating delay i.e. one after the other
		//=>time for up=checkpoint activation time/num of objects
		float timeToY=checkpointActiveTime/objects.Length;
		for(int i=0;i<objects.Length;i++){
			LiftObject(objects[i],timeToY*(i+1));
		}
	}

	public void ActivateCheckpoint(LevelCheckpoint c){
		currentCheckpointPosition=c.transform.TransformPoint(c.boxBounds.center);
		RaycastHit hit;
		if (Physics.Raycast(currentCheckpointPosition,Vector3.down,out hit))
			currentCheckpointPosition=hit.point+Vector3.up;
		currentCheckpointLookDir=c.transform.TransformDirection(c.spawnLookDir);
		LiftObjectSet(c.toRaise);
	}

	public void ActivateCheckpoint(Vector3 pos,Vector3 dir){
		currentCheckpointPosition=pos;
		currentCheckpointLookDir=dir;
	}

	void RespawnPlayer(){
		Debug.Log("Respawning");
		player.position=currentCheckpointPosition;
		player.rotation=Quaternion.identity;
		player.gameObject.BroadcastMessage("Reset",SendMessageOptions.DontRequireReceiver);
		//float dot=Vector3.Dot(currentCheckpointLookDir.normalized,Vector3.forward);
		player.gameObject.GetComponent<NuPlayer>().AlignLookDirection(currentCheckpointLookDir);
		//player.forward=currentCheckpointLookDir;
		//player.gameObject.GetComponentInChildren<Camera>().transform.forward=currentCheckpointLookDir;
		fadeEffect.fadeDirection=-1;
		fadeEffect.invertFade=false;
		fadeEffect.invertWhenFinished=true;
		fadeEffect.stopWhenFinished=false;
		fadeEffect.fadeProgress=0;
		fadeEffect.StartFading();
		respawning=false;
		if (deathTip){
			TipHandler.current.OpenTip("When you fall off, you'll be respawned at the most recent checkpoint you passed.",100,3f);
			deathTip=false;
		}
	}

	void BeginRespawnPlayer(){
		Debug.Log("Beginning respawn");
		fadeEffect.fadeDirection=1;
		fadeEffect.invertFade=false;
		fadeEffect.invertWhenFinished=true;
		fadeEffect.stopWhenFinished=false;
		fadeEffect.fadeProgress=0;
		fadeEffect.fadeTime=0.1f;
		fadeEffect.StartFading();
		respawning=true;
	}

	public void PlayerDied(){
		RespawnPlayer();
	}

	public void StartLevel(){
		levelStartTime=Time.time;
		
	}

	public void EndLevel(){
		if (levelLoader!=null) return;
		levelEndTime=Time.time;
		levelCompleteTime=levelEndTime-levelStartTime;
		if (levelCompleteTime>oneStarTime)
			starRanking=0;
		else if (levelCompleteTime>twoStarTime)
			starRanking=1;
		else if (levelCompleteTime>threeStarTime)
			starRanking=2;
		else
			starRanking=3;

		if (isProcGen){
			LevelProgress oldProcGenProgress=SaveData.GetProcGenProgress(FinalWorldGen.seed);
			if (oldProcGenProgress.timeTaken>levelCompleteTime || oldProcGenProgress.timeTaken==0){
				oldProcGenProgress.timeTaken=levelCompleteTime;
				oldProcGenProgress.starRanking=starRanking;
				SaveData.AddProcGenProgress(FinalWorldGen.seed,oldProcGenProgress);			
			}
		}

		LevelProgress oldProgress=SaveData.GetLevelProgress(currentLevelIndex);
		if (oldProgress.timeTaken>levelCompleteTime || oldProgress.timeTaken==0){
			oldProgress.timeTaken=levelCompleteTime;
			oldProgress.starRanking=starRanking;
			SaveData.SetLevelProgress(currentLevelIndex,oldProgress);			
		}

		if (nextLevelIndex>=0){
			LevelProgress nextProgress=SaveData.GetLevelProgress(nextLevelIndex);
			nextProgress.unlocked=true;
			SaveData.SetLevelProgress(nextLevelIndex,nextProgress);
		}

		SaveData.Save();

		Shotgun s=player.GetChild(0).GetChild(0).gameObject.GetComponent<Shotgun>();
		s.shouldDeploy=false;
		s.StartDeploy();
		player.gameObject.GetComponent<NuPlayer>().BeginLevelEnd();

		//Send the time taken to the server
		if (Application.isEditor||sendInEditor){
			GTTTNetwork.SendTime(levelCompleteTime,GTTTNetwork.EchoResponse);
		}

		levelComplete=true;
	}

	public void BypassedMaxY(){
		//Change speed of end-of-level clouds
	}

	public void StartLoadingLevel(int index){
		int sceneIndex=(index>=0?LevelData.GetLevel(index).sceneIndex:0);
		//Debug.Log("Loading "+LevelData.GetLevel(index).name+" "+index);
		levelLoader=SceneManager.LoadSceneAsync(sceneIndex);
		levelLoader.allowSceneActivation=false;
		loadLevelFadeStart=Time.time;

		GTTTCompleteImageEffect fadeEffect=player.gameObject.GetComponentInChildren<GTTTCompleteImageEffect>();
		fadeEffect.fadeDirection=1;
		fadeEffect.invertFade=false;
		fadeEffect.stopWhenFinished=false;
		fadeEffect.StartFading();
	}
}
