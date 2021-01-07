using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class lockedCameraSystem : MonoBehaviour {
	public List<cameraElement> lockedCameraList = new List<cameraElement> ();
	public bool cameraChanged;
	playerCamera playerCameraManager;
	playerController playerControlerManager;
	Transform currentCameraTransform;
	Transform currentCameraAxis;
	Transform previousCameraAxis;
	Transform currentAxisTransform;
	bool cameraLocked;
	int currentCameraElementIndex;
	int currentCameraAxisIndex;
	GameObject player;

	void Start () {
		player = GameObject.Find ("Player Controller");
		playerCameraManager = GameObject.Find ("Player Camera").GetComponent<playerCamera> ();
		playerControlerManager = player.GetComponent<playerController> ();
		GameObject currentAxis = new GameObject ();
		currentAxisTransform = currentAxis.transform;
		currentAxisTransform.name = "CurrentAxisTransform";
	}
	void Update () {
		if (cameraChanged) {
			if (currentCameraAxis != previousCameraAxis) {
				if (!playerControlerManager.isMoving) {
					setCurrentAxisTransformValues (currentCameraAxis);
					//playerCameraManager.setCameraTransform (currentAxisTransform);

					//playerCameraManager.setCameraTransform (currentCameraAxis);
					previousCameraAxis = null;
					cameraChanged = false;
				}
			} else {
				cameraChanged = false;
			}
		}
		if (cameraLocked) {
			if (lockedCameraList [currentCameraElementIndex].cameraTransformList [currentCameraAxisIndex].followPlayerPosition) {
				Vector3 lookPos = player.transform.position - currentCameraTransform.position;
				Quaternion rotation = Quaternion.LookRotation (lookPos);
				Vector3 rotatioEuler = rotation.eulerAngles;
				float rotatioEulerX = rotatioEuler.x;
				float rotatioEulerY = rotatioEuler.y;
//				if (lockedCameraList [currentCameraElementIndex].cameraTransformList [currentCameraAxisIndex].useRotationLimits) {
//					if (rotatioEulerY > 180) {
//						rotatioEulerY =	Mathf.Clamp (rotatioEulerY, 
//							lockedCameraList [currentCameraElementIndex].cameraTransformList [currentCameraAxisIndex].rotationLimitsLeftY.x + 360,
//							360 - lockedCameraList [currentCameraElementIndex].cameraTransformList [currentCameraAxisIndex].rotationLimitsLeftY.y);
//					} else if (rotatioEulerY < 180 && rotatioEulerY >= 90) {
//						rotatioEulerY =	Mathf.Clamp (rotatioEulerY, 
//							lockedCameraList [currentCameraElementIndex].cameraTransformList [currentCameraAxisIndex].rotationLimitsRightY.y, 
//							lockedCameraList [currentCameraElementIndex].cameraTransformList [currentCameraAxisIndex].rotationLimitsRightY.x);
//					} else {
//						rotatioEulerY =	Mathf.Clamp (rotatioEulerY, 
//							lockedCameraList [currentCameraElementIndex].cameraTransformList [currentCameraAxisIndex].rotationLimitsRightY.x,
//							360 - lockedCameraList [currentCameraElementIndex].cameraTransformList [currentCameraAxisIndex].rotationLimitsRightY.y);
//					}
//				} 
				currentCameraTransform.localRotation = Quaternion.Slerp (currentCameraTransform.localRotation, Quaternion.Euler (new Vector3 (rotatioEulerX, 0, 0)), 
					Time.deltaTime * lockedCameraList [currentCameraElementIndex].cameraTransformList [currentCameraAxisIndex].followSpeed);

				currentCameraAxis.localRotation = Quaternion.Slerp (currentCameraAxis.localRotation, Quaternion.Euler (new Vector3 (0, rotatioEulerY, 0)), 
					Time.deltaTime * lockedCameraList [currentCameraElementIndex].cameraTransformList [currentCameraAxisIndex].followSpeed);

				setCurrentAxisTransformValues (currentCameraAxis);
			}
		}
	}
	public void setCameraTransform(GameObject cameraTransform){
		if (currentCameraTransform) {
			if (currentCameraTransform == cameraTransform) {
				return;
			}
		}
		for (int i = 0; i < lockedCameraList.Count; i++) {
			for (int j = 0; j < lockedCameraList[i].cameraTransformList.Count; j++) {
				if (lockedCameraList [i].cameraTransformList [j].cameraPosition == cameraTransform.transform) {
					if (lockedCameraList [i].setCameraToFree) {
						playerCameraManager.setCameraToFreeOrLocked (playerCamera.typeOfCamera.free, null, null);
						cameraLocked = false;
					} else {
						previousCameraAxis = playerCameraManager.getCameraTransform();
						currentCameraAxis = lockedCameraList [i].cameraTransformList [j].axis;
						setCurrentAxisTransformValues (previousCameraAxis);
						playerCameraManager.setCameraToFreeOrLocked (playerCamera.typeOfCamera.locked, cameraTransform.transform, currentAxisTransform);
						cameraChanged = true;
						cameraLocked = true;
					}
					currentCameraTransform = lockedCameraList [i].cameraTransformList [j].cameraPosition;
					lockedCameraList [i].currentCameraTransform = true;
					currentCameraElementIndex = i;
					currentCameraAxisIndex = j;
				} else {
					lockedCameraList [i].currentCameraTransform = false;
				}
			}
		}
	}
	public void setCurrentAxisTransformValues(Transform newValues){
		currentAxisTransform.position = newValues.position;
		currentAxisTransform.eulerAngles = new Vector3 (0, newValues.eulerAngles.y, 0);
	}
	[System.Serializable]
	public class cameraElement{
		public string name;
		public List<cameraAxis> cameraTransformList = new List<cameraAxis> ();
		public bool currentCameraTransform;
		public bool setCameraToFree;
	}
	[System.Serializable]
	public class cameraAxis{
		public Transform axis;
		public Transform cameraPosition;
		public bool followPlayerPosition;
		public float followSpeed;
//		public bool useRotationLimits;
//		public Vector2 rotationLimitsLeftX;
//		public Vector2 rotationLimitsRightX;
//		public Vector2 rotationLimitsLeftY;
//		public Vector2 rotationLimitsRightY;
	}
}