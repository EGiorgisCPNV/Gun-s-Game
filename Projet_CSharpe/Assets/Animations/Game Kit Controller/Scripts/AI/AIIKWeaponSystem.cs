using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class AIIKWeaponSystem : MonoBehaviour {
	public IKWeaponInfo weaponInfo;
	public bool aiming;
	public float movementSpeed;
	public float recoilSpeed;
	[HideInInspector] public bool moving;
	int i,j;
	Coroutine weaponMovement;
	Vector3 weaponPositionTarget;
	Quaternion weaponRotationTarget;

	void Start () {
		weaponInfo.IKManager.weaponsState (weaponInfo);
	}
	public void startOrStopUseWeapons(bool state){
		aiming = state;
		if (aiming) {
			weaponPositionTarget = weaponInfo.aimPosition.localPosition;
			weaponRotationTarget = weaponInfo.aimPosition.localRotation;
		} else {
			weaponPositionTarget = weaponInfo.walkPosition.localPosition;
			weaponRotationTarget = weaponInfo.walkPosition.localRotation;
		}
		//stop the coroutine to translate the camera and call it again
		if (weaponMovement != null) {
			StopCoroutine (weaponMovement);
		}
		weaponMovement = StartCoroutine (aimOrStopAimWeapon ());
	}
	IEnumerator aimOrStopAimWeapon(){
		weaponInfo.weapon.GetComponent<AIWeaponSystem> ().aimingWeapon (aiming);
		Vector3 currentWeaponPosition = weaponInfo.weapon.transform.localPosition;
		Quaternion currentWeaponRotation = weaponInfo.weapon.transform.localRotation;
		for (float t = 0; t < 1;) {
			t += Time.deltaTime * movementSpeed;
			weaponInfo.weapon.transform.localPosition = Vector3.Lerp (currentWeaponPosition,weaponPositionTarget, t);
			weaponInfo.weapon.transform.localRotation = Quaternion.Slerp (currentWeaponRotation,weaponRotationTarget, t);
			yield return null;
		}
	}
	public void startRecoil(){
		if (weaponMovement!=null) {
			StopCoroutine (weaponMovement);
		}
		weaponMovement = StartCoroutine (recoilMovementBack ());
	}
	IEnumerator recoilMovementBack(){
		weaponPositionTarget = weaponInfo.recoilPosition.localPosition;
		weaponRotationTarget = weaponInfo.recoilPosition.localRotation;
		Vector3 currentWeaponPosition = weaponInfo.weapon.transform.localPosition;
		Quaternion currentWeaponRotation = weaponInfo.weapon.transform.localRotation;
		for (float t = 0; t < 1;) {
			t += Time.deltaTime * recoilSpeed * 2;
			weaponInfo.weapon.transform.localPosition = Vector3.Lerp (currentWeaponPosition,weaponPositionTarget, t);
			weaponInfo.weapon.transform.localRotation = Quaternion.Slerp (currentWeaponRotation,weaponRotationTarget, t);
			yield return null;
		}
		StartCoroutine (recoilMovementForward ());
	}
	IEnumerator recoilMovementForward(){
		weaponPositionTarget = weaponInfo.aimPosition.localPosition;
		weaponRotationTarget = weaponInfo.aimPosition.localRotation;
		Vector3 currentWeaponPosition = weaponInfo.weapon.transform.localPosition;
		Quaternion currentWeaponRotation = weaponInfo.weapon.transform.localRotation;
		for (float t = 0; t < 1;) {
			t += Time.deltaTime * recoilSpeed * 2;
			weaponInfo.weapon.transform.localPosition = Vector3.Lerp (currentWeaponPosition,weaponPositionTarget, t);
			weaponInfo.weapon.transform.localRotation = Quaternion.Slerp (currentWeaponRotation,weaponRotationTarget, t);
			yield return null;
		}
	}
	[System.Serializable]
	public class IKWeaponInfo{
		public AIIKSystem IKManager;
		public GameObject weapon;
		public Transform aimPosition;
		public Transform walkPosition;
		public Transform recoilPosition;
		public List<IKWeaponsPosition> handsInfo=new List<IKWeaponsPosition>();
	}
	[System.Serializable]
	public class IKWeaponsPosition{
		public string Name;
		public Transform handTransform;
		public AvatarIKGoal limb;
		public Transform position;
		public float HandIKWeight;
		public IKWeaponsPositionElbow elbowInfo;
	}
	[System.Serializable]
	public class IKWeaponsPositionElbow{
		public string Name;
		public AvatarIKHint elbow;
		public Transform position;
		public float elbowIKWeight;
	}
}