using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public class IKSystem : MonoBehaviour {
	public aimMode currentAimMode;
	public Transform rightHandIKPos;
	public Transform leftHandIKPos;
	public Transform currentHandIKPos;
	public LayerMask layer;
	[Range(1,10)] public float IKSpeed;
	public float IKWeaponsCollisionSpeed;
	public bool usingWeapons;
	public bool usingArms;
	public bool driving;
	public bool usingZipline;
	public bool usingJetpack;
	public bool usingFlyingMode;
	public bool showGizmo;
	public IKSettings settings;
	public enum aimMode{
		hands, weapons
	}
	IKDrivingSystem.IKDrivingInformation IKDrivingSettings;
	IKWeaponInfo IKWeaponsSettings;
	zipline.IKZiplineInfo IKZiplineSettings;
	jetpackSystem.IKJetpackInfo IKJetpackSettings;
	flySystem.IKFlyInfo IKFlyingModeSettings;
	AvatarIKGoal currentHand;
	Animator animator;
	float IKWeight;
	float IKWeightTargetValue;
	float originalDist;
	float hitDist;
	float currentDist;
	public Vector3 originalWeaponDist;
	public Vector3 currentWeaponDist;
	public Vector3 hitWeaponDist;
	Ray ray;
	RaycastHit hit;
	playerWeaponsManager weaponsManager;
	Coroutine leftHandMovement;
	Coroutine rightHandMovement;
	Coroutine powerHandRecoil;
	float originalCurrentDist;
	bool disableWeapons;
	int handsDisabled;
	float currentHeadWeight;
	float headWeightTarget;

	void Start (){
		animator = GetComponent<Animator>();
		weaponsManager = GetComponent<playerWeaponsManager> ();
	}
	void Update(){
		if (!driving && usingArms){
			//change the current weight of the ik 
			if (IKWeight != IKWeightTargetValue) {
				IKWeight = Mathf.MoveTowards (IKWeight, IKWeightTargetValue, Time.deltaTime * IKSpeed);
			}
			if (IKWeight > 0) {
				//if the raycast detects a surface, get the distance to it
				if (Physics.Raycast (currentHandIKPos.position, transform.forward, out hit, 3, layer)) {
					if (!hit.collider.isTrigger) {
						if (hit.distance < originalDist) {
							hitDist = hit.distance;
						} else {
							hitDist = originalDist;
						}
					}
				}
				//else, set the original distance
				else {
					hitDist = originalDist;
				}
				hitDist = Mathf.Clamp (hitDist, 0.1f, originalDist);
				//set the correct position of the current hand to avoid cross any collider with it
				currentDist = Mathf.Lerp (currentDist, hitDist, Time.deltaTime * IKSpeed);
				currentHandIKPos.transform.localPosition = new Vector3 (currentHandIKPos.transform.localPosition.x, currentDist, currentHandIKPos.transform.localPosition.z);
			}
			if (IKWeight == 0) {
				usingArms = false;
			}
		}
		if (usingWeapons) {
			if (disableWeapons) {
				handsDisabled = 0;
			}
			for (int j = 0; j < IKWeaponsSettings.handsInfo.Count; j++) {
				if (IKWeaponsSettings.handsInfo [j].limb == AvatarIKGoal.LeftHand) {
					if (IKWeaponsSettings.handsInfo [j].HandIKWeight != IKWeaponsSettings.handsInfo [j].targetValue) {
						IKWeaponsSettings.handsInfo [j].HandIKWeight = Mathf.MoveTowards (IKWeaponsSettings.handsInfo [j].HandIKWeight, IKWeaponsSettings.handsInfo [j].targetValue, Time.deltaTime * IKSpeed);
					}
				}
				if (IKWeaponsSettings.handsInfo [j].limb == AvatarIKGoal.RightHand) {
					if (IKWeaponsSettings.handsInfo [j].HandIKWeight != IKWeaponsSettings.handsInfo [j].targetValue) {
						IKWeaponsSettings.handsInfo [j].HandIKWeight = Mathf.MoveTowards (IKWeaponsSettings.handsInfo [j].HandIKWeight, IKWeaponsSettings.handsInfo [j].targetValue, Time.deltaTime * IKSpeed);
					}
				}
				if (IKWeaponsSettings.handsInfo [j].elbowInfo.elbowIKWeight != IKWeaponsSettings.handsInfo [j].elbowInfo.targetValue) {
					IKWeaponsSettings.handsInfo [j].elbowInfo.elbowIKWeight = Mathf.MoveTowards (IKWeaponsSettings.handsInfo [j].elbowInfo.elbowIKWeight, IKWeaponsSettings.handsInfo [j].elbowInfo.targetValue, Time.deltaTime * IKSpeed);
				}
				if (disableWeapons) {
					if (IKWeaponsSettings.handsInfo [j].HandIKWeight == 0) {
						handsDisabled++;
					}
				}
			}
			if (disableWeapons) {					
				if (handsDisabled == 2) {
					disableWeapons = false;
					handsDisabled = 0;
					setUsingWeaponsState (false);
				}
			}
		}
		if (usingZipline || usingJetpack || usingFlyingMode) {
			if (IKWeight != IKWeightTargetValue) {
				IKWeight = Mathf.MoveTowards (IKWeight, IKWeightTargetValue, Time.deltaTime * IKSpeed);
			}
			if (IKWeight == 0) {
				usingJetpack = false;
				usingFlyingMode = false;
				IKJetpackSettings = null;
				IKFlyingModeSettings = null;
			}
		}
	}
	void FixedUpdate(){
		if (usingWeapons && weaponsManager.currentIKWeapon.checkSurfaceCollision && weaponsManager.aimingInThirdPerson) {
			//if the raycast detects a surface, get the distance to it
			Debug.DrawRay(IKWeaponsSettings.weapon.transform.position,
				IKWeaponsSettings.weapon.transform.forward*(weaponsManager.currentIKWeapon.weaponLenght +originalWeaponDist.z),Color.yellow);
			if (Physics.Raycast (IKWeaponsSettings.weapon.transform.position + IKWeaponsSettings.weapon.transform.forward * weaponsManager.currentIKWeapon.weaponLenght, 
				     transform.forward, out hit, 2, layer)) {
				if (!hit.collider.isTrigger) {
					if (hit.distance < (originalWeaponDist.z + weaponsManager.currentIKWeapon.weaponLenght)) {
						Debug.DrawRay (IKWeaponsSettings.weapon.transform.position + IKWeaponsSettings.weapon.transform.forward * weaponsManager.currentIKWeapon.weaponLenght,
							transform.forward * hit.distance, Color.red);
						Debug.DrawRay (IKWeaponsSettings.weapon.transform.position + IKWeaponsSettings.weapon.transform.forward * weaponsManager.currentIKWeapon.weaponLenght,
							IKWeaponsSettings.weapon.transform.forward * hit.distance, Color.red);
						hitWeaponDist.z = hit.distance;
						hitWeaponDist.y = weaponsManager.currentIKWeapon.weaponLenght - hit.distance;
					} 
					else{
//						hitWeaponDist.z = originalWeaponDist.z;
//						hitWeaponDist.y = 0;
					}
				}
			} else {
				//else, set the original distance
				hitWeaponDist.z = originalWeaponDist.z;
				hitWeaponDist.y = 0;
			}
			hitWeaponDist.z = Mathf.Clamp (hitWeaponDist.z, 0, originalWeaponDist.z);
			currentWeaponDist.z = Mathf.Lerp (currentWeaponDist.z, hitWeaponDist.z, Time.deltaTime * IKWeaponsCollisionSpeed);
			hitWeaponDist.y = Mathf.Clamp (hitWeaponDist.y, 0, 0.4f);
			currentWeaponDist.y = Mathf.Lerp (currentWeaponDist.y, hitWeaponDist.y, Time.deltaTime * IKWeaponsCollisionSpeed);
			IKWeaponsSettings.weapon.transform.localPosition = Vector3.MoveTowards (IKWeaponsSettings.weapon.transform.localPosition, 
				new Vector3 ( IKWeaponsSettings.weapon.transform.localPosition.x, 
					originalWeaponDist.y - currentWeaponDist.y, currentWeaponDist.z), Time.deltaTime * IKWeaponsCollisionSpeed);
		}
	}
	void OnAnimatorIK(){
		if (!driving && !usingWeapons && IKWeight > 0 && usingArms) {
			//set the current hand target position and rotation
			animator.SetIKPositionWeight (currentHand, IKWeight);
			animator.SetIKRotationWeight (currentHand, IKWeight);  
			animator.SetIKPosition (currentHand, currentHandIKPos.position);
			animator.SetIKRotation (currentHand, currentHandIKPos.rotation);      
		}
		//if the player is driving, set all the position and rotations of every player's limb
		if (driving) {
			for (int i = 0; i < IKDrivingSettings.IKDrivingPos.Count; i++) {
				//hands and foots
				animator.SetIKPositionWeight (IKDrivingSettings.IKDrivingPos [i].limb, 1);
				animator.SetIKRotationWeight (IKDrivingSettings.IKDrivingPos [i].limb, 1);  
				animator.SetIKPosition (IKDrivingSettings.IKDrivingPos [i].limb, IKDrivingSettings.IKDrivingPos [i].position.position);
				animator.SetIKRotation (IKDrivingSettings.IKDrivingPos [i].limb, IKDrivingSettings.IKDrivingPos [i].position.rotation);   
			}
			//knees and elbows
			for (int i = 0; i < IKDrivingSettings.IKDrivingKneePos.Count; i++) {
				animator.SetIKHintPositionWeight (IKDrivingSettings.IKDrivingKneePos [i].knee, 1);
				animator.SetIKHintPosition (IKDrivingSettings.IKDrivingKneePos [i].knee, IKDrivingSettings.IKDrivingKneePos [i].position.position);
			}
			//comment/discomment these two lines to edit correctly the body position of the player ingame.
			transform.position = IKDrivingSettings.bodyPosition.position;
			transform.rotation = IKDrivingSettings.bodyPosition.rotation;
			//set the rotation of the upper body of the player according to the steering direction
			if (IKDrivingSettings.steerDirecion) {
				Vector3 lookDirection = IKDrivingSettings.steerDirecion.transform.forward + IKDrivingSettings.steerDirecion.transform.position;
				animator.SetLookAtPosition (lookDirection);
				animator.SetLookAtWeight (settings.weight, settings.bodyWeight, settings.headWeight, settings.eyesWeight, settings.clampWeight);
			}
		}
		if (!driving && usingWeapons) {
			for (int i = 0; i < IKWeaponsSettings.handsInfo.Count; i++) {
				animator.SetIKPositionWeight (IKWeaponsSettings.handsInfo [i].limb, IKWeaponsSettings.handsInfo [i].HandIKWeight);
				animator.SetIKRotationWeight (IKWeaponsSettings.handsInfo [i].limb, IKWeaponsSettings.handsInfo [i].HandIKWeight);  
				if (IKWeaponsSettings.handsInfo [i].transformFollowByHand) {
					animator.SetIKPosition (IKWeaponsSettings.handsInfo [i].limb, IKWeaponsSettings.handsInfo [i].transformFollowByHand.position);
					animator.SetIKRotation (IKWeaponsSettings.handsInfo [i].limb, IKWeaponsSettings.handsInfo [i].transformFollowByHand.rotation); 
				}
				animator.SetIKHintPositionWeight (IKWeaponsSettings.handsInfo[i].elbowInfo.elbow, IKWeaponsSettings.handsInfo[i].elbowInfo.elbowIKWeight);
				animator.SetIKHintPosition (IKWeaponsSettings.handsInfo[i].elbowInfo.elbow, IKWeaponsSettings.handsInfo[i].elbowInfo.position.position);
			}
			if (weaponsManager.currentIKWeapon.headLookWhenAiming) {
				if (weaponsManager.aimingInThirdPerson) {
					headWeightTarget = 1;
				} else {
					headWeightTarget = 0;
				}
				if (currentHeadWeight != headWeightTarget) {
					currentHeadWeight = Mathf.MoveTowards (currentHeadWeight, headWeightTarget, Time.deltaTime * weaponsManager.currentIKWeapon.headLookSpeed);
				}
				animator.SetLookAtWeight (1, 0, currentHeadWeight);
				animator.SetLookAtPosition (weaponsManager.currentIKWeapon.headLookTarget.position);
			}
		}
		if (usingZipline) {
			for (int i = 0; i < IKZiplineSettings.IKGoals.Count; i++) {
				animator.SetIKPositionWeight (IKZiplineSettings.IKGoals[i].limb, IKWeight);
				animator.SetIKRotationWeight (IKZiplineSettings.IKGoals[i].limb, IKWeight);  
				animator.SetIKPosition (IKZiplineSettings.IKGoals[i].limb, IKZiplineSettings.IKGoals[i].position.position);
				animator.SetIKRotation (IKZiplineSettings.IKGoals[i].limb, IKZiplineSettings.IKGoals[i].position.rotation); 
			}
			for (int i = 0; i < IKZiplineSettings.IKHints.Count; i++) {
				animator.SetIKHintPositionWeight (IKZiplineSettings.IKHints[i].limb, IKWeight);
				animator.SetIKHintPosition (IKZiplineSettings.IKHints[i].limb, IKZiplineSettings.IKHints[i].position.position);
			}
			transform.position = IKZiplineSettings.bodyPosition.position;
			transform.rotation = IKZiplineSettings.bodyPosition.rotation;
		}
		if (usingJetpack) {
			for (int i = 0; i < IKJetpackSettings.IKGoals.Count; i++) {
				animator.SetIKPositionWeight (IKJetpackSettings.IKGoals[i].limb, IKWeight);
				animator.SetIKRotationWeight (IKJetpackSettings.IKGoals[i].limb, IKWeight);  
				animator.SetIKPosition (IKJetpackSettings.IKGoals[i].limb, IKJetpackSettings.IKGoals[i].position.position);
				animator.SetIKRotation (IKJetpackSettings.IKGoals[i].limb, IKJetpackSettings.IKGoals[i].position.rotation); 
			}
			for (int i = 0; i < IKJetpackSettings.IKHints.Count; i++) {
				animator.SetIKHintPositionWeight (IKJetpackSettings.IKHints[i].limb, IKWeight);
				animator.SetIKHintPosition (IKJetpackSettings.IKHints[i].limb, IKJetpackSettings.IKHints[i].position.position);
			}
		}
		if (usingFlyingMode) {
			for (int i = 0; i < IKFlyingModeSettings.IKGoals.Count; i++) {
				animator.SetIKPositionWeight (IKFlyingModeSettings.IKGoals[i].limb, IKWeight);
				animator.SetIKRotationWeight (IKFlyingModeSettings.IKGoals[i].limb, IKWeight);  
				animator.SetIKPosition (IKFlyingModeSettings.IKGoals[i].limb, IKFlyingModeSettings.IKGoals[i].position.position);
				animator.SetIKRotation (IKFlyingModeSettings.IKGoals[i].limb, IKFlyingModeSettings.IKGoals[i].position.rotation); 
			}
			for (int i = 0; i < IKFlyingModeSettings.IKHints.Count; i++) {
				animator.SetIKHintPositionWeight (IKFlyingModeSettings.IKHints[i].limb, IKWeight);
				animator.SetIKHintPosition (IKFlyingModeSettings.IKHints[i].limb, IKFlyingModeSettings.IKHints[i].position.position);
			}
		}
	}
	//change the ik weight in the current arm
	public void changeArmState(float value){
		if (currentAimMode == aimMode.weapons) {
			setUsingWeaponsState (true);
			usingArms = false;
		} else {
			setUsingWeaponsState (false);
			usingArms = true;
		}
		IKWeightTargetValue = value;
	}
	//change current arm to aim
	public void changeArmSide(bool value){
		if (value) {
			//set the right arm as the current ik position
			currentHandIKPos.transform.position = rightHandIKPos.position;
			currentHandIKPos.transform.rotation = rightHandIKPos.rotation;
			currentHand = AvatarIKGoal.RightHand;
		} 
		else {
			//set the left arm as the current ik position
			currentHandIKPos.transform.position = leftHandIKPos.position;
			currentHandIKPos.transform.rotation = leftHandIKPos.rotation;
			currentHand = AvatarIKGoal.LeftHand;
		}
		originalDist = currentHandIKPos.transform.localPosition.y;
		currentDist = originalDist;
		hitDist = originalDist;
	}
	//set if the player is driving or not, getting the current positions to every player's limb
	public void drivingState(bool state,IKDrivingSystem.IKDrivingInformation IKPositions){
		driving = state;
		if (driving) {
			IKDrivingSettings = IKPositions;
		} else {
			IKDrivingSettings = null;
		}
	}
	public void weaponsState(bool state, IKWeaponInfo IKPositions){
		if (state) {
			setUsingWeaponsState (state);
			IKWeaponsSettings = IKPositions;
			for (int i = 0; i < IKWeaponsSettings.handsInfo.Count; i++) {
				if (!IKWeaponsSettings.handsInfo [i].usedToDrawWeapon) {
					IKWeaponsSettings.handsInfo [i].elbowInfo.elbowIKWeight = 1;
				}
				// && IKWeaponsSettings.handsInfo[i].usedToDrawWeapon
				if (IKWeaponsSettings.handsInfo [i].limb == AvatarIKGoal.LeftHand) {
					if (leftHandMovement != null) {
						StopCoroutine (leftHandMovement);
					}
					leftHandMovement = StartCoroutine (moveThroughWaypoints (IKWeaponsSettings.handsInfo [i], false, IKWeaponsSettings.movementSpeed));
				}
				//&& IKWeaponsSettings.handsInfo[i].usedToDrawWeapon
				if (IKWeaponsSettings.handsInfo [i].limb == AvatarIKGoal.RightHand) {
					if (rightHandMovement != null) {
						StopCoroutine (rightHandMovement);
					}
					rightHandMovement = StartCoroutine (moveThroughWaypoints (IKWeaponsSettings.handsInfo [i], false, IKWeaponsSettings.movementSpeed));
				}
			}
		} else {
			IKWeaponsSettings = IKPositions;
			for (int i = 0; i < IKWeaponsSettings.handsInfo.Count; i++) {
				if (!IKWeaponsSettings.handsInfo [i].usedToDrawWeapon) {
					if (IKWeaponsSettings.handsInfo [i].limb == AvatarIKGoal.LeftHand) {
						if (leftHandMovement != null) {
							StopCoroutine (leftHandMovement);
						}
						leftHandMovement = StartCoroutine (moveThroughWaypoints (IKWeaponsSettings.handsInfo [i], true, IKWeaponsSettings.movementSpeed));
					}
					if (IKWeaponsSettings.handsInfo [i].limb == AvatarIKGoal.RightHand) {
						if (rightHandMovement != null) {
							StopCoroutine (rightHandMovement);
						}
						rightHandMovement = StartCoroutine (moveThroughWaypoints (IKWeaponsSettings.handsInfo [i], true, IKWeaponsSettings.movementSpeed));
					}
				}
			}
		}
		originalWeaponDist = IKWeaponsSettings.aimPosition.localPosition;
	}
	IEnumerator moveThroughWaypoints(IKWeaponsPosition IKWeapon,bool keepingWeapon, float drawWeaponSpeed){
		Transform follower = IKWeapon.waypointFollower;
		List<Transform> wayPoints = new List<Transform> (IKWeapon.wayPoints);
		if (keepingWeapon) {
			wayPoints.Reverse ();
			wayPoints.RemoveAt (0);
		}
		follower.position = IKWeapon.handTransform.position;
		follower.rotation = IKWeapon.handTransform.rotation;
		IKWeapon.transformFollowByHand = follower;
		foreach (Transform transformPath in wayPoints) {
//			while (Vector3.Distance (follower.position, transformPath.position) > 0.01f) {
//				follower.position = Vector3.Slerp (follower.position, transformPath.position, Time.deltaTime * drawWeaponSpeed);
//				follower.rotation = Quaternion.Slerp (follower.rotation, transformPath.rotation, Time.deltaTime * drawWeaponSpeed);
//				yield return null;
//			}
			float dist = Vector3.Distance (follower.position, transformPath.position); // find the distance to travel
			float duration = dist / drawWeaponSpeed; // calculate the movement duration
			float t = 0; // t is the control variable
			while (t < 1) {
				t += Time.deltaTime / duration;
				follower.position = Vector3.Slerp (follower.position, transformPath.position, t);
				follower.rotation = Quaternion.Slerp (follower.rotation, transformPath.rotation, t);
				yield return null;
			}
		}
		if (keepingWeapon) {
			IKWeapon.handInPositionToDraw = false;
			IKWeapon.targetValue = 0;
			disableWeapons = true;
		} else {
			IKWeapon.handInPositionToDraw = true;
			IKWeapon.transformFollowByHand = IKWeapon.position;
			if (IKWeapon.usedToDrawWeapon) {
				weaponsManager.weaponReadyToMove ();
			} 
			IKWeapon.elbowInfo.targetValue = 1;
		}
	}
	public void quickDrawWeaponState(IKWeaponInfo IKPositions){
		setUsingWeaponsState (true);
		IKWeaponsSettings = IKPositions;
		for (int i = 0; i < IKWeaponsSettings.handsInfo.Count; i++) {
			IKWeaponsSettings.handsInfo [i].targetValue = 1;
			IKWeaponsSettings.handsInfo [i].HandIKWeight = 1;
			IKWeaponsSettings.handsInfo [i].elbowInfo.targetValue = 1;
			IKWeaponsSettings.handsInfo [i].elbowInfo.elbowIKWeight = 1;
			IKWeaponsSettings.handsInfo [i].handInPositionToDraw = true;
			IKWeaponsSettings.handsInfo [i].transformFollowByHand = IKWeaponsSettings.handsInfo [i].position;
		}
		originalWeaponDist = IKWeaponsSettings.aimPosition.localPosition;
	}
	public void quickKeepWeaponState(){
		disableWeapons = true;
	}
	public void disableIKWeight(){
		headWeightTarget = 0;
		currentHeadWeight = 0;
	}
	public void setUsingWeaponsState(bool state){
		usingWeapons = state;
	}
	public void startRecoil(){
		if (powerHandRecoil!=null) {
			StopCoroutine (powerHandRecoil);
		}
		powerHandRecoil = StartCoroutine (recoilMovementBack ());
	}
	IEnumerator recoilMovementBack(){
		originalCurrentDist = currentDist;
		float newDist = currentDist - settings.powerShootRecoilAmount;
		for (float t = 0; t < 1;) {
			t += Time.deltaTime * settings.powerShootRecoilSpeed;
			currentDist = Mathf.Lerp (currentDist,newDist, t);
			yield return null;
		}
		StartCoroutine (recoilMovementForward ());
	}
	IEnumerator recoilMovementForward(){
		for (float t = 0; t < 1;) {
			t += Time.deltaTime * settings.powerShootRecoilSpeed;
			currentDist = Mathf.Lerp (currentDist, originalCurrentDist, t);
			yield return null;
		}
	}
	public void ziplineState(bool state,zipline.IKZiplineInfo IKPositions){
		usingZipline = state;
		if (usingZipline) {
			IKWeightTargetValue = 1;
			IKZiplineSettings = IKPositions;
		} else {
			IKWeightTargetValue = 0;
			IKZiplineSettings = null;
		}
	}
	public void jetpackState(bool state,jetpackSystem.IKJetpackInfo IKPositions){
		if (state) {
			usingJetpack = true;
			IKWeightTargetValue = 1;
			IKJetpackSettings = IKPositions;
		} else {
			IKWeightTargetValue = 0;
		}
	}
	public void flyingModeState(bool state,flySystem.IKFlyInfo IKPositions){
		if (state) {
			usingFlyingMode = true;
			IKWeightTargetValue = 1;
			IKFlyingModeSettings = IKPositions;
		} else {
			IKWeightTargetValue = 0;
		}
	}
	void OnDrawGizmos(){
		DrawGizmos();
	}
	void OnDrawGizmosSelected(){
		DrawGizmos();
	}
	//draw the pivot and the final positions of every door
	void DrawGizmos(){
		if (showGizmo && !Application.isPlaying) {
			Gizmos.color = Color.yellow;
			Gizmos.DrawCube (leftHandIKPos.transform.position, Vector3.one/10);
			Gizmos.DrawCube (rightHandIKPos.transform.position, Vector3.one/10);
		}
	}
	[System.Serializable]
	public class IKSettings{
		public float weight;
		public float bodyWeight;
		public float headWeight;
		public float eyesWeight;
		public float clampWeight;
		public float powerShootRecoilAmount;
		public float powerShootRecoilSpeed;
	}
}