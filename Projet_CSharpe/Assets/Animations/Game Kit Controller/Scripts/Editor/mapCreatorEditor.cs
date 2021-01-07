using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
//a simple editor to add a button in the ragdollBuilder script inspector
[CustomEditor(typeof(mapCreator))]
public class mapCreatorEditor : Editor{
	SerializedObject list;
	mapCreator map;
	void OnEnable(){
		list = new SerializedObject(target);
		map = (mapCreator)target;
	}
	void OnSceneGUI(){   
		if (!Application.isPlaying) {
			map = (mapCreator)target;
			if (map.showGizmo) {
				for (int i = 0; i < map.floorsList.Count; i++) {
					Handles.color = Color.red;
					Handles.Label ( map.floorsList[i].floor.transform.position, map.floorsList[i].Name);						
				}
			}
		}
	}
	public override void OnInspectorGUI(){
		if (list == null) {
			return;
		}
		list.Update ();
		GUILayout.BeginVertical("box");
		EditorGUILayout.PropertyField(list.FindProperty("floorMaterial"), new GUIContent("Floor Material"), false);
		EditorGUILayout.PropertyField(list.FindProperty("mapLayer"), new GUIContent("Map Layer"), false);
		EditorGUILayout.PropertyField(list.FindProperty("showGizmo"), new GUIContent("Show Gizmo"), false);
		GUILayout.EndVertical();
		EditorGUILayout.Space();
		GUILayout.BeginVertical ("Floor List", "window", GUILayout.Height (50));
		showUpperList(list.FindProperty("floorsList"));
		GUILayout.EndVertical();
		if (GUI.changed){
			list.ApplyModifiedProperties();
		}
		EditorGUILayout.Space();
	}
	void showListElementInfo(SerializedProperty list,bool showListNames, int index){
		GUILayout.BeginVertical("box");
		EditorGUILayout.PropertyField(list.FindPropertyRelative("Name"), new GUIContent("Name"), false);
		EditorGUILayout.PropertyField(list.FindPropertyRelative("floorNumber"), new GUIContent("Floor Number"), false);
		EditorGUILayout.PropertyField(list.FindPropertyRelative("floor"), new GUIContent("floor"), false);
		if (showListNames) {
			showLowerList (list.FindPropertyRelative ("mapPartsList"),index);
		}
		GUILayout.EndVertical();
	}
	void showUpperList(SerializedProperty list){
		GUILayout.BeginVertical();
		EditorGUILayout.PropertyField(list);
		if (list.isExpanded){
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add Floor")){
				map.addNewFloor ();
			}
			if (GUILayout.Button("Clear List")){
				list.arraySize = 0;
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();
			for (int i = 0; i < list.arraySize; i++){
				GUILayout.BeginHorizontal();
				GUILayout.BeginHorizontal("box");
				EditorGUILayout.Space();
				if (i < list.arraySize && i >= 0){
					EditorGUILayout.BeginVertical();
					EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i));
					if (list.GetArrayElementAtIndex (i).isExpanded) {
						showListElementInfo (list.GetArrayElementAtIndex (i), true, i);
					}
					EditorGUILayout.Space();
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
				if (GUILayout.Button("x")){
					GameObject floor =list.GetArrayElementAtIndex (i).FindPropertyRelative ("floor").objectReferenceValue as GameObject;
					DestroyImmediate (floor);
					list.DeleteArrayElementAtIndex(i);
				}
				GUILayout.EndHorizontal();
			}
		}
		GUILayout.EndVertical();
	}
	void showLowerList(SerializedProperty list, int index){
		EditorGUILayout.PropertyField(list);
		if (list.isExpanded){
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add Floor Part")){
				map.addNewMapPartFromMapCreator (index);
			}
			if (GUILayout.Button("Clear")){
				list.arraySize=0;
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();
			for (int i = 0; i < list.arraySize; i++){
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("x")){
					GameObject mapPart =list.GetArrayElementAtIndex (i).objectReferenceValue as GameObject;
					DestroyImmediate (mapPart);
					list.DeleteArrayElementAtIndex(i);
					list.DeleteArrayElementAtIndex(i);
				}
				if (i < list.arraySize && i >= 0) {
					EditorGUILayout.PropertyField (list.GetArrayElementAtIndex (i), new GUIContent ("", null, ""));
				}
				GUILayout.EndHorizontal();
			}
		}       
	}
}
#endif