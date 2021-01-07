using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif
[System.Serializable]
//this script allows to edit the touch buttons to set their positions in run time with the mouse or finger in a touch screen, save those positions in a file, 
//and reload in the next execution, in case you want to load it
public class editControlPosition : MonoBehaviour
{
	public GameObject touchPanel;
	public bool loadButtonsPos = false;
	public bool useRelativePath;
	public string relativePath;
	public string saveFileName;
	bool enableButtonEdit = false;
	bool touching;
	bool grab;
	List<RectTransform> buttons = new List<RectTransform> ();
	List<Vector2> buttonsPos = new List<Vector2> ();
	List<Vector2> buttonPosDefault = new List<Vector2> ();
	RectTransform buttonToMove;
	int i = 0;
	readonly List<RaycastResult> captureRaycastResults = new List<RaycastResult> ();
	Touch currentTouch;
	bool touchPlatform;
	string currentSaveDataPath;

	void Start ()
	{
		touchPlatform = touchJoystick.checkTouchPlatform ();
		currentSaveDataPath = getDataPath ();
	}

	void Update ()
	{
		if (enableButtonEdit) {
			//check the mouse position in the screen if we are in the editor, or the finger position in a touch device
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
				if (currentTouch.phase == TouchPhase.Began) {
					//if the finger/mouse position is above a button, it is selected to translate
					captureRaycastResults.Clear ();
					PointerEventData p = new PointerEventData (EventSystem.current);
					p.position = currentTouch.position;
					p.clickCount = i;
					p.dragging = false;
					EventSystem.current.RaycastAll (p, captureRaycastResults);
					foreach (RaycastResult r in captureRaycastResults) {
						if (buttons.Contains (r.gameObject.GetComponent<RectTransform> ())) {
							grab = true;
							buttonToMove = r.gameObject.GetComponent<RectTransform> ();
							touching = true;
						}
					}
				}
				//the button follows the mouse/finger position
				if ((currentTouch.phase == TouchPhase.Stationary || currentTouch.phase == TouchPhase.Moved) && grab) {
					if (grab && buttonToMove != null) {
						buttonToMove.GetComponent<RectTransform> ().position = new Vector2 (currentTouch.position.x, currentTouch.position.y);
					}
				}
				//release the button 
				if (currentTouch.phase == TouchPhase.Ended && touching) {
					touching = false;
					if (grab) {
						grab = false;
						buttonToMove = null;
					}
				}
			}
		}
	}

	public string getDataPath ()
	{
		string dataPath = "";
		if (useRelativePath) {
			if (!Directory.Exists (relativePath)) {
				Directory.CreateDirectory (relativePath);
			}
			dataPath = relativePath + "/";
		} else {
			dataPath = Application.persistentDataPath + "/";
		}
		return dataPath;
	}

	public void getTouchButtons (bool state)
	{
		if (touchPanel) {
			//save all touch buttons in a list and their positions
			touchPanel.SetActive (true);
			Component[] components = touchPanel.transform.GetComponentsInChildren (typeof(RectTransform));
			foreach (Component c in components) {
				if (c.gameObject != touchPanel) {
					buttons.Add (c.gameObject.GetComponent<RectTransform> ());
					buttonPosDefault.Add (c.gameObject.GetComponent<RectTransform> ().position);
				}
			}
			touchPanel.SetActive (state);
			//load or not the previous positions of the buttons, saved in another execution
			if (loadButtonsPos) {
				Load ();
				loadButtonsPos = false;
			}
			for (i = 0; i < buttons.Count; i++) {
				buttonsPos.Add (buttons [i].position);
			}
		}
	}
	//after edit the buttons, their positions are saved
	public void activateEdit ()
	{
		enableButtonEdit = !enableButtonEdit;
		if (!enableButtonEdit) {
			buttonsPos.Clear ();
			for (i = 0; i < buttons.Count; i++) {
				buttonsPos.Add (buttons [i].transform.position);
			}
			Save ();
		} else {
			changeButtonsState (false);
		}
	}
	//if we cancel the edition, the buttons back to their previous position, if any of them has been moved
	public void disableEdit ()
	{
		if (enableButtonEdit) {
			enableButtonEdit = false;
			for (i = 0; i < buttons.Count; i++) {
				buttons [i].transform.position = buttonsPos [i];
			}
		}
	}
	//set the buttons with their default position
	public void changeToDefault ()
	{
		for (i = 0; i < buttons.Count; i++) {
			buttons [i].transform.position = buttonPosDefault [i];
		}
		Save ();
	}
	//disable and enable the pointer click components in the buttons, to avoid activate them while the game is paused
	public void changeButtonsState (bool state)
	{
		for (i = 0; i < buttons.Count; i++) {
			if (buttons [i].GetComponent<EventTrigger> ()) {
				buttons [i].GetComponent<EventTrigger> ().enabled = state;
			}
			if (buttons [i].GetComponent<touchButtonListener> ()) {
				buttons [i].GetComponent<touchButtonListener> ().enabled = state;
			}
		}
	}
	//save in a file the positions of the button, vector2 are not serializable if I am not wrong, so every axis is save in a list of float
	public void Save ()
	{
		savedButtons sB = new savedButtons ();
		for (i = 0; i < buttons.Count; i++) {
			sB.saveButtonsPosX.Add (buttons [i].position.x);
			sB.saveButtonsPosY.Add (buttons [i].position.y);
		}
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Create (currentSaveDataPath + saveFileName + ".txt");
		bf.Serialize (file, sB.saveButtonsPosX);
		bf.Serialize (file, sB.saveButtonsPosY);
		file.Close ();
		buttonsPos.Clear ();
		for (i = 0; i < buttons.Count; i++) {
			buttonsPos.Add (new Vector2 (sB.saveButtonsPosX [i], sB.saveButtonsPosY [i]));
		}
	}
	//load the positions of the buttons reading the file if it exists
	public void Load ()
	{
		savedButtons sB = new savedButtons ();
		if (File.Exists (currentSaveDataPath + saveFileName + ".txt")) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (currentSaveDataPath + saveFileName + ".txt", FileMode.Open);
			sB.saveButtonsPosX = (List<float>)bf.Deserialize (file);
			sB.saveButtonsPosY = (List<float>)bf.Deserialize (file);
			file.Close ();
		}
		//also check the lenght are equal in both list, to avoid exceptions
		if (sB.saveButtonsPosX.Count == buttons.Count) {
			for (i = 0; i < buttons.Count; i++) {
				buttons [i].position = new Vector2 (sB.saveButtonsPosX [i], sB.saveButtonsPosY [i]);
			}
		}
	}
}
//the positions of the buttons need to be serializable to save them in a file
[Serializable]
class savedButtons
{
	public List<float> saveButtonsPosX = new List<float> ();
	public List<float> saveButtonsPosY = new List<float> ();
}