using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class QuitButton : ButtonCallback {
	public override void BeenClicked(){
		if (MenuHandler.current.animating) return;
		Application.Quit();
		#if UNITY_EDITOR
		EditorApplication.isPlaying=false;
		#endif
	}
}
