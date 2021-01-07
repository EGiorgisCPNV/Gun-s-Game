using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;

public class tagLayerSystem : MonoBehaviour {
	public string newTag;
	public string newLayer;
	public List<string> newTagList = new List<string> ();
	public List<string> newLayerList = new List<string> ();
	public List<string> tagList = new List<string> ();
	public List<string> layerList = new List<string> ();
	public List<string> temporalTagList = new List<string> ();
	public List<string> temporalLayerList = new List<string> ();

	public void addTag(string tagToAdd){
		if (tagToAdd != "") {
			SerializedObject tagManager = new SerializedObject (AssetDatabase.LoadAllAssetsAtPath ("ProjectSettings/TagManager.asset") [0]);
			SerializedProperty tagsProp = tagManager.FindProperty ("tags");
			//check if it is not already present
			bool found = false;
			for (int i = 0; i < tagsProp.arraySize; i++) {
				SerializedProperty t = tagsProp.GetArrayElementAtIndex (i);
				//print (t.stringValue.ToString());
				if (t.stringValue.Equals (tagToAdd)) { 
					found = true;
				}
			}
			//if not found, add it
			if (!found) {
				int index = -1;
				if (tagsProp.arraySize > 0) {
					index = tagsProp.arraySize - 1;
				} else {
					index = 0;
				}
				tagsProp.InsertArrayElementAtIndex (index);
				SerializedProperty n = tagsProp.GetArrayElementAtIndex (index);
				tagToAdd = tagToAdd.Replace (" ", "");
				n.stringValue = tagToAdd;
				print ("Tag " + tagToAdd + " added.");
			}
			newTag = "";
			tagManager.ApplyModifiedProperties ();
			getTagList ();
			updateEditor ();
		}
	}
	public void removeTag(string tagToRemove){
		SerializedObject tagManager = new SerializedObject (AssetDatabase.LoadAllAssetsAtPath ("ProjectSettings/TagManager.asset") [0]);
		SerializedProperty tagsProp = tagManager.FindProperty ("tags");
		// First check if it is not already present
		bool found = false;
		int index = -1;
		for (int i = 0; i < tagsProp.arraySize; i++) {
			SerializedProperty t = tagsProp.GetArrayElementAtIndex (i);
			//print (t.stringValue.ToString());
			if (t.stringValue.Equals (tagToRemove)) { 
				found = true;
				index = i;
			}
		}
		if (found) {
			print ("Tag "+tagToRemove+ " removed.");
			tagsProp.DeleteArrayElementAtIndex (index);
		}
		tagManager.ApplyModifiedProperties ();
		getTagList ();
		updateEditor ();
	}
	public void addTagList(List<string> tagListToAdd){
		for (int i = 0; i < tagListToAdd.Count; i++) {
			addTag (tagListToAdd [i]);
		}
		newTagList.Clear ();
		updateEditor ();
	}
	public void addCurrentTagList(){
		temporalTagList = new List<string> (tagList);
		addTagList (temporalTagList);
	}
	public void getTagList(){
		tagList.Clear ();
		SerializedObject tagManager = new SerializedObject (AssetDatabase.LoadAllAssetsAtPath ("ProjectSettings/TagManager.asset") [0]);
		SerializedProperty tagsProp = tagManager.FindProperty ("tags");
		for (int i = 0; i < tagsProp.arraySize; i++) {
			SerializedProperty t = tagsProp.GetArrayElementAtIndex (i);
			tagList.Add (t.stringValue);
		}
		tagManager.ApplyModifiedProperties ();
		updateEditor ();
	}
	public void addLayer(string layerToAdd){
		if (layerToAdd != "") {
			SerializedObject tagManager = new SerializedObject (AssetDatabase.LoadAllAssetsAtPath ("ProjectSettings/TagManager.asset") [0]);
			SerializedProperty layersProp = tagManager.FindProperty ("layers");

			//check if it is not already present
			bool found = false;
			for (int i = 0; i < layersProp.arraySize; i++) {
				SerializedProperty t = layersProp.GetArrayElementAtIndex (i);
				if (t.stringValue.Equals (layerToAdd)) { 
					found = true;
				}
			}
			//if not found, add it
			if (!found) {
				int index = -1;
				for (int i = 0; i < layersProp.arraySize; i++) {
					SerializedProperty t = layersProp.GetArrayElementAtIndex (i);
					if (i > 7 && t.stringValue == "" && index == -1) {
						index = i;
					}
				}
				if (index != -1) {
					//print (layersProp.GetArrayElementAtIndex (index - 1).stringValue);
					layerToAdd = layerToAdd.Replace (" ", "");
					SerializedProperty sp = layersProp.GetArrayElementAtIndex (index);
					if (sp != null) {
						sp.stringValue = layerToAdd;
					}
					print ("Layer " + layerToAdd + " added.");
				}
			}
			// and to save the changes
			tagManager.ApplyModifiedProperties ();
			newLayer = "";
			getLayerList ();
			updateEditor ();
		}
	}
	public void removeLayer(string layerToRemove){
		SerializedObject tagManager = new SerializedObject (AssetDatabase.LoadAllAssetsAtPath ("ProjectSettings/TagManager.asset") [0]);
		SerializedProperty layersProp = tagManager.FindProperty ("layers");
		// First check if it is not already present
		bool found = false;
		int index = -1;
		for (int i = 0; i < layersProp.arraySize; i++) {
			SerializedProperty t = layersProp.GetArrayElementAtIndex (i);
			//print (t.stringValue.ToString());
			if (t.stringValue.Equals (layerToRemove)) { 
				found = true;
				index = i;
			}
		}
		if (found) {
			print ("Layer "+layerToRemove+ " removed.");
			layersProp.DeleteArrayElementAtIndex (index);
		}
		tagManager.ApplyModifiedProperties ();
		getLayerList ();
		updateEditor ();
	}
	public void addLayerList(List<string> layerListToAdd){
		for (int i = 0; i < layerListToAdd.Count; i++) {
			addLayer (layerListToAdd [i]);
		}
		newLayerList.Clear ();
		updateEditor ();
	}
	public void addCurrentLayerList(){
		temporalLayerList = new List<string> (layerList);
		addLayerList (temporalLayerList);
	}
	public void getLayerList(){
		layerList.Clear ();
		SerializedObject tagManager = new SerializedObject (AssetDatabase.LoadAllAssetsAtPath ("ProjectSettings/TagManager.asset") [0]);
		SerializedProperty layersProp = tagManager.FindProperty ("layers");
		for (int i = 0; i < layersProp.arraySize; i++) {
			if (i > 7 && layersProp.GetArrayElementAtIndex(i).stringValue!="") {
				SerializedProperty t = layersProp.GetArrayElementAtIndex (i);
				layerList.Add (t.stringValue);
			}
		}
		tagManager.ApplyModifiedProperties ();
		updateEditor ();
	}
	public void updateEditor(){
		EditorUtility.SetDirty (GetComponent<tagLayerSystem> ());
	}
}
#endif
