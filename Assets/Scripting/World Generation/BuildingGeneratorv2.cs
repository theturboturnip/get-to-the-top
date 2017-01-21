using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
//using System;



[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class BuildingGeneratorv2 : MonoBehaviour {
	public Mesh buildingShape;
	//public GameObject window;
	//public bool generateWindows=false;
	//public float windowGap=0;
	public Vector3 bounds;
	public Vector2 sideUVOffsetPerUnit=Vector2.one,roofUVOffsetPerUnit=Vector2.one;
	public bool generateUVs=false,generateCollider=true,useBoxCollider=true,topRelative=false;//if topRelative, the roof is placed at y=0
	public Mesh overrideMesh;
	//[ReadOnly] public int vertexCount=0; 
	//public int numOfFloors;
	//public float floorHeight;
	//public bool generateFloorsWithHeight=false;
	int[][] edges;

	// Use this for initialization
	void Start () {

		//DestroyAllChildren();
		transform.localScale=Vector3.one;
		Mesh g = Generate(buildingShape);
		MeshFilter mf=GetComponent<MeshFilter>();
		//if(mf==null) mf=gameObject.AddComponent<MeshFilter>();
		mf.mesh=g;
		//actual color is rgb 0,0.12,1
		//invert color is rgb 1,0.78,0
		//c=lerp(white,actual color,s)*value
		float newSaturation=Random.value*0.1f+0.9f,newValue=Random.value*0.2f+0.8f;
		Color c=Color.Lerp(Color.white,new Color(0,0.12f,1),newSaturation)*newValue;
		#if UNITY_EDITOR
		if (EditorApplication.isPlaying){
		#endif
		GetComponent<MeshRenderer>().material.SetColor("_DiffuseColor",c);	//Color.Lerp(Color.white,Color.grey,Random.value-0.5f));
		#if UNITY_EDITOR
		}
		#endif

		if(generateCollider){
			if(useBoxCollider){
				BoxCollider bc= GetComponent<BoxCollider>();
				if (bc==null) bc=gameObject.AddComponent<BoxCollider>();
				bc.center=g.bounds.center;
				bc.size=g.bounds.size;
			}else{
				Debug.Log("Mesh collider");
				MeshCollider mc=GetComponent<MeshCollider>();
				if (mc!=null)
				mc.sharedMesh=g;
			}
		}
//		vertexCount=g.vertexCount;
	}

	void DestroyAllChildren(){
		var children = new List<GameObject>();
		foreach (Transform child in transform) children.Add(child.gameObject);
		#if UNITY_EDIOR
		children.ForEach(child => DestroyImmediate(child));
		#else
		children.ForEach(child => DestroyImmediate(child));
		#endif
	}

	Mesh Generate(Mesh shape_){
		if (overrideMesh!=null)
			return overrideMesh;
		return BuildingMeshGenLib.GenerateBuildingMesh(shape_,bounds,generateUVs,sideUVOffsetPerUnit,roofUVOffsetPerUnit,isTopRelative:topRelative);
	}

	
	/*static List<Vector3> AddRangeWithOffset(this List<Vector3> me,Vector3[] range,Vector3 offset){
		foreach(Vector3 v in range){
			me.Add(v+offset);
		}
		return me;
	}
	static List<int> AddRangeWithOffset(this List<int> me,int[] range,int offset){
		foreach(int v in range){
			me.Add(v+offset);
		}
		return me;
	}*/

	

	static float RoundTo(float target,float roundTo){
		if (roundTo==0) return target;
		return Mathf.Floor(target/roundTo)*roundTo;
	}

	public void Regen(){
		Start();
	}
	
}

#if UNITY_EDITOR
[CustomEditor(typeof(BuildingGeneratorv2))]
[CanEditMultipleObjects]
class BuildingGeneratorv2Editor : Editor{
	//BuildingGeneratorv2 bg;

	public override void OnInspectorGUI() {
		//return;
		//bg=target as BuildingGeneratorv2;

		DrawDefaultInspector();
		EditorGUILayout.Space();
		if (GUILayout.Button("Recalculate")){
			foreach(Object bg in targets)
				((BuildingGeneratorv2)bg).Regen();
		}
		
	}
}
#endif