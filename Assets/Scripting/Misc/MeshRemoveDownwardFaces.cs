using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshRemoveDownwardFaces : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Mesh m=GetComponent<MeshFilter>().mesh;
		List<int> newTriangles=new List<int>(m.triangles);
		for(int t=m.triangles.Length-1-3;t>=0;t-=3){
			//Debug.Log(m.normals[m.triangles[t]]+","+m.normals[m.triangles[t+1]]+","+m.normals[m.triangles[t+2]]);
			if (m.normals[m.triangles[t]].y<0 || m.normals[m.triangles[t+1]].y<0 || m.normals[m.triangles[t+2]].y<0){
				newTriangles.RemoveAt(t);
				newTriangles.RemoveAt(t+1);
				newTriangles.RemoveAt(t+2);
			}
		}
		m.triangles=newTriangles.ToArray();
		m.UploadMeshData(false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
