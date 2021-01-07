using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(dropPickUpSystem))]
public class dropPickUpSystemEditor : Editor{
	SerializedObject list;
	bool isRandomContent;
	dropPickUpSystem manager;
	bool settings;
	Color buttonColor;

	void OnEnable(){
		list = new SerializedObject(target);
		manager = (dropPickUpSystem)target;
	}
	public override void OnInspectorGUI(){
		if (list == null) {
			return;
		}
		list.Update ();
		GUILayout.BeginVertical ("box");
		EditorGUILayout.Space ();
		GUILayout.BeginVertical ("Drop PickUps List", "window", GUILayout.Height (30));
		showDropPickUpList (list.FindProperty ("dropPickUpList"));
		GUILayout.EndVertical ();
		EditorGUILayout.Space ();
		EditorGUILayout.PropertyField (list.FindProperty ("dropDelay"));
		EditorGUILayout.PropertyField (list.FindProperty ("destroyAfterDropping"));
		EditorGUILayout.PropertyField (list.FindProperty ("pickUpScale"));
		EditorGUILayout.PropertyField (list.FindProperty ("randomContent"));
		EditorGUILayout.PropertyField (list.FindProperty ("showGizmo"));
		EditorGUILayout.PropertyField (list.FindProperty ("maxRadiusToInstantiate"));
		EditorGUILayout.PropertyField (list.FindProperty ("pickUpOffset"));
		EditorGUILayout.Space ();
		if (GUILayout.Button ("Get PickUp Manager List")) {
			manager.getManagerPickUpList ();
		}
		EditorGUILayout.Space ();
		isRandomContent = list.FindProperty ("randomContent").boolValue;
		GUILayout.EndVertical ();
		if (GUI.changed) {
			list.ApplyModifiedProperties ();
		}
	}
	void showDropPickUpTypeInfo(SerializedProperty list){
		GUILayout.BeginVertical("box");
		if (manager.managerPickUpList.Count > 0) {
			list.FindPropertyRelative ("typeIndex").intValue = EditorGUILayout.Popup ("PickUp Type", list.FindPropertyRelative ("typeIndex").intValue, getTypeList ());
			list.FindPropertyRelative ("pickUpType").stringValue = manager.managerPickUpList [list.FindPropertyRelative ("typeIndex").intValue].pickUpType;

			EditorGUILayout.Space ();
			GUILayout.BeginVertical ("box");
			EditorGUILayout.Space ();
			showDropPickUpList (list.FindPropertyRelative ("dropPickUpTypeList"),list.FindPropertyRelative ("pickUpType").stringValue, list.FindPropertyRelative ("typeIndex").intValue);
			EditorGUILayout.Space ();
			GUILayout.EndVertical ();
		}
		GUILayout.EndVertical();
	}
	void showDropPickUpList(SerializedProperty list){
		GUILayout.BeginVertical();
		EditorGUILayout.PropertyField (list, new GUIContent ("Drop PickUp List"));
		if (list.isExpanded){
			EditorGUILayout.Space ();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox ("Add the pickups to drop here", MessageType.None);
			GUI.color = Color.white;
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
	void showDropPickUpElementInfo(SerializedProperty list, int typeIndex){
		if (manager.managerPickUpList [typeIndex].pickUpTypeList.Count > 0) {
			list.FindPropertyRelative ("nameIndex").intValue = EditorGUILayout.Popup ("Name", list.FindPropertyRelative ("nameIndex").intValue, getNameList (typeIndex));
			list.FindPropertyRelative ("name").stringValue = manager.managerPickUpList [typeIndex].pickUpTypeList [list.FindPropertyRelative ("nameIndex").intValue].name;
		}
		if (isRandomContent) {
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("amountLimits"));
			EditorGUILayout.PropertyField (list.FindPropertyRelative ("quantityLimits"));
		} else {
			EditorGUILayout.PropertyField(list.FindPropertyRelative("amount"));
			EditorGUILayout.PropertyField(list.FindPropertyRelative("quantity"));
		}
	}
	void showDropPickUpList(SerializedProperty list, string pickUpType, int typeIndex){
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
				GUILayout.BeginHorizontal("box");
				EditorGUILayout.Space();
				if (i < list.arraySize && i >= 0){
					EditorGUILayout.BeginVertical();
					EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i));
					if (list.GetArrayElementAtIndex (i).isExpanded) {
						expanded = true;
						showDropPickUpElementInfo (list.GetArrayElementAtIndex (i), typeIndex);
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
	string[] getTypeList(){
		string[] names = new string[manager.managerPickUpList.Count];
		for (int i = 0; i < manager.managerPickUpList.Count; i++) {
			names [i] = manager.managerPickUpList [i].pickUpType;
		}
		return names;
	}
	string[] getNameList(int index){
		string[] names = new string[manager.managerPickUpList[index].pickUpTypeList.Count];
		for (int i = 0; i < manager.managerPickUpList[index].pickUpTypeList.Count; i++) {
			names [i] = manager.managerPickUpList[index].pickUpTypeList [i].name;
		}
		return names;
	}
}
#endif