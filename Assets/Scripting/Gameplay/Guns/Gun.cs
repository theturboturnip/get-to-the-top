using UnityEngine;
using System.Collections;

public enum GunFireType{
	SingleShot=0,
	Spray=1
};

public class Gun : MonoBehaviour {

	public MuzzleFlash muzzleFlash;
	public GunFireType fireType;
	public AudioClip fireClip;
	AudioSource fireSource;
	public Transform bulletExit;

	//public float coneAngle;
	public float coneRadius;//Radius at 1m away
	public int pelletCount;
	public float pelletSpeed,pelletLife,pelletDamage;
	public Texture2D[] bulletHoles;
	public Vector3 bulletHoleScale;
	public Vector3 pelletScale;
	public GameObject pelletPrefab;

	public Allegiance allegiance=Allegiance.Enemy;

	public virtual void Start(){
		if (bulletExit==null)
			bulletExit=transform;
		//if (fireSource==null)
			fireSource=GetComponent<AudioSource>();
		if (fireType==GunFireType.SingleShot){
			coneRadius=0;
			pelletCount=1;
		}
	}

	public void Fire(){
		if (muzzleFlash!=null)
			muzzleFlash.StartFlash();
		if (fireSource!=null && fireClip!=null)
			fireSource.PlayOneShot(fireClip);
		
		//Fire a spray
		//Generate points on unit circle
		//Fire through those points
		GameObject pellet;
		Bullet b;
		for(int i=0;i<pelletCount;i++){
			//Create bullet at correct position
			Vector2 pointInCircle=Random.insideUnitCircle*coneRadius;
			if (i==0)
				pointInCircle=Vector2.zero;
			Vector3 worldSpacePoint=bulletExit.TransformPoint(new Vector3(pointInCircle.x,pointInCircle.y,1));
			pellet=Instantiate(pelletPrefab,bulletExit.position,bulletExit.rotation) as GameObject;
			pellet.transform.localScale=Vector3.Scale(pellet.transform.localScale,pelletScale);

			//Set essential bullet properties 
			b=pellet.GetComponent<Bullet>();
			b.velocity=(worldSpacePoint-bulletExit.position).normalized*pelletSpeed;
			b.lifetime=pelletLife;
			b.allegiance=allegiance;
			b.damage=pelletDamage;

			if (bulletHoles==null) continue;
			if (bulletHoles.Length==0) continue;
			b.decalTexScale=bulletHoleScale;
			b.decalTex=bulletHoles[Random.Range(0,bulletHoles.Length)];
		}
	}
}
