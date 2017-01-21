using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCompletionCheckpoint : MonoBehaviour {
	public TargetHandler[] targets;
	public Transform[] toLift;
	bool activated=false;

	// Use this for initialization
	void Start () {
		LevelObject lo;
		foreach(Transform t in toLift){
			lo=t.gameObject.GetComponent<LevelObject>();
			if (lo==null)
				lo=t.gameObject.AddComponent<LevelObject>();
			t.gameObject.SetActive(false);

		}
	}
	
	// Update is called once per frame
	void Update () {
		if (activated) return;
		activated=true;
		foreach(TargetHandler th in targets){
			activated=activated&&th.hasBeenShot;
		}
		if (activated)
			LevelHandler.currentLevel.LiftObjectSet(toLift);
	}
}
