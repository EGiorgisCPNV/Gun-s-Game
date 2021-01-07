using UnityEngine;
using System.Collections;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
//a simple editor to add a button in the ragdollBuilder script inspector
[CustomEditor(typeof(mapSystem))]
public class mapSystemEditor : Editor{
	SerializedObject list;
	bool mapComponents;
	bool mapSettings;
	bool compassComponents;
	bool compassSettings;
	bool mapFloorAndIcons;
	bool markSettings;
	Color defBackgroundColor;

	void OnEnable(){
		list = new SerializedObject(target);
	}
	public override void OnInspectorGUI(){
		if (list == null) {
			return;
		}
		list.Update ();
		GUILayout.BeginVertical("box");
		EditorGUILayout.PropertyField(list.FindProperty("mapEnabled"));
		GUILayout.Label ("Current Floor\t\t" + (list.FindProperty ("currentFloor").intValue + 1).ToString ());
		EditorGUILayout.Space();
		GUILayout.EndVertical();
		EditorGUILayout.Space();

		defBackgroundColor = GUI.backgroundColor;
		EditorGUILayout.BeginVertical();
		if (mapComponents) {
			GUI.backgroundColor = Color.gray;
		} else {
			GUI.backgroundColor = defBackgroundColor;
		}
		if (GUILayout.Button ("Map Components")) {
			mapComponents = !mapComponents;
		}
		if (mapSettings) {
			GUI.backgroundColor = Color.gray;
		} else {
			GUI.backgroundColor = defBackgroundColor;
		}
		if (GUILayout.Button ("Map Settings")) {
			mapSettings = !mapSettings;
		}
		if (markSettings) {
			GUI.backgroundColor = Color.gray;
		} else {
			GUI.backgroundColor = defBackgroundColor;
		}
		if (GUILayout.Button ("Mark Settings")) {
			markSettings = !markSettings;
		}
		if (compassComponents) {
			GUI.backgroundColor = Color.gray;
		} else {
			GUI.backgroundColor = defBackgroundColor;
		}
		if (GUILayout.Button ("Compass Components")) {
			compassComponents = !compassComponents;
		}
		if (compassSettings) {
			GUI.backgroundColor = Color.gray;
		} else {
			GUI.backgroundColor = defBackgroundColor;
		}
		if (GUILayout.Button ("Compass Settings")) {
			compassSettings = !compassSettings;
		}
		if (mapFloorAndIcons) {
			GUI.backgroundColor = Color.gray;
		} else {
			GUI.backgroundColor = defBackgroundColor;
		}
		if (GUILayout.Button ("Map Floor And Icons")) {
			mapFloorAndIcons = !mapFloorAndIcons;
		}
		GUI.backgroundColor = defBackgroundColor;
		EditorGUILayout.EndVertical();
		if(mapComponents){
			GUILayout.BeginVertical("box");
			EditorGUILayout.Space();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox("Set every Map Component here", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(list.FindProperty("mapContent"));
			EditorGUILayout.PropertyField(list.FindProperty("mapCamera"));
			EditorGUILayout.PropertyField(list.FindProperty("player"));
			EditorGUILayout.PropertyField(list.FindProperty("mapMenu"));
			EditorGUILayout.PropertyField(list.FindProperty("mapWindowTargetPosition"));
			EditorGUILayout.PropertyField(list.FindProperty("mapRender"));
			EditorGUILayout.PropertyField(list.FindProperty("mapWindow"));
			EditorGUILayout.PropertyField(list.FindProperty("playerMapIcon"));
			EditorGUILayout.PropertyField(list.FindProperty("removeMarkButton"));
			EditorGUILayout.PropertyField(list.FindProperty("mapObjectNameField"));
			EditorGUILayout.PropertyField(list.FindProperty("mapObjectInfoField"));
			EditorGUILayout.PropertyField(list.FindProperty("quickTravelButton"));
			EditorGUILayout.PropertyField(list.FindProperty("currentFloorNumber"));
			EditorGUILayout.PropertyField(list.FindProperty("useMapIndexWindow"));
			EditorGUILayout.PropertyField(list.FindProperty("mapIndexWindow"));
			EditorGUILayout.PropertyField(list.FindProperty("mapIndexWindowContent"));
			EditorGUILayout.PropertyField(list.FindProperty("mapIndexWindowScroller"));
			EditorGUILayout.Space();
			GUILayout.EndVertical ();
		}
		if (mapSettings) {
			GUILayout.BeginVertical("box");
			EditorGUILayout.Space();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox("Map Settings", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			GUILayout.BeginVertical("CONTROL", "window", GUILayout.Height(30));
			EditorGUILayout.PropertyField(list.FindProperty("playerIconMovementSpeed"));
			EditorGUILayout.PropertyField(list.FindProperty("openMapSpeed"));
			EditorGUILayout.PropertyField(list.FindProperty("dragMapSpeed"));
			GUILayout.EndVertical ();

			EditorGUILayout.Space();

			GUILayout.BeginVertical("ROTATION", "window", GUILayout.Height(30));
			EditorGUILayout.PropertyField(list.FindProperty("rotateMap"));
			if (list.FindProperty ("rotateMap").boolValue) {
				EditorGUILayout.PropertyField (list.FindProperty ("smoothRotationMap"));
				EditorGUILayout.PropertyField (list.FindProperty ("rotationSpeed"));
			}
			GUILayout.EndVertical ();

			EditorGUILayout.Space();

			GUILayout.BeginVertical("ICONS", "window", GUILayout.Height(30));
			EditorGUILayout.PropertyField(list.FindProperty("showOffScreenIcons"));
			EditorGUILayout.PropertyField(list.FindProperty("iconSize"));
			EditorGUILayout.PropertyField(list.FindProperty("offScreenIconSize"));
			EditorGUILayout.PropertyField(list.FindProperty("openMapIconSizeMultiplier"));
			EditorGUILayout.PropertyField(list.FindProperty("changeIconSizeSpeed"));
			EditorGUILayout.PropertyField(list.FindProperty("showIconsByFloor"));
			EditorGUILayout.PropertyField(list.FindProperty("borderOffScreen"));
			GUILayout.EndVertical ();

			EditorGUILayout.Space();

			GUILayout.BeginVertical("ZOOM", "window", GUILayout.Height(30));
			EditorGUILayout.PropertyField(list.FindProperty("zoomWhenOpen"));
			EditorGUILayout.PropertyField(list.FindProperty("zoomWhenClose"));
			EditorGUILayout.PropertyField(list.FindProperty("openCloseZoomSpeed"));
			EditorGUILayout.PropertyField(list.FindProperty("zoomSpeed"));
			EditorGUILayout.PropertyField(list.FindProperty("maxZoom"));
			EditorGUILayout.PropertyField(list.FindProperty("minZoom"));
			GUILayout.EndVertical ();

			EditorGUILayout.Space();

			GUILayout.BeginVertical("MARKS", "window", GUILayout.Height(30));
			EditorGUILayout.PropertyField(list.FindProperty("disabledRemoveMarkColor"));
			EditorGUILayout.PropertyField(list.FindProperty("disabledQuickTravelColor"));
			GUILayout.EndVertical ();

			EditorGUILayout.Space();

			GUILayout.EndVertical ();
		}
		if (markSettings) {
			GUILayout.BeginVertical("box");
			EditorGUILayout.Space ();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox ("Mark Settings", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space ();
			EditorGUILayout.Space ();
			EditorGUILayout.PropertyField (list.FindProperty ("showOffScreenIcon"));
			EditorGUILayout.PropertyField (list.FindProperty ("showMapWindowIcon"));
			EditorGUILayout.PropertyField (list.FindProperty ("showDistance"));
			EditorGUILayout.PropertyField (list.FindProperty ("markVisibleInAllFloors"));
			EditorGUILayout.PropertyField (list.FindProperty ("useDefaultObjectiveRadius"));
			if (!list.FindProperty ("useDefaultObjectiveRadius").boolValue) {
				EditorGUILayout.PropertyField (list.FindProperty ("markRadiusDistance"));
			}
			EditorGUILayout.Space ();
			GUILayout.EndVertical ();
		}
		if (compassComponents) {
			GUILayout.BeginVertical("box");
			EditorGUILayout.Space();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox("Set every Compass Component here", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(list.FindProperty("compassWindow"));
			EditorGUILayout.PropertyField(list.FindProperty("north"));
			EditorGUILayout.PropertyField(list.FindProperty("south"));
			EditorGUILayout.PropertyField(list.FindProperty("east"));
			EditorGUILayout.PropertyField(list.FindProperty("west"));
			EditorGUILayout.Space();
			GUILayout.EndVertical ();
		}
		if (compassSettings) {
			GUILayout.BeginVertical("box");
			EditorGUILayout.Space();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox("Compass Settings", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(list.FindProperty("compassEnabled"));
			EditorGUILayout.PropertyField(list.FindProperty("showIntermediateDirections"));
			EditorGUILayout.Space();
			GUILayout.EndVertical ();
		}
		if (mapFloorAndIcons) {
			GUILayout.BeginVertical("box");
			EditorGUILayout.Space();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox("Configure every Floor and Icon element for the map", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			GUILayout.BeginVertical("Floor List", "window");
			showLowerList(list.FindProperty("floors"));
			GUILayout.EndVertical();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			GUILayout.BeginVertical("Map Icons List", "window");
			showUpperList(list.FindProperty("mapIconTypes"));
			GUILayout.EndVertical();
			EditorGUILayout.Space();
			GUILayout.EndVertical ();
		}
		GUI.backgroundColor = defBackgroundColor;
		if (GUI.changed){
			list.ApplyModifiedProperties();
		}
		EditorGUILayout.Space();
	}
	void showListElementInfo(SerializedProperty list){
		GUILayout.BeginVertical("box");
		EditorGUILayout.PropertyField(list.FindPropertyRelative("typeName"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("icon"));
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("showIconPreview"));
		bool showIconPreview = list.FindPropertyRelative ("showIconPreview").boolValue;
		if (showIconPreview) {
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Icon Preview \t");
			GUILayout.BeginHorizontal ("box", GUILayout.Height (50), GUILayout.Width (50));
			if (list.FindPropertyRelative ("icon").objectReferenceValue) {
				RectTransform icon = list.FindPropertyRelative ("icon").objectReferenceValue as RectTransform;
				Object texture = new Object ();
				if (icon.GetComponent<RawImage> ()) {
					texture = icon.GetComponent<RawImage> ().texture;
				} else if (icon.GetComponent<Image> ()) {
					texture = icon.GetComponent<Image> ().sprite;
				}
				Texture2D myTexture = AssetPreview.GetAssetPreview (texture);
				GUILayout.Label (myTexture, GUILayout.Width (50), GUILayout.Height (50));
			}
			GUILayout.EndHorizontal ();
			GUILayout.Label ("");
			GUILayout.EndHorizontal ();
			EditorGUILayout.Space ();
		}

		GUILayout.EndVertical();
	}
	void showUpperList(SerializedProperty list){
		GUILayout.BeginVertical();
		EditorGUILayout.PropertyField(list);
		if (list.isExpanded){
			EditorGUILayout.Space ();
			GUILayout.Label ("Number Of Map Icons: " + list.arraySize.ToString ());
			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add Icon")){
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
	void showLowerList(SerializedProperty list){
		EditorGUILayout.PropertyField(list);
		if (list.isExpanded){
			EditorGUILayout.Space();
			GUILayout.Label ("Number of floors: " + list.arraySize.ToString ());
			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add Floor")){
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
				GUILayout.EndHorizontal();
			}
		}       
	}
}
#endif