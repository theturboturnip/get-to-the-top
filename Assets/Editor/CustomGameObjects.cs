using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System;

public class CustomGameObjects : MonoBehaviour {
	[MenuItem("GameObject/Level Objects/Building", false, 10)]
	static void CreateBuilding(MenuCommand menuCommand) {
		// Create a custom game object
		GameObject go = new GameObject("Building");
		go.AddComponent<BuildingGeneratorv2>();
		go.AddComponent<MeshFilter>();
		go.AddComponent<MeshRenderer>();
		// Ensure it gets reparented if this was a context click (otherwise does nothing)
		GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
		// Register the creation in the undo system
		Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
		Selection.activeObject = go;
	}

	[MenuItem("GameObject/Level Objects/Level Checkpoint",false,10)]
	static void CreateCheckpoint(MenuCommand menuCommand) {
		//Create the GameObject
		GameObject go=new GameObject("Checkpoint");
		LevelCheckpoint lc=go.AddComponent<LevelCheckpoint>();
		// Ensure it gets reparented if this was a context click (otherwise does nothing)
		GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
		if (go.transform.parent!=null){
			Bounds b=go.transform.parent.gameObject.GetComponent<Collider>().bounds;
			//go.transform.localPosition=Vector3.up*(b.size.y/2+1)/go.transform.parent.lossyScale.y;
			Bounds newB=new Bounds(Vector3.up,new Vector3(b.size.x/go.transform.parent.localScale.x,2/go.transform.parent.localScale.y,b.size.z/go.transform.parent.localScale.z));
			lc.boxBounds=newB;
		}
		// Register the creation in the undo system
		Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
		Selection.activeObject = go;
	}

	[MenuItem("GameObject/Level Objects/High Building Reflection",false,10)]
	static void CreateHighBuildingRefl(MenuCommand menuCommand){
		//Create the GameObject
		BuildingGeneratorv2 bg=(menuCommand.context as GameObject).GetComponent<BuildingGeneratorv2>();
		if(bg==null) return;

		GameObject go=new GameObject("Reflection Probe");
		go.tag="HighReflections";
		ReflectionProbe r=go.AddComponent<ReflectionProbe>();
		r.mode=UnityEngine.Rendering.ReflectionProbeMode.Realtime;
		// Ensure it gets reparented if this was a context click (otherwise does nothing)
		GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
		r.boxProjection=true;
		r.refreshMode=UnityEngine.Rendering.ReflectionProbeRefreshMode.EveryFrame;
		r.importance=2;
		r.resolution=256;
		r.size=bg.bounds;
		//r.bounds.size=bg.bounds;
		Collider c=bg.gameObject.GetComponent<Collider>();
		r.transform.localPosition=Vector3.up*((bg.topRelative?0:bg.bounds.y/2)+((LevelHandler)FindObjectOfType(typeof(LevelHandler))).cloudLevel-bg.transform.position.y)/2;
		//r.transform.position=c.bounds.center;
		r.size=c.bounds.size;
		r.size=r.size+Vector3.up*(Mathf.Abs(r.transform.localPosition.y)-r.size.y);
		/*r.transform.localPosition=Vector3.zero;
		r.transform.rotation=bg.transform.rotation;
		if (bg.topRelative)
			r.transform.localPosition=Vector3.down*bg.bounds.y/2;*/
		// Register the creation in the undo system
		Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
		Selection.activeObject = go;

	}

	[MenuItem("GameObject/Level Objects/Tip",false,10)]
	static void CreateTip(MenuCommand menuCommand) {
		//Create the GameObject
		GameObject go=new GameObject("Tip");
		TipTrigger t=go.AddComponent<TipTrigger>();
		// Ensure it gets reparented if this was a context click (otherwise does nothing)
		GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
		if (go.transform.parent!=null){
			Bounds b=go.transform.parent.gameObject.GetComponent<Collider>().bounds;
			//go.transform.localPosition=Vector3.up*(b.size.y/2+1)/go.transform.parent.lossyScale.y;
			Bounds newB=new Bounds(Vector3.up,new Vector3(b.size.x/go.transform.parent.localScale.x,2/go.transform.parent.localScale.y,b.size.z/go.transform.parent.localScale.z));
			t.activateBox=newB;
		}
		// Register the creation in the undo system
		Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
		Selection.activeObject = go;
	}

	[MenuItem("GameObject/Level Objects/Level Prerequisites",false,10)]
	static void CreateLevelPrerequisites(MenuCommand menuCommand=null){
		GameObject handlerParent=new GameObject("Handlers");
		handlerParent.AddComponent<LevelHandler>();
		handlerParent.AddComponent<CursorHandler>();
		handlerParent.AddComponent<SettingsApplier>();
		if (EventSystem.current==null){
			handlerParent.AddComponent<EventSystem>();
			handlerParent.AddComponent<StandaloneInputModule>();
		}
		// Register the creation in the undo system
		Undo.RegisterCreatedObjectUndo(handlerParent, "Create " + handlerParent.name);
		Selection.activeObject = handlerParent;
		
		GameObject go=new GameObject("Physics Handler");
		go.AddComponent<PhysicsObjectHandler>();
		go.transform.parent=handlerParent.transform;
		Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
		go=new GameObject("Decal Handler");
		go.AddComponent<DecalHandler>().baseDecal=(Material)AssetDatabase.LoadAssetAtPath("Assets/Graphics/Materials/BaseDecalMaterial.mat",typeof(Material));
		go.transform.parent=handlerParent.transform;
		Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);

		CreatePrefab("EntranceCapsule",menuCommand);
		CreatePrefab("ExitCapsule",menuCommand);

		CreatePrefab("CloudLayer",menuCommand);
		CreatePrefab("CloudSky",menuCommand);

		CreatePrefab("UI/Pause Menu",menuCommand);

		CreatePrefab("Playar",menuCommand);

		GameObject rfObj=new GameObject("Reflection Probe");
		Undo.RegisterCreatedObjectUndo(rfObj,"Create Reflection Probe");
		ReflectionProbe rf=rfObj.AddComponent<ReflectionProbe>();
		rf.resolution=1024;
		rf.size=new Vector3(1000,1000,1000);
		rf.hdr=false;
		rf.shadowDistance=0;
		rf.refreshMode=UnityEngine.Rendering.ReflectionProbeRefreshMode.OnAwake;
	}

	static GameObject CreatePrefab(string name,MenuCommand m){
		GameObject p=(GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/"+name+".prefab",typeof(GameObject));
		GameObject instantiated=PrefabUtility.InstantiatePrefab(p) as GameObject;
		Debug.Log(name);
		if (m!=null&&m.context!=null)
			GameObjectUtility.SetParentAndAlign(instantiated,m.context as GameObject);
		Undo.RegisterCreatedObjectUndo(instantiated, "Create " + instantiated.name);
		return instantiated;
	} 

	[MenuItem("GameObject/Level Objects/Faux Building",false,10)]
	static void CreateFauxBuilding(MenuCommand menuCommand){
		GameObject go=GameObject.CreatePrimitive(PrimitiveType.Cube);
		go.name="Building?";
		go.GetComponent<MeshRenderer>().material=(Material)AssetDatabase.LoadAssetAtPath("Assets/Graphics/Materials/Floor.mat",typeof(Material));
		// Ensure it gets reparented if this was a context click (otherwise does nothing)
		GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
		// Register the creation in the undo system
		Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
		Selection.activeObject = go;
		//go.AddComponent<BoxCollider>();
	}

	[MenuItem("GameObject/Level Objects/Faux Billboard",false,10)]
	static void CreateFauxBillboard(MenuCommand menuCommand){
		GameObject go=GameObject.CreatePrimitive(PrimitiveType.Cube);
		go.name="Billboard?";
		go.GetComponent<MeshRenderer>().material=(Material)AssetDatabase.LoadAssetAtPath("Assets/Graphics/Materials/TrumpFish.mat",typeof(Material));
		// Ensure it gets reparented if this was a context click (otherwise does nothing)
		GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
		// Register the creation in the undo system
		Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
		Selection.activeObject = go;
		//go.AddComponent<BoxCollider>();
	}

	[MenuItem("GameObject/Level Objects/Proto Enemy",false,10)]
	static void CreateProtoEnemy(MenuCommand menuCommand){
		Selection.activeObject = CreatePrefab("PlaceholderEnemy",menuCommand);
	}

	[MenuItem("Get To The Top/New Level",false,0)]
	static void CreateLevelScene(MenuCommand menuCommand){
		EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
		
		GameObject dirLight=new GameObject("Directional Light");
		dirLight.AddComponent<Light>().type=LightType.Directional;
		dirLight.transform.rotation=Quaternion.Euler(50,-30,0);

		RenderSettings.skybox=(Material)AssetDatabase.LoadAssetAtPath("Assets/Graphics/Skyboxes/Skybox.mat",typeof(Material));

		CreateLevelPrerequisites();
	}

	/*[MenuItem("GameObject/Add Selected To Checkpoint",false,10)]
	static void AddToCheckpoint(MenuCommand menuCommand){
		EditorApplication.ExecuteMenuItem("Game/Add Selected To Checkpoint");
	}*/

	[MenuItem("GameObject/Convert Cube to Building",false,10)]
	static void ConvertSelectionToBuildings(MenuCommand menuCommand){
		CubeToBuilding(menuCommand.context as GameObject);
	}

	static void CubeToBuilding(GameObject g){
		if (g.GetComponent<MeshFilter>()==null){
			Debug.Log("Excluded "+g+" from conversion as no mesh filter present");
		}
		Vector3 scale=g.transform.localScale;
		float top=g.transform.position.y+scale.y/2;
		Vector3 pos=g.transform.position;
		pos.y=((LevelHandler)UnityEngine.Object.FindObjectOfType(typeof(LevelHandler))).cloudLevel-50;
		scale.y=top-pos.y;
		pos.y=top;

		Debug.Log(g+" Conversion Details: "+pos+","+scale);
		GameObject building=GameObject.CreatePrimitive(PrimitiveType.Quad);//new GameObject(g.name+"_b");
		//Debug.Log("MESHCOLL "+building.GetComponent<MeshCollider>());
		UnityEngine.Object.DestroyImmediate(building.GetComponent<MeshCollider>());
		building.name=g.name+"_b";
		building.transform.parent=g.transform.parent;
		building.transform.SetSiblingIndex(g.transform.GetSiblingIndex()+1);
		building.transform.position=pos;
		building.transform.eulerAngles=Vector3.up*g.transform.eulerAngles.y;
		BuildingGeneratorv2 buildingGen=building.AddComponent<BuildingGeneratorv2>();
		buildingGen.bounds=scale;
		buildingGen.buildingShape=building.GetComponent<MeshFilter>().sharedMesh;
		buildingGen.sideUVOffsetPerUnit=new Vector2(0.6f,0.4f);
		buildingGen.roofUVOffsetPerUnit=Vector2.one*0.5f;
		buildingGen.generateUVs=true;
		buildingGen.topRelative=true;
		MeshRenderer mr=building.GetComponent<MeshRenderer>();
		mr.materials=new Material[]{(Material)AssetDatabase.LoadAssetAtPath("Assets/Graphics/Materials/Buildings/ShinyWindow.mat",typeof(Material)),
									(Material)AssetDatabase.LoadAssetAtPath("Assets/Graphics/Materials/Buildings/TiledRoof.mat",typeof(Material))};
		Undo.RegisterCreatedObjectUndo(building, "Create "+building.name);
		foreach(Transform t in g.transform){
			Undo.RecordObject(t,"Converting to building child");
			Vector3 oldScale=t.lossyScale;
			t.parent=building.transform;
			t.localScale=oldScale;
		}
		//Undo.RecordObject(g,"Disabling original building");
		g.SetActive(false);
	}
}

/*public class AddToCheckpointWindow:EditorWindow{
	static Transform[] toAdd;
	LevelCheckpoint addTo;
	[MenuItem("Game/Add Selected To Checkpoint")]
	static void Init(){
		if (Selection.transforms.Length==0) return;
		toAdd=Selection.transforms.Clone() as Transform[];

		// Get existing open window or if none, make a new one:
        AddToCheckpointWindow window = (AddToCheckpointWindow)EditorWindow.GetWindow (typeof (AddToCheckpointWindow));
        window.Show();
		//Drag checkpoint into slot 
	}

	void OnGUI(){
		EditorGUILayout.LabelField("To Add: "+toAdd.Length);
		addTo=EditorGUILayout.ObjectField("Checkpoint",addTo,typeof(LevelCheckpoint),true) as LevelCheckpoint;
		if (GUILayout.Button("Add Selected")&&(addTo!=null)){
			CheckpointSorter cs=new CheckpointSorter();
			cs.start=addTo.transform;
			Array.Sort(toAdd,cs);
			addTo.toLoad=toAdd;
			this.Close();
		}
	}
}*/

public class CheckpointSorter : IComparer{
	public Transform start;
	public int Compare(System.Object x,System.Object y){
		float distX=(((Transform)x).position-start.position).magnitude;
		float distY=(((Transform)y).position-start.position).magnitude;
		if (distY>distX) return -1;
		return 1;
	}
}
