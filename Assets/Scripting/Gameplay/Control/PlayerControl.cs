using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using UnityEngine.EventSystems;


[RequireComponent(typeof(CharacterController))]
public class PlayerControl : MonoBehaviour {
	[HideInInspector]
	public CharacterController c;
	public Vector2 mouseLookSensitivity;
	public float lookSmoothing;
	[Header("Physics")]
	public float gravity=10f;
	public float friction;
	public float wallFriction;
	public float maxAcceleration;
	public float maxVelocity;
	[Header("Movement")]
	public float moveSpeed=0.5f;
	public float timeToFullSpeed=1f;
	public float airTiltAmount=0.4f;
	public float maxAirTiltVelocity;
	public float jumpMagnitude=5f;
	public float jumpEnergy=1f;
	public float jumpEnergyIncreaseSpeed=1f;
	public float jumpEnergyPercentOnJump=0.9f;
	[Header("Wallrunning")]
	public float wallJumpYAmount;
	public float wallJumpMagnitude;
	public float wallRunZRot;
	public float wallStickiness;
	public float baseWallRunSpeed;
	public float forwardMultiplier;
	[Header("Misc")]
	public float zRotateSpeed;
	public float collisionDetectionRayLength=0.2f;
	public Vector3 momentum;
	public Vector3 velocity;
	public LayerMask collisionLayers;
	public Camera playerCamera;
	public Vector3 cachedCollisionDir;
	float xRot=0,yRot=0,targetZRot=0,zRot,actualMoveSpeed,runTime; //runTime is how long a movement input has been held
	Transform neck;
	Vector3 wallNormal,localInput,globalInput,collisionDir;
	Vector3 wallDirection,localCollisionDir,wallrunStart;
	bool wallrunning=false,recalculateCollision=true;
	float wallRunDistance;

	//int bulletsLeft;

	// Use this for initialization
	void Start () {
		c=GetComponent<CharacterController>();
		playerCamera=GetComponentInChildren<Camera>();
		Cursor.lockState=CursorLockMode.Locked;
		xRot=transform.localEulerAngles.y;
		zRot=playerCamera.transform.localEulerAngles.z;
		neck=transform.GetChild(0);
		wallRunDistance=1.5f*(c.skinWidth+c.radius);
	}
	
	// Update is called once per frame
	void Update () {
		momentum=ApplyCollisionForces(velocity);
		velocity=Vector3.down*gravity*Time.deltaTime+momentum;
		collisionDir=GetCollisionDirection();
		localCollisionDir=transform.InverseTransformDirection(collisionDir);
		localInput=new Vector3(Input.GetAxisRaw("Horizontal"),0,Input.GetAxisRaw("Vertical"));
		globalInput=transform.TransformDirection(localInput.normalized);

		HandleMouseLook();		
		HandleMovementVelocity();
		

		/*#if UNITY_EDITOR
		if(Input.GetKeyDown(KeyCode.P))
			EditorApplication.isPaused=true;
		#endif*/
	}

	void LateUpdate(){
		Vector3 velIncrease=velocity-momentum;
		if(maxAcceleration>0){
			velIncrease=velIncrease.normalized*Mathf.Clamp(velIncrease.magnitude,0,maxAcceleration);
			if(velIncrease.magnitude/maxAcceleration>0.9)
				velIncrease+=(0.9f-velIncrease.magnitude/maxAcceleration)*10f*velIncrease;
		}
		velocity=momentum+velIncrease;
		if(maxVelocity>0)
			velocity=velocity.normalized*Mathf.Clamp(velocity.magnitude,0,maxVelocity);
		c.Move(velocity*Time.deltaTime);
		//cachedCollisionDir=Vector3.one*2;
		recalculateCollision=true;
		CheckCursorLock();
	}

	void HandleMouseLook(){
		if (Cursor.lockState==CursorLockMode.None) return;
		//Point transform in mouse direction
		Vector2 mouseLook=new Vector2(Input.GetAxis("Mouse X")*mouseLookSensitivity.x,Input.GetAxis("Mouse Y")*mouseLookSensitivity.y);
		xRot+=mouseLook.x;
		yRot+=mouseLook.y;
		yRot=Mathf.Clamp(yRot,-90,90);
		zRot=Mathf.Lerp(zRot,targetZRot,Time.deltaTime*zRotateSpeed);

		if(lookSmoothing>0)
			transform.localRotation=Quaternion.Slerp(transform.localRotation,Quaternion.Euler(Vector3.up*xRot),Time.deltaTime/lookSmoothing);
		else transform.localEulerAngles=new Vector3(0,xRot,0);

		if(lookSmoothing>0) 
			playerCamera.transform.localRotation=Quaternion.Slerp(playerCamera.transform.localRotation,Quaternion.Euler(Vector3.right*-yRot),Time.deltaTime/lookSmoothing);
		else playerCamera.transform.localEulerAngles=Vector3.right*-yRot;
		neck.localEulerAngles=new Vector3(0,0,zRot);
		//neck.localEulerAngles=new Vector3(-yRot,0,zRot);
	}

	void HandleMovementVelocity(){
		//Run relative direction of transform
		if(localInput!=Vector3.zero)
			runTime=Mathf.Clamp(runTime+Time.deltaTime,0,timeToFullSpeed);
		else runTime=0f;
		float targetMoveSpeed;
		if(wallrunning) targetMoveSpeed=baseWallRunSpeed*((localInput.z>0)?forwardMultiplier:1);
		else targetMoveSpeed=moveSpeed;
		actualMoveSpeed=targetMoveSpeed*runTime/timeToFullSpeed;

		if (!wallrunning){
			//Fire ray to find normal
			RaycastHit hit;
			if (Physics.Raycast(transform.position,transform.right,out hit,wallRunDistance,collisionLayers)){
				wallrunStart=hit.point;
				wallNormal=hit.normal.normalized;
				if(Mathf.Abs(Vector3.Dot(playerCamera.transform.forward,wallNormal))<0.8f){
					wallDirection=Vector3.Cross(wallNormal,Vector3.up);
					if(Vector3.Dot(wallDirection,transform.forward)<0)
						wallDirection=-wallDirection;
					wallrunning=true;
				}
			}
			if (Physics.Raycast(transform.position,-transform.right,out hit,wallRunDistance,collisionLayers)){
				wallrunStart=hit.point;
				wallNormal=hit.normal.normalized;
				if(Mathf.Abs(Vector3.Dot(playerCamera.transform.forward,wallNormal))<0.8f){
					wallDirection=Vector3.Cross(wallNormal,Vector3.up);
					if(Vector3.Dot(wallDirection,transform.forward)<0)
						wallDirection=-wallDirection;
					wallrunning=true;
				}
			}
		}else{
			//Check for reasons to detach
			if (!Physics.Raycast(transform.position,-wallNormal,wallRunDistance*2,collisionLayers))
				wallrunning=false;
		}

		if(localCollisionDir.y<0){
			velocity-=momentum*friction*Time.deltaTime;
			//Run with constant velocity 
			velocity+=globalInput*actualMoveSpeed;
			jumpEnergy=1;//Mathf.Clamp01(jumpEnergy+Time.deltaTime*jumpEnergyIncreaseSpeed);
			if (Input.GetKey(KeyCode.Space)){
				velocity.y=jumpMagnitude*jumpEnergy;
				jumpEnergy*=jumpEnergyPercentOnJump;
			}
		}else if (wallrunning){
			//Colliding with horizontal wall
			Vector3 alignedWallDirection=wallDirection;
			if (Vector3.Dot(transform.forward,wallDirection)<0)
				alignedWallDirection=-wallDirection;
			velocity-=momentum*wallFriction*Time.deltaTime;
			//push out if horizontal axis pushing away from wall
			float wallDot=Vector3.Dot(transform.right*localInput.x,wallNormal);
			velocity+=actualMoveSpeed*alignedWallDirection;
			if (wallDot>0){
				velocity+=globalInput*actualMoveSpeed;
				wallrunning=false;
			}
			if (Vector3.Dot(transform.forward,wallNormal)<-0.7f)
				wallrunning=false;
			float distanceFromWall=Vector3.Dot(wallNormal,transform.position-wallrunStart);
			transform.position+=wallNormal*(wallRunDistance-distanceFromWall);

			if(Input.GetKey(KeyCode.Space)){
				if(velocity.y<0) velocity.y=0;
				velocity+=(Vector3.up*wallJumpYAmount*jumpEnergy+wallNormal)*wallJumpMagnitude;
				wallrunning=false;
				jumpEnergy*=jumpEnergyPercentOnJump;
			}
		}else if (localCollisionDir.z!=0&&localCollisionDir.x==0&&localCollisionDir.y==0){
			//Colliding with forward wall
			velocity-=momentum*wallFriction*Time.deltaTime*(momentum.y>0?1:jumpEnergy);
			velocity+=globalInput*actualMoveSpeed;
			if(Input.GetKey(KeyCode.Space)&&jumpEnergy>0.2f){
				if(velocity.y<0) velocity.y=0;
				velocity+=(Vector3.up*wallJumpYAmount*jumpEnergy-collisionDir*Mathf.Max(0.5f,jumpEnergy))*wallJumpMagnitude;
				jumpEnergy*=jumpEnergyPercentOnJump;
			}
		}else{
			//If not already moving in input dir by threshold, add
			if (Vector3.Dot(globalInput,velocity)<maxAirTiltVelocity)
				velocity+=globalInput*airTiltAmount;
			//jumpEnergy=Mathf.Clamp01(jumpEnergy*(2-jumpEnergyPercentOnJump));
		}
		if(wallrunning) targetZRot=(Vector3.Dot(transform.right,wallNormal)<0?1:-1)*wallRunZRot;
		else targetZRot=0f;
	}

	Vector3 ApplyCollisionForces(Vector3 momentum){
		Vector3 collDir=GetCollisionDirection();
		Vector3 forceValue=Vector3.zero;
		if(collDir.x*momentum.x>0)
			forceValue.x=-momentum.x;
		if(collDir.y*momentum.y>0)
			forceValue.y=-momentum.y;
		if(collDir.z*momentum.z>0)
			forceValue.z=-momentum.z;
		momentum+=forceValue;
		return momentum;
	}

	void CheckCursorLock(){
	//	Debug.Log(EventSystem.current.IsPointerOverGameObject());
		if(Input.GetKeyUp(KeyCode.Escape)){
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible=true;
		}else if(Input.GetMouseButtonDown(0)&&Cursor.lockState==CursorLockMode.None&&!EventSystem.current.IsPointerOverGameObject()){
			Cursor.lockState=CursorLockMode.Locked;
			Cursor.visible=false;
		}
	}

	public Vector3 GetCollisionDirection(){
		if(!recalculateCollision)
			return cachedCollisionDir;
		//Scaled down by 4/5ths
		Vector3 collDir=Vector3.zero;
		Vector3 capsuleStart=transform.position+transform.up*(c.height-2*c.radius)/2;
		Vector3 capsuleEnd=transform.position-transform.up*(c.height-2*c.radius)/2;
		for(int i=0;i<5;i++){
			Vector3 testDir=Vector3.right;
			if (i==1) testDir=-Vector3.right;
			//if (i==2) testDir=Vector3.up;
			if (i==2) testDir=-Vector3.up;
			if (i==3) testDir=Vector3.forward;
			if (i==4) testDir=-Vector3.forward;
			//if (Physics.CapsuleCast(capsuleStart,capsuleEnd,c.radius,transform.TransformDirection(testDir),collisionDetectionRayLength)){
			if (Physics.Raycast(c.ClosestPointOnBounds(transform.position+transform.TransformDirection(testDir)*2),transform.TransformDirection(testDir),collisionDetectionRayLength,collisionLayers)){
				if(collDir.x==0)
					collDir.x=testDir.x;
				if(collDir.y==0)
					collDir.y=testDir.y;
				if(collDir.z==0)
					collDir.z=testDir.z;
			}
		}
		collDir=transform.TransformDirection(collDir);
		cachedCollisionDir=collDir;
		recalculateCollision=false;
		return collDir;
	}

	float ClampAngle(float toClamp,float min,float max){
		//Assume min>180 and max<180
		if (toClamp<min&&toClamp>180) return min;
		if (toClamp>max&&toClamp<180) return max;
		return toClamp;
	}

	Vector3 AbsVector(Vector3 v){
		return new Vector3(Mathf.Abs(v.x),Mathf.Abs(v.y),Mathf.Abs(v.z));
	}

	Vector3 VecMask(Vector3 mask,Vector3 target){
		return new Vector3(mask.x*target.x,mask.y*target.y,mask.z*target.z);
	}

	Vector3 InvertMask(Vector3 mask){
		mask.x=(mask.x==0)?1:0;
		mask.y=(mask.y==0)?1:0;
		mask.z=(mask.z==0)?1:0;
		return mask;
	}

	public void Reset(){
		c.Move(Vector3.zero);
		velocity=Vector3.zero;
	}
}


/*((localCollisionDir!=Vector3.zero&&localCollisionDir.y==0)){
			if(wallNormal==Vector3.zero&&localCollisionDir.x!=0){
				RaycastHit hit;
				Physics.Raycast(transform.position,transform.right*localCollisionDir.x,out hit);
				wallNormal=hit.normal.normalized;
				wallDirection=Vector3.Cross(wallNormal,Vector3.up);
			}else if (localCollisionDir.x==0) wallNormal=Vector3.zero;
			

			//Wallrunning, add vertical friction and parallel-to-wall force
			velocity-=momentum*wallFriction*Time.deltaTime;
			//If pushing away from wall detatch
			//Otherwise push forward
			float wallDot=Vector3.Dot(globalInput,wallNormal);
			//if(wallNormal.magnitude!=0) Debug.Log(wallDot+"="+wallNormal+" dot "+globalInput);
			//if normal dot global input is > stickiness (!NO ABS!)
			//	can push out
			if (wallDot>wallStickiness||localCollisionDir.x==0){
				velocity+=globalInput*actualMoveSpeed;
				//Debug.Log("Moving with user's choice");
			}else{
				//Debug.Log("Moving forward on wall "+transform.forward+" dot "+wallDirection+" = "+Vector3.Dot(transform.forward,wallDirection));
				if (Vector3.Dot(transform.forward,wallDirection)>0)
					velocity+=wallDirection.normalized*actualMoveSpeed;
				else velocity-=wallDirection.normalized*actualMoveSpeed;
			}
			//velocity+=collisionDir*0.1f;
			if(Input.GetKey(KeyCode.Space)){
				if(velocity.y<0) velocity.y=0;
				velocity+=(Vector3.up*wallJumpYAmount-collisionDir).normalized*wallJumpMagnitude;
			}
			targetZRot=localCollisionDir.x*wallRunZRot;*/