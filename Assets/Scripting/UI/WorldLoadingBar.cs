using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WorldLoadingBar : MonoBehaviour {
	Slider s;
	public GameObject StartMenuUI,GameUI;
	// Use this for initialization
	void Start () {
		s=GetComponent<Slider>();
	}
	
	// Update is called once per frame
	void Update () {
		s.value=FinalWorldGen.buildProgress;
		if (s.value>=1){
			//GameUI.SetActive(true);
			enabled=false;
			//StartMenuUI.SetActive(false);
			//CursorHandler.current.LockCursor();
		}
	}
}
