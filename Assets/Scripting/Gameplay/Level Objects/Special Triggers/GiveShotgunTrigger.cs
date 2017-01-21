using UnityEngine;
using System.Collections;

public class GiveShotgunTrigger : PlayerTrigger {
	public TargetHandler[] targetsToActivate;

	public override void OnPlayerEnter(Transform player){
		Debug.Log("Giving Shotgun");
		player.GetChild(0).GetChild(0).gameObject.SetActive(true);
		Debug.Log("Removing Shotgun from stand");
		transform.Find("ShotgunWorldmodel").gameObject.SetActive(false);
		foreach(TargetHandler t in targetsToActivate){
			t.Activate();
		}
		Debug.Log("/killself");
		GameObject.Destroy(this);
	}
}
