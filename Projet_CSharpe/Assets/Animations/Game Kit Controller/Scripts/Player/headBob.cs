using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class headBob : MonoBehaviour {
	public bool headBobEnabled;
	public bool firstPersonMode;
	public bool externalShake;
	public bool headBobCanBeUsed = true;
	public string currentState;
	public string externalForceStateName;
	public bool useDynamicIdle;
	public string dynamicIdleName;
	public float timeToActiveDynamicIdle;
	public List<bobStates> bobStatesList = new List<bobStates> ();
	public enum bobTransformType {onlyPosition, onlyRotation, both, none};
	public enum viewTypes {firstPerson, thirdPerson, both};
	public float resetSpeed;
	public Vector3 jumpStartMaxIncrease;
	public float jumpStartSpeed;
	public Vector3 jumpEndMaxDecrease;
	public float jumpEndSpeed;
	public float jumpResetSpeed;
	public bool headBobPaused = false;
	bool stateChanged = false;
	bool checkResetCamera;
	Coroutine coroutineStartJump;
	Coroutine coroutineEndJump;
	Coroutine coroutineToStop;
	Coroutine externalForceCoroutine;
	Coroutine waitToActiveCoroutine;
	Vector3 initialTargetEul;
	Vector3 initialTargetPos;		
	Transform mainCamera;		
	GameObject player;
	changeGravity gravity;
	bobStates playerBobState;
	bool dead;
	float externalShakeDuration;

	void Start () {
		//get the object to shake, in this case the main camera
		mainCamera = transform;
		player = GameObject.Find ("Player Controller");
		gravity = player.GetComponent<changeGravity> ();
		//set the position and rotation to reset the camera transform
		initialTargetEul = Vector3.zero;
		initialTargetPos = Vector3.zero;
		//set the initial state of the player
		playerBobState=new bobStates();
		setState(currentState);
	}
	void Update () {
		//if headbod enabled, check the current state
		if (headBobEnabled && headBobCanBeUsed) {
			if (playerBobState.bobTransformStyle != bobTransformType.none) {
				if (canBeUsed ()) {
					movementBob (playerBobState);
					if (stateChanged) {
						stateChanged = false;
					}

				}
			} else {
				if (!stateChanged) {
					if (!headBobPaused) {
						stopBobTransform ();
					}
					stateChanged = true;
				}
			}
		}
	}
	public bool canBeUsed(){
		//if the camera is not being moved from the third to first move or viceversa,
		//or the camera is in first person mode and the current bobstate is only applied in first mode, 
		//or the camera is in third person mode and the current bobstate is only applied in third mode,
		//or the in the current bob state the camera is shake in both modes, then
		if (playerBobState.enableBobIn == viewTypes.both ||
		    ((playerBobState.enableBobIn == viewTypes.firstPerson && gravity.settings.firstPersonView && firstPersonMode) ||
		    (playerBobState.enableBobIn == viewTypes.thirdPerson && !gravity.settings.firstPersonView)) &&
		    !headBobPaused) {
			return true;
		}
		return false;
	}

	public void stopBobTransform(){
		//print ("parar transform");
		if (coroutineToStop != null) {
			StopCoroutine (coroutineToStop);
		}
		coroutineToStop = StartCoroutine (resetCameraTransform ());
	}
	public void stopBobRotation(){
		//print ("parar rotation");
		if (coroutineToStop != null) {
			StopCoroutine (coroutineToStop);
		}
		coroutineToStop = StartCoroutine (resetCameraRotation ());
	}

	public void setExternalShakeDuration(){
		externalShake = true;
		if (externalForceCoroutine != null) {
			StopCoroutine (externalForceCoroutine);
		}
		externalForceCoroutine = StartCoroutine (setExternalShakeDurationCoroutine ());
	}
	IEnumerator setExternalShakeDurationCoroutine(){
		yield return new WaitForSeconds (externalShakeDuration);
		externalShake = false;
		if (!firstPersonMode) {
			stopBobRotation ();
			stateChanged = true;
		}
		yield return null;			
	}
	//set a state in the current player state
	public void setState(string stateName){
		//search the state recieved
		if ((stateName != playerBobState.Name && !externalShake) || stateName == externalForceStateName || (!externalShake && useDynamicIdle && stateName == dynamicIdleName)) {
			for (int i = 0; i < bobStatesList.Count; i++) {
				if (bobStatesList [i].Name == stateName) {
					//if found, set the state values, and the enable this state as the current state
					playerBobState = bobStatesList [i];
					currentState = bobStatesList [i].Name;
					playerBobState.isCurrentState = true;
				} else {
					bobStatesList [i].isCurrentState = false;
				}
			}
			if (firstPersonMode) {
				if (stateName == "Jump Start") {
					jumpStarted ();
				}
				if (stateName == "Jump End") {
					jumpEnded ();
				}
			}
		}
	}
	public void setFirstOrThirdHeadBobView(bool state){
		//if the camera is in first person view, then check the headbob
		firstPersonMode = state;
		//if the camera is set back to the third person mode, reset the camera rotation
		if (!firstPersonMode) {
			//stop the previous coroutine and play the reset camera rotation coroutine
			if (coroutineToStop != null) {
				StopCoroutine (coroutineToStop);
			}
			coroutineToStop = StartCoroutine (resetCameraRotation ());
		}
	}
	public void setFirstOrThirdMode(bool state){
		firstPersonMode = state;
	}
	public void setShotShakeState(IKWeaponSystem.weaponShotShakeInfo shotShakeInfo){
		setState (externalForceStateName);
		if (firstPersonMode) {
			playerBobState.eulAmount = shotShakeInfo.shakeRotation;
			playerBobState.posAmount = shotShakeInfo.shakePosition;
			playerBobState.posSmooth = shotShakeInfo.shakeSmooth;
			playerBobState.eulSmooth = shotShakeInfo.shakeSmooth;
			playerBobState.bobTransformStyle = bobTransformType.both;
		} else {
			playerBobState.eulAmount = shotShakeInfo.shakeRotation;
			playerBobState.posAmount = Vector3.zero;
			playerBobState.posSmooth = 0;
			playerBobState.eulSmooth = shotShakeInfo.shakeSmooth;
			playerBobState.bobTransformStyle = bobTransformType.onlyRotation;
		}
		playerBobState.posSpeed = Vector3.one * shotShakeInfo.shotForce;
		playerBobState.eulSpeed = Vector3.one * shotShakeInfo.shotForce;
		externalShakeDuration = shotShakeInfo.shakeDuration;
		setExternalShakeDuration ();
	}
	public void setExternalShakeState(externalShakeInfo shakeInfo){
		setState (externalForceStateName);
		if (firstPersonMode) {
			playerBobState.eulAmount = shakeInfo.shakeRotation;
			playerBobState.posAmount = shakeInfo.shakePosition;
			playerBobState.posSmooth = shakeInfo.shakePositionSmooth;
			playerBobState.eulSmooth = shakeInfo.shakeRotationSmooth;
			playerBobState.bobTransformStyle = bobTransformType.both;
		} else {
			playerBobState.eulAmount = shakeInfo.shakeRotation;
			playerBobState.posAmount = Vector3.zero;
			playerBobState.posSmooth = 0;
			playerBobState.eulSmooth = shakeInfo.shakeRotationSmooth;
			playerBobState.bobTransformStyle = bobTransformType.onlyRotation;
		}
		playerBobState.posSpeed = shakeInfo.shakePositionSpeed;
		playerBobState.eulSpeed = shakeInfo.shakeRotationSpeed;
		externalShakeDuration = shakeInfo.shakeDuration;
		setExternalShakeDuration ();
	}
	public void playerAliveOrDead(bool state){
		dead = state;
		headBobCanBeUsed = !dead;
		if (dead) {
			stateChanged = true;
			headBobPaused = false;
		}
	}
	public void playOrPauseHeadBob(bool state){
		headBobCanBeUsed = state;
	}
	//check the info of the current state, to apply rotation, translation, both or anything according to the parameters of the botState
	void movementBob(bobStates state){
		bool changePos = false;
		bool changeRot = false;
		//check the type of shake
		if (playerBobState.bobTransformStyle == bobTransformType.onlyPosition) {
			changePos = true;
		} else if (playerBobState.bobTransformStyle == bobTransformType.onlyRotation) {
			changeRot = true;
		} else if (playerBobState.bobTransformStyle == bobTransformType.both) {
			changePos = true;
			changeRot = true;
		}
		//apply translation
		if (changePos) {
			float posTargetX = Mathf.Sin (Time.time * state.posSpeed.x) * state.posAmount.x;
			float posTargetY = Mathf.Sin (Time.time * state.posSpeed.y) * state.posAmount.y;
			float posTargetZ = Mathf.Cos (Time.time * state.posSpeed.z) * state.posAmount.z;
			Vector3 posTarget = new Vector3 (posTargetX, posTargetY, posTargetZ);
			mainCamera.localPosition = Vector3.Lerp (mainCamera.localPosition, posTarget, Time.deltaTime * state.posSmooth);
		}
		//apply rotation
		if(changeRot){
			float eulTargetX = Mathf.Sin (Time.time * state.eulSpeed.x) * state.eulAmount.x;
			float eulTargetY = Mathf.Sin (Time.time * state.eulSpeed.y) * state.eulAmount.y;
			float eulTargetZ = Mathf.Cos (Time.time * state.eulSpeed.z) * state.eulAmount.z;
			Vector3 eulTarget = new Vector3(eulTargetX, eulTargetY, eulTargetZ);
			mainCamera.localRotation = Quaternion.Lerp (mainCamera.localRotation, Quaternion.Euler(eulTarget), Time.deltaTime * state.eulSmooth);
		}
	}
	public void jumpStarted(){
		//if the player jumps, stop the current coroutine, and play the jump coroutine
		if (coroutineEndJump != null) {
			StopCoroutine (coroutineEndJump);
		}
		coroutineStartJump = StartCoroutine(startJump());
	}
	public void jumpEnded(){
		//if the player is in firts person view and the camera is not moving from first to third mode, then
		if (gravity.settings.firstPersonView && headBobEnabled && !dead) {
			//if the player reachs the ground after jump, stop the current coroutine, and play the landing coroutine
			if (coroutineStartJump != null) {
				StopCoroutine (coroutineStartJump);
			}
			coroutineEndJump = StartCoroutine (endJump ());
		}
	}
	IEnumerator startJump(){
		//walk or run shakes are blocked
		headBobPaused = true;
		float i = 0.0f;
		float rate = jumpStartSpeed;
		//add to the current rotation the jumpStartMaxIncrease value, when the player jumps
		Vector3 targetEUL = new Vector3(mainCamera.localEulerAngles.x - jumpStartMaxIncrease.x, mainCamera.localEulerAngles.y - jumpStartMaxIncrease.y, mainCamera.localEulerAngles.z - jumpStartMaxIncrease.z);
		//store the current rotation
		Quaternion currentQ = mainCamera.localRotation;
		//store the target rotation
		Quaternion targetQ = Quaternion.Euler(targetEUL);
		while(i < 1.0f){
			i += Time.deltaTime * rate;
			mainCamera.localRotation = Quaternion.Lerp (currentQ, targetQ, i);
			yield return null;
		}
	}
	IEnumerator endJump(){
		float i = 0.0f;
		float rate = jumpEndSpeed;
		//add to the current rotation the jumpMaxDrecrease value, when the player touch the ground again after jumping
		Vector3 targetEUL = new Vector3(mainCamera.localEulerAngles.x + jumpEndMaxDecrease.x, mainCamera.localEulerAngles.y + jumpEndMaxDecrease.y, mainCamera.localEulerAngles.z + jumpEndMaxDecrease.z);
		//store the current rotation
		Quaternion currentQ = mainCamera.localRotation;
		//store the target rotation
		Quaternion targetQ = Quaternion.Euler(targetEUL);
		while(i < 1.0f){
			i += Time.deltaTime * rate;
			mainCamera.localRotation = Quaternion.Lerp (currentQ, targetQ, i);
			yield return null;
		}
		//reset again the rotation of the camera
		i = 0;
		rate = jumpResetSpeed;
		currentQ = mainCamera.localRotation;
		targetQ = Quaternion.Euler(initialTargetEul);
		while(i < 1.0f){
			i += Time.deltaTime * rate;
			mainCamera.localRotation = Quaternion.Lerp (currentQ, targetQ, i);
			yield return null;
		}
		//the jump state has finished, so the camera can be shaked again
		headBobPaused = false;
	}
	IEnumerator resetCameraTransform(){
		if (firstPersonMode) {
			float i = 0.0f;
			float rate = resetSpeed;
			//store the current rotation
			Quaternion currentQ = mainCamera.localRotation;
			//store the current position
			Vector3 currentPos = mainCamera.localPosition;
			while (i < 1.0f) {
				//reset the position and rotation of the camera to 0,0,0
				i += Time.deltaTime * rate;
				mainCamera.localRotation = Quaternion.Lerp (currentQ, Quaternion.Euler (initialTargetEul), i);
				mainCamera.localPosition = Vector3.Lerp (currentPos, initialTargetPos, i);
				yield return null;
			}
			headBobPaused = false;
		}
	}
	IEnumerator resetCameraRotation(){
		float i = 0.0f;
		float rate = resetSpeed;
		//store the current rotation
		Quaternion currentQ = mainCamera.localRotation;
		while(i < 1.0f){
			//reset the rotation of the camera to 0,0,0
			i += Time.deltaTime * rate;
			mainCamera.localRotation = Quaternion.Lerp (currentQ, Quaternion.Euler(initialTargetEul), i);
			yield return null;
		}
	}
	[System.Serializable]
	public class bobStates{
		public string Name;
		public bobTransformType bobTransformStyle;
		public viewTypes enableBobIn;
		public Vector3 posAmount;
		public Vector3 posSpeed;
		public float posSmooth;
		public Vector3 eulAmount;
		public Vector3 eulSpeed;
		public float eulSmooth;	
		public bool isCurrentState;
	}
	[System.Serializable]
	public class externalShakeInfo{
		public Vector3 shakePosition;
		public Vector3 shakePositionSpeed;
		public float shakePositionSmooth;
		public Vector3 shakeRotation;
		public Vector3 shakeRotationSpeed;
		public float shakeRotationSmooth;	
		public float shakeDuration;
	}
}