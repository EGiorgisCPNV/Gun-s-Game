using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
//a simple editor to add a button in the ragdollBuilder script inspector
[CustomEditor(typeof(IKDrivingSystem))]
public class IKDrivingSystemEditor : Editor{
	SerializedObject list;
	IKDrivingSystem drivingManager;
	void OnEnable(){
		list = new SerializedObject(target);
		drivingManager = (IKDrivingSystem)target;
	}
	void OnSceneGUI(){   
		if (!Application.isPlaying) {
			drivingManager = (IKDrivingSystem)target;
			if (drivingManager.showGizmo) {
				GUIStyle style = new GUIStyle ();
				style.normal.textColor = drivingManager.gizmoLabelColor;
				style.alignment = TextAnchor.MiddleCenter;
				for (int i = 0; i < drivingManager.IKDrivingInfo.IKDrivingPos.Count; i++) {
					if (drivingManager.IKDrivingInfo.IKDrivingPos [i].position) {
						Handles.Label (drivingManager.IKDrivingInfo.IKDrivingPos [i].position.position, drivingManager.IKDrivingInfo.IKDrivingPos [i].Name, style);	
					}
				}
				for (int i = 0; i < drivingManager.IKDrivingInfo.IKDrivingKneePos.Count; i++) {
					if (drivingManager.IKDrivingInfo.IKDrivingKneePos [i].position) {
						Handles.Label (drivingManager.IKDrivingInfo.IKDrivingKneePos [i].position.position, drivingManager.IKDrivingInfo.IKDrivingKneePos [i].Name, style);	
					}
				}
				if (drivingManager.IKDrivingInfo.bodyPosition) {
					Handles.Label (drivingManager.IKDrivingInfo.bodyPosition.position, "Body Position", style);	
				}
				if (drivingManager.IKDrivingInfo.steerDirecion) {
					Handles.Label (drivingManager.IKDrivingInfo.steerDirecion.position, "Steer Position", style);
				}
				if (drivingManager.useExplosionForceWhenDestroyed) {
					Handles.Label (drivingManager.gameObject.transform.position + drivingManager.gameObject.transform.up * drivingManager.explosionRadius, 
						"Explosion Radius "+drivingManager.explosionRadius.ToString()+"\n"+"Explosion Force "+drivingManager.explosionForce, style);
				}
			}
		}
	}
	bool settings;
	Color defBackgroundColor;
	public override void OnInspectorGUI(){
		if (list == null) {
			return;
		}
		list.Update ();
		GUILayout.BeginVertical("box");
		EditorGUILayout.PropertyField(list.FindProperty("hidePlayerFromNPCs"), new GUIContent("Hide Player From NPCs"), false);
		EditorGUILayout.PropertyField(list.FindProperty("playerVisibleInVehicle"));
		EditorGUILayout.PropertyField(list.FindProperty("ejectPlayerWhenDestroyed"));
		if (list.FindProperty ("ejectPlayerWhenDestroyed").boolValue) {
			EditorGUILayout.PropertyField (list.FindProperty ("ejectingPlayerForce"));
		}
		EditorGUILayout.PropertyField(list.FindProperty("useExplosionForceWhenDestroyed"));
		if (list.FindProperty ("useExplosionForceWhenDestroyed").boolValue) {
			EditorGUILayout.PropertyField (list.FindProperty ("explosionRadius"));
			EditorGUILayout.PropertyField (list.FindProperty ("explosionForce"));
			EditorGUILayout.PropertyField (list.FindProperty ("explosionDamage"));
		}
		EditorGUILayout.PropertyField(list.FindProperty("showGizmo"));
		if (list.FindProperty ("showGizmo").boolValue) {
			EditorGUILayout.PropertyField (list.FindProperty ("gizmoLabelColor"));
			EditorGUILayout.PropertyField (list.FindProperty ("gizmoRadius"));
		}
		EditorGUILayout.Space();

		defBackgroundColor = GUI.backgroundColor;
		EditorGUILayout.BeginHorizontal();
		if (settings) {
			GUI.backgroundColor = Color.gray;
		} else {
			GUI.backgroundColor = defBackgroundColor;
		}
		if (GUILayout.Button ("Settings")) {
			settings = !settings;
		}
		GUI.backgroundColor = defBackgroundColor;
		EditorGUILayout.EndHorizontal();
		if(settings){
			EditorGUILayout.Space();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox("IK positions in vehicle", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			GUILayout.BeginVertical("IK Positions List", "window",GUILayout.Height (50));
			showUpperList(list.FindProperty("IKDrivingInfo.IKDrivingPos"));
			showLowerList(list.FindProperty("IKDrivingInfo.IKDrivingKneePos"));
			EditorGUILayout.PropertyField(list.FindProperty("IKDrivingInfo.bodyPosition"), new GUIContent("Body Position"), false);
			EditorGUILayout.PropertyField(list.FindProperty("IKDrivingInfo.steerDirecion"), new GUIContent("Steer Direcion"), false);
			GUILayout.EndVertical();
			EditorGUILayout.Space();
		}
		GUI.backgroundColor = defBackgroundColor;
		GUILayout.EndVertical();
		if (GUI.changed){
			list.ApplyModifiedProperties();
		}
		EditorGUILayout.Space();
	}
	void showUpperListElementInfo(SerializedProperty list,bool showListNames){
		GUILayout.BeginVertical("box");
		EditorGUILayout.PropertyField(list.FindPropertyRelative("Name"), new GUIContent("Name"), false);
		EditorGUILayout.PropertyField(list.FindPropertyRelative("limb"), new GUIContent("Limb"), false);
		EditorGUILayout.PropertyField(list.FindPropertyRelative("position"), new GUIContent("Position Transform"), false);
		GUILayout.EndVertical();
	}
	void showUpperList(SerializedProperty list){
		GUILayout.BeginVertical();
		EditorGUILayout.PropertyField(list, new GUIContent("IK Hint List"), false);
		if (list.isExpanded){
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add IK Pos")){
				list.arraySize++;
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
						showUpperListElementInfo (list.GetArrayElementAtIndex (i), true);
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
	void showLowerListElementInfo(SerializedProperty list,bool showListNames){
		GUILayout.BeginVertical("box");
		EditorGUILayout.PropertyField(list.FindPropertyRelative("Name"), new GUIContent("Name"), false);
		EditorGUILayout.PropertyField(list.FindPropertyRelative("knee"), new GUIContent("Limb"), false);
		EditorGUILayout.PropertyField(list.FindPropertyRelative("position"), new GUIContent("Position Transform"), false);
		GUILayout.EndVertical();
	}
	void showLowerList(SerializedProperty list){
		GUILayout.BeginVertical();
		EditorGUILayout.PropertyField(list, new GUIContent("IK Goal List"), false);
		if (list.isExpanded){
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add IK Pos")){
				list.arraySize++;
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
						showLowerListElementInfo (list.GetArrayElementAtIndex (i), true);
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
}
#endif