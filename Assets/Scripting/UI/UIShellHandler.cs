using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIShellHandler : MonoBehaviour {
	public int shellNumber;
	public Shotgun shotgun;
	Component[] images;
	float[] initialAlphas;
	// Use this for initialization
	void Start () {
		images=(gameObject.GetComponentsInChildren(typeof(Image)));
		initialAlphas=new float[images.Length];
		for(int i=0;i<images.Length;i++){
			initialAlphas[i]=((Image)images[i]).color.a;
			//((Image)images[i]).enabled=false;
		}
		//shotgun=(Shotgun)Object.FindObjectOfType(typeof(Shotgun));
	}
	
	// Update is called once per frame
	void Update () {
		if (shotgun==null) return;
		/*if (shotgun==null){
			shotgun=(Shotgun)Object.FindObjectOfType(typeof(Shotgun));
			if (shotgun==null) return;
			//else StartDrawingBullet();
		}*/
		
		if (shotgun.bulletsLeft<shellNumber) 
			DisableBullet();
		else
			EnableBullet();
	}

	void DisableBullet(){
		foreach (Image i in images){
			((Image)i).color=new Color(1,1,1,0);
		}
	}

	void EnableBullet(){
		for(int i=0;i<images.Length;i++){//(Image i in images){
			((Image)images[i]).color=new Color(1,1,1,initialAlphas[i]);
		}
	}
}
