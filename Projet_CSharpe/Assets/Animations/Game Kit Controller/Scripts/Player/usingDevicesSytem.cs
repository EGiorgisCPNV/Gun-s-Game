using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class usingDevicesSytem : MonoBehaviour {
	public bool canUseDevices;
	public GameObject touchButton;
	public GameObject iconButton;
	public Text actionText;
	public Text keyText;
	public Text objectNameText;
	public string useDeviceFunctionName = "activateDevice";
	[HideInInspector] public bool driving;
	GameObject objectToUse;
	GameObject currentVehicle;
	List<GameObject> deviceList = new List<GameObject> ();
	inputManager input;
	Touch currentTouch;
	bool showIconButton=true;
	ragdollActivator ragdollManager;
	int i;
	float screenOffset;
	deviceStringAction deviceStringManager;

	void Start () {
		input = transform.parent.GetComponent<inputManager> ();
		ragdollManager = GetComponent<ragdollActivator> ();
	}
	void Update () {
		if (input.checkInputButton ("Activate Devices", inputManager.buttonType.getKeyDown)) {
			useDevice ();
		}
		//set the icon button above the device to use, just to indicate to the player that he can activate a device by pressing T
		if (deviceList.Count>0) {
			int index = getclosestDevice ();
			if(showIconButton && index != -1) {
				Vector3 screenPoint = Camera.main.WorldToScreenPoint (deviceList [index].transform.position + transform.up * screenOffset);
				if (screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < Screen.width && screenPoint.y > 0 && screenPoint.y < Screen.height) {
					iconButton.transform.position = screenPoint;
					iconButton.SetActive (true);
				} else {
					iconButton.SetActive (false);
				}
			}
		}
	}
	public int getclosestDevice(){
		int index = -1;
		float minDistance = 100;
		for (i = 0; i < deviceList.Count; i++) {
			if (deviceList [i]) {
				if (Vector3.Distance (deviceList [i].transform.position, transform.position) < minDistance) {
					minDistance = Vector3.Distance (deviceList [i].transform.position, transform.position);
					index = i;
				}
			} else {
				deviceList.RemoveAt (i);
			}
		}
		if (index != -1) {
			if (objectToUse != deviceList [index]) {
				objectToUse = deviceList [index];
				//get the action made by the current device
				string deviceAction = "";
				deviceStringManager = objectToUse.GetComponent<deviceStringAction> ();
				deviceAction = deviceStringManager.deviceAction;
				//show the icon in the hud of the screen according to the deviceStringAction component
				if (deviceStringManager.showIcon && deviceAction.Length > 0) {
					showIconButton = true;
					iconButton.SetActive (true);
				} else {
					iconButton.SetActive (false);
					showIconButton = false;
				}
				//enable the interection button in the touch screen
				if (deviceStringManager.showTouchIconButton) {
					touchButton.SetActive (true);
				} else {
					touchButton.SetActive (false);
				}
				//set the key text in the icon with the current action
				keyText.text= "[" + input.getButtonKey ("Activate Devices") + "]";
				actionText.text = deviceAction;
				objectNameText.text = deviceStringManager.deviceName;
				if(objectToUse.GetComponentInChildren<inventoryObject>()){
					if (objectToUse.GetComponentInChildren<inventoryObject> ().inventoryObjectInfo.amount > 1) {
						objectNameText.text += " x " + objectToUse.GetComponentInChildren<inventoryObject> ().inventoryObjectInfo.amount.ToString ();
					}
				}
				if(objectToUse.GetComponentInParent<pickUpObject>()){
					if (objectToUse.GetComponentInParent<pickUpObject>().amount>1) {
						objectNameText.text += " x " + objectToUse.GetComponentInParent<pickUpObject>().amount.ToString ();
					}
				}
				if (objectToUse.GetComponent<vehicleHUDManager> ()) {
					//if the player is driving, and he is inside the trigger of other vehicle, disable the icon to use the other vehicle
					if (objectToUse.GetComponent<vehicleHUDManager> ().driving) {
						iconButton.SetActive (false);
						showIconButton = false;
					}
				}
				screenOffset = deviceStringManager.actionOffset;
			}
		}
		return index;
	}

	//check if the player enters or exits the trigger of a device 
	public void OnTriggerEnter(Collider col){
		//if the player is driving, he can't use any other device
		if (!canUseDevices || driving) {
			return;
		}
		GameObject usableObjectFound = col.gameObject;
		if (col.GetComponent<Collider> ().tag == "device" || col.GetComponent<Collider> ().tag == "inventory") {
			if (!deviceList.Contains (usableObjectFound)) {
				deviceList.Add (usableObjectFound);
			}
		}
	}
	public void OnTriggerExit(Collider col){
		bool isDevice = false;
		if (col.GetComponent<Collider> ().tag == "device" || col.GetComponent<Collider> ().tag == "inventory") {
			isDevice = true;
			//when the player exits from the trigger of a device, if he is not driving, set the device to null
			//else the player is driving, so the current device is that vehicle, so the device can't be changed
			if (deviceList.Contains (col.gameObject)) {
				if (driving) {
					if (col.gameObject != currentVehicle) {
						deviceList.Remove (col.gameObject);
					}
				} else {
					deviceList.Remove (col.gameObject);
				}
			}
		}
		if ((isDevice && !driving)) {
			touchButton.SetActive (false);
			iconButton.SetActive (false);
		}
		if (deviceList.Count == 0) {
			objectToUse = null;
			showIconButton = true;
		}
	}
	//call the device action
	public void useDevice(){
		if (!ragdollManager.canMove) {
			return;
		}
		GameObject usableObjectFound = objectToUse;
		if (usableObjectFound && canUseDevices) {
			if (usableObjectFound.GetComponent<useInventoryObject> ()) {
				if (usableObjectFound.GetComponent<useInventoryObject> ().useInventoryType == useInventoryObject.useInventoryObjectType.button
				    && !usableObjectFound.GetComponent<useInventoryObject> ().objectUsed) {
					GetComponent<inventoryManager> ().useCurrentObject ();
					return;
				}
			}
			usableObjectFound.SendMessage (useDeviceFunctionName,SendMessageOptions.DontRequireReceiver);
			if (!usableObjectFound) {
				return;
			}
			//if the device is a turret or a chest, disable the icon
			if (usableObjectFound.GetComponent<deviceStringAction> ()) {
				deviceStringManager = usableObjectFound.GetComponent<deviceStringAction> ();
				if (deviceStringManager.disableIconOnPress) {
					OnTriggerExit (usableObjectFound.GetComponent<Collider> ());
					return;
				}
				if (deviceStringManager.hideIconOnPress) {
					iconButton.SetActive (false);
					showIconButton = false;
				}
			}
			if (usableObjectFound.GetComponent<vehicleHUDManager> ()) {
				if (usableObjectFound.GetComponent<vehicleHUDManager> ().driving) {
					iconButton.SetActive (false);
					showIconButton = false;
					driving = true;
					currentVehicle = usableObjectFound;
				}
				if (!usableObjectFound.GetComponent<vehicleHUDManager> ().driving) {
					iconButton.SetActive (true);
					showIconButton = true;
					driving = false;
					currentVehicle = null;
				}
			}
		}
	}
	//disable the icon showed when the player is inside a device's trigger
	public void disableIcon(){
		iconButton.SetActive(false);
		touchButton.SetActive(false);
		showIconButton = true;
		driving = false;
		objectToUse = null;
	}
	public void hideIconButton(){
		showIconButton = false;
		iconButton.SetActive (false);
	}
	public void removeVehicleFromList(){
		if (currentVehicle) {
			deviceList.Remove (currentVehicle);
		}
	}
	public void clearDeviceList(){
		deviceList.Clear ();
	}
	public void checkDeviceName(){
		if (objectToUse) {
			if (objectToUse.GetComponent<deviceStringAction> ()) {
				deviceStringManager = objectToUse.GetComponent<deviceStringAction> ();
				string deviceAction = "";
				deviceAction = deviceStringManager.deviceAction;
				keyText.text= "[" + input.getButtonKey ("Activate Devices") + "]";
				actionText.text = deviceAction;
				objectNameText.text = deviceStringManager.deviceName;
				screenOffset = deviceStringManager.actionOffset;
			}
		}
	}
}