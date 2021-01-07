using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor (typeof(playerWeaponSystem))]
[CanEditMultipleObjects]
public class playerWeaponSystemEditor : Editor
{
	SerializedObject objectToUse;
	bool settings;
	Color buttonColor;

	void OnEnable ()
	{
		objectToUse = new SerializedObject (targets);
	}

	public override void OnInspectorGUI ()
	{
		if (objectToUse == null) {
			return;
		}
		objectToUse.Update ();
		GUILayout.BeginVertical ("box");

		GUILayout.BeginVertical ("Weapon State", "window", GUILayout.Height (30));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("reloading"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("carryingWeaponInThirdPerson"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("carryingWeaponInFirstPerson"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("aimingInThirdPerson"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("aimingInFirstPerson"));
		GUILayout.EndVertical ();

		EditorGUILayout.Space ();

		EditorGUILayout.PropertyField (objectToUse.FindProperty ("character"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("outOfAmmo"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("weaponProjectile"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("layer"));

		EditorGUILayout.Space ();

		buttonColor = GUI.backgroundColor;
		EditorGUILayout.BeginVertical ();
		string inputListOpenedText = "";
		if (settings) {
			GUI.backgroundColor = Color.gray;
			inputListOpenedText = "Hide Weapon Settings";
		} else {
			GUI.backgroundColor = buttonColor;
			inputListOpenedText = "Show Weapon Settings";
		}
		if (GUILayout.Button (inputListOpenedText)) {
			settings = !settings;
		}
		GUI.backgroundColor = buttonColor;
		EditorGUILayout.EndVertical ();

		if (settings) {
			EditorGUILayout.Space ();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox ("Configure the shot settings of this weapon", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space ();
			showWeaponSettings (objectToUse.FindProperty ("weaponSettings"));
		}
				
		EditorGUILayout.Space ();
		GUILayout.EndVertical ();
		if (GUI.changed) {
			objectToUse.ApplyModifiedProperties ();
		}
	}

	void showWeaponSettings (SerializedProperty list)
	{
		GUILayout.BeginVertical ("box");
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("Name"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("numberKey"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("useRayCastShoot"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("fireWeaponForward"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("infiniteAmmo"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("automatic"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("clipSize"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("remainAmmo"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("fireRate"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("reloadTime"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("projectileDamage"));
		if (!list.FindPropertyRelative ("useRayCastShoot").boolValue) {
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("projectileSpeed"));
		}

		EditorGUILayout.PropertyField (list.FindPropertyRelative ("projectileForce"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("projectilesPerShoot"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("useProjectileSpread"));
		if (list.FindPropertyRelative ("useProjectileSpread").boolValue) {
			GUILayout.BeginVertical("Spread Settings", "window", GUILayout.Height(30));
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("spreadAmount"));
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("sameSpreadInThirdPerson"));
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("thirdPersonSpreadAmount"));
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("useSpreadAming"));
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("useLowerSpreadAiming"));
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("lowerSpreadAmount"));
			GUILayout.EndVertical ();
		}
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("isExplosive"));
		if (list.FindPropertyRelative ("isExplosive").boolValue) {
			GUILayout.BeginVertical("Explosion Settings", "window", GUILayout.Height(30));
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("explosionForce"));
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("explosionRadius"));
			GUILayout.EndVertical ();
		}
		showSimpleList (list.FindPropertyRelative ("projectilePosition"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("shell"));
		if (list.FindPropertyRelative ("shell").objectReferenceValue) {
			GUILayout.BeginVertical("Shell Settings", "window", GUILayout.Height(30));
			showSimpleList (list.FindPropertyRelative ("shellPosition"));
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("shellEjectionForce"));
			showSimpleList (list.FindPropertyRelative ("shellDropSoundList"));
			GUILayout.EndVertical ();
		}
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("weapon"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("weaponMesh"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("weaponParent"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("animation"));
		if (list.FindPropertyRelative ("animation").stringValue != "") {
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("animationSpeed"));
		}
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("scorch"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("particles"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("muzzleParticles"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("soundEffect"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("projectileSoundEffect"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("reloadSoundEffect"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("cockSound"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("useHUD"));
		if (list.FindPropertyRelative ("useHUD").boolValue) {
			GUILayout.BeginVertical("HUD Settings", "window", GUILayout.Height(30));
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("clipSizeText"));
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("remainAmmoText"));
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("HUD"));
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("ammoInfoHUD"));
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("disableHUDInFirstPersonAim"));
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("changeHUDPosition"));
			if (list.FindPropertyRelative ("changeHUDPosition").boolValue) {
				EditorGUILayout.PropertyField (list.FindPropertyRelative ("HUDTransformInThirdPerson"));
				EditorGUILayout.PropertyField (list.FindPropertyRelative ("HUDTransformInFirstPerson"));
			}
			GUILayout.EndVertical ();
		}
		GUILayout.EndVertical ();
	}

	void showSimpleList (SerializedProperty list)
	{
		EditorGUILayout.PropertyField (list);
		if (list.isExpanded) {
			GUILayout.BeginVertical ("box");
			EditorGUILayout.Space ();
			GUILayout.Label ("Amount: \t" + list.arraySize.ToString ());
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
				GUILayout.BeginHorizontal ();
				if (i < list.arraySize && i >= 0) {
					EditorGUILayout.PropertyField (list.GetArrayElementAtIndex (i), new GUIContent ("", null, ""));
				}
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
				GUILayout.EndHorizontal ();
				EditorGUILayout.Space ();
			}
			GUILayout.EndVertical ();
		}       
	}
}
#endif