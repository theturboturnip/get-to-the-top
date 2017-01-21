using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public enum LevelType{
	NextLevel,
	CurrentLevel,
	MainMenu
}

public class EndUIButton : ButtonCallback {
	public LevelType levelType;
	bool active=false;

	public override void BeenClicked(){
		if (active) return;
			
		int sceneToLoad=-1;
		if (levelType==LevelType.CurrentLevel){
			sceneToLoad=LevelHandler.currentLevel.currentLevelIndex;
			LevelHandler.shouldSkipIntro=true;
		}else if (levelType==LevelType.NextLevel)
			sceneToLoad=LevelHandler.currentLevel.nextLevelIndex;
		LevelHandler.currentLevel.StartLoadingLevel(sceneToLoad);
		active=true;
		//Tell the UI to close
		transform.parent.parent.gameObject.GetComponent<EndLevelUIHandler>().CloseUI();
	}
}
