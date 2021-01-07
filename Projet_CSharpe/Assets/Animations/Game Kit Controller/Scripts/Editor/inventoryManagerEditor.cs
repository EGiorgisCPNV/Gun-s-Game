using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(inventoryManager),true)]
public class inventoryManagerEditor : Editor{
	SerializedObject list;
	inventoryManager inventory;
	bool showElementSettings;
	Color buttonColor;
	inventoryCaptureManager inventoryWindow;

	void OnEnable(){
		list = new SerializedObject(target);
		inventory = (inventoryManager)target;
	}
	public override void OnInspectorGUI(){
		if (list == null) {
			return;
		}
		list.Update ();
		GUILayout.BeginVertical ("box");
		GUILayout.BeginVertical ("box");

		//showListElementInfo (list.FindProperty ("currentObject"),true,0);

		GUILayout.BeginVertical("Inventory Menu State", "window");
		string menuOpened = "NO";
		if (Application.isPlaying) {
			if (list.FindProperty ("inventoryOpened").boolValue) {
				menuOpened = "YES";
			} 
		} 
		GUILayout.Label ("Inventory Menu Opened \t " + menuOpened);
		GUILayout.EndVertical();

		EditorGUILayout.Space ();

		GUILayout.BeginVertical("Inventory Settings", "window");
		EditorGUILayout.PropertyField (list.FindProperty ("inventoryEnabled"));
		EditorGUILayout.PropertyField (list.FindProperty ("combineElementsAtDrop"));
		EditorGUILayout.PropertyField (list.FindProperty ("inventorySpace"));
		EditorGUILayout.PropertyField (list.FindProperty ("maxObjectsAmountPerSpace"));
		EditorGUILayout.PropertyField (list.FindProperty ("buttonUsable"), new GUIContent("Button Usable Color"),false);
		EditorGUILayout.PropertyField (list.FindProperty ("buttonNotUsable"), new GUIContent("Button Not Usable Color"),false);
		GUILayout.EndVertical();

		EditorGUILayout.Space ();

		GUILayout.BeginVertical("Inventory Messages Settings", "window");
		EditorGUILayout.PropertyField (list.FindProperty ("usedObjectMessage"));
		EditorGUILayout.PropertyField (list.FindProperty ("usedObjectMessageTime"));
		EditorGUILayout.PropertyField (list.FindProperty ("unableToUseObjectMessage"));
		EditorGUILayout.PropertyField (list.FindProperty ("fullInventoryMessage"));
		EditorGUILayout.PropertyField (list.FindProperty ("fullinventoryMessageTime"));
		GUILayout.EndVertical();

		EditorGUILayout.Space ();

		GUILayout.BeginVertical("Inventory Show Object Settings", "window");
		EditorGUILayout.PropertyField (list.FindProperty ("rotationSpeed"));
		EditorGUILayout.PropertyField (list.FindProperty ("zoomSpeed"));
		EditorGUILayout.PropertyField (list.FindProperty ("maxZoomValue"));
		EditorGUILayout.PropertyField (list.FindProperty ("minZoomValue"));
		GUILayout.EndVertical();

		EditorGUILayout.Space ();

		GUILayout.BeginVertical("Inventory Capture Tool Settings", "window");
		EditorGUILayout.PropertyField (list.FindProperty ("useRelativePath"));
		if (list.FindProperty ("useRelativePath").boolValue) {
			EditorGUILayout.PropertyField (list.FindProperty ("relativePath"));
		}
		GUILayout.EndVertical();
		GUILayout.EndVertical ();

		EditorGUILayout.Space ();
		buttonColor = GUI.backgroundColor;
		showElementSettings = list.FindProperty ("showElementSettings").boolValue;
		EditorGUILayout.BeginVertical();
		string inputListOpenedText = "";
		if (showElementSettings) {
			GUI.backgroundColor = Color.gray;
			inputListOpenedText = "Hide Element Settings";
		} else {
			GUI.backgroundColor = buttonColor;
			inputListOpenedText = "Show Element Settings";
		}
		if (GUILayout.Button (inputListOpenedText)) {
			showElementSettings = !showElementSettings;
		}
		GUI.backgroundColor = buttonColor;
		EditorGUILayout.EndHorizontal();
		list.FindProperty ("showElementSettings").boolValue = showElementSettings;
		if (showElementSettings) {
			GUILayout.BeginVertical ("box");
			EditorGUILayout.Space ();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox ("Set here every element used in the inventory", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space ();
			EditorGUILayout.Space ();
			EditorGUILayout.PropertyField (list.FindProperty ("inventoryPanel"));
			EditorGUILayout.PropertyField (list.FindProperty ("inventoryListContent"));
			EditorGUILayout.PropertyField (list.FindProperty ("objectIcon"));
			EditorGUILayout.PropertyField (list.FindProperty ("useButton"));
			EditorGUILayout.PropertyField (list.FindProperty ("equipButton"));
			EditorGUILayout.PropertyField (list.FindProperty ("dropButton"));
			EditorGUILayout.PropertyField (list.FindProperty ("currentObjectName"));
			EditorGUILayout.PropertyField (list.FindProperty ("currentObjectInfo"));
			EditorGUILayout.PropertyField (list.FindProperty ("objectImage"));
			EditorGUILayout.PropertyField (list.FindProperty ("inventoryCamera"));
			EditorGUILayout.PropertyField (list.FindProperty ("lookObjectsPosition"));
			EditorGUILayout.PropertyField (list.FindProperty ("emptyInventoryPrefab"));
			GUILayout.EndVertical ();
		}

		EditorGUILayout.Space ();
		GUILayout.BeginVertical ("Inventory List", "window");
		showUpperList (list.FindProperty ("inventoryList"));
		GUILayout.EndVertical ();
		if (GUI.changed) {
			list.ApplyModifiedProperties ();
		}
		EditorGUILayout.Space ();
	}
	void showListElementInfo(SerializedProperty list, bool expanded, int index){
		GUILayout.BeginVertical("box");
		EditorGUILayout.PropertyField(list.FindPropertyRelative("Name"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("inventoryGameObject"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("objectInfo"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("icon"));

		GUILayout.BeginHorizontal();
		GUILayout.Label ("Object Icon Preview \t");
		GUILayout.BeginHorizontal("box", GUILayout.Height(50), GUILayout.Width(50));
		if (list.FindPropertyRelative ("icon").objectReferenceValue && expanded) {
			Object texture = list.FindPropertyRelative ("icon").objectReferenceValue as Texture2D;
			Texture2D myTexture = AssetPreview.GetAssetPreview (texture);
			GUILayout.Label (myTexture, GUILayout.Width (50), GUILayout.Height (50));
		}
		GUILayout.EndHorizontal();
		GUILayout.Label ("");
		GUILayout.EndHorizontal();

		if (GUILayout.Button ("Open Inventory Capture Tool")) {
			inventoryWindow = (inventoryCaptureManager)EditorWindow.GetWindow (typeof(inventoryCaptureManager));
			inventoryWindow.init ();


			inventoryWindow.setCurrentInventoryObjectInfo (inventory.inventoryList[index],inventory.getDataPath());
		}

		EditorGUILayout.PropertyField(list.FindPropertyRelative("amount"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("canBeUsed"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("canBeEquiped"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("canBeDropped"));

//		EditorGUILayout.PropertyField(list.FindPropertyRelative("button"));
//		EditorGUILayout.PropertyField(list.FindPropertyRelative("menuIconElement"));

		GUILayout.EndVertical();
	}
	void showUpperList(SerializedProperty list){
		GUILayout.BeginVertical();
		EditorGUILayout.PropertyField(list);
		if (list.isExpanded){
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add Object")){
				inventory.addNewInventoryObject ();
			}
			if (GUILayout.Button("Clear")){
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
						showListElementInfo (list.GetArrayElementAtIndex (i),expanded, i);
					}
					EditorGUILayout.Space();
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
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