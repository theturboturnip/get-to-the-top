using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISpeedometer : MonoBehaviour {
	NuPlayer player;
	public Text speedText,unitText;
	public float multiplier=1;
	public string unitString;
	// Use this for initialization
	void Start () {
		player=GameObject.FindWithTag("Player").GetComponent<NuPlayer>();
		unitText.text=unitString;
	}
	
	// Update is called once per frame
	void Update () {
		speedText.text=(player.momentum.magnitude*multiplier).ToString("n0");
	}
}
