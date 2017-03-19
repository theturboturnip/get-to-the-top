using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

struct Cloud{
	public Transform t;
	public Texture2D tex;
}

public class CloudSkyHandler : MonoBehaviour {
	public int clumpCount=16,cloudsPerClump;
	public float cloudHeightMin,cloudHeightMax,cloudCrossTime=30,cloudHorizMax,cloudHorizMin,clumpRadius;
	public Mesh quad;
	public Material material;
	public Vector3 cloudScale,windVel;
	public Vector2 texScale;
	public bool moveWithCamera=true,circularPlacement=true,localRotation=false;
	Texture2D tex;

	int texResX,texResY;
	Cloud[] clouds;

	// Use this for initialization
	void Start () {
		texResX=Mathf.RoundToInt(texScale.x);
		texResY=Mathf.RoundToInt(texScale.y);
		GenerateTexture();
		Vector3 clumpPos=Vector3.zero; 
		for (int i=0;i<clumpCount*cloudsPerClump;i++){
			if (i%cloudsPerClump == 0) {
				if (circularPlacement)
					clumpPos=WorldGenLib.PolarToWorld(new Vector2(Random.Range(cloudHorizMin,cloudHorizMax),Random.Range(0,360)))+Vector3.up*Random.Range(cloudHeightMin,cloudHeightMax);
				else
					clumpPos=Vector3.right*Random.Range(-cloudHorizMax,cloudHorizMax)+Vector3.up*Random.Range(cloudHeightMin,cloudHeightMax);
			}
			CreateCloud(i,clumpPos);
		}
	}

	void CreateCloud(int index,Vector3 clumpPos){
		Cloud c=new Cloud();
		GameObject g=new GameObject("Cloud "+index);

		g.transform.parent=transform;
		c.t=g.transform;

		c.tex=tex;
		g.AddComponent<MeshFilter>().sharedMesh=quad;
		g.AddComponent<MeshRenderer>().sharedMaterial=material;
		Material m=g.GetComponent<MeshRenderer>().material;

		g.GetComponent<MeshRenderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;

		m.SetTexture("_MainTex",c.tex);
		m.SetInt("_ClampToBlack",1);
		m.SetVector("_MainTexSize",new Vector4(texResX,texResY,0,0));

		//g.transform.localEulerAngles=Vector3.right*90;
		g.transform.localScale=cloudScale*Random.Range(1,5f);
		g.transform.localPosition=clumpPos+Vector3.right*Random.Range(-clumpRadius,clumpRadius)+Vector3.up*Random.Range(-clumpRadius,clumpRadius);
		if(circularPlacement)
			g.transform.localPosition+=Vector3.forward*Random.Range(-clumpRadius,clumpRadius);//Vector3.right*Random.Range(cloudHorizMin,cloudHorizMax)*-Mathf.Sign(Random.Range(-1,1))+Vector3.forward*Random.Range(cloudHorizMin,cloudHorizMax)*-Mathf.Sign(Random.Range(-1,1))+Vector3.up*cloudHeight;
		if (localRotation)
			g.transform.localRotation=Quaternion.identity;
		g.layer=gameObject.layer;

		clouds[index]=c;
	}

	Texture2D GenerateTexture(){
		tex=new Texture2D(texResX,texResY);
		tex.filterMode=FilterMode.Point;
		//int rectCount=64;
		//int minRectDimension=res/16,maxRectDimension=res/8;
		//int x=0,y,w=texResX,oldEdge;
		FillRect(tex,0,0,texResX,texResY,Color.white);
		//Start at the bottom, choose a random x start and width
		/*for(y=0;y<texResY;y++){
			FillRect(tex,x,y,w,1,Color.white);
			oldEdge=w+x;
			x=Random.Range(x,x+3);
			Debug.Log(x+","+oldEdge);
			w=Random.Range(Mathf.Max(1,oldEdge-x-3),oldEdge-x+1);
			//x has to be inbetween the old x and the old x+w
			//new x+w has to be inbetween new x and old x+w
		}*/
		tex.Apply();
		return tex;
	}

	void FillRect(Texture2D tex, int x, int y, int w, int h, Color fillColor){
		for(int i=x;i<x+w;i++){
			for(int j=y;j<y+h;j++){
					tex.SetPixel(i,j,fillColor);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (circularPlacement) return;
		foreach(Cloud c in clouds){
			c.t.localPosition+=windVel*Time.deltaTime;
			if (Mathf.Abs(c.t.localPosition.x)>cloudHorizMax)
				c.t.localPosition=c.t.localPosition+Vector3.right*(-c.t.localPosition.x+cloudHorizMin);
			//if (Mathf.Abs(c.t.localPosition.y)>cloudHeightMax)
			//	c.t.localPosition-=2*c.t.localPosition.y*Vector3.up;
		}
	}

	void CamPreCull(Camera cam){
		if (cam==Camera.main||cam==null) return;
		foreach(Cloud c in clouds){
			if (circularPlacement)
				c.t.LookAt(cam.transform);
		}
		if (!moveWithCamera) return;
		transform.position=Vector3.up*transform.position.y+cam.transform.position-Vector3.up*cam.transform.position.y;
		transform.localScale=Vector3.one*Mathf.Clamp01(Camera.main.farClipPlane/cloudHorizMax*0.5f);
	}

	void OnEnable(){
		clouds=new Cloud[clumpCount*cloudsPerClump];
		Camera.onPreCull+=CamPreCull;
	}

	void OnDisable(){
		Camera.onPreCull-=CamPreCull;
	}
}
