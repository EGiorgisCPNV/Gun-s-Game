using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ragdollActivator : MonoBehaviour {
	public ragdollState currentState=ragdollState.animated;
	public float ragdollToMecanimBlendTime=0.5f;
	public float timeToShowMenu;
	public LayerMask layer;
	public bool onGround;
	public deathType typeOfDeath;
	public float maxRagdollVelocity;
	public float maxVelocityToGetUp;
	public float extraForceOnRagdoll;
	public healthState playerState;
	public bool canMove=true;
	public enum healthState{
		alive, dead, fallen
	}
	public enum ragdollState{
		animated, ragdolled, blendToAnim
	}
	public enum deathType{
		ragdoll, mecanim
	}
	public List<GameObject> objectsToIgnore=new List<GameObject>();
	[HideInInspector] public GameObject leftFoot;
	[HideInInspector] public GameObject rightFoot;
	List<BodyPart> bodyParts=new List<BodyPart>();
	List<Transform> objectsToIgnoreChildren=new List<Transform>();
	float mecanimToGetUpTransitionTime=0.05f;
	float ragdollingEndTime=-1;
	float deadMenuTimer;
	Vector3 ragdolledHipPosition;
	Vector3 ragdolledHeadPosition;
	Vector3 ragdolledFeetPosition;
	Vector3 playerVelocity;
	Vector3 damagePos;
	Vector3 damageDirection;
	Vector3 originalFirtPersonPivotPosition;
	GameObject character;
	GameObject gravityCenter;
	GameObject body;
	Transform rootMotion;
	Transform headTransform;
	Transform leftFootTransform, rightFootTransform;
	GameObject skeleton;
	CapsuleCollider capsule;
	bool ragdollAdded;
	Animator anim;
	playerController playerManager;
	playerCamera cameraManager;
	otherPowers powersManager;
	changeGravity gravityManager;
	Rigidbody mainRigidbody;
	Rigidbody hipsRigidbody;
	Rigidbody closestPart;
	menuPause pauseManager;
	RaycastHit hit;
	bool dropCamera;
	bool enableBehaviour;
	static int belly = Animator.StringToHash("die.belly");
	static int back = Animator.StringToHash("die.back");
	AnimatorStateInfo stateInfo;

	void Start (){
		leftFoot.SetActive (true);
		rightFoot.SetActive (true);
		gravityCenter = GameObject.Find ("gravityCenter");
		character = GameObject.Find ("Character");
		pauseManager = character.GetComponent<menuPause> ();
		body = gravityCenter.transform.GetChild (0).gameObject;
		setKinematic (true);
		for (int i = 0; i < objectsToIgnore.Count; i++) {
			Component[] childrens = objectsToIgnore[i].GetComponentsInChildren (typeof(Transform));
			foreach (Component c in childrens) {
				objectsToIgnoreChildren.Add (c.GetComponent<Transform> ());
			}
		}

		//store all the part inside the model of the player, in this case, his bones
		Component[] components = body.GetComponentsInChildren (typeof(Transform));
		foreach (Component c in components) {
			//the objects with the ignore raycast layer belong to the head, and those are not neccessary for the ragdoll
			if (c.gameObject.layer != LayerMask.NameToLayer ("Ignore Raycast") && !checkChildsObjectsToIgnore (c.gameObject)) {
				BodyPart bodyPart = new BodyPart ();
				bodyPart.transform = c as Transform;
				bodyParts.Add (bodyPart);
			}
		}
		anim = GetComponent<Animator> ();
		capsule = GetComponent<CapsuleCollider> ();
		rootMotion = anim.GetBoneTransform (HumanBodyBones.Hips);
		headTransform = anim.GetBoneTransform (HumanBodyBones.Head);
		leftFootTransform = anim.GetBoneTransform (HumanBodyBones.LeftFoot);
		rightFootTransform = anim.GetBoneTransform (HumanBodyBones.RightFoot);
		if (rootMotion) {
			skeleton = rootMotion.parent.gameObject;
		}
		playerManager = GetComponent<playerController> ();
		powersManager = GetComponent<otherPowers> ();
		gravityManager = GetComponent<changeGravity> ();
		mainRigidbody = GetComponent<Rigidbody> ();
		cameraManager = GameObject.Find ("Player Camera").GetComponent<playerCamera> ();
		hipsRigidbody = rootMotion.GetComponent<Rigidbody> ();
		components = body.GetComponentsInChildren (typeof(Rigidbody));
		if (components.Length > 0) {
			ragdollAdded = true;
		}
	}
	void Update() {
		//use this buttons to test the ragdoll
		if (Input.GetKeyDown (KeyCode.L)) {
			//die (transform.position);
			GetComponent<health>().setDamage(GetComponent<health>().healthAmount,transform.forward,transform.position+transform.up*1.5f,gameObject, gameObject, false);
		}
		//when the ragdoll is enabled
		if (playerState == healthState.dead) {
			//check if the player is on the ground, so he can get up
			if (Physics.Raycast (rootMotion.position + Vector3.up, -Vector3.up, out hit, 2, layer) && mainRigidbody.velocity.magnitude < maxVelocityToGetUp) {
				onGround = true;
				if (!dropCamera && gravityManager.settings.firstPersonView) {
					originalFirtPersonPivotPosition = Camera.main.transform.localPosition;
					StartCoroutine (dropOrPickCamera (true));
					dropCamera = true;
				}
			} else {
				onGround = false;
			}
			//enable the die menu after a few seconds and he is on the ground
			if (deadMenuTimer > 0 && onGround) {
				deadMenuTimer -= Time.deltaTime;
				if (deadMenuTimer < 0) {
					pauseManager.death ();
				}
			}
		}
		if (playerState == healthState.fallen) {
			//check if the player is on the ground, so he can get up
			if (mainRigidbody.velocity.magnitude < maxVelocityToGetUp) {
				onGround = true;
			} else {
				onGround = false;
			}
			//enable the die menu after a few seconds and he is on the ground
			if (deadMenuTimer > 0 && onGround) {
				deadMenuTimer -= Time.deltaTime;
			}
			if (deadMenuTimer <= 0) {
				getUp ();
			}
		}
		if (currentState == ragdollState.ragdolled) {
			//set the empty player gameObject position with the hips of the character
			transform.position = rootMotion.position;
			//prevent the ragdoll reachs a high velocity
			if (hipsRigidbody.velocity.y <= -maxRagdollVelocity) {
				Vector3 newVelocity = new Vector3 (hipsRigidbody.velocity.x, -maxRagdollVelocity, hipsRigidbody.velocity.z);
				hipsRigidbody.velocity = newVelocity;
			}
		}
		if (currentState == ragdollState.animated && enableBehaviour) {
			stateInfo = anim.GetCurrentAnimatorStateInfo(3);
			if (stateInfo.fullPathHash != belly && stateInfo.fullPathHash!=back){
				//allow the scripts work again
				gravityManager.death (false);
				powersManager.death (false);
				playerManager.changeScriptState (true);
				enableBehaviour = false;
				canMove = true;
			}
		}
	}

	//get the direction of the projectile that killed the player
	void deathDirection(Vector3 dir){
		damageDirection = dir;
	}
	//the player has dead, get the last damage position, and the rigidbody velocity of the player
	void die(Vector3 pos){
		canMove = false;
		playerState = healthState.dead;
		character.GetComponent<playerStatesManager> ().checkPlayerStates ();
		damagePos = pos;
		playerVelocity = mainRigidbody.velocity;
		//check if the player has a ragdoll, if he hasn't it, then use the mecanim instead, to avoid issues
		bool canUseRagdoll=false;
		if (!ragdollAdded) {
			typeOfDeath = deathType.mecanim;
		} else {
			if (!Physics.Raycast (transform.position + Vector3.up, -Vector3.up, out hit, 2, layer)) {
				canUseRagdoll = true;
			}
		}
		GetComponent<footStepManager> ().enableOrDisableFootSteps (false);
		//check if the player use mecanim for the death, and if the first person mode is enabled, to use animations instead ragdoll
		if ((typeOfDeath == deathType.mecanim || gravityManager.settings.firstPersonView) && !canUseRagdoll) {
			//disable the player and enable the gravity in the player's ridigdboby
			playerManager.changeScriptState (false);
			gravityManager.death (true);
			cameraManager.death (true);
			powersManager.death (true);
			//set the dead state in the mecanim
			anim.SetBool ("dead", true);
		}
		//else enable the ragdoll
		else {
			enableOrDisableRagdoll (true);
		}
		deadMenuTimer = timeToShowMenu;
	}
	//play the game again
	public void getUp(){
		if (playerState == healthState.dead) {
			GetComponent<health> ().resurrect ();
		}
		playerState = healthState.alive;
		onGround = false;
		GetComponent<footStepManager> ().enableOrDisableFootSteps (true);
		//check if the player use mecanim for the death, and if the first person mode is enabled, to use animations instead ragdoll
		if ((typeOfDeath == deathType.mecanim || gravityManager.settings.firstPersonView) && currentState == ragdollState.animated) {
			//set the get up animation in the mecanim
			anim.SetBool ("dead", false);
			anim.SetBool ("back", true);
			//enable again the player
			playerManager.enabled = true;
			gravityManager.death (false);
			cameraManager.death (false);
			powersManager.death (false);
			//reset the rotation of the player
			if (gravityManager.settings.firstPersonView) {
				if (dropCamera) {
					StartCoroutine (dropOrPickCamera (false));
					dropCamera = false;
				}
			}
			playerManager.changeScriptState (true);
			if (gravityManager.settings.firstPersonView) {
				capsule.enabled = false;
				capsule.enabled = true;
				canMove = true;
			}
		} 
		//else disable the ragdoll
		else {
			enableOrDisableRagdoll (false);
		}
		damageDirection = Vector3.zero;
		resetLastTimeMoved ();
	}
	public void damageToFall(){
		if (!gravityManager.settings.firstPersonView && ragdollAdded) {
			canMove = false;
			playerState = healthState.fallen;
			character.GetComponent<playerStatesManager> ().checkPlayerStates ();
			damagePos = transform.position;
			playerVelocity = mainRigidbody.velocity;
			GetComponent<footStepManager> ().enableOrDisableFootSteps (false);
			enableOrDisableRagdoll (true);
			deadMenuTimer = timeToShowMenu;
		}
	}
	IEnumerator dropOrPickCamera(bool state){
		Vector3 targetPosition = Vector3.zero;
		Vector3 currentPosition = Vector3.zero;
		if (state) {
			Camera.main.transform.SetParent (null);
			if (Physics.Raycast (Camera.main.transform.position, -Vector3.up, out hit, Mathf.Infinity, layer)) {
				targetPosition = hit.point+transform.up*0.3f;
			}
			currentPosition = Camera.main.transform.position;
		} else {
			Camera.main.transform.SetParent (cameraManager.pivot.transform);
			targetPosition = originalFirtPersonPivotPosition;
			currentPosition = Camera.main.transform.localPosition;
		}
		float i = 0.0f;
		while (i < 1.0f) {
			i += Time.deltaTime * 3;
			if (state) {
				Camera.main.transform.position = Vector3.Lerp (currentPosition, targetPosition, i);
			} else {
				Camera.main.transform.localPosition = Vector3.Lerp (currentPosition, targetPosition, i);
				Camera.main.transform.localRotation = Quaternion.Slerp (Camera.main.transform.localRotation, Quaternion.identity, i);
			}
			yield return null;
		}
	}
	//public property that can be set to toggle between ragdolled and animated character
	public void enableOrDisableRagdoll(bool value){
		if (value) {
			if (currentState == ragdollState.animated) {
				//transition from animated to ragdolled
				body.gameObject.transform.parent = null;
				rootMotion.parent = null;
				body.gameObject.transform.rotation = new Quaternion (0, body.gameObject.transform.rotation.y, 0, body.gameObject.transform.rotation.w);
				rootMotion.parent = skeleton.transform;
				setKinematic (false);
				anim.enabled = false;
				currentState = ragdollState.ragdolled;
				//pause the scripts to stop any action of the player 
				playerManager.changeScriptState( false);
				gravityManager.death (true);
				cameraManager.death (true);
				powersManager.death (true);
				capsule.isTrigger = true;
			} 
		} else {
			if (currentState == ragdollState.ragdolled) {
				//transition from ragdolled to animated through the blendToAnim state
				setKinematic (true);
				//store the state change time
				ragdollingEndTime = Time.time; 
				anim.enabled = true;
				currentState = ragdollState.blendToAnim;  
				//store the ragdolled position for blending
				foreach (BodyPart b in bodyParts) {
					b.storedRotation = b.transform.rotation;
					b.storedPosition = b.transform.position;
				}
				//save some key positions
				ragdolledFeetPosition = 0.5f * (leftFootTransform.position + rightFootTransform.position);
				ragdolledHeadPosition = headTransform.position;
				ragdolledHipPosition = rootMotion.position;
				//start the get up animation checking if the character is on his back or face down, to play the correct animation
				if (rootMotion.up.y > 0) { 
					anim.SetBool ("back", true);
				} else {
					anim.SetBool ("belly", true);
				}
			}
		}	
	}
	//set the state of all the rigidbodies inside the character
	//kinematic is enabled or disabled according to the state
	void setKinematic(bool state){
		//if state== false, it means the player has dead, so get the position of the projectile that kills him,
		//and them the closest rigidbody of the character, to add velocity in the opposite direction to that part of the player
		if (!state) {
			closestPart = searchClosestBodyPart ();
		}
		Component[] components = body.GetComponentsInChildren (typeof(Collider));
		foreach (Component c in components) {
			//if the collider is not trigger, set its state
			Collider colliderPart = c.GetComponent<Collider> ();
			if (!colliderPart.isTrigger) {
				//set the state of the colliders and rigidbodies inside the character to enable or disable them
				if (c.GetComponent<Rigidbody> ()) {
					Rigidbody rigidPart = c.GetComponent<Rigidbody> ();
					rigidPart.isKinematic = state;
					colliderPart.enabled = !state;
					//change the layer of the colliders in the ragdoll, so the camera has not problems with it
					if (!state) {
						c.gameObject.layer = LayerMask.NameToLayer ("Ignore Raycast");
					} else {
						c.gameObject.layer = LayerMask.NameToLayer ("Default");
					}
					//if state== false, it means the player has dead, so get the position of the projectile that kills him,
					//and them the closest rigidbody of the character, to add velocity in the opposite direction to that part of the player
					if (!state) {
						rigidPart.velocity = playerVelocity;
						if (rigidPart == closestPart) {
							//print (closestPart.name+" "+rigidPart.name);
							rigidPart.AddForce (rigidPart.mass * damageDirection * extraForceOnRagdoll,ForceMode.Impulse);
						}
					}
				} else {
					Physics.IgnoreCollision (GetComponent<Collider> (), colliderPart);
				}
			}
			//if the collider is trigger, it is the foot trigger for the feetsteps sounds, so set the opposite state, to avoid play the sounds when
			//the player is dead
			else {
				colliderPart.enabled = state;
			}
		}
	}
	//get the closest rigidbody to the projectile that killed the player, to add velocity with an opposite direction of the bullet
	Rigidbody searchClosestBodyPart(){
		float distance = 100;
		Rigidbody part = new Rigidbody ();
		Component[] components = body.GetComponentsInChildren (typeof(Rigidbody));
		foreach (Component c in components) {
			float currentDistance = Vector3.Distance (c.transform.position, damagePos);
			if (currentDistance < distance) {
				distance = currentDistance;
				part = c.GetComponent<Rigidbody> ();
			}
		}
		return part;
	}
	void LateUpdate(){
		//avoid to repeat the animations
		anim.SetBool ("back", false);
		anim.SetBool ("belly", false);
		if (currentState == ragdollState.blendToAnim) {
			if (Time.time <= ragdollingEndTime + mecanimToGetUpTransitionTime) {
				//set the position of all the parts of the character to match them with the animation
				Vector3 animatedToRagdolled = ragdolledHipPosition - rootMotion.position;
				Vector3 newRootPosition = body.transform.position + animatedToRagdolled;
				//use a raycast downwards and find the highest hit that does not belong to the character 
				RaycastHit[] hits = Physics.RaycastAll (new Ray (newRootPosition, Vector3.down)); 
				float distance = Mathf.Infinity;
				foreach (RaycastHit hit in hits) {
					if (!hit.transform.IsChildOf (body.transform)) {
						if (distance < Mathf.Max (newRootPosition.y, hit.point.y)) {
							distance = Mathf.Max (newRootPosition.y, hit.point.y);
						}
					}
				}
				if (distance != Mathf.Infinity) {
					newRootPosition.y = distance;
				}
				body.transform.position = newRootPosition;
				//set the rotation of all the parts of the character to match them with the animation
				Vector3 ragdolledDirection = ragdolledHeadPosition - ragdolledFeetPosition;
				ragdolledDirection.y = 0;
				Vector3 meanFeetPosition = 0.5f * (leftFootTransform.position + rightFootTransform.position);
				Vector3 animatedDirection = headTransform.position - meanFeetPosition;
				animatedDirection.y = 0;
				body.transform.rotation *= Quaternion.FromToRotation (animatedDirection.normalized, ragdolledDirection.normalized);
			}
			//compute the ragdoll blend amount in the range 0 to 1
			float ragdollBlendAmount = 1.0f - (Time.time - ragdollingEndTime - mecanimToGetUpTransitionTime) / ragdollToMecanimBlendTime;
			ragdollBlendAmount = Mathf.Clamp01 (ragdollBlendAmount);
			//to get a smooth transition from a ragdoll to animation, lerp the position of the hips 
			//and slerp all the rotations towards the ones stored when ending the ragdolling
			foreach (BodyPart b in bodyParts) {
				//this if is to avoid change the root of the character, only the actual body parts
				if (b.transform != body.transform) { 
					//position is only interpolated for the hips
					if (b.transform == rootMotion) {
						b.transform.position = Vector3.Lerp (b.transform.position, b.storedPosition, ragdollBlendAmount);
					}
					//rotation is interpolated for all body parts
					b.transform.rotation = Quaternion.Slerp (b.transform.rotation, b.storedRotation, ragdollBlendAmount);
				}
			}
			//if the ragdoll blend amount has decreased to zero, change to animated state
			if (ragdollBlendAmount == 0) {
				setPlayerToRegularState ();
				currentState = ragdollState.animated;
				return;
			}
		}
	}
	public bool checkChildsObjectsToIgnore(GameObject obj){
		bool value = false;
		if (!objectsToIgnore.Contains (obj)) {
			value = true;
		}
		for (int i = 0; i < objectsToIgnoreChildren.Count; i++) {
			if (obj.transform.IsChildOf (objectsToIgnoreChildren [i].transform)) {
				value = true;
			}
		}
		return value;
	}
	void setPlayerToRegularState(){
		//set the parent of every object to back everything to the situation before the player died
		transform.position = body.transform.position;
		transform.rotation = new Quaternion (0, body.transform.rotation.y, 0, body.transform.rotation.w);
		body.transform.parent = gravityCenter.transform;
		capsule.isTrigger = false;
		capsule.enabled = false;
		capsule.enabled = true;
		enableBehaviour = true;
		cameraManager.death (false);
	}
	public void resetLastTimeMoved(){
		playerManager.setLastTimeMoved ();
		cameraManager.setLastTimeMoved ();
		powersManager.setLastTimeFired ();
		GetComponent<playerWeaponsManager> ().setLastTimeFired ();
	}
	public class BodyPart{
		public Transform transform;
		public Vector3 storedPosition;
		public Quaternion storedRotation;
	}
}