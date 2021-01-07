using UnityEngine;
using System.Collections;
public class buildPlayer : MonoBehaviour {
	public GameObject trail;
	public GameObject hitCombat;
	public GameObject shootZone;
	public GameObject arrow;
	public GameObject footStep;
	public GameObject currentHandIKPos;
	public GameObject handIKPosition;
	public GameObject player;
	public GameObject jetPack;
	public GameObject weapons;
	GameObject gravityCenter;
	Animator anim;
	Vector3 IKHandPos;
	Transform chest;
	ragdollActivator ragdollActivatorManager;
	playerWeaponsManager weaponsManager;
	jetpackSystem jetpackManager;

	void Start(){
		Destroy (GetComponent<buildPlayer>());
	}
	//set all the objects inside the character's body
	public void buildBody(){
		//it only works in the editor mode, checking the game is not running
		if (!Application.isPlaying) {
			weaponsManager = player.GetComponent<playerWeaponsManager> ();
			ragdollActivatorManager = player.GetComponent<ragdollActivator> ();
			jetpackManager = player.GetComponent<jetpackSystem> ();
			gravityCenter = GameObject.Find ("gravityCenter");
			//check if the player is already builded, to avoid build the player incorrectly
			if (gravityCenter.transform.childCount >= 1) {
				//also, set the instantiated objects in others scripts
				closeCombatSystem combat = player.GetComponent<closeCombatSystem> ();
				combat.legTrails.Clear ();
				combat.handTrails.Clear ();
				combat.legColliders.Clear ();
				combat.handColliders.Clear ();
				//get the character to build inside the player
				GameObject character = gravityCenter.transform.GetChild (0).gameObject;
				character.transform.localPosition = gravityCenter.transform.localPosition * (-1);
				//get and set the animator and avatar of the model
				anim = character.GetComponent<Animator> ();
				if (anim.GetBoneTransform (HumanBodyBones.Chest)) {
					chest = anim.GetBoneTransform (HumanBodyBones.Chest);
				} else {
					print ("Chest not found, check the player parts");
					chest = anim.GetBoneTransform (HumanBodyBones.Spine);
				}
				player.GetComponent<Animator> ().avatar = anim.avatar;
				//create a list of the needed bones, to set every object inside of everyone, in this case for the trailes
				Transform[] trailsPositions = new Transform[] {
					anim.GetBoneTransform (HumanBodyBones.LeftFoot),
					anim.GetBoneTransform (HumanBodyBones.RightFoot),
					anim.GetBoneTransform (HumanBodyBones.LeftLowerLeg),
					anim.GetBoneTransform (HumanBodyBones.RightLowerLeg),
					anim.GetBoneTransform (HumanBodyBones.LeftHand),
					anim.GetBoneTransform (HumanBodyBones.RightHand),
					anim.GetBoneTransform (HumanBodyBones.LeftLowerArm),
					anim.GetBoneTransform (HumanBodyBones.RightLowerArm),
					anim.GetBoneTransform (HumanBodyBones.Spine),
					anim.GetBoneTransform (HumanBodyBones.Head)
				};
				for (int i = 0; i < trailsPositions.Length; i++) {
					GameObject trailClone = (GameObject)Instantiate (trail, Vector3.zero, Quaternion.identity);
					//remove the clone string inside the instantiated object
					trailClone.name = trailClone.name.Replace ("(Clone)", "");
					trailClone.transform.SetParent (trailsPositions [i]);
					trailClone.transform.localPosition = Vector3.zero;
					//trailClone.transform.localRotation = Quaternion.identity;
					trailClone.GetComponent<TrailRenderer> ().enabled = false;
					//set the components of the closecombat script
					if (trailsPositions [i] == anim.GetBoneTransform (HumanBodyBones.LeftFoot) ||
					    trailsPositions [i] == anim.GetBoneTransform (HumanBodyBones.RightFoot)) {
						combat.legTrails.Add (trailClone);
					}
					if (trailsPositions [i] == anim.GetBoneTransform (HumanBodyBones.LeftHand) ||
					    trailsPositions [i] == anim.GetBoneTransform (HumanBodyBones.RightHand)) {
						combat.handTrails.Add (trailClone);
					}
				}
				//create the shoot zone in the right hand of the player
				GameObject shootZoneClone = (GameObject)Instantiate (shootZone, Vector3.zero, Quaternion.identity);
				shootZoneClone.name = shootZoneClone.name.Replace ("(Clone)", "");
				shootZoneClone.transform.SetParent (anim.GetBoneTransform (HumanBodyBones.RightHand).transform);
				shootZoneClone.transform.localPosition = Vector3.zero;
				shootZoneClone.transform.localRotation = Quaternion.identity;
				//another list of bones, to the triggers in hands and feet for the combat
				Transform[] hitCombatPositions = new Transform[] {
					anim.GetBoneTransform (HumanBodyBones.LeftToes),
					anim.GetBoneTransform (HumanBodyBones.RightToes),
					anim.GetBoneTransform (HumanBodyBones.LeftHand),
					anim.GetBoneTransform (HumanBodyBones.RightHand)
				};
				for (int i = 0; i < hitCombatPositions.Length; i++) {
					GameObject hitCombatClone = (GameObject)Instantiate (hitCombat, Vector3.zero, Quaternion.identity);
					hitCombatClone.name = hitCombatClone.name.Replace ("(Clone)", "");
					hitCombatClone.transform.SetParent (hitCombatPositions [i]);
					hitCombatClone.transform.localPosition = Vector3.zero;
					hitCombatClone.transform.localRotation = Quaternion.identity;
					//set the triggers in the close combat script
					if (hitCombatPositions [i] == anim.GetBoneTransform (HumanBodyBones.LeftToes) ||
					    hitCombatPositions [i] == anim.GetBoneTransform (HumanBodyBones.RightToes)) {
						combat.legColliders.Add (hitCombatClone);
					} else {
						combat.handColliders.Add (hitCombatClone);
					}
				}
				//a list for the footsteps triggers in the feet of the player
				Transform[] footStepsPositions = new Transform[] {
					anim.GetBoneTransform (HumanBodyBones.LeftToes),
					anim.GetBoneTransform (HumanBodyBones.RightToes)
				};
				for (int i = 0; i < footStepsPositions.Length; i++) {
					GameObject footStepClone = (GameObject)Instantiate (footStep, Vector3.zero, Quaternion.identity);
					footStepClone.name = footStepClone.name.Replace ("(Clone)", "");
					footStepClone.transform.SetParent (footStepsPositions [i]);
					footStepClone.transform.rotation = Quaternion.identity;
					footStepClone.transform.localPosition = new Vector3 (0, 0, 0);
					if (footStepsPositions [i] == anim.GetBoneTransform (HumanBodyBones.LeftToes)) {
						ragdollActivatorManager.leftFoot = footStepClone;
						footStepClone.GetComponent<footStep> ().footSide = global::footStep.footType.left;
					} else {
						ragdollActivatorManager.rightFoot = footStepClone;
						footStepClone.GetComponent<footStep> ().footSide = global::footStep.footType.right;
					}
				}
				//set the arrow in the back of the player
				GameObject arrowClone = (GameObject)Instantiate (arrow, Vector3.zero, Quaternion.identity);
				arrowClone.name = arrowClone.name.Replace ("(Clone)", "");
				arrowClone.transform.SetParent (anim.GetBoneTransform (HumanBodyBones.Head).parent);
				arrowClone.transform.position = transform.position + transform.up * 1.6f - transform.forward * 0.3f;
				arrowClone.transform.rotation = transform.rotation;
				//another list, with the arms of the player, for the aim mode
				Transform[] aimParts = new Transform[] {
					anim.GetBoneTransform (HumanBodyBones.LeftHand),
					anim.GetBoneTransform (HumanBodyBones.RightHand)
				};
				//set the part of every arm in the otherpowers script
				setPowerSettings (aimParts, shootZoneClone);
				//set the shoot zone in the hand of the player in the grab object script
				setGrabSettings (shootZoneClone);
				//get every part in the head of the player, to set their layer in ignore raycast
				//this is for the ragdoll to mecanim system, to avoid that the face of the player deforms in the transition from ragdoll to mecnanim
				GameObject head = anim.GetBoneTransform (HumanBodyBones.Head).gameObject;
				Component[] components = head.GetComponentsInChildren (typeof(Transform));
				foreach (Component c in components) {
					if (c.gameObject != head) {
						c.gameObject.layer = LayerMask.NameToLayer ("Ignore Raycast");
					}
				}
				//add the elements in the player's IK system
				//add the current IK position
				GameObject currentHandIKPosClone = (GameObject)Instantiate (currentHandIKPos, Vector3.zero, Quaternion.identity);
				currentHandIKPosClone.name = "currentHandIKPos"; 
				currentHandIKPosClone.transform.SetParent (chest);
				player.GetComponent<IKSystem> ().currentHandIKPos = currentHandIKPosClone.transform;
				currentHandIKPosClone.transform.localPosition = Vector3.zero;
				IKHandPos = transform.position + new Vector3 (-0.20f, 1.45f, 0.5f);
				//add the right and left IK positions
				for (int i = 0; i < aimParts.Length; i++) {
					if (aimParts [i] == anim.GetBoneTransform (HumanBodyBones.LeftHand)) {
						GameObject handIKPositionClone = (GameObject)Instantiate (handIKPosition, Vector3.zero, Quaternion.identity);
						handIKPositionClone.name = "leftHandIKPosition"; 
						handIKPositionClone.transform.SetParent (chest);
						handIKPositionClone.transform.position = IKHandPos;
						player.GetComponent<IKSystem> ().leftHandIKPos = handIKPositionClone.transform;
					}
					if (aimParts [i] == anim.GetBoneTransform (HumanBodyBones.RightHand)) {
						GameObject handIKPositionClone = (GameObject)Instantiate (handIKPosition, Vector3.zero, Quaternion.identity);
						handIKPositionClone.name = "rightHandIKPosition"; 
						handIKPositionClone.transform.SetParent (chest);
						IKHandPos.x *= (-1);
						handIKPositionClone.transform.position = IKHandPos;
						player.GetComponent<IKSystem> ().rightHandIKPos = handIKPositionClone.transform;
					}
				}
				//set the animator in the ragdill builder component
				if (GetComponent<ragdollBuilder> ()) {
					GetComponent<ragdollBuilder> ().getAnimator (anim);
				}
				ragdollActivatorManager.objectsToIgnore.Clear ();
				setJetpack (chest);
				setWeapons (chest);
			}
		}
	}
	void setPowerSettings(Transform[] parts,GameObject zone){
		otherPowers powers = player.GetComponent<otherPowers> ();
		powers.aimsettings.leftHand = parts [0].gameObject;
		powers.aimsettings.rightHand = parts [1].gameObject;
		powers.aimsettings.spine = chest.gameObject;
		powers.aimsettings.chest = chest.transform.GetChild (0).gameObject;
		powers.laser = zone.transform.Find ("laserPlayer").gameObject;
		powers.laser.SetActive(false);
		powers.shootsettings.shootZone = zone.transform;
		int charactersMaterials = gravityCenter.GetComponentInChildren<SkinnedMeshRenderer> ().sharedMaterials.Length;
		player.GetComponent<changeGravity> ().settings.materialToChange=new int[charactersMaterials];
	}
	void setGrabSettings(GameObject zone){
		grabObjects grab = player.GetComponent<grabObjects> ();
		grab.settings.particles [0] = zone.transform.Find ("moveObject").gameObject;
		grab.settings.particles [1] = zone.transform.Find ("chargeLaunchObject").gameObject;
	}
	void setWeapons(Transform parent){
		GameObject weaponsClone = (GameObject)Instantiate (weapons, player.transform.position, player.transform.rotation);
		weaponsClone.transform.SetParent (parent);
		weaponsClone.name = "Weapons";
		ragdollActivatorManager.objectsToIgnore.Add (weaponsClone);
		GameObject weaponsParent = new GameObject ();
		weaponsParent.transform.position = weaponsClone.transform.position;
		weaponsParent.transform.rotation = weaponsClone.transform.rotation;
		weaponsParent.name = "weaponsTransformInThirdPerson";
		weaponsParent.transform.SetParent (parent);
		weaponsManager.weaponsParent = weaponsClone.transform;
		weaponsManager.weaponsTransformInThirdPerson = weaponsParent.transform;
		weaponsManager.thirdPersonParent = parent;
		weaponsManager.getWeaponList ();
		for (int i = 0; i < weaponsManager.weaponsList.Count; i++) {
			//weaponsManager.weaponsList [i].setHandTransform ();
			weaponsManager.weaponsList [i].weapon.setCharacter( player);
		}
	}
	void setJetpack(Transform parent){
		GameObject jetPackClone = (GameObject)Instantiate (jetPack, player.transform.position, player.transform.rotation);
		jetPackClone.transform.position += transform.up * 1.4f - transform.forward * 0.25f;
		jetPackClone.transform.SetParent (parent);
		jetPackClone.name = "JetPack";
		ragdollActivatorManager.objectsToIgnore.Add (jetPackClone);
		jetpackManager.jetpack = jetPackClone;
		jetpackManager.thrustsParticles.Clear ();
		Component[] components = jetPackClone.GetComponentsInChildren (typeof(ParticleSystem));
		foreach (Component c in components) {
			jetpackManager.thrustsParticles.Add (c.GetComponent<ParticleSystem> ());
		}
	}
}