using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetHandler : MonoBehaviour {
	public float activateTime,initialRot,finalRot;
	public bool hasBeenShot=false;
	public float activateStart=-1;
	public bool shouldBeActivated=false;
	 Collider playerColl;

	
	public void Activate(){
		activateStart=Time.time;
		shouldBeActivated=true;
	}

	public void Deactivate(){
		if (!shouldBeActivated) return;
		activateStart=Time.time;
		shouldBeActivated=false;
	}

	public void GetShot(){
		Deactivate();
		hasBeenShot=true;
	}

	void Start(){
		transform.GetChild(0).localEulerAngles=Vector3.right*initialRot+Vector3.up*180;
		shouldBeActivated=false;
		//playerColl=GetComponent<Collider>();
		//playerColl.enabled=false;
	}
	
	void Update(){
		if (activateStart!=-1){
			float p=(Time.time-activateStart)/activateTime;
			if (shouldBeActivated)
				transform.GetChild(0).localEulerAngles=Vector3.right*Mathf.Lerp(initialRot,finalRot,Mathf.Clamp01(p))+Vector3.up*180;
			else
				transform.GetChild(0).localEulerAngles=Vector3.right*Mathf.Lerp(finalRot,initialRot,Mathf.Clamp01(p))+Vector3.up*180;
			if (p>=1){
				activateStart=-1;
				//playerColl.enabled=shouldBeActivated;
			}
		}
	}
}
