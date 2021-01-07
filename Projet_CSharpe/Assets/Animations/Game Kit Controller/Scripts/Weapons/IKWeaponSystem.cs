using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class IKWeaponSystem : MonoBehaviour {
	[SerializeField] public IKWeaponInfo weaponInfo;
	public IKWeaponInfo thirdPersonWeaponInfo;
	public IKWeaponInfo firstPersonWeaponInfo;
	public weaponSwayInfo firstPersonSwayInfo;
	public bool useShotShakeInFirstPerson;
	public bool useShotShakeInThirdPerson;
	public weaponShotShakeInfo thirdPersonshotShakeInfo;
	public weaponShotShakeInfo firstPersonshotShakeInfo;
	public bool showShotShakeettings;
	public GameObject weaponPrefabModel;
	public playerWeaponSystem weapon;
	public GameObject firstPersonArms;
	public bool headLookWhenAiming;
	public float headLookSpeed;
	public Transform headLookTarget;
	public bool checkSurfaceCollision;
	public float weaponLenght;
	public bool canAimInFirstPerson;
	public bool currentWeapon;
	public bool aiming;
	public bool carrying;
	public float recoilSpeed;
	public float extraRotation;
	public float aimFovValue;
	public float aimFovSpeed;
	public bool weaponEnabled;
	public bool showThirdPersonGizmo;
	public bool showFirstPersonGizmo;
	public Color gizmoLabelColor;
	public bool showSettings;
	public bool showElementSettings;
	public bool useWeaponIdle;
	public float timeToActiveWeaponIdle=3;
	public bool playerMoving;
	public Vector3 idlePositionAmount;
	public Vector3 idleRotationAmount;
	public Vector3 idleSpeed;
	public bool idleActive;
	public bool showIdleSettings;
	public bool useLowerRotationSpeedAimed;
	public float rotationSpeedAimedInFirstPerson;
	public GameObject player;
	public bool moving;
	Coroutine weaponMovement;
	Vector3 weaponPositionTarget;
	Quaternion weaponRotationTarget;
	List<Transform> inverseKeepPath =new List<Transform>();
	List<Transform> currentKeepPath =new List<Transform>();
	Vector3 swayRotation;
	Vector3 swayTilt;	
	float swayPositionRunningMultiplier = 1;
	float swayRotationRunningMultiplier = 1;
	float bobPositionRunningMultiplier = 1;
	float bobRotationRunningMultiplier = 1;
	float lastTimeMoved=0;

	void Start () {
		if (!weaponEnabled) {
			enableOrDisableWeaponMesh (false);
		}
//		Animator anim = player.GetComponent<Animator> ();
//		for (int j = 0; j < weaponInfo.handsInfo.Count; j++) {
//			if (!weaponInfo.handsInfo [j].handTransformElement) {
//				if (weaponInfo.handsInfo [j].limb == AvatarIKGoal.RightHand) {
//					print ("right");
//					Transform rightHand = anim.GetBoneTransform (HumanBodyBones.RightHand);
//					weaponInfo.handsInfo [j].handTransformElement = rightHand;
//				}
//				if (weaponInfo.handsInfo [j].limb == AvatarIKGoal.LeftHand) {
//					print ("left");
//					Transform leftHand = anim.GetBoneTransform (HumanBodyBones.LeftHand);
//					weaponInfo.handsInfo [j].handTransformElement = leftHand;
//				}
//			} else {
//				print (weapon.weaponSettings.Name);
//			}
//		}
	}
	//third person
	public void aimOrDrawWeaponThirdPerson(bool state){
		if (currentWeapon) {
			aiming = state;
			if (aiming) {
				weaponPositionTarget = thirdPersonWeaponInfo.aimPosition.localPosition;
				weaponRotationTarget = thirdPersonWeaponInfo.aimPosition.localRotation;
			} else {
				weaponPositionTarget = thirdPersonWeaponInfo.walkPosition.localPosition;
				weaponRotationTarget = thirdPersonWeaponInfo.walkPosition.localRotation;
			}
			//stop the coroutine to translate the camera and call it again
			if (weaponMovement!=null) {
				StopCoroutine (weaponMovement);
			}
			weaponMovement = StartCoroutine (aimOrDrawWeaponThirdPersonCoroutine ());
		}
	}
	IEnumerator aimOrDrawWeaponThirdPersonCoroutine(){
		Vector3 currentWeaponPosition = weapon.gameObject.transform.localPosition;
		Quaternion currentWeaponRotation = weapon.gameObject.transform.localRotation;
		for (float t = 0; t < 1;) {
			t += Time.deltaTime * thirdPersonWeaponInfo.aimMovementSpeed;
			weapon.gameObject.transform.localPosition = Vector3.Lerp (currentWeaponPosition,weaponPositionTarget, t);
			weapon.gameObject.transform.localRotation = Quaternion.Slerp (currentWeaponRotation,weaponRotationTarget, t);
			yield return null;
		}
	}

	public void drawOrKeepWeaponThirdPerson(bool state){
		carrying = state;
		if (carrying) {
			currentKeepPath = thirdPersonWeaponInfo.keepPath;
		} else {
			inverseKeepPath.Clear ();
			inverseKeepPath = new List<Transform> (thirdPersonWeaponInfo.keepPath);
			inverseKeepPath.Reverse ();
			currentKeepPath = inverseKeepPath;
			aiming = false;
		}
		//stop the coroutine to translate the camera and call it again
		if (weaponMovement != null) {
			StopCoroutine (weaponMovement);
		}
		weaponMovement = StartCoroutine (drawOrKeepWeaponThirdPersonCoroutine ());
	}
	IEnumerator drawOrKeepWeaponThirdPersonCoroutine(){
		moving = true;
		if (carrying) {
			weapon.weaponSettings.weapon.transform.SetParent (weapon.getWeaponParent());
		}
		foreach (Transform transformPath in  currentKeepPath) {
//			Vector3 pos = transformPath.transform.localPosition;
//			Quaternion rot = transformPath.transform.localRotation;
//			while (Vector3.Distance (weapon.gameObject.transform.localPosition, pos) > 0.000001f) {
//				weapon.gameObject.transform.localPosition = Vector3.Lerp (weapon.gameObject.transform.localPosition, pos, Time.deltaTime * thirdPersonWeaponInfo.movementSpeed);
//				weapon.gameObject.transform.localRotation = Quaternion.Slerp (weapon.gameObject.transform.localRotation, rot, Time.deltaTime * thirdPersonWeaponInfo.movementSpeed);
//				yield return null;
//			}
//
			float dist = Vector3.Distance (weapon.gameObject.transform.position, transformPath.transform.position);
			float duration = dist / thirdPersonWeaponInfo.movementSpeed;
			float t = 0;
			Vector3 pos = transformPath.transform.localPosition;
			Quaternion rot = transformPath.transform.localRotation;
			while (t < 1) {
				t += Time.deltaTime / duration; 
				weapon.gameObject.transform.localPosition = Vector3.Lerp (weapon.gameObject.transform.localPosition, pos, t);
				weapon.gameObject.transform.localRotation = Quaternion.Slerp (weapon.gameObject.transform.localRotation, rot, t);
				yield return null;
			}
		}
		if (!aiming && !carrying) {
			weapon.weaponSettings.weapon.transform.SetParent (weapon.weaponSettings.weaponParent);
			for (float t = 0; t < 1;) {
				t += Time.deltaTime * thirdPersonWeaponInfo.movementSpeed;
				weapon.gameObject.transform.localPosition = Vector3.Lerp (weapon.gameObject.transform.localPosition, thirdPersonWeaponInfo.keepPosition.localPosition, t);
				weapon.gameObject.transform.localRotation = Quaternion.Slerp (weapon.gameObject.transform.localRotation, thirdPersonWeaponInfo.keepPosition.localRotation, t);
				yield return null;
			}
			setIKWeight (0, 0);
			setIKWeightElbows (0, 0);
			for (int i = 0; i < thirdPersonWeaponInfo.handsInfo.Count; i++) {
				if (thirdPersonWeaponInfo.handsInfo [i].usedToDrawWeapon) {
					thirdPersonWeaponInfo.handsInfo [i].handInPositionToDraw = false;
					thirdPersonWeaponInfo.handsInfo [i].waypointFollower.position = thirdPersonWeaponInfo.handsInfo [i].position.position;
					thirdPersonWeaponInfo.handsInfo [i].transformFollowByHand = thirdPersonWeaponInfo.handsInfo [i].waypointFollower;
				}
			}
		}
		moving = false;
	}
	public void setIKWeight(float leftValue, float rightValue){
		for (int i = 0; i < thirdPersonWeaponInfo.handsInfo.Count; i++) {
			if (thirdPersonWeaponInfo.handsInfo [i].limb == AvatarIKGoal.LeftHand) {
				thirdPersonWeaponInfo.handsInfo [i].targetValue = leftValue;
			}
			if (thirdPersonWeaponInfo.handsInfo [i].limb == AvatarIKGoal.RightHand) {
				thirdPersonWeaponInfo.handsInfo [i].targetValue = rightValue;
			}
		}
	}
	public void setIKWeightElbows(float leftValue, float rightValue){
		for (int i = 0; i < thirdPersonWeaponInfo.handsInfo.Count; i++) {
			if (thirdPersonWeaponInfo.handsInfo [i].limb == AvatarIKGoal.LeftHand) {
				thirdPersonWeaponInfo.handsInfo [i].elbowInfo.targetValue = leftValue;
			}
			if (thirdPersonWeaponInfo.handsInfo [i].limb == AvatarIKGoal.RightHand) {
				thirdPersonWeaponInfo.handsInfo [i].elbowInfo.targetValue = rightValue;
			}
		}
	}
	public void quickDrawWeaponThirdPerson(){
		carrying = true;
		currentKeepPath = thirdPersonWeaponInfo.keepPath;
		weapon.weaponSettings.weapon.transform.SetParent (weapon.getWeaponParent());
		weapon.gameObject.transform.localPosition = thirdPersonWeaponInfo.walkPosition.localPosition;
		weapon.gameObject.transform.localRotation = thirdPersonWeaponInfo.walkPosition.localRotation;
	}
	public void quickKeepWeaponThirdPerson(){
		aiming = false;
		carrying = false;
		weapon.weaponSettings.weapon.transform.SetParent (weapon.weaponSettings.weaponParent);
		weapon.gameObject.transform.localPosition = thirdPersonWeaponInfo.keepPosition.localPosition;
		weapon.gameObject.transform.localRotation = thirdPersonWeaponInfo.keepPosition.localRotation;
		setIKWeight (0, 0);
		setIKWeightElbows (0, 0);
		for (int i = 0; i < thirdPersonWeaponInfo.handsInfo.Count; i++) {
			thirdPersonWeaponInfo.handsInfo [i].handInPositionToDraw = false;
			thirdPersonWeaponInfo.handsInfo [i].transformFollowByHand = thirdPersonWeaponInfo.handsInfo [i].waypointFollower;
			thirdPersonWeaponInfo.handsInfo [i].transformFollowByHand.position = thirdPersonWeaponInfo.handsInfo [i].handTransform.position;
		}
	}

	//first person
	public void aimOrDrawWeaponFirstPerson(bool state){
		if (currentWeapon) {
			aiming = state;
			if (aiming) {
				weaponPositionTarget = firstPersonWeaponInfo.aimPosition.localPosition;
				weaponRotationTarget = firstPersonWeaponInfo.aimPosition.localRotation;
			} else {
				weaponPositionTarget = firstPersonWeaponInfo.walkPosition.localPosition;
				weaponRotationTarget = firstPersonWeaponInfo.walkPosition.localRotation;
			}
			//stop the coroutine to translate the camera and call it again
			if (weaponMovement!=null) {
				StopCoroutine (weaponMovement);
			}
			weaponMovement = StartCoroutine (aimOrDrawWeaponFirstPersonCoroutine ());
			setLastTimeMoved ();
		}
	}
	IEnumerator aimOrDrawWeaponFirstPersonCoroutine(){
		Vector3 currentWeaponPosition = weapon.gameObject.transform.localPosition;
		Quaternion currentWeaponRotation = weapon.gameObject.transform.localRotation;
		for (float t = 0; t < 1;) {
			t += Time.deltaTime * firstPersonWeaponInfo.aimMovementSpeed;
			weapon.gameObject.transform.localPosition = Vector3.Lerp (currentWeaponPosition,weaponPositionTarget, t);
			weapon.gameObject.transform.localRotation = Quaternion.Slerp (currentWeaponRotation,weaponRotationTarget, t);
			yield return null;
		}
	}
	public void drawOrKeepWeaponFirstPerson(bool state){
		carrying = state;
		if (!carrying) {
			aiming = false;
		}
		//stop the coroutine to translate the camera and call it again
		if (weaponMovement!=null) {
			StopCoroutine (weaponMovement);
		}
		weaponMovement = StartCoroutine (drawOrKeepWeaponFirstPersonCoroutine ());
		setLastTimeMoved ();
	}
	IEnumerator drawOrKeepWeaponFirstPersonCoroutine(){
		Vector3 targetPosition = Vector3.zero;
		Quaternion targetRotation = Quaternion.identity;
		moving = true;
		if (carrying) {
			weapon.weaponSettings.weapon.transform.SetParent (weapon.getWeaponParent());
			enableOrDisableWeaponMesh (true);
			enableOrDisableFirstPersonArms (true);
			targetPosition = firstPersonWeaponInfo.walkPosition.localPosition;
			targetRotation = firstPersonWeaponInfo.walkPosition.localRotation;
			weapon.gameObject.transform.localPosition = firstPersonWeaponInfo.keepPosition.localPosition;
			weapon.gameObject.transform.localRotation = firstPersonWeaponInfo.keepPosition.localRotation;

		} else {
			targetPosition = firstPersonWeaponInfo.keepPosition.localPosition;
			targetRotation = firstPersonWeaponInfo.keepPosition.localRotation;
		}
		while (Vector3.Distance (weapon.gameObject.transform.localPosition, targetPosition) > .01f) {
			weapon.gameObject.transform.localPosition = Vector3.Lerp (weapon.gameObject.transform.localPosition, targetPosition, Time.deltaTime * firstPersonWeaponInfo.movementSpeed);
			weapon.gameObject.transform.localRotation = Quaternion.Slerp (weapon.gameObject.transform.localRotation, targetRotation, Time.deltaTime * firstPersonWeaponInfo.movementSpeed);
			yield return null;
		}
		if (!aiming && !carrying) {
			weapon.weaponSettings.weapon.transform.SetParent (weapon.weaponSettings.weaponParent);
			enableOrDisableWeaponMesh (false);
			enableOrDisableFirstPersonArms (false);
		}
		moving = false;
	}

	public void quickKeepWeaponFirstPerson(){
		carrying = false;
		aiming = false;
		weapon.weaponSettings.weapon.transform.SetParent (weapon.weaponSettings.weaponParent);
		weapon.gameObject.transform.position = firstPersonWeaponInfo.keepPosition.position;
		weapon.gameObject.transform.rotation = weapon.gameObject.transform.rotation;
		enableOrDisableWeaponMesh (false);
	}

	//Recoil functions
	public void startRecoil(bool isThirdPersonView){
		if (weaponMovement!=null) {
			StopCoroutine (weaponMovement);
		}
		weaponMovement = StartCoroutine (recoilMovementBack (isThirdPersonView));
	}
	IEnumerator recoilMovementBack(bool isThirdPersonView){
		if (isThirdPersonView) {
			weaponPositionTarget = thirdPersonWeaponInfo.aimRecoilPosition.localPosition;
			weaponRotationTarget = thirdPersonWeaponInfo.aimRecoilPosition.localRotation;
			if (thirdPersonWeaponInfo.useExtraRandomRecoil) {
				Vector3 extraPosition = thirdPersonWeaponInfo.extraRandomRecoilPosition;
				Vector3 randomPosition = new Vector3 (Random.Range (-extraPosition.x, extraPosition.x), 
					                         Random.Range (0, extraPosition.y), Random.Range (-extraPosition.z, 0));
				weaponPositionTarget += randomPosition;
				Vector3 extraRotatation = thirdPersonWeaponInfo.extraRandomRecoilRotation;
				Vector3 randomRotation = new Vector3 (Random.Range (-extraRotatation.x, 0), 
					                         Random.Range (-extraRotatation.y, extraRotatation.y), Random.Range (-extraRotatation.z, extraRotatation.z));
				weaponRotationTarget = Quaternion.Euler (weaponRotationTarget.eulerAngles + randomRotation);
			}
		} else {
			if (aiming) {
				weaponPositionTarget = firstPersonWeaponInfo.aimRecoilPosition.localPosition;
				weaponRotationTarget = firstPersonWeaponInfo.aimRecoilPosition.localRotation;
			} else {
				weaponPositionTarget = firstPersonWeaponInfo.walkRecoilPosition.localPosition;
				weaponRotationTarget = firstPersonWeaponInfo.walkRecoilPosition.localRotation;
			}
			if (firstPersonWeaponInfo.useExtraRandomRecoil) {
				Vector3 extraPosition = firstPersonWeaponInfo.extraRandomRecoilPosition;
				Vector3 randomPosition = new Vector3 (Random.Range (-extraPosition.x, extraPosition.x), 
					                         Random.Range (0, extraPosition.y), Random.Range (-extraPosition.z, 0));

				if (aiming) {
					randomPosition *= firstPersonSwayInfo.swayPositionPercentageAiming;
				}

				weaponPositionTarget += randomPosition;
				Vector3 extraRotatation = firstPersonWeaponInfo.extraRandomRecoilRotation;
				Vector3 randomRotation = new Vector3 (Random.Range (-extraRotatation.x, 0), 
					                         Random.Range (-extraRotatation.y, extraRotatation.y), Random.Range (-extraRotatation.z, extraRotatation.z));

				if (aiming) {
					randomRotation *= firstPersonSwayInfo.swayRotationPercentageAiming;
				}

				weaponRotationTarget = Quaternion.Euler (weaponRotationTarget.eulerAngles + randomRotation);
			}
		}
		Vector3 currentWeaponPosition = weapon.gameObject.transform.localPosition;
		Quaternion currentWeaponRotation = weapon.gameObject.transform.localRotation;
		for (float t = 0; t < 1;) {
			t += Time.deltaTime * recoilSpeed * 2;
			weapon.gameObject.transform.localPosition = Vector3.Lerp (currentWeaponPosition,weaponPositionTarget, t);
			weapon.gameObject.transform.localRotation = Quaternion.Slerp (currentWeaponRotation,weaponRotationTarget, t);
			yield return null;
		}
		StartCoroutine (recoilMovementForward (isThirdPersonView));
	}
	IEnumerator recoilMovementForward(bool isThirdPersonView){
		if (isThirdPersonView) {
			weaponPositionTarget = thirdPersonWeaponInfo.aimPosition.localPosition;
			weaponRotationTarget = thirdPersonWeaponInfo.aimPosition.localRotation;
		} else {
			if (aiming) {
				weaponPositionTarget = firstPersonWeaponInfo.aimPosition.localPosition;
				weaponRotationTarget = firstPersonWeaponInfo.aimPosition.localRotation;
			} else {
				weaponPositionTarget = firstPersonWeaponInfo.walkPosition.localPosition;
				weaponRotationTarget = firstPersonWeaponInfo.walkPosition.localRotation;
			}
		}
		Vector3 currentWeaponPosition = weapon.gameObject.transform.localPosition;
		Quaternion currentWeaponRotation = weapon.gameObject.transform.localRotation;
		for (float t = 0; t < 1;) {
			t += Time.deltaTime * recoilSpeed * 2;
			weapon.gameObject.transform.localPosition = Vector3.Lerp (currentWeaponPosition,weaponPositionTarget, t);
			weapon.gameObject.transform.localRotation = Quaternion.Slerp (currentWeaponRotation,weaponRotationTarget, t);
			yield return null;
		}
	}

	public void currentWeaponSway(float mouseX, float mouseY, float vertical, float horizontal, bool running, bool shooting, bool onGround){
		if (firstPersonSwayInfo.useSway) {
			if (useWeaponIdle) {
				if (horizontal == 0 && vertical == 0 && mouseX == 0 && mouseY == 0 && onGround) {
					playerMoving = false;
				} else {
					playerMoving = true;
					idleActive = false;
					setLastTimeMoved ();
				}
				if (!playerMoving) {
					if (Time.time > lastTimeMoved + timeToActiveWeaponIdle && !moving) { 
						idleActive = true;
					} else {
						idleActive = false;
					}
				}
			}
			Transform weaponTransform = weapon.weaponSettings.weaponMesh.transform;
			if (running && !aiming) {
				swayPositionRunningMultiplier = firstPersonSwayInfo.swayPositionRunningMultiplier;
				swayRotationRunningMultiplier = firstPersonSwayInfo.swayRotationRunningMultiplier;
				bobPositionRunningMultiplier = firstPersonSwayInfo.bobPositionRunningMultiplier;
				bobRotationRunningMultiplier = firstPersonSwayInfo.bobRotationRunningMultiplier;
			} else {
				swayPositionRunningMultiplier = 1;
				swayRotationRunningMultiplier = 1;
				bobPositionRunningMultiplier = 1;
				bobRotationRunningMultiplier = 1;
			}
			if (firstPersonSwayInfo.usePositionSway) {
				Vector3 swayPosition = Vector3.zero;
				swayPosition.x = -mouseX * firstPersonSwayInfo.swayPositionVertical * swayPositionRunningMultiplier;
				swayPosition.y = -mouseY * firstPersonSwayInfo.swayPositionHorizontal * swayPositionRunningMultiplier;
				if (swayPosition.x > firstPersonSwayInfo.swayPositionMaxAmount) {
					swayPosition.x = firstPersonSwayInfo.swayPositionMaxAmount;
				}
				if (swayPosition.x < -firstPersonSwayInfo.swayPositionMaxAmount) {
					swayPosition.x = -firstPersonSwayInfo.swayPositionMaxAmount;
				}
				if (swayPosition.y > firstPersonSwayInfo.swayPositionMaxAmount) {
					swayPosition.y = firstPersonSwayInfo.swayPositionMaxAmount;
				}
				if (swayPosition.y < -firstPersonSwayInfo.swayPositionMaxAmount) {
					swayPosition.y = -firstPersonSwayInfo.swayPositionMaxAmount;
				}
				if (firstPersonSwayInfo.useBobPosition) {
					if ((Mathf.Abs (horizontal) > 0 || Mathf.Abs (vertical) > 0) && onGround) {
						Vector3 posTarget = getSwayPosition (firstPersonSwayInfo.bobPositionSpeed, firstPersonSwayInfo.bobPositionAmount, bobRotationRunningMultiplier);
						if (aiming) {
							posTarget *= firstPersonSwayInfo.bobRotationPercentageAiming;
						}
						swayPosition += posTarget;
					} 
				}
				Vector3 extraPosition = firstPersonSwayInfo.movingExtraPosition;
				if (aiming) {
					extraPosition*= firstPersonSwayInfo.bobPositionPercentageAiming;
				}
				if (vertical > 0) {
					swayPosition += Vector3.forward * extraPosition.z;
				} 
				if (vertical < 0) {
					swayPosition -= Vector3.forward * extraPosition.z;
				}
				if (horizontal > 0) {
					swayPosition += Vector3.right * extraPosition.x;
				} 
				if (horizontal < 0) {
					swayPosition -= Vector3.right * extraPosition.x;
				}
				if (aiming) {
					swayPosition *= firstPersonSwayInfo.swayPositionPercentageAiming;
				}
				if (!moving && idleActive) {
					swayPosition = getSwayPosition (idleSpeed, idlePositionAmount, 1);
				}
				weaponTransform.localPosition = Vector3.Lerp (weaponTransform.localPosition, swayPosition, Time.deltaTime * firstPersonSwayInfo.swayPositionSmooth);
			}

			if (firstPersonSwayInfo.useRotationSway) {
				swayRotation.z = mouseX * firstPersonSwayInfo.swayRotationHorizontal * swayRotationRunningMultiplier;
				swayRotation.x = mouseY * firstPersonSwayInfo.swayRotationVertical * swayRotationRunningMultiplier;
				if (firstPersonSwayInfo.useBobRotation) {
					if (!shooting) {
						swayTilt.x = vertical * firstPersonSwayInfo.bobRotationVertical * bobPositionRunningMultiplier;
						swayTilt.z = horizontal * firstPersonSwayInfo.bobRotationHorizontal * bobPositionRunningMultiplier;
					} else {
						swayTilt = Vector3.zero;
					}
				}
				if (aiming) {
					swayTilt *= firstPersonSwayInfo.bobPositionPercentageAiming;
				}
				swayRotation += swayTilt;
				if (aiming) {
					swayRotation *= firstPersonSwayInfo.swayRotationPercentageAiming;
				}
				if (!moving && idleActive) {
					swayRotation = getSwayPosition (idleSpeed, idleRotationAmount, 1);
				}
				Quaternion targetRotation = Quaternion.Euler (swayRotation);
				weaponTransform.localRotation = Quaternion.Slerp (weaponTransform.localRotation, targetRotation, Time.deltaTime * firstPersonSwayInfo.swayRotationSmooth);
			}
		}
	}
	public Vector3 getSwayPosition(Vector3 speed, Vector3 amount, float multiplier){
		Vector3 posTarget = Vector3.zero;
		posTarget.x = Mathf.Sin (Time.time * speed.x) * amount.x * multiplier;
		posTarget.y = Mathf.Sin (Time.time * speed.y) * amount.y * multiplier;
		posTarget.z = Mathf.Sin (Time.time * speed.z) * amount.z * multiplier;
		return posTarget;
	}
	public void setLastTimeMoved(){
		lastTimeMoved = Time.time;
	}
	public void enableOrDisableFirstPersonArms(bool state){
		if (firstPersonArms) {
			firstPersonArms.SetActive (state);
		}
	}
	public void enableOrDisableWeaponMesh(bool state){
		weapon.weaponSettings.weaponMesh.SetActive (state);
	}

	public void setHandTransform(){
		player = GameObject.Find ("Player Controller");
		Animator anim = player.GetComponent<Animator> ();
		for (int j = 0; j < weaponInfo.handsInfo.Count; j++) {
			if (weaponInfo.handsInfo [j].limb == AvatarIKGoal.RightHand) {
				weaponInfo.handsInfo [j].handTransform = anim.GetBoneTransform (HumanBodyBones.RightHand);
				#if UNITY_EDITOR
				EditorUtility.SetDirty (this);
				#endif
			}
			if (weaponInfo.handsInfo [j].limb == AvatarIKGoal.LeftHand) {
				weaponInfo.handsInfo [j].handTransform = anim.GetBoneTransform (HumanBodyBones.LeftHand);
				#if UNITY_EDITOR
				EditorUtility.SetDirty (this);
				#endif
			}
		}
	}
	void OnDrawGizmos(){
		DrawGizmos();
	}
	void OnDrawGizmosSelected(){
		DrawGizmos();
	}
	void DrawGizmos(){
		if (showThirdPersonGizmo) {
			drawWeaponInfoPositions (thirdPersonWeaponInfo);
		}
		if (showFirstPersonGizmo) {
			drawWeaponInfoPositions (firstPersonWeaponInfo);
		}
	}
	void drawWeaponInfoPositions(IKWeaponInfo info){
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere (info.aimPosition.position, 0.03f);
		Gizmos.color = Color.white;
		Gizmos.DrawLine (info.aimPosition.position, info.walkPosition.position);
		Gizmos.color = Color.green;
		Gizmos.DrawSphere (info.walkPosition.position, 0.03f);
		for (int i = 0; i < info.keepPath.Count; i++) {
			if (i + 1 < info.keepPath.Count) {
				Gizmos.color = Color.yellow;
				Gizmos.DrawLine (info.keepPath[i].position, info.keepPath[i+1].position);
			}
			if (i != info.keepPath.Count - 1) {
				Gizmos.color = Color.red;
				Gizmos.DrawSphere (info.keepPath [i].position, 0.03f);
			}
		}
		if (info.keepPath.Count > 0) {
			Gizmos.color = Color.white;
			Gizmos.DrawLine (info.keepPosition.position, info.keepPath [0].position);
		}
		Gizmos.color = Color.red;
		Gizmos.DrawSphere (info.keepPosition.position, 0.03f);
		Gizmos.color = Color.white;
		Gizmos.DrawLine (info.aimPosition.position, info.aimRecoilPosition.position);
		if (info.walkRecoilPosition) {
			Gizmos.DrawLine (info.walkPosition.position, info.walkRecoilPosition.position);
		}
		Gizmos.color = Color.magenta;
		Gizmos.DrawSphere (info.aimRecoilPosition.position, 0.03f);
		if (info.walkRecoilPosition) {
			Gizmos.DrawSphere (info.walkRecoilPosition.position, 0.03f);
		}
		for (int i = 0; i < info.handsInfo.Count; i++) {
			Gizmos.color = Color.blue;
			if (info.handsInfo [i].position) {
				Gizmos.DrawSphere (info.handsInfo [i].position.position, 0.02f);
			}
			if (info.handsInfo [i].waypointFollower) {
				Gizmos.color = Color.cyan;
				Gizmos.DrawSphere (info.handsInfo [i].waypointFollower.position, 0.01f);
			}
			for (int j = 0; j < info.handsInfo[i].wayPoints.Count; j++) {
				if (j == 0) {
					if (info.handsInfo [i].handTransform) {
						Gizmos.color = Color.black;
						Gizmos.DrawLine (info.handsInfo [i].wayPoints [j].position, info.handsInfo [i].handTransform.position);
					}
				}
				Gizmos.color = Color.gray;
				Gizmos.DrawSphere (info.handsInfo[i].wayPoints[j].position, 0.01f);
				if (j + 1 < info.handsInfo[i].wayPoints.Count) {
					Gizmos.color = Color.yellow;
					Gizmos.DrawLine (info.handsInfo[i].wayPoints[j].position, info.handsInfo[i].wayPoints[j+1].position);
				}
			}
			Gizmos.color = Color.blue;
			if (info.handsInfo [i].elbowInfo.position) {
				Gizmos.DrawSphere (info.handsInfo [i].elbowInfo.position.position, 0.02f);
			}
		}
		if (checkSurfaceCollision) {
			Gizmos.color = Color.white;
			Gizmos.DrawLine (weapon.transform.position, weapon.transform.position + weapon.transform.forward * weaponLenght);
			Gizmos.color = Color.red;
			Gizmos.DrawSphere (weapon.transform.position + weapon.transform.forward * weaponLenght, 0.01f);
		}
	}
	[System.Serializable]
	public class weaponSwayInfo{
		public bool useSway;
		public bool usePositionSway;
		public float swayPositionVertical = 0.02f;
		public float swayPositionHorizontal = 0.03f;
		public float swayPositionMaxAmount = 0.03f;
		public float swayPositionSmooth = 2;
		public bool useRotationSway;
		public float swayRotationVertical = 20;
		public float swayRotationHorizontal = 30;
		public float swayRotationSmooth = 3;
		public bool useBobPosition;
		public Vector3 bobPositionSpeed = new Vector3 (5, 10, 3);
		public Vector3 bobPositionAmount = new Vector3 (0.01f, 0.05f, 0.05f);
		public bool useBobRotation;
		public float bobRotationVertical = 15;
		public float bobRotationHorizontal = 10;
		public Vector3 movingExtraPosition;
		public float swayPositionRunningMultiplier = 1;
		public float swayRotationRunningMultiplier = 1;
		public float bobPositionRunningMultiplier = 1;
		public float bobRotationRunningMultiplier = 1;
		[Range(0,1)] public float swayPositionPercentageAiming;
		[Range(0,1)] public float swayRotationPercentageAiming;
		[Range(0,1)] public float bobPositionPercentageAiming;
		[Range(0,1)] public float bobRotationPercentageAiming;
		public bool showSwaySettings;
	}
	[System.Serializable]
	public class weaponShotShakeInfo{
		public float shotForce;
		public float shakeSmooth;
		public float shakeDuration;
		public Vector3 shakePosition;
		public Vector3 shakeRotation;
	}
}