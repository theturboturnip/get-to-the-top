using UnityEngine;
using System.Collections;

public class Shotgun : Gun,IKController {
	AudioSource shotgunAudio;

	[Header("Firing")]
	public float shotgunForce;
	public float thrustModifier=1f;
	public float lookBackAngle=180f;
	public float lookBackTime=5f;
	public float recoilPosAmount,recoilRotAmount,recoilRecovery=1;
	float currentRecoilPosAmount=0,currentRecoilRotAmount=0,targetRecoilRot;
	public bool canLookBack=true;

	[Header("Reloading")]
	public int maxBullets;
	public float reloadTimePerShell;
	public float waitToReload;
	public Transform animShell;
	public Transform animShellStart;
	public Transform animShellEnd;
	public float reloadTurnAngle=65;
	public float rotateToReloadSpeed=4,rotateToReloadTime;
	public AudioClip reloadShellClip;
	float reloadStart;

	[Header("Cocking")]
	public float cockTime;
	public float waitToCock;
	public AudioClip startCockClip;
	public AudioClip endCockClip;
	Vector3 startCockPos,midCockPos;
	public Transform cocker;
	public Vector3 handleCockOffset;
	public GameObject physicsShell;
	public Transform physicsShellSpawn;
	public Vector3 localShellEjectDirection;

	[Header("Muzzle Flash")]
	public MuzzleFlash muzzleFlashScript;

	[Header("IK")]
	public PlayerIK playerIK;
	public Transform rightHandHold,rightElbowHint,leftHandHold,leftElbowHint;

	[Header("Deploy Data")]
	public bool shouldDeploy=true; //if false then when the animstate becomes "Deploy" we undeploy the weapon
	public float deployStart=0; //Time we started deploying
	public float deployTime=0; //Time deploying takes
	public float deployDeltaY=-1; //Local position influence, start with this delta if deploying or go to this delta if undeploying
	public float deployGunLookAngle=-10; //gun look angle at start of deploy/end of undeploy
	public float deployWaitForCock=0.2f;

	[Header("Misc")]
	public float middleDistance=10f;
	public float barrelLength;
	public LayerMask decalCollision;
	public Camera playerCamera;
	public NuPlayer player;
	public string animState="None";
	public float lookAdaptSpeed=180; //x degrees/second
	public bool flamboyant=false;

	public int bulletsLeft;
	bool reloading,cocking,hasBulletToEject;
	float reloadTimeTaken,cockTimeTaken;
	Vector3 currentLook,lookVelocity;
	public float smoothTime=0.1f;
	float currentLookBackAngle=0,timeLookingBack,zRot,targetZRot;
	public Vector3 originalLocalPos,lookTarget;
	Quaternion updateStartRot,currentLookRot;
	bool started=false;
	public bool shouldFire=false;

	// Use this for initialization
	public override void Start () {
		if (started) return;
		base.Start();
		shotgunAudio=GetComponent<AudioSource>();
		startCockPos=cocker.localPosition;
		bulletsLeft=maxBullets;
		animShell.gameObject.SetActive(false);
		originalLocalPos=transform.localPosition;
		currentLook=playerCamera.transform.position+playerCamera.transform.forward*middleDistance;
		started=true;
		//StartDeploy();
	}

	void OnEnable(){
		Start();
		animState="None";
		StartDeploy();
	}

	void OnDisable(){
		animState="None";
	}
	
	// Update is called once per frame
	void Update () {
		HandleAnimation();
		HandleFiring();
		
	}
	
	void LateUpdate(){
		HandleIK();
		if (animState!="Deploy")
			transform.rotation=updateStartRot;
	}

	void HandleIK(){
		//playerIK.SetLHandIK(this,leftHandHold.position);
		//playerIK.SetIKDirections(this,"LeftArm",up:-transform.up);
		//playerIK.SetRArmUp(this,rightHandHold.up);
		//playerIK.SetRHandIK(this,rightHandHold.position);
	}

	public bool AllowControl(IKController ikc,IKRig ikr){
		return true;
	}

	void HandleAnimation(){
		if (animState=="Deploy"){
			HandleDeploying();
			return;
		}
		HandleGunLook();

		HandleReloading();
		HandleCocking();
	}

	void HandleDeploying(){
		if (animState!="Deploy") return;
		
		float deployProgress=(Time.time-deployStart)/deployTime;
		deployProgress=Mathf.Clamp01(deployProgress);
		//deployProgress=Mathf.Pow(deployProgress,2);

		Vector3 tempLocalPos=transform.localPosition;
		if (shouldDeploy){
			tempLocalPos.y=Mathf.SmoothStep(deployDeltaY,0,deployProgress)+originalLocalPos.y;
		}else{
			tempLocalPos.y=Mathf.SmoothStep(0,deployDeltaY,deployProgress)+originalLocalPos.y;
		}
		transform.localPosition=tempLocalPos;

		lookTarget=playerCamera.transform.position+playerCamera.transform.forward*middleDistance;
		if(smoothTime!=0) 
			currentLook=Vector3.SmoothDamp(currentLook, lookTarget, ref lookVelocity, smoothTime);
		else currentLook=lookTarget;
		Vector3 upVector=playerCamera.transform.up;
		currentLookRot=Quaternion.LookRotation(currentLook-transform.position,upVector);
		transform.rotation=currentLookRot;

		float rotLook=Mathf.SmoothStep(deployGunLookAngle,0,shouldDeploy?deployProgress:(1-deployProgress));
		transform.Rotate(rotLook*Vector3.right);

		if (deployProgress==1){
			if (!shouldDeploy){
				gameObject.SetActive(false);
			}
			StartCocking(false);
			cockTimeTaken=-deployWaitForCock;
			//lookVelocity=Vector3.up;
		}
	}

	void HandleReloading(){
		if((InputHandler.GetButtonDown("Reload")||bulletsLeft<=0)&&bulletsLeft!=maxBullets&&animState=="None"){
			animState="Reloading";
			reloadTimeTaken=(bulletsLeft<=0)?(-waitToReload):0;
			animShell.gameObject.SetActive(true);
			reloadStart=Time.time;
			shouldFire=false;
		}
		if (animState=="Reloading"){
			reloadTimeTaken=Mathf.Min(reloadTimeTaken+Time.deltaTime,reloadTimePerShell);
			//Show the 'reloading shell' animation maxBullets times
			if(reloadTimeTaken>=0){
				if(reloadTimeTaken-Time.deltaTime<=0)
					shotgunAudio.PlayOneShot(reloadShellClip);
				float t=reloadTimeTaken/reloadTimePerShell;
				animShell.position=Vector3.Lerp(animShellStart.position,animShellEnd.position,t);
			}

			//If we've finished loading a bullet, start again
			if(reloadTimeTaken>=reloadTimePerShell){
				bulletsLeft++;
				reloadTimeTaken=0;
			}

			//Finish up
			if(bulletsLeft==maxBullets){
				reloadStart=Time.time;
				StartCocking(false);
				animShell.gameObject.SetActive(false);
			}
		}
	}

	void StartCocking(bool ejectBullet=true){
		if (animState=="Cocking") return;
		animState="Cocking";
		cockTimeTaken=0;
		hasBulletToEject=ejectBullet;
		animShell.gameObject.SetActive(false);
	}

	void HandleCocking(){
		if (animState=="Cocking"){
			cockTimeTaken=Mathf.Clamp(cockTimeTaken+Time.deltaTime,-waitToCock,cockTime);

			if (cockTimeTaken>=0 && cockTimeTaken-Time.deltaTime<=0)
				shotgunAudio.PlayOneShot(startCockClip);

			if (cockTimeTaken>cockTime/2){
				//Finished cock?
				if(cockTimeTaken-Time.deltaTime<cockTime/2){
					shotgunAudio.PlayOneShot(endCockClip);
					//Spawn physics shell with force in direction
					if(hasBulletToEject&&PhysicsObjectHandler.currentHandler!=null)
						PhysicsObjectHandler.currentHandler.CreateObject(physicsShell,physicsShellSpawn.position,transform.rotation,transform.TransformDirection(localShellEjectDirection)+player.velocity,Vector3.forward*5);
				}
				//At ctt=ct p=0
				//At ctt=ct/2 p=1
				//(ctt-ct)/(-.5ct)
				cocker.localPosition=startCockPos-handleCockOffset*((cockTime-cockTimeTaken)/(0.5f*cockTime));//Vector3.Lerp(midCockPos,startCockPos,cockTimeTaken*2/cockTime-1f);
				
			}else if(cockTimeTaken>=0)cocker.localPosition=startCockPos-(cockTimeTaken*2/cockTime)*handleCockOffset;

			if(cockTimeTaken>=cockTime){
				animState="None";
			}
		}
	}

	
	void HandleFiring(){
		if (Cursor.lockState==CursorLockMode.None) return;
		shouldFire=shouldFire||(InputHandler.GetButton("Fire")&&animState=="None");
		if (shouldFire&&bulletsLeft>0&&animState=="None"){
			float lookBackMod=((InputHandler.GetButton("Lookback"))&&(animState!="Reloading")&&canLookBack?-1:1);
			currentLookRot=Quaternion.LookRotation( playerCamera.transform.forward*lookBackMod,playerCamera.transform.up);
			transform.rotation=currentLookRot;
			base.Fire();
			bulletsLeft--;
			player.velocity-=playerCamera.transform.forward*lookBackMod*shotgunForce;
			
			//Inform the animation
			StartCocking(true);
			cockTimeTaken=-waitToCock;
			currentRecoilPosAmount=recoilPosAmount;
			targetRecoilRot=recoilRotAmount;
			shouldFire=FinalWorldGen.autoFire;
		}
	}

	void HandleGunLook(){
		//Handle shotgun movement

		//Do a camera rot lerp
		int rotLerpVersion=1;
		if (rotLerpVersion==0){ 
			Quaternion targetLookRot=playerCamera.transform.rotation;
			if (lookAdaptSpeed!=0)
				currentLookRot=Quaternion.RotateTowards(currentLookRot,targetLookRot,lookAdaptSpeed*Time.deltaTime);
			else currentLookRot=targetLookRot;
		}else{
			lookTarget=playerCamera.transform.position+playerCamera.transform.forward*middleDistance;
			if(smoothTime!=0) 
				currentLook=Vector3.SmoothDamp(currentLook, lookTarget, ref lookVelocity, smoothTime);
			else currentLook=lookTarget;
			Vector3 upVector=playerCamera.transform.up;
			currentLookRot=Quaternion.LookRotation(currentLook-transform.position,upVector);
		}
		transform.rotation=currentLookRot;

		//Do lookback
		bool lookingBack=(InputHandler.GetButton("Lookback"))&&(animState!="Reloading")&&canLookBack;
		if(lookingBack)
			timeLookingBack=Mathf.Clamp(timeLookingBack+Time.deltaTime,0,lookBackTime);
		else timeLookingBack=Mathf.Clamp(timeLookingBack-Time.deltaTime,0,lookBackTime);
		float t=timeLookingBack/lookBackTime;
		t = t*t*t * (t * (6f*t - 15f) + 10f);//Smoothing 
		currentLookBackAngle=lookBackAngle*t;
		
		transform.Rotate(currentLookBackAngle*Vector3.right);		

		//Do recoil
		currentRecoilPosAmount=Mathf.Lerp(currentRecoilPosAmount,0,recoilRecovery*Time.deltaTime);
		if (currentRecoilPosAmount<recoilPosAmount/2)
			targetRecoilRot=0;
		currentRecoilRotAmount=Mathf.Lerp(currentRecoilRotAmount,targetRecoilRot,recoilRecovery*Time.deltaTime);
		
		transform.Rotate(-currentRecoilRotAmount*Vector3.right);
		//currentLookRot*=Quaternion.AngleAxis(-currentRecoilRotAmount,Vector3.right);

		//Do reload
		targetZRot=(animState=="Reloading")?reloadTurnAngle:0;
		zRot=Mathf.Lerp(zRot,targetZRot,(Time.time-reloadStart)/rotateToReloadTime);
		transform.Rotate(Vector3.forward*zRot);

		//Update pos
		Vector3 newPos=transform.localPosition;
		newPos=originalLocalPos-Vector3.forward*currentRecoilPosAmount*(lookingBack?-0.5f:1);
		transform.localPosition=newPos;

		updateStartRot=transform.rotation; //This allows us to do look lag
	}

	public void StartDeploy(){
		if (animState=="Deploy") return;
		animState="Deploy";
		deployStart=Time.time;
		if (shouldDeploy) //deploying
			transform.localPosition=originalLocalPos+Vector3.up*deployDeltaY;
		else
			transform.localPosition=originalLocalPos;
	}

	public void Reset(){
		Debug.Log("Shotgun Reset");
		//Reset all animation and ammo
		bulletsLeft=maxBullets;
		cocker.localPosition=startCockPos;
		animShell.gameObject.SetActive(false);
		currentLook=playerCamera.transform.position+playerCamera.transform.forward*middleDistance;
		currentLookRot=Quaternion.LookRotation(currentLook-transform.position,playerCamera.transform.up);
		lookVelocity=Vector3.zero;
		StartDeploy();
	}
}
