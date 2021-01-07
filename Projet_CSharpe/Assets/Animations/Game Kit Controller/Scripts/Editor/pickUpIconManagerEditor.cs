using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(pickUpIconManager))]
public class pickUpIconManagerEditor : Editor{
	SerializedObject list;
	pickUpIconManager manager;

	void OnEnable(){
		list = new SerializedObject(target);
		manager = (pickUpIconManager)target;
	}
	public override void OnInspectorGUI(){
		if (list == null) {
			return;
		}
		list.Update ();
		GUILayout.BeginVertical ("box");
		EditorGUILayout.Space ();
		GUILayout.BeginVertical ("PickUp Icon List", "window", GUILayout.Height (30));
		showDropPickUpList (list.FindProperty ("pickUpList"));
		GUILayout.EndVertical ();
		EditorGUILayout.Space ();
		EditorGUILayout.PropertyField (list.FindProperty ("pickUpIconObject"));
		EditorGUILayout.PropertyField (list.FindProperty ("layer"));
		EditorGUILayout.PropertyField (list.FindProperty ("checkIcontype"));
		EditorGUILayout.PropertyField (list.FindProperty ("maxDistanceIconEnabled"));
		EditorGUILayout.Space ();
		if (GUILayout.Button ("Get PickUp Manager List")) {
			manager.getManagerPickUpList ();
		}
		GUILayout.EndVertical ();
		EditorGUILayout.Space ();
		if (GUI.changed) {
			list.ApplyModifiedProperties ();
		}
	}
	void showDropPickUpTypeInfo(SerializedProperty list){
		GUILayout.BeginVertical("box");
		if (manager.managerPickUpList.Length > 0) {
			list.FindPropertyRelative ("typeIndex").intValue = EditorGUILayout.Popup ("PickUp Type", list.FindPropertyRelative ("typeIndex").intValue, manager.managerPickUpList);
			list.FindPropertyRelative ("pickUpType").stringValue = manager.managerPickUpList [list.FindPropertyRelative ("typeIndex").intValue];
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("isRawImage"));
			if (list.FindPropertyRelative ("isRawImage").boolValue) {
				EditorGUILayout.PropertyField (list.FindPropertyRelative ("iconTexture"));
			}
			else {
				EditorGUILayout.PropertyField (list.FindPropertyRelative ("iconTextureSprite"));
			}
		}
		GUILayout.EndVertical();
	}
	void showDropPickUpList(SerializedProperty list){
		GUILayout.BeginVertical();
		EditorGUILayout.PropertyField (list, new GUIContent ("PickUp Icon List"));
		if (list.isExpanded){
			EditorGUILayout.Space ();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox ("Configure the icons used in every pickup type", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space ();
			GUILayout.Label ("Number Of Icons: \t" + list.arraySize.ToString ());
			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add PickUp")){
				list.arraySize++;
			}
			if (GUILayout.Button("Clear List")){
				list.arraySize = 0;
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();
			for (int i = 0; i < list.arraySize; i++){
				bool expanded = false;
				GUILayout.BeginHorizontal();
				GUILayout.BeginHorizontal("box");
				EditorGUILayout.Space();
				if (i < list.arraySize && i >= 0){
					EditorGUILayout.BeginVertical();
					EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i));
					if (list.GetArrayElementAtIndex (i).isExpanded) {
						expanded = true;
						showDropPickUpTypeInfo (list.GetArrayElementAtIndex (i));
					}
					EditorGUILayout.Space();
					GUILayout.EndVertical();
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
				GUILayout.EndHorizontal();
			}
		}
		GUILayout.EndVertical();
	}
}
#endif