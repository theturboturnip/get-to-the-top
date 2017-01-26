using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameValidator : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Debug.Log(GTTTNetwork.RequestValidate(3));
		Debug.Log(GTTTNetwork.GetCurrentVersion());
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
