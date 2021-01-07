using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class scannerSystem : MonoBehaviour
{
	public bool scannerSystemEnabled;
	public GameObject scannerHUD;
	public GameObject scanIcon;
	public GameObject scannerCamera;
	public Slider slider;
	public Text objectName;
	public Text objectInfo;
	public Text scanStatus;
	public float scanSpeed;
	public LayerMask layer;
	[HideInInspector] public bool activate;
	bool viewStatus;
	float fovIn = 1;
	float fovOut;
	float fovValue;
	float fovZoom = 4;
	GameObject scannedObject;
	bool zoomIn;
	bool lookingObject;
	RaycastHit hit;
	inputManager input;
	menuPause pauseManager;
	Animation iconAnimation;
	RectTransform scanIconRect;
	Camera scannerMainCamera;

	void Start ()
	{
		//get the input manager and the menu pause manager
		input = transform.parent.GetComponent<inputManager> ();
		pauseManager = transform.parent.GetComponent<menuPause> ();
		//get the field of view of the camera
		fovOut = scannerCamera.GetComponent<Camera> ().fieldOfView;
		fovValue = fovOut;
		iconAnimation = scanIcon.GetComponent<Animation> ();
		scanIconRect = scanIcon.GetComponent<RectTransform> ();
		scannerMainCamera = scannerCamera.GetComponent<Camera> ();
	}

	void Update ()
	{
		//if the scan button is pressed
		if (input.checkInputButton ("Scan", inputManager.buttonType.getKeyUp)) {
			//activate the scanner mode
			enableScanner ();
			//if the key button is released, reset the info of the scanner
			if (scannedObject) {
				if (slider.value == slider.maxValue || slider.value != slider.maxValue) {
					reset ();
				}
			}
		}
		//if the scan button is being holding
		if (input.checkInputButton ("Scan", inputManager.buttonType.getKey)) {
			//if there is a scannedObject detected
			if (scannedObject) {
				//check if the info of the object has been already scanned
				if (!scannedObject.GetComponent<scanElementInfo> ().dataObject.read) {
					//in that case, scan the object
					scanStatus.text = "SCANNING...";
					//while the key is held, increase the slider value
					slider.value += Time.deltaTime * scanSpeed;
					//when the slider reachs its max value
					if (slider.value == slider.maxValue) {
						//set the object to already scanned
						scannedObject.GetComponent<scanElementInfo> ().dataObject.read = true;
						//get the info of the object
						objectChecked ();
					}
				}
				//if the object has been already scanned
				else {
					//get the info of the object
					objectChecked ();
				}
			}
		}
		//if there is a scanned object, make the scan icon follow this object in the screen
		if (scannedObject) {
			Vector3 screenPoint = Camera.main.WorldToScreenPoint (scannedObject.transform.position);
			//if the target is visible in the screnn, set the icon position
			if (screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < Screen.width && screenPoint.y > 0 && screenPoint.y < Screen.height) {
				scanIcon.transform.position = Vector3.MoveTowards (scanIcon.transform.position, screenPoint, Time.deltaTime * 500);
				if (!scanIcon.activeSelf) {
					//play the scan icon animation to signal the scannable object
					scanIcon.SetActive (true);
					iconAnimation ["scannerTarget"].speed = -1; 
					iconAnimation ["scannerTarget"].time = iconAnimation ["scannerTarget"].length;
					iconAnimation.Play ("scannerTarget");
				}
			} 
			//if the object is off screen, disable the scan icon in the screen
			else {
				scanIconRect.anchoredPosition = Vector2.zero;
				scanIcon.SetActive (false);
				reset ();
			}
		}
		//if the scan mode is enabled, launch a ray from the center of the screen in forward direction, searching a scannable object
		if (activate) {
			if (Physics.Raycast (Camera.main.transform.position, Camera.main.transform.forward, out hit, Mathf.Infinity, layer)) {
				//scannble object detected
				if (hit.collider.GetComponent<scanElementInfo> ()) {
					//if it the first scannable object found, set as scannedObject
					if (!scannedObject) {
						scannedObject = hit.collider.gameObject;
					}
					//if there was already another scannable object different from the current found, change it
					else if (scannedObject != hit.collider.gameObject) {
						scannedObject = hit.collider.gameObject;
					}
					lookingObject = true;
				}
				//nothing found
				else {
					lookingObject = false;
				}
			}
		}
		//if the small screen in the center is enabled, change the fov to the scanner mode, checking also if the player use the zoom mode
		if (scannerCamera.activeSelf) {
			if (scannerMainCamera.fieldOfView != fovValue) {
				scannerMainCamera.fieldOfView = Mathf.MoveTowards (scannerMainCamera.fieldOfView, fovValue, Time.deltaTime * 20);
			}
			if (scannerMainCamera.fieldOfView == fovOut) {
				scannerCamera.SetActive (false);
			}
		}
	}
	//get the info from the scannable object, name and description
	void objectChecked ()
	{
		slider.value = slider.maxValue;
		objectInfo.text = scannedObject.GetComponent<scanElementInfo> ().dataObject.info;
		objectName.text = scannedObject.GetComponent<scanElementInfo> ().dataObject.name;
		scanStatus.text = "SCAN COMPLETED";
	}
	//reset the info of the scanner
	void reset ()
	{
		slider.value = 0;
		scanStatus.text = "SCAN VISOR ACTIVE";
		objectInfo.text = "...";
		objectName.text = "";
		scannedObject = null;
	}
	//enable of disable the scanner according to the situation
	public void enableScanner ()
	{
		//if the player is not in aim mode, or the scanner mode is not already enabled, or using a device and the scanner mode is active in the feature manager
		if (!GetComponent<playerController> ().aiming && !lookingObject && !pauseManager.usingDevice && scannerSystemEnabled) {
			//change its state
			activate = !activate;
			bool value = activate;
			scannerHUD.SetActive (value);
			//if the scanner mode is enabled, check if the player is in first person mode
			if (value) {
				scannerCamera.SetActive (value);
				viewStatus = GetComponent<changeGravity> ().settings.firstPersonView;
			}
			//if the player is not in first person mode, change it to that view
			if (!viewStatus) {
				GetComponent<otherPowers> ().deactivateAimMode ();
				GetComponent<changeGravity> ().changeCameraView ();
			}
			//change the fov of the scanner camera
			fovIn *= (-1);
			fovValue += fovIn * 5;
			reset ();
			scanIcon.SetActive (false);
			//check if the player set the zoom mode, so if the scanned mode is enabled or disabled the camera fov is correctly changed
			if (zoomIn) {
				if (!value) {
					changeZoom (0);
					//the scanner mode is disabled when the zoom was enabled
				} else {
					changeZoom (-1);
					//the scanner mode is enabled when the zoom was enabled
				}
			}
		}
	}
	//disable the scanner from other script
	public void disableScanner ()
	{
		activate = false;
		bool value = activate;
		scannerHUD.SetActive (value);
		fovIn *= (-1);
		fovValue += fovIn * 5;
		reset ();
		scanIcon.SetActive (false);
	}
	//change the zoom in the scanner camera if the player use the zoom
	public void changeZoom (float value)
	{
		//decrease the fov
		if (value < 0) {
			//zoom enabled
			zoomIn = true;
			fovValue = fovZoom;
		}
		//increase the fov
		else {
			if (value > 0) {
				zoomIn = false;
			}
			if (activate) {
				fovValue = fovOut - 5;
				//zoom disabled when the scanner mode was enabled
			} else {
				fovValue = fovOut;
				//zoom disable when the scanner mode was disabled
			}
		}
	}
}