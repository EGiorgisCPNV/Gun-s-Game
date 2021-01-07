using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class IKWeaponInfo
{
	public GameObject weapon;
	public Transform aimPosition;
	public Transform walkPosition;
	public Transform keepPosition;
	public Transform aimRecoilPosition;
	public Transform walkRecoilPosition;
	public float movementSpeed;
	public float aimMovementSpeed;
	public bool useExtraRandomRecoil;
	public Vector3 extraRandomRecoilPosition;
	public Vector3 extraRandomRecoilRotation;
	public List<Transform> keepPath = new List<Transform> ();
	public List<IKWeaponsPosition> handsInfo = new List<IKWeaponsPosition> ();

}

[System.Serializable]
public class IKWeaponsPosition
{
	public string Name;
	public Transform handTransform;
	public AvatarIKGoal limb;
	public Transform position;
	public float HandIKWeight;
	public float targetValue;
	public Transform waypointFollower;
	public List<Transform> wayPoints = new List<Transform> ();
	public bool handInPositionToDraw;
	public Transform transformFollowByHand;
	public bool usedToDrawWeapon;
	public IKWeaponsPositionElbow elbowInfo;
	public bool showElbowInfo;
}

[System.Serializable]
public class IKWeaponsPositionElbow
{
	public string Name;
	public AvatarIKHint elbow;
	public Transform position;
	public float elbowIKWeight;
	public float targetValue;
}

