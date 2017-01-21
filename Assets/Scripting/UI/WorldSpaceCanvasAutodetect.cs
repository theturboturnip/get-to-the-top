using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WorldSpaceCanvasAutodetect : MonoBehaviour {
	Canvas uiCanvas;
	// Use this for initialization
	void Start () {
		uiCanvas=GetComponent<Canvas>();
		uiCanvas.worldCamera=Camera.main;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
