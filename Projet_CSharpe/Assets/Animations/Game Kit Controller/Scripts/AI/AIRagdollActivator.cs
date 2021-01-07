using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class AIRagdollActivator : MonoBehaviour {
	public ragdollState currentState=ragdollState.animated;
	public float ragdollToMecanimBlendTime=0.5f;
	public LayerMask layerMask;
	public deathType typeOfDeath;
	public float maxRagdollVelocity;
	public GameObject body;
	public float timeToGetUp;
	public bool onGround;
	public float maxVelocityToGetUp;
	public float extraForceOnRagdoll;
	public string pauseOrPlayCharacterFunction;
	public string tagForColliders;
	public healthState playerState;
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
	public AudioClip deathSound;
	[HideInInspector] public List<BodyPart> bodyParts=new List<BodyPart>();
	float mecanimToGetUpTransitionTime=0.05f;
	float ragdollingEndTime=-1;
	float deadMenuTimer;
	Vector3 ragdolledHipPosition;
	Vector3 ragdolledHeadPosition;
	Vector3 ragdolledFeetPosition;
	Vector3 playerVelocity;
	Vector3 damagePos;
	Vector3 damageDirection;
	Transform rootMotion;
	Transform headTransform;
	Transform leftFootTransform, rightFootTransform;
	GameObject skeleton;
	CapsuleCollider capsule;
	Animator anim;
	Rigidbody mainRigidbody;
	Rigidbody hipsRigidbody;
	Rigidbody closestPart;
	bool enableBehaviour;
	static int belly = Animator.StringToHash("die.belly");
	static int back = Animator.StringToHash("die.back");
	AnimatorStateInfo stateInfo;

	void Start (){
		setKinematic (true);
		//store all the part inside the model of the player, in this case, his bones
		Component[] components = body.GetComponentsInChildren (typeof(Transform));
		foreach (Component c in components) {
			//the objects with the ignore raycast layer belong to the head, and those are not neccessary for the ragdoll
			if (c.gameObject.layer != LayerMask.NameToLayer ("Ignore Raycast") && !objectsToIgnore.Contains (c.gameObject) && !checkChildsObjectsToIgnore (c.gameObject)) {
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
		mainRigidbody = GetComponent<Rigidbody> ();
		hipsRigidbody = rootMotion.GetComponent<Rigidbody> ();
	}
	void Update() {
		//use this buttons to test the ragdoll
//		if (Input.GetKeyDown (KeyCode.L)) {
//			//die (transform.position);
//			GetComponent<health>().setDamage(GetComponent<health>().healthAmount,transform.forward,transform.position+transform.up*1.5f,gameObject, gameObject, false);
//		}
		//when the ragdoll is enabled
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
				if (deadMenuTimer < 0) {
					getUp ();
				}
			}
		}
		if (currentState == ragdollState.ragdolled) {
			//set the empty player gameObject position with the hips of the character
			transform.position = rootMotion.position;
			//prevent the ragdoll reachs a high velocity 
			if (hipsRigidbody.velocity.magnitude > maxRagdollVelocity && hipsRigidbody.velocity.y <= -maxRagdollVelocity) {
				Vector3 newVelocity = new Vector3 (hipsRigidbody.velocity.x, -maxRagdollVelocity, hipsRigidbody.velocity.z);
				hipsRigidbody.velocity = newVelocity;
			}
		}
		if (currentState == ragdollState.animated && enableBehaviour) {
			stateInfo = anim.GetCurrentAnimatorStateInfo(3);
			if (stateInfo.fullPathHash != belly && stateInfo.fullPathHash!=back){
				//print ("up");
				SendMessage (pauseOrPlayCharacterFunction, false);
				enableBehaviour = false;
			}
		}
	}
	//get the direction of the projectile that killed the player
	public void deathDirection(Vector3 dir){
		damageDirection = dir;
	}
	//the player has dead, get the last damage position, and the rigidbody velocity of the player
	public void die(Vector3 pos){
		mainRigidbody.isKinematic = true;
		SendMessage (pauseOrPlayCharacterFunction, true);
		playerState = healthState.dead;
		damagePos = pos;
		playerVelocity = mainRigidbody.velocity;
		//check if the player has a ragdoll, if he hasn't it, then use the mecanim instead, to avoid issues
		bool canUseRagdoll=false;
		Component[] components = body.GetComponentsInChildren (typeof(Rigidbody));
		if (components.Length == 0) {
			typeOfDeath = deathType.mecanim;
		}
		else {
			if (!Physics.Raycast (transform.position + Vector3.up, -Vector3.up, 2, layerMask)) {
				canUseRagdoll = true;
			}
		}
		if (GetComponent<footStepManager> ()) {
			GetComponent<footStepManager> ().enableOrDisableFootSteps (false);
		}
		//check if the player use mecanim for the death, and if the first person mode is enabled, to use animations instead ragdoll
		if (typeOfDeath == deathType.mecanim && !canUseRagdoll) {
			anim.SetBool ("dead", true);
		}
		//else enable the ragdoll
		else {
			enableOrDisableRagdoll (true);
		}
		tag = "Untagged";
		SendMessage ("removeFromPartnerList", SendMessageOptions.DontRequireReceiver);
		GetComponentInChildren<AudioSource> ().PlayOneShot (deathSound);
	}
	//play the game again
	public void getUp(){
		mainRigidbody.isKinematic = false;
		onGround = false;
		if (GetComponent<footStepManager> ()) {
			GetComponent<footStepManager> ().enableOrDisableFootSteps (true);
		}
		//check if the player use mecanim for the death, to use animations instead ragdoll
		if (typeOfDeath == deathType.mecanim) {
			//set the get up animation in the mecanim
			anim.SetBool ("dead", false);
			anim.SetBool ("back", true);
		} 
		//else disable the ragdoll
		else {
			enableOrDisableRagdoll (false);
		}
		damageDirection = Vector3.zero;
	}
	public void damageToFall(){
		mainRigidbody.isKinematic = true;
		SendMessage (pauseOrPlayCharacterFunction, true);
		playerState = healthState.fallen;
		damagePos = transform.position;
		playerVelocity = mainRigidbody.velocity;
		//check if the player has a ragdoll, if he hasn't it, then use the mecanim instead, to avoid issues
		bool canUseRagdoll=false;
		Component[] components = body.GetComponentsInChildren (typeof(Rigidbody));
		if (components.Length == 0) {
			typeOfDeath = deathType.mecanim;
		}
		else {
			if (!Physics.Raycast (transform.position + Vector3.up, -Vector3.up, 2, layerMask)) {
				canUseRagdoll = true;
			}
		}
		if (GetComponent<footStepManager> ()) {
			GetComponent<footStepManager> ().enableOrDisableFootSteps (false);
		}
		//check if the player use mecanim for the death, and if the first person mode is enabled, to use animations instead ragdoll
		if (typeOfDeath == deathType.mecanim && !canUseRagdoll) {
			anim.SetBool ("dead", true);
		}
		//else enable the ragdoll
		else {
			enableOrDisableRagdoll (true);
		}
		deadMenuTimer = timeToGetUp;
	}
	public void pushCharacter(Vector3 direction){
		damageToFall ();
		damagePos = hipsRigidbody.position;
		damageDirection = direction;
		setKinematic (false);
	}
	//public property that can be set to toggle between ragdolled and animated character
	public void enableOrDisableRagdoll(bool value){
		if (value) {
			if (currentState == ragdollState.animated) {
				//transition from animated to ragdolled
				body.gameObject.transform.SetParent( null);
				rootMotion.SetParent(null);
				body.gameObject.transform.rotation = new Quaternion (0, body.gameObject.transform.rotation.y, 0, body.gameObject.transform.rotation.w);
				rootMotion.SetParent (skeleton.transform);
				setKinematic (false);
				anim.enabled = false;
				currentState = ragdollState.ragdolled;
				capsule.enabled = false;
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
					if (playerState != healthState.fallen) {
						if (!state) {
							c.gameObject.tag = tagForColliders;
						} else {
							c.gameObject.tag = "Untagged";
						}
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
							print (hit.distance);
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
		for (int i = 0; i < objectsToIgnore.Count; i++) {
			if (objectsToIgnore [i]) {
				if (obj.transform.IsChildOf (objectsToIgnore [i].transform)) {
					value = true;
				}
			}
		}
		return value;
	}
	void setPlayerToRegularState(){
		//set the parent of every object to back everything to the situation before the player died
		transform.position = body.transform.position;
		transform.rotation = new Quaternion (0, body.transform.rotation.y, 0, body.transform.rotation.w);
		body.transform.SetParent (transform);
		//allow the scripts work again
		capsule.enabled = false;
		capsule.enabled = true;
		enableBehaviour = true;
	}
	[System.Serializable]
	public class BodyPart{
		public Transform transform;
		public Vector3 storedPosition;
		public Quaternion storedRotation;
	}
}
