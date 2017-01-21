using UnityEngine;
using System.Collections;

public class MainMenuButton : ButtonCallback {
	public override void BeenClicked(){
		LevelHandler.currentLevel.StartLoadingLevel(-1);
		PauseMenuHandler.current.Unpause();
	}
}
