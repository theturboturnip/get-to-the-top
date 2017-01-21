using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelSelectButton : ButtonCallback {
	public int levelIndexToLoad;

	public override void BeenClicked () {
		if (MenuHandler.current.animating) return;
		LevelHandler.shouldSkipIntro=false;
		GTTTCompleteImageEffect g=Camera.main.transform.gameObject.GetComponent<GTTTCompleteImageEffect>();
		g.fadeDirection=1;
		g.StartFading();
		g.stopWhenFinished=false;
		SceneManager.LoadSceneAsync(LevelData.GetLevel(levelIndexToLoad).sceneIndex);
	}
}
