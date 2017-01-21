using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartButton : ButtonCallback{
	public override void BeenClicked(){
		LevelHandler.currentLevel.StartLoadingLevel(LevelHandler.currentLevel.currentLevelIndex);
		LevelHandler.shouldSkipIntro=true;
		PauseMenuHandler.current.Unpause();
	}
}

