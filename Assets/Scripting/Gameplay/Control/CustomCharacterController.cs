using UnityEngine;
using System.Collections;
using System.Collections.Generic;

struct CapsuleCastIntersectData{
	public Vector3 normal;
	public Vector3 endPoint;
	public Vector3 extraDelta;
}
struct FrameCollisionData{
	public bool collision;
	public Vector3 pos,actualMove,intendedMove;
	public FrameCollisionData(bool c,Vector3 p,Vector3 am,Vector3 im){
		collision=c;
		pos=p;
		actualMove=am;
		intendedMove=im;
	}
}

public class CustomCharacterController : MonoBehaviour {
	public bool conservativeCollision=false;
	public bool capsuleCollision=true;
	public LayerMask layerMask;
	[Header("Capsule Data")]
	//public Vector3 center;
	public float radius,height,skinWidth;
	[Header("Character Data")]
	public Vector3 velocity;
	public Vector3 collisionDirection;
	public List<Vector3> collisionDirections;
	public List<Collider> collisionColliders,toIgnore;
	public List<RaycastHit> collisionHits;

	[Header("Debug Data")]
	public Vector3 debugMove,tickDelta,endTickPos;

	public Vector3 capsuleStart,capsuleEnd;
	public Vector3 cylinderStart,cylinderEnd;

	FrameCollisionData fourAgo,threeAgo,twoAgo,oneAgo,now;

	Collider ourCollider;

	public virtual void Start(){
		capsuleStart=Vector3.up*(height/2-radius); //If we were using a capsule it would be up*height/2-radius
		capsuleEnd=Vector3.down*(height/2-radius);
		cylinderStart=Vector3.up*(height/2); //If we were using a capsule it would be up*height/2-radius
		cylinderEnd=Vector3.down*(height/2);
		//Debug.Log(capsuleStart+","+capsuleEnd);
		if (capsuleCollision){
			ourCollider=gameObject.AddComponent<CapsuleCollider>();
			((CapsuleCollider)ourCollider).radius=radius;
			((CapsuleCollider)ourCollider).height=height;
		}else{
			ourCollider=gameObject.AddComponent<BoxCollider>();
			((BoxCollider)ourCollider).center=Vector3.zero;
			((BoxCollider)ourCollider).extents=new Vector3(radius,height/2,radius);
		}
			ourCollider.enabled=false;

		//if(minMovement<=0f)
		//	minMovement=0.05f;
	}

	void OnEnable(){
		collisionDirections=new List<Vector3>();
		collisionColliders=new List<Collider>();
		collisionHits=new List<RaycastHit>();
	}

	public float SmallestDistanceFromCylinder(Vector3 p){
		p.y=Mathf.Clamp(p.y,(transform.position+cylinderEnd).y,(transform.position+cylinderStart).y);

		Vector3 closestPointOnLine=ClosestPointOnSelf(p,true);
		//Clamp p.y to our start and end heights
		return (closestPointOnLine-p).magnitude;
	}

	public float SmallestDistanceFromCapsule(Vector3 p){
		Vector3 closestPointOnLine=ClosestPointOnSelf(p);
		return (p-closestPointOnLine).magnitude;
	}

	public Vector3 ClosestPointOnSelf(Vector3 p,bool cylinder=false){
		Vector3 v=capsuleStart+transform.position,w=capsuleEnd+transform.position;
		if (cylinder){
			v=cylinderStart+transform.position;
			w=cylinderEnd+transform.position;
		}
		//Debug.Log(v+","+w);
		
		float l2=height*height;
		float t=Mathf.Clamp01(Vector3.Dot(p-v,w-v)/l2);
		Vector3 closestPointOnLine=v+t*(w-v);
		return closestPointOnLine;
	}

	Vector3 ClosestPointOnLine(Vector3 v,Vector3 w,Vector3 p){
		float l2=Mathf.Pow((v-w).magnitude,2);
		float t=Mathf.Clamp01(Vector3.Dot(p-v,w-v)/l2);
		Vector3 closestPointOnLine=v+t*(w-v);
		return closestPointOnLine;
	}

	public virtual void Move(Vector3 moveDelta){
		if (Time.deltaTime<=0) return;		
		Vector3 originalPos=transform.position;
		Vector3 origMoveDelta=moveDelta;
		collisionDirections=new List<Vector3>();
		collisionColliders=new List<Collider>();
		collisionHits=new List<RaycastHit>();
		
		Vector3 collBoxDim=new Vector3(radius,height/2,radius);

		Collider[] isect;
		Vector3 actualDelta=Vector3.zero;
		bool collision=false;
		Vector3 preMovePos;
		while (moveDelta.magnitude>0){
			preMovePos=transform.position;
			transform.position+=moveDelta.normalized*Mathf.Min(moveDelta.magnitude,radius);
			collision = collision || ResolveCollision();
			if ((preMovePos-transform.position).magnitude<0.001f) break; //If we can't move then stop trying (v. defeatist)
			moveDelta=moveDelta.normalized*Mathf.Max(0,moveDelta.magnitude-radius);
		}
		collision=collision||ResolveCollision();
		velocity=transform.position-originalPos;
		//now=new FrameCollisionData(collision,transform.position,velocity/Time.deltaTime,origMoveDelta/Time.deltaTime);
		
		Vector3 changeInVelocity=(velocity-origMoveDelta);
		changeInVelocity.y=0;
		velocity/=Time.deltaTime;
		endTickPos=transform.position+Vector3.one;

		//fourAgo=threeAgo;
		//threeAgo=twoAgo;
		//twoAgo=oneAgo;
		//oneAgo=now;

	}

	bool ResolveCollision(){
		

		//With a capsule the player can get caught on ledges. We really want a cylinder, which we can approximate with a cube overlap and some special testing
		Collider[] isect;
		if (capsuleCollision)
			isect=Physics.OverlapCapsule(capsuleStart+transform.position,capsuleEnd+transform.position,radius);
		else
			isect=Physics.OverlapBox(transform.position,new Vector3(radius,height/2,radius),transform.rotation);


		//isect=Physics.OverlapBox(transform.position,collBoxDim);
		//velocity=Vector3.zero;
		Vector3 direction=Vector3.zero;
		float distance=0,smallestDist;
		Vector3 collPoint;
		bool hadCollision=false;
		foreach (Collider c in isect){
			if (c==ourCollider) continue;
			if (!c.enabled) continue;
			if (c.isTrigger) continue;
			//if (!(c is BoxCollider)) continue;
			if (toIgnore.IndexOf(c)!=-1) continue;
			if (layerMask != (layerMask | (1 << c.gameObject.layer))) continue;

			//Make sure we're actually touching the object
			//collPoint=Physics.ClosestPoint(transform.position,c,c.transform.position,c.transform.rotation);//SuperCollisions.ClosestPointInBounds(c,transform.position);
			//smallestDist=SmallestDistanceFromCylinder(collPoint);
			//if (smallestDist>radius){ 
				//Debug.Log("Collider was excluded, not within cylinder");
			//	continue;
			//}

			ourCollider.enabled=true;
			//Debug.Log(c);
			if (Physics.ComputePenetration(c          , c.transform.position, c.transform.rotation,
										   ourCollider,   transform.position, transform.rotation,  //Use the same y-rotation to make sure the box is aligned with the plane
										   out direction, out distance)){
				//velocity-=direction*distance;
				
				transform.position-=direction*distance;
				if (collisionColliders.IndexOf(c)==-1){
					collPoint=Physics.ClosestPoint(transform.position,c,c.transform.position,c.transform.rotation);
					if ((collPoint-transform.position).magnitude<=0.0001){
						Debug.Log("Penetration wasn't resolved for "+c);
						continue;
					}
					collisionDirections.Add(direction);
					collisionColliders.Add(c);
					///FIXMEIEMEIEIEME
				RaycastHit hit;//=new RaycastHit();
				//Cast from the closest point on our line to the collPoint
					Ray r=new Ray(transform.position,(collPoint-transform.position).normalized);
					c.Raycast(r,out hit,(collPoint-transform.position).magnitude*1.1f);
					collisionHits.Add(hit);
					hadCollision=true;
				}
			}else{
				//Debug.Log("False Negative");
			}
			ourCollider.enabled=false;
		}
		return hadCollision;
	}

	Vector3 MaxDirectionToDimensions(Vector3 toMax){
		if (toMax.x<-0.01f || toMax.x>0.01f)
			toMax.x=Mathf.Sign(toMax.x)*radius;
		if (toMax.y<-0.01f || toMax.y>0.01f)
			toMax.y=Mathf.Sign(toMax.y)*height/2;
		if (toMax.z<-0.01f || toMax.z>0.01f)
			toMax.z=Mathf.Sign(toMax.z)*radius;
		return toMax;
	}

	Vector3 MaxDirectionToCardinal(Vector3 toMax){
		Vector3 absVec=new Vector3(Mathf.Abs(toMax.x)/radius,Mathf.Abs(toMax.y)/(height/2),Mathf.Abs(toMax.z)/radius);//(Mathf.Abs(toMax.x),Mathf.Abs(toMax.y),Mathf.Abs(toMax.z));
		Vector3 toReturn=Vector3.zero;
		/*if (absVec.x>absVec.y){
			if (absVec.x>absVec.z)
				return Vector3.right*Mathf.Sign(toMax.x);
			return Vector3.forward*Mathf.Sign(toMax.z);
		}else{
			if (absVec.y>absVec.z)
				return Vector3.up*Mathf.Sign(toMax.y);
			return Vector3.forward*Mathf.Sign(toMax.z);
		}*/
		if (absVec.y>absVec.x&&absVec.y>absVec.z)
			return Vector3.up*Mathf.Sign(toMax.y);
		return toMax+Vector3.down*toMax.y;
		/*if (absVec.x>0.1)
			toReturn.x=toMax.x;
		if (absVec.y>0.1)
			toReturn.y=toMax.y;
		if (absVec.z>0.1)
			toReturn.z=toMax.z;
		return toReturn;*/
	}

	/*Vector3 MaxComponent(Vector3 one,Vector3 two){
		//Make two a cardinal direction 
		if(Mathf.Abs(two.x)>Mathf.Abs(two.z)){
			if (Mathf.Abs(two.x)>Mathf.Abs(two.y)){
				two.x=(two.x>0?1:-1);
				two.y=0;
			}else{
				two.y=(two.y>0?1:-1);
				two.x=0;
			}
		}else{
			if (Mathf.Abs(two.z)>Mathf.Abs(two.y)){
				two.z=(two.z>0?1:-1);
				two.y=0;
			}else{
				two.z=(two.z>0?1:-1);
				two.x=0;
			}
		}

		if (Mathf.Abs(one.x)<Mathf.Abs(two.x))
			one.x=two.x;
		if (Mathf.Abs(one.y)<Mathf.Abs(two.y))
			one.y=two.y;
		if (Mathf.Abs(one.z)<Mathf.Abs(two.z))
			one.z=two.x;
		return one;
	}*/

	void Update(){
		//if (debugMove.magnitude!=0)
		//	Move(debugMove*Time.deltaTime);
	}

	public virtual void IgnoreCollision(Collider c,bool ignore=true){
		int index=toIgnore.IndexOf(c);
		if(index==-1 && ignore)
			toIgnore.Add(c);
		else if (index>=0 && !ignore)
			toIgnore.RemoveAt(index);
	}

	/*public bool CapsuleTest(Vector3 start,Vector3 delta,out RaycastHit h,float length=1){
		int sphereCount=(int)((height/radius)/2) +1;
		Vector3 spherePos=start+delta.normalized*length-Vector3.up*(height/2-radius);
		//Debug.Log(spherePos);
		Collider[] colliders;
		for(int i=0;i<sphereCount;i++){
			colliders=Physics.OverlapSphere(spherePos,radius);
			foreach(Collider c in colliders){
				if (toIgnore.IndexOf(c)>=0) continue;
				//h.collider=colliders[0];
				Ray r=new Ray(transform.position,(c.ClosestPointOnBounds(transform.position)-transform.position).normalized);
				if (r.direction.magnitude<1) continue;
				if(!c.Raycast(r,out h,10f)) continue;
				//}else h=new RaycastHit();
				//collNormal=h.normal;
				return true;
			}
			spherePos.y+=radius;
		}
		h=new RaycastHit();
		return false;
	}

	public bool SphereTest(Vector3 pos,float radius){
		Collider[] colliders=Physics.OverlapSphere(pos,radius);
		foreach(Collider c in colliders){
			if (toIgnore.IndexOf(c)>=0) continue;
			//h.collider=colliders[0];
			//Physics.Raycast(transform.position,c.ClosestPointOnBounds(transform.position)-transform.position,out h);
			//collNormal=h.normal;
			return true;
		}
		//h=new RaycastHit();
		return false;
	}*/

	public bool LineTest(Vector3 start,Vector3 end){
		Vector3 p=ClosestPointOnLine(start,end,transform.position);
		if (SmallestDistanceFromCapsule(p)<=radius) return true;
		return false;
	}


}
