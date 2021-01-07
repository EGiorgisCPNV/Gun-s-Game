using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
//a simple editor to add a button in the ragdollBuilder script inspector
[CustomEditor(typeof(tagLayerSystem))]
public class tagLayerSystemEditor : Editor{
	SerializedObject list;
	tagLayerSystem settingsManager;
	bool tagMenu;
	bool layerMenu;
	Color defBackgroundColor;

	void OnEnable(){
		list = new SerializedObject(target);
		settingsManager = (tagLayerSystem)target;
	}
	public override void OnInspectorGUI(){
		if (list == null) {
			return;
		}
		list.Update ();

		EditorGUILayout.Space();

		GUILayout.BeginVertical("box");

		EditorGUILayout.Space();
		GUI.color = Color.cyan;
		EditorGUILayout.HelpBox("Manage all Tags/Layers with this editor", MessageType.None);
		GUI.color = Color.white;

		EditorGUILayout.Space();
		EditorGUILayout.Space();
		defBackgroundColor = GUI.backgroundColor;
		EditorGUILayout.BeginHorizontal();
		if (tagMenu) {
			GUI.backgroundColor = Color.gray;
		} else {
			GUI.backgroundColor = defBackgroundColor;
		}
		if (GUILayout.Button ("Tag Menu")) {
			tagMenu = !tagMenu;
		}
		if (layerMenu) {
			GUI.backgroundColor = Color.gray;
		} else {
			GUI.backgroundColor = defBackgroundColor;
		}
		if (GUILayout.Button ("Layer Menu")) {
			layerMenu = !layerMenu;
		}
		GUI.backgroundColor = defBackgroundColor;
		EditorGUILayout.EndHorizontal();
		if (tagMenu) {
			GUILayout.BeginVertical ("box");
			EditorGUILayout.Space();
			GUILayout.Label ("TAG MANAGER");
			EditorGUILayout.Space();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox("Add/remove tags one by one or in a list", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			GUILayout.Label ("ADD SINGLE TAG");
			EditorGUILayout.Space ();
			GUILayout.BeginVertical ("box");
			EditorGUILayout.PropertyField (list.FindProperty ("newTag"), new GUIContent ("New Tag To Add"), false);
			EditorGUILayout.Space ();
			if (list.FindProperty ("newTag").stringValue.Length > 0) {
				if (GUILayout.Button ("Add Tag")) {
					settingsManager.addTag (list.FindProperty ("newTag").stringValue);
				}
			}
			GUILayout.EndVertical ();

			EditorGUILayout.Space ();

			GUILayout.Label ("ADD TAG LIST");
			EditorGUILayout.Space ();
			GUILayout.BeginVertical ("New Tag List", "window", GUILayout.Height (50));
			showLowerList (list.FindProperty ("newTagList"), true, false, false);
			EditorGUILayout.Space ();
			if (list.FindProperty ("newTagList").arraySize > 0) {
				if (GUILayout.Button ("Add Tag List")) {
					settingsManager.addTagList (settingsManager.newTagList);
				}
			}
			GUILayout.EndVertical ();

			EditorGUILayout.Space ();

			GUILayout.Label ("GET TAG LIST");
			EditorGUILayout.Space ();
			GUILayout.BeginVertical ("Tag List", "window", GUILayout.Height (50));
			showLowerList (list.FindProperty ("tagList"), false, true, false);
			EditorGUILayout.Space ();
			if (GUILayout.Button ("Get Tag List")) {
				settingsManager.getTagList ();
			}
			EditorGUILayout.Space ();
			if (GUILayout.Button ("Add Current Tag List")) {
				settingsManager.addCurrentTagList ();
			}
			GUILayout.EndVertical ();

			EditorGUILayout.Space ();
			GUILayout.EndVertical ();
		}
		if (layerMenu) {
			GUILayout.BeginVertical ("box");
			EditorGUILayout.Space();
			GUILayout.Label ("LAYER MANAGER");
			EditorGUILayout.Space();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox("Add/remove layers one by one or in a list", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			GUILayout.Label ("ADD SINGLE LAYER");
			EditorGUILayout.Space ();
			GUILayout.BeginVertical ("box");
			EditorGUILayout.PropertyField (list.FindProperty ("newLayer"), new GUIContent ("New Layer To Add"), false);
			EditorGUILayout.Space ();
			if (list.FindProperty ("newLayer").stringValue.Length > 0) {
				if (GUILayout.Button ("Add Layer")) {
					settingsManager.addLayer (list.FindProperty ("newLayer").stringValue);
				}
			}
			GUILayout.EndVertical ();

			EditorGUILayout.Space ();

			GUILayout.Label ("ADD LAYER LIST");
			EditorGUILayout.Space ();
			GUILayout.BeginVertical ("New Layer List", "window", GUILayout.Height (50));
			showLowerList (list.FindProperty ("newLayerList"), true, false, false);
			EditorGUILayout.Space ();
			if (list.FindProperty ("newLayerList").arraySize > 0) {
				if (GUILayout.Button ("Add Layer List")) {
					settingsManager.addLayerList (settingsManager.newLayerList);
				}
			}
			GUILayout.EndVertical ();


			GUILayout.Label ("GET LAYER LIST");
			EditorGUILayout.Space ();
			GUILayout.BeginVertical ("Layer List", "window", GUILayout.Height (50));
			showLowerList (list.FindProperty ("layerList"), false, false, true);
			EditorGUILayout.Space ();
			if (GUILayout.Button ("Get Layer List")) {
				settingsManager.getLayerList ();
			}
			EditorGUILayout.Space ();
			if (GUILayout.Button ("Add Current Layer List")) {
				settingsManager.addCurrentLayerList();
			}
			GUILayout.EndVertical ();

			EditorGUILayout.Space ();
			GUILayout.EndVertical ();
		}

		EditorGUILayout.Space();
		EditorGUILayout.Space();
		GUILayout.EndVertical ();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		GUI.backgroundColor = defBackgroundColor;
		if (GUI.changed){
			list.ApplyModifiedProperties();
		}
	}
	void showLowerList(SerializedProperty list, bool managerList, bool isTag, bool isLayer){
		EditorGUILayout.PropertyField(list);
		if (list.isExpanded){
			if (managerList) {
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Add Element")) {
					list.arraySize++;
				}
				if (GUILayout.Button ("Clear List")) {
					list.arraySize = 0;
				}
				GUILayout.EndHorizontal ();
			}
			EditorGUILayout.Space();
			GUILayout.BeginVertical ("box");
			EditorGUILayout.Space();
			if (!managerList) {
				GUILayout.Label (list.arraySize+" Elements");
				EditorGUILayout.Space();
			}
			for (int i = 0; i < list.arraySize; i++){
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("x")){
					if (isTag) {
						Debug.Log ("remove tag");
						string tagToRemove = list.GetArrayElementAtIndex (i).stringValue as string;
						settingsManager.removeTag (tagToRemove);
						list.DeleteArrayElementAtIndex (i);
					}
					if (isLayer) {
						Debug.Log ("remove layer");
						string layerToRemove = list.GetArrayElementAtIndex (i).stringValue as string;
						settingsManager.removeLayer (layerToRemove);
						list.DeleteArrayElementAtIndex (i);
					}
					if (!isTag && !isLayer) {
						list.DeleteArrayElementAtIndex (i);
					}
				}
				if (i < list.arraySize && i >= 0) {
					EditorGUILayout.PropertyField (list.GetArrayElementAtIndex (i), new GUIContent ("", null, ""));
				}
				GUILayout.EndHorizontal();
			}
			EditorGUILayout.Space();
			GUILayout.EndVertical ();
		}       
	}
}
#endif
