using UnityEngine;
using System.Collections;

public class TransformTarget : MonoBehaviour {
	public Transform toEmulate;
	public float positionSpeed,rotationSpeed,scaleSpeed;
	public bool localPosition,localRotation;
	Vector3 originalPosition,originalScale;
	Quaternion originalRotation;
	//public f emulatePosition,emulateRotation,emulateScale;
	// Use this for initialization
	void Start () {
		if (toEmulate==null){
			this.enabled=false;
			return;
		}
		if (localPosition) originalPosition=transform.localPosition;
		else originalPosition=transform.position;
		if (localRotation) originalRotation=transform.localRotation;
		else originalRotation=transform.rotation;
		originalScale=transform.localScale;
	}
	
	// Update is called once per frame
	void Update () {
		if(positionSpeed!=0){
			Vector3 newPosition=Vector3.Lerp(localPosition?toEmulate.localPosition:toEmulate.position,originalPosition,Time.deltaTime*positionSpeed);
			if (localPosition) transform.localPosition=newPosition;
			else transform.position=newPosition;
		}
		if(rotationSpeed!=0){
			Quaternion newRotation=Quaternion.Slerp(localRotation?toEmulate.localRotation:toEmulate.rotation,originalRotation,Time.deltaTime*rotationSpeed);
			if (localRotation) transform.localRotation=newRotation;
			else transform.rotation=newRotation;
		}
		if(scaleSpeed!=0){
			Vector3 newScale=Vector3.Lerp(toEmulate.localScale,originalScale,Time.deltaTime*scaleSpeed);
			transform.localScale=newScale;
		}
	}
}
