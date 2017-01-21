using UnityEngine;
using System.Collections;

/*[RequireComponent(typeof(Renderer))]
public class CameraCuller : MonoBehaviour {
	Renderer r;
	void Start(){
		r=GetComponent<Renderer>();
	}

	void Update(){
		//PreCull(Camera.main);
	}

	/*public void PreCull(Camera c){
		return;
		Vector3 delta=c.transform.position-transform.position;
		if(Vector3.Dot(delta,c.transform.forward)<0)
			r.enabled=false;
		else
			r.enabled=true;
		if(delta.magnitude>c.farClipPlane)
			r.enabled=false;
		else
			r.enabled=true;
	}

	public void OnEnable(){
		Camera.onPreCull+=PreCull;
	}

	public void OnDisable(){
		Camera.onPreCull-=PreCull;
	}
}*/
