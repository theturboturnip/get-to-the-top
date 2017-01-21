using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DynamicSkybox : MonoBehaviour {
	//public Camera targetCamera;
	public Vector3 size;
	public Vector3 playingAreaPosition,playingAreaSize;
	public int cubemapFaceSize=512,cubemapColorDepth=8;
	public Material backgroundSkybox;
	public Shader skyboxShader;
	public string skyboxLayer;
	public bool smartClippingPlanes=true;
	public Vector3 excludeFromRender=Vector3.down;
	public float cameraMovementThreshold=0.1f,renderTime;
	Dictionary<Camera,RenderTexture> skyboxTextures;
	Vector3 oldPosition;
	Quaternion oldRotation;
	bool hasCameraRenderedYet=false;
	Camera skyboxRenderer;
	RenderTexture skybox;
	Material skyboxMat;
	Vector3 scalingVector;
	[HideInInspector]
	public Matrix4x4 playingAreaToSkybox,skyboxToPlayingArea,playingAreaScaleToSkybox;
	public static DynamicSkybox currentSkybox;
	#if UNITY_EDITOR
	Timer t;
	#endif

	void Start(){
		if(currentSkybox!=null){
			Debug.Log("Another dynamic skybox already exists!");
			this.enabled=false;
			return;
		}
		skyboxTextures=new Dictionary<Camera,RenderTexture>();
		//skybox=new RenderTexture(1920,1080,cubemapColorDepth,RenderTextureFormat.Default);//new Cubemap(cubemapFaceSize,TextureFormat.ARGB32,false);
		//skybox.isCubemap=true;

		skyboxRenderer=gameObject.GetComponentInChildren<Camera>();
		skyboxRenderer.enabled=false;
		skyboxRenderer.cullingMask=LayerMask.GetMask(skyboxLayer);
		skyboxRenderer.clearFlags=CameraClearFlags.SolidColor;
		skyboxRenderer.backgroundColor=new Color(1,0,0,0);
		if(smartClippingPlanes)
			skyboxRenderer.farClipPlane=size.magnitude;

		//skyboxMat=new Material(skyboxShader);

		scalingVector=new Vector3(size.x/playingAreaSize.x,size.y/playingAreaSize.y,size.z/playingAreaSize.z);
		playingAreaToSkybox=Matrix4x4.TRS(transform.position-playingAreaPosition,Quaternion.identity,scalingVector);
		playingAreaScaleToSkybox=Matrix4x4.Scale(scalingVector);
		skyboxToPlayingArea=playingAreaToSkybox.inverse;

		currentSkybox=this;
		RenderSettings.skybox=backgroundSkybox;

		#if UNITY_EDITOR
		t=new Timer(false);
		#endif
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.green;
		Gizmos.DrawWireCube(transform.position,size);
		Gizmos.color=Color.blue;
		Gizmos.DrawWireCube(playingAreaPosition,playingAreaSize);
	}

	int VectorToCubeSide(Vector3 v,int layerMask=0){
		Vector3 absV=new Vector3(Mathf.Abs(v.x),Mathf.Abs(v.y),Mathf.Abs(v.z));
		if (absV.x>absV.y&&absV.x>absV.z){
			if (v.x>0) layerMask=layerMask | (int) CubemapFace.PositiveX;
			else layerMask=layerMask | (int) CubemapFace.NegativeX;
		}else if (absV.y>absV.x&&absV.y>absV.z){
			if (v.y>0) layerMask=layerMask | (int) CubemapFace.PositiveY;
			else layerMask=layerMask | (int) CubemapFace.NegativeY;
		}else if (absV.z>absV.x&&absV.z>absV.y){
			if (v.z>0) layerMask=layerMask | (int) CubemapFace.PositiveZ;
			else layerMask=layerMask | (int) CubemapFace.NegativeZ;
		}
		return layerMask;
	}

	public void TargetPreRender(Camera cam){
		#if UNITY_EDITOR
		if(!EditorApplication.isPlaying||EditorApplication.isPaused) return;
		#endif
		if(cam==skyboxRenderer) return;
		if((cam.transform.position-oldPosition).magnitude<cameraMovementThreshold&&hasCameraRenderedYet&&oldRotation==cam.transform.rotation) return;
		#if UNITY_EDITOR
		t.Start();
		#endif
		BackTextureImageEffect bt=cam.GetComponent<BackTextureImageEffect>();
		if(bt==null){
			bt=cam.gameObject.AddComponent<BackTextureImageEffect>();
			bt.enabled=true;
			bt.shader=skyboxShader;
		}

		//Find good camera settings
		cam.cullingMask=cam.cullingMask & ~LayerMask.GetMask(skyboxLayer);
		//Set renderer pos and clip plane
		skyboxRenderer.transform.position=playingAreaToSkybox.MultiplyPoint3x4(cam.transform.position);
		if(smartClippingPlanes)
			skyboxRenderer.nearClipPlane=cam.farClipPlane*size.magnitude/playingAreaSize.magnitude*0.5f;//*0.7f;
		skyboxRenderer.fieldOfView=cam.fieldOfView;
		skyboxRenderer.aspect=cam.aspect;
		skyboxRenderer.transform.rotation=cam.transform.rotation;
		//int facesToRender=63;//~(VectorToCubeSide(-cam.transform.forward));
		//if(!hasCameraRenderedYet) facesToRender=63;
		//skybox.width=cam.pixelWidth;
		//skybox.height=cam.pixelHeight;//RenderTexture.GetTemporary(cam.pixelWidth,cam.pixelHeight);
		if(!skyboxTextures.ContainsKey(cam))
			skyboxTextures[cam]=new RenderTexture(cam.pixelWidth,cam.pixelHeight,cubemapColorDepth,RenderTextureFormat.Default);
		else if (skyboxTextures[cam].width!=cam.pixelWidth||skyboxTextures[cam].height!=cam.pixelHeight)
			skyboxTextures[cam]=new RenderTexture(cam.pixelWidth,cam.pixelHeight,cubemapColorDepth,RenderTextureFormat.Default);
		
		skyboxRenderer.targetTexture=skyboxTextures[cam];
		skyboxRenderer.Render();
		bt.backTexture=skyboxTextures[cam];
		//RenderTexture.ReleaseTemporary(skybox);
		//skyboxMat.SetTexture("_Tex",skybox);
		//RenderSettings.skybox.SetTexture("_LayerCube",skybox);
		#if UNITY_EDITOR
		renderTime=t.Stop();
		#else
		renderTime=-1;
		#endif
		oldPosition=cam.transform.position;
		oldRotation=cam.transform.rotation;
		hasCameraRenderedYet=true;
	}

	/*public void TargetPostRender(Camera cam){
		if(cam!=skyboxRenderer) return;
		
		//targetCamera.
	}*/

	public void OnEnable(){
		// register the callback when enabling object
		Camera.onPreRender += TargetPreRender;
		//Camera.onPostRender += TargetPostRender;
	}
	public void OnDisable(){
		// remove the callback when disabling object
		Camera.onPreRender -= TargetPreRender;
		//Camera.onPostRender -= TargetPostRender;
	}
}
