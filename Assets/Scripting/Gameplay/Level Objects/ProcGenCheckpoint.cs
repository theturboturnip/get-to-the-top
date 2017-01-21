using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcGenCheckpoint : MonoBehaviour {
	public CustomCharacterController playerCollider;
	public NuPlayer playerControl;

	// Use this for initialization
	void Start () {
		playerCollider=(CustomCharacterController)Object.FindObjectOfType(typeof(CustomCharacterController));
		playerControl=playerCollider.gameObject.GetComponent<NuPlayer>();
	}
	
	// Update is called once per frame
	void Update () {

	}
}
