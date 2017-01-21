using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DecalHandler : MonoBehaviour {
	public float maxLiveTime=0,maxCameraDistance=0;
	public int maxDecals=50;
	public Material baseDecal;
	List<Material> knownMaterialVariations=new List<Material>();
	List<Texture2D> knownTextures=new List<Texture2D>();
	List<int> materialUsageCount=new List<int>();
	List<GameObject> decals=new List<GameObject>();
	public Mesh decalBaseMesh;
	int decalCount=0;
	[HideInInspector]
	public static DecalHandler currentHandler=null;
	//const Vector3 one=Vector3.one;
	//public Shader

	void Start(){
		if(currentHandler!=null){
			Debug.LogError("A DecalHandler already exists!");
			this.enabled=false;
			return;
		}

		if(baseDecal==null){
			Debug.LogError("No base material for decal!");
			this.enabled=false;
			return;
		}



		if(decalBaseMesh==null){
			//Debug.Log("Generating 1x1 square for base mesh");
			decalBaseMesh=new Mesh();
			decalBaseMesh.vertices=new Vector3[]{new Vector3(-0.5f,0.5f,0),new Vector3(-0.5f,-0.5f,0),new Vector3(0.5f,-0.5f,0),new Vector3(0.5f,0.5f,0)};
			decalBaseMesh.triangles=new int[]{2,3,0,0,1,2};
			decalBaseMesh.uv=new Vector2[]{new Vector2(0,0),new Vector2(0,1),new Vector2(1,1),new Vector2(1,0)};
			decalBaseMesh.RecalculateNormals();
			decalBaseMesh.RecalculateBounds();
			decalBaseMesh.UploadMeshData(true);
		}
		currentHandler=this;
	}

	void Update(){
		while(decals.Count>=maxDecals){
			GameObject child=decals[0];
			if (child==null){
				decals.RemoveAt(0);
				continue;
			}
			int matIndex=knownTextures.IndexOf((Texture2D)child.GetComponent<MeshRenderer>().sharedMaterial.mainTexture);
			if(matIndex>=0){ 
				materialUsageCount[matIndex]--;
				//Debug.Log("Deleted decal with valid material "+materialUsageCount[matIndex]);
			}//else Debug.Log("Deleted decal with invalid material "+child.gameObject.GetComponent<MeshRenderer>().sharedMaterial.mainTexture+"!="+knownMaterialVariations[0].mainTexture);
			Destroy(child);
			decals.RemoveAt(0);
		}
		for (int m=0;m<materialUsageCount.Count;m++){
			if(materialUsageCount[m]<=0){
				//Debug.Log("Removing material at "+m);
				knownMaterialVariations.RemoveAt(m);
				knownTextures.RemoveAt(m);
				materialUsageCount.RemoveAt(m);
				break;
			}
		}
	}
	
	public void CreateDecal(Vector3 point,Vector3 normal,Texture2D image,Vector3 scale,Transform parent=null,bool randomRotation=true){
		//Create 2D plane at point, rotated to face normal, and with correct scaling
		GameObject decal_g=new GameObject("Decal "+transform.childCount);
		Transform decal_t=decal_g.transform;
		decal_t.position=point+normal*0.02f;
		decal_t.rotation=Quaternion.LookRotation(normal);
		//Correct scaling for image aspect ratio
		int biggestDimension=Mathf.Max(image.width,image.height);
		Vector2 img_d=new Vector2(image.width*1.0f/biggestDimension,image.height*1.0f/biggestDimension);//*64.0f/biggestDimension;
		if(randomRotation) decal_t.Rotate(Vector3.forward*Random.value*360);
		decal_t.localScale=new Vector3(scale.x*img_d.x,scale.y*img_d.y,scale.z);

		decal_t.parent=transform;
		if (parent!=null)
			decal_t.parent=parent;
		Material decal_m;
		int texIndex=knownTextures.IndexOf(image);
		if (texIndex>=0){
			decal_m=knownMaterialVariations[texIndex];
			materialUsageCount[texIndex]++;
		}else{
			decal_m=new Material(baseDecal);
			decal_m.mainTexture=image;
			knownMaterialVariations.Add(decal_m);
			knownTextures.Add(image);
			materialUsageCount.Add(1);
			//Debug.Log("Added material variation, current count is "+knownMaterialVariations.Count);
		}
		decal_g.AddComponent<MeshFilter>().mesh=decalBaseMesh;
		decal_g.AddComponent<MeshRenderer>().sharedMaterial=decal_m;
		decals.Add(decal_g);
		decalCount++;
	}

	public void CreateDecal(Vector3 point,Vector3 normal, Texture2D image){
		CreateDecal(point,normal,image,Vector3.one);
	}

}
