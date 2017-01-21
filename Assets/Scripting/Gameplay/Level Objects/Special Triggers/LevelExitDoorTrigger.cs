using UnityEngine;
using System.Collections;

public class LevelExitDoorTrigger : TubeTrigger {
	public override void OnPlayerEnter(Transform player){
		GetComponent<CapsuleCollider>().enabled=false;
		GetComponent<BoxCollider>().enabled=true;
	}

	public override void OnPlayerStay(Transform p){
		if (!doorsOpen&&startOpenTime==-1){
			startOpenTime=Time.time;
			startCloseTime=-1;
		}
	}

	public override void OnPlayerStayOut(Transform player){
		if (doorsOpen&&startCloseTime==-1){
			startOpenTime=-1;
			startCloseTime=Time.time;
		}
		GetComponent<CapsuleCollider>().enabled=true;
		GetComponent<BoxCollider>().enabled=false;
	}
}
