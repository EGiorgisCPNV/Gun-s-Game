using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
//a simple editor to add some buttons in the vehicle weapon script inspector
[CustomEditor(typeof(hoverBoardWayPoints))]
public class hoverBoardWayPointsEditor : Editor{
	SerializedObject list;
	hoverBoardWayPoints points;
	void OnEnable(){
		list = new SerializedObject(target);
		points = (hoverBoardWayPoints)target;
	}
	public override void OnInspectorGUI(){
		if (list == null) {
			return;
		}
		list.Update ();
		GUILayout.BeginVertical("box");
		EditorGUILayout.PropertyField(list.FindProperty("wayPointElement"), new GUIContent("WayPoint Element"), false);
		EditorGUILayout.PropertyField(list.FindProperty("movementSpeed"), new GUIContent("Movement Speed"), false);
		EditorGUILayout.PropertyField(list.FindProperty("moveInOneDirection"), new GUIContent("Move In One Direction"), false);
		EditorGUILayout.PropertyField(list.FindProperty("triggerRadius"), new GUIContent("Trigger Radius"), false);
		EditorGUILayout.PropertyField(list.FindProperty("extraRotation"), new GUIContent("Extra Rotation"), false);
		EditorGUILayout.PropertyField(list.FindProperty("forceAtEnd"), new GUIContent("Force At End"), false);
		EditorGUILayout.PropertyField(list.FindProperty("railsOffset"), new GUIContent("Rail Offset"), false);
		EditorGUILayout.PropertyField(list.FindProperty("extraScale"), new GUIContent("Extra Scale"), false);
		EditorGUILayout.PropertyField(list.FindProperty("showGizmo"), new GUIContent("Show Gizmo"), false);
		EditorGUILayout.PropertyField(list.FindProperty("gizmoRadius"), new GUIContent("Gizmo Radius"), false);
		GUILayout.EndVertical();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		GUILayout.BeginVertical("WayPoints List", "window",GUILayout.Height(30));
		showUpperList(list.FindProperty("wayPoints"));
		GUILayout.EndVertical();
		EditorGUILayout.Space();
		if (GUI.changed){
			list.ApplyModifiedProperties();
		}
	}
	void showListElementInfo(SerializedProperty list){
		GUILayout.BeginVertical("box");
		EditorGUILayout.PropertyField(list.FindPropertyRelative("Name"), new GUIContent("Name"), false);
		EditorGUILayout.PropertyField(list.FindPropertyRelative("wayPoint"), new GUIContent("WayPoint"), false);
		EditorGUILayout.PropertyField(list.FindPropertyRelative("direction"), new GUIContent("Direction"), false);
		EditorGUILayout.PropertyField(list.FindPropertyRelative("trigger"), new GUIContent("Trigger"), false);
		EditorGUILayout.PropertyField(list.FindPropertyRelative("railMesh"), new GUIContent("Rail Mesh"), false);
		GUILayout.EndVertical();
	}
	void showUpperList(SerializedProperty list){
		GUILayout.BeginVertical();
		EditorGUILayout.PropertyField(list);
		if (list.isExpanded){
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add Point")){
				points.addNewWayPoint ();
			}
			if (GUILayout.Button("Clear")){
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
				GUILayout.EndHorizontal();
				if (expanded) {
					GUILayout.BeginVertical ();
					if (GUILayout.Button ("x")) {
						Transform point = list.GetArrayElementAtIndex (i).FindPropertyRelative ("wayPoint").objectReferenceValue as Transform;
						DestroyImmediate (point.gameObject);
						list.DeleteArrayElementAtIndex(i);
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
					GUILayout.EndVertical ();
				} else {
					GUILayout.BeginHorizontal ();
					if (GUILayout.Button ("x")) {
						Transform point = list.GetArrayElementAtIndex (i).FindPropertyRelative ("wayPoint").objectReferenceValue as Transform;
						DestroyImmediate (point.gameObject);
						list.DeleteArrayElementAtIndex(i);
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
					GUILayout.EndHorizontal ();
				}
				GUILayout.EndHorizontal();
			}
		}
		GUILayout.EndVertical();
	}
}
#endif