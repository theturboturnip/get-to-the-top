using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class MeshNormalsGizmo : MonoBehaviour {
	public MeshFilter mf;
	Mesh m;
	public float length=0.1f;
	void OnEnable(){
		mf=GetComponent<MeshFilter>();
	}
	void Start(){
		OnEnable();
	}
	void OnDrawGizmos(){
		Matrix4x4 trs=transform.localToWorldMatrix;
		m=mf.sharedMesh;
		Gizmos.color=Color.red;
		for(int v=0;v<m.vertexCount;v++){
			Gizmos.DrawLine(trs*m.vertices[v],trs*(m.vertices[v]+m.normals[v]*length));
			Debug.Log(m.normals[v]);
		}
	}
}
