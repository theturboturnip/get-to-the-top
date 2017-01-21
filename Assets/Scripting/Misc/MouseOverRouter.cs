using UnityEngine;
using System.Collections;

public class MouseOverReceiver : MonoBehaviour{
	public virtual void OnMouseOver(){}
	public virtual void OnMouseEnter(){}
	public virtual void OnMouseExit(){}
}

public class MouseOverRouter : MonoBehaviour {
	public MouseOverReceiver routeTo;
	
	void OnMouseOver(){
		Debug.Log("MOUSE OVER");
		routeTo.OnMouseOver();
	}

	void OnMouseEnter(){
		routeTo.OnMouseEnter();
	}

	void OnMouseExit(){
		routeTo.OnMouseExit();
	}
}
