using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class BuildingUVs : MonoBehaviour {

	#if UNITY_EDITOR
	private Vector3 oldScale=Vector3.one;
	#endif

	// Use this for initialization
	void Start () {
		#if UNITY_EDITOR
		oldScale=transform.lossyScale;
		#endif
	}
	
	#if UNITY_EDITOR
	// Update is called once per frame
	void Update () {
		if (!transform.lossyScale.Equals(oldScale)){
			SetUVs();
			oldScale=transform.lossyScale;
		}
	}
	#endif

	void SetUVs(){
		Mesh m=GetComponent<MeshFilter>().mesh;
		List<int[]> edges=BuildingMeshGenLib.IdentifyShapeEdges(m,0,m.vertexCount/2-1);

	}
}
