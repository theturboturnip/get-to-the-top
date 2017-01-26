using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameValidator : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GTTTNetwork.RequestValidate(Random.Range(0,100),GTTTNetwork.EchoResponse);
		GTTTNetwork.GetCurrentVersion(GTTTNetwork.EchoResponse);
		//Debug.Log(GTTTNetwork.RequestValidate(3));
		//Debug.Log(GTTTNetwork.GetCurrentVersion());
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
