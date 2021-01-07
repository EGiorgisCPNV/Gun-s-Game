using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
//a simple editor to add a button in the ragdollBuilder script inspector
[CustomEditor(typeof(mapTileBuilder))]
[CanEditMultipleObjects]
public class mapTileBuilderEditor : Editor{
	mapTileBuilder builder;
	SerializedObject objectToUse;
	GUIStyle style = new GUIStyle();

	void OnEnable(){
		objectToUse = new SerializedObject(target);
		builder = (mapTileBuilder)target;
	}
	void OnSceneGUI(){   
		if (!Application.isPlaying) {
			builder = (mapTileBuilder)target;
			if (builder.showGizmo ) {
				if (builder.eventTriggerList.Count>0) {
					style.normal.textColor = builder.gizmoLabelColor;
					style.alignment = TextAnchor.MiddleCenter;
					for (int i = 0; i < builder.eventTriggerList.Count; i++) {
						Handles.Label (builder.eventTriggerList[i].transform.position, "Event\n Trigger "+(i+1).ToString(), style);
					}
				}
				if (builder.textMesh) {
					style.normal.textColor = builder.gizmoLabelColor;
					style.alignment = TextAnchor.MiddleCenter;
					Handles.Label (builder.textMesh.transform.position, ("Text Mesh:\n "+builder.textMesh.GetComponent<TextMesh>().text.ToString()), style);
				}
			}
			Handles.Label (builder.transform.position, ("Map Part\n Color: "+
				"("+builder.mapPartMaterialColor.r.ToString("0.0")+","+builder.mapPartMaterialColor.g.ToString("0.0")+","+builder.mapPartMaterialColor.b.ToString("0.0")+")"), style);
		}
	}
	public override void OnInspectorGUI(){
		if (objectToUse == null) {
			return;
		}
		objectToUse.Update ();
		GUILayout.BeginVertical ("box");

		GUILayout.BeginVertical("Map Part State", "window", GUILayout.Height(30));
		string eventTriggerAdded ="NO";
		if ( objectToUse.FindProperty ("eventTriggerList").arraySize>0) {
			eventTriggerAdded = "YES";
		}
		GUILayout.Label ("Event Trigger Added\t\t" + eventTriggerAdded);
		string textMeshAdded ="NO";
		if ( objectToUse.FindProperty ("textMesh").objectReferenceValue) {
			textMeshAdded = "YES";
		}
		GUILayout.Label ("Text Mesh Added\t\t" + textMeshAdded);
		EditorGUILayout.Space();
		GUILayout.EndVertical();

		EditorGUILayout.PropertyField (objectToUse.FindProperty ("newPositionOffset"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("mapPartEnabled"));
		if (!objectToUse.FindProperty ("mapPartEnabled").boolValue) {
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("useOtherColorIfMapPartDisabled"));
			if (objectToUse.FindProperty ("useOtherColorIfMapPartDisabled").boolValue) {
				EditorGUILayout.PropertyField (objectToUse.FindProperty ("colorIfMapPartDisabled"));
			}
		}
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("showGizmo"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("mapPartMaterialColor"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("cubeGizmoScale"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("gizmoLabelColor"));
		GUILayout.EndVertical ();
		EditorGUILayout.Space ();
		GUILayout.BeginVertical ("Map Transform List", "window", GUILayout.Height (50));
		showUpperList (objectToUse.FindProperty ("verticesPosition"),false);
		GUILayout.EndVertical ();

		EditorGUILayout.Space ();

		if (!Application.isPlaying) {
			if (GUILayout.Button ("Rename All Transforms")) {
				builder.renameTransforms ();
			}
			if (GUILayout.Button ("Add New Map Part")) {
				builder.mapManager.addNewMapPart (builder.gameObject);
			}
			if (GUILayout.Button ("Duplicate Map Part")) {
				builder.mapManager.duplicateMapPart (builder.gameObject);
			}
			if (GUILayout.Button ("Add Trigger Event to Enable Map Part")) {
				builder.addEventTriggerToActive ();
			}
			if (GUILayout.Button ("Add Map Part Text Mesh")) {
				builder.addMapPartTextMesh ();
			}
			EditorGUILayout.Space ();
			if (objectToUse.FindProperty ("eventTriggerList").arraySize > 0) {
				GUILayout.BeginVertical ("Event Trigger List", "window", GUILayout.Height (50));
				showUpperList (objectToUse.FindProperty ("eventTriggerList"),true);
				GUILayout.EndVertical ();
			}
		}
		EditorGUILayout.Space ();
		if (GUI.changed) {
			objectToUse.ApplyModifiedProperties ();
		}
	}
	void showUpperList(SerializedProperty list, bool isEventTrigger){
		EditorGUILayout.PropertyField(list);
		if (list.isExpanded) {
			GUILayout.BeginHorizontal ();
			if (isEventTrigger) {
				if (GUILayout.Button ("Add Event Trigger")) {
					builder.addEventTriggerToActive ();
				}
			} else {
				if (GUILayout.Button ("Add Transform")) {
					builder.addNewTransform ();
				}
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
				if (GUILayout.Button("x")){
					if (list.GetArrayElementAtIndex (i).objectReferenceValue) {
						if (isEventTrigger) {
							GameObject eventTrigger = list.GetArrayElementAtIndex (i).objectReferenceValue as GameObject;
							DestroyImmediate (eventTrigger);
						} else {
							Transform point = list.GetArrayElementAtIndex (i).objectReferenceValue as Transform;
							DestroyImmediate (point.gameObject);
						}
					}
					list.DeleteArrayElementAtIndex(i);
					list.DeleteArrayElementAtIndex(i);
				}
				GUILayout.EndHorizontal();
			}
		}       
	}
}
#endif