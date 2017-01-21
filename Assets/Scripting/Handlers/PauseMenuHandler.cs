using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PauseMenuHandler : MenuHandler{
	public KeyCode pauseButton;
	public bool isPaused;
	public GameObject pauseMenuObject,hudObject;
	public static PauseMenuHandler current;
	public static bool canTogglePause=true;
	BlurOptimized blur;

	public override void Start(){
		base.Start();
		current=this;
		blur=(BlurOptimized) FindObjectOfType(typeof(BlurOptimized));
		Unpause();
		ChangeMenu(startMenu);
	}

	public override void Update(){
		base.Update();
		if (LevelHandler.levelComplete){
			hudObject.SetActive(false);
			return;
		}
		if (Input.GetKeyDown(pauseButton)&&canTogglePause){
			if (isPaused) Unpause();
			else Pause();
		}

		#if UNITY_EDITOR
		if (!isPaused && Cursor.lockState!=CursorLockMode.Locked && CursorHandler.current.canLock)
			CursorHandler.current.LockCursor();
		#endif
	}

	public void Pause(){
		Time.timeScale=0;
		CursorHandler.current.UnlockCursor();
		CursorHandler.current.canLock=false;
		isPaused=true;
		//pauseMenuObject.SetActive(true);
		//hudObject.SetActive(false);
		ChangeMenu(pauseMenuObject);
		blur.StartBlur();
		AudioListener.pause=true;
	}

	public void Unpause(){
		Time.timeScale=1;
		CursorHandler.current.LockCursor();
		CursorHandler.current.canLock=true;
		hudObject.SetActive(true);
		isPaused=false;
		pauseMenuObject.SetActive(false);
		if (blur.blurring)
			blur.EndBlur();
		AudioListener.pause=false;
		ChangeMenu(hudObject);
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(PauseMenuHandler))]
class PauseMenuHandlerEditor : Editor{
	PauseMenuHandler ph;
	public override void OnInspectorGUI(){
		DrawDefaultInspector();
		ph=target as PauseMenuHandler;

		EditorGUILayout.Space();
		if (GUILayout.Button("Toggle Pause")){
			if (ph.isPaused) ph.Unpause();
			else ph.Pause();
		}
	}
}
#endif