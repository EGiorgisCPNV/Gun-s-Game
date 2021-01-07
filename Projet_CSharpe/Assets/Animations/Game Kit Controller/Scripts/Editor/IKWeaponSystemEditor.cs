using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(IKWeaponSystem))]
[CanEditMultipleObjects]
public class IKWeaponSystemEditor : Editor{
	IKWeaponSystem IKWeaponManager;
	SerializedObject objectToUse;
	GUIStyle style = new GUIStyle();
	bool settings;
	bool elementSettings;
	bool showThirdPerson;
	bool showFirstPerson;
	bool showWeaponIdleSettings;
	Color buttonColor;

	void OnEnable(){
		objectToUse = new SerializedObject(targets);
		IKWeaponManager = (IKWeaponSystem)target;
	}
	void OnSceneGUI(){   
		if (!Application.isPlaying) {
			IKWeaponManager = (IKWeaponSystem)target;
			if (IKWeaponManager.showThirdPersonGizmo) {
				style.normal.textColor = IKWeaponManager.gizmoLabelColor;
				style.alignment = TextAnchor.MiddleCenter;
				Handles.Label (IKWeaponManager.thirdPersonWeaponInfo.aimPosition.position, "Aim \n Position", style);
				Handles.Label (IKWeaponManager.thirdPersonWeaponInfo.walkPosition.position, "Walk \n Position", style);
				Handles.Label (IKWeaponManager.thirdPersonWeaponInfo.keepPosition.position, "Keep \n Position", style);
				Handles.Label (IKWeaponManager.thirdPersonWeaponInfo.aimRecoilPosition.position, "Aim \n Recoil \n Position", style);
				for (int i = 0; i < IKWeaponManager.thirdPersonWeaponInfo.handsInfo.Count; i++) {
					Handles.Label (IKWeaponManager.thirdPersonWeaponInfo.handsInfo[i].position.position, IKWeaponManager.thirdPersonWeaponInfo.handsInfo[i].Name, style);

				}
				if (IKWeaponManager.checkSurfaceCollision) {
					style.normal.textColor = IKWeaponManager.gizmoLabelColor;
					style.alignment = TextAnchor.MiddleCenter;
					Handles.Label (IKWeaponManager.weapon.transform.position + IKWeaponManager.weapon.transform.forward * IKWeaponManager.weaponLenght, "Weapon \n Size", style);
				}
			}
			if (IKWeaponManager.showFirstPersonGizmo) {
				style.normal.textColor = IKWeaponManager.gizmoLabelColor;
				style.alignment = TextAnchor.MiddleCenter;
				Handles.Label (IKWeaponManager.firstPersonWeaponInfo.aimPosition.position, "Aim \n Position", style);
				Handles.Label (IKWeaponManager.firstPersonWeaponInfo.walkPosition.position, "Walk \n Position", style);
				Handles.Label (IKWeaponManager.firstPersonWeaponInfo.keepPosition.position, "Keep \n Position", style);
				Handles.Label (IKWeaponManager.firstPersonWeaponInfo.aimRecoilPosition.position, "Aim \n Recoil \n Position", style);
				Handles.Label (IKWeaponManager.firstPersonWeaponInfo.walkRecoilPosition.position, "Walk \n Recoil \n Position", style);
			}
		}
	}
	public override void OnInspectorGUI(){
		if (objectToUse == null) {
			return;
		}
		objectToUse.Update ();
		GUILayout.BeginVertical ("box");

		GUILayout.BeginVertical("Weapon State", "window", GUILayout.Height(30));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("carrying"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("aiming"));
		GUILayout.EndVertical();

		EditorGUILayout.Space ();
		buttonColor = GUI.backgroundColor;
		settings = objectToUse.FindProperty ("showSettings").boolValue;
		elementSettings = objectToUse.FindProperty ("showElementSettings").boolValue;

		EditorGUILayout.BeginVertical();
		string inputListOpenedText = "";
		if (settings) {
			GUI.backgroundColor = Color.gray;
			inputListOpenedText = "Hide Settings";
		} else {
			GUI.backgroundColor = buttonColor;
			inputListOpenedText = "Show Settings";
		}
		if (GUILayout.Button (inputListOpenedText)) {
			settings = !settings;
		}
		if (elementSettings) {
			GUI.backgroundColor = Color.gray;
			inputListOpenedText = "Hide Element Settings";
		} else {
			GUI.backgroundColor = buttonColor;
			inputListOpenedText = "Show Element Settings";
		}
		if (GUILayout.Button (inputListOpenedText)) {
			elementSettings = !elementSettings;
		}
		GUI.backgroundColor = buttonColor;
		EditorGUILayout.EndVertical();

		objectToUse.FindProperty ("showSettings").boolValue = settings;
		objectToUse.FindProperty ("showElementSettings").boolValue = elementSettings;
		if (settings) {
			GUILayout.BeginVertical ("box");
			EditorGUILayout.Space ();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox ("Configure the max amount of weapons adn the layer used in weapons", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space ();
			EditorGUILayout.Space ();
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("weapon"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("currentWeapon"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("recoilSpeed"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("extraRotation"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("aimFovValue"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("aimFovSpeed"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("weaponEnabled"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("weaponPrefabModel"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("checkSurfaceCollision"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("weaponLenght"));
			EditorGUILayout.Space ();
			GUILayout.EndVertical ();
		}
		if (elementSettings) {
			GUILayout.BeginVertical ("box");
			EditorGUILayout.Space();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox("Configure the settings for third and first person", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space();
			EditorGUILayout.Space ();

			buttonColor = GUI.backgroundColor;
			EditorGUILayout.BeginVertical();
			if (showThirdPerson) {
				GUI.backgroundColor = Color.gray;
				inputListOpenedText = "Hide Third Person Settings";
			} else {
				GUI.backgroundColor = buttonColor;
				inputListOpenedText = "Show Third Person Settings";
			}
			if (GUILayout.Button (inputListOpenedText)) {
				showThirdPerson = !showThirdPerson;
			}
			if (showFirstPerson) {
				GUI.backgroundColor = Color.gray;
				inputListOpenedText = "Hide First Person Settings";
			} else {
				GUI.backgroundColor = buttonColor;
				inputListOpenedText = "Show First Person Settings";
			}
			if (GUILayout.Button (inputListOpenedText)) {
				showFirstPerson = !showFirstPerson;
			}
			GUI.backgroundColor = buttonColor;
			EditorGUILayout.EndVertical();

			if (showThirdPerson) {
				EditorGUILayout.Space ();
				GUILayout.BeginVertical ("Third Person Weapon Settings", "window", GUILayout.Height (30));
				showWeaponSettings (objectToUse.FindProperty ("thirdPersonWeaponInfo"),true);
				EditorGUILayout.Space ();
				GUILayout.EndVertical ();
				EditorGUILayout.Space ();
			}
			if(showFirstPerson){
				EditorGUILayout.Space ();
				GUILayout.BeginVertical ("First Person Weapon Settings", "window", GUILayout.Height (30));
				showWeaponSettings (objectToUse.FindProperty ("firstPersonWeaponInfo"),false);
				EditorGUILayout.Space ();
				GUILayout.EndVertical ();
				EditorGUILayout.Space ();

				bool showSwaySettings = objectToUse.FindProperty ("firstPersonSwayInfo.showSwaySettings").boolValue;

				EditorGUILayout.BeginVertical();
				if (showSwaySettings) {
					GUI.backgroundColor = Color.gray;
					inputListOpenedText = "Hide First Person Sway Settings";
				} else {
					GUI.backgroundColor = buttonColor;
					inputListOpenedText = "Show First Person Sway Settings";
				}
				if (GUILayout.Button (inputListOpenedText)) {
					showSwaySettings = !showSwaySettings;
				}
				GUI.backgroundColor = buttonColor;
				EditorGUILayout.EndVertical();

				objectToUse.FindProperty ("firstPersonSwayInfo.showSwaySettings").boolValue = showSwaySettings;
				if (showSwaySettings) {
					EditorGUILayout.Space ();
					GUILayout.BeginVertical("First Person Sway Settings", "window", GUILayout.Height(30));
					EditorGUILayout.PropertyField (objectToUse.FindProperty ("firstPersonSwayInfo.useSway"));
					bool useSway = objectToUse.FindProperty ("firstPersonSwayInfo.useSway").boolValue;
					if (useSway) {
						EditorGUILayout.PropertyField (objectToUse.FindProperty ("firstPersonSwayInfo.usePositionSway"));
						if (objectToUse.FindProperty ("firstPersonSwayInfo.usePositionSway").boolValue) {
							EditorGUILayout.PropertyField (objectToUse.FindProperty ("firstPersonSwayInfo.swayPositionVertical"));
							EditorGUILayout.PropertyField (objectToUse.FindProperty ("firstPersonSwayInfo.swayPositionHorizontal"));
							EditorGUILayout.PropertyField (objectToUse.FindProperty ("firstPersonSwayInfo.swayPositionMaxAmount"));
							EditorGUILayout.PropertyField (objectToUse.FindProperty ("firstPersonSwayInfo.swayPositionSmooth"));
						}
						EditorGUILayout.PropertyField (objectToUse.FindProperty ("firstPersonSwayInfo.useRotationSway"));
						if (objectToUse.FindProperty ("firstPersonSwayInfo.useRotationSway").boolValue) {
							EditorGUILayout.PropertyField (objectToUse.FindProperty ("firstPersonSwayInfo.swayRotationVertical"));
							EditorGUILayout.PropertyField (objectToUse.FindProperty ("firstPersonSwayInfo.swayRotationHorizontal"));
							EditorGUILayout.PropertyField (objectToUse.FindProperty ("firstPersonSwayInfo.swayRotationSmooth"));
						}
						EditorGUILayout.PropertyField (objectToUse.FindProperty ("firstPersonSwayInfo.useBobPosition"));
						if (objectToUse.FindProperty ("firstPersonSwayInfo.useBobPosition").boolValue) {
							EditorGUILayout.PropertyField (objectToUse.FindProperty ("firstPersonSwayInfo.bobPositionSpeed"));
							EditorGUILayout.PropertyField (objectToUse.FindProperty ("firstPersonSwayInfo.bobPositionAmount"));
						}
						EditorGUILayout.PropertyField (objectToUse.FindProperty ("firstPersonSwayInfo.useBobRotation"));
						if (objectToUse.FindProperty ("firstPersonSwayInfo.useBobRotation").boolValue) {
							EditorGUILayout.PropertyField (objectToUse.FindProperty ("firstPersonSwayInfo.bobRotationVertical"));
							EditorGUILayout.PropertyField (objectToUse.FindProperty ("firstPersonSwayInfo.bobRotationHorizontal"));
						}
						EditorGUILayout.PropertyField (objectToUse.FindProperty ("firstPersonSwayInfo.movingExtraPosition"));
						EditorGUILayout.PropertyField (objectToUse.FindProperty ("firstPersonSwayInfo.swayPositionRunningMultiplier"));
						EditorGUILayout.PropertyField (objectToUse.FindProperty ("firstPersonSwayInfo.swayRotationRunningMultiplier"));
						EditorGUILayout.PropertyField (objectToUse.FindProperty ("firstPersonSwayInfo.bobPositionRunningMultiplier"));
						EditorGUILayout.PropertyField (objectToUse.FindProperty ("firstPersonSwayInfo.bobRotationRunningMultiplier"));
						EditorGUILayout.PropertyField (objectToUse.FindProperty ("firstPersonSwayInfo.swayPositionPercentageAiming"));
						EditorGUILayout.PropertyField (objectToUse.FindProperty ("firstPersonSwayInfo.swayRotationPercentageAiming"));
						EditorGUILayout.PropertyField (objectToUse.FindProperty ("firstPersonSwayInfo.bobPositionPercentageAiming"));
						EditorGUILayout.PropertyField (objectToUse.FindProperty ("firstPersonSwayInfo.bobRotationPercentageAiming"));
					}
					GUILayout.EndVertical ();
					EditorGUILayout.Space ();

				}
				showWeaponIdleSettings = objectToUse.FindProperty ("showIdleSettings").boolValue;
				EditorGUILayout.BeginVertical();
				if (showWeaponIdleSettings) {
					GUI.backgroundColor = Color.gray;
					inputListOpenedText = "Hide Idle Settings";
				} else {
					GUI.backgroundColor = buttonColor;
					inputListOpenedText = "Show Idle Settings";
				}
				if (GUILayout.Button (inputListOpenedText)) {
					showWeaponIdleSettings = !showWeaponIdleSettings;
				}
				GUI.backgroundColor = buttonColor;
				EditorGUILayout.EndVertical();
				objectToUse.FindProperty ("showIdleSettings").boolValue = showWeaponIdleSettings;
				if (showWeaponIdleSettings) {
					EditorGUILayout.Space ();
					GUILayout.BeginVertical ("First Person Weapon Idle Settings", "window", GUILayout.Height (30));
					EditorGUILayout.PropertyField (objectToUse.FindProperty ("useWeaponIdle"));
					if (objectToUse.FindProperty ("useWeaponIdle").boolValue) {
						EditorGUILayout.PropertyField (objectToUse.FindProperty ("timeToActiveWeaponIdle"));
						EditorGUILayout.PropertyField (objectToUse.FindProperty ("idlePositionAmount"));
						EditorGUILayout.PropertyField (objectToUse.FindProperty ("idleRotationAmount"));
						EditorGUILayout.PropertyField (objectToUse.FindProperty ("idleSpeed"));
						GUILayout.BeginVertical("Weapon State", "window", GUILayout.Height(30));
						EditorGUILayout.PropertyField (objectToUse.FindProperty ("playerMoving"));
						EditorGUILayout.PropertyField (objectToUse.FindProperty ("idleActive"));
						GUILayout.EndVertical();
					}
					GUILayout.EndVertical ();
					EditorGUILayout.Space ();
				}
			}

			bool showShotShakeSettings = objectToUse.FindProperty ("showShotShakeettings").boolValue;
			EditorGUILayout.BeginVertical();
			if (showShotShakeSettings) {
				GUI.backgroundColor = Color.gray;
				inputListOpenedText = "Hide Shot Shake Settings";
			} else {
				GUI.backgroundColor = buttonColor;
				inputListOpenedText = "Show Shot Shake Settings";
			}
			if (GUILayout.Button (inputListOpenedText)) {
				showShotShakeSettings = !showShotShakeSettings;
			}
			GUI.backgroundColor = buttonColor;
			EditorGUILayout.EndVertical();
			objectToUse.FindProperty ("showShotShakeettings").boolValue = showShotShakeSettings;
			if (showShotShakeSettings) {
				EditorGUILayout.Space ();
				GUILayout.BeginVertical ("First Person Shot Shake Settings", "window", GUILayout.Height (30));
				EditorGUILayout.PropertyField (objectToUse.FindProperty ("useShotShakeInThirdPerson"));
				if (objectToUse.FindProperty ("useShotShakeInThirdPerson").boolValue) {
					showShakeInfo (objectToUse.FindProperty ("thirdPersonshotShakeInfo"), false);
				}
				EditorGUILayout.PropertyField (objectToUse.FindProperty ("useShotShakeInFirstPerson"));
				if (objectToUse.FindProperty ("useShotShakeInFirstPerson").boolValue) {
					showShakeInfo (objectToUse.FindProperty ("firstPersonshotShakeInfo"), true);
				}
				GUILayout.EndVertical ();
				EditorGUILayout.Space ();
			}

			EditorGUILayout.Space ();
			GUILayout.BeginVertical("Gizmo Options", "window", GUILayout.Height(30));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("showThirdPersonGizmo"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("showFirstPersonGizmo"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("gizmoLabelColor"));
			EditorGUILayout.Space ();
			GUILayout.EndVertical ();

			GUILayout.EndVertical ();
		}
		EditorGUILayout.Space ();
		GUILayout.EndVertical ();
		if (GUI.changed) {
			objectToUse.ApplyModifiedProperties ();
		}
	}
	void showWeaponSettings(SerializedProperty list, bool isThirdPerson){
		GUILayout.BeginVertical("box");
		EditorGUILayout.PropertyField(list.FindPropertyRelative("weapon"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("aimPosition"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("walkPosition"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("keepPosition"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("aimRecoilPosition"));
		if (!isThirdPerson) {
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("walkRecoilPosition"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("firstPersonArms"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("canAimInFirstPerson"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("useLowerRotationSpeedAimed"));
			if (objectToUse.FindProperty ("useLowerRotationSpeedAimed").boolValue) {
				EditorGUILayout.PropertyField (objectToUse.FindProperty ("rotationSpeedAimedInFirstPerson"));
			}
		}
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("movementSpeed"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("aimMovementSpeed"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("useExtraRandomRecoil"));
		if (list.FindPropertyRelative ("useExtraRandomRecoil").boolValue) {
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("extraRandomRecoilPosition"));
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("extraRandomRecoilRotation"));
		}
		if (isThirdPerson) {
			showSimpleList (objectToUse.FindProperty ("thirdPersonWeaponInfo.keepPath"));
			showHandList (objectToUse.FindProperty ("thirdPersonWeaponInfo.handsInfo"));
			showHandList (objectToUse.FindProperty ("headLookWhenAiming"));
			if (objectToUse.FindProperty ("headLookWhenAiming").boolValue) {
				showHandList (objectToUse.FindProperty ("headLookSpeed"));
				showHandList (objectToUse.FindProperty ("headLookTarget"));
			}
//			if (GUILayout.Button ("Set Hands Transform")) {
//				Undo.RecordObject(target,"Set hands transform");
//				IKWeaponManager.setHandTransform ();
//				EditorUtility.SetDirty (target);
//			}
		}
		GUILayout.EndVertical ();
	}
	void showSimpleList(SerializedProperty list){
		EditorGUILayout.PropertyField(list);
		if (list.isExpanded){
			GUILayout.BeginVertical ("box");
			EditorGUILayout.Space();
			GUILayout.Label ("Number Of Points: \t" + list.arraySize.ToString ());
			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add Point")){
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
				EditorGUILayout.Space();
			}
			GUILayout.EndVertical();
		}       
	}
	void showHandElementInfo(SerializedProperty list){
		Color listButtonBackgroundColor;
		bool showElbowInfo = list.FindPropertyRelative ("showElbowInfo").boolValue;

		GUILayout.BeginVertical("box");
		EditorGUILayout.PropertyField(list.FindPropertyRelative("Name"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("handTransform"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("limb"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("position"),new GUIContent("IK Position"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("waypointFollower"));
		showSimpleList (list.FindPropertyRelative ("wayPoints"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("usedToDrawWeapon"));

		listButtonBackgroundColor = GUI.backgroundColor;
		EditorGUILayout.BeginHorizontal ();
		if (showElbowInfo) {
			GUI.backgroundColor = Color.gray;
		} else {
			GUI.backgroundColor = buttonColor;
		}
		if (GUILayout.Button ("Elbow Settings")) {
			showElbowInfo = !showElbowInfo;
		}
		GUI.backgroundColor = listButtonBackgroundColor;
		EditorGUILayout.EndHorizontal ();
		list.FindPropertyRelative ("showElbowInfo").boolValue = showElbowInfo;
		if (showElbowInfo) {
			GUILayout.BeginVertical ("box");
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("elbowInfo.Name"));
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("elbowInfo.elbow"));
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("elbowInfo.position"));
			GUILayout.EndVertical ();
		}
		GUILayout.EndVertical();
	}
	void showHandList(SerializedProperty list){
		EditorGUILayout.PropertyField(list);
		if (list.isExpanded){
			EditorGUILayout.Space();
			if (list.arraySize < 2) {
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Add Hand")) {
					list.arraySize++;
				}
				if (GUILayout.Button ("Clear")) {
					list.arraySize = 0;
				}
				GUILayout.EndHorizontal ();
				EditorGUILayout.Space ();
			}
			for (int i = 0; i < list.arraySize; i++) {
				bool expanded = false;
				GUILayout.BeginHorizontal ();
				GUILayout.BeginHorizontal ("box");
				EditorGUILayout.Space ();
				if (i < list.arraySize && i >= 0) {
					EditorGUILayout.BeginVertical ();
					EditorGUILayout.PropertyField (list.GetArrayElementAtIndex (i));
					if (list.GetArrayElementAtIndex (i).isExpanded) {
						expanded = true;
						showHandElementInfo (list.GetArrayElementAtIndex (i));
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
	}
	void showShakeInfo(SerializedProperty list, bool isFirstPerson){
		GUILayout.BeginVertical ("box");
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("shotForce"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("shakeSmooth"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("shakeDuration"));
		if (isFirstPerson) {
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("shakePosition"));
		}
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("shakeRotation"));
		GUILayout.EndVertical ();
	}
}
#endif