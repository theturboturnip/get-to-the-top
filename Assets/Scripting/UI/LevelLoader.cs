using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelLoader : MonoBehaviour {
	public int KeyLoad=1;
	public KeyCode loadKey;
	
	void Update(){
		if (Input.GetKeyDown(loadKey))
			LoadLevel(KeyLoad);
	}

	public void LoadLevel(int id){
		SceneManager.LoadScene(id);
	}
}
