using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CursorHandler : MonoBehaviour {
	public KeyCode[] unlockKeys=new KeyCode[]{KeyCode.Escape};
	public bool startLocked=true,canLock=true;
	public static CursorHandler current;

	// Use this for initialization
	void Start () {
		if (current!=null&&current!=this){
			this.enabled=false;
			return;
		}
		if(startLocked) LockCursor();
		else UnlockCursor();
		current=this;
	}

	void OnEnable(){
		Start();
	}
	
	// Update is called once per frame
	void Update () {
		/*foreach(KeyCode k in unlockKeys){
			if(Input.GetKeyDown(k))
				UnlockCursor();
		}*/
		if (Cursor.visible&&Input.GetMouseButtonDown(0)&&(EventSystem.current==null||!EventSystem.current.IsPointerOverGameObject())&&canLock)
			LockCursor();
		///=
	}

	public void LockCursor(){
		Cursor.lockState=CursorLockMode.Locked;
		Cursor.visible=false;
	}

	public void UnlockCursor(){
		Cursor.lockState=CursorLockMode.None;
		Cursor.visible=true;
	}
}
