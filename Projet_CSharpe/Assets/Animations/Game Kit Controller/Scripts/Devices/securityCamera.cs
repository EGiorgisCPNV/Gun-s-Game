using UnityEngine;
using System.Collections;
public class securityCamera : MonoBehaviour {
	public float sensitivity;
	public Vector2 clampTiltY;
	public Vector2 clampTiltX;
	public Vector2 zoomLimit;
	public GameObject baseX;
	public GameObject baseY;
	public bool activated;
	public float zoomSpeed;
	[HideInInspector] public Vector2 lookAngle;
	Vector2 axisValues;
	Camera cam;
	float originalFov;

	void Start () {
		//get the camera in the children, store the origianl fov and disable it
		cam=GetComponentInChildren<Camera> ();
		cam.enabled = false;
		originalFov = cam.fieldOfView;
	}

	void Update () {
		//if the camera is being used
		if (activated) {
			//get the look angle value
			lookAngle.x += axisValues.x * sensitivity;
			lookAngle.y += axisValues.y * sensitivity;
			//clamp these values to limit the camera rotation
			lookAngle.y = Mathf.Clamp (lookAngle.y, -clampTiltX.x, clampTiltX.y);
			lookAngle.x = Mathf.Clamp (lookAngle.x, -clampTiltY.x, clampTiltY.y);
			//set every angle in the camera and the pivot
			baseX.transform.localRotation = Quaternion.Euler (-lookAngle.y,0 , 0);
			baseY.transform.localRotation = Quaternion.Euler (0, lookAngle.x, 0);
		}
	}
	//the camera is being rotated, so set the axis values
	public void getLookValue(Vector2 currentAxisValues){
		axisValues = currentAxisValues;
	}
	//the zoom is being used, so change the fov according to the type of zoom, in or out
	public void setZoom(int mult){
		float zoomValue = cam.fieldOfView;
		zoomValue += Time.deltaTime * mult * zoomSpeed;
		zoomValue = Mathf.Clamp (zoomValue, zoomLimit.x, zoomLimit.y);
		cam.fieldOfView = zoomValue;
	}
	//enable or disable the camera according to if the control is being using if a computer device
	public void changeCameraState(bool state){
		activated = state;
		if (cam) {
			cam.enabled = state;
			if (!activated) {
				cam.fieldOfView = originalFov;
			}
		}
	}
}