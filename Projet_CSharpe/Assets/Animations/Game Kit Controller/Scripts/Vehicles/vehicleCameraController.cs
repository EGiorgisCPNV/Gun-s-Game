using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class vehicleCameraController : MonoBehaviour {
	public float rotationSpeed = 10;
	public float clipCastRadius = 0.16f;
	public float backClipSpeed;
	public float maximumBoostDistance;
	public float cameraBoostSpeed;
	public float smoothBetweenState;
	public string currentStateName;
	public List<vehicleCameraStateInfo> vehicleCameraStates = new List<vehicleCameraStateInfo> ();
	public shakeSettingsInfo shakeSettings;
	public float gizmoRadius;
	public bool showGizmo;
	public Color labelGizmoColor;
	public GameObject vehicle;
	public LayerMask layer;
	public bool cameraChangeEnabled;
	public float rotationDamping = 3;
	public bool cameraPaused;
	public bool zoomEnabled;
	public float zoomSpeed = 120;
	public float zoomFovValue = 17;
	public float rotationSpeedZoomIn;
	public bool isFirstPerson;
	public bool usingZoomOn;
	[HideInInspector] public vehicleCameraStateInfo currentState;
	float currentCameraDistance;
	float originalCameraDistance;
	float currentOriginalDistValue;
	float cameraSpeed;
	float originalRotationSpeed;
	float originalCameraFov;
	Vector2 mouseAxis;
	Vector2 lookAngle;
	Vector3 currentPivotPosition;
	Vector3 originalPivotPosition;
	Vector3 nextPivotPositon;
	bool boosting;
	bool releaseCamera;
	bool pickCamera;
	bool drivingVehicle;
	bool followVehiclePosition = true;
	bool firstCameraEnabled;
	Ray ray;
	RaycastHit[] hits;
	vehicleWeaponSystem weaponManager;
	inputActionManager actionManager;
	Coroutine moveCamera;
	vehicleGravityControl gravityControl;
	Rigidbody mainRigidbody;
	int cameraStateIndex;
	vehicleCameraShake shakingManager;
	GameObject player;
	playerController playerManager;
	 
	void Start () {
		for (int i = 0; i < vehicleCameraStates.Count; i++) {
			vehicleCameraStates [i].originalDist = vehicleCameraStates [i].cameraTransform.localPosition.magnitude;
			vehicleCameraStates [i].originalPivotPosition = vehicleCameraStates [i].pivotTransform.localPosition;
		}
		//get the main components of the camera, like the pivot and the transform which contains the main camera when the player is driving this vehicle
		setCameraState (currentStateName);
		//get the current local position of the camera
		originalCameraDistance = currentState.cameraTransform.localPosition.magnitude;
		//if the vehicle has a weapon system, store it
		if (vehicle.GetComponent<vehicleWeaponSystem> ()) {
			weaponManager=vehicle.GetComponent<vehicleWeaponSystem> ();
			//set the current camera used in the vehicle in the weapon component
			weaponManager.getCameraInfo (currentState.cameraTransform);
		}
		//get the original local position of the pivot
		originalPivotPosition = currentState.pivotTransform.localPosition;
		gravityControl = vehicle.GetComponent<vehicleGravityControl> ();
		if (gravityControl) {
			gravityControl.getCurrentCameraPivot (currentState.pivotTransform);
		}
		mainRigidbody = vehicle.GetComponent<Rigidbody>();
		shakingManager = GetComponent<vehicleCameraShake> ();
		if (shakingManager) {
			shakingManager.getCurrentCameraTransform (Camera.main.transform);
		}
		originalRotationSpeed = rotationSpeed;
		originalCameraFov = Camera.main.fieldOfView;
	}
	void Update (){
		//print (currentState.cameraTransform.localPosition);
		//print (currentDist);
		//print(originalCameraDistance);
		//set the camera position in the vehicle position to follow it
		if (followVehiclePosition) {
			transform.position = vehicle.transform.position;
		}
		//if the vehicle is being driving and the pause menu is not active, allow the camera to rotate
		if (drivingVehicle && !actionManager.input.gamePaused && !cameraPaused) {
			if (!currentState.cameraFixed) {
				//get the current input axis values from the input manager
				mouseAxis.x = actionManager.input.getMovementAxis ("mouse").x;
				mouseAxis.y = actionManager.input.getMovementAxis ("mouse").y;
				//if the first camera view is enabled
				if (currentState.firstPersonCamera) {
					isFirstPerson = true;
					if (currentState.xLimits != Vector2.zero || currentState.yLimits != Vector2.zero) {
						//get the look angle value
						lookAngle.x += mouseAxis.x * rotationSpeed;
						lookAngle.y -= mouseAxis.y * rotationSpeed;
						//clamp these values to limit the camera rotation
						lookAngle.y = Mathf.Clamp (lookAngle.y, -currentState.xLimits.x, currentState.xLimits.y);
						lookAngle.x = Mathf.Clamp (lookAngle.x, -currentState.yLimits.x, currentState.yLimits.y);
						//set every angle in the camera and the pivot
						currentState.cameraTransform.localRotation = Quaternion.Euler (lookAngle.y, 0, 0);
						currentState.pivotTransform.localRotation = Quaternion.Euler (0, lookAngle.x, 0);
					}
				}
				//else, the camera is in third person view
				else {
					isFirstPerson = false;
					//get the look angle value
					lookAngle.x = mouseAxis.x * rotationSpeed;
					lookAngle.y -= mouseAxis.y * rotationSpeed;
					//clamp these values to limit the camera rotation
					lookAngle.y = Mathf.Clamp (lookAngle.y, -currentState.xLimits.x, currentState.xLimits.y);
					//set every angle in the camera and the pivot
					transform.Rotate (0, lookAngle.x, 0);
					currentState.pivotTransform.transform.localRotation = Quaternion.Euler (lookAngle.y, 0, 0);
					//get the current camera position for the camera collision detection
					currentCameraDistance = checkCameraCollision ();
					//set the local camera position
					currentCameraDistance = Mathf.Clamp (currentCameraDistance, 0, originalCameraDistance);
					currentState.cameraTransform.localPosition = -Vector3.forward * currentCameraDistance;
				}
			} else {
				float speed = (mainRigidbody.transform.InverseTransformDirection(mainRigidbody.velocity).z) * 3f;
				int multSign = 1;
				float angleY = Mathf.Asin(transform.InverseTransformDirection( Vector3.Cross(transform.right, vehicle.transform.right)).y) * Mathf.Rad2Deg;
				if(speed < -2){
					multSign = -1;
				}
				transform.Rotate (0, angleY * Time.deltaTime * rotationDamping * multSign, 0);
				//get the current camera position for the camera collision detection
				currentCameraDistance = checkCameraCollision ();
				//set the local camera position
				currentCameraDistance = Mathf.Clamp (currentCameraDistance, 0, originalCameraDistance);
				currentState.cameraTransform.localPosition = -Vector3.forward * currentCameraDistance;
			}
			//check if the change camera input is used
			if (actionManager.getActionInput ("Change Camera")) {
				changeCameraPosition ();
			}
			if (actionManager.getActionInput ("Zoom")) {
				setZoom (!usingZoomOn);
			}
			//check if the move away camera input is used
//			if (input.getButton ("Move Away Camera", inputManager.buttonType.getKeyDown) || input.getTouchButton ("Move Away Camera", inputManager.buttonType.getKeyDown)) {
//				moveAwayCamera();
//			}
		}
		//if the boost is being used, move the camera in the backward direction
		if (boosting) {
			//the camera is moving in backward direction
			if (releaseCamera) {
				originalCameraDistance += Time.deltaTime*cameraBoostSpeed;
				if (originalCameraDistance >= maximumBoostDistance + currentState.originalDist) {
					originalCameraDistance = currentState.originalDist + maximumBoostDistance;
					releaseCamera = false;
				}
			}
			//the camera is moving to its regular position
			if (pickCamera) {
				originalCameraDistance -= Time.deltaTime*cameraBoostSpeed;
				if (originalCameraDistance <= currentState.originalDist) {
					originalCameraDistance = currentState.originalDist;
					pickCamera = false;
					boosting = false;
				}
			}
		}
	}
	public void setZoom(bool state){
		if (zoomEnabled) {
			//to the fieldofview of the camera, it is added of substracted the zoomvalue
			usingZoomOn = state;
			float targetFov = zoomFovValue;
			float rotationSpeedTarget = rotationSpeedZoomIn;
			if (!usingZoomOn) {
				rotationSpeedTarget = originalRotationSpeed;
				targetFov = originalCameraFov;
			}
			playerManager.pCamera.GetComponent<playerCamera> ().checkFovCoroutine (targetFov, zoomSpeed);
			//also, change the sensibility of the camera when the zoom is on or off, to control the camera properly
			rotationSpeed = rotationSpeedTarget;
		}
	}
	public void setCameraState(string stateName){
		for (int i = 0; i < vehicleCameraStates.Count; i++) {
			if (vehicleCameraStates [i].name == stateName) {
				currentState = new vehicleCameraStateInfo( vehicleCameraStates [i]);
				currentStateName = stateName;
			}
		}
	}
	//function called when the player uses the boost in the vehicle
	public void usingBoost(bool state, string shakeName){
		boosting = true;
		if (state) {
			releaseCamera = true;
			pickCamera = false;
			if (shakingManager) {
				shakingManager.startShake (shakeName);
			}
		} else {
			releaseCamera = false;
			pickCamera = true;
			if (shakingManager) {
				shakingManager.stopShake ();
			}
		}
	}
	//the player has changed the current camera view for the other option, firts or third
	public void changeCameraPosition(){
		//if the camera can be changed
		if (cameraChangeEnabled) {
			cameraStateIndex++;
			if (cameraStateIndex > vehicleCameraStates.Count-1) {
				cameraStateIndex = 0;
			}
			bool exit = false;
			int max = 0;
			while (!exit) {
				for (int k = 0; k < vehicleCameraStates.Count; k++) {
					if (vehicleCameraStates [k].enabled && k == cameraStateIndex) {
						cameraStateIndex = k;
						exit = true;
					}
				}
				if (!exit) {
					max++;
					if (max > 100) {
						print ("fallo");
						return;
					}
					//set the current power
					cameraStateIndex++;
					if (cameraStateIndex > vehicleCameraStates.Count - 1) {
						cameraStateIndex = 0;
					}
				}
			}
			nextPivotPositon = currentState.pivotTransform.position;
			setCameraState (vehicleCameraStates [cameraStateIndex].name);
			//reset the look angle
			lookAngle = Vector2.zero;
			//reset the pivot rotation
			currentState.pivotTransform.localRotation = Quaternion.identity;
			currentState.cameraTransform.localRotation = Quaternion.identity;
			//set the new parent of the camera as the first person position
			Camera.main.transform.SetParent (currentState.cameraTransform);
			//reset camera rotation and position
			Camera.main.transform.localPosition = Vector3.zero;
			Camera.main.transform.localRotation = Quaternion.identity;
			currentOriginalDistValue = currentState.originalDist;
			if (currentState.smoothTransition) {
				checkCameraTranslation ();
			} else {
				originalCameraDistance = currentState.originalDist;
			}
			//change the current camera in the gravity controller component
			if (gravityControl) {
				gravityControl.getCurrentCameraPivot (currentState.pivotTransform);
			}
			if (shakingManager) {
				shakingManager.getCurrentCameraTransform (Camera.main.transform);
			}
			//do the same in the weapons system if the vehicle has it
			if (weaponManager) {
				weaponManager.getCameraInfo (currentState.cameraTransform);
			}
			if (currentState.firstPersonCamera) {
				playerManager.changeHeadScale (true);
			} else {
				playerManager.changeHeadScale (false);
			}
		}
	}
	public void setDamageCameraShake(){
		if (isFirstPerson) {
			if (shakeSettings.useDamageShakeInFirstPerson) {
				shakingManager.setExternalShakeState (shakeSettings.firstPersonDamageShake);
			}
		} else {
			if (shakeSettings.useDamageShakeInThirdPerson) {
				shakingManager.setExternalShakeState (shakeSettings.thirdPersonDamageShake);
			}
		}
	}
	public void setCameraExternalShake(headBob.externalShakeInfo externalShake){
		shakingManager.setExternalShakeState (externalShake);
	}
	//move away or turn back the camera
	public void moveAwayCamera(){
		
	}
	//stop the current coroutine and start it again
	void checkCameraTranslation(){
		if (moveCamera != null) {
			StopCoroutine (moveCamera);
		}
		moveCamera = StartCoroutine(changeCamerCollisionDistanceCoroutine());
	}
	IEnumerator changeCamerCollisionDistanceCoroutine(){
		currentState.pivotTransform.position = nextPivotPositon;
		//move the pivot and the camera dist for the camera collision 
		float t = 0;
		//translate position of the pivot
		while (t < 1) {
			t += Time.deltaTime * smoothBetweenState;
			originalCameraDistance = Mathf.Lerp (originalCameraDistance, currentOriginalDistValue, t);
			currentState.pivotTransform.localPosition=Vector3.Lerp(currentState.pivotTransform.localPosition, currentState.originalPivotPosition, t);
			yield return null;
		}
	}
	public void getPlayer(GameObject playerElement){
		if (!player) {
			player = playerElement;
			playerManager = player.GetComponent<playerController> ();
		}
	}
	//the vehicle is being driving or not
	public void changeCameraDrivingState(bool state){
		drivingVehicle=state;
		//if the vehicle is not being driving, stop all its states
		if (!drivingVehicle) {
			releaseCamera = false;
			pickCamera = false;
			boosting = false;
			playerManager.changeHeadScale (false);
			if (usingZoomOn) {
				setZoom (false);
			}
		} 
		//else, reset the vehicle camera rotation
		else {
			if (firstCameraEnabled) {
				playerManager.changeHeadScale (true);
			}
			//reset the camera position in the vehicle, so always that the player gets on, the camera is set just behind the vehicle
			originalCameraDistance = currentState.originalDist;
			//reset the local angle x of the pivot camera
			currentState.pivotTransform.localRotation = Quaternion.identity;
			currentState.cameraTransform.localPosition = -Vector3.forward * originalCameraDistance;
			lookAngle = Vector2.zero;
			//reset the local angle y of the vehicle camera
			float angleY = Vector3.Angle (vehicle.transform.forward, transform.forward);
			angleY *= Mathf.Sign (transform.InverseTransformDirection (Vector3.Cross (vehicle.transform.forward, transform.forward)).y);
			transform.Rotate (0, -angleY, 0);
		}
	}
	//when the player gets on to the vehicle, it is checked if the first person was enabled or not, to set that camera view in the vehicle too
	public void setCameraPosition(bool state){
		//get the current view of the camera, so when it is changed, it is done correctly
		firstCameraEnabled = state;
		//set the current camera view in the weapons system
		if (weaponManager) {
			if (firstCameraEnabled) {
				setFirstOrThirdPerson (true);

			} else {
				setFirstOrThirdPerson (false);
			}
			weaponManager.getCameraInfo (currentState.cameraTransform);
		}
	}
	public void setFirstOrThirdPerson(bool state){
		bool assigned = false;
		for (int k = 0; k < vehicleCameraStates.Count; k++) {
			if (!assigned) {
				if (state) {
					if (vehicleCameraStates [k].firstPersonCamera) {
						setCameraState (vehicleCameraStates [k].name);
						cameraStateIndex = k;
						assigned = true;
					}
				} else {
					if (!vehicleCameraStates [k].firstPersonCamera) {
						setCameraState (vehicleCameraStates [k].name);
						cameraStateIndex = k;
						assigned = true;
					}
				}
			}
		}
	}
	//adjust the camera position to avoid cross any collider
	public float checkCameraCollision(){
		//launch a ray from the pivot position to the camera direction
		ray.origin = currentState.pivotTransform.position;
		ray.direction = -currentState.pivotTransform.forward;
		//store the hits received
		hits = Physics.SphereCastAll (ray, clipCastRadius, originalCameraDistance + clipCastRadius,layer);
		float closest = Mathf.Infinity;
		float hitDist = originalCameraDistance;
		//find the closest
		for (int i = 0; i < hits.Length; i++) {
			if (hits [i].distance < closest && !hits [i].collider.isTrigger) {
				//the camera will be moved that hitDist in its forward direction
				closest = hits [i].distance;
				hitDist = -currentState.pivotTransform.InverseTransformPoint (hits [i].point).z;
			}
		}
		//clamp the hidDist value
		if (hitDist < 0) {
			hitDist = 0;
		}
		if (hitDist > originalCameraDistance) {
			hitDist = originalCameraDistance;
		}
		//return the value of the collision in the camera
		return Mathf.SmoothDamp (currentCameraDistance, hitDist, ref cameraSpeed, currentCameraDistance > hitDist ? 0 : backClipSpeed);
	}
	//get the input manager component
	public void getInputActionManager(inputActionManager manager){
		actionManager = manager;
	}
	public void startOrStopFollowVehiclePosition(bool state){
		followVehiclePosition = state;
	}
	public void pauseOrPlayVehicleCamera(bool state){
		cameraPaused = state;
	}
	//draw the move away position of the pivot and the camera in the inspector
	void OnDrawGizmos(){
		DrawGizmos ();
	}
	void OnDrawGizmosSelected(){
		DrawGizmos ();
	}
	void DrawGizmos(){
		//&& !Application.isPlaying
		if (showGizmo ) {
			for (int i = 0; i < vehicleCameraStates.Count; i++) {
				if (vehicleCameraStates [i].showGizmo) {
					Gizmos.color = Color.white;
					Gizmos.DrawLine (vehicleCameraStates [i].pivotTransform.position, vehicleCameraStates [i].cameraTransform.position);
					Gizmos.DrawLine (vehicleCameraStates [i].pivotTransform.position, transform.position);
					Gizmos.color = vehicleCameraStates [i].gizmoColor;
					Gizmos.DrawSphere (vehicleCameraStates [i].pivotTransform.position, gizmoRadius);
					Gizmos.DrawSphere (vehicleCameraStates [i].cameraTransform.position, gizmoRadius);
				}
			}
		}
	}
	[System.Serializable]
	public class vehicleCameraStateInfo{
		public string name;
		public Transform pivotTransform;
		public Transform cameraTransform;
		public Vector2 xLimits;
		public Vector2 yLimits;
		public bool enabled;
		public bool firstPersonCamera;
		public bool cameraFixed;
		public bool smoothTransition;
		public bool showGizmo;
		public Color gizmoColor;
		public float labelGizmoOffset;
		public bool gizmoSettings;
		[HideInInspector] public float originalDist;
		[HideInInspector] public Vector3 originalPivotPosition;

		public vehicleCameraStateInfo(vehicleCameraStateInfo newState){
			name = newState.name;
			pivotTransform =newState.pivotTransform;
			cameraTransform=newState.cameraTransform;
			xLimits= newState.xLimits;
			yLimits= newState.yLimits;
			enabled=newState.enabled;
			firstPersonCamera=newState.firstPersonCamera;
			cameraFixed=newState.cameraFixed;
			smoothTransition=newState.smoothTransition;
			originalDist=newState.originalDist;
			originalPivotPosition=newState.originalPivotPosition;
		}
	}
	[System.Serializable]
	public class shakeSettingsInfo{
		public bool useDamageShake;
		public bool useDamageShakeInThirdPerson;
		public headBob.externalShakeInfo thirdPersonDamageShake;
		public bool useDamageShakeInFirstPerson;
		public headBob.externalShakeInfo firstPersonDamageShake;
	}
}