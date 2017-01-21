using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TargetHandler))]
public class TargetCloseTrigger : PlayerTrigger {
	public override void OnPlayerEnter(Transform player){
		TargetHandler th=GetComponent<TargetHandler>();
		if (th.shouldBeActivated)
			th.Deactivate();
	}

	public override Color GetGizmoColor(){ return Color.yellow; }
	
}
