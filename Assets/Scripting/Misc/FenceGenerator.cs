using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class FenceGenerator : MonoBehaviour {
	static float scale=0.25f;

	// Use this for initialization
	void Start () {
		Material m=GetComponent<MeshRenderer>().material;
		m.SetTextureScale("_MainTex",new Vector2(transform.lossyScale.x,transform.lossyScale.y)*scale);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
