using UnityEngine;
using System.Collections;

public class SeaControl : MonoBehaviour {
	[Header("Island Foam")]
	public float seaRadius;
	public float foamCutoff,seaPhaseTime;
	public Vector2 maxFoamScale,bottomLayerOffset;
	public float bottomLayerMovementAmount,maxFoamPriority;
	[Header("Wave Control")]
	public GameObject wavePrefab;
	[Header("Dynamic Skybox Version")]
	public bool makeDynamicSkyboxCopy=true;
	public Mesh plane;
	public Shader dsCopyShader;
	Material currentMat;
	float timeTaken,worldRadius;
	Vector2 originalFoamScale;
	// Use this for initialization
	void Start () {
		worldRadius=WorldData.worldRadius;
		transform.localScale=new Vector3(seaRadius,transform.localScale.y,seaRadius);
		currentMat=GetComponent<MeshRenderer>().material;
		currentMat.SetFloat("_WorldRadius",worldRadius);
		currentMat.SetFloat("_MaxFoamDist",foamCutoff);
		originalFoamScale=currentMat.GetTextureScale("_FoamTex");
		//currentMat.SetVector("_FoamOffset2", new Vector4(1,1,0,0));

		if (makeDynamicSkyboxCopy&&DynamicSkybox.currentSkybox!=null){
			DynamicSkybox ds= DynamicSkybox.currentSkybox;
			GameObject seaCopy=new GameObject("Sea Copy for Dynamic Skybox");
			seaCopy.AddComponent<MeshFilter>().mesh=plane;
			Material matCopy=new Material(dsCopyShader);
			matCopy.color=currentMat.color;
			seaCopy.AddComponent<MeshRenderer>().material=matCopy;
			seaCopy.transform.position=ds.playingAreaToSkybox.MultiplyPoint3x4(transform.position);
			seaCopy.transform.localScale=ds.size;//playingAreaToSkybox.MultiplyVector(transform.localScale);
			seaCopy.transform.parent=ds.transform;
			seaCopy.layer=LayerMask.NameToLayer(ds.skyboxLayer);
		}
	}
	
	// Update is called once per frame
	void Update () {
		//currentMat.SetFloat("_FoamPriority", Mathf.Abs(Mathf.Sin(Time.time)));
		UpdateFoam();
	}

	void UpdateFoam(){
		//Foam priority fluctuates around 0.5
		/*float foamPriority=0.5f;//Mathf.Lerp(0.3f,0.7f,Mathf.Sin(Time.time)/2+0.5f);
		currentMat.SetFloat("_FoamPriority", foamPriority);
		Vector4 topLayerOffset=currentMat.GetTextureOffset("_BottomFoamTex");
		topLayerOffset.x+=windVelocity.x*Time.deltaTime;
		topLayerOffset.y+=windVelocity.y*Time.deltaTime;
		currentMat.SetTextureOffset("_BottomFoamTex",topLayerOffset);*/
		timeTaken+=Time.deltaTime;
		float foamProgress=(timeTaken/seaPhaseTime);
		//currentMat.SetFloat("_FoamPriority",0.5f);
		//Scale bottom layer out 
		//At 0 it's 1
		//At 0.5 it's 2
		//at 1 it's 1
		//1+p/0.5  from 0 to 0.5
		//2-(p-.5)*2
		//The swap between original and max should happen at foamPriority 1 i.e. at foamProgress==0.5
		Vector2 topLayerScale=Vector2.Lerp(originalFoamScale,maxFoamScale,Wrap01(foamProgress-0.5f));//(2-2*(foamProgress-0.5f));
		if (foamProgress<0.5f) currentMat.SetFloat("_FoamPriority",(2*foamProgress)*maxFoamPriority);
		else currentMat.SetFloat("_FoamPriority",(2-2*foamProgress)*maxFoamPriority);

		currentMat.SetTextureOffset("_FoamTex",-topLayerScale/2);
		currentMat.SetTextureScale("_FoamTex",topLayerScale);
		//Oscillate bottom layer around original scale to increase movement
		float bottomLayerScaleSize=(0.5f-foamProgress)%0.5f*4*bottomLayerMovementAmount;
		Vector2 bottomLayerScale=originalFoamScale+new Vector2(bottomLayerScaleSize,bottomLayerScaleSize);
		currentMat.SetTextureOffset("_BottomFoamTex",bottomLayerOffset-bottomLayerScale/2);
		currentMat.SetTextureScale("_BottomFoamTex",bottomLayerScale);
		//
		if(timeTaken>=seaPhaseTime) timeTaken=0;
		
	}

	void SetCircleTexScale(string name,Vector2 scale){
		currentMat.SetTextureScale(name,scale);
		currentMat.SetTextureOffset(name,-scale/2);
	}

	float Wrap01(float toWrap){
		toWrap%=1;
		if (toWrap<0) toWrap++;
		return toWrap;
	}
}
