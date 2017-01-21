using UnityEngine;
using System.Collections;

public class EndLevelTrigger : PlayerTrigger {
	public override void OnPlayerEnter(Transform player){
		Debug.Log("End level trigger fired");
		LevelHandler.currentLevel.EndLevel();
		//Open the UI
		EndLevelUIHandler eluh=GetComponentInChildren<EndLevelUIHandler>();
		eluh.OpenUI();
	}
}
