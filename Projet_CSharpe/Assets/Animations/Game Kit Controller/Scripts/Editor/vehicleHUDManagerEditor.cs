using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
//a simple editor to add a button in the ragdollBuilder script inspector
[CustomEditor(typeof(vehicleHUDManager))]
public class vehicleHUDManagerEditor : Editor{
	SerializedObject list;
	vehicleHUDManager vehicleHUD;
	GUIStyle style = new GUIStyle();
	bool useWeakSpots;

	void OnEnable(){
		list = new SerializedObject(target);
		vehicleHUD = (vehicleHUDManager)target;
	}
	void OnSceneGUI(){   
		if (!Application.isPlaying) {
			vehicleHUD = (vehicleHUDManager)target;
			if (vehicleHUD.advancedSettings.showGizmo) {
				for (int i = 0; i < vehicleHUD.advancedSettings.damageReceiverList.Count; i++) {
					if (vehicleHUD.advancedSettings.damageReceiverList [i].spotTransform) {
						style.normal.textColor = vehicleHUD.advancedSettings.gizmoLabelColor;
						style.alignment = TextAnchor.MiddleCenter;
						string label = vehicleHUD.advancedSettings.damageReceiverList [i].name;
						if (vehicleHUD.advancedSettings.damageReceiverList [i].killedWithOneShoot) {
							if (vehicleHUD.advancedSettings.damageReceiverList [i].needMinValueToBeKilled) {
								label +="\nOne Shoot\n >=" + vehicleHUD.advancedSettings.damageReceiverList [i].minValueToBeKilled;
							} else {
								label += "\nOne Shoot";	
							}
						} else {
							label += "\nx" + vehicleHUD.advancedSettings.damageReceiverList [i].damageMultiplier;
						}

						Handles.Label (vehicleHUD.advancedSettings.damageReceiverList [i].spotTransform.position, label, style);	
					}
				}
				style.normal.textColor = vehicleHUD.advancedSettings.gizmoLabelColor;
				style.alignment = TextAnchor.MiddleCenter;
				Handles.Label (vehicleHUD.transform.position + vehicleHUD.transform.right * vehicleHUD.rightGetOffDistance + vehicleHUD.transform.up * vehicleHUD.getOffHeight + vehicleHUD.transform.forward * vehicleHUD.getOffForward,
				"right Get \n Off Ray", style);	
				Handles.Label (vehicleHUD.transform.position - vehicleHUD.transform.right * vehicleHUD.leftGetOffDistance + vehicleHUD.transform.up * vehicleHUD.getOffHeight + vehicleHUD.transform.forward * vehicleHUD.getOffForward,
					"left Get \n Off Ray", style);	
			}
		}
	}
	bool advancedSettings;
	Color defBackgroundColor;
	public override void OnInspectorGUI(){
		if (list == null) {
			return;
		}
		list.Update ();
		GUILayout.BeginVertical("box");
		EditorGUILayout.PropertyField(list.FindProperty("healthAmount"), new GUIContent("Health Amount"), false);
		EditorGUILayout.PropertyField(list.FindProperty("boostAmount"), new GUIContent("Boost Amount"), false);
		EditorGUILayout.PropertyField(list.FindProperty("regenerateHealthSpeed"), new GUIContent("Regenerate Health Speed"), false);
		EditorGUILayout.PropertyField(list.FindProperty("regenerateBoostSpeed"), new GUIContent("Regenerate Boost Speed"), false);
		EditorGUILayout.PropertyField(list.FindProperty("boostUseRate"), new GUIContent("Boost Use Rate"), false);
		EditorGUILayout.PropertyField(list.FindProperty("invincible"), new GUIContent("Invincible"), false);
		EditorGUILayout.PropertyField(list.FindProperty("dead"), new GUIContent("Dead"), false);
		EditorGUILayout.PropertyField(list.FindProperty("destroyedSound"), new GUIContent("Destroyed Sound"), false);
		EditorGUILayout.PropertyField(list.FindProperty("destroyedSource"), new GUIContent("Destroyed Source"), false);
		EditorGUILayout.PropertyField(list.FindProperty("layer"), new GUIContent("Layer"), false);
		EditorGUILayout.PropertyField(list.FindProperty("leftGetOffDistance"), new GUIContent("Left Get Off Distance"), false);
		EditorGUILayout.PropertyField(list.FindProperty("rightGetOffDistance"), new GUIContent("Right Get Off Distance"), false);
		EditorGUILayout.PropertyField(list.FindProperty("getOffHeight"), new GUIContent("Get Off Height"), false);
		EditorGUILayout.PropertyField(list.FindProperty("getOffForward"), new GUIContent("Get Off Forward"), false);
		EditorGUILayout.PropertyField(list.FindProperty("getOffPlace"), new GUIContent("Get Off Place"), false);
		EditorGUILayout.PropertyField(list.FindProperty("damageParticles"), new GUIContent("Damage Particles"), false);
		EditorGUILayout.PropertyField(list.FindProperty("destroyedParticles"), new GUIContent("Destroyed Particles"), false);
		EditorGUILayout.PropertyField(list.FindProperty("healthPercentageDamageParticles"), new GUIContent("Health Percentage Damage Particles"), false);
		EditorGUILayout.PropertyField(list.FindProperty("extraGrabDistance"), new GUIContent("Extra Grab Distance"), false);
		EditorGUILayout.PropertyField(list.FindProperty("placeToShoot"), new GUIContent("Place To Shoot"), false);
		EditorGUILayout.PropertyField(list.FindProperty("timeToFadePieces"), new GUIContent("Time To Fade Pieces"), false);
		GUILayout.EndVertical();
		EditorGUILayout.Space();

		defBackgroundColor = GUI.backgroundColor;
		EditorGUILayout.BeginHorizontal();
		if (advancedSettings) {
			GUI.backgroundColor = Color.gray;
		} else {
			GUI.backgroundColor = defBackgroundColor;
		}
		if (GUILayout.Button ("Advanced Settings")) {
			advancedSettings = !advancedSettings;
		}
		GUI.backgroundColor = defBackgroundColor;
		EditorGUILayout.EndHorizontal();
		if (advancedSettings) {
			EditorGUILayout.Space();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox("Check all the damage receivers in this vehicle", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(list.FindProperty("damageMultiplierOnCollision"));
			EditorGUILayout.PropertyField(list.FindProperty("useWeakSpots"));
			useWeakSpots = list.FindProperty ("useWeakSpots").boolValue;
			EditorGUILayout.PropertyField(list.FindProperty("advancedSettings.showGizmo"));
			EditorGUILayout.PropertyField(list.FindProperty("advancedSettings.gizmoLabelColor"));
			EditorGUILayout.PropertyField(list.FindProperty("advancedSettings.gizmoRadius"));
			EditorGUILayout.PropertyField(list.FindProperty("advancedSettings.alphaColor"));
			GUILayout.BeginVertical("Damage Receiver List", "window");
			showUpperList(list.FindProperty("advancedSettings.damageReceiverList"));
			GUILayout.EndVertical();
			EditorGUILayout.Space();
			if (GUILayout.Button("Update Damage Receivers")){
				vehicleHUD.updateDamageReceivers ();
			}
			EditorGUILayout.Space();
		}
		GUI.backgroundColor = defBackgroundColor;
		if (GUI.changed){
			list.ApplyModifiedProperties();
		}
		EditorGUILayout.Space();
	}
	void showListElementInfo(SerializedProperty list,bool showListNames){
		GUILayout.BeginVertical("box");
		EditorGUILayout.PropertyField(list.FindPropertyRelative("name"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("spotTransform"));
		if (!useWeakSpots || !list.FindPropertyRelative ("killedWithOneShoot").boolValue) {
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("damageMultiplier"));
		}
		if (useWeakSpots) {
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("killedWithOneShoot"));
			if (list.FindPropertyRelative ("killedWithOneShoot").boolValue) {
				EditorGUILayout.PropertyField (list.FindPropertyRelative ("needMinValueToBeKilled"));
				if (list.FindPropertyRelative ("needMinValueToBeKilled").boolValue) {
					EditorGUILayout.PropertyField (list.FindPropertyRelative ("minValueToBeKilled"));
				}
			}
		} 

		GUILayout.EndVertical();
	}
	void showUpperList(SerializedProperty list){
		GUILayout.BeginVertical();
		EditorGUILayout.PropertyField(list);
		if (list.isExpanded){
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Get List")){
				vehicleHUD.getAllDamageReceivers ();
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
						showListElementInfo (list.GetArrayElementAtIndex (i), true);
					}
					EditorGUILayout.Space();
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
				GUILayout.EndHorizontal();
			}
		}
		GUILayout.EndVertical();
	}
}
#endif