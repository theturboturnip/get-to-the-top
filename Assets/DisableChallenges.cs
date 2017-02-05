using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableChallenges : MonoBehaviour {

	// Use this for initialization
	void OnEnable () {
		FinalWorldGen.nightMode=false;
		FinalWorldGen.autoFire=false;	
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
