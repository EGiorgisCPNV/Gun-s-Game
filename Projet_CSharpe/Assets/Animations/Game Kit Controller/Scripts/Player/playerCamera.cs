using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class playerCamera : MonoBehaviour {
	public bool cameraCanBeUsed;
	public string currentStateName;
	public float rotationSpeed = 10; 
	public float smoothBetweenState;
	public float maxCheckDist = 0.1f;
	public float movementLerpSpeed = 5;
	public float zoomSpeed=120;
	public float fovChangeSpeed;
	public float maxFovValue;
	public float minFovValue=17;
	public float rotationSpeedZoomIn;
	public List<cameraStateInfo> playerCameraStates = new List<cameraStateInfo> ();
	public cameraSettings settings = new cameraSettings();
	public bool grounded;
	public bool aiming;
	public bool moveAwayActive;
	public bool crouching;
	public bool firstPersonActive;
	public bool usingZoomOn;
	public bool usingZoomOff;
	public bool cameraCanRotate=true;
	public typeOfCamera cameraType;
	public enum typeOfCamera
	{
		free, locked
	}
	[HideInInspector] public Transform mainCameraTransform; 
	[HideInInspector] public Transform pivot; 
	[HideInInspector] public Vector2 lookAngle;
	[HideInInspector] public cameraStateInfo currentState;
	[HideInInspector] public cameraStateInfo lerpState;
	[HideInInspector] public float x,y;
	float originalCameraFov;
	float originalRotationSpeed;
	float AccelerometerUpdateInterval = 0.01f;
	float LowPassKernelWidthInSeconds = 0.001f;
	float lastTimeMoved;
	bool adjustPivotAngle;
	bool isMoving;
	RaycastHit hit;
	Vector3 lowPassValue = Vector3.zero;
	Vector2 acelerationAxis;
	GameObject player;
	inputManager input;
	Matrix4x4 calibrationMatrix;
	playerController pController;
	otherPowers powers;
	changeGravity gravity;
	Camera mainCamera;
	Transform hips;
	Coroutine changeFovCoroutine;
	Transform targetToFollow;
	headBob headBobManager;
	bool smoothFollow;
	bool smoothReturn;
	bool smoothGo;
	bool dead;
	playerWeaponsManager weaponsManager;
	damageInScreen damageInScreenManager;
	jetpackSystem jetpackSystemManager;

	void Start (){
		//get the player gameObject
		player = GameObject.Find ("Player Controller");
		//get other components of the player
		pController = player.GetComponent<playerController> ();
		//get the player's hips, so the camera can follow the ragdoll
		hips = player.GetComponent<Animator> ().GetBoneTransform (HumanBodyBones.Hips).transform;
		//get the camera of the player and the pivot
		mainCamera = Camera.main;
		mainCameraTransform = mainCamera.transform;
		pivot = mainCameraTransform.parent;
		//if the game doesn't starts with the first person view, get the original camera position and other parameters for the camera collision system and 
		//movement ranges
		if (!firstPersonActive) {
			//check if the player uses animator in first person
			pController.checkAnimatorIsEnabled (false);		
		} else {
			//check if the player uses animator in first person
			pController.checkAnimatorIsEnabled (true);			
		}
		//get the original field of view of the camera and other parameters for the zoom mode
		originalCameraFov = mainCamera.fieldOfView;
		originalRotationSpeed = rotationSpeed;
		//get the input manager to get every key or touch press
		input = transform.parent.GetComponent<inputManager> ();
		powers = player.GetComponent<otherPowers> ();
		gravity = player.GetComponent<changeGravity> ();
		//set the camera state when the game starts
		setCameraState (currentStateName);
		currentState = new cameraStateInfo (lerpState);
		targetToFollow = player.transform;
		headBobManager = mainCamera.GetComponent<headBob> ();
		weaponsManager = player.GetComponent<playerWeaponsManager> ();
		damageInScreenManager = player.GetComponent<damageInScreen> ();
		jetpackSystemManager = player.GetComponent<jetpackSystem> ();
	}
	void Update(){
		if (cameraType == typeOfCamera.free) {
			if (cameraCanBeUsed) {
				if (!dead) {
					Vector3 dir = mainCameraTransform.position - pivot.position;
					float dist = Mathf.Abs (currentState.camPositionOffset.z);
					if (Physics.SphereCast (pivot.position, maxCheckDist, dir, out hit, dist, settings.layer)) {
						mainCameraTransform.position = pivot.position + (dir.normalized * hit.distance);
					} else {
						Vector3 mainCamPos = mainCameraTransform.localPosition;
						Vector3 newPos = Vector3.Lerp (mainCamPos, currentState.camPositionOffset, Time.deltaTime * movementLerpSpeed);
						mainCameraTransform.localPosition = newPos;
					}
					pivot.localPosition = Vector3.Lerp (pivot.localPosition, currentState.pivotPositionOffset, Time.deltaTime * movementLerpSpeed);

					//shake the camera if the player is moving in the air or accelerating on it
					if (settings.enableShakeCamera && settings.shake) {
						if (!settings.accelerateShaking) {
							headBobManager.setState ("Shaking");
						}
						if (settings.accelerateShaking) {
							headBobManager.setState ("High Shaking");
						}
					}
				}
				if (input.checkInputButton ("Move Away Camera", inputManager.buttonType.getKeyDown) && settings.moveAwayCameraEnabled) {
					moveAwayCamera ();
				}
				//enable and disable the zoom of the player
				if (input.checkInputButton ("Zoom", inputManager.buttonType.getKeyDown) && settings.zoomEnabled) {
					setZoom (!usingZoomOn);
				}
			}
			//the camera follows the player position
			//if smoothfollow is false, it means that the player is alive
			if (!smoothFollow) {
				//smoothreturn is used to move the camera from the hips to the player controller position smoothly, to avoid change their positions quickly
				if (smoothReturn) {
					float speed = 1;
					float distance = Vector3.Distance (transform.position, targetToFollow.transform.position);
					if (distance > 1) {
						speed = distance;
					}
					transform.position = Vector3.MoveTowards (transform.position, targetToFollow.transform.position, Time.deltaTime * speed);
					if (transform.position == targetToFollow.transform.position) {
						smoothReturn = false;
					}
				} else {
					//in this state the player is playing normally
					transform.position = targetToFollow.transform.position;
				}
			} else {
				//else follow the ragdoll
				//in this state the player has dead, he cannot move, and the camera follows the skeleton, until the player chooses play again
				if (smoothGo) {
					float speed = 1;
					float distance = Vector3.Distance (transform.position, targetToFollow.transform.position);
					if (distance > 1) {
						speed = distance;
					}
					transform.position = Vector3.MoveTowards (transform.position, targetToFollow.transform.position - Vector3.up / 1.5f, Time.deltaTime * speed);
					if (transform.position == targetToFollow.transform.position - Vector3.up / 1.5f) {
						smoothGo = false;
					}
				} else {
					transform.position = targetToFollow.transform.position - Vector3.up / 1.5f;
				}
			}
		}
	}
	void LateUpdate(){
		//convert the mouse input in the tilt angle for the camera or the input from the touch screen depending of the settings
		if (cameraCanRotate) {
			x = input.getMovementAxis ("mouse").x;
			y = input.getMovementAxis ("mouse").y;
		} 
		if (!cameraCanRotate || !cameraCanBeUsed) {
			x = 0;
			y = 0;
		}
		isMoving = Mathf.Abs(x) > 0.1f || Mathf.Abs(y) > 0.1f;
		if (isMoving) {
			setLastTimeMoved ();
		}
		//if the use of the accelerometer is enabled, check the rotation of the device, to add its rotation to the x and y values, to roate the camera
		if (input.touchControlsCurrentlyEnabled && settings.useAcelerometer && (pController.aiming || pController.powerActive)) {
			//x rotates y camera axis
			acelerationAxis.x = Input.acceleration.x;
			x += acelerationAxis.x * input.rightTouchSensitivity;
			//y rotates x camera axis
			acelerationAxis.y = lowpass ().z;
			y += acelerationAxis.y * input.rightTouchSensitivity;
			//accelerometer axis in left landscape
			//z righ phone
			//y up phone
			//x out phone
		}
		if (cameraCanRotate && Time.deltaTime!=0 && cameraCanBeUsed) {
			//add the values from the input to the angle applied to the camera
			lookAngle.x = x * rotationSpeed;
			lookAngle.y -= y * rotationSpeed;
		} else {
			lookAngle.x = 0;
		}
		//apply the rotation to the Y axis of the camera
		//transform.Rotate (0, lookAngle.x, 0);
		//when the player is in ground after a jump or a fall, if the camera rotation is higher than the limits, it is returned to a valid rotation
		if (grounded) {
			if (adjustPivotAngle) {
				if (lookAngle.y < currentState.yLimits.x) {
					lookAngle.y += Time.deltaTime * 250;
				}
				if (lookAngle.y > currentState.yLimits.y) {
					lookAngle.y -= Time.deltaTime * 250;
				} else if (lookAngle.y > currentState.yLimits.x && lookAngle.y < currentState.yLimits.y) {
					adjustPivotAngle = false;
				}
			} else {
				lookAngle.y = Mathf.Clamp (lookAngle.y, currentState.yLimits.x, currentState.yLimits.y);
			}
		}
		//restart the rotation to avoid acumulate a high value in the x axis
		else {
			if (lookAngle.y > 360 || lookAngle.y < -360) {
				lookAngle.y = 0;
			}
		}
	}
	void FixedUpdate(){
		if (cameraCanBeUsed) {
			pivot.localRotation = Quaternion.Euler (lookAngle.y, 0, 0);
			transform.Rotate (0, lookAngle.x, 0);
			Slerp (currentState, lerpState, smoothBetweenState);
		}
	}
	public void setCameraToFreeOrLocked(typeOfCamera state, Transform cameraTransform, Transform cameraAxis){
		if (state == typeOfCamera.free) {
			if (cameraType != state) {
				cameraType = state;
				transform.eulerAngles = new Vector3 (transform.eulerAngles.x, mainCameraTransform.eulerAngles.y, transform.eulerAngles.z);
				pivot.eulerAngles = new Vector3 (mainCameraTransform.eulerAngles.x, pivot.eulerAngles.y, pivot.eulerAngles.z);
				mainCameraTransform = mainCamera.transform;
				mainCameraTransform.SetParent (pivot);
				mainCamera.transform.localPosition = lerpState.camPositionOffset;
				mainCamera.transform.localRotation = Quaternion.identity;
				lookAngle = Vector2.zero;
				changeCameraRotationState (true);
				pauseOrPlayCamera (true);
			}
		} 
		if (state == typeOfCamera.locked) {
			cameraType = state;
			changeCameraRotationState (false);
			pauseOrPlayCamera (false);
			mainCameraTransform = cameraAxis;
			mainCamera.transform.SetParent (cameraTransform);
			mainCamera.transform.localPosition = Vector3.zero;
			mainCamera.transform.localRotation = Quaternion.identity;
		}
	}
	public void setCameraTransform(Transform cameraAxis){
		mainCameraTransform = cameraAxis;
	}
	public Transform getCameraTransform(){
		return mainCameraTransform;
	}
	public void death(bool state){
		dead = state;
		headBobManager.playerAliveOrDead (dead);
		if (!firstPersonActive) {
			if (state) {
				smoothFollow = true;
				smoothReturn = false;
				smoothGo = true;
				//this is for the ragdoll, it gets the hips of the player, which is the hips and the parent of the ragdoll
				//the hips is the object that the camera will follow when the player dies, because when this happens, the body of the player is out of the player controller
				//because, while the skeleton of the model will move by the gravity, player controller will not move of its position, due to player has dead
				targetToFollow = hips;
			} else {
				smoothFollow = false;
				smoothReturn = true;
				smoothGo = false;
				targetToFollow = player.transform;
			}
		}
	}
	public void Slerp(cameraStateInfo to, cameraStateInfo from, float time){
		to.Name = from.Name;
		to.camPositionOffset = Vector3.Lerp (to.camPositionOffset, from.camPositionOffset, time);  
		to.pivotPositionOffset = Vector3.Lerp (to.pivotPositionOffset, from.pivotPositionOffset, time);
		to.yLimits.x = Mathf.Lerp (to.yLimits.x, from.yLimits.x, time);
		to.yLimits.y = Mathf.Lerp (to.yLimits.y, from.yLimits.y, time);
	}
	public void setCameraState(string stateName){
		for (int i = 0; i < playerCameraStates.Count; i++) {
			if (playerCameraStates [i].Name == stateName) {
				cameraStateInfo newState = new cameraStateInfo( playerCameraStates [i]);
				lerpState = newState;
				currentStateName = stateName;
				//print (stateName);
			}
		}
	}
	//if the player crouchs, move down the pivot
	public void crouch(int type){
		//check if the camera has been moved away from the player, then the camera moves from its position to the crouch position
		//else the pivot also is moved, but with other parameters
		if (type == 1) {
			if (firstPersonActive) {
				setCameraState ("First Person Crouch");
			} else {
				setCameraState ("Crouch");
			}
			crouching = true;
			moveAwayActive = false;
		} else {
			if (firstPersonActive) {
				setCameraState ("First Person");
			} else {
				setCameraState ("Third Person");
			}
			crouching = false;
		}
	}
	//move away the camera
	public void moveAwayCamera(){
		//check that the player is not in the first person mode or the aim mode
		if (!aiming && !firstPersonActive) {
			bool canMoveAway = false;
			//if the player is crouched, the pivot is also moved, the player get up, but with other parameters
			if(pController.crouch){
				pController.crouching();
			}
			//if the player can not get up due the place where he is, stops the move away action of the pivot
			if(!pController.crouch){
				canMoveAway = true;
			}
			if (canMoveAway) {
				if (moveAwayActive) {
					setCameraState ("Third Person");
					moveAwayActive = false;
				} else {
					setCameraState ("Move Away");
					moveAwayActive = true;
				}
			}
		}
	}
	public void checkFovCoroutine(float targetValue, float speed){
		if(changeFovCoroutine != null){
			StopCoroutine(changeFovCoroutine);
		}
		changeFovCoroutine = StartCoroutine(changeFovValue(targetValue, speed));
	}
	public IEnumerator changeFovValue(float targetValue, float speed){
		while (mainCamera.fieldOfView != targetValue) {
			mainCamera.fieldOfView = Mathf.MoveTowards (mainCamera.fieldOfView, targetValue, Time.deltaTime * speed);
			yield return null;
		}
	}
	//set the zoom state
	public void setZoom(bool state){
		if (!pController.aimingInFirstPerson) {
			//to the fieldofview of the camera, it is added of substracted the zoomvalue
			usingZoomOn = state;
			float targetFov = minFovValue;
			float rotationSpeedTarget = rotationSpeedZoomIn;
			int zoomType = -1;
			if (!usingZoomOn) {
				rotationSpeedTarget = originalRotationSpeed;
				targetFov = originalCameraFov;
				zoomType = 1;
			}
			checkFovCoroutine (targetFov, zoomSpeed);
			//also, change the sensibility of the camera when the zoom is on or off, to control the camera properly
			changeRotationSpeedValue (rotationSpeedTarget);
			player.GetComponent<scannerSystem> ().changeZoom (zoomType);
			if (weaponsManager.carryingWeaponInFirstPerson) {
				weaponsManager.changeWeaponsCameraFov (usingZoomOn, targetFov, zoomSpeed);
			}
		}
	}
	public void disableZoom(){
		if (usingZoomOn) {
			usingZoomOn = false;
			rotationSpeed = originalRotationSpeed;
			player.GetComponent<scannerSystem> ().changeZoom (1);
		}
	}
	public void changeRotationSpeedValue(float newRotationValue){
		rotationSpeed = newRotationValue;
	}
	public void setOriginalRotationSpeed(){
		rotationSpeed = originalRotationSpeed;
	}
	public float getOriginalRotationSpeed(){
		return originalRotationSpeed;
	}
	//move away the camera when the player accelerates his movement velocity in the air, if the power of gravity is activated
	//once the player release shift, find a surface or stop in the air, the camera backs to its position
	//it is just to give the feeling of velocity
	public void changeCameraFov(bool state){
		if (settings.enableMoveAwayInAir) {
			if (pController.aimingInFirstPerson) {
				return;
			}
			//print ("disable zoom when land on ground");
			usingZoomOff = state;
			float targetFov = maxFovValue;
			float targetSpeed = fovChangeSpeed;
			if (!usingZoomOff) {
				targetFov = originalCameraFov;
			}
			if (usingZoomOn) {
				targetSpeed = zoomSpeed;
				if (weaponsManager.carryingWeaponInFirstPerson) {
					weaponsManager.changeWeaponsCameraFov (false, targetFov, targetSpeed);
				}
			}
			checkFovCoroutine(targetFov, targetSpeed);
			disableZoom ();
		}
	}
	//enable or disable the aim mode
	public void activateAiming(otherPowers.sideToAim side){
		aiming = true;
		if (side == otherPowers.sideToAim.Right) {
			setCameraState ("Aim Right");
		} else {
			setCameraState ("Aim Left");
		}
		calibrateAccelerometer ();
	}
	public void deactivateAiming(){
		aiming = false;
		setCameraState ("Third Person");
	}
	//change the aim side to left or right
	public void changeAimSide(int value){
		if (value == 1) {
			setCameraState ("Aim Right");
		} else {
			setCameraState ("Aim Left");
		}
	}
	//if the player is in the air, the camera can rotate 360 degrees, unlike when the player is in the ground where the rotation in x and y is limited
	public void onGroundOrOnAir(bool state){
		grounded = state;
		if (!grounded) {
			adjustPivotAngle = true;
		}
	}
	//set the shake of the camera when the player moves in the air
	public void shakeCamera(){
		settings.shake = true;
	}
	public void stopShakeCamera(){
		settings.shake = false;
		settings.accelerateShaking = false;
	}
	//set first and third person camera position
	public void activateFirstPersonCamera(){
		firstPersonActive = true;
		if (crouching) {
			setCameraState ("First Person Crouch");
		} else {
			setCameraState ("First Person");
		}
		//check if in first person the animator is used
		pController.checkAnimatorIsEnabled (true);	
	}
	public void deactivateFirstPersonCamera(){
		firstPersonActive = false;
		if (crouching) {
			setCameraState ("Crouch");
		} else {
			setCameraState ("Third Person");
		}
		//the third person is enabled, so enable again the animator if it was disabled 
		pController.checkAnimatorIsEnabled (false);	
	}
	//now this funcion is here so it can be called by keyboard or touch button
	public void accelerateShake(bool value){
		settings.accelerateShaking = value;
	}
	//stop the camera rotation or the camera collision detection
	public void changeCameraRotationState(bool state){
		cameraCanRotate = state;
	}
	public void pauseOrPlayCamera(bool state){
		cameraCanBeUsed = state;
	}
	//calibrate the initial accelerometer input according to how the player is holding the touch device
	public void calibrateAccelerometer () {
		if (settings.useAcelerometer) {
			Vector3 wantedDeadZone = Input.acceleration;
			Quaternion rotateQuaternion = Quaternion.FromToRotation (new Vector3 (1, 0, 0), wantedDeadZone);
			//create identity matrix
			Matrix4x4 matrix = Matrix4x4.TRS (Vector3.zero, rotateQuaternion, Vector3.one);
			//get the inverse of the matrix
			calibrationMatrix = matrix.inverse;
		}
	}
	//get the accelerometer value, taking in account that the device is holing in left scape mode, with the home button in the right side
	Vector3 getAccelerometer ( Vector3 accelerator ) {
		Vector3 accel = calibrationMatrix.MultiplyVector(accelerator);
		return accel;
	}
	//get the accelerometer value more smoothly
	Vector3 lowpass(){
		float LowPassFilterFactor = AccelerometerUpdateInterval / LowPassKernelWidthInSeconds; // tweakable
		lowPassValue = Vector3.Lerp(lowPassValue, getAccelerometer(Input.acceleration), LowPassFilterFactor);
		return lowPassValue;
	}
	public bool isCameraRotating(){
		return isMoving;
	}
	public void setLastTimeMoved(){
		lastTimeMoved = Time.time;
	}
	public float getLastTimeMoved(){
		return lastTimeMoved;
	}
	public void playOrPauseHeadBob(bool state){
		headBobManager.playOrPauseHeadBob (state);
	}
	public void getCameraComponents(){
		//get all the elements neccesary, since the start function has not being called yet
		mainCameraTransform = Camera.main.transform;
		pivot = mainCameraTransform.parent;
		player=GameObject.Find("Player Controller");
		pController = player.GetComponent<playerController> ();
		powers = player.GetComponent<otherPowers> ();
		gravity = player.GetComponent<changeGravity> ();
		gravity.settings.arrow=GameObject.Find("characterArrow");
		pController.animator=player.GetComponent<Animator>();
		gravity.settings.meshCharacter=player.GetComponentInChildren<SkinnedMeshRenderer>();
		headBobManager = mainCameraTransform.GetComponent<headBob> ();
		damageInScreenManager = player.GetComponent<damageInScreen> ();
		jetpackSystemManager = player.GetComponent<jetpackSystem> ();
		weaponsManager = player.GetComponent<playerWeaponsManager> ();
	}
	//set in editor mode, without game running, the camera position, starting the game in fisrt person view
	public void setFirstPersonEditor(){
		//check that the player is not in this mode already and that the game is not being played
		if (!firstPersonActive && !Application.isPlaying) {
			//get all the elements neccesary, since the start function has not being called yet
			getCameraComponents ();
			//set the parameters correctly, so there won't be issues
			gravity.settings.firstPersonView = true;
			//disable the player's meshes
			gravity.settings.meshCharacter.enabled = false;
			gravity.settings.arrow.transform.GetChild (0).GetComponent<MeshRenderer> ().enabled = false;
			headBobManager.setFirstOrThirdMode (true);
			//put the camera in the correct position
			activateFirstPersonCamera ();
			//this is the first person view, so move the camera position directly to the first person view
			mainCameraTransform.localPosition = lerpState.camPositionOffset;
			pivot.localPosition = lerpState.pivotPositionOffset;
			//in this mode the player hasn't to aim, so enable the grab objects function
			player.GetComponent<grabObjects> ().aiming = true;
			powers.aim = true;
			//change the textures in the touch button
			powers.settings.buttonShoot.GetComponent<RawImage> ().texture = powers.settings.buttonShootTexture;
			//change the position where the projectiles are instantiated, in this case a little below the camera
			powers.shootsettings.shootZone.transform.parent = mainCameraTransform.transform;
			powers.shootsettings.shootZone.transform.localPosition = -transform.up;
			powers.shootsettings.shootZone.transform.localRotation = Quaternion.identity;

			damageInScreenManager.pauseOrPlayDamageInScreen (true);
			jetpackSystemManager.enableOrDisableJetPackMesh (false);
			weaponsManager.getPlayerWeaponsManagerComponents (true);
			weaponsManager.setWeaponsParent (true);
			headBobManager.setFirstOrThirdMode (true);

			updateCameraComponents ();
		}
	}
	//set in editor mode, without game running, the camera position, starting the game in third person view
	public void setThirdPersonEditor(){
		//check that the player is not in this mode already and that the game is not being played
		if (firstPersonActive && !Application.isPlaying) {
			//get all the elements neccesary, since the start function has not being called yet
			getCameraComponents ();
			//set the parameters correctly, so there won't be issues
			gravity.settings.firstPersonView = false;
			//enable the player's meshes
			gravity.settings.meshCharacter.enabled = true;
			gravity.settings.arrow.transform.GetChild (0).GetComponent<MeshRenderer> ().enabled = true;
			headBobManager.setFirstOrThirdMode (false);
			//put the camera in the correct position
			deactivateFirstPersonCamera ();
			mainCameraTransform.localPosition = lerpState.camPositionOffset;
			pivot.localPosition = lerpState.pivotPositionOffset;
			//set the changes in grabObjects and other powers
			player.GetComponent<grabObjects> ().aiming = false;
			powers.aimsettings.aiming = false;
			powers.aim = false;
			//change the textures in the touch controls
			powers.settings.buttonShoot.GetComponent<RawImage> ().texture = powers.settings.buttonKickTexture;
			//set the position where the projectiles are instantiated, in this case, in the right hand of the player
			powers.aimsettings.handActive = powers.aimsettings.rightHand;
			powers.shootsettings.shootZone.transform.parent = powers.aimsettings.handActive.transform;
			powers.shootsettings.shootZone.transform.localPosition = Vector3.zero;
			powers.shootsettings.shootZone.transform.localRotation = Quaternion.identity;

			damageInScreenManager.pauseOrPlayDamageInScreen (false);
			jetpackSystemManager.enableOrDisableJetPackMesh (true);
			weaponsManager.getPlayerWeaponsManagerComponents (false);
			weaponsManager.setWeaponsParent (false);
			headBobManager.setFirstOrThirdMode (false);

			updateCameraComponents ();
		}
	}
	void updateCameraComponents(){
		#if UNITY_EDITOR
		EditorUtility.SetDirty (GetComponent<playerCamera>() );
		EditorUtility.SetDirty (pController);
		EditorUtility.SetDirty (powers);
		EditorUtility.SetDirty (gravity);
		EditorUtility.SetDirty (headBobManager);
		EditorUtility.SetDirty (player.GetComponent<grabObjects> ());
		EditorUtility.SetDirty (player.GetComponent<damageInScreen> ());
		EditorUtility.SetDirty (player.GetComponent<jetpackSystem> ());
		EditorUtility.SetDirty ( player.GetComponent<playerWeaponsManager> ());
		#endif
	}
	//draw the lines of the pivot camera in the editor
	void OnDrawGizmos(){
		DrawGizmos ();
	}
	void OnDrawGizmosSelected(){
		DrawGizmos ();
	}
	void DrawGizmos(){
		if (settings.showCameraGizmo && !Application.isPlaying) {
			if (pivot && mainCameraTransform) {
				for (int i = 0; i < playerCameraStates.Count; i++) {
					if(playerCameraStates[i].showGizmo){
						Gizmos.color = playerCameraStates[i].gizmoColor;
						Vector3 pivotPosition = transform.position + playerCameraStates [i].pivotPositionOffset;
						Vector3 cameraPosition = pivotPosition + playerCameraStates [i].camPositionOffset;
						Gizmos.DrawSphere (cameraPosition, 0.1f);
						Gizmos.color = playerCameraStates[i].gizmoColor;
						Gizmos.DrawSphere (pivotPosition, 0.1f);
						Gizmos.color = Color.white;
						Gizmos.DrawLine (cameraPosition, pivotPosition);
						Gizmos.DrawLine (pivotPosition, transform.position);
					}
				}
			} else {
				mainCameraTransform = Camera.main.transform;
				pivot = mainCameraTransform.parent;
			}
		}
	}	
	//a group of parameters to configure the shake of the camera
	[System.Serializable]
	public class cameraSettings{
		public LayerMask layer;
		public bool useAcelerometer;
		public bool zoomEnabled;
		public bool moveAwayCameraEnabled;
		public bool enableMoveAwayInAir = true;
		public bool enableShakeCamera = true;
		public bool showCameraGizmo = true;
		[HideInInspector] public bool shake = false;
		[HideInInspector] public bool accelerateShaking = false;
	}
}