using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Force{
	public Vector3 value;
	public bool isVelocityChange=false,contributesToMomentum=true;
	public Force(){
		value=Vector3.zero;
	}
	public Force(float amount, Vector3 direction){
		value=amount*direction;
	}
	public Force(Vector3 direction, float amount){
		value=amount*direction;
	}
	public Force(Vector3 value_){
		value=value_;
	}
	public Force(Vector3 value_, bool velocityChange){
		isVelocityChange=velocityChange;
		value=value_;
	}
	public Force(Vector3 value_, bool velocityChange,bool addToMomentum){
		isVelocityChange=velocityChange;
		value=value_;
		contributesToMomentum=addToMomentum;
	}
	public virtual Vector3 Apply(Vector3 baseVelocity,float deltaTime){
		if(!isVelocityChange) return baseVelocity+value*deltaTime;
		return baseVelocity+value;
	}
	public Vector3 Apply(Vector3 baseVelocity,float deltaTime,float mass){
		if(!isVelocityChange) return baseVelocity+value*deltaTime/mass;
		return baseVelocity+value/mass;
	}
}
[RequireComponent(typeof(CharacterController))]
public class ForceCharacterController : MonoBehaviour {
	public Vector3 momentum;
	public List<Force> forces;
	CharacterController c;
	public bool isGrounded;
	public float radius,height,skinWidth;
	Vector3 cachedCollisionDir=2*Vector3.one;

	Vector3 GetMomentum(){
		/*Vector3 newMomentum=momentum;
		if(c.isGrounded&&newMomentum.y<0)
			newMomentum.y=0;
			//forces.Add(new Force(-momentum.y*Vector3.up,true,false));
		else if((c.collisionFlags&CollisionFlags.Above)!=0&&newMomentum.y>0)
			newMomentum.y=0;
			//forces.Add(new Force(-momentum.y*Vector3.up,true,false));
		if((c.collisionFlags&CollisionFlags.Sides)!=0){
			newMomentum.x=0;
			//if (CheckSideHit()*momentum.x>0)
			//	forces.Add(new Force(-momentum.x*Vector3.right,true,false));
		}
		//Debug.Log(momentum-SumForces().value*Time.deltaTime);
		return newMomentum;//+SumForces().value*Time.deltaTime;*/
		//Vector3 collDir=GetCollisionDirection();
		//If collDir is Vector3.down
		//Vector3.Dot(collDir,momentum)=-momentum.y;
		//
		//AddForce(new Force(-collDir.normalized*Vector3.Dot(collDir,momentum),true,false));
		return momentum;//+SumForcesWithoutMomentum().value*Time.deltaTime;
	}

	void ApplyCollisionForces(){
		Vector3 collDir=GetCollisionDirection();
		Vector3 forceValue=Vector3.zero;
		if(collDir.x*momentum.x>0)
			forceValue.x=-momentum.x;
		if(collDir.y*momentum.y>0)
			forceValue.y=-momentum.y;
		if(collDir.z*momentum.z>0)
			forceValue.z=-momentum.z;
		momentum+=forceValue;
		//forces.Add(new Force(forceValue,true));
	}

	public Vector3 GetCollisionDirection(){
		if(cachedCollisionDir.magnitude<2)
			return cachedCollisionDir;
		//Scaled down by 4/5ths
		Vector3 collDir=Vector3.zero;
		Vector3 capsuleStart=transform.position+transform.up*(c.height-2*c.radius)/2;
		Vector3 capsuleEnd=transform.position-transform.up*(c.height-2*c.radius)/2;
		for(int i=0;i<6;i++){
			Vector3 testDir=transform.right;
			if (i==1) testDir=-transform.right;
			if (i==2) testDir=transform.up;
			if (i==3) testDir=-transform.up;
			if (i==4) testDir=transform.forward;
			if (i==5) testDir=-transform.forward;
			/*Vector3 furthestPoint=transform.position+testDir*Mathf.Abs(Vector3.Dot(dimVec,testDir));//c.ClosestPointOnBounds(transform.position+testDir*(Mathf.Abs(Vector3.Dot(dimVec,testDir))))+testDir*Mathf.Abs(Vector3.Dot(dimVec,testDir))*0.2f;
			Ray r=new Ray(furthestPoint,testDir);
			Debug.DrawLine(furthestPoint,furthestPoint+testDir*0.04f,Color.white,0f,false);*/
			if (Physics.CapsuleCast(capsuleStart,capsuleEnd,c.radius,testDir,c.skinWidth*2)){
				//testDir=transform.InverseTransformDirection(testDir);
				if(collDir.x==0)
					collDir.x=testDir.x;
				if(collDir.y==0)
					collDir.y=testDir.y;
				if(collDir.z==0)
					collDir.z=testDir.z;
			}	
		}
		cachedCollisionDir=collDir;
		return collDir;
	}

	// Use this for initialization
	public virtual void Start () {
		c=GetComponent<CharacterController>();
		radius=c.radius;
		height=c.height;
		skinWidth=c.skinWidth;
		forces=new List<Force>();
	}
	
	// Update is called once per frame
	public void UpdatePosition () {
		Vector3 velocity=momentum;
		foreach(Force f in forces){
			velocity=f.Apply(velocity,Time.deltaTime);
			if(f.contributesToMomentum)
				momentum=f.Apply(momentum,Time.deltaTime);
		}
		forces=new List<Force>();
		//Debug.Log()
		c.Move(velocity*Time.deltaTime);
		cachedCollisionDir=2*Vector3.one;
		ApplyCollisionForces();
		momentum=GetMomentum();
		isGrounded=(GetCollisionDirection().y<0);
	}

	public void AddForce(Force f){
		forces.Add(f);
	}

	public Force SumForces(){
		Vector3 v=momentum;
		foreach(Force f in forces){
			v=f.Apply(v,Time.deltaTime);
		}
		return new Force (v/Time.deltaTime);
	}

	public Force SumForcesWithoutMomentum(){
		Vector3 v=Vector3.zero;
		foreach(Force f in forces){
			v=f.Apply(v,Time.deltaTime);
		}
		return new Force (v/Time.deltaTime);
	}

	public Vector3 GetProjectedPosition(){
		Vector3 currentAcceleration=SumForces().value;
		return transform.position+currentAcceleration*Time.deltaTime*Time.deltaTime;
	}

	public Vector3 ClosestPointOnBounds(Vector3 target){
		return c.ClosestPointOnBounds(target);
	}

	float CheckSideHit(){
		for(int i=0;i<2;i++){
			float dirMultiplier=-1f;
			if (i==1) dirMultiplier=1f;
			Vector3 furthestPoint=c.ClosestPointOnBounds(transform.position+Vector3.right*2*(c.radius+c.skinWidth)*dirMultiplier)+Vector3.right*dirMultiplier*c.skinWidth;
			Ray r=new Ray(furthestPoint,Vector3.right*dirMultiplier);
			Debug.DrawLine(furthestPoint,furthestPoint+Vector3.right*dirMultiplier*c.skinWidth*2);
			if (Physics.Raycast(r,c.skinWidth*2))
				return dirMultiplier;
		}
		return 0f;
	}

	void OnControllerColliderHit(ControllerColliderHit hit){
	//	forces.Add(new Force(hit.normal*-Vector3.Dot(hit.normal,momentum)));
	}
}
