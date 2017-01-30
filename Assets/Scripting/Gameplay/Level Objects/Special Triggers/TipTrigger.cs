using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TipTrigger : PlayerTrigger {
	public string tipMessage;
	public bool oneShot=false;
	public bool requireShotgun=false;
	public bool hasOpened=false;


	// Use this for initialization
	public override void OnPlayerStay (Transform player) {
		if (hasOpened) return;
		if (requireShotgun&&GameObject.FindWithTag("Shotgun")==null) return;
		TipHandler.current.OpenTip(tipMessage);
		hasOpened=true;
	}
	
	public override void OnPlayerLeave(Transform player){
		TipHandler.current.CloseTip(tipMessage);
		this.enabled=!oneShot;
		hasOpened=false;
	}

	public override Color GetGizmoColor(){ return Color.red; }
}
