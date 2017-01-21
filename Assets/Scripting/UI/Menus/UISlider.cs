using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISlider : MonoBehaviour {
	public float min,max,defaultValue;
	public string inputFormat;
	Slider s;
	InputField i;
	public float value;

	public delegate void ValueChangedCallback(float newValue);
	ValueChangedCallback callback;

	// Use this for initialization
	void OnEnable () {
		s=GetComponentInChildren<Slider>();
		s.onValueChanged.AddListener(SliderChanged);
		i=GetComponentInChildren<InputField>();
		i.onValueChanged.AddListener(InputChanged);
		SetValue(value);
	}
	
	// Update is called once per frame
	public void SetValue (float f) {
		if (s==null || i==null) OnEnable();
		f=Mathf.Clamp(f,min,max);
		value=f;
		s.value=(f-min)/(max-min);
		i.text=""+f.ToString(inputFormat);
	}

	void InputChanged(string text){
		try{
			value=float.Parse(text);
		}catch(Exception e){
			return;
		}
		s.value=(value-min)/(max-min);
		if (callback!=null)
			callback(value);
		//SetValue(value);
	}

	void SliderChanged(float v){
		value=Mathf.Lerp(min,max,v);
		if (inputFormat!="")
			i.text=""+value.ToString(inputFormat);
		else
			i.text=""+value;
		if (callback!=null)
			callback(value);
	}

	public void SetValueChangedCallback(ValueChangedCallback newCallback){
		callback=newCallback;
	}
}
