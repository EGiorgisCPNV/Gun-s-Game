using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(chestSystem))]
public class chestSystemEditor : Editor{
	SerializedObject list;
	bool isRandomContent;
	chestSystem manager;
	GUIStyle style = new GUIStyle();
	bool settings;
	Color buttonColor;

	void OnEnable(){
		list = new SerializedObject(target);
		manager = (chestSystem)target;
	}
	void OnSceneGUI(){   
		if (!Application.isPlaying && manager.showGizmo) {
			Vector3 currentPosition = manager.placeWhereInstantiatePickUps.position + manager.placeOffset;
			//the original x and z values, to make rows of the objects
			float originalX = currentPosition.x;
			float originalZ = currentPosition.z;
			int rows = 0;
			//set the localposition of every object, so every object is actually inside the chest
			for (int i = 0; i < manager.numberOfObjects; i++) {	
				style.normal.textColor = manager.gizmoLabelColor;
				style.alignment = TextAnchor.MiddleCenter;
				Handles.Label (currentPosition, (i + 1).ToString(), style);
				currentPosition.x += manager.space.x;
				if (i != 0 && (i + 1) % Mathf.Round (manager.amount.y) == 0) {
					currentPosition.z -= manager.space.z;
					currentPosition.x = originalX;
					rows++;
				}
				if (rows == Mathf.Round (manager.amount.x)) {
					currentPosition.y += manager.space.y;
					rows = 0;
					currentPosition.z = originalZ;
				}
			}
		}
	}
	public override void OnInspectorGUI(){
		if (list == null) {
			return;
		}
		list.Update ();
		GUILayout.BeginVertical ("box");
		EditorGUILayout.Space ();
		GUILayout.BeginVertical ("Chest PickUps List", "window", GUILayout.Height (30));
		showChestPickUpTypeList (list.FindProperty ("chestPickUpList"));
		GUILayout.EndVertical ();
		EditorGUILayout.Space ();
		EditorGUILayout.PropertyField (list.FindProperty ("pickUpIcon"));
		EditorGUILayout.PropertyField (list.FindProperty ("randomContent"));
		EditorGUILayout.PropertyField (list.FindProperty ("rachargeable"));
		if (list.FindProperty ("rachargeable").boolValue) {
			EditorGUILayout.PropertyField (list.FindProperty ("timeOpenedAfterEmtpy"));
			EditorGUILayout.PropertyField (list.FindProperty ("refilledTime"));
		}
		EditorGUILayout.PropertyField (list.FindProperty ("openAnimationName"));

		EditorGUILayout.Space ();
		buttonColor = GUI.backgroundColor;
		settings = list.FindProperty ("settings").boolValue;
		EditorGUILayout.BeginVertical();
		string inputListOpenedText = "";
		if (settings) {
			GUI.backgroundColor = Color.gray;
			inputListOpenedText = "Hide Settings";
		} else {
			GUI.backgroundColor = buttonColor;
			inputListOpenedText = "Show Settings";
		}
		if (GUILayout.Button (inputListOpenedText)) {
			settings = !settings;
		}
		GUI.backgroundColor = buttonColor;
		EditorGUILayout.EndHorizontal();
		list.FindProperty ("settings").boolValue = settings;
		if (settings) {
			GUILayout.BeginVertical ("box");
			EditorGUILayout.Space ();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox ("Configure the position and offset for every pickup to spawn, along with number of rows and columns", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space ();
			if (isRandomContent) {
				GUILayout.Label ("Number of pickups to spawn (random): \t" + "(Min) " + list.FindProperty ("minAmount").intValue
					+ " + " + " (Max) " + list.FindProperty ("maxAmount").intValue + " = " + list.FindProperty ("numberOfObjects").intValue);
			} else {
				GUILayout.Label ("Number of pickups to spawn: \t" + list.FindProperty ("numberOfObjects").intValue);
			}
			EditorGUILayout.Space ();
			EditorGUILayout.PropertyField (list.FindProperty ("placeWhereInstantiatePickUps"));
			EditorGUILayout.PropertyField (list.FindProperty ("placeOffset"));
			EditorGUILayout.PropertyField (list.FindProperty ("space"));
			EditorGUILayout.PropertyField (list.FindProperty ("amount"));
			EditorGUILayout.PropertyField (list.FindProperty ("pickUpScale"));
			EditorGUILayout.PropertyField (list.FindProperty ("showGizmo"));
			if (list.FindProperty ("showGizmo").boolValue) {
				EditorGUILayout.PropertyField (list.FindProperty ("gizmoColor"));
				EditorGUILayout.PropertyField (list.FindProperty ("gizmoLabelColor"));
				EditorGUILayout.PropertyField (list.FindProperty ("gizmoRadius"));
			}
			GUILayout.EndVertical ();
		}

		EditorGUILayout.Space ();
		if (GUILayout.Button ("Get PickUp Manager List")) {
			manager.getManagerPickUpList ();
		}
		EditorGUILayout.Space ();
		GUILayout.EndVertical ();
		isRandomContent = list.FindProperty ("randomContent").boolValue;
		if (GUI.changed) {
			list.ApplyModifiedProperties ();
		}
	}
	void showChestPickUpTypeInfo(SerializedProperty list){
		GUILayout.BeginVertical("box");
		if (manager.managerPickUpList.Count > 0) {
			list.FindPropertyRelative ("typeIndex").intValue = EditorGUILayout.Popup ("PickUp Type", list.FindPropertyRelative ("typeIndex").intValue, getTypeList ());
			list.FindPropertyRelative ("pickUpType").stringValue = manager.managerPickUpList [list.FindPropertyRelative ("typeIndex").intValue].pickUpType;

			EditorGUILayout.Space ();
			GUILayout.BeginVertical ("box");
			EditorGUILayout.Space ();
			showChestPickUpList (list.FindPropertyRelative ("chestPickUpTypeList"),list.FindPropertyRelative ("pickUpType").stringValue, list.FindPropertyRelative ("typeIndex").intValue);
			EditorGUILayout.Space ();
			GUILayout.EndVertical ();
		}
		GUILayout.EndVertical();
	}
	void showChestPickUpTypeList(SerializedProperty list){
		GUILayout.BeginVertical();
		EditorGUILayout.PropertyField (list, new GUIContent ("Chest PickUp List"));
		if (list.isExpanded){
			EditorGUILayout.Space ();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox ("Add the pickups for the chest here", MessageType.None);
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
						showChestPickUpTypeInfo (list.GetArrayElementAtIndex (i));
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
	void showChestPickUpElementInfo(SerializedProperty list, int typeIndex){
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
	void showChestPickUpList(SerializedProperty list, string pickUpType, int typeIndex){
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
						showChestPickUpElementInfo (list.GetArrayElementAtIndex (i), typeIndex);
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