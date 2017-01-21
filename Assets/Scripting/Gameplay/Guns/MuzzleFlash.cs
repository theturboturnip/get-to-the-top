using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MuzzleFlash : MonoBehaviour {

	public float flashTime,maxmiumZ,baseZIncrease,xyDeviation;
	public Vector3 insideMeshScale;
	public Light flashLight;
	public bool fireOnClick=false;
	Mesh m;
	MeshFilter mf;
	float totalFlashTime=-1;
	Vector3[] startVerts,endVerts,verts;
	Vector3 originalScale;

	// Use this for initialization
	void Start () {
		mf=GetComponent<MeshFilter>();
		m=mf.mesh;
		startVerts=new Vector3[m.vertexCount];
		for(int i=0;i<m.vertexCount;i++){
			startVerts[i]=m.vertices[i]+Vector3.zero; //Ensure the pointer is different
		}

		//Setup inside mesh
		m.subMeshCount++;
		int[] tris=m.GetTriangles(0),newTris=new int[tris.Length];
		verts=m.vertices;
		List<Vector3> newVerts=new List<Vector3>();
		for(int i=0;i<tris.Length;i++){
			newTris[i]=tris[i]+m.vertexCount;
		}
		for(int i=0;i<verts.Length*2;i++){
			newVerts.Add(Vector3.zero);//verts[i%(m.vertexCount)]);
		}
		m.SetVertices(newVerts);
		m.SetTriangles(newTris,1);

		if(flashTime<=0)
			flashTime=0.01f;
		originalScale=transform.localScale;
		//transform.localScale=Vector3.zero;
		if(flashLight!=null) flashLight.enabled=false;

		//StartFlash();
	}
	
	public void StartFlash(){
		totalFlashTime=0;
		verts=m.vertices;

		//Basic muzzle flash creation:
		//	1. All non-negative z verts given an outward z-value ((1-relative dist from center) * maxZ)
		//	2. All non-negative z verts get a random xyz addition
		//At runtime lerp between startVerts and endVerts;
		float centerDist;
		endVerts=new Vector3[m.vertexCount/2];
		for(int i=0;i<m.vertexCount/2;i++){
			endVerts[i]=startVerts[i]+Vector3.zero;
			if (startVerts[i].z<0) continue;
			centerDist=new Vector2(endVerts[i].x,endVerts[i].y).magnitude;
			endVerts[i].z=Mathf.Pow(Mathf.Clamp01(1-centerDist),1)*maxmiumZ+baseZIncrease;
			//endVerts[i].xy*=Random.value*centerDist*2;
			float deviation=centerDist*xyDeviation*Random.value;
			endVerts[i].x*=deviation;
			endVerts[i].y*=deviation;
		}
		if(flashLight!=null) flashLight.enabled=true;
	}

	// Update is called once per frame
	void Update () {
		//TEST ONLY
		if (Input.GetMouseButtonDown(0)&&fireOnClick&&totalFlashTime<0)
			StartFlash();

		if(totalFlashTime<0) return; //Not flashing
		totalFlashTime+=Time.deltaTime;
		
		//Handle Mesh
		float p=(totalFlashTime/flashTime);
		if(p>2){
			p=0;
			totalFlashTime=-1;
			if(flashLight!=null) flashLight.enabled=false;
		}
		if (p>1){
			p=2-p;
		}
		
		verts=m.vertices;
		Vector3 localScale=(new Vector3(p,p,1));
		for(int i=0;i<m.vertexCount;i++){
			verts[i]=Vector3.Lerp(startVerts[i%(m.vertexCount/2)],endVerts[i%(m.vertexCount/2)],p);
			if (i>m.vertexCount/2)
				verts[i]=Vector3.Scale(verts[i],insideMeshScale);
			verts[i]=Vector3.Scale(verts[i],localScale);
		}
		//transform.localScale=Vector3.Scale(new Vector3(p,p,p*p),originalScale);

		m.vertices=verts;
		mf.mesh=m;
		m.UploadMeshData(false);
	}
}
