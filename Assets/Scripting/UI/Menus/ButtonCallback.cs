using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonCallback : MonoBehaviour {
	Button b;

	// Use this for initialization
	public virtual void Start () {
		b=GetComponent<Button>();
		if (b==null){
			this.enabled=false;
			return;
		}

		b.onClick.AddListener(BeenClicked);

	}
	
	// Update is called once per frame
	public virtual void BeenClicked () {
		Debug.Log("Base Button "+transform+" was clicked!");
	}
}
