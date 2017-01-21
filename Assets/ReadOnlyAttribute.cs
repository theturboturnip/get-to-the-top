using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

public class ReadOnlyAttribute : PropertyAttribute{}

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer{ 
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label){
		 GUI.enabled = false;
		 EditorGUI.PropertyField(position, property, label, true);
		 GUI.enabled = true;
	 }
}
#else
public class ReadOnlyAttribute : PropertyAttribute{}
public class ReadOnlyDrawer{}
#endif
