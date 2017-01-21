using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ActivateMenuButton : ButtonCallback {
	public GameObject currentMenu,menuToOpen;
	// Use this for initialization
	public override void Start () {
		base.Start();
		
		//if (menuToOpen==null){
		//	this.enabled=false;
		//	return;
		//}

		if (currentMenu==null){
			Debug.Log("No current menu found for "+transform+", using parent.");
			currentMenu=transform.parent.gameObject;
		}
	}
	
	public override void BeenClicked () {
		//menuToOpen.SetActive(true);
		//currentMenu.SetActive(false);
		MenuHandler.current.ChangeMenu(menuToOpen);
	}
}
