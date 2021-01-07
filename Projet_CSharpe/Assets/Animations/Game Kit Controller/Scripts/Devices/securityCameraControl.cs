using UnityEngine;
using System.Collections;
public class securityCameraControl : MonoBehaviour {
	public GameObject camera;
	public GameObject joystick;
	public Transform joystickAxisX;
	public Transform joystickAxisZ;
	public GameObject zoomOutButton;
	public GameObject zoomInButton;
	public Vector2 clampTiltX;
	public Vector2 clampTiltZ;
	public LayerMask layer;
	public float dragDistance = 1;
	public bool controlEnabled;
	Touch currentTouch;
	int i;
	bool touchPlatform;
	bool touching;
	RaycastHit hit;
	Vector2 currentAxisValue;
	Vector3 beginTouchPosition;
	securityCamera securityCameraManager;
	Vector2 lookAngle;
	bool zoomIn;
	bool zoomOut;
	Coroutine inCoroutine;
	Coroutine outCoroutine;

	void Start () {
		//get the security camera component of the camera
		securityCameraManager = camera.GetComponent<securityCamera> ();
		//check if the current platform is PC or android
		touchPlatform = touchJoystick.checkTouchPlatform ();
	}

	void Update () {
		//if the security camera control is being used
		if (controlEnabled) {
			//check for touch input from the mouse or the finger
			int touchCount = Input.touchCount;
			if (!touchPlatform) {
				touchCount++;
			}
			for (i = 0; i < touchCount; i++) {
				if (!touchPlatform) {
					currentTouch = touchJoystick.convertMouseIntoFinger ();
				}
				else{
					currentTouch = Input.GetTouch(i);
				}
				//in the touch begin phase
				if (currentTouch.phase == TouchPhase.Began && !touching) {
					//check with a ray
					Ray ray = Camera.main.ScreenPointToRay (currentTouch.position);
					if (Physics.Raycast (ray, out hit, Mathf.Infinity, layer)) {
						//the pressed position is the joystick of the camera
						if (hit.collider.gameObject == joystick) {
							touching = true;
							//store the initial press position
							beginTouchPosition = new Vector3(currentTouch.position.x,currentTouch.position.y,0);
						}
						//the pressed position is the zoom in button
						if (hit.collider.gameObject == zoomInButton) {
							zoomIn = true;
							//move the button down
							checkButtonTranslation (true,zoomInButton,true);
						}
						//the pressed position is the zoom out button
						if (hit.collider.gameObject == zoomOutButton) {
							zoomOut = true;
							//move the button down
							checkButtonTranslation (false,zoomOutButton,true);
						}
					}
				}
				//the current touch press is being moved
				if ((currentTouch.phase == TouchPhase.Moved || currentTouch.phase == TouchPhase.Stationary) && touching) {
					//like in the joystick script, get the amount of movement between the inital press position and the current touch position
					Vector3 globalTouchPosition = new Vector3(currentTouch.position.x,currentTouch.position.y,0);
					Vector3 differenceVector = globalTouchPosition - beginTouchPosition;
					if (differenceVector.sqrMagnitude > dragDistance * dragDistance){
						differenceVector.Normalize();
					}
					//get the axis from the touch movement
					currentAxisValue = differenceVector;
					//rotate the camera joystick too
					lookAngle.x += currentAxisValue.x * securityCameraManager.sensitivity;
					lookAngle.y += currentAxisValue.y * securityCameraManager.sensitivity;
					//clamp these values to limit the joystick rotation
					lookAngle.y = Mathf.Clamp (lookAngle.y, -clampTiltX.x, clampTiltX.y);
					lookAngle.x = Mathf.Clamp (lookAngle.x, -clampTiltZ.x, clampTiltZ.y);
					//apply the rotation to every component in the X and Z axis
					joystickAxisX.transform.localRotation=Quaternion.Euler (0, 0, -lookAngle.x);
					joystickAxisZ.transform.localRotation=Quaternion.Euler (lookAngle.y, 0, 0);
				}
				//if the touch ends, reset the rotation of the joystick, the current axis values and the zoom buttons positions
				if (currentTouch.phase == TouchPhase.Ended) {
					touching = false;
					currentAxisValue = Vector2.zero;
					lookAngle = Vector2.zero;
					StartCoroutine (resetJoystick ());
					if (zoomIn) {
						zoomIn = false;
						checkButtonTranslation (true, zoomInButton,false);
					}
					if (zoomOut) {
						zoomOut = false;
						checkButtonTranslation (false,zoomOutButton,false);
					}
				}
			}
			//send the current axis value to the security camera to rotate it
			securityCameraManager.getLookValue(currentAxisValue);
		}
		//set the zoom in the camera
		if (zoomIn) {
			securityCameraManager.setZoom (-1);
		}
		if (zoomOut) {
			securityCameraManager.setZoom (1);
		}
	}
	//reset the joystick rotation
	IEnumerator resetJoystick(){
		for (float t = 0; t < 1;) {
			t += Time.deltaTime * 3;
			joystickAxisX.transform.localRotation = Quaternion.Slerp (joystickAxisX.transform.localRotation, Quaternion.identity, t);
			joystickAxisZ.transform.localRotation = Quaternion.Slerp (joystickAxisZ.transform.localRotation, Quaternion.identity, t);
			yield return null;
		}
	}
	//if this control is enabled, enable the camera component too
	void OnEnable(){
		if (!securityCameraManager) {
			Start ();
		} 
		securityCameraManager.changeCameraState (true);
		controlEnabled = true;
	}
	//else, this control is disabled, so disable the camera component too
	void OnDisable(){
		securityCameraManager.changeCameraState (false);
		controlEnabled = false;
	}
	//if a zoom button is pressed, check the coroutine, stop it and play it again
	void checkButtonTranslation(bool state,GameObject button, bool value){
		if (state) {
			if (inCoroutine != null) {
				StopCoroutine (inCoroutine);
			}
			inCoroutine = StartCoroutine (pressButton (button,value));
		} else {
			if (outCoroutine != null) {
				StopCoroutine (outCoroutine);
			}
			outCoroutine = StartCoroutine (pressButton (button,value));
		}
	}
	//move the zoom buttons up and down when they are pressed, instead of using animations
	IEnumerator pressButton(GameObject button, bool value){
		int mult = 1;
		if (value) {
			mult = -1;
		}
		Vector3 buttonPos = button.transform.localPosition;
		Vector3 newButtonPos = button.transform.localPosition + Vector3.up * mult*0.03f;
		for (float t = 0; t < 1;) {
			t += Time.deltaTime * 3;
			button.transform.localPosition = Vector3.MoveTowards (buttonPos, newButtonPos, t);
			yield return null;
		}
	}
}