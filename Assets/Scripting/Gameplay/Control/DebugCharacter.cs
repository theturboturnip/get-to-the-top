using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCharacter : MonoBehaviour {
	public Vector3 initialVelocity;
	CustomCharacterController c;
	Vector3 velocity;
	// Use this for initialization
	void Start () {
		c=GetComponent<CustomCharacterController>();
		c.debugMove=Vector3.zero;
		velocity=initialVelocity;
	}
	
	// Update is called once per frame
	void Update () {
		c.Move(velocity*Time.deltaTime);
		velocity=c.velocity;
	}
}
