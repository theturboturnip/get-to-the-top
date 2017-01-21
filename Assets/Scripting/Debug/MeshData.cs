using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class MeshData : MonoBehaviour {
	public int polyCount,vertexCount;
	public Vector3 centre,size;
	MeshFilter mf;
	// Use this for initialization
	void Start () {
		mf=GetComponent<MeshFilter>();
	}
	
	// Update is called once per frame
	void Update () {
		polyCount=mf.sharedMesh.triangles.Length/3;
		vertexCount=mf.sharedMesh.vertexCount;
		centre=mf.sharedMesh.bounds.center;
		size=mf.sharedMesh.bounds.size;
	}
}
