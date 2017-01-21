using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LevelCheckpoint))]
public class StartLevelTrigger : TubeTrigger {
	bool hasStarted=false;

	public override void OnPlayerEnter(Transform player){
		if (hasStarted) return;
		LevelHandler.currentLevel.StartLevel();
		startOpenTime=Time.time;
	}

	public override void OnPlayerStay(Transform player){}

	public override void OnPlayerLeave(Transform player){
		if (hasStarted) return;
		GetComponent<CapsuleCollider>().enabled=true;
		BoxCollider[] bcArray=GetComponents<BoxCollider>();
		foreach(BoxCollider bc in bcArray){
			bc.enabled=false;
		}
		if (LevelHandler.currentLevel.startWithShotgun)
			player.GetChild(0).GetChild(0).gameObject.SetActive(true);
		//Close the doors, killself
		startCloseTime=Time.time;
		hasStarted=true;
	}
}
