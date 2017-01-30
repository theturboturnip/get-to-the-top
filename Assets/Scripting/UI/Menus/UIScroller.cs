using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScroller : MonoBehaviour {
	public RectTransform toScroll;
	public float minY,maxY;

	Scrollbar s;
	// Use this for initialization
	void OnEnable () {
		toScroll.anchoredPosition=Vector2.right*toScroll.anchoredPosition.x;
		s=GetComponent<Scrollbar>();
		s.value=0;
		s.onValueChanged.AddListener(delegate{UpdateRectPos(s.value);});
	}
	
	// Update is called once per frame
	void UpdateRectPos (float value) {
		toScroll.anchoredPosition=Vector2.up*Mathf.Lerp(minY,maxY,value)+Vector2.right*toScroll.anchoredPosition.x;
	}
}
