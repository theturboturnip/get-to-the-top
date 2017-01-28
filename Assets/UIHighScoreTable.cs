using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHighScoreTable : MonoBehaviour {
	Text t;
	string scoreString="";

	void Start(){
		GTTTNetwork.GetCurrentLevelTimes(PopulateTable);
		t=GetComponent<Text>();
		t.text="";
	}
	void Update(){
		if (t.text=="" && scoreString!="")
			t.text=scoreString;
	}

	void PopulateTable(string response){
		if (response[0]!='y') return; 
		//Clip the 'y\n' and append to text
		scoreString="highscores\n"+response.Substring(2);
	}

}
