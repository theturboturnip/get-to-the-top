using UnityEngine;
using System.Collections;

public class LookTarget : MonoBehaviour {
	public Vector3 lookTarget;
	public float smoothTime;
	Vector3 actualTarget;
	Vector3 velocity;
	Quaternion lookRot;
	// Use this for initialization
	void Start () {
		actualTarget=lookTarget;
	}
	
	// Update is called once per frame
	void Update () {
		if(smoothTime!=0) 
			actualTarget=Vector3.SmoothDamp(actualTarget, lookTarget, ref velocity, smoothTime);
		else actualTarget=lookTarget;
		//lookRot=Quaternion.LookRotation((actualTarget-transform.position).normalized);
		transform.LookAt(actualTarget);
		//transform.rotation=Quaternion.Lerp(transform.rotation,lookRot,Time.deltaTime/smoothTime);
	}
}
