using UnityEngine;
using System.Collections;

public enum Allegiance{
	Player=0,
	Enemy=1
};

public class Bullet : MonoBehaviour {
	public Allegiance allegiance=Allegiance.Player;
	public Vector3 velocity;
	public float lifetime,radius,damage;
	public Vector3 decalTexScale;
	public Texture2D decalTex;

	float totalLifeTime=0;

	CustomCharacterController playerControl;

	// Use this for initialization
	void Start () {
		playerControl=GameObject.FindWithTag("Player").GetComponent<CustomCharacterController>();		
	}
	
	// Update is called once per frame
	void Update () {
		//Handle lifetime
		totalLifeTime+=Time.deltaTime;
		if (totalLifeTime>lifetime&&lifetime>0)
			Destroy(gameObject);

		//Handle movement
		Vector3 newPos=transform.position+velocity*Time.deltaTime;
		RaycastHit rh;
		bool hit=false;
		hit=Physics.Raycast(transform.position,(newPos-transform.position).normalized,out rh,(newPos-transform.position).magnitude);
		if (hit){
			if (rh.transform.gameObject.tag=="Target"){
				rh.transform.parent.gameObject.GetComponent<TargetHandler>().GetShot();
			}
			if (rh.transform.gameObject.tag=="IgnoreBullet") return;
			if(DecalHandler.currentHandler!=null && decalTex!=null)
				DecalHandler.currentHandler.CreateDecal(rh.point+rh.normal*Random.Range(0f,0.01f),rh.normal,decalTex,decalTexScale,rh.transform,true);
			TrailRenderer tr=GetComponent<TrailRenderer>();
			if (tr==null)
				Destroy(gameObject);
			else{
				totalLifeTime=0;
				lifetime=tr.time;
				velocity=Vector3.zero;
			}
		}
		transform.position=newPos;
	}
}
