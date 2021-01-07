using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(headBob))]
public class headBobEditor : Editor{
	SerializedObject objectToUse;
	bool settings;
	bool elementSettings;
	bool showThirdPerson;
	bool showFirstPerson;
	Color buttonColor;

	void OnEnable(){
		objectToUse = new SerializedObject(target);
	}
	public override void OnInspectorGUI(){
		if (objectToUse == null) {
			return;
		}
		objectToUse.Update ();

		GUILayout.BeginVertical ("box");

		GUILayout.BeginVertical ("Settings", "window", GUILayout.Height (30));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("headBobEnabled"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("currentState"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("externalForceStateName"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("resetSpeed"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("useDynamicIdle"));
		if (objectToUse.FindProperty ("useDynamicIdle").boolValue) {
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("dynamicIdleName"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("timeToActiveDynamicIdle"));
		}
		GUILayout.EndVertical ();

		EditorGUILayout.Space ();

		GUILayout.BeginVertical ("Jump Settings", "window", GUILayout.Height (30));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("jumpStartMaxIncrease"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("jumpStartSpeed"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("jumpEndMaxDecrease"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("jumpEndSpeed"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("jumpResetSpeed"));

		GUILayout.EndVertical ();

		EditorGUILayout.Space ();
		GUILayout.BeginVertical ("Bob States List", "window", GUILayout.Height (30));
		showList (objectToUse.FindProperty ("bobStatesList"));
		EditorGUILayout.Space ();
		GUILayout.EndVertical ();

		EditorGUILayout.Space ();

		GUILayout.BeginVertical ("Player Bob State ", "window");
		string isFirstPerson = "-";
		if (Application.isPlaying) {
			if (objectToUse.FindProperty ("firstPersonMode").boolValue) {
				isFirstPerson = "YES";
			} else {
				isFirstPerson = "NO";
			}
		} 
		string isExternalShaking = "-";
		if (Application.isPlaying) {
			if (objectToUse.FindProperty ("externalShake").boolValue) {
				isExternalShaking = "YES";
			} else {
				isExternalShaking = "NO";
			}
		} 
		string canBeUsed = "-";
		if (Application.isPlaying) {
			if (objectToUse.FindProperty ("headBobCanBeUsed").boolValue) {
				canBeUsed = "YES";

			} else {
				canBeUsed = "NO";
			}
		} 
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("First Person View ");
		GUILayout.Label (isFirstPerson);
		GUILayout.EndHorizontal ();
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("External Shake Active");
		GUILayout.Label (isExternalShaking);
		GUILayout.EndHorizontal ();
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("Head Bob can be used");
		GUILayout.Label (canBeUsed);
		GUILayout.EndHorizontal ();
		GUILayout.EndVertical ();

		EditorGUILayout.Space ();
		GUILayout.EndVertical ();
		if (GUI.changed) {
			objectToUse.ApplyModifiedProperties ();
		}
	}
	void showElementInfo(SerializedProperty list){
		GUILayout.BeginVertical ("box");
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("Name"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("bobTransformStyle"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("enableBobIn"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("posAmount"), new GUIContent ("Position Amount"), false);
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("posSpeed"), new GUIContent ("Position Speed"), false);
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("posSmooth"), new GUIContent ("Position Smooth"), false);
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("eulAmount"), new GUIContent ("Rotation Amount"), false);
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("eulSpeed"), new GUIContent ("Rotation Speed"), false);
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("eulSmooth"), new GUIContent ("Rotation Smooth"), false);
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("isCurrentState"));
		GUILayout.EndVertical ();
	}
	void showList(SerializedProperty list){
		EditorGUILayout.PropertyField (list);
		if (list.isExpanded) {
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
						expanded = true;
						showElementInfo (list.GetArrayElementAtIndex (i));
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
}
#endif