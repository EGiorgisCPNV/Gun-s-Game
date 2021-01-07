using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(mapObjectInformation))]
public class mapObjectInformationEditor : Editor{
	mapObjectInformation mapObject;
	SerializedObject objectToUse;
	void OnEnable(){
		objectToUse = new SerializedObject(target);
		mapObject = (mapObjectInformation)target;
	}
	void OnSceneGUI(){   
		if (!Application.isPlaying) {
			if (mapObject.showGizmo && (mapObject.typeName == "Objective" || mapObject.typeName == "Path Element")) {
				GUIStyle style = new GUIStyle ();
				style.normal.textColor = mapObject.gizmoLabelColor;
				style.alignment = TextAnchor.MiddleCenter;
				Handles.Label (mapObject.transform.position + mapObject.transform.up * mapObject.triggerRadius + mapObject.transform.up * mapObject.gizmoLabelOffset, 
					"Objective: " + mapObject.gameObject.name, style);
			}
		}
	}
	public override void OnInspectorGUI(){
		if (objectToUse == null) {
			return;
		}
		objectToUse.Update ();
		GUILayout.BeginVertical ("box");
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("name"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("description"));
		if (objectToUse.FindProperty ("typeNameList").arraySize > 0) {
			objectToUse.FindProperty ("typeIndex").intValue = EditorGUILayout.Popup ("Map Icon Type", objectToUse.FindProperty ("typeIndex").intValue, mapObject.typeNameList);
			objectToUse.FindProperty ("typeName").stringValue = mapObject.typeNameList [objectToUse.FindProperty ("typeIndex").intValue];
		} else {
//			EditorGUILayout.Space();
//			GUI.color = Color.cyan;
//			EditorGUILayout.HelpBox("Press the button to get the Map Icon List ", MessageType.None);
//			GUI.color = Color.white;
//			EditorGUILayout.Space();
		}
		if (objectToUse.FindProperty ("floorList").arraySize > 0) {
			objectToUse.FindProperty ("floorIndex").intValue = EditorGUILayout.Popup ("Floor Number", objectToUse.FindProperty ("floorIndex").intValue, mapObject.floorList);
			if (objectToUse.FindProperty ("floorIndex").intValue >= 0) {
				objectToUse.FindProperty ("currentFloor").stringValue = mapObject.typeNameList [objectToUse.FindProperty ("floorIndex").intValue];
			}
		}
		if (mapObject.typeName == "Objective" || mapObject.typeName == "Path Element") {
			GUILayout.BeginVertical ("box");
			EditorGUILayout.Space();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox("Configure the Objective options", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("showGizmo"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("showOffScreenIcon"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("showMapWindowIcon"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("showDistance"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("triggerRadius"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("triggerColor"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("gizmoLabelOffset"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("gizmoLabelColor"));
			GUILayout.EndVertical ();
		}
		EditorGUILayout.Space();
//		if (GUILayout.Button ("Get Map Icon Type Manager List")) {
//			mapObject.getMapIconTypeList ();
//		}
//		if (GUILayout.Button ("Get Map Floor List")) {
//			mapObject.getFloorList ();
//		}
		//EditorGUILayout.Space();
		GUILayout.EndVertical ();
		if (GUI.changed) {
			objectToUse.ApplyModifiedProperties ();
		}
	}
}
#endif