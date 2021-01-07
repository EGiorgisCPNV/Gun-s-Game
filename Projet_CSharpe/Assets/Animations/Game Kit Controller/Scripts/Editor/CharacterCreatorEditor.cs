using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using UnityEngine.EventSystems;

public class CharacterCreatorEditor : EditorWindow
{
	GameObject currentCharacterGameObject;
	Animator charAnimator;
	RuntimeAnimatorController controller;
	Vector2 rect = new Vector2 (400, 400);
	Editor characterPreview;
	bool modelIsHumanoid;
	bool correctAnimatorAvatar;
	bool characterModelSelected;
	GameObject character;
	bool characterCreated;
	float timeToBuild = 0.2f;
	float timer;
	string assetsPath=Application.dataPath;

	[MenuItem ("Game Kit Controller/Create New Player")]
	static void createNewPlayer ()
	{
		GetWindow<CharacterCreatorEditor> ();
	}

	void OnEnable ()
	{
		loadAllAssets (Application.dataPath);
	}

	void OnGUI ()
	{
		this.minSize = rect;
		this.titleContent = new GUIContent ("Character", null, "Game Kit Controller Character Creator");
		GUILayout.BeginVertical ("Character Creator Window", "window");
		GUILayout.BeginVertical ("box");
		if (!currentCharacterGameObject) {
			EditorGUILayout.HelpBox ("The FBX model needs to be humanoid", MessageType.Info);
		} else if (!characterModelSelected) {
			EditorGUILayout.HelpBox ("The object needs an animator component", MessageType.Error);
		} else if (!modelIsHumanoid) {
			EditorGUILayout.HelpBox ("The model is not humanoid", MessageType.Error);
		} else if (!correctAnimatorAvatar) {
			EditorGUILayout.HelpBox (currentCharacterGameObject.name + " is not a valid humanoid", MessageType.Info);
		}
		currentCharacterGameObject = EditorGUILayout.ObjectField ("FBX Model", currentCharacterGameObject, typeof(GameObject), true, GUILayout.ExpandWidth (true)) as GameObject;
		if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect ().Contains (Event.current.mousePosition)) {
			loadAllAssets (assetsPath);
		}
		if (GUI.changed && currentCharacterGameObject != null) {
			characterPreview = Editor.CreateEditor (currentCharacterGameObject);
		}
		GUILayout.EndVertical ();
		if (currentCharacterGameObject) {
			charAnimator = currentCharacterGameObject.GetComponent<Animator> ();
		}
		if (charAnimator != null) {
			characterModelSelected = true;
		} else {
			characterModelSelected = false;
		}
		if (characterModelSelected && charAnimator.isHuman) {
			modelIsHumanoid = true;
		} else {
			modelIsHumanoid = false;
		}
		if (characterModelSelected && charAnimator.avatar.isValid) {
			correctAnimatorAvatar = true;
		} else {
			correctAnimatorAvatar = false;
		}
		if (currentCharacterGameObject) {
			if (characterPreview != null) {
				characterPreview.OnInteractivePreviewGUI (GUILayoutUtility.GetRect (100, 400), "window");
			}
		}
		if (correctAnimatorAvatar && modelIsHumanoid) {
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Create Player")) {
				createCharacter ();
			}
			GUILayout.EndHorizontal ();
		}
		GUILayout.EndVertical ();
	}

	void loadAllAssets (string path)
	{
		if (!Directory.Exists (path)) {
			Directory.CreateDirectory (path);
		}
		DirectoryInfo directoryInfo = new DirectoryInfo (path);
		DirectoryInfo[] directoryInfoList = directoryInfo.GetDirectories ("*", SearchOption.TopDirectoryOnly);
		foreach (DirectoryInfo directory in directoryInfoList) {
			loadAllAssets (directory.FullName);
		}
	}

	void createCharacter ()
	{
		GameObject previousCharacter = GameObject.Find ("GKC_Prefab");
		if (previousCharacter) {
			DestroyImmediate (previousCharacter);
		}
		character = (GameObject)AssetDatabase.LoadAssetAtPath ("Assets/Game Kit Controller/Prefabs/Player Controller/GKC_Prefab (Without Model).prefab", typeof(GameObject));
		if (character) {
			character = (GameObject)Instantiate (character, Vector3.zero, Quaternion.identity);
			character.name = "GKC_Prefab";
			Transform gravityCenter = character.GetComponentInChildren<changeGravity> ().settings.gravityCenter;
			GameObject model = GameObject.Instantiate (currentCharacterGameObject, Vector3.zero, Quaternion.identity) as GameObject;
			model.transform.SetParent (gravityCenter);
			model.transform.localPosition = gravityCenter.transform.localPosition * (-1);
			model.name = currentCharacterGameObject.name;
			character.GetComponentInChildren<Animator> ().avatar = model.GetComponentInChildren<Animator> ().avatar;
			character.transform.position = Vector3.zero;
			characterCreated = true;

		} else {
			Debug.Log ("Character prefab not found in path Assets/Game Kit Controller/Prefabs/Player Controller/GKC_Prefrab (Without Model).prefab");
		}
	}

	void Update ()
	{
		if (characterCreated) {

			if (timer < timeToBuild) {
				timer += 0.01f;
				if (timer > timeToBuild) {
					character.GetComponentInChildren<buildPlayer> ().buildBody ();
					characterCreated = false;
					timer = 0;
					this.Close ();
				}
			}
		}
	}

}