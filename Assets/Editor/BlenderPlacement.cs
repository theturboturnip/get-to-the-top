using UnityEngine;
using UnityEditor;
using System.Collections;

enum MoveMode{
	None=0,
	Translate=1,
	Rotate=2,
	Scale=3
};

[CustomEditor(typeof(Transform))]
public class BlenderPlacement : Editor{

	/*
		VARIABLES
				   */
	static MoveMode currentMoveMode;
	static Transform handle,endPos,currentTransform;
	static Vector3 startingForward,worldMouseStart;
	static bool shouldFindMouseStartPosition;
	static Vector2 mouseStart;
	static Vector3 transformBounds=Vector3.one;
	static Vector3 referencePoint;
	static bool rmbDown=false;

	/*
		ENABLE/DISABLE
						*/

	public void OnEnable () {
		if (handle!=null && endPos!=null) return;
		//Debug.Log("Blender Movement Plugin Activated");
		handle=new GameObject("Blender Handle").transform;
		handle.gameObject.hideFlags=HideFlags.HideAndDontSave;
		endPos=new GameObject("Blender End Pos").transform;
		endPos.gameObject.hideFlags=HideFlags.HideAndDontSave;
	}

	public void OnDisable(){
		ApplyTransformation();
	}

	/*
		KEYBOARD SHORTCUTS
							*/

	//[MenuItem("Blender Movement/Translate _g")]
	static void BeginTranslate(){
		BeginTransformation(MoveMode.Translate);
	}

	//[MenuItem("Blender Movement/Rotate _r")]
	static void BeginRotate(){
		BeginTransformation(MoveMode.Rotate);
	}

	//[MenuItem("Blender Movement/Scale _s")]
	static void BeginScale(){
		BeginTransformation(MoveMode.Scale);
	}


	//[MenuItem("Blender Movement/X _x")]
	static void TransformBoundsX(){
		if (currentMoveMode==MoveMode.None) return;
		if (transformBounds==Vector3.right)
			transformBounds=Vector3.one;
		else
			transformBounds=Vector3.right;

		Debug.Log("Locking to X");
	}

	//[MenuItem("Blender Movement/Y _y")]
	static void TransformBoundsY(){
		if (currentMoveMode==MoveMode.None) return;
		if (transformBounds==Vector3.up)
			transformBounds=Vector3.one;
		else
			transformBounds=Vector3.up;

		Debug.Log("Locking to Y");
	}

	//[MenuItem("Blender Movement/Z _z")]
	static void TransformBoundsZ(){
		if (currentMoveMode==MoveMode.None) return;
		if (transformBounds==Vector3.forward)
			transformBounds=Vector3.one;
		else
			transformBounds=Vector3.forward;

		Debug.Log("Locking to Z");
	}
	

	//[MenuItem("Blender Movement/YZ #x")]
	static void TransformBoundsYZ(){
		if (currentMoveMode==MoveMode.None) return;
		if (transformBounds==Vector3.forward+Vector3.up)
			transformBounds=Vector3.one;
		else
			transformBounds=Vector3.forward+Vector3.up;

		Debug.Log("Locking to YZ");
	}

	//[MenuItem("Blender Movement/XZ #y")]
	static void TransformBoundsXZ(){
		if (currentMoveMode==MoveMode.None) return;
		if (transformBounds==Vector3.right+Vector3.forward)
			transformBounds=Vector3.one;
		else
			transformBounds=Vector3.right+Vector3.forward;

		Debug.Log("Locking to XZ");
	}

	//[MenuItem("Blender Movement/XY #z")]
	static void TransformBoundsXY(){
		if (currentMoveMode==MoveMode.None) return;
		if (transformBounds==Vector3.right+Vector3.up)
			transformBounds=Vector3.one;
		else
			transformBounds=Vector3.right+Vector3.up;

		Debug.Log("Locking to XY");
	}

	/*
		KEYBOARD SHORTCUT VALIDATIONS
										*/

	static bool ShortcutAllowed(){
		return !(EditorApplication.isPlaying&&!EditorApplication.isPaused);
	}

	//[MenuItem("Blender Movement/Translate _g", true)]
	static bool TranslateAllowed(){ return ShortcutAllowed(); }

	//[MenuItem("Blender Movement/Rotate _r", true)]
	static bool RotateAllowed(){ return ShortcutAllowed(); }

	/*
		TRANSFORMATION FUNCTIONS
								   */

	static void BeginTransformation(MoveMode newMoveMode){
		if (currentMoveMode!=MoveMode.None)
			return;
		if (Selection.activeTransform==null)
			return;
		if (EditorApplication.isPlaying&&!EditorApplication.isPaused) return;

		Debug.Log("Moving "+Selection.activeTransform);

		currentMoveMode=newMoveMode;
		currentTransform=Selection.activeTransform;
		shouldFindMouseStartPosition=true;
		transformBounds=Vector3.one;
		ApplyTransformData(currentTransform,handle);
	}

	static void ApplyTransformation(){
		if (currentTransform==null) return;
		if (currentMoveMode==MoveMode.None) return;
		Debug.Log("Applying Transformation");
		currentMoveMode=MoveMode.None;

		ApplyTransformData(currentTransform,endPos);
		Debug.Log(currentTransform.position+"="+endPos.position);
		ApplyTransformData(handle,currentTransform);
		Undo.RecordObject(currentTransform,"Applied Blender Movement");
		ApplyTransformData(endPos,currentTransform);
	}

	static void UndoTransformation(){
		if (currentTransform==null) return;
		if (currentMoveMode==MoveMode.None) return;
		currentMoveMode=MoveMode.None;
		ApplyTransformData(handle,currentTransform);
	}

	static void ApplyTransformData(Transform copyFrom,Transform pasteTo){
		pasteTo.position=copyFrom.position;
		pasteTo.rotation=copyFrom.rotation;
		pasteTo.localScale=copyFrom.localScale;
	}

	/*
		MOVEMENT FUNCTIONS
							*/

	void OnSceneGUI(){
		//Find helper vars
		currentTransform=target as Transform;
		Event e=Event.current;
		Transform currentCamera=Camera.current.transform;
		Vector3 currentMousePos=ScreenToWorldPoint(e.mousePosition);
		
		//If we just started a thing, find the mouse position
		if (shouldFindMouseStartPosition){
			mouseStart=e.mousePosition;
			//startingForward=currentTransform.forward;
			shouldFindMouseStartPosition=false;
			worldMouseStart=currentMousePos;
		}

		if (currentMoveMode==MoveMode.Translate){
			//Calculate new position and apply
			currentTransform.position=Vector3.Scale(currentMousePos-worldMouseStart,transformBounds)+handle.position;

			//Draw Label
			Handles.Label(currentTransform.position+currentCamera.up,""+(currentTransform.position-handle.position));
		}else if (currentMoveMode==MoveMode.Rotate){
			//Find helper vars
			Vector2 currentPos=Camera.current.WorldToScreenPoint(referencePoint);
			currentPos.y=Camera.current.pixelHeight-currentPos.y;
			Vector2 startMouseDisp=mouseStart-currentPos;
			if (startMouseDisp.sqrMagnitude==0) startMouseDisp=Vector2.up;
			Vector2 newMouseDisp=e.mousePosition-currentPos;
			//Vector2 mouseDisp=newMouseDisp-startMouseDisp;

			//Calculate new angle and apply
			float angleBetweenCos=Vector2.Dot(startMouseDisp.normalized,newMouseDisp.normalized);
			float angleBetweenSin=Vector2.Dot(new Vector2(startMouseDisp.y,-startMouseDisp.x).normalized,newMouseDisp.normalized);
			float angleBetween=Mathf.Atan2(angleBetweenCos,angleBetweenSin)*Mathf.Rad2Deg;
			Vector3 rotationAxis=currentCamera.forward;
			if (transformBounds!=Vector3.one)
				rotationAxis=transformBounds;
			currentTransform.forward=Quaternion.AngleAxis(90-angleBetween, rotationAxis)*handle.forward;

			//Draw Label
			Handles.Label(currentTransform.position+currentCamera.up,angleBetween+"");
		}else if (currentMoveMode==MoveMode.Scale){
			//Calculate new scale and apply
			float scaleAmount=((currentMousePos-currentTransform.position).magnitude/(worldMouseStart-currentTransform.position).magnitude);
			Vector3 scaleToScale=Vector3.Scale(handle.localScale,transformBounds);
			Vector3 scaleToLeave=handle.localScale-scaleToScale;
			currentTransform.localScale=scaleToScale*scaleAmount+scaleToLeave;

			//Draw Label
			Handles.Label(currentTransform.position+currentCamera.up,scaleAmount+"");
		}

		//Check for apply/undo
		if (e!=null&&e.type==EventType.MouseDown){
			if (e.button==0)
				ApplyTransformation();
			else if (e.button==1){
				UndoTransformation();
				rmbDown=true;
			}
		}
		if (e!=null&&e.type==EventType.MouseUp){
			if (e.button==1)
				rmbDown=false;
		}
		//Debug.Log(e.button+","+e.isMouse);
		if (e!=null&&e.type==EventType.KeyDown&&ShortcutAllowed()&&!rmbDown){
			if (e.keyCode==KeyCode.G){
				BeginTranslate();
			}else if (e.keyCode==KeyCode.R){
				BeginRotate();
			}else if (e.keyCode==KeyCode.S){
				BeginScale();
			}else if (e.keyCode==KeyCode.X){
				if (e.shift) TransformBoundsYZ();
				else TransformBoundsX();
			}else if (e.keyCode==KeyCode.Y){
				if (e.shift) TransformBoundsXZ();
				else TransformBoundsY();
			}else if (e.keyCode==KeyCode.Z){
				if (e.shift) TransformBoundsXY();
				else TransformBoundsZ();
			}
		}
	}

	Vector3 ScreenToWorldPoint(Vector2 screen){
		float cameraDepth=Vector3.Dot(currentTransform.position-Camera.current.transform.position,Camera.current.transform.forward);
		Vector3 world=Camera.current.ScreenToWorldPoint(new Vector3(screen.x,Camera.current.pixelHeight-screen.y,cameraDepth));
		return world;
	}

	/*
		INSPECTOR FUNCTIONS
							  */

	public override void OnInspectorGUI() {
		//DrawDefaultInspector();
		//return;
		currentTransform=target as Transform;
		referencePoint=Vector3.zero;
		foreach  (Transform t in targets){
			referencePoint+=t.position;
		}
		referencePoint/=targets.Length*1f;

		//Draw position, rotation, local scale
		currentTransform.localPosition=EditorGUILayout.Vector3Field("Local Position",currentTransform.localPosition);

		currentTransform.localEulerAngles=EditorGUILayout.Vector3Field("Local Rotation",currentTransform.localEulerAngles);

		currentTransform.localScale=EditorGUILayout.Vector3Field("Local Scale",currentTransform.localScale);

		EditorGUILayout.Space();
		EditorGUILayout.Vector3Field("Position",currentTransform.position);
		EditorGUILayout.Vector3Field("Rotation",currentTransform.eulerAngles);
	}
}
