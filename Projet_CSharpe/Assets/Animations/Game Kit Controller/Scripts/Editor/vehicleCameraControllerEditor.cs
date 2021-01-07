using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(vehicleCameraController))]
[CanEditMultipleObjects]
public class vehicleCameraControllerEditor : Editor{
	SerializedObject list;
	bool shakeSettings;
	Color defBackgroundColor;

	void OnEnable(){
		list = new SerializedObject(target);
	}
	void OnSceneGUI(){   
		if (!Application.isPlaying) {
			vehicleCameraController camera = (vehicleCameraController)target;
			if (camera.showGizmo) {
				if (camera.gameObject == Selection.activeGameObject) {
					GUIStyle style = new GUIStyle();
					style.normal.textColor = list.FindProperty("labelGizmoColor").colorValue;
					style.alignment = TextAnchor.MiddleCenter;
					for (int i = 0; i < camera.vehicleCameraStates.Count; i++) {
						if (camera.vehicleCameraStates [i].showGizmo) {
							Handles.color = camera.vehicleCameraStates [i].gizmoColor;
							Handles.Label (camera.vehicleCameraStates [i].cameraTransform.position+(camera.transform.up*camera.vehicleCameraStates [i].labelGizmoOffset), camera.vehicleCameraStates [i].name,style);						
						}
					}
				}    
			}
		}
	}
	public override void OnInspectorGUI(){
		if (list == null) {
			return;
		}
		list.Update ();
		GUILayout.BeginVertical ("box");
		EditorGUILayout.PropertyField (list.FindProperty ("rotationSpeed"));
		EditorGUILayout.PropertyField (list.FindProperty ("clipCastRadius"));
		EditorGUILayout.PropertyField (list.FindProperty ("backClipSpeed"));
		EditorGUILayout.PropertyField (list.FindProperty ("maximumBoostDistance"));
		EditorGUILayout.PropertyField (list.FindProperty ("cameraBoostSpeed"));
		EditorGUILayout.PropertyField (list.FindProperty ("rotationDamping"));
		EditorGUILayout.PropertyField (list.FindProperty ("cameraChangeEnabled"));
		EditorGUILayout.PropertyField (list.FindProperty ("smoothBetweenState"));
		EditorGUILayout.PropertyField (list.FindProperty ("currentStateName"));
		EditorGUILayout.PropertyField (list.FindProperty ("vehicle"));
		EditorGUILayout.PropertyField (list.FindProperty ("layer"));
		EditorGUILayout.PropertyField (list.FindProperty ("zoomEnabled"));
		if (list.FindProperty ("zoomEnabled").boolValue) {
			EditorGUILayout.PropertyField (list.FindProperty ("zoomSpeed"));
			EditorGUILayout.PropertyField (list.FindProperty ("zoomFovValue"));
			EditorGUILayout.PropertyField (list.FindProperty ("rotationSpeedZoomIn"));
		}
		EditorGUILayout.PropertyField (list.FindProperty ("showGizmo"));
		if (list.FindProperty ("showGizmo").boolValue) {
			EditorGUILayout.PropertyField (list.FindProperty ("gizmoRadius"));
			EditorGUILayout.PropertyField (list.FindProperty ("labelGizmoColor"));
		}
		GUILayout.EndVertical ();

		GUILayout.BeginVertical ("Vehicle Camera States", "window", GUILayout.Height(30));
		showUpperList (list.FindProperty ("vehicleCameraStates"));
		GUILayout.EndVertical ();
		EditorGUILayout.Space ();

		GUILayout.BeginVertical ("Vehicle Camera Current State ", "window");
		string cameraPaused = "-";
		if (Application.isPlaying) {
			if (list.FindProperty ("cameraPaused").boolValue) {
				cameraPaused = "YES";
			} else {
				cameraPaused = "NO";
			}
		} 
		string isFirstPerson = "-";
		if (Application.isPlaying) {
			if (list.FindProperty ("isFirstPerson").boolValue) {
				isFirstPerson = "YES";
			} else {
				isFirstPerson = "NO";
			}
		} 
		string usingZoomOn = "-";
		if (Application.isPlaying) {
			if (list.FindProperty ("usingZoomOn").boolValue) {
				usingZoomOn = "YES";
			} else {
				usingZoomOn = "NO";
			}
		}
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("Camera Paused ");
		GUILayout.Label (cameraPaused);
		GUILayout.EndHorizontal ();
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("First Person View ");
		GUILayout.Label (isFirstPerson);
		GUILayout.EndHorizontal ();
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("Using Zoom ");
		GUILayout.Label (usingZoomOn);
		GUILayout.EndHorizontal ();
		GUILayout.EndVertical ();

		EditorGUILayout.Space ();

		defBackgroundColor = GUI.backgroundColor;
		EditorGUILayout.BeginVertical();
		if (shakeSettings) {
			GUI.backgroundColor = Color.gray;
		} else {
			GUI.backgroundColor = defBackgroundColor;
		}
		if (GUILayout.Button ("Shake Settings")) {
			shakeSettings = !shakeSettings;
		}

		GUI.backgroundColor = defBackgroundColor;
		EditorGUILayout.EndVertical();
		if (shakeSettings) {
			EditorGUILayout.Space ();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox ("Shake Settings when the vehicle receives Damage", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space ();
			GUILayout.BeginVertical ("box");
			EditorGUILayout.PropertyField (list.FindProperty ("shakeSettings.useDamageShake"));
			if (list.FindProperty ("shakeSettings.useDamageShake").boolValue) {
				EditorGUILayout.Space ();
				EditorGUILayout.PropertyField (list.FindProperty ("shakeSettings.useDamageShakeInThirdPerson"));
				if (list.FindProperty ("shakeSettings.useDamageShakeInThirdPerson").boolValue) {
					showShakeInfo (list.FindProperty ("shakeSettings.thirdPersonDamageShake"));
					EditorGUILayout.Space ();
				}
				EditorGUILayout.PropertyField (list.FindProperty ("shakeSettings.useDamageShakeInFirstPerson"));
				if (list.FindProperty ("shakeSettings.useDamageShakeInFirstPerson").boolValue) {
					showShakeInfo (list.FindProperty ("shakeSettings.firstPersonDamageShake"));
					EditorGUILayout.Space ();
				}
			}
		}

		if (GUI.changed) {
			list.ApplyModifiedProperties ();
		}
		EditorGUILayout.Space ();
	}
	void showCameraStateElementInfo(SerializedProperty list){
		Color listButtonBackgroundColor;
		bool listGizmoSettings = list.FindPropertyRelative ("gizmoSettings").boolValue;
		GUILayout.BeginVertical ("box");
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("name"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("pivotTransform"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("cameraTransform"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("xLimits"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("yLimits"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("enabled"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("firstPersonCamera"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("cameraFixed"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("smoothTransition"));
		listButtonBackgroundColor = GUI.backgroundColor;
		EditorGUILayout.BeginHorizontal ();
		if (listGizmoSettings) {
			GUI.backgroundColor = Color.gray;
		} else {
			GUI.backgroundColor = listButtonBackgroundColor;
		}
		if (GUILayout.Button ("Gizmo Settings")) {
			listGizmoSettings = !listGizmoSettings;
		}
		GUI.backgroundColor = listButtonBackgroundColor;
		EditorGUILayout.EndHorizontal ();
		list.FindPropertyRelative ("gizmoSettings").boolValue = listGizmoSettings;
		if (listGizmoSettings) {
			EditorGUILayout.Space ();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox ("Camera State Gizmo Settings", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space ();
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("showGizmo"));
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("gizmoColor"));
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("labelGizmoOffset"));
			EditorGUILayout.Space ();
		}
		GUILayout.EndVertical ();
	}
	void showUpperList(SerializedProperty list){
		GUILayout.BeginVertical ();
		EditorGUILayout.PropertyField (list);
		if (list.isExpanded) {
			EditorGUILayout.Space ();
			GUILayout.Label ("Number Of States: \t" + list.arraySize.ToString ());
			EditorGUILayout.Space ();
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Add State")) {
				list.arraySize++;
			}
			if (GUILayout.Button ("Clear")) {
				list.arraySize = 0;
			}
			GUILayout.EndHorizontal ();
			EditorGUILayout.Space ();
			for (int i = 0; i < list.arraySize; i++) {
				bool expanded = false;
				GUILayout.BeginHorizontal ();
				GUILayout.BeginHorizontal ("box");
				EditorGUILayout.Space ();
				if (i < list.arraySize && i >= 0) {
					EditorGUILayout.BeginVertical ();
					EditorGUILayout.PropertyField (list.GetArrayElementAtIndex (i));
					if (list.GetArrayElementAtIndex (i).isExpanded) {
						showCameraStateElementInfo (list.GetArrayElementAtIndex (i));
						expanded = true;
					}
					EditorGUILayout.Space ();
					GUILayout.EndVertical ();
				}
				GUILayout.EndHorizontal ();
				if (expanded) {
					GUILayout.BeginVertical ();
				} else {
					GUILayout.BeginHorizontal ();
				}
				if (GUILayout.Button ("x")) {
					list.DeleteArrayElementAtIndex (i);
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
				if (expanded) {
					GUILayout.EndVertical ();
				} else {
					GUILayout.EndHorizontal ();
				}
				GUILayout.EndHorizontal ();
			}
		}
		GUILayout.EndVertical ();
	}
	void showShakeInfo(SerializedProperty list){
		GUILayout.BeginVertical ("box");
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("shakeRotation"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("shakeRotationSpeed"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("shakeRotationSmooth"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("shakeDuration"));
		GUILayout.EndVertical ();
	}
}
#endif