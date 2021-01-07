using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(footStepManager))]
[CanEditMultipleObjects]
public class footStepsManagerEditor : Editor{
	SerializedObject list;
	void OnEnable(){
		list = new SerializedObject(target);
	}
	public override void OnInspectorGUI(){
		if (list == null) {
			return;
		}
		list.Update ();
		GUILayout.BeginVertical("box");
		EditorGUILayout.PropertyField(list.FindProperty("soundsEnabled"));
		if (list.FindProperty ("soundsEnabled").boolValue) {
			EditorGUILayout.PropertyField (list.FindProperty ("character"));
			EditorGUILayout.PropertyField (list.FindProperty ("feetVolume"));
			EditorGUILayout.PropertyField (list.FindProperty ("stepInterval"));
			EditorGUILayout.PropertyField (list.FindProperty ("typeOfFootStep"));
			EditorGUILayout.PropertyField (list.FindProperty ("layer"));
			GUILayout.BeginVertical ("Foot Prints Settings", "window", GUILayout.Height (30));
			EditorGUILayout.PropertyField (list.FindProperty ("useFootPrints"));
			if (list.FindProperty ("useFootPrints").boolValue) {
				EditorGUILayout.PropertyField (list.FindProperty ("rightFootPrint"));
				EditorGUILayout.PropertyField (list.FindProperty ("leftFootPrint"));
				EditorGUILayout.PropertyField (list.FindProperty ("maxFootPrintDistance"));
				EditorGUILayout.PropertyField (list.FindProperty ("useFootPrintMaxAmount"));
				if (list.FindProperty ("useFootPrintMaxAmount").boolValue) {
					EditorGUILayout.PropertyField (list.FindProperty ("footPrintMaxAmount"));
				}
				EditorGUILayout.PropertyField (list.FindProperty ("removeFootPrintsInTime"));
				if (list.FindProperty ("removeFootPrintsInTime").boolValue) {
					EditorGUILayout.PropertyField (list.FindProperty ("timeToRemoveFootPrints"));
				}
				EditorGUILayout.PropertyField (list.FindProperty ("vanishFootPrints"));
				if (list.FindProperty ("vanishFootPrints").boolValue) {
					EditorGUILayout.PropertyField (list.FindProperty ("vanishSpeed"));
				}
			}
			GUILayout.EndVertical ();

			EditorGUILayout.Space ();

			GUILayout.BeginVertical ("Foot Prints Settings", "window", GUILayout.Height (30));
			EditorGUILayout.PropertyField (list.FindProperty ("useFootParticles"));
			if (list.FindProperty ("useFootParticles").boolValue) {
				EditorGUILayout.PropertyField (list.FindProperty ("footParticles"));
			}
			GUILayout.EndVertical ();

			EditorGUILayout.Space ();

			GUILayout.BeginVertical ("Foot Steps List", "window");
			showUpperList (list.FindProperty ("footSteps"));
			GUILayout.EndVertical ();

		}

		GUILayout.EndVertical();
		if (GUI.changed){
			list.ApplyModifiedProperties();
		}
	}
	void showListElementInfo(SerializedProperty list,bool showListNames){
		GUILayout.BeginVertical("box");
		EditorGUILayout.PropertyField(list.FindPropertyRelative("Name"));
		if (showListNames) {
			showLowerList (list.FindPropertyRelative ("poolSounds"));
		}
		EditorGUILayout.PropertyField(list.FindPropertyRelative("layerName"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("checkLayer"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("terrainTextureName"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("checkTerrain"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("terrainTextureIndex"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("randomPool"));
		GUILayout.EndVertical();
	}
	void showUpperList(SerializedProperty list){
		GUILayout.BeginVertical();
		EditorGUILayout.PropertyField(list);
		if (list.isExpanded){
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add Surface")){
				list.arraySize++;
			}
			if (GUILayout.Button("Clear")){
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
						showListElementInfo (list.GetArrayElementAtIndex (i), true);
					}
					EditorGUILayout.Space();
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
				if (GUILayout.Button("x")){
					list.DeleteArrayElementAtIndex(i);
				}
				GUILayout.EndHorizontal();
			}
		}
		GUILayout.EndVertical();
	}
	void showLowerList(SerializedProperty list){
		EditorGUILayout.PropertyField(list);
		if (list.isExpanded){
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add Sound")){
				list.arraySize++;
			}
			if (GUILayout.Button("Clear")){
				list.arraySize=0;
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();
			for (int i = 0; i < list.arraySize; i++){
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("x")){
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