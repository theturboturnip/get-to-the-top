using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class BuildingGenerator : MonoBehaviour {
	public Vector3 size;
	public Mesh slice,roof;
	public bool useBoxCollider=true;
	public float sliceHeightScale;
	//public GameObject slice; //Prefab with bones
	//public GameObject roof; //Prefab with bones

	void Start(){
		Mesh buildingMesh=Generate();
		GetComponent<MeshFilter>().mesh=buildingMesh;
		if(useBoxCollider){
			BoxCollider bc=GetComponent<BoxCollider>();
			if(bc==null)bc=gameObject.AddComponent<BoxCollider>();
			bc.center=Vector3.up*buildingMesh.bounds.size.y/2;
			bc.size=buildingMesh.bounds.size;
		}
	}
	
	public Mesh Generate(){
		//You can't have half-slices 
		slice.RecalculateBounds();
		roof.RecalculateBounds();
		float sliceHeight=slice.bounds.size.y*sliceHeightScale,roofHeight=roof.bounds.size.y;
		int sliceNum=(int)((size.y-roofHeight)/sliceHeight);
		Debug.Log(sliceNum+","+roofHeight+","+sliceHeight);
		//Pre-deform slice and roof mesh given width+depth
		Mesh deformedSlice=DeformSlice(slice,true),deformedRoof=DeformSlice(roof);
		//Squish meshes on top of each other
		//Start from height/2
		List<Vector3> vertices=new List<Vector3>();
		List<Vector3> normals=new List<Vector3>();
		List<Vector2> uvs=new List<Vector2>();
		List<int> triangles=new List<int>();
		List<int> roofTriangles=new List<int>();
		int triangleOffset=0;
		for(Vector3 slicePos=Vector3.up*sliceHeight/2;slicePos.y<sliceHeight*sliceNum;slicePos.y+=sliceHeight){
			Debug.Log("Adding slice!");
			foreach(Vector3 v in deformedSlice.vertices)
				vertices.Add(v+slicePos);
			normals.AddRange(deformedSlice.normals);
			uvs.AddRange(deformedSlice.uv);
			foreach(int t in deformedSlice.triangles){
				triangles.Add(t+triangleOffset);
			}
			triangleOffset+=slice.vertexCount;//.Length;
		}
		Vector3 roofOffset=Vector3.up*(sliceHeight*sliceNum+roofHeight/2);
		foreach(Vector3 v in deformedRoof.vertices){
			vertices.Add(v+roofOffset);
		}
		normals.AddRange(deformedRoof.normals);
		uvs.AddRange(deformedRoof.uv);
		foreach(int t in deformedRoof.triangles){
			roofTriangles.Add(t+triangleOffset);
		}
		Mesh m=new Mesh();
		m.subMeshCount=2;
		m.SetVertices(vertices);
		m.SetNormals(normals);
		m.SetTriangles(triangles,0);
		m.SetTriangles(roofTriangles,1);
		m.SetUVs(0,uvs);
		m.RecalculateBounds();
		;
		m.UploadMeshData(false);
		return m;
	}

	Mesh DeformSlice(Mesh m,bool scaleUp=false){
		Mesh deformed=new Mesh();
		List<Vector3> vertices=new List<Vector3>();
		Vector3 nonHeightSize=size-Vector3.up*Vector3.Dot(size,Vector3.up)+Vector3.up*(scaleUp?sliceHeightScale:1);
		Vector3 nonHeightOldSize=new Vector3(m.bounds.size.x,1,m.bounds.size.z);
		foreach(Vector3 vertex in m.vertices){
			vertices.Add(MultiplyVectors(DivideVectors(vertex,nonHeightOldSize),nonHeightSize));
		}
		deformed.SetVertices(vertices);
		deformed.triangles=m.triangles;
		deformed.uv=m.uv;
		deformed.normals=m.normals;
		deformed.RecalculateBounds();
		;
		return deformed;
	}

	Vector3 MultiplyVectors(Vector3 one,Vector3 two){
		return new Vector3(one.x*two.x,one.y*two.y,one.z*two.z);
	}

	Vector3 DivideVectors(Vector3 one,Vector3 two){
		return new Vector3(one.x/two.x,one.y/two.y,one.z/two.z);
	}
}
