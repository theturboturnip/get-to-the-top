using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class CircleMeshGen : MonoBehaviour {
	public float radius;
	public int subdivisions;
	// Use this for initialization
	void Start () {
		GetComponent<MeshFilter>().mesh=GenMesh();
		MeshCollider mc=GetComponent<MeshCollider>();
		if(mc!=null)
			mc.sharedMesh=GetComponent<MeshFilter>().mesh;
	}
	
	// Update is called once per frame
	Mesh GenMesh () {
		//vertex count=subdivisions+1
		//for triangles join adjacent verts
		int vertexCount=subdivisions+1; 
		Vector3[] verts=new Vector3[vertexCount],normals=new Vector3[vertexCount];
		Vector2[] uvs=new Vector2[vertexCount];
		verts[0]=Vector3.zero;
		normals[0]=Vector3.up;
		float theta=0f;//theta in radians
		for(int v=0;v<subdivisions;v++){
			//use radial coords
			theta=v*1.0f/subdivisions*2.0f*Mathf.PI;
			verts[v+1]=new Vector3(Mathf.Sin(theta),0,Mathf.Cos(theta))*radius;
			normals[v+1]=Vector3.up;
		}
		List<int> triangles=new List<int>();
		for(int v=1;v<vertexCount-1;v++){
			triangles.AddRange(new int[]{v,v+1,0});
		}
		triangles.AddRange(new int[]{vertexCount-1,1,0});
		for(int u=0;u<vertexCount;u++){
			uvs[u]=new Vector2(verts[u].x/(2*radius),verts[u].z/(2*radius))-Vector2.one*0.5f;
		}
		Mesh m=new Mesh();
		m.vertices=verts;
		m.normals=normals;
		m.uv=uvs;
		m.SetTriangles(triangles,0);
		return m;
	}
}
