using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class AIIKSystem : MonoBehaviour {
	public bool usingWeapons;
	public IKSettings settings;
	AIIKWeaponSystem.IKWeaponInfo IKWeaponsSettings;
	Animator animator;
	int i,j,k;

	void Start (){
		
	}
	void Update(){
		
	}
	void OnAnimatorIK(){
		if (usingWeapons) {
			for (i = 0; i < IKWeaponsSettings.handsInfo.Count; i++) {
				animator.SetIKPositionWeight (IKWeaponsSettings.handsInfo [i].limb, IKWeaponsSettings.handsInfo [i].HandIKWeight);
				animator.SetIKRotationWeight (IKWeaponsSettings.handsInfo [i].limb, IKWeaponsSettings.handsInfo [i].HandIKWeight);  
				animator.SetIKPosition (IKWeaponsSettings.handsInfo [i].limb, IKWeaponsSettings.handsInfo [i].position.position);
				animator.SetIKRotation (IKWeaponsSettings.handsInfo [i].limb, IKWeaponsSettings.handsInfo [i].position.rotation); 
				animator.SetIKHintPositionWeight (IKWeaponsSettings.handsInfo[i].elbowInfo.elbow, IKWeaponsSettings.handsInfo[i].elbowInfo.elbowIKWeight);
				animator.SetIKHintPosition (IKWeaponsSettings.handsInfo[i].elbowInfo.elbow, IKWeaponsSettings.handsInfo[i].elbowInfo.position.position);
			}
		}
	}
	public void weaponsState(AIIKWeaponSystem.IKWeaponInfo IKPositions){
		animator = GetComponent<Animator>();
			usingWeapons = true;
			IKWeaponsSettings = IKPositions;
	}
	[System.Serializable]
	public class IKSettings{
		public float weight;
		public float bodyWeight;
		public float headWeight;
		public float eyesWeight;
		public float clampWeight;
	}
}