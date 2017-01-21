using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TargetHandler))]
public class TargetWakeTrigger : PlayerTrigger {
	public TargetHandler[] requiredTargets;
	//bool activated=false;

	// Use this for initialization
	public override void OnPlayerStay (Transform player) {
		if (GetComponent<TargetHandler>().shouldBeActivated||GetComponent<TargetHandler>().activateStart!=-1) return;
		foreach(TargetHandler t in requiredTargets){
			if (!t.hasBeenShot) return;
		}
		GetComponent<TargetHandler>().Activate();
		//activated=true;
	}

	public override Color GetGizmoColor(){ return Color.yellow; }
}
