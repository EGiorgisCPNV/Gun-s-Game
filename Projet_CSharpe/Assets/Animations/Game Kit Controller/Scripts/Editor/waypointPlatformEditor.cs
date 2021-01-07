using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(waypointPlatform))]
public class waypointPlatformEditor : Editor{
	waypointPlatform platform;
	SerializedObject objectToUse;

	void OnEnable(){
		objectToUse = new SerializedObject(target);
		platform = (waypointPlatform)target;
	}
	void OnSceneGUI(){   
		platform = (waypointPlatform)target;
		if (platform.showGizmo) {
			for (int i = 0; i < platform.wayPoints.Count; i++) {
				GUIStyle style = new GUIStyle();
				style.normal.textColor = platform.gizmoLabelColor;
				style.alignment = TextAnchor.MiddleCenter;
				if (platform.wayPoints [i]) {
					Handles.Label (platform.wayPoints [i].position, (i+1).ToString(),style);	
				}
			}
		}
	}
	public override void OnInspectorGUI(){
		if (objectToUse == null) {
			return;
		}
		objectToUse.Update ();
		GUILayout.BeginVertical ("box");
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("waypointsParent"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("repeatWaypoints"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("moveInCircles"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("stopIfPlayerOutSide"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("waitTimeBetweenPoints"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("movementSpeed"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("movingForward"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("showGizmo"));
		if (objectToUse.FindProperty ("showGizmo").boolValue) {
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("gizmoLabelColor"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("gizmoRadius"));
		}
		GUILayout.EndVertical ();
		EditorGUILayout.Space ();
		GUILayout.BeginVertical ("Waypoints List", "window",GUILayout.Height(30));
		showUpperList (objectToUse.FindProperty ("wayPoints"));
		GUILayout.EndVertical ();
		if (GUI.changed) {
			objectToUse.ApplyModifiedProperties ();
		}
		EditorGUILayout.Space ();
	}
	void showUpperList(SerializedProperty list){
		EditorGUILayout.PropertyField(list);
		if (list.isExpanded){
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add Point")){
				platform.addNewWayPoint ();
			}
			if (GUILayout.Button("Clear")){
				list.arraySize=0;
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();
			for (int i = 0; i < list.arraySize; i++){
				GUILayout.BeginHorizontal();
				if (i < list.arraySize && i >= 0) {
					EditorGUILayout.PropertyField (list.GetArrayElementAtIndex (i), new GUIContent ("", null, ""));
				}
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button("x")){
					if (list.GetArrayElementAtIndex (i).objectReferenceValue) {
						Transform point = list.GetArrayElementAtIndex (i).objectReferenceValue as Transform;
						DestroyImmediate (point.gameObject);
					}
					list.DeleteArrayElementAtIndex(i);
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
				GUILayout.EndHorizontal();
			}
		}       
	}
}
#endif