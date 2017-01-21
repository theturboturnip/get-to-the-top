using UnityEngine;
using System.Collections;

public class LevelObject : MonoBehaviour {
	public float targetY,timeToY,timeDelay;
	public float startY;
	float timeSum;
	public bool canBeDestroyed=false,moveImmediately=false;
	bool moving=false;
	// Use this for initialization
	void Start () {
		//startY=transform.position.y;
		//targetY=transform.position.y;
		if (moveImmediately)
			StartMoving();
	}

	public void StartMoving(){
		//startY=transform.position.y;
		timeSum=-timeDelay;
		if (timeToY<=0)
			timeToY=1;
		moving=true;
	}

	public void StartMovingDown(){
		targetY=startY;
		StartMoving();
	}
	
	// Update is called once per frame
	void Update () {
		if(!moving) return;
		timeSum+=Time.deltaTime;
		float t=Mathf.Min(timeSum/timeToY,1);
		t=1-t;
		t = t*t;//*t * (t * (6f*t - 15f) + 10f); //Smoothing (Optional)
		t=1-t;
		if (t==1) moving=false;
		Vector3 newPos=transform.position;
		newPos.y=Mathf.Lerp(startY,targetY,t);
		if (t<0)
			newPos.y=startY;
		transform.position=newPos;
	}
}
