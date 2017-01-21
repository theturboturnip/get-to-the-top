using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(WorldData))]
public class WorldDataEditor : Editor{
	public override void OnInspectorGUI(){
		WorldData.worldRadius=EditorGUILayout.FloatField("World Radius",WorldData.worldRadius);
	}
}
#endif

public class WorldData : MonoBehaviour {
	//public float islandRadius=2500,buildingGenerationRadius=1100,viewDistance=1000,roofGenerationRadius=100,camFOV=90;
	public bool zeroPopIn=false;
	public static float worldRadius,buildingGenRadius,viewDist,roofGenRadius,cameraFOV;
	/*void OnEnable(){
		worldRadius=islandRadius;
		cameraFOV=camFOV;
		float sideLookLength=zeroPopIn?(viewDistance/Mathf.Cos(cameraFOV/2*Mathf.Deg2Rad)):viewDistance;
		sideLookLength=Mathf.Clamp(sideLookLength,0,islandRadius*2);
		if(buildingGenerationRadius<sideLookLength)
			buildingGenerationRadius=sideLookLength+100;
			//viewDistance=buildingGenerationRadius*0.9f;
		buildingGenRadius=buildingGenerationRadius;
		viewDist=viewDistance;
		roofGenRadius=roofGenerationRadius;
	}*/
}

