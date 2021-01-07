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

public class accessTerminal : MonoBehaviour
{
	public RectTransform pointer;
	public GameObject keys;
	public Text currectCode;
	public Text stateText;
	public string code;
	public LayerMask layer;
	public GameObject objectToUnlock;
	public string unlockFunctionName;
	public Color unlockedColor;
	public Image wallPaper;
	public AudioClip wrongPassSound;
	public AudioClip corretPassSound;
	public AudioClip keyPressSound;
	public GameObject hackPanel;
	public GameObject hackActiveButton;
	public bool useMoveCameraToDevice;
	List<Image> keysList = new List<Image> ();
	readonly List<RaycastResult> captureRaycastResults = new List<RaycastResult> ();
	int totalKeysPressed = 0;
	int length;
	bool unlocked;
	bool enter;
	bool changedColor;
	bool checkPressedButton;
	bool touchPlatform;
	RaycastHit hit;
	GameObject currentCaptured;
	Touch currentTouch;
	bool cameraMoved;
	AudioSource audioSource;
	moveCameraToDevice cameraMovementManager;

	void Start ()
	{
		//get all the keys inside the keys gameobject, checking the name of every object, comparing if it is a number from 0 to 9
		Component[] components = keys.GetComponentsInChildren (typeof(Image));
		foreach (Component c in components) {
			int n;
			if (int.TryParse (c.name.ToString (), out n)) {
				keysList.Add (c.gameObject.GetComponent<Image> ());
			}
		}
		//set the current code to 0 according to the length of the real code
		currectCode.text = "";
		length = code.Length;
		for (int i = 0; i < length; i++) {
			currectCode.text += "0";
		}
		touchPlatform = touchJoystick.checkTouchPlatform ();
		if (hackPanel) {
			hackActiveButton.SetActive (true);
			if (useMoveCameraToDevice) {
				hackPanel.GetComponent<hackTerminal> ().hasMoveCamerToDevice ();
			}
		}
		audioSource = GetComponent<AudioSource> ();
		cameraMovementManager = GetComponent<moveCameraToDevice> ();
	}

	void Update ()
	{
		//if the terminal still locked, and the player is using it
		if (!unlocked && enter && (!useMoveCameraToDevice || (useMoveCameraToDevice && cameraMoved))) {
			//use the center of the camera as mouse, checking also the touch input
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
				//get a list with all the objects under the center of the screen of the finger tap
				captureRaycastResults.Clear ();
				PointerEventData p = new PointerEventData (EventSystem.current);
				p.position = currentTouch.position;
				p.clickCount = i;
				p.dragging = false;
				EventSystem.current.RaycastAll (p, captureRaycastResults);
				foreach (RaycastResult r in captureRaycastResults) {
					currentCaptured = r.gameObject;
					//if the center of the camera is looking at the screen, move the cursor image inside it
					if (currentCaptured.name == "terminal") {
						if (Physics.Raycast (Camera.main.ScreenPointToRay (currentTouch.position), out hit, Mathf.Infinity, layer)) {
							if (!hit.collider.isTrigger) {
								pointer.GetComponent<RectTransform> ().position = hit.point + hit.normal * 0.03f;
							}
						}
					}
					//check the current number key pressed with the finger
					if (currentTouch.phase == TouchPhase.Began) {
						checkButton (currentCaptured);
					}
					//check the current number key preesed with the interaction button in the keyboard
					if (checkPressedButton) {
						checkButton (currentCaptured);
					}
				}
				//disable the boolean
				if (checkPressedButton) {
					checkPressedButton = false;
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
			}
		}
	}
	//this function is called when the interaction button in the keyboard is pressed, so in pc, the code is written by aiming the center of the camera to
	//every number and pressing the interaction button. In touch devices, the code is written by tapping with the finger every key number directly
	void activateDevice ()
	{
		if (useMoveCameraToDevice) {
			cameraMoved = !cameraMoved;
			cameraMovementManager.moveCamera (cameraMoved);
		} else {
			checkPressedButton = true;
		}
	}
	//the currentCaptured is checked, to write the value of the number key in the screen device
	void checkButton (GameObject button)
	{
		if (button.GetComponent<Image> ()) {
			//check if the currentCaptured is a key number
			if (keysList.Contains (button.GetComponent<Image> ())) {
				//reset the code in the screen
				if (totalKeysPressed == 0) {
					currectCode.text = "";
				}	
				//add the current key number pressed to the code
				currectCode.text += button.GetComponent<Image> ().name;
				totalKeysPressed++;
				//play the key press sound
				audioSource.PlayOneShot (keyPressSound);
				//if the player has pressed the an amount of key numbers equal to the lenght of the code, check if it is correct
				if (totalKeysPressed == length) {
					//if it is equal, then call the object to unlock, play the corret pass sound, and disable this terminal
					if (currectCode.text == code) {
						audioSource.PlayOneShot (corretPassSound);
						stateText.text = "Unlocked";
						unlocked = true;
						if (objectToUnlock) {
							objectToUnlock.SendMessage (unlockFunctionName);
						}
						if (hackPanel) {
							hackPanel.GetComponent<hackTerminal> ().moveHackTerminal (false);
						}
						if (useMoveCameraToDevice) {
							cameraMoved = !cameraMoved;
							cameraMovementManager.moveCamera (cameraMoved);
						}
					}
					//else, reset the terminal, and try again
					else {
						audioSource.PlayOneShot (wrongPassSound);
						totalKeysPressed = 0;
					}
				}
			} else {
				if (hackActiveButton) {
					if (button == hackActiveButton) {
						hackPanel.GetComponent<hackTerminal> ().activeHack ();
					}
				}
			}
		}
	}
	//check when the player enters or exits of the trigger in the terminal
	void OnTriggerEnter (Collider col)
	{
		if (col.GetComponent<Collider> ().tag == "Player") {
			enter = true;
		}
	}

	void OnTriggerExit (Collider col)
	{
		if (col.GetComponent<Collider> ().tag == "Player") {
			enter = false;
		}
	}
}