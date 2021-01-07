using UnityEngine;
using System.Collections;
public class AICharacterController : MonoBehaviour {
	public float jumpPower = 12;
	public float airSpeed = 6;
	public float airControl = 2;
	public float gravityMultiplier = 2;
	public float moveSpeedMultiplier = 1;
	public  float animSpeedMultiplier = 1;

	public float stationaryTurnSpeed = 180;
	public float movingTurnSpeed = 360;
	public float crouchHeightFactor = 0.6f;
	public float crouchChangeSpeed = 4;
	public float autoTurnThresholdAngle = 100;
	public float autoTurnSpeed = 2;
	public PhysicMaterial zeroFrictionMaterial;
	public PhysicMaterial highFrictionMaterial;
	public float jumpRepeatDelayTime = 0.25f;
	public float runCycleLegOffset = 0.2f;
	public float groundStickyEffect = 5f;

	public LayerMask groundCheckMask;
	public bool controllerPaused;
	public bool dead;
	public Transform spine;
	public Transform rayCastPosition;
	public bool onGround;
	Vector3 currentLookPos;
	float originalHeight;
	Animator animator;
	float lastAirTime;
	CapsuleCollider capsule;
	Vector3 moveInput;
	bool crouchInput;
	bool jumpInput;
	float turnAmount;
	float forwardAmount;
	Vector3 velocity;
	public bool lookingPathAfterJump;
	AINavMesh navMeshManager;
	float originalAnimationSpeed;
	bool lookAtPosition;

	void Start(){
		capsule = GetComponent<Collider>() as CapsuleCollider;
		originalHeight = capsule.height;
		capsule.center = Vector3.up*originalHeight*0.5f;
		animator = GetComponent<Animator>();
		currentLookPos = Camera.main.transform.position;
		navMeshManager = GetComponent<AINavMesh> ();
		originalAnimationSpeed = animSpeedMultiplier;
	}
	void Update(){
		
	}
	void LateUpdate(){
		if (lookAtPosition) {
			Quaternion rotationX =  Quaternion.FromToRotation (spine.transform.InverseTransformDirection(transform.up), rayCastPosition.InverseTransformDirection(transform.forward));
			Vector3 directionX = rotationX.eulerAngles;
			Quaternion rotationZ =  Quaternion.FromToRotation (spine.transform.InverseTransformDirection(transform.forward), rayCastPosition.transform.InverseTransformDirection(transform.up));
			Vector3 directionZ = rotationZ.eulerAngles;
			spine.transform.localEulerAngles = new Vector3 (directionX.x, spine.transform.localEulerAngles.y, -directionZ.z);
		}
	}
	// The Move function is designed to be called from a separate component
	// based on User input, or an AI control script
	public void Move(AINavMesh.AIMoveInfo inputInfo){
		if (inputInfo.moveInput.magnitude > 1) {
			inputInfo.moveInput.Normalize ();
		}
		// transfer input parameters to member variables.
		moveInput = inputInfo.moveInput;
		crouchInput = inputInfo.crouchInput;
		jumpInput = inputInfo.jumpInput;
		currentLookPos = inputInfo.currentLookPosition;
		lookAtPosition = inputInfo.lookAtTarget;
		// grab current velocity, we will be changing it.
		velocity = GetComponent<Rigidbody>().velocity;
		ConvertMoveInput(); // converts the relative move vector into local turn & fwd values
		TurnTowardsCameraForward(); // makes the character face the way the camera is looking
		PreventStandingInLowHeadroom(); // so the character's head doesn't penetrate a low ceiling
		ScaleCapsuleForCrouching(); // so you can fit under low areas when crouching
		ApplyExtraTurnRotation(); // this is in addition to root rotation in the animations
		GroundCheck(); // detect and stick to ground
		SetFriction(); // use low or high friction values depending on the current state
		// control and velocity handling is different when grounded and airborne:
		if (onGround){
			HandleGroundedVelocities();
		}
		else{
			HandleAirborneVelocities();
		}

		UpdateAnimator(); // send input and other state parameters to the animator
		// reassign velocity, since it will have been modified by the above functions.
		GetComponent<Rigidbody>().velocity = velocity;		
	}
	void ConvertMoveInput(){
		// convert the world relative moveInput vector into a local-relative
		// turn amount and forward amount required to head in the desired
		// direction. 
		Vector3 localMove = transform.InverseTransformDirection(moveInput);
		if (moveInput.magnitude > 0) {
			turnAmount = Mathf.Atan2 (localMove.x, localMove.z);
		} else {
			turnAmount = Mathf.Atan2 (0, 0);
		}
		forwardAmount = localMove.z;
	}
	void TurnTowardsCameraForward(){
		// automatically turn to face camera direction,
		// when not moving, and beyond the specified angle threshold
		if (Mathf.Abs(forwardAmount) < .01f || lookAtPosition){
			Vector3 lookDelta = transform.InverseTransformDirection(currentLookPos - transform.position);
			float lookAngle = Mathf.Atan2(lookDelta.x, lookDelta.z)*Mathf.Rad2Deg;
			// are we beyond the threshold of where need to turn to face the camera?
			if (Mathf.Abs(lookAngle) > autoTurnThresholdAngle){
				turnAmount += lookAngle*autoTurnSpeed*.001f;
			}
		}
	}
	void ApplyExtraTurnRotation(){
		// help the character turn faster (this is in addition to root rotation in the animation)
		float turnSpeed = Mathf.Lerp(stationaryTurnSpeed, movingTurnSpeed,forwardAmount);
		transform.Rotate (0, turnAmount * turnSpeed * Time.deltaTime * animSpeedMultiplier, 0);
	}
	void GroundCheck(){
		RaycastHit hit;
		if (velocity.y < jumpPower*.5f)	{
			onGround = false;
			GetComponent<Rigidbody>().useGravity = true;
			if (Physics.Raycast (transform.position + transform.up * .1f, -transform.up, out hit, .5f, groundCheckMask)) {
				// check whether we hit a non-trigger collider (and not the character itself)
				if (!hit.collider.isTrigger) {
					// this counts as being on ground.
					// stick to surface - helps character stick to ground - specially when running down slopes
					if (velocity.y <= 0) {
						GetComponent<Rigidbody> ().position = Vector3.MoveTowards (GetComponent<Rigidbody> ().position, hit.point,
							Time.deltaTime * groundStickyEffect);
					}
					onGround = true;
					if (lookingPathAfterJump) {
						navMeshManager.jumpEnded ();
						lookingPathAfterJump = false;
					}
					GetComponent<Rigidbody> ().useGravity = false;
				}
			} else {
				navMeshManager.recalculatePath();
			}
		}
		// remember when we were last in air, for jump delay
		if (!onGround) lastAirTime = Time.time;
	}
	void SetFriction(){
		if (onGround){
			// set friction to low or high, depending on if we're moving
			if (moveInput.magnitude == 0){
				// when not moving this helps prevent sliding on slopes:
				GetComponent<Collider>().material = highFrictionMaterial;
			}
			else{
				// but when moving, we want no friction:
				GetComponent<Collider>().material = zeroFrictionMaterial;
			}
		}
		else{
			// while in air, we want no friction against surfaces (walls, ceilings, etc)
			GetComponent<Collider>().material = zeroFrictionMaterial;
		}
	}
	void HandleGroundedVelocities(){
		velocity.y = 0;
		if (moveInput.magnitude == 0){
			// when not moving this prevents sliding on slopes:
			velocity.x = 0;
			velocity.z = 0;
		}
		// check whether conditions are right to allow a jump:
		bool animationGrounded = animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded");
		bool okToRepeatJump = Time.time > lastAirTime + jumpRepeatDelayTime;
		if (jumpInput && !crouchInput && okToRepeatJump && animationGrounded){
			// jump!
			onGround = false;
			velocity = moveInput*airSpeed;
			velocity.y = jumpPower;
			lookingPathAfterJump = true;
		}
	}
	void HandleAirborneVelocities(){
		// we allow some movement in air, but it's very different to when on ground
		// (typically allowing a small change in trajectory)
		Vector3 airMove = new Vector3(moveInput.x*airSpeed, velocity.y, moveInput.z*airSpeed);
		velocity = Vector3.Lerp(velocity, airMove, Time.deltaTime*airControl);
		GetComponent<Rigidbody>().useGravity = true;
		// apply extra gravity from multiplier:
		Vector3 extraGravityForce = (Physics.gravity*gravityMultiplier) - Physics.gravity;
		GetComponent<Rigidbody>().AddForce(extraGravityForce);
	}
	void UpdateAnimator(){
		// Here we tell the animator what to do based on the current states and inputs.
		// only use root motion when on ground:
		animator.applyRootMotion = onGround;
		// update the animator parameters
		animator.SetFloat("Forward", forwardAmount, 0.1f, Time.deltaTime);
		animator.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);
		animator.SetBool("Crouch", crouchInput);
		animator.SetBool("OnGround", onGround);
		if (!onGround){
			animator.SetFloat("Jump", velocity.y);
		}
		// calculate which leg is behind, so as to leave that leg trailing in the jump animation
		// (This code is reliant on the specific run cycle offset in our animations,
		// and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
		float runCycle =Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime + runCycleLegOffset, 1);
		float jumpLeg = (runCycle < 0.5f ? 1 : -1)*forwardAmount;
		if (onGround){
			animator.SetFloat("JumpLeg", jumpLeg);
		}
		// the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
		// which affects the movement speed because of the root motion.
		if (onGround && moveInput.magnitude > 0){
			animator.speed = animSpeedMultiplier;
		}
		else{
			// but we don't want to use that while airborne
			animator.speed = 1;
		}
	}
	public void OnAnimatorMove(){
		if (!controllerPaused) {
			// we implement this function to override the default root motion.
			// this allows us to modify the positional speed before it's applied.
			GetComponent<Rigidbody> ().rotation = animator.rootRotation;
			if (onGround && Time.deltaTime > 0) {
				Vector3 v = (animator.deltaPosition * moveSpeedMultiplier) / Time.deltaTime;
				// we preserve the existing y part of the current velocity.
				v.y = GetComponent<Rigidbody> ().velocity.y;
				GetComponent<Rigidbody> ().velocity = v;
			}
		}
	}
	void PreventStandingInLowHeadroom(){
		// prevent standing up in crouch-only zones
		if (!crouchInput){
			Ray crouchRay = new Ray(GetComponent<Rigidbody>().position + Vector3.up*capsule.radius*0.5f, Vector3.up);
			float crouchRayLength = originalHeight - capsule.radius*0.5f;
			if (Physics.SphereCast(crouchRay, capsule.radius*0.5f, crouchRayLength , groundCheckMask)){
				crouchInput = true;
			}
		}
	}
	void ScaleCapsuleForCrouching(){
		// scale the capsule collider according to
		// if crouching ...
		if (onGround && crouchInput && (capsule.height != originalHeight*crouchHeightFactor)){
			capsule.height = Mathf.MoveTowards(capsule.height, originalHeight*crouchHeightFactor,Time.deltaTime*4);
			capsule.center = Vector3.MoveTowards(capsule.center,
				Vector3.up*originalHeight*crouchHeightFactor*0.5f,Time.deltaTime*2);
		}
		// ... everything else 
		else if (capsule.height != originalHeight && capsule.center != Vector3.up*originalHeight*0.5f){
			capsule.height = Mathf.MoveTowards(capsule.height, originalHeight, Time.deltaTime*4);
			capsule.center = Vector3.MoveTowards(capsule.center, Vector3.up*originalHeight*0.5f, Time.deltaTime*2);
		}
	}
	public void pauseAction(bool state){
		controllerPaused=state;
		if (controllerPaused) {
			moveInput = Vector3.zero;
			crouchInput = false;
			jumpInput = false;
			currentLookPos = Vector3.zero;
			lookAtPosition = false;
		}
	}
	public void die(Vector3 damagePos){
		dead = true;
		lookAtPosition = false;
	}
	public void reduceVelocity(float newValue){
		animSpeedMultiplier = newValue;
	}
	public void normalVelocity(){
		animSpeedMultiplier = originalAnimationSpeed;
	}
}