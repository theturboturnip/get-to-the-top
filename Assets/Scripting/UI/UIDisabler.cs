using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIDisabler : MonoBehaviour {
	public KeyCode toggleKey;
	public bool isEnabled=true;
	Graphic i;
	// Use this for initialization
	void Start () {
		i=GetComponent<Graphic>();
		UpdateComponents();
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(toggleKey)){
			isEnabled=!isEnabled;
			UpdateComponents();
		}
	}

	void UpdateComponents(){
		i.enabled=isEnabled;
		foreach(Transform child in transform){
			child.gameObject.SetActive(isEnabled);
		}
	}
}
