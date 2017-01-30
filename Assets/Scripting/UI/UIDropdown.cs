using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDropdown : MonoBehaviour {
	public string[] options;
	public int value;
	public Sprite verticalTop,verticalMiddle,verticalBottom,normal;
	public GameObject buttonPrefab;
	Text mainText;
	RectTransform optionButtonParent;
	bool open=false;
	// Use this for initialization
	void Start () {
		SetOptions(options);
		GetComponent<Button>().onClick.AddListener(ToggleDropdown);
		transform.GetChild(0).gameObject.GetComponent<Text>().text=options[value];
	}

	public void SetOptions(string[] newOptions){
		options=newOptions;
		if (optionButtonParent!=null){
			Destroy(optionButtonParent.GetComponent<GraphicRaycaster>());
			Destroy(optionButtonParent.GetComponent<Canvas>());
			Destroy(optionButtonParent.gameObject);
		}

		optionButtonParent=new GameObject("Options", typeof(RectTransform)).GetComponent<RectTransform>();
		optionButtonParent.SetParent(transform,false);
		Canvas optionCanvas=optionButtonParent.gameObject.AddComponent<Canvas>();
		optionCanvas.overrideSorting=true;
		optionCanvas.sortingOrder=3000;
		optionButtonParent.gameObject.AddComponent<GraphicRaycaster>();
		GameObject button;
		for (int i=0;i<options.Length;i++){
			//Instantiate a buttonPrefab
			button=(GameObject)Instantiate(buttonPrefab,optionButtonParent);
			button.transform.localRotation=Quaternion.identity;
			button.transform.localScale=Vector3.one;
			//Set the image correctly, and text
			if (i<options.Length-1)
				button.GetComponent<Image>().sprite=verticalMiddle;
			else
				button.GetComponent<Image>().sprite=verticalBottom;
			button.GetComponentInChildren<Text>().text=options[i];
			//button.GetComponent<Button>().onClick.AddListener(delegate{SelectOption(i);});
			int tempI=i;
			button.GetComponent<Button>().onClick.AddListener(() => SelectOption(tempI));
		}
		optionButtonParent.anchorMin=optionButtonParent.anchorMax=Vector2.right*0.5f;
		optionButtonParent.pivot=Vector2.right*0.5f+Vector2.up;
		optionButtonParent.localPosition=Vector2.zero;
		optionButtonParent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,OrganiseChildren(optionButtonParent));
		optionButtonParent.anchoredPosition=Vector2.zero;
		optionButtonParent.localScale=Vector3.one;
		optionButtonParent.localRotation=Quaternion.identity;
		DestroyDropdown();

	}

	float OrganiseChildren(RectTransform r){
		float height=0;
		//if (r.parent==scrollParent)
		//height-=r.rect.height;
		RectTransform childR;
		foreach(Transform t in r){
			childR=(RectTransform)t;
			childR.anchorMin=childR.anchorMax=Vector2.right*0.5f+Vector2.up;

			childR.pivot=Vector2.right*0.5f+Vector2.up;
			childR.localPosition=Vector2.up*height;
			height-=childR.rect.height;
		}
		return -height;
	}

	void ToggleDropdown(){
		open=!open;
		if (open) CreateDropdown();
		else DestroyDropdown();
	}
	
	// Update is called once per frame
	void CreateDropdown () {
		//Set our image to verticalTop
		GetComponent<Image>().sprite=verticalTop;
		//Enable child buttons
		optionButtonParent.gameObject.SetActive(true);
		open=true;
	}

	void DestroyDropdown() {
		//Set our image to normal 
		GetComponent<Image>().sprite=normal;
		//Disable children Buttons
		optionButtonParent.gameObject.SetActive(false);
		open=false;
	}

	void SelectOption(int newValue){
		value=newValue;
		transform.GetChild(0).gameObject.GetComponent<Text>().text=options[value];
		DestroyDropdown();
	}
}
