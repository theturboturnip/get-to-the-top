using UnityEngine;
using System.Collections;

public class EnemyControl : MonoBehaviour {
	public GameObject player;
	public Transform gun;
	public float rotateVelocity,fireCooldown,findRadius,maxForwardY=1,similarLookFireThreshold=0.9f;
	public Gun[] guns;
	float[] gunTimeSinceFire;
	float timeSinceFire=0f;
	public float health=1;
	bool shouldDie=false ;

	int mode=0; //0=idling, 1=found player

	void Start(){
		if (guns==null)
			guns=GetComponentsInChildren<Gun>();
		gunTimeSinceFire=new float[guns.Length];
		float gunTimeOffset=fireCooldown/guns.Length;
		for(int i=0;i<guns.Length;i++){
			gunTimeSinceFire[i]=-i*gunTimeOffset;
		}
		if (player==null){
			player=GameObject.FindWithTag("Player");
		}
	}

	public bool TakeDamage(float damage){
		Debug.Log("Taking "+damage+" damage!");
		health-=damage;
		shouldDie=(health<=0);
		return shouldDie;
	}

	int FindMode(){
		if (player==null)
			return 0; //Player doesn't exist
		Vector3 playerDelta=(player.transform.position-transform.position);
		if (playerDelta.sqrMagnitude>findRadius*findRadius)
			return 0; //Can't see player
		if (shouldDie)
			return 2;
		return 1;
		
	}

	Vector3 GetTargetLook(){
		if (mode==1)
			return (player.transform.position-transform.position).normalized;
		return SetY(gun.forward).normalized;
	}

	void Update(){
		mode=FindMode();

		Vector3 targetLook=GetTargetLook();
		targetLook.y=Mathf.Min(maxForwardY,targetLook.y);
		Vector3 oldLook=gun.forward;

		//if (Vector3.Dot(playerDir,oldForward)<0.9999f){
			Vector3 forward=Vector3.RotateTowards(oldLook,targetLook,rotateVelocity*Time.deltaTime,0);
			forward.y=Mathf.Min(maxForwardY,forward.y);
			transform.forward=SetY(forward).normalized;
			gun.forward=forward;
		//}

		if (mode==2)
			gun.Rotate(Vector3.forward*20f);

		if (mode!=1) return; //No player, don't shoot
		if (Vector3.Dot(gun.forward,targetLook)<similarLookFireThreshold) return; //Don't have clear shot
		Gun g;
		for(int i=0;i<guns.Length;i++){
			g=guns[i];
			gunTimeSinceFire[i]+=Time.deltaTime;
			if (gunTimeSinceFire[i]>fireCooldown){
				gunTimeSinceFire[i]=0;
				g.Fire();
			}
		}
	}

	Vector3 SetY(Vector3 v,float y=0){
		v.y=y;
		return v;
	}
}
