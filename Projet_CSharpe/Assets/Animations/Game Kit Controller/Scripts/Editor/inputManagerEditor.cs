using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
//a simple editor to add some buttons in the inputmanager script inspector
[CustomEditor(typeof(inputManager))]
public class inputManagerEditor : Editor{
	SerializedObject list;
	inputManager input;
	string controlScheme;
	bool checkState;
	bool saveSettings;
	Color buttonColor;

	void OnEnable(){
		list = new SerializedObject(target);
	}
	public override void OnInspectorGUI(){
		if (list == null) {
			return;
		}
		list.Update ();
		GUILayout.BeginVertical ("box");
		EditorGUILayout.PropertyField (list.FindProperty ("editInputMenu"), new GUIContent ("Edit Input Menu"), false);
		EditorGUILayout.PropertyField (list.FindProperty ("buttonPrefab"), new GUIContent ("Button Prefab"), false);
		EditorGUILayout.PropertyField (list.FindProperty ("touchMovementControl"), new GUIContent ("Touch Movement Control"), false);
		EditorGUILayout.PropertyField (list.FindProperty ("touchCameraControl"), new GUIContent ("Touch Camera Control"), false);
		EditorGUILayout.PropertyField (list.FindProperty ("leftTouchSensitivity"), new GUIContent ("Left Touch Sensitivity"), false);
		EditorGUILayout.PropertyField (list.FindProperty ("rightTouchSensitivity"), new GUIContent ("Right Touch Sensitivity"), false);
		EditorGUILayout.PropertyField (list.FindProperty ("mouseSensitivity"), new GUIContent ("Mouse Sensitivity"), false);
		EditorGUILayout.PropertyField (list.FindProperty ("touchPanel"), new GUIContent ("Touch Panel"), false);
		showUpperList (list.FindProperty ("buttonsDisabledAtStart"));
		buttonColor = GUI.backgroundColor;
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.Space ();
		if (saveSettings) {
			GUI.backgroundColor = Color.gray;
		} else {
			GUI.backgroundColor = buttonColor;
		}
		if (GUILayout.Button ("Save Settings")) {
			saveSettings = !saveSettings;
		}
		EditorGUILayout.Space ();
		GUI.backgroundColor = buttonColor;
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space ();
		if (saveSettings) {
			GUILayout.BeginVertical("box");
			EditorGUILayout.Space ();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox ("Configure the save/load settings and path for files", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space ();
			EditorGUILayout.Space ();
			EditorGUILayout.PropertyField (list.FindProperty ("loadOption"), new GUIContent ("Load Option"), false);
			EditorGUILayout.PropertyField (list.FindProperty ("useRelativePath"));
			if (list.FindProperty ("useRelativePath").boolValue) {
				EditorGUILayout.PropertyField (list.FindProperty ("relativePath"));
				EditorGUILayout.PropertyField (list.FindProperty ("saveFileName"));
			}
			EditorGUILayout.Space ();
			GUILayout.EndVertical ();
			EditorGUILayout.Space ();
		}
		GUILayout.EndVertical ();
		EditorGUILayout.Space ();
		GUILayout.BeginVertical ("Input List", "window");
		showUpperList (list.FindProperty ("axes"));
		GUILayout.EndVertical ();
		if (GUI.changed) {
			list.ApplyModifiedProperties ();
		}
		EditorGUILayout.Space ();
		//DrawDefaultInspector ();
		inputManager manager = (inputManager)target;
		//check the current controls enabled
		if (!checkState) {
			if (manager.menus.useTouchControls) {
				controlScheme = "Mobile";
			} else {
				controlScheme = "Keyboard";
			}
			checkState = true;
		}
		//add a new axe
		GUILayout.Label ("\nAxes List options");
		if (GUILayout.Button ("Add New Axe")) {
			manager.addNewAxe ();
			EditorUtility.SetDirty (manager);
		}
		GUILayout.Label ("\nInput scheme options");
		//set the axes list in the inspector to the default value
		if (GUILayout.Button ("Default")) {
			manager.setToDefault ();
			EditorUtility.SetDirty (manager);
		}
		//save the axes list in the inspector in a file
		if (GUILayout.Button ("Save To File")) {
			manager.saveButtonsInputFromInspector ();
			EditorUtility.SetDirty (manager);
		}
		//set the axes list in the inspector to the values stored in a file
		if (GUILayout.Button ("Load From File")) {
			manager.loadButtonsInputFromInspector ();
			EditorUtility.SetDirty (manager);
		}
		//show the controls scheme
		GUILayout.Label ("\nCURRENT CONTROLS: " + controlScheme);
		//set the keyboard controls
		if (GUILayout.Button ("Set Keyboard Controls")) {
			manager.setKeyboardControls (true);
			controlScheme = "Keyboard";
		}
		//set the touch controls
		if (GUILayout.Button ("Set Touch Controls")) {
			manager.setKeyboardControls (false);
			controlScheme = "Mobile";
		}
	}
	void showListElementInfo(SerializedProperty list){
		GUILayout.BeginVertical ("box");
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("Name"), new GUIContent ("Name"), false);
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("keyButton"), new GUIContent ("Key Button"), false);
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("touchButton"), new GUIContent ("Touch Button"), false);
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("joystickButton"), new GUIContent ("Joystick Button"), false);
		EditorGUILayout.PropertyField (list.FindPropertyRelative ("actionEnabled"), new GUIContent ("Action Enabled"), false);

		if(!Application.isPlaying){
			Color listButtonBackgroundColor;
			EditorGUILayout.Space();
			touchButtonListener buttonListener = list.FindPropertyRelative ("touchButton").objectReferenceValue as touchButtonListener;
			if (buttonListener) {
				RectTransform buttonRectTransform = buttonListener.gameObject.GetComponent<RectTransform> ();
				Vector2 scale = buttonRectTransform.sizeDelta;

				bool scaleTouchButton = list.FindPropertyRelative ("scaleTouchButtonInEditor").boolValue;
				listButtonBackgroundColor = GUI.backgroundColor;
				EditorGUILayout.BeginHorizontal ();
				string inputListOpenedText = "";
				if (scaleTouchButton) {
					GUI.backgroundColor = Color.gray;
					inputListOpenedText = "Hide Touch Button Scale";
				} else {
					GUI.backgroundColor = buttonColor;
					inputListOpenedText = "Show Touch Button Scale";
				}
				if (GUILayout.Button (inputListOpenedText)) {
					scaleTouchButton = !scaleTouchButton;
				}
				GUI.backgroundColor = listButtonBackgroundColor;
				EditorGUILayout.EndHorizontal ();
				list.FindPropertyRelative ("scaleTouchButtonInEditor").boolValue = scaleTouchButton;
				if (scaleTouchButton) {

					bool sameWidhtHeight = list.FindPropertyRelative ("sameTouchButtonWidhtHeight").boolValue;
					GUILayout.BeginHorizontal ();
					string tobbleText = "Widht = Height";
					if (sameWidhtHeight) {
						tobbleText = "Widht =/= Height";
					}
					GUILayout.Label (tobbleText + "\t");
					sameWidhtHeight = GUILayout.Toggle (sameWidhtHeight, "");
					GUILayout.EndHorizontal ();

					if (sameWidhtHeight) {
						GUILayout.BeginHorizontal ();
						GUILayout.Label ("Scale");
						scale.x = GUILayout.HorizontalSlider (scale.x, 0, 100);
						scale.y = scale.x;
						GUILayout.BeginVertical ("box", GUILayout.Width (35));
						GUILayout.Label (scale.x.ToString ("0"));
						GUILayout.EndVertical ();
						GUILayout.EndHorizontal ();
					} else {
						GUILayout.BeginHorizontal ();
						GUILayout.Label ("X Scale");
						scale.x = GUILayout.HorizontalSlider (scale.x, 0, 100);
						GUILayout.BeginVertical ("box", GUILayout.Width (35));
						GUILayout.Label (scale.x.ToString ("0"));
						GUILayout.EndVertical ();
						GUILayout.EndHorizontal ();
						GUILayout.BeginHorizontal ();
						GUILayout.Label ("Y Scale");
						scale.y = GUILayout.HorizontalSlider (scale.y, 0, 100);
						GUILayout.BeginVertical ("box", GUILayout.Width (35));
						GUILayout.Label (scale.y.ToString ("0"));
						GUILayout.EndVertical ();
						GUILayout.EndHorizontal ();
					}
					list.FindPropertyRelative ("sameTouchButtonWidhtHeight").boolValue = sameWidhtHeight;
					scale.x = Mathf.RoundToInt (scale.x);
					scale.y = Mathf.RoundToInt (scale.y);
					buttonRectTransform.sizeDelta = scale;
				}
				EditorGUILayout.Space ();
			}
		}
		GUILayout.EndVertical ();
	}
	void showUpperList(SerializedProperty list){
		GUILayout.BeginVertical();
		EditorGUILayout.PropertyField(list);
		if (list.isExpanded){
			EditorGUILayout.Space();
			GUILayout.Label ("Number Of Actions: \t" + list.arraySize.ToString ());
			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add Action")){
				list.arraySize++;
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
						showListElementInfo (list.GetArrayElementAtIndex (i));
					}
					EditorGUILayout.Space();
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
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
}
#endif