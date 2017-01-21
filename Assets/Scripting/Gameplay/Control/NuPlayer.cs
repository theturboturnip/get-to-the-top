using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum PlayerMovementMode{
	StartingLevel=0,
	Grounded=1,
	InAir=2,
	Vaulting=3,
	Wallrunning=4,
	EndingLevel=5
};

/*public struct CapsuleData{
	public Vector3 centre;
	public float radius,height;
}*/


public class NuPlayer : MonoBehaviour {
	public PlayerMovementMode currentMode=PlayerMovementMode.StartingLevel;
	public float health=5; //Take 5 shots before death
	[Header("Mouse Look Data")]
	public bool lockedLook=false;
	public float lookSmoothing=0;
	public Vector2 mouseSensitivity,camRot,viewPunch;
	public float zRot,targetZRot,zRotateSpeed;
	public Vector3 clampLook;
	public float minClampDot;
	[Header("Grounded Data")]
	public KeyCode jumpKey=KeyCode.Space;
	public float groundVelocity,groundAccel,movingFriction,jumpAmount,turnSpeed;
	float moveSpeed=0;//For utility
	Vector3 groundStartVelocity;
	[Header("Air Data")]
	public Vector3 airStartVelocity;
	public float gravity,airMoveAmount,maxAirVelForPush;
	[Header("Vaulting Data")]
	public bool canVault;
	[ReadOnly]
	public int vaultStage=0;
	public float minVaultTime,maxVaultTime,ledgeClimbTime,vaultUpwardsImpulse,vaultZRot,maxVaultDist,climbFinishTime,vaultEndImpulse,vaultStartAngle;
	[ReadOnly]
	public Vector3 vaultVel;
	[ReadOnly]
	public float vaultTargetHeight,vaultOverDistance;
	[ReadOnly]
	public Vector3 vaultStart;
	[ReadOnly]
	public RaycastHit vaultHit;
	[Header("Wallrun Data")]
	public bool canWallrun;
	public Vector3 wallrunDir,wallrunStart;
	public float wallrunStartThreshold=0.2f,wallrunEndThreshold=0.5f,wallrunGravity,wallrunCheckRayDist,wallrunZRot,wallrunMoveSpeed;
	public float wallrunDistance,wallrunLookClampAngle,wallrunAcceleration,wallrunInputAmount,maxWallrunDistance,wallrunDetachSpeed;
	public Vector2 wallJumpAmount,wallrunStartImpulse;
	public RaycastHit wallrunHit;
	[Header("Level Bookend Data")]
	public float startDeceleration=5f; //Decelerate by x m/s per second
	public float startLevelTargetHeight,startLevelStartHeight;
	public static float startLevelTime=5;
	public float startLevelStart;
	public Transform tubeFloor;
	public float endVelocity;
	public float levelEndPositionCorrection;
	[Header("Physics Framerate Data")]
	public int maxUpdateTimesteps;
	public int targetPhysicsFramerate;

	[Header("Wind Settings")]
	public AudioSource windSource;
	public float maxWindVolume,windVolumeAdjustSpeed;
	[ReadOnly]
	public float targetWindVolume,actualWindVolume;
	public float windMinSpeed,windMaxSpeed;
	public float windPitchMag;
	public float windPanAmount;

	[Header("Feet Audio Settings")]
	public AudioSource feetSource;
	public AudioClip[] walkingFeetClips;
	public AudioClip jumpClip,landClip;
	public float moveDistanceForFootEffect,wallrunDistanceMultiplier;
	public float groundedVolume,wallrunVolume,footPan,footVolumeVar,headBobAmount,headBobTime;
	float groundedMoveDistance,startGroundedTime=-1;

	[Header("Misc")]
	public float armLength;
	public Vector3 velocity;
	[ReadOnly]
	public Vector3 localInputDir,globalInputDir,momentum;
	CustomCharacterController c;
	Camera cam;
	bool recalculateCollision=true;
	float debugFloat,multistepTimePending=0;
	Vector3 debugVector;

	delegate void MultistepFunction(float deltaTime);

	//MultistepFunction airFunc=HandleInAir;

	// Use this for initialization
	void Start () {
		c=GetComponent<CustomCharacterController>();
		cam=gameObject.GetComponentInChildren<Camera>();
		camRot.x=transform.rotation.eulerAngles.y;
		camRot.y=-cam.transform.rotation.eulerAngles.x;
		feetSource.panStereo=footPan;

		if (currentMode==PlayerMovementMode.StartingLevel)
			BeginLevelStart();
	}
	
	// Update is called once per frame
	void Update () {
		if (currentMode!=PlayerMovementMode.EndingLevel)
			HandleMouseLook();
		
		float oldLocalInputMag=localInputDir.magnitude;
		localInputDir=InputHandler.GetMovementAxis();
		if (localInputDir.magnitude>0 && oldLocalInputMag==0)
			startGroundedTime=Time.time;
		globalInputDir=transform.TransformDirection(localInputDir).normalized;
		UpdateMovementMode();

		if (currentMode==PlayerMovementMode.InAir) DoPhysMultistep(HandleInAir);
		else if (currentMode==PlayerMovementMode.Grounded) MultistepGrounded();
		else if (currentMode==PlayerMovementMode.Vaulting) DoPhysMultistep(HandleVaulting);
		else if (currentMode==PlayerMovementMode.Wallrunning) MultistepWallrun();
		else if (currentMode==PlayerMovementMode.StartingLevel) DoPhysMultistep(HandleLevelStart);
		else if (currentMode==PlayerMovementMode.EndingLevel) DoPhysMultistep(HandleLevelEnd);


	}

	void LateUpdate(){
		if (currentMode==PlayerMovementMode.StartingLevel||currentMode==PlayerMovementMode.EndingLevel){
			if (currentMode==PlayerMovementMode.EndingLevel){
				transform.position=Vector3.Lerp(transform.position,tubeFloor.position+Vector3.up*(tubeFloor.localScale.y/2+1),Time.deltaTime*levelEndPositionCorrection);
				transform.position+=velocity*Time.deltaTime;
				cam.transform.rotation=Quaternion.Slerp(cam.transform.rotation,Quaternion.LookRotation(-tubeFloor.forward),Time.deltaTime*levelEndPositionCorrection);
			}
			tubeFloor.position=transform.position+Vector3.down*(c.height/2+tubeFloor.lossyScale.y/2);
			//tubeFloor.position=velocity*Time.deltaTime;
			//

		}else{
			//MultistepMovement();
			c.Move(velocity*Time.deltaTime);
			//cachedCollisionDir=Vector3.one*2;
			//recalculateCollision=true;
			momentum=c.velocity;//ApplyCollision(velocity);
			velocity=momentum+Vector3.zero;
			windSource.panStereo=Mathf.Sin(Time.time)*windPanAmount;
			targetWindVolume=Mathf.Clamp01((velocity.magnitude-windMinSpeed)/(windMaxSpeed-windMinSpeed))*maxWindVolume;
			windSource.pitch=0.5f+windPitchMag*windSource.volume/maxWindVolume;
		}

		if (currentMode==PlayerMovementMode.Grounded){
			groundedMoveDistance+=SetY(velocity).magnitude*Time.deltaTime*(currentMode==PlayerMovementMode.Wallrunning?wallrunDistanceMultiplier:1);
			if (groundedMoveDistance>=moveDistanceForFootEffect){
				groundedMoveDistance-=moveDistanceForFootEffect;
				//if (!(feetSource.isPlaying&&feetSource.clip==landClip))
					feetSource.PlayOneShot(walkingFeetClips[0]);
					feetSource.volume=groundedVolume+Random.Range(-footVolumeVar,footVolumeVar);
					//targetZRot=-Mathf.Sign(feetSource.panStereo)*groundedHeadBob;
					feetSource.panStereo=-Mathf.Sign(feetSource.panStereo)*footPan;
			}
		}
		if (currentMode==PlayerMovementMode.Wallrunning){
			//if (velocity.y>0)
			//	groundedMoveDistance+=velocity.y*Time.deltaTime*wallrunDistanceMultiplier;
			//else 
			//	groundedMoveDistance+=Mathf.Min(velocity.y*-0.5f,1)*Time.deltaTime*wallrunDistanceMultiplier;
			groundedMoveDistance+=SetY(velocity).magnitude*Time.deltaTime*wallrunDistanceMultiplier;
			if (groundedMoveDistance>=moveDistanceForFootEffect){
				groundedMoveDistance-=moveDistanceForFootEffect;
				//if (!(feetSource.isPlaying&&feetSource.clip==landClip))
					feetSource.PlayOneShot(walkingFeetClips[0]);
					feetSource.volume=wallrunVolume+Random.Range(-footVolumeVar,footVolumeVar);
			}
		}
		if (Time.timeScale==0){
			feetSource.volume=0;
			windSource.volume=0;
		}
	}

	void MultistepMovement(){
		DoPhysMultistep(MovementMultistepProxy);
	}

	void MovementMultistepProxy(float deltaTime){
		c.Move(velocity*deltaTime);
	}

	void UpdateMovementMode(){
		if (currentMode==PlayerMovementMode.Grounded){
			if (CanStartVault()) BeginVault();
			else if (CanStartInAir()) BeginInAir();
			if (currentMode!=PlayerMovementMode.Grounded){
				viewPunch=Vector2.zero;
				startGroundedTime=Time.time;
			}
		}else if (currentMode==PlayerMovementMode.InAir){
			if (CanStartWallrun()) BeginWallrun();
			else if (CanStartVault()) BeginVault();
			else if (CanStartGrounded()){
				feetSource.clip=landClip;
				feetSource.PlayOneShot(landClip);
				BeginGrounded();
			}
		}else if (currentMode==PlayerMovementMode.Wallrunning){
			if (CanStartVault()){
				EndWallrun();
				BeginVault();
			}else if (CanStartGrounded()){
				EndWallrun();
				BeginGrounded();
			}
		}else if (currentMode==PlayerMovementMode.Vaulting&&vaultStage<0){
			if (CanStartGrounded()) BeginGrounded();
			else BeginInAir();
		}
	}

	void DoPhysMultistep(MultistepFunction func){
		int timesteps;
		multistepTimePending += Time.deltaTime;
		int safePhysicsIterator = Mathf.RoundToInt(((multistepTimePending) * (float)targetPhysicsFramerate)); //If our framerate is low, we run our physics more times for more accuracy.
		timesteps = Mathf.Clamp(safePhysicsIterator, 0, maxUpdateTimesteps);
		if (timesteps==0) return;
		float deltaTime = multistepTimePending / (float)timesteps;
		multistepTimePending -= timesteps*deltaTime;
		for(int i = 0; i < timesteps; i++){
			func(deltaTime);
			windSource.volume=Mathf.Lerp(windSource.volume,targetWindVolume,deltaTime*windVolumeAdjustSpeed);
		}
		actualWindVolume=windSource.volume;
	}

	/*
		GROUNDED FUNCTIONS
							*/

	bool CanStartGrounded(){
		foreach(Vector3 cDir in c.collisionDirections){
			if (cDir.y<-0.5f) return true;
		}
		return false;
	}

	void BeginGrounded(){
		currentMode=PlayerMovementMode.Grounded;
		groundStartVelocity=velocity;
		multistepTimePending=0;
		if (LevelHandler.currentLevel.isProcGen){
			Vector3 checkpointPos,checkpointDir;
			for(int i=0;i<c.collisionDirections.Count;i++){
				//Debug.Log(c.collisionHits[i].transform.gameObject);
				if (c.collisionDirections[i].y<-0.5f&&c.collisionHits[i].transform.gameObject.GetComponent<BuildingGeneratorv2>()!=null){
					checkpointPos=c.collisionColliders[i].bounds.center+(c.collisionColliders[i].bounds.extents.y+1)*Vector3.up;
					checkpointDir=c.collisionHits[i].transform.right;
					LevelHandler.currentLevel.ActivateCheckpoint(checkpointPos,checkpointDir);
					break;
				}
			}
		}
		targetWindVolume=0;
		groundedMoveDistance=moveDistanceForFootEffect;
		feetSource.panStereo=footPan;
		//Debug.Log(startGroundedTime);
		if (Time.time-startGroundedTime>0.5f)
			startGroundedTime=Time.time;
		//Debug.Log(startGroundedTime);

	}

	void HandleGrounded(float deltaTime=0){	
		targetZRot=0;
		velocity=SetY(velocity,0);
		moveSpeed=velocity.magnitude;
		float accel=movingFriction;
		if (moveSpeed<localInputDir.magnitude*groundVelocity) accel=groundAccel;

		moveSpeed=Mathf.Lerp(moveSpeed,localInputDir.magnitude*groundVelocity,accel*deltaTime);
		if (Vector3.Dot(globalInputDir,momentum)<0)
			//Super special turn-aroundy stuff
			velocity=globalInputDir*(1+Vector3.Dot(globalInputDir,momentum.normalized));
		else if (localInputDir.sqrMagnitude>0.5f)
			velocity=Vector3.Lerp(velocity.normalized,globalInputDir,turnSpeed*deltaTime)*moveSpeed;
		else{
			velocity=velocity.normalized*moveSpeed;
		}

		if (velocity.magnitude<0.01f)
			velocity=Vector3.zero;

	}

	void MultistepGrounded(){
		DoPhysMultistep(HandleGrounded);
		//if (Input.GetKey(jumpKey)){ 
		if (InputHandler.GetButton("Jump")){ 
			velocity=SetY(velocity,0)*0.9f;
			velocity.y=jumpAmount;
			feetSource.clip=jumpClip;
			feetSource.PlayOneShot(jumpClip);
		}else velocity.y=-1; //Ensure that we stay grounded

		//Handle head bob
		//Mathf.Sin(groundedMoveDistance/moveDistanceForFootEffect*Mathf.PI*2)
		//cam.transform.localPosition=Vector3.up*(0.9f+(Mathf.Sin(groundedMoveDistance/moveDistanceForFootEffect*2*Mathf.PI)*2+0.5f)*groundedHeadBob);
		//viewPunch=new Vector2();
		viewPunch=Vector2.zero;
		if (velocity.magnitude>0.01f){
			viewPunch.x=Mathf.Sin((Time.time-startGroundedTime)*headBobTime)*SetY(velocity).magnitude*headBobAmount/1;
			viewPunch.y=Mathf.Sin(2*(Time.time-startGroundedTime)*headBobTime)*SetY(velocity).magnitude*headBobAmount/4;
		}
	}

	/*
		IN AIR FUNCTIONS 
							*/

	bool CanStartInAir(){
		foreach(Vector3 cDir in c.collisionDirections){
			if (cDir.y<-0.5f) return false;
		}
		return true;
	}

	void BeginInAir(){
		currentMode=PlayerMovementMode.InAir;
		airStartVelocity=velocity;
		multistepTimePending=0;
		if (actualWindVolume<0.1f&&targetWindVolume==0f)
			windSource.Play();
		targetWindVolume=maxWindVolume;
	}

	void HandleInAir(float deltaTime){
		targetZRot=0;
		int airVelocityVersion=1;
		//V1 Air Velocity (Realistic sort of)
		//if right velocity . global < maxAirVelForPush add 
		float rightDot=Vector3.Dot(transform.right.normalized*localInputDir.x,velocity),forwardDot=Vector3.Dot(transform.forward.normalized*localInputDir.z,velocity);
		if (rightDot<maxAirVelForPush||maxAirVelForPush<=0)
			velocity+=transform.right*localInputDir.x*airMoveAmount*deltaTime;
		if (forwardDot<maxAirVelForPush||maxAirVelForPush<=0)
			velocity+=transform.forward*localInputDir.z*airMoveAmount*deltaTime;

		//V2 Air Velocity (Doom-Style)
		if (airVelocityVersion==2)
			velocity=SetY(airStartVelocity+globalInputDir*airMoveAmount,velocity.y);

		velocity.y-=gravity*deltaTime;
//		windSource.volume=Mathf.Clamp01((velocity.magnitude-windMinSpeed)/(windMaxSpeed-windMinSpeed));
	}

	/* 
		VAULTING FUNCTIONS
							*/

	bool CanStartVault(){
		if (!canVault) return false;

		if (localInputDir.z<0) return false;
		bool foundVaulter=false;
		for(int i=0;i<c.collisionDirections.Count;i++){
			vaultHit=c.collisionHits[i];
			if (Vector3.Dot(c.collisionDirections[i],transform.forward)>Mathf.Cos(vaultStartAngle*Mathf.Deg2Rad)&&vaultHit.transform!=null&&vaultHit.transform.gameObject.tag!="BlockParkour"){
				foundVaulter=true;
				break;
			}
		}
		//Debug.Log(foundVaulter);
		if (!foundVaulter) return false;

		//We'll be vaulting over vaultHit
		float distanceToTop=SuperCollisions.ClosestPointOnSurface(vaultHit.collider,transform.position+Vector3.up*c.height/2).y/*vaultHit.collider.bounds.extents.y+vaultHit.transform.position.y*/-transform.position.y;
		distanceToTop=vaultHit.collider.bounds.center.y+vaultHit.collider.bounds.extents.y-transform.position.y-1;
		if (distanceToTop>armLength){
			//.Log("Failed dist check "+(vaultHit.collider.bounds.center.y+vaultHit.collider.bounds.extents.y)+" "+(1+transform.position.y));
			return false;
		}


		//Do a box test to make sure there's enough room to vault
		/*Collider[] intersecting=Physics.OverlapBox(transform.position+Vector3.up*(c.height+Mathf.Max(0,distanceToTop))-vaultHit.normal*c.radius,new Vector3(c.radius,c.height/2,c.radius*2));
		if (intersecting.Length>0){
			Debug.Log("Failed room check");
			return false;
		}*/
		return true;
	}

	void BeginVault(){
		currentMode=PlayerMovementMode.Vaulting;

		Vector3 vaultObjSize=vaultHit.collider.bounds.size;
		if (vaultHit.collider is BoxCollider){
			vaultObjSize=Vector3.Scale(vaultHit.transform.lossyScale,((BoxCollider)vaultHit.collider).size);
		}
		vaultTargetHeight=vaultHit.collider.bounds.center.y+vaultObjSize.y/2+c.height/2;

		vaultOverDistance=vaultObjSize.z;
		

		if(Mathf.Abs(vaultHit.normal.x)>Mathf.Abs(vaultHit.normal.z)){
			vaultOverDistance=vaultObjSize.x;
		}
		if(maxVaultDist<vaultOverDistance){
			//We are climbing
			vaultOverDistance=2*c.radius;
		}else{
			//We are vaulting over
			vaultOverDistance+=2*c.radius;
			vaultTargetHeight=vaultHit.transform.position.y+vaultObjSize.y/2+c.height/4;
			Vector3 relativeView=cam.transform.forward-Vector3.Dot(cam.transform.forward,vaultHit.normal)*vaultHit.normal;
			targetZRot=vaultZRot*(relativeView.x>0?1:-1);
		}
		vaultOverDistance+=0.1f;

		vaultStage=0;

		//clampLook=-vaultHit.normal;
		//minClampDot=0.5f;

		//=Vector3.up*(c.height/2-c.radius);
		//c.height=c.radius*2;
		vaultStart=transform.position;
		vaultStart.y=vaultTargetHeight;

		multistepTimePending=0;
		targetWindVolume=maxWindVolume;
	}

	void HandleVaulting(float deltaTime){
		//Stages of vaulting over:
		//At start set velocity to go up (0)
		//Once at targetY from start>vaultOverDistance set velocity to go over (1)
		//Once travelled correct dist end (2)

		//Stages of climbing up:
		//When target height cleared add some forward (3)
		//Once forward dist traveled >2radius end (4)

		//NEW VERSION
		//(1) If going over set velocity to go over without slowing the player down
		//by adding vertical velocity so that you get to the correct y when you've travelled the 2*radii
		//and (if necessary) adding horizontal velocity to get to the right distance in vaultTime
		//(2) Once we've gone the right distance goto -1

		//To stop set vaultStage to -1

		switch (vaultStage){
			case 0:
				//timeToRightDistance is essential, as it's how valuting ends
				//
				float timeToRightDistance;
				float v=-Vector3.Dot(velocity,vaultHit.normal); //d is the velocity towards the wall
				if (Mathf.Abs(v)<0.01f)
					v=0.01f;
				if (v<0)
					Debug.Log("Panic, v<0"); //D should be >0 otherwise we are vaulting through a wall
 				timeToRightDistance=vaultOverDistance/v;
				timeToRightDistance=Mathf.Clamp(timeToRightDistance,minVaultTime,maxVaultTime);
				float newVelMag=vaultOverDistance/timeToRightDistance;
				velocity=SetY(transform.forward,0).normalized*newVelMag;

				//conditions for vertical:
				//find u such that
				//	a=-gravity (only one left to eliminate)
				//	v=0 (essential)
				// 	t=timeToRightDistance (essential for horizontal v)
				//  s is such that i get to target height (essential)
				//s=1/2 * (u+v) * t
				//u+v = 2s/t
				//v=u+at
				//u=-at

				//The new deceleration ensures that v=0 i.e. a=-u/t
				//=>a is unecessary

				float verticalVel=Mathf.Max(0,2*(vaultTargetHeight-transform.position.y)/timeToRightDistance);
				velocity.y=verticalVel;
				vaultStart.y=verticalVel;
				vaultVel=SetY(velocity,0);
				c.IgnoreCollision(vaultHit.collider,true);
				vaultStage=1;
				break;
			case 1:
				float progressFromStart=SetY(vaultStart-transform.position,0).magnitude/(vaultOverDistance);
				velocity=vaultVel;
				velocity.y=Mathf.Lerp(vaultStart.y,0,progressFromStart);
				if (progressFromStart>=1){
					vaultStage=-1;
				}
				break;
		}

		if (vaultStage<0){
			//Quit vaulting
			c.IgnoreCollision(vaultHit.collider,false);
			targetZRot=0;
			moveSpeed=0;
			targetWindVolume=maxWindVolume;
		}
	}

	/*
		WALLRUN FUNCTIONS 
							*/

	bool CanStartWallrun(){
		if (!canWallrun) return false;
		if (Vector3.Dot(transform.forward,momentum)<0) return false;

		bool foundWallrun=false;
		int loops=0;
		
		//check each collision
		//if we have a collision with a horizontal dir
		//and we aren't looking into the wall
		//use it as a wallrun
		int i=0;
		Collider wallCollider;
		foreach(Vector3 collisionDir in c.collisionDirections){
			if (Mathf.Abs(collisionDir.y)>0.01) continue;

			//We want dot (normal,forward) to be as close to zero as possible
			float parallelAngle=90-Mathf.Acos(Mathf.Abs(Vector3.Dot(transform.forward,c.collisionHits[i].normal)))*Mathf.Rad2Deg;
			//Debug.Log(Vector3.Dot(transform.forward,c.collisionHits[i].normal)+","+Mathf.Acos(Vector3.Dot(transform.forward,c.collisionHits[i].normal))*Mathf.Rad2Deg);
			if (parallelAngle>wallrunStartThreshold||parallelAngle<0) continue;
			//If the collider top or bottom is within the player then don't do
			wallCollider=c.collisionHits[i].collider;
			if (wallCollider.bounds.center.y+wallCollider.bounds.size.y/2<transform.position.y+c.height/2) continue;
			if (wallCollider.bounds.center.y-wallCollider.bounds.size.y/2>transform.position.y+c.height/2) continue;
			if (wallCollider.transform.gameObject.tag=="BlockParkour") continue;

			//Wallrun here
			wallrunHit=c.collisionHits[i];
			return true;
		}
		return foundWallrun;
		targetWindVolume=maxWindVolume;
	}

	void BeginWallrun(){
		currentMode=PlayerMovementMode.Wallrunning;
		Vector3 perpVelocity=Vector3.Cross(Vector3.up,wallrunHit.normal);
		if (Vector3.Dot(perpVelocity,transform.forward)>0)
			wallrunDir=perpVelocity;
		else
			wallrunDir=-perpVelocity;
		//clampLook=(wallrunHit.normal).normalized;
		//minClampDot=Mathf.Cos(wallrunLookClampAngle/2*Mathf.Deg2Rad);

		moveSpeed=Vector3.Dot(momentum,wallrunDir);
		velocity=wallrunDir*Mathf.Max(wallrunStartImpulse.x,momentum.magnitude)+Vector3.up*Mathf.Max(Vector3.Dot(momentum,Vector3.up)*0.5f,wallrunStartImpulse.y);
		targetZRot=wallrunZRot*(Vector3.Dot(transform.right,wallrunHit.normal)>0?-1:1);

		wallrunDistance=((wallrunCheckRayDist)/2+c.radius);
		transform.position=wallrunHit.point+wallrunHit.normal*wallrunDistance;
		wallrunStart=wallrunHit.point;
		multistepTimePending=0;
		groundedMoveDistance=moveDistanceForFootEffect;
		feetSource.panStereo=Mathf.Sign(targetZRot)*footPan;
	}

	void HandleWallrun(float deltaTime){
		moveSpeed=Vector3.Dot(momentum,wallrunDir);
		if(moveSpeed<wallrunMoveSpeed)
			moveSpeed=Mathf.Lerp(moveSpeed,wallrunMoveSpeed,deltaTime*wallrunAcceleration);
		velocity=Vector3.up*velocity.y+wallrunDir*(moveSpeed+localInputDir.z*wallrunInputAmount);
		velocity.y-=wallrunGravity*deltaTime;

		float distanceFromWall=Vector3.Dot(wallrunHit.normal,transform.position-wallrunStart);
		transform.position+=wallrunHit.normal*(wallrunDistance-distanceFromWall);
	}

	void MultistepWallrun(){
		bool shouldEnd=false;
		if (Vector3.Dot(velocity,wallrunHit.normal)>wallrunDetachSpeed)
			shouldEnd=true;
		else
			DoPhysMultistep(HandleWallrun);
		//if (Input.GetKeyDown(jumpKey)){
		if (InputHandler.GetButtonDown("Jump")){
			velocity+=(wallrunHit.normal)*(wallJumpAmount.x-Vector3.Dot(velocity,wallrunHit.normal));
			velocity.y=Mathf.Max(velocity.y,wallJumpAmount.y);
			BeginInAir();
			shouldEnd=true;
		}
		if (!Physics.Raycast(transform.position,-wallrunHit.normal,c.radius+wallrunCheckRayDist) ){//|| (localInputDir.magnitude>0 && Vector3.Dot(wallrunHit.normal,transform.TransformDirection(localInputDir))>wallrunEndThreshold)){
			BeginInAir();
			shouldEnd=true;
		}
		if(shouldEnd) EndWallrun();
	}

	void EndWallrun(){
		clampLook=Vector3.zero;
		targetZRot=0;
		lockedLook=false;
		moveSpeed=SetY(momentum,0).magnitude;
	}

	/*
		LEVEL BOOKEND FUNCTIONS
								*/

	void BeginLevelStart(){
		currentMode=PlayerMovementMode.StartingLevel;
		StartLevelTrigger st=(GameObject.FindObjectOfType(typeof(StartLevelTrigger)) as StartLevelTrigger);
		AlignLookDirection(st.transform.TransformDirection(st.gameObject.GetComponent<LevelCheckpoint>().spawnLookDir));
		tubeFloor=st.transform.Find("TubeFloor");
		

		startLevelTargetHeight=st.transform.position.y-1.00f;
		startLevelStartHeight=startLevelTargetHeight-80;
		transform.position=st.transform.position+Vector3.up*(startLevelTargetHeight-st.transform.position.y-80);
		float tubeFloorHeight=tubeFloor.gameObject.GetComponent<Collider>().bounds.size.y;
		tubeFloor.position=transform.position+Vector3.down*(c.height/2+tubeFloorHeight/2);
		targetWindVolume=0;
		if (LevelHandler.shouldSkipIntro){
			transform.position=st.transform.position+Vector3.down*st.transform.position.y+Vector3.up*startLevelTargetHeight;
			tubeFloor.position=transform.position+Vector3.down*(c.height/2+tubeFloorHeight/2);
			LevelHandler.shouldSkipIntro=false;
			BeginGrounded();
			return;
		}
		startLevelStart=Time.time;

		//s=0.5(u+v)t
		//u+v=2s/t
		velocity=160/startLevelTime*Vector3.up;
		startDeceleration=-velocity.y/startLevelTime;
		debugVector=velocity;
		multistepTimePending=0;
	}

	void HandleLevelStart(float deltaTime){
		//velocity is a function of distance
		//velocity.y=Mathf.Lerp(160/startLevelTime,0,(transform.position.y-startLevelStartHeight)/80f);
		//V=U+AT
		//velocity.y+=startDeceleration*deltaTime;
		//velocity.y=160/startLevelTime+*startDeceleration;
		float t=(Time.time-startLevelStart);
		//Mathf.Min(-160/startLevelTime,0.5f*startDeceleration*t*t)
		transform.position+=Vector3.up*(startLevelStartHeight+160/startLevelTime*t+0.5f*startDeceleration*t*t-transform.position.y);
		//debugFloat=startDeceleration;
		debugVector= transform.position;
		debugFloat=transform.position.y-startLevelTargetHeight;
		if (velocity.y<0||transform.position.y-startLevelTargetHeight>=-0.01f){
			velocity=Vector3.zero;
			BeginGrounded();
		}
	}

	#if UNITY_EDITOR
	void OnGUI(){
		GUILayout.Button("Float: "+debugFloat+" Vector: "+debugVector);
	}
	#endif

	public void BeginLevelEnd(){
		currentMode=PlayerMovementMode.EndingLevel;
		EndLevelTrigger et=(GameObject.FindObjectOfType(typeof(EndLevelTrigger)) as EndLevelTrigger);
		tubeFloor=et.transform.Find("TubeFloor");
		velocity=endVelocity*Vector3.up;
		clampLook=Vector3.zero;
		targetZRot=0;
		multistepTimePending=0;
		targetWindVolume=0;
	}

	void HandleLevelEnd(float deltaTime){
		if (transform.position.y>LevelHandler.currentLevel.maxYHeight){
			velocity=Vector3.zero;
			LevelHandler.currentLevel.BypassedMaxY();
		}
		windSource.volume=0;
	}

	/*
		UTIL FUNCTIONS
						*/

	Vector3 SetY(Vector3 v,float y=0){
		v.y=y;
		return v;
	}

	public void AlignLookDirection(Vector3 dir){
		Quaternion lookRot=Quaternion.LookRotation(dir);
		camRot.x=lookRot.eulerAngles.y;
		camRot.y=lookRot.eulerAngles.x;
		/*dir=dir.normalized;
		Vector3 horizDir=(dir-Vector3.up*dir.y).normalized;
		camRot.x=Mathf.Acos(Vector3.Dot(horizDir,Vector3.forward))*Mathf.Rad2Deg;
		Debug.Log(horizDir);
		camRot.y=Mathf.Acos(dir.y)*Mathf.Rad2Deg-90;
		Debug.Log(dir.y);*/
	}

	void HandleMouseLook(){
		if (Cursor.lockState==CursorLockMode.None) return;

		Vector2 oldCamRot=camRot;
		camRot+=InputHandler.GetMouseDelta();

		camRot.x=camRot.x%360;
		camRot.y=Mathf.Clamp(camRot.y,-90,90);
		zRot=Mathf.Lerp(zRot,targetZRot,Time.deltaTime*zRotateSpeed);

		Vector3 camEuler=cam.transform.localEulerAngles,tEuler=transform.localEulerAngles;
		
		tEuler=Vector3.up*(camRot.x+viewPunch.x);
		camEuler=Vector3.left*(camRot.y+viewPunch.y)+Vector3.forward*zRot;

		Vector3 newForward=Quaternion.Euler(tEuler+camEuler)*Vector3.forward;
		if (Vector3.Dot(newForward,clampLook)<minClampDot && clampLook.magnitude>0){
			camRot=oldCamRot;
			return;
		}

		if (lookSmoothing<=0){
			transform.localEulerAngles=tEuler;
			cam.transform.localEulerAngles=camEuler;
		}else{
			transform.rotation=Quaternion.Lerp(transform.rotation,Quaternion.Euler(tEuler),Time.deltaTime/lookSmoothing);
			cam.transform.localRotation=Quaternion.Lerp(cam.transform.localRotation,Quaternion.Euler(camEuler),Time.deltaTime/lookSmoothing);
		}
	}

	float AngleDist(float a1,float a2){
		//assume a1+a2 between 0 and 360
		//if a1=5 && a2=355 a1-a2=-10
		//if angle > 180 angle-=360
		//a1=5 a2=-5 a1-a2=10 => return -(a1-a2) => return a2-a1
		float a = a1 - a2;
		a = PosiMod((a + 180),360) - 180;
		return a;
	}

	float PosiMod(float a,float n){
		return a - Mathf.Floor(a/n) * n;
	}


	Vector3 GetCapsuleStart(){
		return transform.position+transform.up*(c.height-2*c.radius)/2;
	}

	Vector3 GetCapsuleEnd(){
		return transform.position-transform.up*(c.height-2*c.radius)/2;
	}


	public void Reset(){
		Debug.Log("Ran Reset");
		velocity=Vector3.zero;
		momentum=Vector3.zero;
		currentMode=PlayerMovementMode.InAir;
		c.Move(Vector3.zero);
	}

	public void GetHit(float damage){
		health=Mathf.Max(0,health-damage);
		if (health==0 && LevelHandler.currentLevel!=null)
			LevelHandler.currentLevel.PlayerDied();
	}
}
