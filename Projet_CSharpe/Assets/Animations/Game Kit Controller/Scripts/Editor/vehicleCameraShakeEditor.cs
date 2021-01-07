using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(vehicleCameraShake))]
[CanEditMultipleObjects]
public class vehicleCameraShakeEditor : Editor{
	SerializedObject list;
	string currentState;

	void OnEnable(){
		list = new SerializedObject(target);
	}
	public override void OnInspectorGUI(){
		if (list == null) {
			return;
		}
		list.Update ();
		GUILayout.BeginVertical ("box");
		GUILayout.Label ("Shaking Active: \t" + list.FindProperty ("shakingActive").boolValue.ToString ());
		EditorGUILayout.Space ();
		if (list.FindProperty ("shakingActive").boolValue) {
			if (list.FindProperty ("playerBobState")!=null) {
				currentState = list.FindProperty ("playerBobState").FindPropertyRelative ("Name").stringValue.ToString ();
			}
		} else {
			currentState = "Idle";
		}
		GUILayout.Label ("Current Shake State: \t" + currentState);
		EditorGUILayout.Space ();
		EditorGUILayout.PropertyField(list.FindProperty("headBobEnabled"));
		EditorGUILayout.PropertyField(list.FindProperty("externalForceStateName"));
		GUILayout.EndVertical ();
		EditorGUILayout.Space ();
		GUI.color = Color.cyan;
		EditorGUILayout.HelpBox ("Configure a Camera Shake List", MessageType.None);
		GUI.color = Color.white;
		EditorGUILayout.Space ();
		EditorGUILayout.Space ();
		GUILayout.BeginVertical("Camera Shake List", "window",GUILayout.Height(30));
		showUpperList(list.FindProperty("bobStatesList"));
		GUILayout.EndVertical();
		EditorGUILayout.Space ();
		if (GUI.changed) {
			list.ApplyModifiedProperties ();
		}
		EditorGUILayout.Space ();
	}
	void showListElementInfo(SerializedProperty list){
		GUILayout.BeginVertical("box");
		EditorGUILayout.PropertyField(list.FindPropertyRelative("Name"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("eulAmount"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("eulSpeed"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("eulSmooth"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("stateEnabled"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("isCurrentState"));
		GUILayout.EndVertical();
	}
	void showUpperList(SerializedProperty list){
		GUILayout.BeginVertical();
		EditorGUILayout.PropertyField(list,new GUIContent("Camera Shake States"));
		if (list.isExpanded){
			GUILayout.Label ("Number Of States: \t" + list.arraySize.ToString ());
			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add State")){
				list.arraySize++;
			}
			if (GUILayout.Button("Clear List")){
				list.arraySize = 0;
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();
			for (int i = 0; i < list.arraySize; i++){
				bool expanded = false;
				GUILayout.BeginHorizontal();
				GUILayout.BeginHorizontal("box");
				EditorGUILayout.Space();
				if (i < list.arraySize && i >= 0){
					EditorGUILayout.BeginVertical();
					EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i));
					if (list.GetArrayElementAtIndex (i).isExpanded) {
						expanded = true;
						showListElementInfo (list.GetArrayElementAtIndex (i));
					}
					EditorGUILayout.Space();
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal ();
				if (expanded) {
					GUILayout.BeginVertical ();
				} else {
					GUILayout.BeginHorizontal ();
				}
				if (GUILayout.Button ("x")) {
					list.DeleteArrayElementAtIndex (i);
				}
				if (GUILayout.Button ("v")) {
					if (i >= 0) {
						list.MoveArrayElement (i, i + 1);
					}
				}
				if (GUILayout.Button ("^")) {
					if (i < list.arraySize) {
						list.MoveArrayElement (i, i - 1);
					}
				}
				if (expanded) {
					GUILayout.EndVertical ();
				} else {
					GUILayout.EndHorizontal ();
				}
				GUILayout.EndHorizontal();
			}
		}
		GUILayout.EndVertical();
	}
}
#endif