using UnityEngine;
using System.Collections;

public class CloudLayerHandler : MonoBehaviour {
	public float minFlowSpeed,maxFlowSpeed;
	float minUVFlowSpeed,maxUVFlowSpeed;
	public float minTexSize,maxTexSize;
	float minTexScale,maxTexScale;
	public float baseAlpha,cloudAlpha;
	public int layerAmount=5;
	public Mesh quadMesh;
	public int noiseRes;
	public bool autoAdjustHeight=true,moveClouds=true,debugMode=false;
	public int cloudSeed=-1;

	public Vector2 windDir;
	Vector2 posOffset;
	static string texName="_MainTex";
	Texture2D noiseTex;

	Material[] cloudMaterials;
	public Material sharedMat;
	// Use this for initialization
	void Start () {
		if (LevelHandler.currentLevel!=null&&autoAdjustHeight){
			transform.position=Vector3.up*(LevelHandler.currentLevel.cloudLevel+1);
		}
		transform.localScale=new Vector3(transform.localScale.x,1,transform.localScale.x);
		cloudMaterials=new Material[layerAmount];
		if (cloudSeed!=-1)
			Random.seed=cloudSeed;
		/*int i=0;
		foreach(Transform layer in transform){
			cloudMaterials[i]=layer.gameObject.GetComponent<Renderer>().material;
			if (i==0)
				sharedMat=layer.gameObject.GetComponent<Renderer>().sharedMaterial;
			i++;
		}*/
		GameObject layerObj;
		for (int i=0;i<layerAmount;i++){
			layerObj=new GameObject("Layer "+(1+i));
			layerObj.layer=gameObject.layer;
			layerObj.transform.parent=transform;
			layerObj.transform.localEulerAngles=Vector3.right*90;
			layerObj.transform.localScale=Vector3.one;
			layerObj.transform.localPosition=Vector3.down*i;
			layerObj.AddComponent<MeshFilter>().sharedMesh=quadMesh;
			layerObj.AddComponent<MeshRenderer>().sharedMaterial=sharedMat;
			cloudMaterials[i]=layerObj.GetComponent<MeshRenderer>().material;
		}

		if (windDir.magnitude==0&&!debugMode)
			windDir=RandomVec2().normalized;

		minUVFlowSpeed=minFlowSpeed/transform.localScale.x;
		maxUVFlowSpeed=maxFlowSpeed/transform.localScale.x;

		minTexScale=minTexSize/transform.localScale.x;
		maxTexScale=maxTexSize/transform.localScale.x;

		noiseTex=CreateSkyNoiseTexture(noiseRes);

		InitClouds();
	}
	
	void InitClouds(){
		int i=0;
		foreach(Material m in cloudMaterials){
			m.SetTexture(texName,noiseTex);
			m.SetVector(texName+"Size",new Vector4(noiseRes,noiseRes,0,0));
			m.SetTextureScale(texName,SingleValVector2(1f/Mathf.Lerp(minTexScale,maxTexScale,(cloudMaterials.Length-i)*1f/cloudMaterials.Length)));
			if (debugMode) continue;
			m.SetTextureOffset(texName,RandomVec2());
			m.SetColor("_CloudColor",new Color(1,1,1,cloudAlpha));
			m.SetColor("_BaseColor",new Color(1,1,1,0));
		}
		if (!debugMode)
			cloudMaterials[0].SetColor("_BaseColor",new Color(1,1,1,baseAlpha));
	}

	// Update is called once per frame
	void Update () {
		int i=0;
		Vector2 currentOffset,velocity;
		foreach(Material m in cloudMaterials){
			velocity=Mathf.Lerp(minUVFlowSpeed,maxUVFlowSpeed,i*1.0f/cloudMaterials.Length)*windDir;
			currentOffset=m.GetTextureOffset(texName);
			m.SetTextureOffset(texName,currentOffset+velocity*Time.deltaTime);
			i++;
		}
	}

	Vector2 RandomVec2(){
		return new Vector2(Random.value,Random.value);
	}

	Vector2 RandomSingleValVector2(){
		return SingleValVector2(Random.value);
	}

	Vector2 SingleValVector2(float value){
		return new Vector2(value,value);
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.blue;
		Gizmos.matrix=transform.localToWorldMatrix;
		Gizmos.DrawWireCube(Vector3.zero,Vector3.one);
	}

	Texture2D CreateSkyNoiseTexture(int res){
		Texture2D tex=new Texture2D(res,res);
		tex.filterMode=FilterMode.Point;
		int rectCount=64;
		int minRectDimension=res/16,maxRectDimension=res/8;
		int x,y,w,h;
		float noiseDensity=0.5f;
		FillRect(tex,0,0,res,res,new Color(0,0,0,0));
		FillRectWithNoise(tex,0,0,res,res,Color.white,0.2f);
		for (int r=0;r<rectCount;r++){
			w=Random.Range(minRectDimension,maxRectDimension);
			h=Random.Range(minRectDimension,maxRectDimension);
			x=Random.Range(0,res-w);
			y=Random.Range(0,res-h);
			FillRectWithNoise(tex,x,y,w,h,Color.white,noiseDensity);
			FillRect(tex,x+w/20,y+h/20,w*9/10,h*9/10,Color.white);
		}
		tex.Apply();
		return tex;
	}

	void FillRect(Texture2D tex, int x, int y, int w, int h, Color fillColor){
		for(int i=x;i<=x+w;i++){
			for(int j=y;j<=y+h;j++){
				tex.SetPixel(i,j,fillColor);
			}
		}
	}

	void FillRectWithNoise(Texture2D tex, int x, int y, int w, int h, Color fillColor, float p){
		for(int i=x;i<=x+w;i++){
			for(int j=y;j<=y+h;j++){
				if (Random.value<p)
					tex.SetPixel(i,j,fillColor);
			}
		}
	}

	void ApplyCameraOffset(Camera c){
		if (!moveClouds) return;
		if (c!=Camera.main) return;
		transform.position=new Vector3(c.transform.position.x,transform.position.y,c.transform.position.z);
		posOffset=new Vector2(c.transform.position.x,c.transform.position.z);
		posOffset*=1;//transform.localScale.x;
		foreach(Material m in cloudMaterials){
			//texture world width=localScale.x/tiling
			m.SetTextureOffset(texName,m.GetTextureOffset(texName)+posOffset/transform.localScale.x*m.GetTextureScale(texName).x);
		}
	}

	void UnapplyCameraOffset(Camera c){
		if (!moveClouds) return;
		if (c!=Camera.main) return;
		foreach(Material m in cloudMaterials){
			m.SetTextureOffset(texName,m.GetTextureOffset(texName)-posOffset/transform.localScale.x*m.GetTextureScale(texName).x);
		}
	}

	void OnEnable(){
		Camera.onPreCull+=ApplyCameraOffset;
		Camera.onPostRender+=UnapplyCameraOffset;
	}
	void OnDisable(){
		Camera.onPreCull-=ApplyCameraOffset;
		Camera.onPostRender-=UnapplyCameraOffset;
	}
}
