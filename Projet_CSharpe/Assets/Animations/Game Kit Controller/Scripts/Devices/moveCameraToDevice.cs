using UnityEngine;
using System.Collections;

public class moveCameraToDevice : MonoBehaviour
{
	public GameObject cameraPosition;
	public bool smoothCameraMovement = true;
	public bool secondMoveCameraToDevice;
	GameObject character;
	GameObject player;
	GameObject cameraParent;
	Vector3 previousCameraPos;
	Vector3 finalCameraPos;
	Quaternion previousCameraRot;
	Quaternion finalCameraRot;
	Coroutine cameraState;
	bool deviceEnabled;
	Camera mainCamera;
	menuPause menuPauseManager;

	//this function was placed in computer device, but now it can be added to any type of device when the player is using it,
	//to move the camera position and rotation in front of the device and place it again in its regular place when the player stops using the device
	void Start ()
	{
		mainCamera = Camera.main;
		player = GameObject.Find ("Player Controller");
		character = GameObject.Find ("Character");
		menuPauseManager = character.GetComponent<menuPause> ();
		finalCameraRot = Quaternion.identity;
	}
	//activate the device
	public void moveCamera (bool state)
	{
		deviceEnabled = state;
		Camera.main.GetComponent<headBob> ().enabled = !deviceEnabled;
		character.GetComponent<mouseCursorController> ().showOrHideCursor (deviceEnabled);
		//if the player is using the computer, disable the player controller, the camera, and set the parent of the camera inside the computer, 
		//to move to its view position
		if (deviceEnabled) {
			if (player.GetComponent<otherPowers> ().running) {
				player.GetComponent<otherPowers> ().stopRun ();
			}
			if (!secondMoveCameraToDevice) {
				//make the mouse cursor visible according to the action of the player
				menuPauseManager.usingDeviceState (deviceEnabled);
				player.GetComponent<playerController> ().changeScriptState (!deviceEnabled);
				player.transform.GetChild (0).gameObject.SetActive (!deviceEnabled);
				menuPauseManager.showOrHideCursor (deviceEnabled);
				menuPauseManager.changeCameraState (!deviceEnabled);
			}
			previousCameraPos = mainCamera.transform.localPosition;
			cameraParent = mainCamera.transform.parent.gameObject;
			mainCamera.transform.parent = cameraPosition.transform;
			finalCameraPos = Vector3.zero;
		} else {
			//if the player disconnect the computer, then enabled of its components and set the camera to its previous position inside the player
			if (!secondMoveCameraToDevice) {
				//make the mouse cursor visible according to the action of the player
				menuPauseManager.usingDeviceState (deviceEnabled);
				player.GetComponent<playerController> ().changeScriptState (!deviceEnabled);
				player.transform.GetChild (0).gameObject.SetActive (!deviceEnabled);
				menuPauseManager.showOrHideCursor (deviceEnabled);
				menuPauseManager.changeCameraState (!deviceEnabled);
			}
			finalCameraPos = previousCameraPos;
			mainCamera.transform.parent = cameraParent.transform;
		}
		if (smoothCameraMovement) {
			//stop the coroutine to translate the camera and call it again
			if (cameraState != null) {
				StopCoroutine (cameraState);
			}
			cameraState = StartCoroutine (adjustCamera ());
		} else {
			mainCamera.transform.localRotation = finalCameraRot;
			mainCamera.transform.localPosition = finalCameraPos;
		}
	}
	//move the camera from its position in player camera to a fix position for a proper looking of the computer and vice versa
	IEnumerator adjustCamera ()
	{
		float i = 0;
		//store the current rotation of the camera
		Quaternion currentQ = mainCamera.transform.localRotation;
		//store the current position of the camera
		Vector3 currentPos = mainCamera.transform.localPosition;
		//translate position and rotation camera
		while (i < 1) {
			i += Time.deltaTime * 2;
			mainCamera.transform.localRotation = Quaternion.Lerp (currentQ, finalCameraRot, i);
			mainCamera.transform.localPosition = Vector3.Lerp (currentPos, finalCameraPos, i);
			yield return null;
		}
//		if (!deviceEnabled) {
//			menuPauseManager.changeCameraState (!deviceEnabled);
//		}
	}

	public void hasSecondMoveCameraToDevice ()
	{
		secondMoveCameraToDevice = true;
	}
}