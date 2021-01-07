using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(pickUpObject))]
public class pickUpObjectEditor : Editor{
	SerializedObject list;

	void OnEnable(){
		list = new SerializedObject(target);
	}
	public override void OnInspectorGUI(){
		if (list == null) {
			return;
		}
		list.Update ();
		GUILayout.BeginVertical ("box");
		EditorGUILayout.Space ();
		EditorGUILayout.PropertyField (list.FindProperty ("pickType"));
		EditorGUILayout.PropertyField (list.FindProperty ("amount"));
		EditorGUILayout.PropertyField (list.FindProperty ("useSecondaryString"));
		if (list.FindProperty ("useSecondaryString").boolValue) {
			EditorGUILayout.PropertyField (list.FindProperty ("secondaryString"));
		}
		EditorGUILayout.PropertyField (list.FindProperty ("pickUpSound"));
		EditorGUILayout.PropertyField (list.FindProperty ("staticPickUp"));
		EditorGUILayout.PropertyField (list.FindProperty ("moveToPlayerOnTrigger"));
		EditorGUILayout.PropertyField (list.FindProperty ("pickUpOption"));
		EditorGUILayout.Space ();
		GUILayout.EndVertical();
		if (GUI.changed) {
			list.ApplyModifiedProperties ();
		}
	}
}
#endif