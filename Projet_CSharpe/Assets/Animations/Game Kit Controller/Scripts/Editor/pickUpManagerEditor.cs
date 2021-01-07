using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(pickUpManager))]
public class pickUpManagerEditor : Editor{
	SerializedObject list;

	void OnEnable(){
		list = new SerializedObject(target);
	}
	public override void OnInspectorGUI(){
		if (list == null) {
			return;
		}
		list.Update ();
		GUILayout.BeginVertical ("box");
		EditorGUILayout.Space ();
		GUILayout.BeginVertical ("Full PickUp List", "window", GUILayout.Height (30));
		showPickUpTypeList (list.FindProperty ("mainPickUpList"));
		GUILayout.EndVertical ();
		GUILayout.EndVertical ();
		EditorGUILayout.Space ();
		if (GUI.changed) {
			list.ApplyModifiedProperties ();
		}
	}
	void showPickUpTypeInfo(SerializedProperty list){
		GUILayout.BeginVertical ("box");
		EditorGUILayout.Space ();
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("pickUpType"));
		EditorGUILayout.Space ();
		GUILayout.BeginVertical ("box");
		EditorGUILayout.Space ();
		showPickUpList (list.FindPropertyRelative ("pickUpTypeList"),list.FindPropertyRelative ("pickUpType").stringValue);
		EditorGUILayout.Space ();
		GUILayout.EndVertical ();
		GUILayout.EndVertical ();
	}
	void showPickUpTypeList(SerializedProperty list){
		GUILayout.BeginVertical();
		EditorGUILayout.PropertyField (list, new GUIContent ("Full PickUp List"));
		if (list.isExpanded){
			EditorGUILayout.Space ();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox ("Add every type of pickup here", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space ();
			GUILayout.Label ("Number Of PickUps Type: \t" + list.arraySize.ToString ());
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
				bool mainExpanded = false;
				GUILayout.BeginHorizontal();
				GUILayout.BeginVertical("box");
				EditorGUILayout.Space();
				if (i < list.arraySize && i >= 0){
					EditorGUILayout.BeginVertical();
					EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i));
					if (list.GetArrayElementAtIndex (i).isExpanded) {
						mainExpanded = true;
						showPickUpTypeInfo (list.GetArrayElementAtIndex (i));
					}
					EditorGUILayout.Space();
					GUILayout.EndVertical();
				}
				GUILayout.EndVertical ();
				if (mainExpanded) {
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
				if (mainExpanded) {
					GUILayout.EndVertical ();
				} else {
					GUILayout.EndHorizontal ();
				}
				GUILayout.EndHorizontal();
			}
		}
		GUILayout.EndVertical();
	}
	void showPickUpElementInfo(SerializedProperty list){
		GUILayout.BeginVertical ("box");
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("name"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("pickUpObject"));
		GUILayout.EndVertical ();
	}
	void showPickUpList(SerializedProperty list, string pickUpType){
		GUILayout.BeginVertical();
		EditorGUILayout.PropertyField (list, new GUIContent (pickUpType+ " Type List"));
		if (list.isExpanded){
			EditorGUILayout.Space ();
			GUILayout.Label ("Number Of PickUps: \t" + list.arraySize.ToString ());
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
				GUILayout.BeginVertical("box");
				EditorGUILayout.Space();
				if (i < list.arraySize && i >= 0){
					EditorGUILayout.BeginVertical();
					EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i));
					if (list.GetArrayElementAtIndex (i).isExpanded) {
						expanded = true;
						showPickUpElementInfo (list.GetArrayElementAtIndex (i));
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