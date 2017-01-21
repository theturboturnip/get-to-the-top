using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UnpauseButton : ButtonCallback {
	public override void BeenClicked(){
		PauseMenuHandler.current.Unpause();
	}
}
