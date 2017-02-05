using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EndLevelUIHandler : MonoBehaviour {

	public float uiOpenTime;
	public float uiOpenDelay;
	float uiOpenStart=-1;
	bool uiIsOpen=false;
	Vector3 openScale;

	public float uiCloseTime;
	//public float uiCloseDelay;
	float uiCloseStart=-1;

	public float timeDecreaseTime; //The time should decrease in x seconds
	public float timeDecreaseDelay; //Start time decrease in x seconds
	float timeDecreaseStart;
	bool correctTime=false;
	public Text timeText,extraTime,invalidText;

	public EndUIStar star1,star2,star3;
	public int currentActiveStars=0;

	void Start(){
		openScale=transform.localScale;
		transform.localScale=Vector3.zero;
		extraTime.gameObject.SetActive(false);
	}

	public void OpenUI(){
		uiOpenStart=Time.time+uiOpenDelay;
		timeDecreaseStart=Time.time+timeDecreaseDelay+uiOpenTime+uiOpenDelay;
		invalidText.gameObject.SetActive(!LevelHandler.currentLevel.validScore);
	}

	public void CloseUI(){
		uiCloseStart=Time.time;
	}

	void Update(){
		if (!uiIsOpen&&uiOpenStart!=-1){
			//We're opening the UI
			float p=Mathf.Clamp01((Time.time-uiOpenStart)/uiOpenTime);
			//From 0 to 0.5 go from 0 to x*1.1,0 to 0.1z
			//From 0.5 to 1 go to x,z
			//y is straight lerp

			Vector3 newScale=new Vector3(0,openScale.y*p,0);
			if (p<0.5){
				newScale.x=Mathf.SmoothStep(0,openScale.x*1.5f,p*2);
				newScale.z=Mathf.SmoothStep(0,openScale.z*0.1f,p*2);
			}else{
				newScale.x=Mathf.SmoothStep(openScale.x*1.5f,openScale.x,p*2-1);
				newScale.z=Mathf.SmoothStep(openScale.z*0.1f,openScale.z,p*2-1);
			}

			transform.localScale=newScale;
			if (p==1){
				uiIsOpen=true;
				CursorHandler.current.UnlockCursor();
				CursorHandler.current.canLock=false;
			}
		}

		if (uiIsOpen&&uiCloseStart!=-1){
			//We're closing the UI
			float p=1-Mathf.Clamp01((Time.time-uiCloseStart)/uiCloseTime);
			//From 0 to 0.5 go from 0 to x*1.1,0 to 0.1z
			//From 0.5 to 1 go to x,z
			//y is straight lerp

			Vector3 newScale=new Vector3(0,openScale.y*p,0);
			if (p<0.5){
				newScale.x=Mathf.SmoothStep(0,openScale.x*1.5f,p*2);
				newScale.z=Mathf.SmoothStep(0,openScale.z*0.1f,p*2);
			}else{
				newScale.x=Mathf.SmoothStep(openScale.x*1.5f,openScale.x,p*2-1);
				newScale.z=Mathf.SmoothStep(openScale.z*0.1f,openScale.z,p*2-1);
			}

			transform.localScale=newScale;
			if (p==0){
				uiIsOpen=false;
				CursorHandler.current.LockCursor();
			}
		}

		if (!correctTime&&uiIsOpen){
			float p=Mathf.Clamp01((Time.time-timeDecreaseStart)/timeDecreaseTime);

			float startTime=99*60+0.99f; //The clock starts at 99:0.99
			float endTime=LevelHandler.currentLevel.levelCompleteTime;
			float displayTime=Mathf.Lerp(startTime,endTime,p);

			if (p>0 && currentActiveStars==0 && LevelHandler.currentLevel.starRanking>=1){
				star1.Activate();
				currentActiveStars++;
			}else if (p>1f/3f && currentActiveStars==1 && LevelHandler.currentLevel.starRanking>=2){
				star2.Activate();
				currentActiveStars++;
			}else if (p>2f/3f && currentActiveStars==2 && LevelHandler.currentLevel.starRanking>=3){
				star3.Activate();	
				currentActiveStars++;
			}

			//Put the display string on the text mesh
			timeText.text=SecondsToString(displayTime,p);
			if (p==1){
				correctTime=true;
				if (LevelHandler.currentLevel.starRanking==2){
					extraTime.gameObject.SetActive(true);
					extraTime.text=SecondsToString(displayTime-LevelHandler.currentLevel.threeStarTime)+" to next rank";
				}else if (LevelHandler.currentLevel.starRanking==1){
					extraTime.gameObject.SetActive(true);
					extraTime.text=SecondsToString(displayTime-LevelHandler.currentLevel.twoStarTime)+" to next rank";
				}else if (LevelHandler.currentLevel.starRanking==0){
					extraTime.gameObject.SetActive(true);
					extraTime.text=SecondsToString(displayTime-LevelHandler.currentLevel.oneStarTime)+" to next rank";
				}
			}
		}
	}

	string SecondsToString(float time,float p=1){
		int minutes=Mathf.FloorToInt(time/60);
		float seconds=time%60;
		if (p==0)
			seconds=99.99f;
		return ((minutes.ToString("00"))+":"+(seconds.ToString("00.00")));
	}
}
