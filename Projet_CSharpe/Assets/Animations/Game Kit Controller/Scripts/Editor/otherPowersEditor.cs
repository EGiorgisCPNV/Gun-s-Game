using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
//a simple editor to add a button in the ragdollBuilder script inspector
[CustomEditor(typeof(otherPowers))]
public class otherPowersEditor : Editor{
	SerializedObject list;
	otherPowers powersManager;
	void OnEnable(){
		list = new SerializedObject(target);
		powersManager = (otherPowers)target;
	}
	void OnSceneGUI(){   
//		if (!Application.isPlaying) {
//			powersManager = (otherPowers)target;
//			if (powersManager.shootsettings.showGizmo) {
//				GUIStyle style = new GUIStyle ();
//				style.normal.textColor = powersManager.settings.gizmoLabelColor;
//				style.alignment = TextAnchor.MiddleCenter;
////				if (healthManager.advancedSettings.weakSpots [i].spotTransform) {
////					Handles.Label (healthManager.advancedSettings.weakSpots [i].spotTransform.position, healthManager.advancedSettings.weakSpots [i].name
////					+ "\n" + "x" + healthManager.advancedSettings.weakSpots [i].damageMultiplier, style);	
//				//				}
//			}
//		}
	}
	bool settings;
	bool aimSettings;
	bool shootSettings;
	bool shakeSettings;
	Color defBackgroundColor;

	public override void OnInspectorGUI(){
		if (list == null) {
			return;
		}
		list.Update ();
		EditorGUILayout.Space();
		GUILayout.BeginVertical("Current Powers Action", "window");
		string powerName = "";
		if (powersManager.shootsettings.powersList.Count > 0) {
			powerName = powersManager.shootsettings.powersList [powersManager.choosedPower].Name;
		}
		GUILayout.Label("Current Power         "+list.FindProperty ("choosedPower").intValue.ToString()+"-"+powerName);
		EditorGUILayout.PropertyField(list.FindProperty("carryingObjects"));
		EditorGUILayout.PropertyField(list.FindProperty("carryingObject"));
		EditorGUILayout.PropertyField(list.FindProperty("wallWalk"));
		EditorGUILayout.PropertyField(list.FindProperty("running"));
		EditorGUILayout.PropertyField(list.FindProperty("activatedShield"), new GUIContent("Activated Shield"), false);
		EditorGUILayout.PropertyField(list.FindProperty("laserActive"));
		GUILayout.EndVertical();
		EditorGUILayout.Space();

		defBackgroundColor = GUI.backgroundColor;
		EditorGUILayout.BeginVertical();
		if (settings) {
			GUI.backgroundColor = Color.gray;
		} else {
			GUI.backgroundColor = defBackgroundColor;
		}
		if (GUILayout.Button ("Settings")) {
			settings = !settings;
		}
		if (aimSettings) {
			GUI.backgroundColor = Color.gray;
		} else {
			GUI.backgroundColor = defBackgroundColor;
		}
		if (GUILayout.Button ("Aim Settings")) {
			aimSettings = !aimSettings;
		}
		if (shootSettings) {
			GUI.backgroundColor = Color.gray;
		} else {
			GUI.backgroundColor = defBackgroundColor;
		}
		if (GUILayout.Button ("Shoot Settings")) {
			shootSettings = !shootSettings;
		}
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
		if (settings) {
			EditorGUILayout.Space ();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox ("Basic Settings", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space ();
			GUILayout.BeginVertical ("box");
			EditorGUILayout.PropertyField (list.FindProperty ("settings.runPowerEnabled"));
			EditorGUILayout.PropertyField (list.FindProperty ("settings.aimModeEnabled"));
			EditorGUILayout.PropertyField (list.FindProperty ("settings.shieldEnabled"));
			EditorGUILayout.PropertyField (list.FindProperty ("settings.grabObjectsEnabled"));
			EditorGUILayout.PropertyField (list.FindProperty ("settings.changeCameraViewEnabled"));
			EditorGUILayout.PropertyField (list.FindProperty ("settings.shootEnabled"));
			EditorGUILayout.PropertyField (list.FindProperty ("settings.changePowersEnabled"));
			EditorGUILayout.PropertyField (list.FindProperty ("settings.cursor"));
			EditorGUILayout.PropertyField (list.FindProperty ("settings.runMat"), new GUIContent ("Run Material"), false);
			EditorGUILayout.PropertyField (list.FindProperty ("settings.layer"));
			EditorGUILayout.PropertyField (list.FindProperty ("settings.trailsActive"));
			EditorGUILayout.PropertyField (list.FindProperty ("settings.slider"), new GUIContent ("Throw Objects Slider"), false);
			EditorGUILayout.PropertyField (list.FindProperty ("settings.healthBar"));
			EditorGUILayout.PropertyField (list.FindProperty ("settings.powerBar"));
			EditorGUILayout.PropertyField (list.FindProperty ("settings.runVelocity"));
			EditorGUILayout.PropertyField (list.FindProperty ("settings.runJumpPower"));
			EditorGUILayout.PropertyField (list.FindProperty ("settings.runAirSpeed"));
			EditorGUILayout.PropertyField (list.FindProperty ("settings.runAirControl"));
			EditorGUILayout.PropertyField (list.FindProperty ("settings.grabRadius"));
			EditorGUILayout.PropertyField (list.FindProperty ("settings.jointObjectsForce"));
			EditorGUILayout.Space ();
			GUILayout.BeginVertical ("Tags list able to grab", "window");
			showLowerList (list.FindProperty ("settings.ableToGrabTags"));
			GUILayout.EndVertical ();
			EditorGUILayout.Space ();
			EditorGUILayout.PropertyField (list.FindProperty ("settings.buttonKickTexture"));
			EditorGUILayout.PropertyField (list.FindProperty ("settings.buttonShootTexture"));
			EditorGUILayout.PropertyField (list.FindProperty ("settings.buttonShoot"));
			EditorGUILayout.PropertyField (list.FindProperty ("settings.highFrictionMaterial"));
			EditorGUILayout.PropertyField (list.FindProperty ("settings.gravityObjectsLayer"));
			EditorGUILayout.PropertyField (list.FindProperty ("settings.shield"));
			GUILayout.EndVertical ();
			EditorGUILayout.Space ();
		}
		if (aimSettings) {
			EditorGUILayout.Space ();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox ("Aim Settings for bones using powers and weapons", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space ();
			GUILayout.BeginVertical ("box");
			EditorGUILayout.PropertyField (list.FindProperty ("aimsettings.aiming"));
			EditorGUILayout.PropertyField (list.FindProperty ("aimsettings.aimSide"));
			EditorGUILayout.PropertyField (list.FindProperty ("aimsettings.chest"));
			EditorGUILayout.PropertyField (list.FindProperty ("aimsettings.spine"));
			EditorGUILayout.PropertyField (list.FindProperty ("aimsettings.leftHand"));
			EditorGUILayout.PropertyField (list.FindProperty ("aimsettings.rightHand"));
			EditorGUILayout.PropertyField (list.FindProperty ("aimsettings.spineVector"));
			GUILayout.EndVertical ();
			EditorGUILayout.Space ();
			EditorGUILayout.Space ();
		}
		if (shootSettings) {
			EditorGUILayout.Space ();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox ("Shoot Settings for every power", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space ();
			GUILayout.BeginVertical ("box");
			GUILayout.BeginVertical ("Powers List", "window");
			showUpperList (list.FindProperty ("shootsettings.powersList"));
			GUILayout.EndVertical ();
			EditorGUILayout.Space ();
			EditorGUILayout.PropertyField (list.FindProperty ("shootsettings.powerAmount"));
			EditorGUILayout.PropertyField (list.FindProperty ("shootsettings.powersSlotsAmount"));
			EditorGUILayout.PropertyField (list.FindProperty ("shootsettings.powerUsedByShield"));
			EditorGUILayout.PropertyField (list.FindProperty ("shootsettings.powerRegenerateSpeed"));
			EditorGUILayout.PropertyField (list.FindProperty ("shootsettings.homingProjectilesMaxAmount"));
			EditorGUILayout.PropertyField (list.FindProperty ("shootsettings.locatedEnemyIcon"));
			EditorGUILayout.PropertyField (list.FindProperty ("shootsettings.slowObjectsColor"));
			EditorGUILayout.PropertyField (list.FindProperty ("shootsettings.slowValue"));
			EditorGUILayout.PropertyField (list.FindProperty ("shootsettings.selectedPowerIcon"));
			EditorGUILayout.PropertyField (list.FindProperty ("shootsettings.selectedPowerHud"));
			EditorGUILayout.PropertyField (list.FindProperty ("shootsettings.shootZone"));
			EditorGUILayout.PropertyField (list.FindProperty ("shootsettings.firstPersonShootPosition"));
			EditorGUILayout.PropertyField (list.FindProperty ("shootsettings.bullet"), new GUIContent ("Power Projectile"), false);
			EditorGUILayout.PropertyField (list.FindProperty ("shootsettings.nanoBladeProjectile"));
			EditorGUILayout.PropertyField (list.FindProperty ("shootsettings.pushObjectsCenter"));
			EditorGUILayout.PropertyField (list.FindProperty ("shootsettings.touchZoneSize"));
			EditorGUILayout.PropertyField (list.FindProperty ("shootsettings.minSwipeDist"));
			EditorGUILayout.PropertyField (list.FindProperty ("shootsettings.touching"));
			EditorGUILayout.PropertyField (list.FindProperty ("shootsettings.hudZone"));
			EditorGUILayout.PropertyField (list.FindProperty ("shootsettings.showGizmo"));
			if (list.FindProperty ("shootsettings.showGizmo").boolValue) {
				EditorGUILayout.PropertyField (list.FindProperty ("shootsettings.gizmoColor"));
			}
			GUILayout.EndVertical ();
			EditorGUILayout.Space ();
			EditorGUILayout.Space ();
		}
		if (shakeSettings) {
			EditorGUILayout.Space ();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox ("Shake Settings when the player receives Damage", MessageType.None);
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
			GUILayout.EndVertical ();
			EditorGUILayout.Space ();
		}

//		EditorGUILayout.PropertyField (list.FindProperty ("extraRotation"));
//		EditorGUILayout.PropertyField (list.FindProperty ("targetRotation"));

		GUI.backgroundColor = defBackgroundColor;
		if (GUI.changed){
			list.ApplyModifiedProperties();
		}
	}
	void showListElementInfo(SerializedProperty list,bool showListNames){
		GUILayout.BeginVertical ("box");
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("Name"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("numberKey"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("texture"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("enabled"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("useRayCastShoot"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("amountPowerNeeded"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("projectileDamage"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("projectileSpeed"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("shootSoundEffect"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("impactSoundEffect"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("scorch"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("shootParticles"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("secundaryParticles"));
		GUILayout.EndVertical ();
	}
	void showUpperList(SerializedProperty list){
		GUILayout.BeginVertical ();
		EditorGUILayout.PropertyField (list);
		if (list.isExpanded) {
			EditorGUILayout.Space ();
			GUILayout.Label ("Number of powers: " + list.arraySize.ToString ());
			EditorGUILayout.Space ();
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Add Power")) {
				list.arraySize++;
			}
			if (GUILayout.Button ("Clear List")) {
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
						showListElementInfo (list.GetArrayElementAtIndex (i), true);
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
	void showLowerList(SerializedProperty list){
		EditorGUILayout.PropertyField (list);
		if (list.isExpanded) {
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("New")) {
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
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("shakePosition"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("shakePositionSpeed"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("shakePositionSmooth"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("shakeRotation"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("shakeRotationSpeed"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("shakeRotationSmooth"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("shakeDuration"));
		GUILayout.EndVertical ();
	}
}
#endif
