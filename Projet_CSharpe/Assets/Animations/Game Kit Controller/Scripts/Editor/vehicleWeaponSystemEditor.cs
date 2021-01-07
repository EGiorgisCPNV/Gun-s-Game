using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
//a simple editor to add some buttons in the vehicle weapon script inspector
[CustomEditor(typeof(vehicleWeaponSystem))]
[CanEditMultipleObjects]
public class vehicleWeaponSystemEditor : Editor{
	SerializedObject systemToUse;
	Color buttonColor;

	void OnEnable(){
		systemToUse = new SerializedObject(target);
	}
	public override void OnInspectorGUI(){
		if (systemToUse == null)
			return;
		systemToUse.Update ();
		GUILayout.BeginVertical ("box");
		EditorGUILayout.PropertyField (systemToUse.FindProperty ("weaponsActivate"));
		EditorGUILayout.PropertyField (systemToUse.FindProperty ("weaponsEffectsSource"));
		EditorGUILayout.PropertyField (systemToUse.FindProperty ("outOfAmmo"));
		EditorGUILayout.PropertyField (systemToUse.FindProperty ("locatedEnemyIcon"));
		EditorGUILayout.PropertyField (systemToUse.FindProperty ("layer"));
		EditorGUILayout.PropertyField (systemToUse.FindProperty ("minimumX"));
		EditorGUILayout.PropertyField (systemToUse.FindProperty ("maximumX"));
		EditorGUILayout.PropertyField (systemToUse.FindProperty ("minimumY"));
		EditorGUILayout.PropertyField (systemToUse.FindProperty ("maximumY"));
		EditorGUILayout.PropertyField (systemToUse.FindProperty ("baseX"));
		EditorGUILayout.PropertyField (systemToUse.FindProperty ("baseY"));
		EditorGUILayout.PropertyField (systemToUse.FindProperty ("weaponLookDirection"));
		EditorGUILayout.PropertyField (systemToUse.FindProperty ("weaponsSlotsAmount"));
		EditorGUILayout.PropertyField (systemToUse.FindProperty ("touchZoneSize"));
		EditorGUILayout.PropertyField (systemToUse.FindProperty ("minSwipeDist"));
		EditorGUILayout.PropertyField (systemToUse.FindProperty ("showGizmo"));
		if (systemToUse.FindProperty ("showGizmo").boolValue) {
			EditorGUILayout.PropertyField (systemToUse.FindProperty ("gizmoColor"));
		}
		EditorGUILayout.PropertyField (systemToUse.FindProperty ("reloading"));
		EditorGUILayout.PropertyField (systemToUse.FindProperty ("aimingCorrectly"));
		GUILayout.EndVertical ();
		EditorGUILayout.Space ();
		EditorGUILayout.Space ();
		GUILayout.BeginVertical ("Weapons List", "window");
		showUpperList (systemToUse.FindProperty ("weapons"));
		GUILayout.EndVertical ();
		EditorGUILayout.Space ();
		if (GUI.changed) {
			systemToUse.ApplyModifiedProperties ();
		}
	}
	void showListElementInfo(SerializedProperty list,bool showListNames){
		GUILayout.BeginVertical ("box");
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("Name"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("numberKey"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("useRayCastShoot"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("fireWeaponForward"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("enabled"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("infiniteAmmo"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("clipSize"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("remainAmmo"));
		if (!list.FindPropertyRelative ("useRayCastShoot").boolValue) {
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("projectileSpeed"));
		}
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("isExplosive"));
		if (list.FindPropertyRelative ("isExplosive").boolValue) {
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("explosionForce"));
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("explosionRadius"));
		} else {
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("projectileForce"));
		}
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("isHomming"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("fireRate"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("reloadTime"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("projectileDamage"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("projectilesPerShoot"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("projectileToShoot"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("useProjectileSpread"));
		if (list.FindPropertyRelative ("useProjectileSpread").boolValue) {
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("spreadAmount"));
		}
		if (showListNames) {
			showLowerList (list.FindPropertyRelative ("projectilePosition"));
		}
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("ejectShellOnShot"));
		if (list.FindPropertyRelative ("ejectShellOnShot").boolValue) {
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("shell"));
			if (showListNames) {
				showLowerList (list.FindPropertyRelative ("shellPosition"));
			}
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("shellEjectionForce"));
			showLowerList (list.FindPropertyRelative ("shellDropSoundList"));
		}
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("secundaryObject"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("scorch"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("weapon"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("animation"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("particles"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("muzzleParticles"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("soundEffect"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("projectileSoundEffect"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("reloadSoundEffect"));

		EditorGUILayout.Space ();
		bool shakeSettings = list.FindPropertyRelative ("showShakeSettings").boolValue;
		buttonColor = GUI.backgroundColor;
		EditorGUILayout.BeginVertical();
		if (shakeSettings) {
			GUI.backgroundColor = Color.gray;
		} else {
			GUI.backgroundColor = buttonColor;
		}
		if (GUILayout.Button ("Shake Settings")) {
			shakeSettings = !shakeSettings;
		}
		GUI.backgroundColor = buttonColor;
		EditorGUILayout.EndVertical();
		list.FindPropertyRelative ("showShakeSettings").boolValue = shakeSettings;

		if (shakeSettings) {
			EditorGUILayout.Space ();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox ("Shake Settings when this weapon fires", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space ();
			GUILayout.BeginVertical ("box");
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("shootShakeInfo.useDamageShake"));
			if (list.FindPropertyRelative ("shootShakeInfo.useDamageShake").boolValue) {
				EditorGUILayout.Space ();
				EditorGUILayout.PropertyField (list.FindPropertyRelative ("shootShakeInfo.useDamageShakeInThirdPerson"));
				if (list.FindPropertyRelative ("shootShakeInfo.useDamageShakeInThirdPerson").boolValue) {
					showShakeInfo (list.FindPropertyRelative ("shootShakeInfo.thirdPersonDamageShake"));
					EditorGUILayout.Space ();
				}
				EditorGUILayout.PropertyField (list.FindPropertyRelative ("shootShakeInfo.useDamageShakeInFirstPerson"));
				if (list.FindPropertyRelative ("shootShakeInfo.useDamageShakeInFirstPerson").boolValue) {
					showShakeInfo (list.FindPropertyRelative ("shootShakeInfo.firstPersonDamageShake"));
					EditorGUILayout.Space ();
				}
			}
			GUILayout.EndVertical ();
		}

		GUILayout.EndVertical ();
	}
	void showUpperList(SerializedProperty list){
		GUILayout.BeginVertical ();
		EditorGUILayout.PropertyField (list);
		if (list.isExpanded) {
			EditorGUILayout.Space ();
			GUILayout.Label ("Number of weapons: " + list.arraySize.ToString ());
			EditorGUILayout.Space ();
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Add")) {
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
						expanded = true;
						showListElementInfo (list.GetArrayElementAtIndex (i), true);
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
	void showLowerList(SerializedProperty list){
		EditorGUILayout.PropertyField (list);
		if (list.isExpanded) {
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Add")) {
				list.arraySize++;
			}
			if (GUILayout.Button ("Clear")) {
				list.arraySize = 0;
			}
			GUILayout.EndHorizontal ();
			EditorGUILayout.Space ();
			for (int i = 0; i < list.arraySize; i++) {
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("x")) {
					list.DeleteArrayElementAtIndex (i);
					return;
				}
				if (i < list.arraySize && i >= 0) {
					EditorGUILayout.PropertyField (list.GetArrayElementAtIndex (i), new GUIContent ("", null, ""));
				}
				GUILayout.EndHorizontal ();
			}
		}       
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