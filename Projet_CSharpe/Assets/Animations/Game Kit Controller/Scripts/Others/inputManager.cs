using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif
[System.Serializable]
public class inputManager : MonoBehaviour
{
	public List<Axes> axes = new List<Axes> ();
	public GameObject editInputMenu;
	public GameObject buttonPrefab;
	public loadType loadOption;
	public bool useRelativePath;
	public string relativePath;
	public string saveFileName;
	public touchJoystick touchMovementControl;
	public touchJoystick touchCameraControl;
	[Range (0, 2)] public float leftTouchSensitivity;
	[Range (0, 2)] public float rightTouchSensitivity;
	[Range (0, 2)] public float mouseSensitivity;
	public GameObject touchPanel;
	public List<string> buttonsDisabledAtStart = new List<string> ();
	[HideInInspector] public bool touchControlsCurrentlyEnabled;
	[HideInInspector] public menuPause menus;
	[HideInInspector] public bool touchPlatform;
	[HideInInspector] public bool gamePaused;
	List<editButtonInput> buttonsList = new List<editButtonInput> ();
	List<Axes> auxAxesList = new List<Axes> ();
	List<touchButtonListener> touchButtonList = new List<touchButtonListener> ();
	Scrollbar scroller;
	int i = 0;
	int j = 0;
	bool menusLocated;
	Touch currentTouch;
	touchButtonListener auxButton;
	Vector2 movementAxis;
	Vector2 mouseAxis;
	bool editingInput;
	editButtonInput currentEditButtonInput;
	string currentEditButtonInputPreviouseValue;

	public enum buttonType
	{
		//type of press of a key
		getKey,
		getKeyDown,
		getKeyUp,
		negMouseWheel,
		posMouseWheel,
	}

	//load the key input in the game from the current configuration in the inspector or load from a file
	public enum loadType
	{
		loadFile,
		loadEditorConfig,
	}

	void Start ()
	{
		//print (Application.persistentDataPath);
		//get if the current platform is a touch device
		touchPlatform = touchJoystick.checkTouchPlatform ();
		menus = GetComponent<menuPause> ();
		if (menus) {
			menusLocated = true;
		}
		//if the current platform is a mobile, enable the touch controls in case they are not active
		if (touchPlatform && !menus.useTouchControls) {
			menus.useTouchControls = true;
			menus.reloadStart ();
		}
		//load the key input
		loadButtonsInput ();
		//set the position of the camera joystick according to the screen size
		touchCameraControl.setJoystickPosition ();
		//set the joystick position at the beginning of the game
		touchMovementControl.setJoystickPosition ();
	}

	void Update ()
	{
		//convert the input from keyboard or a touch screen into values to move the player, given the camera direction
		//also, it checks in the player is using a device, like a vehicle
		//convert the mouse input in the tilt angle for the camera or the input from the touch screen depending of the settings
		//also, it checks in the player is using a device, like a vehicle
//		if (Input.GetJoystickNames ().Length > 0) {
//			print ("joystick Connected"+Input.GetJoystickNames().Length);
//		}
		if (!menus.useTouchControls) {
			movementAxis.x = Input.GetAxis ("Horizontal");
			movementAxis.y = Input.GetAxis ("Vertical");

			mouseAxis.x = Input.GetAxis ("Mouse X") * mouseSensitivity;
			mouseAxis.y = Input.GetAxis ("Mouse Y") * mouseSensitivity;
		} else if (menus.useTouchControls) {
			movementAxis.x = touchMovementControl.GetAxis ().x * leftTouchSensitivity;
			movementAxis.y = touchMovementControl.GetAxis ().y * leftTouchSensitivity;

			mouseAxis.x = touchCameraControl.GetAxis ().x * rightTouchSensitivity;
			mouseAxis.y = touchCameraControl.GetAxis ().y * rightTouchSensitivity;
		}

		//if the player is changin this input field, search for any keyboard press
		if (editingInput) {
			foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode))) {
				//set the value of the key pressed in the input field
				if (Input.GetKeyDown (vKey)) {
					//check the the pressed key is not already used for other axe
					if (!checkRepeatInputKey (vKey.ToString (), currentEditButtonInputPreviouseValue)) {
						currentEditButtonInput.actionKeyText.text = vKey.ToString ();
						//stop the checking of the keyboard
						editingInput = false;
					} else {
						print ("key already used");
					}
				}
			}
		}
	}
	//get the current values of the axis keys or the mouse in the input manager
	public Vector2 getMovementAxis (string controlType)
	{
		Vector2 axisValues = Vector2.zero;
		if (controlType == "keys") {
			axisValues = movementAxis;
		}
		if (controlType == "mouse") {
			axisValues = mouseAxis;
		}
		return axisValues;
	}
	//get if the touch controls are enabled, so any other component can check it
	public void changeControlsType (bool state)
	{
		touchControlsCurrentlyEnabled = state;
		touchCameraControl.gameObject.SetActive (touchControlsCurrentlyEnabled);
		touchMovementControl.gameObject.SetActive (touchControlsCurrentlyEnabled);
	}
	//checks if the new key input set in the edit input menu is already defined, to avoid have two actions with the same key
	public bool checkRepeatInputKey (string key, string currentKey)
	{
		if (key == currentKey) {
			return false;
		}
		for (i = 0; i < axes.Count; i++) {
			if (axes [i].keyButton == key) {
				return true;
			}
		}
		return false;
	}

	void getCurrentAxesListFromInspector ()
	{
		//get all the keys field inside the edit input menu
		editInputMenu.SetActive (true);
		//every key field in the edit input button has a editButtonInput component, so create every of them
		GameObject bottom = buttonPrefab.transform.parent.GetChild (0).gameObject;
		for (i = 0; i < axes.Count; i++) {
			GameObject buttonClone = (GameObject)Instantiate (buttonPrefab, buttonPrefab.transform.position, Quaternion.identity);
			buttonClone.transform.SetParent (buttonPrefab.transform.parent);
			buttonClone.name = axes [i].Name;
			editButtonInput currentEditButtonInput = buttonClone.GetComponent<editButtonInput> ();
			currentEditButtonInput.actionNameText.text = axes [i].Name;
			currentEditButtonInput.actionKeyText.text = axes [i].keyButton;
			buttonClone.transform.localScale = Vector3.one;
			buttonsList.Add (currentEditButtonInput);
		}
		//get the scroller in the edit input menu
		scroller = editInputMenu.GetComponentInChildren<Scrollbar> ();
		//set the scroller in the top position
		scroller.value = 1;
		//set the empty element of the list in the bottom of the list
		bottom.transform.SetParent (null);
		bottom.transform.SetParent (buttonPrefab.transform.parent);
		buttonPrefab.SetActive (false);
		//disable the menu
		editInputMenu.SetActive (false);
	}

	public void saveButtonsInput ()
	{
		//for every key field in the edit input menu, save its value and change them in the inputManager inspector aswell
		if (Application.isPlaying) {
			for (i = 0; i < buttonsList.Count; i++) {
				changeKeyValue (buttonsList [i].actionNameText.text, buttonsList [i].actionKeyText.text);
			}
		}
		//create a list of axes to store it, except the touchButtonListener
		List<Axes> axesList = new List<Axes> ();
		for (i = 0; i < axes.Count; i++) {
			Axes axe = new Axes ();
			axe.Name = axes [i].Name;
			axe.keyButton = axes [i].keyButton;
			axesList.Add (axe);
		}
		//save the input list
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Create (getDataPath ()); 
		bf.Serialize (file, axesList);
		file.Close ();
		print ("Input Saved");
	}

	public string getDataPath ()
	{
		string dataPath = "";
		if (useRelativePath) {
			if (!Directory.Exists (relativePath)) {
				Directory.CreateDirectory (relativePath);
			}
			dataPath = relativePath + "/" + saveFileName;
		} else {
			dataPath = Application.persistentDataPath + "/" + saveFileName;
		}
		return dataPath;
	}

	public void loadButtonsInput ()
	{
		List<Axes> axesList = new List<Axes> ();
		//if the configuration is loaded from a file, get a new axes list with the stored values
		if (loadOption == loadType.loadFile) {
			//if the file of buttons exists, get that list
			if (File.Exists (getDataPath ())) {
				BinaryFormatter bf = new BinaryFormatter ();
				FileStream file = File.Open (getDataPath (), FileMode.Open);
				axesList = (List<Axes>)bf.Deserialize (file);
				file.Close ();
				axes.Clear ();
				for (i = 0; i < axesList.Count; i++) {
					axes.Add (axesList [i]);
				}
			}
			//else, get the list created in the inspector
			else {
				for (i = 0; i < axes.Count; i++) {
					axesList.Add (axes [i]);
				}
				saveButtonsInputFromInspector ();
			}
		} 
		//else the new axes list is the axes in the input manager inspector
		else {
			for (i = 0; i < axes.Count; i++) {
				axesList.Add (axes [i]);
				if (axes [i].touchButton) {
					if (buttonsDisabledAtStart.Contains (axes [i].touchButton.gameObject.name)) {
						axes [i].touchButton.gameObject.SetActive (false);
					}
				}
			}
		}
		//get the current list of axes defined in the inspector
		getCurrentAxesListFromInspector ();
		//set in every key field in the edit input menu with the stored key input for every field
		for (i = 0; i < buttonsList.Count; i++) {
			if (i <= axesList.Count - 1) {
				buttonsList [i].actionKeyText.text = axesList [i].keyButton;
			}
		}
		//get the touch buttons of every input, since the class touchButtonListener can't be stored
		if (loadOption == loadType.loadFile) {
			//use an aux list
			auxAxesList.Clear ();
			//create a copy of the current axes
			for (i = 0; i < axes.Count; i++) {
				auxAxesList.Add (axes [i]);
			}
			getTouchButtonList ();
			//set the touch button for every axes, if it had it
			for (i = 0; i < axes.Count; i++) {
				axes [i] = axesList [i];
				if (i <= axesList.Count - 1) {
					axes [i].touchButton = checkIfTouchButton (axes [i].Name) > -1 ? auxButton : null;
				}
			}
			//clear the aux list
			auxAxesList.Clear ();
		}
	}
	//save the input list in the inspector to a file
	public void saveButtonsInputFromInspector ()
	{
		//create a list of axes to store it, except the touchButtonListener
		List<Axes> axesList = new List<Axes> ();
		for (i = 0; i < axes.Count; i++) {
			Axes axe = new Axes ();
			axe.Name = axes [i].Name;
			axe.keyButton = axes [i].keyButton;
			axesList.Add (axe);
		}
		//save the input list
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Create (getDataPath ()); 
		bf.Serialize (file, axesList);
		file.Close ();
	}
	//load the input list from the file to the inspector
	public void loadButtonsInputFromInspector ()
	{
		List<Axes> axesList = new List<Axes> ();
		//if the configuration is loaded from a file, get a new axes list with the stored values
		if (loadOption == loadType.loadFile) {
			if (File.Exists (getDataPath ())) {
				BinaryFormatter bf = new BinaryFormatter ();
				FileStream file = File.Open (getDataPath (), FileMode.Open);
				axesList = (List<Axes>)bf.Deserialize (file);
				file.Close ();
			}
		} 
		//get the touch buttons of every input, since the class touchButtonListener can't be stored
		//get the touch buttons of every input, since the class touchButtonListener can't be stored
		getTouchButtonList ();
		//set the touch button for every axes, if it had it
		for (i = 0; i < axesList.Count; i++) {
			axes.Add (axesList [i]);
			axes [i].touchButton = checkIfTouchButton (axesList [i].Name) > -1 ? auxButton : null;
		}
		//clear the aux list
		auxAxesList.Clear ();
	}

	public void getTouchButtonList ()
	{
		touchButtonList.Clear ();
		touchPanel.SetActive (true);
		Component[] components = touchPanel.GetComponentsInChildren (typeof(touchButtonListener));
		foreach (Component c in components) {
			touchButtonList.Add (c.GetComponent<touchButtonListener> ());
		}
		if (!menus.useTouchControls) {
			touchPanel.SetActive (false);
		}
	}
	//if the controls are set to the default configuration, check the touch buttons, to reassigned them again
	int checkIfTouchButton (string name)
	{
		for (j = 0; j < touchButtonList.Count; j++) {
			//if the axe that we look it this and has a touchbutton, assign it to a touchButtonListener aux
			if (touchButtonList [j].gameObject.name == name) {
				auxButton = touchButtonList [j];
				if (buttonsDisabledAtStart.Contains (touchButtonList [j].gameObject.name)) {
					touchButtonList [j].gameObject.SetActive (false);
				}
				return j;
			}
		}
		return -1;
	}

	public void setToDefault ()
	{
		//get the current list of axes, as an aux list
		auxAxesList.Clear ();
		for (i = 0; i < axes.Count; i++) {
			auxAxesList.Add (axes [i]);
		}
		axes.Clear ();
		getTouchButtonList ();
		//assign the original axes controls, including the touch buttons, if the current button had a touch button, search and get it
		axes.Add (new Axes ("Jump", "Space", checkIfTouchButton ("Jump") > -1 ? auxButton : null));
		axes.Add (new Axes ("Crouch", "X", checkIfTouchButton ("Crouch") > -1 ? auxButton : null));
		axes.Add (new Axes ("Grab Objects", "E", checkIfTouchButton ("Grab Objects") > -1 ? auxButton : null));
		axes.Add (new Axes ("Use Shield", "Q", checkIfTouchButton ("Use Shield") > -1 ? auxButton : null));
		axes.Add (new Axes ("Aim", "G", checkIfTouchButton ("Aim") > -1 ? auxButton : null));
		axes.Add (new Axes ("Change Controls", "C"));
		axes.Add (new Axes ("Change Camera", "V", checkIfTouchButton ("Change Camera") > -1 ? auxButton : null));
		axes.Add (new Axes ("Gravity Power On", "R", checkIfTouchButton ("Gravity Power On") > -1 ? auxButton : null));
		axes.Add (new Axes ("Gravity Power Off", "F", checkIfTouchButton ("Gravity Power Off") > -1 ? auxButton : null));
		axes.Add (new Axes ("Time Bullet", "Z", checkIfTouchButton ("Time Bullet") > -1 ? auxButton : null));
		axes.Add (new Axes ("Run", "LeftShift", checkIfTouchButton ("Run") > -1 ? auxButton : null));
		axes.Add (new Axes ("Zoom", "Tab", checkIfTouchButton ("Zoom") > -1 ? auxButton : null));
		axes.Add (new Axes ("Move Away Camera", "LeftControl", checkIfTouchButton ("Move Away Camera") > -1 ? auxButton : null));
		axes.Add (new Axes ("Shoot", "Mouse0", checkIfTouchButton ("Shoot") > -1 ? auxButton : null));
		axes.Add (new Axes ("Secondary Button", "Mouse1", checkIfTouchButton ("Secondary Button") > -1 ? auxButton : null));
		axes.Add (new Axes ("Activate Devices", "T", checkIfTouchButton ("Activate Devices") > -1 ? auxButton : null));
		axes.Add (new Axes ("Show Powers Slots", "LeftAlt"));
		axes.Add (new Axes ("Next Power", "O"));
		axes.Add (new Axes ("Previous Power", "P"));
		axes.Add (new Axes ("Select Power Slot", "Backslash"));
		axes.Add (new Axes ("Scan", "B", checkIfTouchButton ("Scan") > -1 ? auxButton : null));
		axes.Add (new Axes ("Map", "M"));
		axes.Add (new Axes ("Inventory", "I", checkIfTouchButton ("Inventory") > -1 ? auxButton : null));
		axes.Add (new Axes ("Change Mode", "H", checkIfTouchButton ("Change Mode") > -1 ? auxButton : null));
		axes.Add (new Axes ("Draw Weapon", "Y", checkIfTouchButton ("Draw Weapon") > -1 ? auxButton : null));
		axes.Add (new Axes ("Change Player Control Mode", "J", checkIfTouchButton ("Change Player Control Mode") > -1 ? auxButton : null));
		axes.Add (new Axes ("Pause", "Escape", checkIfTouchButton ("Pause") > -1 ? auxButton : null));
		axes.Add (new Axes ("Friend Menu", "K", checkIfTouchButton ("Friend Menu") > -1 ? auxButton : null));
		axes.Add (new Axes ("Drop Weapon", "N", checkIfTouchButton ("Drop Weapon") > -1 ? auxButton : null));
		auxAxesList.Clear ();
		//set the default value in every field in the edit input menu
		if (Application.isPlaying) {
			if (buttonsList.Count > 0) {
				for (i = 0; i < buttonsList.Count; i++) {
					buttonsList [i].actionKeyText.text = axes [i].keyButton;
				}
			}
		}
		saveButtonsInput ();
	}

	public void changeKeyValue (string name, string keyButton)
	{
		for (i = 0; i < axes.Count; i++) {
			if (axes [i].Name == name) {
				axes [i].keyButton = keyButton;
				return;
			}
		}
	}
	//get the key button value for an input field, using the action of the button
	public string getButtonKey (string name)
	{
		for (i = 0; i < axes.Count; i++) {
			if (name == axes [i].Name) {
				return axes [i].keyButton;
			}
		}
		return "";
	}

	//if the input field has been pressed, call a coroutine, to avoid the input field get the mouse press as new value
	public void startEditingInput (GameObject button)
	{
		if (!editingInput) {
			StartCoroutine (startEditingInputCoroutine (button));
		}
	}
	//set the text of the input field to ... and start to check the keyboard press
	private IEnumerator startEditingInputCoroutine (GameObject button)
	{
		yield return null;
		currentEditButtonInput = button.GetComponent<editButtonInput> ();
		currentEditButtonInputPreviouseValue = currentEditButtonInput.actionKeyText.text;
		currentEditButtonInput.actionKeyText.text = "...";
		editingInput = true;
	}
	//any change done in the input field is undone
	public void cancelEditingInput ()
	{
		print (currentEditButtonInputPreviouseValue);
		editingInput = false;
		currentEditButtonInput.actionKeyText.text = currentEditButtonInputPreviouseValue;
	}

	public bool checkInputButton(string name, buttonType type){
		if (menus.useTouchControls) {
			if (getTouchButton (name, type)) {
				return true;
			}
		} else {
			if (getButton (name, type)) {
				return true;
			}
		}
		return false;
	}

	//function called in the script where pressing that button will make an action in the game, for example jump, crouch, shoot, etc...
	//every button sends its action and the type of pressing
	public bool getButton (string name, buttonType type)
	{
		//if the game is not paused, and the current control is the keyboard
		if (menusLocated && (!menus.pauseGame || name == "Pause") && !menus.useTouchControls) {
			foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode))) {
				switch (type) {
				//this key is for holding
				case buttonType.getKey:
					if (Input.GetKey (vKey)) {
						//check that the key pressed has being defined as an action
						for (i = 0; i < axes.Count; i++) {
							if (name == axes [i].Name && (vKey.ToString () == axes [i].keyButton || vKey.ToString ().Contains ("Button" + (int)axes [i].joystickButton)) && axes [i].actionEnabled) {
								//if the key pressed has an action and this is the type of pressing, return true
								//print (vKey.ToString()+"  "+name+"  "+axes[i].keyButton);
								return true;
							}
						}
					}
					break;
				case buttonType.getKeyDown:
					//this key is for press once
					if (Input.GetKeyDown (vKey)) {
						//print (vKey.ToString());
						//check that the key pressed has being defined as an action
						for (i = 0; i < axes.Count; i++) {
							if (name == axes [i].Name && (vKey.ToString () == axes [i].keyButton || vKey.ToString ().Contains ("Button" + (int)axes [i].joystickButton)) && axes [i].actionEnabled) {
								//if the key pressed has an action and this is the type of pressing, return true
								//print (vKey.ToString()+"  "+name+"  "+axes[i].keyButton);
								return true;
							}
						}
					}
					break;
				case buttonType.getKeyUp:
					//this key is for release 
					if (Input.GetKeyUp (vKey)) {
						//check that the key pressed has being defined as an action
						for (i = 0; i < axes.Count; i++) {
							if (name == axes [i].Name && (vKey.ToString () == axes [i].keyButton || vKey.ToString ().Contains ("Button" + (int)axes [i].joystickButton)) && axes [i].actionEnabled) {
								//if the key pressed has an action and this is the type of pressing, return true
								//print (vKey.ToString()+"  "+name+"  "+axes[i].keyButton);
								return true;
							}
						}
					}
					break;
				}
			}
			//check if the wheel of the mouse has been used, and in what direction
			if (Input.GetAxis ("Mouse ScrollWheel") > 0 || Input.GetAxis ("Mouse ScrollWheel") < 0) {
				float axisValue = Input.GetAxis ("Mouse ScrollWheel");
				for (i = 0; i < axes.Count; i++) {
					switch (type) {
					//wheel rotated up or down
					case buttonType.negMouseWheel:
						if (axisValue < 0) {
							return true;
						}
						break;
					case buttonType.posMouseWheel:
						if (axisValue > 0) {
							return true;
						}
						break;
					}
				}
			}
			if (Input.GetAxis ("Triggers") != 0) {
				float axisValue = Input.GetAxis ("Triggers");
				//print (axisValue);
				for (i = 0; i < axes.Count; i++) {
					if (name == axes [i].Name && axes [i].actionEnabled) {
						if (axisValue < 0) {
							if (axes [i].joystickButton == joystickButtons.RightTrigger && !usingRightTrigger) {
								usingRightTrigger = true;
								return true;
							}
						} else {
							if (axes [i].joystickButton == joystickButtons.LeftTrigger && !usingLeftTrigger) {
								usingLeftTrigger = true;
								return true;
							}
						}
					}
				}
			} else {
				usingRightTrigger = false;
				usingLeftTrigger = false;
			}
			if (Input.GetAxis ("DPad X") != 0) {
				float axisValue = Input.GetAxis ("DPad X");
				for (i = 0; i < axes.Count; i++) {
					if (name == axes [i].Name && axes [i].actionEnabled) {
						if (axisValue < 0) {
							if (axes [i].joystickButton == joystickButtons.LeftDPadX && !usingDPadX) {
								usingDPadX = true;
								return true;
							}
						} else {
							if (axes [i].joystickButton == joystickButtons.RightDPadX && !usingDPadX) {
								usingDPadX = true;
								return true;
							}
						}
					}
				}
			} else {
				usingDPadX = false;
			}
			if (Input.GetAxis ("DPad Y") != 0) {
				float axisValue = Input.GetAxis ("DPad Y");
				for (i = 0; i < axes.Count; i++) {
					if (name == axes [i].Name && axes [i].actionEnabled) {
						if (axisValue < 0) {
							if (axes [i].joystickButton == joystickButtons.BottomDPadY && !usingDPadY) {
								usingDPadY = true;
								return true;
							}
						} else {
							if (axes [i].joystickButton == joystickButtons.TopDPadY && !usingDPadY) {
								usingDPadY = true;
								return true;
							}
						}
					}
				}
			} else {
				usingDPadY = false;
			}
		}
		return false;
	}

	bool usingRightTrigger;
	bool usingLeftTrigger;
	bool usingDPadX;
	bool usingDPadY;
	//function called in the script where pressing that touch button will make an action in the game, for example jump, crouch, shoot, etc...
	//every button sends its action and the type of pressing, using the script touchButtonListener for that
	public bool getTouchButton (string name, buttonType type)
	{
		//if the game is not paused, and the current control is a touch device
		if (menusLocated && !menus.pauseGame && menus.useTouchControls) {
			int touchCount = Input.touchCount;
			if (!touchPlatform) {
				touchCount++;
			}
			for (int i = 0; i < touchCount; i++) {
				if (!touchPlatform) {
					currentTouch = touchJoystick.convertMouseIntoFinger ();
				} else {
					currentTouch = Input.GetTouch (i);
				}
				//check for a began touch
				if (currentTouch.phase == TouchPhase.Began) {
					if (type == buttonType.getKeyDown) {
						for (int k = 0; k < axes.Count; k++) {
							if (axes [k].touchButton) {
								if (name == axes [k].Name && axes [k].touchButton.pressedDown && axes [i].actionEnabled) {
									//if the button is pressed (OnPointerDown), return true
									//print ("getKeyDown");
									return true;
								}
							}
						}
					}
				}
				//check for a hold touch
				if (currentTouch.phase == TouchPhase.Stationary || currentTouch.phase == TouchPhase.Moved) {
					if (type == buttonType.getKey) {
						for (int k = 0; k < axes.Count; k++) {
							if (axes [k].touchButton) {
								if (name == axes [k].Name && axes [k].touchButton.pressed && axes [i].actionEnabled) {
									//if the button is pressed OnPointerDown, and is not released yet (OnPointerUp), return true
									//print ("getKey");
									return true;
								}
							}
						}
					}
				}
				//check for a release touch
				if (currentTouch.phase == TouchPhase.Ended) {
					if (type == buttonType.getKeyUp) {
						for (int k = 0; k < axes.Count; k++) {
							if (axes [k].touchButton) {
								if (name == axes [k].Name && axes [k].touchButton.pressedUp && axes [i].actionEnabled) {
									//if the button is released (OnPointerUp), return true
									//print ("getKeyUp");
									return true;
								}
							}
						}
					}
				}
			}
		}
		return false;
	}
	//change the current controls to keyboard or mobile
	public void setKeyboardControls (bool state)
	{
		if (!menus) {
			menus = GetComponent<menuPause> ();
		}
		menus.useTouchControls = !state;
		touchPanel.SetActive (!state);
		changeControlsType (!state);
		#if UNITY_EDITOR
		EditorUtility.SetDirty (menus);
		EditorUtility.SetDirty (GetComponent<inputManager> ());
		#endif
	}
	//set the current pause state of the game
	public void setPauseState (bool state)
	{
		gamePaused = state;
	}
	//add a new axe to the list
	public void addNewAxe ()
	{
		Axes newAxe = new Axes ();
		newAxe.Name = "New Button";
		axes.Add (newAxe);
	}

	[System.Serializable]
	public class Axes
	{
		public string Name;
		public string keyButton;
		public touchButtonListener touchButton;
		public joystickButtons joystickButton;
		public bool actionEnabled;
		public bool scaleTouchButtonInEditor;
		public bool sameTouchButtonWidhtHeight;

		//some constructors for a key input, incluing name, key button and touch button
		public Axes ()
		{
			Name = "";
			keyButton = "";
			actionEnabled = true;
		}

		public Axes (string n, string key, touchButtonListener tButton)
		{
			Name = n;
			keyButton = key;
			touchButton = tButton;
			actionEnabled = true;
		}

		public Axes (string n, string key)
		{
			Name = n;
			keyButton = key;
			actionEnabled = true;
		}
	}

	public enum joystickButtons
	{
		A = 0,
		B = 1,
		X = 2,
		Y = 3,
		LeftBumper = 4,
		RightBumper = 5,
		Back = 6,
		Start = 7,
		LeftStickClick = 8,
		RightStickClick = 9,
		LeftDPadX = 10,
		RightDPadX = 11,
		TopDPadY = 12,
		BottomDPadY = 13,
		LeftTrigger = 14,
		RightTrigger = 15,
		None = -1
	}
}