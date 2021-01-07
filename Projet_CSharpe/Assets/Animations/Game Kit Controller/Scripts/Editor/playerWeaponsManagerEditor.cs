using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(playerWeaponsManager))]
public class playerWeaponsManagerEditor : Editor{
	playerWeaponsManager weaponsManager;
	SerializedObject objectToUse;
	bool settings;
	bool elementSettings;
	bool weaponsList;
	Color buttonColor;

	void OnEnable(){
		objectToUse = new SerializedObject(target);
		weaponsManager = (playerWeaponsManager)target;
	}
	public override void OnInspectorGUI(){
		if (objectToUse == null) {
			return;
		}
		objectToUse.Update ();
		GUILayout.BeginVertical ("box");

		GUILayout.BeginVertical("Player Weapons State", "window");
		string carryingWeaponMode = "Not Carrying";
		if (Application.isPlaying) {
			if (objectToUse.FindProperty ("carryingWeaponInThirdPerson").boolValue) {
				carryingWeaponMode = "Third Person";
			} 
			if (objectToUse.FindProperty ("carryingWeaponInFirstPerson").boolValue) {
				carryingWeaponMode = "First Person";
			}
		} 
		GUILayout.Label ("Carrying Weapon\t " + carryingWeaponMode);

		string aimingWeaponMode = "Not Aiming";
		if (Application.isPlaying) {
			if (objectToUse.FindProperty ("aimingInThirdPerson").boolValue) {
				aimingWeaponMode = "Third Person";
			} 
			if (objectToUse.FindProperty ("aimingInFirstPerson").boolValue) {
				aimingWeaponMode = "First Person";
			}
		} 
		GUILayout.Label ("Aiming Weapon\t " + aimingWeaponMode);

		string currentState = "Not Using";
		if (Application.isPlaying) {
			if (objectToUse.FindProperty ("anyWeaponAvaliable").boolValue) {
				if (objectToUse.FindProperty ("carryingWeaponInThirdPerson").boolValue || objectToUse.FindProperty ("carryingWeaponInFirstPerson").boolValue) {
					currentState = "Carrying Weapon";
				} else {
					currentState = "Not Carrying";
				}
			} else {
				currentState = "Not Avaliable";
			}
		} 
		GUILayout.Label ("Current State\t" + currentState);

		string currentWeaponName = "None";
		if (Application.isPlaying) {
			if (objectToUse.FindProperty ("anyWeaponAvaliable").boolValue) {
				currentWeaponName = objectToUse.FindProperty ("currentWeaponName").stringValue.ToString ();
			}
		} 
		GUILayout.Label ("Current Weapon\t" + currentWeaponName);

		string shootingWeapon = "NO";
		if (objectToUse.FindProperty ("shooting").boolValue) {
			shootingWeapon = "YES";
		}
		GUILayout.Label ("Shooting Weapon\t" + shootingWeapon);
		GUILayout.Label ("Choosed Weapon Index\t" + objectToUse.FindProperty ("choosedWeapon").intValue.ToString ());
		GUILayout.EndVertical();

		buttonColor = GUI.backgroundColor;
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
		if (weaponsList) {
			GUI.backgroundColor = Color.gray;
			inputListOpenedText = "Hide Weapon List";
		} else {
			GUI.backgroundColor = buttonColor;
			inputListOpenedText = "Show Weapon List";
		}
		if (GUILayout.Button (inputListOpenedText)) {
			weaponsList = !weaponsList;
		}
		GUI.backgroundColor = buttonColor;
		EditorGUILayout.EndHorizontal();
		if (settings) {
			GUILayout.BeginVertical ("box");
			EditorGUILayout.Space();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox("Configure the max amount of weapons adn the layer used in weapons", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space();
			EditorGUILayout.Space ();
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("weaponsSlotsAmount"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("weaponsLayer"));
			GUILayout.Label ("Touch Options");
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("touchZoneSize"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("minSwipeDist"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("touching"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("showGizmo"));
			if (objectToUse.FindProperty ("showGizmo").boolValue) {
				EditorGUILayout.PropertyField (objectToUse.FindProperty ("gizmoColor"));
			}
			EditorGUILayout.Space ();
			GUILayout.EndVertical ();
		}
		if (elementSettings) {
			GUILayout.BeginVertical ("box");
			EditorGUILayout.Space();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox("Configure every gameObject used for the weapons", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space();
			EditorGUILayout.Space ();
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("weaponsHUD"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("currentWeaponNameText"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("currentWeaponAmmoText"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("ammoSlider"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("weaponsParent"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("weaponsTransformInFirstPerson"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("weaponsTransformInThirdPerson"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("thirdPersonParent"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("firstPersonParent"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("cameraController"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("weaponsCamera"));
			EditorGUILayout.Space ();
			GUILayout.EndVertical ();
		}
		if (weaponsList) {
			GUILayout.BeginVertical ("box");
			EditorGUILayout.Space();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox("Configure every weapon added to the player", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space();
			EditorGUILayout.Space ();
			EditorGUILayout.Space ();
			GUILayout.BeginVertical ("Weapon List", "window",GUILayout.Height(30));
			showUpperList (objectToUse.FindProperty ("weaponsList"));
			EditorGUILayout.Space ();
			if (GUILayout.Button ("Get Weapon List")) {
				weaponsManager.getWeaponList ();
			}
			EditorGUILayout.Space ();
			GUILayout.EndVertical ();
			EditorGUILayout.Space ();
			GUILayout.EndVertical ();
		}
		GUILayout.EndVertical ();
		if (GUI.changed) {
			objectToUse.ApplyModifiedProperties ();
		}
		EditorGUILayout.Space ();
	}
	void showListElementInfo(SerializedProperty list){
		IKWeaponSystem IKWeapon = list.objectReferenceValue as IKWeaponSystem;
		playerWeaponSystem weaponSystem = IKWeapon.weapon.GetComponent<playerWeaponSystem> ();
		GUILayout.BeginVertical();
		GUILayout.Label (weaponSystem.weaponSettings.Name);
		EditorGUILayout.ObjectField(IKWeapon.gameObject, typeof(GameObject));
		IKWeapon.weaponEnabled = EditorGUILayout.Toggle ("Enabled", IKWeapon.weaponEnabled);
		GUILayout.EndVertical ();
	}
	void showUpperList(SerializedProperty list){
		EditorGUILayout.PropertyField(list);
		if (list.isExpanded){
			GUILayout.BeginVertical ("box");
			EditorGUILayout.Space();
			GUILayout.Label ("Number Of Weapons: \t" + list.arraySize.ToString ());
			EditorGUILayout.Space();
			for (int i = 0; i < list.arraySize; i++){
				GUILayout.BeginHorizontal();
				GUILayout.BeginHorizontal("box");
				EditorGUILayout.Space();
				if (i < list.arraySize && i >= 0) {
					EditorGUILayout.BeginVertical ();
					showListElementInfo (list.GetArrayElementAtIndex (i));
					GUILayout.EndVertical ();
				}
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("x")) {
					list.DeleteArrayElementAtIndex (i);
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
				GUILayout.EndHorizontal ();
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();
		}       
	}
}
#endif