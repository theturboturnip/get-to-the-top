using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EndUIStar : MonoBehaviour {
	public float timeToScale=0.5f;
	public Color startColor,endColor;
	Image i;
	float scaleStartTime=-1;
	Vector3 targetScale;

	// Use this for initialization
	void Start () {
		targetScale=transform.localScale;
		transform.localScale=Vector3.zero;
		i=GetComponent<Image>();
	}

	public void Activate(){
		scaleStartTime=Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		if (scaleStartTime!=-1){
			float p=Mathf.Clamp01((Time.time-scaleStartTime)/timeToScale);
			p=p*p*p;
			transform.localScale=targetScale*p;
			i.color=Color.Lerp(startColor,endColor,p);
		}
	}
}
