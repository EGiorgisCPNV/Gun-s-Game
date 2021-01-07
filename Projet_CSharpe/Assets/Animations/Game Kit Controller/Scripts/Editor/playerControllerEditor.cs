using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor (typeof(playerController))]
public class playerControllerEditor : Editor
{
	SerializedObject list;

	void OnEnable ()
	{
		list = new SerializedObject (target);
	}

	public override void OnInspectorGUI ()
	{
		if (list == null) {
			return;
		}
		list.Update ();
		GUILayout.BeginVertical ("box");
		GUILayout.BeginVertical ("Movement Settings", "window");
		EditorGUILayout.PropertyField (list.FindProperty ("walkSpeed"));
		EditorGUILayout.PropertyField (list.FindProperty ("jumpPower"));
		EditorGUILayout.PropertyField (list.FindProperty ("airSpeed"));
		EditorGUILayout.PropertyField (list.FindProperty ("airControl"));
		EditorGUILayout.PropertyField (list.FindProperty ("stationaryTurnSpeed"));
		EditorGUILayout.PropertyField (list.FindProperty ("movingTurnSpeed"));
		EditorGUILayout.PropertyField (list.FindProperty ("autoTurnSpeed"));
		EditorGUILayout.PropertyField (list.FindProperty ("aimTurnSpeed"));
		EditorGUILayout.PropertyField (list.FindProperty ("usingAnimatorInFirstMode"));
		GUILayout.EndVertical ();
		EditorGUILayout.Space ();

		GUILayout.BeginVertical ("Gravity Settings", "window");
		EditorGUILayout.PropertyField (list.FindProperty ("gravityMultiplier"));
		EditorGUILayout.PropertyField (list.FindProperty ("gravityForce"));
		GUILayout.EndVertical ();
		EditorGUILayout.Space ();

		GUILayout.BeginVertical ("Physics Settings", "window");
		EditorGUILayout.PropertyField (list.FindProperty ("zeroFrictionMaterial"));
		EditorGUILayout.PropertyField (list.FindProperty ("highFrictionMaterial"));
		EditorGUILayout.PropertyField (list.FindProperty ("layer"));
		EditorGUILayout.PropertyField (list.FindProperty ("rayDistance"));
		GUILayout.EndVertical ();
		EditorGUILayout.Space ();

		GUILayout.BeginVertical ("Jump Settings", "window");
		EditorGUILayout.PropertyField (list.FindProperty ("enabledRegularJump"));
		EditorGUILayout.PropertyField (list.FindProperty ("enabledDoubleJump"));
		if (list.FindProperty ("enabledDoubleJump").boolValue) {
			EditorGUILayout.PropertyField (list.FindProperty ("maxNumberJumpsInAir"));
		}
		EditorGUILayout.PropertyField (list.FindProperty ("holdJumpSlowDownFall"));
		if (list.FindProperty ("holdJumpSlowDownFall").boolValue) {
			EditorGUILayout.PropertyField (list.FindProperty ("slowDownGravityMultiplier"));
		}
		GUILayout.EndVertical ();
		EditorGUILayout.Space ();

		GUILayout.BeginVertical ("Fall Damage Settings", "window");
		EditorGUILayout.PropertyField (list.FindProperty ("damageFallEnabled"));
		if (list.FindProperty ("damageFallEnabled").boolValue) {
			EditorGUILayout.PropertyField (list.FindProperty ("maxTimeInAirDamage"));
		}
		GUILayout.EndVertical ();
		EditorGUILayout.Space ();

		GUILayout.BeginVertical ("Land Mark Settings", "window");
		EditorGUILayout.PropertyField (list.FindProperty ("useLandMark"));
		if (list.FindProperty ("useLandMark").boolValue) {
			EditorGUILayout.PropertyField (list.FindProperty ("maxLandDistance"));
			EditorGUILayout.PropertyField (list.FindProperty ("minDistanceShowLandMark"));
			EditorGUILayout.PropertyField (list.FindProperty ("landMark"));
		}
		GUILayout.EndVertical ();
		EditorGUILayout.Space ();

		GUILayout.BeginVertical ("Player Modes Settings", "window");
		EditorGUILayout.PropertyField (list.FindProperty ("canUseSphereMode"));
		GUILayout.EndVertical ();
		EditorGUILayout.Space ();
	
		GUILayout.BeginVertical ("Player State", "window");
		GUILayout.Label ("On Ground\t\t" + list.FindProperty ("onGround").boolValue.ToString ());
		GUILayout.Label ("Jumping\t\t" + list.FindProperty ("jump").boolValue.ToString ());
		GUILayout.Label ("Aiming In 3rd Person\t" + list.FindProperty ("aiming").boolValue.ToString ());
		GUILayout.Label ("Aiming In 1st Person\t" + list.FindProperty ("aimingInFirstPerson").boolValue.ToString ());
		GUILayout.Label ("Crouch\t\t" + list.FindProperty ("crouch").boolValue.ToString ());
		GUILayout.Label ("Jet Pack Equiped\t" + list.FindProperty ("jetPackEquiped").boolValue.ToString ());
		GUILayout.Label ("Using Jet Pack\t" + list.FindProperty ("usingJetpack").boolValue.ToString ());
		GUILayout.Label ("Sphere Mode Active\t" + list.FindProperty ("sphereModeActive").boolValue.ToString ());
		GUILayout.Label ("Fly Mode Active\t" + list.FindProperty ("flyModeActive").boolValue.ToString ());
		GUILayout.Label ("Slowing Fall\t" + list.FindProperty ("slowingFall").boolValue.ToString ());
		GUILayout.Label ("Can Move\t\t" + list.FindProperty ("canMove").boolValue.ToString ());
		GUILayout.EndVertical ();
		EditorGUILayout.Space ();

		GUILayout.EndVertical ();

		if (GUI.changed) {
			list.ApplyModifiedProperties ();
		}
	}
}
#endif