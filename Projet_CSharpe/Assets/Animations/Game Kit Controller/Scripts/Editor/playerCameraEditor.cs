using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor (typeof(playerCamera))]
public class playerCameraEditor : Editor
{
	SerializedObject list;

	void OnEnable ()
	{
		list = new SerializedObject (target);
	}

	public string currentCamera;
	bool checkCamera;
	bool settings;
	Color defBackgroundColor;

	void OnSceneGUI ()
	{   
		if (!Application.isPlaying) {
			playerCamera camera = (playerCamera)target;
			if (camera.settings.showCameraGizmo) {
				if (camera.gameObject == Selection.activeGameObject) {
					for (int i = 0; i < camera.playerCameraStates.Count; i++) {
						if (camera.playerCameraStates [i].showGizmo) {
							Handles.color = camera.playerCameraStates [i].gizmoColor;
							Handles.Label (camera.gameObject.transform.position + camera.playerCameraStates [i].pivotPositionOffset
							+ camera.playerCameraStates [i].camPositionOffset, camera.playerCameraStates [i].Name);						
						}
					}
				}    
			}
		}
	}

	public override void OnInspectorGUI ()
	{
		if (list == null) {
			return;
		}
		list.Update ();
		GUILayout.BeginVertical ("box");
		EditorGUILayout.PropertyField (list.FindProperty ("cameraType"));
		EditorGUILayout.PropertyField (list.FindProperty ("currentStateName"));
		EditorGUILayout.PropertyField (list.FindProperty ("rotationSpeed"));
		EditorGUILayout.PropertyField (list.FindProperty ("smoothBetweenState"));
		EditorGUILayout.PropertyField (list.FindProperty ("maxCheckDist"));
		EditorGUILayout.PropertyField (list.FindProperty ("movementLerpSpeed"));
		EditorGUILayout.PropertyField (list.FindProperty ("zoomSpeed"));
		EditorGUILayout.PropertyField (list.FindProperty ("fovChangeSpeed"));
		EditorGUILayout.PropertyField (list.FindProperty ("maxFovValue"));
		EditorGUILayout.PropertyField (list.FindProperty ("minFovValue"));
		EditorGUILayout.PropertyField (list.FindProperty ("rotationSpeedZoomIn"));
		GUILayout.EndVertical ();

		EditorGUILayout.Space ();

		GUILayout.BeginVertical ("Player Camera States", "window");
		showUpperList (list.FindProperty ("playerCameraStates"));
		GUILayout.EndVertical ();

		EditorGUILayout.Space ();

		GUILayout.BeginVertical ("Current Camera Action", "window");
		GUILayout.Label ("Grounded\t\t" + list.FindProperty ("grounded").boolValue.ToString ());
		GUILayout.Label ("Aiming\t\t" + list.FindProperty ("aiming").boolValue.ToString ());
		GUILayout.Label ("Move Away Active\t" + list.FindProperty ("moveAwayActive").boolValue.ToString ());
		GUILayout.Label ("Crouching\t\t" + list.FindProperty ("crouching").boolValue.ToString ());
		GUILayout.Label ("First Person Active\t" + list.FindProperty ("firstPersonActive").boolValue.ToString ());
		GUILayout.Label ("Using Zoom On\t" + list.FindProperty ("usingZoomOn").boolValue.ToString ());
		GUILayout.Label ("Using Zoom Off\t" + list.FindProperty ("usingZoomOff").boolValue.ToString ());
		GUILayout.Label ("Camera Can Rotate\t" + list.FindProperty ("cameraCanRotate").boolValue.ToString ());
		GUILayout.Label ("Camera Can Be Used\t" + list.FindProperty ("cameraCanBeUsed").boolValue.ToString ());
//		EditorGUILayout.PropertyField(list.FindProperty ("grounded"), new GUIContent("Grounded"), false);
//		EditorGUILayout.PropertyField(list.FindProperty("aiming"), new GUIContent("Aiming"), false);
//		EditorGUILayout.PropertyField(list.FindProperty("moveAwayActive"), new GUIContent("Move Away Active"), false);
//		EditorGUILayout.PropertyField(list.FindProperty("crouching"), new GUIContent("Crouching"), false);
//		EditorGUILayout.PropertyField(list.FindProperty("firstPersonActive"), new GUIContent("First Person Active"), false);
//		EditorGUILayout.PropertyField(list.FindProperty("usingZoomOn"), new GUIContent("Using Zoom On"), false);
//		EditorGUILayout.PropertyField(list.FindProperty("usingZoomOff"), new GUIContent("Using Zoom Off"), false);
//		EditorGUILayout.PropertyField(list.FindProperty("cameraCanRotate"), new GUIContent("Camera Can Rotate"), false);
		GUILayout.EndVertical ();
		EditorGUILayout.Space ();

		defBackgroundColor = GUI.backgroundColor;
		EditorGUILayout.BeginHorizontal ();
		if (settings) {
			GUI.backgroundColor = Color.gray;
		} else {
			GUI.backgroundColor = defBackgroundColor;
		}
		if (GUILayout.Button ("Settings")) {
			settings = !settings;
		}
		GUI.backgroundColor = defBackgroundColor;
		EditorGUILayout.EndHorizontal ();
		if (settings) {
			EditorGUILayout.Space ();
			GUILayout.BeginVertical ("box");
			EditorGUILayout.Space ();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox ("Basic Camera Settings", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space ();
			EditorGUILayout.PropertyField (list.FindProperty ("settings.layer"));
			EditorGUILayout.PropertyField (list.FindProperty ("settings.useAcelerometer"));
			EditorGUILayout.PropertyField (list.FindProperty ("settings.zoomEnabled"));
			EditorGUILayout.PropertyField (list.FindProperty ("settings.moveAwayCameraEnabled"));
			EditorGUILayout.PropertyField (list.FindProperty ("settings.enableMoveAwayInAir"));
			EditorGUILayout.PropertyField (list.FindProperty ("settings.enableShakeCamera"));
			EditorGUILayout.PropertyField (list.FindProperty ("settings.showCameraGizmo"));
			GUILayout.EndVertical ();
			EditorGUILayout.Space ();
		}

		if (GUI.changed) {
			list.ApplyModifiedProperties ();
		}
		EditorGUILayout.Space ();

		if (!Application.isPlaying) {
			playerCamera camera = (playerCamera)target;
			//set in the inspector the current camera type
			if (!checkCamera) {
				if (camera.firstPersonActive) {
					currentCamera = "FIRST PERSON";
				} else {
					currentCamera = "THIRD PERSON";
				}
				checkCamera = true;
			}
			GUILayout.Label ("Current Camera: " + currentCamera.ToString ());
			if (GUILayout.Button ("Set First Person")) {
				camera.setFirstPersonEditor ();
				currentCamera = "FIRST PERSON";
			}
			if (GUILayout.Button ("Set Third Person")) {
				camera.setThirdPersonEditor ();
				currentCamera = "THIRD PERSON";
			}
		}
	}

	void showCameraStateElementInfo (SerializedProperty list)
	{
		GUILayout.BeginVertical ("box");
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("Name"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("camPositionOffset"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("pivotPositionOffset"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("yLimits"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("showGizmo"));
		if (list.FindPropertyRelative ("showGizmo").boolValue) {
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("gizmoColor"));
		}
		GUILayout.EndVertical ();
	}

	void showUpperList (SerializedProperty list)
	{
		GUILayout.BeginVertical ();
		EditorGUILayout.PropertyField (list);
		if (list.isExpanded) {
			EditorGUILayout.Space ();
			GUILayout.Label ("Number of States: " + list.arraySize.ToString ());
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
}
#endif