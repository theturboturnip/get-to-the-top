using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class MeshNormalFlattener : MonoBehaviour {
	public float maxY;
	// Use this for initialization
	void Start () {
		Mesh m=GetComponent<SkinnedMeshRenderer>().sharedMesh;
		Vector3[] normals=new Vector3[m.vertexCount];
		for(int i=0;i<m.vertexCount;i++){
			normals[i]=m.normals[i];
			if (m.vertices[i].y<maxY){
				Vector3 newNormal=m.vertices[i];
				newNormal.y=0;
				if (Vector3.Dot(newNormal,normals[i])<0)
					newNormal=-newNormal;
				normals[i]=newNormal.normalized;
			}
			normals[i]=normals[i].normalized;
		}
		m.normals=normals;
		m.UploadMeshData(false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
