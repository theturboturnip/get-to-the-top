using System.Collections;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;




public class UIPopularSeeds : MonoBehaviour {
	public GameObject[] buttons;
	string[] namesToSet,splitSwears;
	string[] timesToSet;

	void Start(){
		string unsplitSwears=((TextAsset)Resources.Load("swears", typeof(TextAsset))).text;
		splitSwears=unsplitSwears.Split(new string[]{"\n"}, StringSplitOptions.RemoveEmptyEntries);

	}

	void OnEnable () {
		//Get first 3 popular seeds
		GTTTNetwork.GetPopularSeeds(5,UpdateSeeds);
	}
	
	// Update is called once per frame
	void UpdateSeeds (string response) {
		string[] responses=response.Split(new string[]{"\n"}, StringSplitOptions.RemoveEmptyEntries);
		List<string> censoredResponses= new List<string>();
		for(int i=0;i<responses.Length;i++){
			if (responses[i]!=CensorString(responses[i])){
				//We know that the string has bad words

			}else{
				censoredResponses.Add(responses[i]);
			}
		}

		namesToSet=new string[censoredResponses.Count];
		timesToSet=new string[censoredResponses.Count];
		string[] strArr;
		for (int i=0;i<censoredResponses.Count;i++){
			//Debug.Log(censoredResponses[i]);
			//Debug.Log(responses[i]);
			strArr=censoredResponses[i].Split();
			//Debug.Log(strArr[0]+","+strArr[1]);
			namesToSet[i]="";
			for (int j=0;j<strArr.Length-1;j++)
				namesToSet[i]+=strArr[j]+" ";
			namesToSet[i]=namesToSet[i].Trim();

			timesToSet[i]=strArr[strArr.Length-1];
		}
	}

	void Update(){
		if (namesToSet!=null && timesToSet!=null){
			//Text[] textComps;
			for (int i=0;i<buttons.Length && i<timesToSet.Length;i++){
				//textComps=buttons[i].GetComponentsInChildren<Text>();
				if (namesToSet[i]==null)
					Debug.Log("nullname "+i);

				buttons[i].transform.GetChild(0).gameObject.GetComponent<Text>().text=CensorString(namesToSet[i]); //PROFANITY PARSING PLZ
				buttons[i].transform.GetChild(1).gameObject.GetComponent<Text>().text="Times played: "+timesToSet[i];
				buttons[i].GetComponent<UIApplySeedButton>().seedToApply=namesToSet[i];
			}
			namesToSet=null;
			timesToSet=null;
		}
	}

	string CensorString(string str){
		string replaceWith;
		foreach(string censorWord in splitSwears){
			replaceWith=censorWord[0]+new string ('*',censorWord.Length-1);
			//Debug.Log(str+","+censorWord+","+replaceWith);
			str=str.Replace(censorWord.Trim(),replaceWith);
			//Debug.Log(str);
		}
		return str;
	}
}
