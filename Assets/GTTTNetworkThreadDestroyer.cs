using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GTTTNetworkThreadDestroyer : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnApplicationQuit(){
		if (GTTTNetwork.networkThread!=null){
			GTTTNetwork.shouldStop=true;
			GTTTNetwork.networkThread.Join();
			GTTTNetwork.networkThread=null;
		}
	}
}
