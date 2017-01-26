using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelScrollHandler : MonoBehaviour {
	
	//public GameObject scrollbarObject;
	//Scrollbar scrollbar;

	public float buttonHeight=80;
	public Material noStarMat,starMat;
	float totalLevelCount,totalPixelHeight;
	float startY;

	// Use this for initialization
	void Start(){
		OnEnable();
	}
	void OnEnable () {
		startY=transform.localPosition.y;

		totalLevelCount=transform.childCount-1;//LevelData.GetLevelCount();
		int unlockedLevelCount=0;

		GameObject buttonObject;
		Text buttonText;
		LevelProgress lp;
		Vector3[] corners=new Vector3[4];
		for (int i=0;i<totalLevelCount;i++){
			buttonObject=transform.GetChild(i).gameObject;
			//((RectTransform)buttonObject.transform).pivot=new Vector2(0.5f,1);
			//buttonObject.transform.localPosition=Vector2.down*buttonHeight*i;
			buttonText=buttonObject.GetComponentInChildren<Text>();
			buttonText.resizeTextForBestFit=false;
			buttonText.alignment=TextAnchor.MiddleCenter;
			lp=SaveData.GetLevelProgress(i);

			if (lp.unlocked){
				unlockedLevelCount++;
				Level l=LevelData.GetLevel(i);
				buttonText.resizeTextForBestFit=true;
				buttonText.fontSize=17;
				buttonText.lineSpacing=1;
				buttonText.text=l.name;
				if (lp.timeTaken>0){
					buttonText.text+="\nBest Time: "+SecondsToString(lp.timeTaken);
					int star=3;
					foreach(Transform starT in buttonObject.transform.Find("Stars")){
						if ((star==1&&lp.starRanking>=1)||(star==2&&lp.starRanking>=2)||(star==3&&lp.starRanking>=3))
							starT.gameObject.GetComponent<Image>().material=starMat;
						else
							starT.gameObject.GetComponent<Image>().material=noStarMat;
						star--;
					}
				}else{
					buttonObject.transform.Find("Stars").gameObject.SetActive(false);
					((RectTransform)buttonText.transform).anchoredPosition=new Vector2(40,0);
				}
				buttonObject.GetComponent<LevelSelectButton>().levelIndexToLoad=i;
				
			}else{
				buttonObject.GetComponent<Button>().interactable=false;
				buttonObject.GetComponentsInChildren<Image>()[1].enabled=false;
				buttonObject.transform.Find("Stars").gameObject.SetActive(false);
				buttonText.text="???";
				buttonText.fontSize=30;
				((RectTransform)buttonText.transform).anchoredPosition=Vector2.zero;
			}
		}
		totalPixelHeight=buttonHeight*totalLevelCount/2-buttonHeight/2;
		
		/*scrollbar=scrollbarObject.GetComponent<Scrollbar>();
		//if totalPixelHeight<=lossyScale.y return 1
		//if totalPixelHeight=2lossyScale.y return 1/2
		float scrollSize=((RectTransform)scrollbarObject.transform).rect.height*1f/(buttonHeight*totalLevelCount);
		if (scrollSize>=1)
			scrollbarObject.SetActive(false);
		scrollbar.size=scrollSize;
		scrollbar.onValueChanged.AddListener(ScrollbarValueChanged);

		ScrollbarValueChanged(0);*/
	}

	/*public void ScrollbarValueChanged(float value){
		float newY=value*totalPixelHeight;
		((RectTransform)transform).anchoredPosition=Vector2.up*newY;
	}*/

	string SecondsToString(float time,float p=1){
		int minutes=Mathf.FloorToInt(time/60);
		float seconds=time%60;
		if (p==0)
			seconds=99.99f;
		return ((minutes.ToString("00"))+":"+(seconds.ToString("00.00")));
	}
}
