using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
//a simple editor to add a button in the ragdollBuilder script inspector
[CustomEditor(typeof(health))]
[CanEditMultipleObjects]
public class healthEditor : Editor{
	SerializedObject list;
	health healthManager;
	GUIStyle style = new GUIStyle ();

	void OnEnable(){
		list = new SerializedObject(target);
		healthManager = (health)target;
	}
	void OnSceneGUI(){   
		//if (!Application.isPlaying) {
			healthManager = (health)target;
		if (healthManager.advancedSettings.showGizmo) {
			for (int i = 0; i < healthManager.advancedSettings.weakSpots.Count; i++) {
				if (healthManager.advancedSettings.weakSpots [i].spotTransform) {
					style.normal.textColor = healthManager.advancedSettings.gizmoLabelColor;
					style.alignment = TextAnchor.MiddleCenter;
					string label = healthManager.advancedSettings.weakSpots [i].name;
					if (healthManager.advancedSettings.weakSpots [i].killedWithOneShoot) {
						if (healthManager.advancedSettings.weakSpots [i].needMinValueToBeKilled) {
							label +="\nOne Shoot\n >=" + healthManager.advancedSettings.weakSpots [i].minValueToBeKilled;
						} else {
							label += "\nOne Shoot";	
						}
					} else {
						label += "\nx" + healthManager.advancedSettings.weakSpots [i].damageMultiplier;
					}

					Handles.Label (healthManager.advancedSettings.weakSpots [i].spotTransform.position, label, style);	
				}
			}
		}
		//}
	}
	bool settings;
	bool advancedSettings;
	Color defBackgroundColor;
	public override void OnInspectorGUI(){
		if (list == null) {
			return;
		}
		list.Update ();
		GUILayout.BeginVertical ("box");
		EditorGUILayout.PropertyField (list.FindProperty ("invincible"));
		if (!list.FindProperty ("invincible").boolValue) {
			EditorGUILayout.PropertyField (list.FindProperty ("healthAmount"));
			EditorGUILayout.PropertyField (list.FindProperty ("regenerateSpeed"));
			EditorGUILayout.PropertyField (list.FindProperty ("dead"));
		}
		EditorGUILayout.PropertyField(list.FindProperty("damagePrefab"));
		EditorGUILayout.PropertyField(list.FindProperty("placeToShoot"));
		EditorGUILayout.PropertyField(list.FindProperty("scorchMarkPrefab"));
		EditorGUILayout.PropertyField(list.FindProperty("damageFunction"));
		EditorGUILayout.PropertyField(list.FindProperty("deadFuncion"));
		EditorGUILayout.PropertyField(list.FindProperty("useExtraDeadFunctions"));
		if (list.FindProperty ("useExtraDeadFunctions").boolValue) {
			EditorGUILayout.Space();
			GUILayout.BeginVertical("Extra Function List", "window");
			extraFunctionList (list.FindProperty ("extraDeadFunctionList"));
			GUILayout.EndVertical ();
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
		if(settings){
			EditorGUILayout.Space();
			GUILayout.BeginVertical("box");
			EditorGUILayout.Space();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox("Enemy/Friend Settings", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(list.FindProperty("settings.enemyHealthSlider"), new GUIContent("Enemy Health Slider"), false);
			EditorGUILayout.PropertyField(list.FindProperty("settings.sliderOffset"), new GUIContent("Slider Offset"), false);
			EditorGUILayout.PropertyField(list.FindProperty("settings.layer"), new GUIContent("Layer"), false);
			EditorGUILayout.PropertyField(list.FindProperty("settings.enemyName"), new GUIContent("Enemy Name"), false);
			EditorGUILayout.PropertyField(list.FindProperty("settings.allyName"), new GUIContent("Ally Name"), false);
			EditorGUILayout.Space();
			GUILayout.EndVertical ();
		}
		if (advancedSettings) {
			EditorGUILayout.Space ();
			GUILayout.BeginVertical ("box");
			EditorGUILayout.Space ();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox ("Ragdoll and weak spots Settings", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space ();
			EditorGUILayout.Space ();
			EditorGUILayout.PropertyField (list.FindProperty ("advancedSettings.notHuman"));
			EditorGUILayout.PropertyField (list.FindProperty ("advancedSettings.useWeakSpots"), new GUIContent ("Use Weak Spots"), false);
			EditorGUILayout.PropertyField (list.FindProperty ("advancedSettings.haveRagdoll"), new GUIContent ("Have Ragdoll"), false);
			EditorGUILayout.PropertyField (list.FindProperty ("advancedSettings.minDamageToEnableRagdoll"), new GUIContent ("Min Damage To Enable Ragdoll"), false);
			EditorGUILayout.PropertyField (list.FindProperty ("advancedSettings.functionToRagdoll"), new GUIContent ("Function To Ragdoll"), false);
			EditorGUILayout.PropertyField (list.FindProperty ("advancedSettings.showGizmo"), new GUIContent ("Show Gizmo"), false);
			EditorGUILayout.PropertyField (list.FindProperty ("advancedSettings.gizmoLabelColor"), new GUIContent ("Gizmo Label Color"), false);
			EditorGUILayout.PropertyField (list.FindProperty ("advancedSettings.gizmoRadius"), new GUIContent ("Gizmo Radius"), false);
			EditorGUILayout.PropertyField (list.FindProperty ("advancedSettings.alphaColor"), new GUIContent ("Alpha Color"), false);
			EditorGUILayout.Space ();
			GUILayout.BeginVertical ("Weak Spots List", "window");
			showUpperList (list.FindProperty ("advancedSettings.weakSpots"));
			GUILayout.EndVertical ();
			EditorGUILayout.Space ();
			if (GUILayout.Button ("Update Damage Receivers")) {
				healthManager.updateDamageReceivers ();
			}
			EditorGUILayout.Space ();
			GUILayout.EndVertical ();
			EditorGUILayout.Space ();
		}
		EditorGUILayout.Space ();
		if (GUILayout.Button ("Kill Character (Only Ingame)")) {
			if (Application.isPlaying) {
				healthManager.killByButton ();
			}
		}
		EditorGUILayout.Space ();
		GUI.backgroundColor = defBackgroundColor;
		GUILayout.EndVertical();
		if (GUI.changed){
			list.ApplyModifiedProperties();
		}
	}
	void showListElementInfo(SerializedProperty list){
		GUILayout.BeginVertical("box");
		EditorGUILayout.PropertyField(list.FindPropertyRelative("name"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("spotTransform"));
		if (!list.FindPropertyRelative ("killedWithOneShoot").boolValue) {
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("damageMultiplier"));
		}
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("killedWithOneShoot"));
		if (list.FindPropertyRelative ("killedWithOneShoot").boolValue) {
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("needMinValueToBeKilled"));
			if (list.FindPropertyRelative ("needMinValueToBeKilled").boolValue) {
				EditorGUILayout.PropertyField (list.FindPropertyRelative ("minValueToBeKilled"));
			}
		}
		GUILayout.EndVertical();
	}
	void showUpperList(SerializedProperty list){
		GUILayout.BeginVertical();
		EditorGUILayout.PropertyField(list);
		if (list.isExpanded){
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add Spot")){
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
						showListElementInfo (list.GetArrayElementAtIndex (i));
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

	void extraFunctionList(SerializedProperty list){
		EditorGUILayout.PropertyField(list);
		if (list.isExpanded){
			EditorGUILayout.Space ();
			GUILayout.Label ("Number Of Functions: \t" + list.arraySize.ToString ());
			EditorGUILayout.Space ();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add Function")){
				list.arraySize++;
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