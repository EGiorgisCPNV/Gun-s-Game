using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Reflection;

public class computerDevice : MonoBehaviour
{
	public bool locked;
	public GameObject keyboard;
	public Text currentCode;
	public Text stateText;
	public string code;
	public GameObject objectToUnlock;
	public string unlockFunctionName;
	public GameObject computerLockedContent;
	public GameObject computerUnlockedContent;
	public Color unlockedColor;
	public Image wallPaper;
	public AudioClip wrongPassSound;
	public AudioClip corretPassSound;
	public AudioClip keyPressSound;
	List<Image> keysList = new List<Image> ();
	readonly List<RaycastResult> captureRaycastResults = new List<RaycastResult> ();
	int totalKeysPressed = 0;
	bool changeScreen;
	bool unlocked;
	bool enter;
	bool changedColor;
	bool check;
	bool touchPlatform;
	GameObject currentCaptured;
	GameObject player;
	Touch currentTouch;
	AudioSource audioSource;

	void Start ()
	{
		//get all the keys button in the keyboard and store them in a list
		if (keyboard) {
			Component[] components = keyboard.GetComponentsInChildren (typeof(Image));
			foreach (Component c in components) {
				keysList.Add (c.gameObject.GetComponent<Image> ());
			}
		}
		touchPlatform = touchJoystick.checkTouchPlatform ();
		if (!locked) {
			if (computerLockedContent) {
				computerLockedContent.SetActive (false);
			}
			if (computerUnlockedContent) {
				computerUnlockedContent.SetActive (true);
			}
		}
		audioSource = GetComponent<AudioSource> ();
	}

	void Update ()
	{
		//if the computer is locked and the player is inside its trigger 
		if (!unlocked && enter && locked) {
			//get all the input touchs, including the mouse
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
				//get a list with all the objects under mouse or the finger tap
				captureRaycastResults.Clear ();
				PointerEventData p = new PointerEventData (EventSystem.current);
				p.position = currentTouch.position;
				p.clickCount = i;
				p.dragging = false;
				EventSystem.current.RaycastAll (p, captureRaycastResults);
				foreach (RaycastResult r in captureRaycastResults) {
					currentCaptured = r.gameObject;
					//check the current key pressed with the finger
					if (currentTouch.phase == TouchPhase.Began) {
						checkButton (currentCaptured);
					}
				}
			}
		}
		//if the device is unlocked, change the color of the interface for the unlocked color
		if (unlocked) {
			if (!changedColor) {
				wallPaper.color = Vector4.MoveTowards (wallPaper.color, unlockedColor, Time.deltaTime * 3);
				stateText.color = Vector4.MoveTowards (stateText.color, unlockedColor, Time.deltaTime * 3);
				if (wallPaper.color == unlockedColor && stateText.color == unlockedColor) {
					changedColor = true;
				}
			} else {
				//change the password screen for the unlocked screen
				if (changeScreen) {
					computerLockedContent.SetActive (false);
					computerUnlockedContent.SetActive (true);
					changeScreen = false;
				}
			}
		}

	}
	//activate the device
	public void activateDevice ()
	{
		check = !check;
		GetComponent<moveCameraToDevice> ().moveCamera (check);
	}
	//the currentCaptured is checked, to write the value of the key in the screen device
	void checkButton (GameObject button)
	{
		if (button.GetComponent<Image> ()) {
			//check if the currentCaptured is a key number
			if (keysList.Contains (button.GetComponent<Image> ())) {
				bool checkPass = false;
				//reset the password in the screen
				if (totalKeysPressed == 0) {
					currentCode.text = "";
				}	
				//add the an space
				if (button.name == "space") {
					currentCode.text += " ";
				}
				//delete the last character
				else if (button.name == "delete") {
					if (currentCode.text.Length > 0) {
						currentCode.text = currentCode.GetComponent<Text> ().text.Remove (currentCode.GetComponent<Text> ().text.Length - 1);
					}
				}
				//check the current word added
				else if (button.name == "enter") {
					checkPass = true;
				}
				//add the current key pressed to the password
				else {
					currentCode.text += button.GetComponent<Image> ().name;
				}
				totalKeysPressed++;
				//play the key press sound
				audioSource.PlayOneShot (keyPressSound);
				//the enter key has been pressed, so check if the current text written is the correct password
				if (checkPass) {
					if (currentCode.text == code) {
						enableAccessToCompturer ();
					} 
					//else, reset the terminal, and try again
					else {
						audioSource.PlayOneShot (wrongPassSound);
						currentCode.text = "Password";
						totalKeysPressed = 0;
					}
				}
			}
		}
	}
	//if the object to unlock is this device, change the screen
	public void unlockComputer ()
	{
		changeScreen = true;
	}

	public void enableAccessToCompturer(){
		//if it is equal, then call the object to unlock and play the corret pass sound
		audioSource.PlayOneShot (corretPassSound);
		stateText.text = "Unlocked";
		unlocked = true;
		if (objectToUnlock) {
			//the object to unlock can be also this terminal, to see more content inside it
			objectToUnlock.SendMessage (unlockFunctionName);
		}
	}
	public void unlockComputerWithUsb(){
		unlockComputer ();
		enableAccessToCompturer ();
	}
	//check when the player enters or exits of the trigger in the terminal
	void OnTriggerEnter (Collider col)
	{
		if (col.GetComponent<Collider> ().tag == "Player") {
			enter = true;
			//get the player gameObject
			if (!player) {
				player = col.gameObject;
			}
		}
	}

	void OnTriggerExit (Collider col)
	{
		if (col.GetComponent<Collider> ().tag == "Player") {
			enter = false;
		}
	}
}