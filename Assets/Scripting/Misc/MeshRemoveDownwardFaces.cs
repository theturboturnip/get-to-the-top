using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshRemoveDownwardFaces : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Mesh m=GetComponent<MeshFilter>().mesh;
		List<int> newTriangles=new List<int>(m.triangles);
		List<int> toRemove=new List<int>();
		for(int t=m.triangles.Length-1-3;t>=0;t-=3){
			//Debug.Log(m.normals[m.triangles[t]]+","+m.normals[m.triangles[t+1]]+","+m.normals[m.triangles[t+2]]);
			if (m.normals[m.triangles[t]].y!=0 || m.normals[m.triangles[t+1]].y!=0 || m.normals[m.triangles[t+2]].y!=0){
				/*newTriangles.RemoveAt(t);
				newTriangles.RemoveAt(t+1);
				newTriangles.RemoveAt(t+2);*/
				if (toRemove.IndexOf(t)==-1) toRemove.Add(t);
				if (toRemove.IndexOf(t+1)==-1) toRemove.Add(t+1);
				if (toRemove.IndexOf(t+2)==-1) toRemove.Add(t+2);
			}
		}
		toRemove.Sort();
		for(int i=toRemove.Count-1;i>=0;i--){
			newTriangles.RemoveAt(toRemove[i]);
		}
		m.triangles=newTriangles.ToArray();
		m.UploadMeshData(false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
