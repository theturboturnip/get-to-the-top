using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(ReflectionProbe))]
public class FinalBuildingReflControl : MonoBehaviour {
	public float reflUpdateTime,angleIncrementRequired;
	float previousRenderAngle;
	ReflectionProbe[] refls;
	public bool updatePos=true;

	// Use this for initialization
	void Start () {
		refls=GetComponentsInChildren<ReflectionProbe>();
		foreach( ReflectionProbe r in refls)
			r.RenderProbe();

		if (reflUpdateTime<1) reflUpdateTime=1;
		InvokeRepeating("UpdateReflection",0f,reflUpdateTime);
	}
	
	// Update is called once per frame
	void UpdateReflection () {
		if (updatePos)
			transform.position=Vector3.up*Camera.main.transform.position.y;
		Vector2 cameraPolar=WorldGenLib.WorldToPolar(Camera.main.transform.position);
		if (Mathf.Abs(previousRenderAngle-cameraPolar.x)>angleIncrementRequired){
			foreach(ReflectionProbe r in refls)
				r.RenderProbe();
			previousRenderAngle=cameraPolar.x;
		}
	}
}
