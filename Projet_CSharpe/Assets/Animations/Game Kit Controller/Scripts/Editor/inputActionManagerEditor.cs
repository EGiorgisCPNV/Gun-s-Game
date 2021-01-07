using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(inputActionManager))]
public class inputActionManagerEditor : Editor{
	SerializedObject list;
	inputActionManager manager;
	bool inputListOpened;
	Color defBackgroundColor;

	void OnEnable(){
		list = new SerializedObject(target);
		manager = (inputActionManager)target;
	}
	public override void OnInspectorGUI(){
		if (list == null) {
			return;
		}
		list.Update ();
		GUILayout.BeginVertical ("box");
		GUILayout.Label ("Input Activated: \t" + list.FindProperty ("inputActivated").boolValue.ToString ());
		EditorGUILayout.Space ();
		GUILayout.EndVertical ();
		EditorGUILayout.Space ();
		GUI.color = Color.cyan;
		EditorGUILayout.HelpBox ("Configure a custom action list", MessageType.None);
		GUI.color = Color.white;
		EditorGUILayout.Space ();
		EditorGUILayout.Space ();
		GUILayout.BeginVertical("Input Action List", "window",GUILayout.Height(30));
		showUpperList(list.FindProperty("inputActionList"));
		GUILayout.EndVertical();
		EditorGUILayout.Space ();

		EditorGUILayout.Space();
		defBackgroundColor = GUI.backgroundColor;
		EditorGUILayout.BeginHorizontal();
		string inputListOpenedText = "";
		if (inputListOpened) {
			GUI.backgroundColor = Color.gray;
			inputListOpenedText = "Hide Current Input List";
		} else {
			GUI.backgroundColor = defBackgroundColor;
			inputListOpenedText = "Show Current Input List";
		}
		if (GUILayout.Button (inputListOpenedText)) {
			inputListOpened = !inputListOpened;
		}
		GUI.backgroundColor = defBackgroundColor;
		EditorGUILayout.EndHorizontal();
		if (inputListOpened) {
			GUILayout.BeginVertical ("box");
			EditorGUILayout.Space();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox("This is the current input list defined in the custom Input Manager", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space();
			EditorGUILayout.Space ();
			GUILayout.BeginVertical("Current Input List", "window",GUILayout.Height(30));
			showLowerList(list.FindProperty("currentInputList"));
			EditorGUILayout.Space ();
			if (GUILayout.Button ("Get/Update Input List")) {
				manager.getCurrentInputList ();
			}
			GUILayout.EndVertical ();
			EditorGUILayout.Space ();
			GUILayout.EndVertical ();
		}
		if (GUI.changed) {
			list.ApplyModifiedProperties ();
		}
		EditorGUILayout.Space ();
	}
	void showListElementInfo(SerializedProperty list){
		GUILayout.BeginVertical("box");
		EditorGUILayout.PropertyField(list.FindPropertyRelative("name"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("inputActionName"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("keyInputType"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("showInControlsMenu"));
		GUILayout.EndVertical();
	}
	void showUpperList(SerializedProperty list){
		GUILayout.BeginVertical();
		EditorGUILayout.PropertyField(list);
		if (list.isExpanded){
			GUILayout.Label ("Number Of Actions: \t" + list.arraySize.ToString ());
			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add Action")){
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
						showListElementInfo (list.GetArrayElementAtIndex (i));
						expanded = true;
					}
					EditorGUILayout.Space();
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal ();
				if (expanded) {
					GUILayout.BeginVertical ();
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
					GUILayout.EndVertical ();
				} else {
					GUILayout.BeginHorizontal ();
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
					GUILayout.EndHorizontal ();
				}
				GUILayout.EndHorizontal();
			}
		}
		GUILayout.EndVertical();
	}
	void showLowerList(SerializedProperty list){
		GUILayout.BeginVertical();
		EditorGUILayout.PropertyField(list);
		if (list.isExpanded){
			EditorGUILayout.Space();
			GUIStyle style = new GUIStyle (EditorStyles.centeredGreyMiniLabel);
			style.fontSize = 10;
			style.fontStyle = FontStyle.Bold;
			style.normal.textColor = Color.black;
			EditorGUILayout.LabelField ("Action | Key", style);
			for (int i = 0; i < list.arraySize; i++){
				GUILayout.BeginHorizontal("box");
				if (i < list.arraySize && i >= 0){
					EditorGUILayout.LabelField(list.GetArrayElementAtIndex (i).FindPropertyRelative ("name").stringValue.ToString () + " ---> " + 
						list.GetArrayElementAtIndex(i).FindPropertyRelative("keyButton").stringValue.ToString (), style);
				}
				GUILayout.EndHorizontal();
			}
		}
		GUILayout.EndVertical();
	}
}
#endif