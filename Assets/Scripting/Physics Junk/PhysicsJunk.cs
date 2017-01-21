using UnityEngine;
using System.Collections;

public class PhysicsJunk : MonoBehaviour {
	public Vector3 velocity,torque,gravity=Vector3.down*10f;
	public float yOffsetOnFloor;
	//public Vector3 bboxCentre,bboxExtents;
	public bool active=true;
	public AudioClip[] bounceClips;
	public AudioClip finishBouncingClip;
	RaycastHit h;
	Quaternion targetRot;
	AudioSource audioSource;
	public bool audioEnabled=true;
	public float volume=1f,maxDistanceFromCameraForVolume=10f;
	public Vector3 startCapsule,endCapsule;
	public float radius;
	CapsuleCollider betterCollision;

	// Use this for initialization
	void Start () {
		if(bounceClips.Length>0&&audioEnabled){
			audioSource=GetComponent<AudioSource>();
			if(audioSource == null) audioSource=gameObject.AddComponent<AudioSource>();
			audioSource.priority=200;
			audioSource.maxDistance=10;
		}
		betterCollision=GetComponent<CapsuleCollider>();
		if (betterCollision!=null)
			betterCollision.enabled=false;
	}
	
	// Update is called once per frame
	void Update () {
		if(active&&Time.timeScale!=0){
			velocity+=gravity*Time.deltaTime;
			/*if (Physics.CapsuleCast(transform.TransformPoint(startCapsule),transform.TransformPoint(endCapsule),radius,velocity.normalized,out h,velocity.magnitude*Time.deltaTime)){
				if(Vector3.Dot(h.normal,Vector3.up)>0.9f&&velocity.magnitude<1f){
					transform.position=h.point+Vector3.up*yOffsetOnFloor;
					
					float oldGlobalY=transform.eulerAngles.y;
					Vector3 newRotation=Vector3.zero;
					if(transform.eulerAngles.z>180) newRotation.z=-90;
					else newRotation.z=90;
					newRotation.y=oldGlobalY;
					transform.localEulerAngles=newRotation;
					active=false;
					if(audioEnabled)
						audioSource.PlayOneShot(finishBouncingClip);
				}else{
					//Bounce off the object by reflecting velocity in the normal and dampening
					velocity=Vector3.Reflect(velocity,h.normal)*0.5f;
					torque/=2;
					if(audioEnabled)
						audioSource.PlayOneShot(bounceClips[Random.Range(0,bounceClips.Length)]);
				}
				
			}*/
			if(active){ 
				Vector3 oldPos=transform.position;
				transform.position+=velocity*Time.deltaTime;
				transform.Rotate(torque*Time.timeScale);
				betterCollision.enabled=true;
				Collider[] overlap=Physics.OverlapCapsule(transform.TransformPoint(betterCollision.center+Vector3.up*(betterCollision.height-betterCollision.radius)),transform.TransformPoint(betterCollision.center-Vector3.up*(betterCollision.height-betterCollision.radius)),betterCollision.radius);
				foreach(Collider c in overlap){
					if (c==betterCollision) continue;
					Vector3 moveDir;
					float moveDist;
					if (Physics.ComputePenetration(betterCollision,transform.position,transform.rotation,
												   c,c.transform.position,c.transform.rotation,out moveDir,out moveDist)){
						//Bounce off the object by reflecting velocity in the normal and dampening
						RaycastHit h;
						if (!Physics.Raycast(transform.position,Physics.ClosestPoint(transform.position,c,c.transform.position,c.transform.rotation)-transform.position,out h)) break;

						transform.position+=moveDir*moveDist;
						/**/
						if(Vector3.Dot(h.normal,Vector3.up)>0.9f&&velocity.magnitude<0.5f){
							//transform.position=h.point+Vector3.up*yOffsetOnFloor;
					
							float oldGlobalY=transform.eulerAngles.y;
							Vector3 newRotation=Vector3.zero;
							/*if(transform.eulerAngles.z>180&&transform.eulerAngles.z<=270) newRotation.z=-90;
							else if (transform.eulerAngles.z>270) newRotation.z=0;
							else newRotation.z=90;*/
							float eulerZ=transform.eulerAngles.z;
							if (eulerZ<45||eulerZ>=315) eulerZ=0;
							else if (eulerZ>=45 && eulerZ<135) eulerZ=90;
							else if (eulerZ>=135 && eulerZ<225) eulerZ=180;
							else eulerZ=270;
							newRotation.z=eulerZ;
							newRotation.y=oldGlobalY;
							transform.localEulerAngles=newRotation;
							active=false;
							Debug.Log("Deactivating");
							if(audioEnabled)
								audioSource.PlayOneShot(finishBouncingClip);
						}else{
							//velocity=(transform.position-oldPos)/Time.deltaTime;
							velocity=Vector3.Reflect(velocity,h.normal)*0.5f;
							torque/=2;
							if(audioEnabled)
								audioSource.PlayOneShot(bounceClips[Random.Range(0,bounceClips.Length)]);
						}
					}
				}
				betterCollision.enabled=false;
			}
						
			//volMod is inversely proportional to distance from camera
			if(audioEnabled){
				float distanceMod=Mathf.Clamp01((maxDistanceFromCameraForVolume-(transform.position-Camera.main.transform.position).magnitude)/maxDistanceFromCameraForVolume);
				audioSource.volume=Mathf.Clamp01(velocity.magnitude/5f)*volume*distanceMod;
			}
		}
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.black;
		Gizmos.matrix=transform.localToWorldMatrix;
		Gizmos.DrawWireSphere(startCapsule,radius);
		Gizmos.DrawWireSphere(endCapsule,radius);
	}

	/*void Update(){
		if (!active) return;
		velocity+=gravity*Time.deltaTime;
		bool collisionStatus=Physics.BoxCast(transform.position+bboxCentre,bboxExtents/2,velocity.normalized,out h,transform.rotation,velocity.magnitude*Time.deltaTime);
		if (!collisionStatus) collisionStatus=Physics.Raycast(transform.position+bboxCentre,velocity.normalized,out h,velocity.magnitude*Time.deltaTime);
		if(collisionStatus&&h.transform!=transform){
			Debug.Log("Collided with "+h.transform);
			transform.position=h.point+Vector3.up*bboxExtents.y/2;
			float oldGlobalY=transform.eulerAngles.y;
			float newZ=90;
			if(transform.eulerAngles.z>180) newZ=-90;
			transform.localEulerAngles=Vector3.forward*newZ;
			transform.Rotate(Vector3.up*oldGlobalY,Space.World);
			if(Vector3.Dot(h.normal,Vector3.up)>0.9f&&velocity.magnitude<1f){
				active=false;
				return;
			}else{
				velocity=Vector3.Reflect(velocity,h.normal)*0.5f;
			}
		}
		transform.position+=velocity*Time.deltaTime;
		transform.Rotate(torque);
	}*/
}
