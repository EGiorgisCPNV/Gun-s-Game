using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(eventTriggerSystem))]
[CanEditMultipleObjects]
public class eventTriggerSystemEditor : Editor{
	SerializedObject list;
	bool useSameFunctionInList;
	bool useSameDelay;
	bool triggeredByButton;

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
		EditorGUILayout.PropertyField (list.FindProperty ("useSameFunctionInList"));
		useSameFunctionInList = list.FindProperty ("useSameFunctionInList").boolValue;
		if (useSameFunctionInList) {
			EditorGUILayout.Space ();
			GUILayout.BeginVertical ("Same Function List", "window", GUILayout.Height (30));
			showSimpleList (list.FindProperty ("sameFunctionList"));
			GUILayout.EndVertical ();
			EditorGUILayout.Space ();
		}
		triggeredByButton = list.FindProperty ("triggeredByButton").boolValue;
		EditorGUILayout.PropertyField (list.FindProperty ("triggeredByButton"));
		EditorGUILayout.PropertyField (list.FindProperty ("useObjectToTrigger"));
		if (list.FindProperty ("useObjectToTrigger").boolValue) {
			EditorGUILayout.PropertyField (list.FindProperty ("objectNeededToTrigger"));
		}
		EditorGUILayout.PropertyField (list.FindProperty ("useTagToTrigger"));
		if (list.FindProperty ("useTagToTrigger").boolValue) {
			EditorGUILayout.PropertyField (list.FindProperty ("tagNeededToTrigger"));
		}
		EditorGUILayout.PropertyField (list.FindProperty ("callFunctionEveryTimeTriggered"));
		if (!list.FindProperty ("callFunctionEveryTimeTriggered").boolValue) {
			EditorGUILayout.PropertyField (list.FindProperty ("eventTriggered"));
		}
		useSameDelay = list.FindProperty ("useSameDelay").boolValue;
		EditorGUILayout.PropertyField (list.FindProperty ("useSameDelay"));
		if (useSameDelay) {
			EditorGUILayout.PropertyField (list.FindProperty ("generalDelay"));
		}
		if (!triggeredByButton) {
			EditorGUILayout.PropertyField (list.FindProperty ("triggerEventType"));
		}
		EditorGUILayout.PropertyField (list.FindProperty ("coroutineActive"));
		EditorGUILayout.PropertyField (list.FindProperty ("setParentToNull"));

		EditorGUILayout.Space ();

		GUILayout.BeginVertical("Event Trigger List", "window", GUILayout.Height(30));
		showList (list.FindProperty ("eventList"));
		GUILayout.EndVertical();

		EditorGUILayout.Space ();

		GUILayout.EndVertical();
		if (GUI.changed) {
			list.ApplyModifiedProperties ();
		}
	}

	void showEventInfo(SerializedProperty list){
		GUILayout.BeginVertical("box");
		EditorGUILayout.PropertyField(list.FindPropertyRelative("name"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("objectToCall"));	
		if (!useSameFunctionInList) {
			showSimpleList (list.FindPropertyRelative ("functionNameList"));
		}
		if (!useSameDelay) {
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("secondsDelay"));
		}
		EditorGUILayout.PropertyField(list.FindPropertyRelative("sendGameObject"));	
		if (list.FindPropertyRelative ("sendGameObject").boolValue) {
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("objectToSend"));
		}
		GUILayout.EndVertical();
	}

	void showList(SerializedProperty list){
		EditorGUILayout.PropertyField(list);
		if (list.isExpanded) {
			EditorGUILayout.Space ();
			EditorGUILayout.Space ();
			GUILayout.Label ("Number Of Events: \t" + list.arraySize.ToString ());
			EditorGUILayout.Space ();
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Add Event")) {
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
						showEventInfo (list.GetArrayElementAtIndex (i));
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
	void showSimpleList(SerializedProperty list){
		EditorGUILayout.PropertyField(list);
		if (list.isExpanded){
			GUILayout.BeginVertical ("box");
			EditorGUILayout.Space();
			GUILayout.Label ("Number Of Functions: \t" + list.arraySize.ToString ());
			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add Function")){
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
}
#endif
