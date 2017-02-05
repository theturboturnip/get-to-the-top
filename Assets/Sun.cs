using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Light[] lights=(Light[])FindObjectsOfType(typeof(Light));
		foreach(Light l in lights){
			if (l.type==LightType.Directional)
				transform.rotation=l.transform.rotation;
		}

	}
	
	// Update is called once per frame
	void Update () {
		transform.position=Camera.main.transform.position-transform.forward*1000;
	}
}
