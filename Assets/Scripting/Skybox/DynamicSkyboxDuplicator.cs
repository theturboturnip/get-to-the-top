using UnityEngine;
using System.Collections;

public class DynamicSkyboxDuplicator : MonoBehaviour {
	void Start(){
		DynamicSkybox ds=DynamicSkybox.currentSkybox;
		if (ds==null){
			Debug.Log("NO DYNAMIC SKYBOX!");
			return;
		} 
		Transform copy=(Instantiate(gameObject,Vector3.zero,transform.rotation) as GameObject).transform;


		Vector3 pos=ds.playingAreaToSkybox.MultiplyPoint3x4(transform.position);
		Vector3 scale=ds.playingAreaScaleToSkybox.MultiplyVector(transform.localScale);
		copy.gameObject.layer=LayerMask.NameToLayer(ds.skyboxLayer);
		Destroy(copy.gameObject.GetComponent<DynamicSkyboxDuplicator>());
		copy.transform.localPosition=Vector3.zero;
		copy.transform.position=pos;
		
		copy.localScale=scale;
		copy.transform.parent=ds.transform;

	}
}
