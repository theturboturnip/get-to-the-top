using UnityEngine;
using System.Collections;

public class TubeTrigger : PlayerTrigger {
	public Transform leftBone,rightBone;
	public float rotateTarget,doorOpenTime;

	protected float startOpenTime=-1,startCloseTime=-1;
	protected bool doorsOpen=false;

	public override void Update(){
		base.Update();
		if (startOpenTime!=-1&&!doorsOpen){
			//float rot=(rotateTarget/doorOpenTime)*Time.deltaTime; //Extra rotation in degrees
			float finalRot=Mathf.Lerp(0,rotateTarget,(Time.time-startOpenTime)/doorOpenTime);
			leftBone.localRotation=Quaternion.Euler(Vector3.forward*(-finalRot+90)+Vector3.up*-90);
			rightBone.localRotation=Quaternion.Euler(Vector3.forward*(finalRot-90)+Vector3.up*-90);

			if (Time.time-startOpenTime>doorOpenTime){
				doorsOpen=true;
			}
		}

		if (startCloseTime!=-1 && doorsOpen){
		//	float rot=(rotateTarget/doorOpenTime)*Time.deltaTime; //Extra rotation in degrees
			float finalRot=Mathf.Lerp(rotateTarget,0,(Time.time-startCloseTime)/doorOpenTime);
			leftBone.localRotation=Quaternion.Euler(Vector3.forward*(-finalRot+90)+Vector3.up*-90);
			rightBone.localRotation=Quaternion.Euler(Vector3.forward*(finalRot-90)+Vector3.up*-90);

			if (Time.time-startCloseTime>doorOpenTime){
				doorsOpen=false;
				//Debug.Log("/killself");
				//GameObject.Destroy(this);
				//this.enabled=false;
			}
		}
	}
}
