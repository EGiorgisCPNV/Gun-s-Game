using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class vehicleCameraShake : MonoBehaviour {
	public List<bobStates> bobStatesList = new List<bobStates> ();
	public bool headBobEnabled;
	public bool shakingActive;
	public float ResetSpeed;
	public bobStates playerBobState;
	public bool externalShake;
	public string externalForceStateName;
	Vector3 initialTargetEul;
	Transform mainCamera;		
	Coroutine coroutineToStop;
	Coroutine externalForceCoroutine;
	float externalShakeDuration;

	void Start () {
		//set the position and rotation to reset the camera transform
		initialTargetEul = Vector3.zero;
	}
	//set a state in the current player state
	public void startShake(string name){
		//search the state recieved
		for (int i = 0; i < bobStatesList.Count; i++) {
			if (bobStatesList [i].Name == name) {
				//if found, set the state values, and the enable this state as the current state
				playerBobState = bobStatesList [i];
				bobStatesList [i].isCurrentState = true;
				//print ("New Shake State " + playerBobState.Name);
			} else {
				//disable all the other states
				bobStatesList [i].isCurrentState = false;
			}
		}
		shakingActive = true;
	}
	void Update () {
		//if headbod enabled, check the current state
		if (headBobEnabled && shakingActive && playerBobState.stateEnabled) {
			movementBob (playerBobState);
		}
	}
	public void stopShake(){
		shakingActive = false;
		if(coroutineToStop != null){
			StopCoroutine(coroutineToStop);
		}
		coroutineToStop = StartCoroutine(resetCameraTransform());
		//print ("stop shake");
	}
	//check the info of the current state, to apply rotation, translation, both or anything according to the parameters of the botState
	void movementBob(bobStates state){
		float eulTargetX = Mathf.Sin (Time.time * state.eulSpeed.x) * state.eulAmount.x;
		float eulTargetY = Mathf.Cos (Time.time * state.eulSpeed.y) * state.eulAmount.y;
		float eulTargetZ = Mathf.Sin (Time.time * state.eulSpeed.z) * state.eulAmount.z;
		Vector3 eulTarget = new Vector3 (eulTargetX, eulTargetY, eulTargetZ);
		mainCamera.localRotation = Quaternion.Lerp (mainCamera.localRotation, Quaternion.Euler (eulTarget), Time.deltaTime * state.eulSmooth);
	}
	IEnumerator resetCameraTransform(){
		float i = 0.0f;
		float rate = ResetSpeed;
		//store the current rotation
		Quaternion currentQ = mainCamera.localRotation;
		while (i < 1.0f) {
			//reset the position and rotation of the camera to 0,0,0
			i += Time.deltaTime * rate;
			mainCamera.localRotation = Quaternion.Lerp (currentQ, Quaternion.Euler (initialTargetEul), i);
			yield return null;
		}
	}
	public void getCurrentCameraTransform(Transform currentCameraTransform){
		mainCamera = currentCameraTransform;
	}
	public void setExternalShakeState(headBob.externalShakeInfo shakeInfo){
		startShake (externalForceStateName);
		playerBobState.eulAmount = shakeInfo.shakeRotation;
		playerBobState.eulSmooth = shakeInfo.shakeRotationSmooth;
		playerBobState.eulSpeed = shakeInfo.shakeRotationSpeed;
		externalShakeDuration = shakeInfo.shakeDuration;
		setExternalShakeDuration ();
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
		stopShake ();
		yield return null;			
	}
	[System.Serializable]
	public class bobStates{
		public string Name;
		public Vector3 eulAmount;
		public Vector3 eulSpeed;
		public float eulSmooth;	
		public bool stateEnabled;
		public bool isCurrentState;
	}
}