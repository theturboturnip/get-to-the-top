using UnityEngine;
using System.Collections;

public class CameraOrbit : MonoBehaviour {
	//public BoxCollider toOrbit; 
	public float radius;
	public float spinSpeed=60f; //(Degrees/second)
	public float pitchChangeSpeed=10f; //(Degrees/second)

	// Use this for initialization
	void Start () {
		Camera c = GetComponentInChildren<Camera>();
		if(c==null){
			Debug.LogError("No child camera present!");
			this.enabled=false;
			return;
		}
		c.transform.localPosition=-Vector3.forward*radius;//Mathf.Sqrt((toOrbit.size.x*toOrbit.size.x)/4+(toOrbit.size.z*toOrbit.size.z)/4);
		//transform.position=(toOrbit.transform.position+Vector3.up*toOrbit.size.y/2);
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate(Vector3.up*spinSpeed*Time.deltaTime,Space.World);
		transform.localEulerAngles=new Vector3(ClampAngle(transform.localEulerAngles.x+Input.GetAxisRaw("Vertical")*Time.deltaTime*pitchChangeSpeed,-90,90),transform.localEulerAngles.y,0);
		//transform.Rotate();
	}

	float ClampAngle(float angle,float min,float max){
		if (angle>180) angle-=360;
		return Mathf.Clamp(angle,min,max);
	}
}
