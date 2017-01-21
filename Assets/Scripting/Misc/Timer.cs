#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System;

public class Timer {
	DateTime start;
	public TimeSpan timeTaken;
	public float timeTakenMS;
	public Timer(bool autoStart=true){
		if(autoStart) Start();
		//start=DateTime.UtcNow;
	}
	public void Start(){
		start=DateTime.UtcNow;
	}
	public float Stop(bool shouldPrint=false){
		timeTaken=(DateTime.UtcNow-start);
		timeTakenMS=(float)timeTaken.TotalMilliseconds;
		if(shouldPrint)
			Debug.Log("Timer stopped in "+timeTakenMS+"ms.");
		return timeTakenMS;
	}
}
#endif