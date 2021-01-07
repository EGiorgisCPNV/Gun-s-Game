using UnityEngine;
using System.Collections;
public class playerController : MonoBehaviour {
	[Range(0,1)]public float walkSpeed = 1;
	public float jumpPower = 12;
	public float airSpeed = 6;
	public float airControl = 2;
	public float gravityMultiplier = 2;
	public bool usingAnimatorInFirstMode;
	public float gravityForce = -9.8f;
	public float stationaryTurnSpeed = 180;
	public float movingTurnSpeed = 200;
	public PhysicMaterial zeroFrictionMaterial;
	public PhysicMaterial highFrictionMaterial;
	public LayerMask layer;
	public float autoTurnSpeed = 2;	
	public float aimTurnSpeed = 10;
	public bool enabledRegularJump;
	public bool enabledDoubleJump;
	public int maxNumberJumpsInAir;
	public bool damageFallEnabled;
	public bool holdJumpSlowDownFall;
	public float slowDownGravityMultiplier;
	public float maxTimeInAirDamage;
	public float rayDistance;
	public bool canUseSphereMode;
	public bool useLandMark;
	public float maxLandDistance;
	public float minDistanceShowLandMark;
	public GameObject landMark;
	public bool onGround;
	public bool jump;
	public bool aiming;
	public bool aimingInFirstPerson;
	public bool crouch = false;
	public bool jetPackEquiped;
	public bool usingJetpack;
	public bool sphereModeActive;
	public bool flyModeActive;
	public bool slowingFall;
	public bool canMove=true;
	[HideInInspector] public Animator animator;
	[HideInInspector] public Vector3 moveInput;
	[HideInInspector] public float moveSpeedMultiplier = 1;
	[HideInInspector] public float animSpeedMultiplier = 1;
	[HideInInspector] public float originalHeight;
	[HideInInspector] public bool jumpInput;
	[HideInInspector] public bool isMoving;
	[HideInInspector] public bool doubleJump;
	[HideInInspector] public bool powerActive;
	[HideInInspector] public float h;
	[HideInInspector] public float v;
	[HideInInspector] public float lastTimeFalling;
	[HideInInspector] public GameObject pCamera;
	[HideInInspector] public Vector3 currentVelocity;
	[HideInInspector] public bool driving;
	public bool usingAnimator;
	GameObject currentVehicle;
	float turnAmount;
	float forwardAmount;
	float lastTimeAir;
	float readyToJumpTime = 0.25f;
	float runCycleLegOffset = 0.2f;
	float groundAdherence = 5;
	float readyToDoubleJumpTime = 0.2f;
	float lastJumpTime;
	float jetpackForce;
	float jetpackAirControl;
	float jetpackAirSpeed;
	public float flyModeForce;
	public float flyModeAirControl;
	public float flyModeAirSpeed;
	public float flyModeTurboSpeed;
	public bool flyModeTurboActive;
	float originalGravityMultiplier;
	float lastDoubleJumpTime;
	float originalJumpPower;
	float lastTimeMoved;
	bool slowingFallInput;
	Vector3 normal = new Vector3 (0, 1, 0);
	CapsuleCollider capsule;
	int c = 0;
	int jumpsAmount;
	RaycastHit hit;
	inputManager input;
	GameObject pivot;
	Rigidbody mainRigidbody;
	otherPowers powersManager;
	playerWeaponsManager weaponsManager;
	Transform head;
	changeGravity  gravityManager;
	Transform landMark1, landMark2;
	headBob headBobManager;
	playerCamera playerCameraManager;
	menuPause pauseMenuManager;

	void Start () {
		animator = GetComponent<Animator>();
		capsule = GetComponent<Collider>() as CapsuleCollider;
		//set the collider center in the correct place
		originalHeight = capsule.height;
		capsule.center = Vector3.up * capsule.height * 0.5f;
		//ge the player camera and the pivot
		pCamera = GameObject.Find ("Player Camera");
		pivot = pCamera.transform.GetChild (0).gameObject;
		//get the input manager, to check the key press
		input = transform.parent.GetComponent<inputManager> ();
		mainRigidbody = GetComponent<Rigidbody> ();
		powersManager = GetComponent<otherPowers> ();
		weaponsManager = GetComponent<playerWeaponsManager> ();
		head = animator.GetBoneTransform (HumanBodyBones.Head);
		originalGravityMultiplier = gravityMultiplier;
		gravityManager = GetComponent<changeGravity> ();
		originalJumpPower = jumpPower;
		if (useLandMark) {
			landMark1 = landMark.transform.GetChild (0);
			landMark2 = landMark.transform.GetChild (1);
			landMark.SetActive (false);
		}
		headBobManager = pCamera.GetComponentInChildren<headBob> ();
		playerCameraManager = pCamera.GetComponent<playerCamera> ();
		pauseMenuManager = transform.parent.GetComponent<menuPause> ();
	}
	public void enableOrDisableSphereMode(bool state){
		if (canUseSphereMode) {
			sphereModeActive = state;
		}
	}
	void Update(){
		if (!jetPackEquiped && !flyModeActive && canMove) {
			//check if the crouch button has been pressed
			if (input.checkInputButton ("Crouch", inputManager.buttonType.getKeyDown)) {
				crouching ();
			}
			//check if the jump button has been pressed
			if (enabledRegularJump && input.checkInputButton ("Jump", inputManager.buttonType.getKeyDown)) {
				jumpInput = true;
			}
			//if the player is in the air, without the gravity power enabled floating in the air and the double jump is enabled
			if (!onGround && !powerActive){
				if (enabledDoubleJump) {
					//then check the last time the jump button has been pressed, so the player can jump again 
					bool readyToJump = Time.time > lastJumpTime + readyToDoubleJumpTime;
					//jump again
					if (readyToJump && jumpsAmount < maxNumberJumpsInAir && input.checkInputButton ("Jump", inputManager.buttonType.getKeyDown)) {
						doubleJump = true;
					}
				}
				if (holdJumpSlowDownFall){
					float waitTimeToSlowDown = lastJumpTime + 0.2f;
					if (enabledDoubleJump && doubleJump) {
						waitTimeToSlowDown += lastDoubleJumpTime + 1;
					}
					if(Time.time > waitTimeToSlowDown){
						if (input.checkInputButton ("Jump", inputManager.buttonType.getKey)) {
							gravityMultiplier = slowDownGravityMultiplier;
							if (!slowingFall) {
								slowingFallInput = true;
							}
						}
					}
					if (input.checkInputButton ("Jump", inputManager.buttonType.getKeyUp)) {
						gravityMultiplier = originalGravityMultiplier;
					}
				}
			}
		} 
		if (useLandMark) {
			if (Physics.Raycast (transform.position, -transform.up, out hit, maxLandDistance, layer) && !onGround && canMove) {
				if (useLandMark) {
					if (hit.distance >= minDistanceShowLandMark) {
						if (!landMark.activeSelf) {
							landMark.SetActive (true);
						}
						landMark.transform.position = hit.point + hit.normal * 0.02f;
						Vector3 myForward = Vector3.Cross (landMark.transform.right, hit.normal);
						Quaternion dstRot = Quaternion.LookRotation (myForward, hit.normal);
						landMark.transform.rotation = dstRot;
						landMark1.transform.Rotate (0, 100 * Time.deltaTime, 0);
						landMark2.transform.Rotate (0, -100 * Time.deltaTime, 0);
					} else {
						if (useLandMark) {
							if (landMark.activeSelf) {
								landMark.SetActive (false);
							}
						}
					}
				}
			} else {
				if (useLandMark) {
					if (landMark.activeSelf) {
						landMark.SetActive (false);
					}
				}
			}
		}
	}
	void FixedUpdate (){
		//convert the input from keyboard or a touch screen into values to move the player, given the camera direction
		if (canMove) {
			h = input.getMovementAxis ("keys").x;
			v = input.getMovementAxis ("keys").y;
		}
		//get the axis of the player camera, to move him properly
		if (playerCameraManager.cameraType == playerCamera.typeOfCamera.free) {
			moveInput = (v * pCamera.transform.forward + h * playerCameraManager.mainCameraTransform.right) * walkSpeed;	
		} 
		if (playerCameraManager.cameraType == playerCamera.typeOfCamera.locked) {
			moveInput = (v * playerCameraManager.mainCameraTransform.forward + h * playerCameraManager.mainCameraTransform.right) * walkSpeed;	
		}
		//isMoving is true if the player is moving, else is false
		isMoving = Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;
		if (moveInput.magnitude > 1) {
			moveInput.Normalize ();
		}
		//get the velocity of the rigidbody
		if (!powerActive) {
			currentVelocity = mainRigidbody.velocity;
		}
		//convert the global movement in local
		getMoveInput ();
		//look in camera direction when the player is aiming
		lookCameraDirection ();
		//add an extra rotation to the player to get a better control of him
		addExtraRotation ();
		//check when the player is on ground
		checkOnGround (); 
		//if the animator is used, then
		if (usingAnimator) {
			//update mecanim
			updateAnimator ();
		} else {
			//else, apply force to the player's rigidbody
			if (onGround) {
				Vector3 force = moveInput * 10 * animSpeedMultiplier;
				//substract the local Y axis velocity of the rigidbody
				force = force - transform.up * transform.InverseTransformDirection (force).y;
				mainRigidbody.AddForce (force);
			}
		}
		//check if the player is on ground or in air
		//also set the friction of the character if he is on the ground or in the air
		if (onGround) {
			onGroundVelocity ();
			if (c == 0) {
				gravityManager.onGroundOrOnAir (true);
				c = 1;
				//send a message to the headbob in the camera, when the player lands from a jump
				headBobManager.setState("Jump End");
				//set the number of jumps made by the player since this moment
				jumpsAmount=0;
				//check the last time since the player is in the air, falling in its gravity direction
				//if the player has been in the air more time than maxTimeInAirDamage and his velocity is higher than 15, then apply damage
				if(damageFallEnabled && Time.time > lastTimeFalling + maxTimeInAirDamage && !powerActive && mainRigidbody.velocity.magnitude>15){
					//get the last time since the player is in the air and his velocity, and call the health damage function
					float damageValue=Mathf.Abs(Time.time-lastTimeFalling)*mainRigidbody.velocity.magnitude;
					if(damageValue>powersManager.settings.healthBar.maxValue){
						damageValue=powersManager.settings.healthBar.maxValue;
					}
					GetComponent<health>().setDamage(damageValue,transform.up,transform.position+transform.up,gameObject, gameObject, false);
				}
			}
			//change the collider material when the player moves and when the player is not moving
			if (moveInput.magnitude == 0) {
				capsule.material = highFrictionMaterial;
			} else {
				capsule.material = zeroFrictionMaterial;
			}
			if (headBobManager.headBobEnabled) {
				if (isMoving) {
					if (powersManager.running) {
						headBobManager.setState ("Running");
					} else {
						headBobManager.setState ("Walking");
					}
					if (headBobManager.useDynamicIdle) {
						setLastTimeMoved ();
					}
				} else {
					if (headBobManager.useDynamicIdle && canMove && !pauseMenuManager.usingDevice && gravityManager.settings.firstPersonView) {
						if (Time.time > lastTimeMoved + headBobManager.timeToActiveDynamicIdle && 
							Time.time > playerCameraManager.getLastTimeMoved() + headBobManager.timeToActiveDynamicIdle && 
							Time.time > weaponsManager.getLastTimeFired() + headBobManager.timeToActiveDynamicIdle &&
							Time.time > powersManager.getLastTimeFired() + headBobManager.timeToActiveDynamicIdle) {
							headBobManager.setState ("Dynamic Idle");
						} else {
							headBobManager.setState ("Static Idle");
						}
					} else {
						headBobManager.setState ("Static Idle");
					}
				}
			}
		}
		//the player is in the air, so
		else {
			//call the air velocity function
			onAirVelocity ();
			if (c == 1) {
				//set in other script this state
				gravityManager.onGroundOrOnAir(false);
				c = 0;
				//if the players was aiming, disable this mode
				if(powersManager.aimsettings.aiming){
					if (!gravityManager.settings.firstPersonView) {
						powersManager.deactivateAimMode ();
					}
					weaponsManager.playerInAir ();
				}
				lastTimeFalling=Time.time;
			}
			capsule.material = zeroFrictionMaterial;
			headBobManager.setState ("Air");
			if (headBobManager.useDynamicIdle) {
				setLastTimeMoved ();
			}
		}
		if (usingJetpack) {
			Vector3 airMove = moveInput * jetpackAirSpeed + transform.InverseTransformDirection(currentVelocity).y * transform.up;
			currentVelocity = Vector3.Lerp (currentVelocity, airMove, Time.deltaTime * jetpackAirControl);	
			mainRigidbody.AddForce(-gravityForce * mainRigidbody.mass * transform.up * jetpackForce);
		}
		if (flyModeActive) {


			Vector3 cameraForward = playerCameraManager.mainCameraTransform.TransformDirection(Vector3.forward);
			cameraForward = cameraForward.normalized;
			//Vector3 cameraRight = new Vector3(cameraForward.z, 0, -cameraForward.x);
			Vector3 targetDirection = v * playerCameraManager.mainCameraTransform.forward + h * playerCameraManager.mainCameraTransform.right;
				//cameraForward * v + cameraRight * h;
			if(isMoving && targetDirection != Vector3.zero){
				Quaternion targetRotation = Quaternion.LookRotation (targetDirection, pCamera.transform.up);
				targetRotation *= Quaternion.Euler (90, 0, 0);
				Quaternion newRotation = Quaternion.Slerp(mainRigidbody.rotation, targetRotation, flyModeAirControl * Time.deltaTime);
				mainRigidbody.MoveRotation (newRotation);
				lastDirection = targetDirection;
			}
			if(!(Mathf.Abs(h) > 0.9f || Mathf.Abs(v) > 0.9f)){
				Vector3 repositioning = lastDirection;
				if(repositioning != Vector3.zero){
					repositioning.y = 0;
					Quaternion targetRotation = Quaternion.LookRotation (repositioning,  pCamera.transform.up);
					Quaternion newRotation = Quaternion.Slerp(mainRigidbody.rotation, targetRotation, flyModeAirControl * Time.deltaTime);
					mainRigidbody.MoveRotation (newRotation);
				}
			}
			if (flyModeTurboActive) {
				mainRigidbody.AddForce (targetDirection * flyModeForce * flyModeTurboSpeed);
			} else {
				mainRigidbody.AddForce (targetDirection * flyModeForce);
			}

//			Vector3 airMove = moveInput * flyingModeAirSpeed + transform.InverseTransformDirection(currentVelocity).y * transform.up;
//			currentVelocity = Vector3.Lerp (currentVelocity, airMove, Time.deltaTime * flyingModeAirControl);	
//			mainRigidbody.AddForce(-settings.gravityForce * mainRigidbody.mass * transform.up * flyingModeForce);
		}
		//in case the player is using the gravity power, the update of the rigidbody velocity stops
		if (!powerActive) {
			mainRigidbody.velocity = currentVelocity;
		}
	}

	Vector3 lastDirection;

	//convert the global movement into local movement
	void getMoveInput(){
		Vector3 localMove = transform.InverseTransformDirection (moveInput);
		//get the amount of rotation added to the character mecanim
		if (moveInput.magnitude > 0) {
			turnAmount = Mathf.Atan2 (localMove.x, localMove.z);
		} else {
			turnAmount = Mathf.Atan2 (0, 0);
		}
		//get the amount of movement in forward direction
		forwardAmount = localMove.z;
	}
	//function used when the player is aim mode, so the character will rotate in the camera direction
	void lookCameraDirection (){
		if (aiming) {
			crouch=false;
			//get the camera direction, getting the local direction in any surface
			Vector3 forward = playerCameraManager.mainCameraTransform.TransformDirection (Vector3.forward);
			forward = forward - transform.up * transform.InverseTransformDirection (forward).y;
			forward = forward.normalized;
			Vector3 targetDirection = forward;
			Quaternion targetRotation = Quaternion.LookRotation (targetDirection, transform.up);
			Quaternion newRotation = Quaternion.Slerp (mainRigidbody.rotation, targetRotation, aimTurnSpeed * Time.deltaTime);
			mainRigidbody.MoveRotation (newRotation);
			//if the player is not moving, set the turnamount to rotate him around, setting its turn animation properly
			if (!isMoving) {
				Vector3 lookDelta = transform.InverseTransformDirection (targetDirection*100);
				float lookAngle = Mathf.Atan2 (lookDelta.x, lookDelta.z) * Mathf.Rad2Deg;
				turnAmount += lookAngle * autoTurnSpeed * .01f * 6;
			}
		} 
	}
	void addExtraRotation (){
		if (!aiming) {
			//add an extra rotation to the player to get a smooth movement
			float turnSpeed = Mathf.Lerp (stationaryTurnSpeed, movingTurnSpeed, forwardAmount);
			transform.Rotate (0, turnAmount * turnSpeed * Time.deltaTime * 1.5f, 0);
		}
	}
	//set the normal of the player every time it is changed in the other script
	public void setNormalCharacter(Vector3 norm){
		normal = norm;
	}
	//check if the player jumps
	void onGroundVelocity()	{
		if (moveInput.magnitude == 0) {
			currentVelocity = Vector3.zero;
		}
		//check when the player is able to jump, according to the timer and the animator state
		bool animationGrounded =false;
		if (usingAnimator) {
			animationGrounded = animator.GetCurrentAnimatorStateInfo (0).IsName ("Grounded");
		} else {
			animationGrounded = onGround;
		}
		bool readyToJump = Time.time > lastTimeAir + readyToJumpTime;
		//if the player jumps, apply velocity to its rigidbody
		if (jumpInput && readyToJump && animationGrounded && !aiming) {
			onGround = false;
			currentVelocity = moveInput * airSpeed;
			currentVelocity = currentVelocity + transform.up * jumpPower;
			jump = true;
			lastJumpTime = Time.time;
			//this is used for the headbod, to shake the camera when the player jumps
			headBobManager.setState ("Jump Start");
		}
		jumpInput = false;
	}
	//check if the player is in the air falling, applying the gravity force in his local up negative
	void onAirVelocity(){
		if (!powerActive && !usingJetpack && !flyModeActive) {
			//when the player falls, allow him to move to his right, left, forward and backward with WASD
			Vector3 airMove = moveInput * airSpeed + transform.InverseTransformDirection(currentVelocity).y * transform.up;
			currentVelocity = Vector3.Lerp (currentVelocity, airMove, Time.deltaTime * airControl);
			//also, apply force in his local negative Y Axis
			if(!onGround){
				if (slowingFallInput) {
					slowingFall = true;
					slowingFallInput = false;
				}
				mainRigidbody.AddForce(gravityForce * mainRigidbody.mass * transform.up);
				Vector3 extraGravityForce = (transform.up * gravityForce * gravityMultiplier) + transform.up * (-gravityForce);
				mainRigidbody.AddForce (extraGravityForce);
				//print (extraGravityForce.normalized);
			}
			//also apply force if the player jumps again
			if(doubleJump){
				currentVelocity += moveInput * airSpeed;
				currentVelocity = currentVelocity + transform.up * jumpPower;
				jumpsAmount++;
				lastDoubleJumpTime = Time.time;
			}
			doubleJump=false;
		}
	}
	//update the animator values
	void updateAnimator(){
		//set the rootMotion according to the player state
		animator.applyRootMotion = onGround;
		//if the player is not aiming, set the forward direction
		if(!aiming){
			animator.SetFloat ("Forward", forwardAmount, 0.1f, Time.deltaTime);
		}
		//else, set its forward to 0, to prevent any issue
		else{
			//this value is set to 0, because the player uses another layer of the mecanim to move while he is aiming
			animator.SetFloat ("Forward",0);
		}
		animator.SetFloat ("Turn", turnAmount, 0.1f, Time.deltaTime);
		if (usingJetpack || flyModeActive) {
			animator.SetBool ("OnGround", true);
		} else {
			animator.SetBool ("OnGround", onGround);
		}
		animator.SetBool ("Crouch", crouch);
		animator.SetBool ("Aiming", aiming);
		animator.SetBool ("Moving", isMoving);
		animator.SetFloat ("Horizontal", h);
		animator.SetFloat ("Vertical", v);
		if (!onGround) {
			//when the player enables the power gravity and he is floating in the aire, set this value to 0 to set 
			//the look like floating animation
			if(powerActive){
				animator.SetFloat ("Jump", 0);
			}
			//else set his jump value as his current rigidbody velocity
			else{
				animator.SetFloat ("Jump", transform.InverseTransformDirection (mainRigidbody.velocity).y);
			}
		}
		if (usingJetpack || flyModeActive) {
			animator.SetFloat ("Forward", 0);
			animator.SetFloat ("Turn", 0);
		}
		//this value is used to know in which leg the player has to jump, left of right
		float runCycle = Mathf.Repeat (animator.GetCurrentAnimatorStateInfo (0).normalizedTime + runCycleLegOffset, 1);
		float jumpLeg = (runCycle < 0.5f ? 1 : -1) * forwardAmount;
		if (onGround) {
			animator.SetFloat ("JumpLeg", jumpLeg);
		}
		//if the player is on ground and moving set the speed of his animator to the properly value
		if (onGround && moveInput.magnitude > 0) {
			animator.speed = animSpeedMultiplier;
		} else {
			animator.speed = 1;
		}
	}
	//update the velocity of the player rigidbody
	public void OnAnimatorMove(){
		if (!powerActive) {
			mainRigidbody.rotation = animator.rootRotation;
			if (onGround && Time.deltaTime > 0) {
				Vector3 v = (animator.deltaPosition * moveSpeedMultiplier) / Time.deltaTime;
				mainRigidbody.velocity = v;
			}
		}
	}
	//check if the player is in the ground with a raycast
	void checkOnGround(){
		if (!powerActive && !usingJetpack && !flyModeActive) {
			if (transform.InverseTransformDirection (currentVelocity).y < jumpPower * .5f) {
				onGround = false;
				if (jump || slowingFall) {
					mainRigidbody.AddForce (gravityForce* mainRigidbody.mass * normal);
				}
				//check what it is under the player
				Vector3 rayPos = transform.position + transform.up;
				float hitAngle = 0;
				Debug.DrawRay (rayPos, -transform.up * rayDistance, Color.white);
				Vector3 hitPoint = Vector3.zero;
				if (Physics.Raycast (rayPos, -transform.up, out hit, rayDistance, layer)) {
					//get the angle of the current surface
					hitAngle = Vector3.Angle (normal, hit.normal);   
					onGround = true;
					jump = false;
					slowingFall = false;
					hitPoint = hit.point;
				}
				//check if the player has to adhere to the surface or not
				bool adhereToGround = false;
				if (onGround) {
					//if the player is moving
					if (isMoving) {
						//use a raycast to check the distance to the ground in front of the player, 
						//if the ray doesn't find a collider, it means that the player is going down in an inclinated surface, so adhere to that surface
						bool hitInFront=false;
						if (Physics.Raycast (rayPos + transform.forward * 0.5f, -transform.up, out hit, (rayDistance - 0.2f), layer)) {
							Debug.DrawRay (rayPos + transform.forward * 0.5f, -transform.up * (rayDistance - 0.2f), Color.red);
							hitInFront = true;
						} 
						//else the the player is going up in an inclinated surface, so there is no need to adhere to that surface
						if(!hitInFront || (hitInFront && hitAngle==0)) {
							adhereToGround = true;		
						}
					}
					//if the player is not moving and the angle of the surface is 0, adhere to it, so if the player jumps for example, the player is correctly
					//placed in the ground, with out a gap between the player's collider and the ground
					if (!isMoving && hitAngle == 0) {
						adhereToGround = true;
					}
				}
				//the player has to ahdere to the current surface, so
				if (adhereToGround) {
					//print ("adhere");
					//move towards the surface the player's rigidbody 
					mainRigidbody.position = Vector3.MoveTowards (mainRigidbody.position, hitPoint, Time.deltaTime * groundAdherence);
				}
			}
			if (!onGround) {
				lastTimeAir = Time.time;
			}
		} else {
			onGround = false;
		}
	}
	//set the scale of the capsule if the player is crouched
	void scaleCapsule(bool state){
		if (state) {
			capsule.height = originalHeight * 0.6f;
			capsule.center =  Vector3.up * originalHeight * 0.6f * 0.5f;
		}
		else {
			capsule.height = originalHeight;
			capsule.center = Vector3.up * originalHeight * 0.5f;
			//capsule.center=Vector3.up;
		}
	}
	//check with a sphere cast if the there are any surface too close
	public void crouching(){
		if (!powerActive && checkWeaponsState()) {
			crouch = !crouch;
			if(!crouch){
				//check if there is anything above the player when he is crouched, to prevent he stand up
				Ray crouchRay = new Ray (mainRigidbody.position + transform.up * capsule.radius * 0.5f, transform.up);
				float crouchRayLength = originalHeight - capsule.radius * 0.5f;
				if (Physics.SphereCast (crouchRay, capsule.radius * 0.5f, crouchRayLength,layer)) {
					//stop the player to get up
					crouch=true;
				}
			}
			//set the pivot position
			if(crouch){
				playerCameraManager.crouch(1);
				GetComponent<health>().changePlaceToShootPosition (true);
			}
			else{
				playerCameraManager.crouch(-1);
				GetComponent<health>().changePlaceToShootPosition (false);
			}
			scaleCapsule(crouch);
		}
	}
	public void setLastTimeMoved(){
		lastTimeMoved = Time.time;
	}
	public bool checkWeaponsState(){
		if (gravityManager.settings.firstPersonView) {
			return true;
		} else if (!powersManager.aimsettings.aiming && !powersManager.usingWeapons && !GetComponent<IKSystem> ().usingWeapons) {
			return true;
		}
		return false;
	}
	//set if the animator is enabled or not according to if the usingAnimatorInFirstMode is true or false
	public void checkAnimatorIsEnabled(bool state){
		if (!animator) {
			animator = GetComponent<Animator>();
		}
		//if the animator in first person is disabled, then
		if (!usingAnimatorInFirstMode) {
			//the first person mode is enabled, so disable the animator
			if(state){
				animator.enabled=false;
			}
			//the third person mode is enabled, so enable the animator
			else{
				animator.enabled=true;
			}
		}
		//if the animator is enabled, 
		if (animator.enabled) {
			//check the state of the animator to set the values in the mecanim
			usingAnimator = true;
		} 
		else {
			//disable the functions that set the values in the mecanim, and apply force to the player rigidbody directly, instead of using the animation motion
			usingAnimator = false;
		}
		//change the type of footsteps, with the triggers in the feet of the player or the raycast checking the surface under the player
		GetComponent<footStepManager>().changeFootStespType(usingAnimator);
	}
	//if the vehicle driven by the player ejects him, add force to his rigidbody
	public void ejectPlayerFromVehicle(float force){
		//velocity = moveInput * airSpeed;
		//velocity = velocity + transform.up * force;
		jumpInput=true;
		mainRigidbody.AddForce (normal * force,ForceMode.Impulse);
	}
	//use a jump platform
	public void useJumpPlatform(Vector3 direction){
		jumpInput=true;
		mainRigidbody.AddForce (direction,ForceMode.Impulse);
	}
	public void useJumpPlatformWithKeyButton(bool state, float newJumpPower){
		if (state) {
			jumpPower = newJumpPower;
		} else {
			jumpPower = originalJumpPower;
		}
	}
	public void changeHeadScale(bool state){
		if (state) {
			head.localScale = Vector3.zero;
		} else {
			head.localScale = Vector3.one;
		}
	}
	//if the player is driving, set to 0 his movement values and disable the player controller component
	public void drivingState(bool state, GameObject vehicle){
		driving = state;
		if (driving) {
			currentVehicle = vehicle;
		} else {
			currentVehicle = null;
		}
		if (usingAnimator) {
			animator.SetFloat ("Forward", 0);
			animator.SetFloat ("Turn", 0);
		}
		enabled = !state;
	}
	public GameObject getCurrentVehicle(){
		return currentVehicle;
	}
	//if it is neccessary, stop any movement from the keyboard or the touch controls in the player controller
	public void changeScriptState(bool state){
		if (usingAnimator) {
			animator.SetFloat ("Forward", 0);
			animator.SetFloat ("Turn", 0);
		}
		isMoving = false;
		canMove = state;
		h = 0;
		v = 0;
	}
	public void enableOrDisablePlayerControllerScript(bool state){
		enabled = state;
	}
	public void equipJetpack(bool state, float force, float airControl, float airSpeed){
		jetPackEquiped = state;
		jetpackForce = force;
		jetpackAirControl = airControl;
		jetpackAirSpeed = airSpeed;
	}
	public void enableOrDisableFlyingMode(bool state, float force, float airControl, float airSpeed, float turboSpeed){
		flyModeActive = state;
		flyModeForce = force;
		flyModeAirControl = airControl;
		flyModeAirSpeed = airSpeed;
		flyModeTurboSpeed = turboSpeed;
	}
	public void enableOrDisableFlyModeTurbo(bool state){
		flyModeTurboActive = state;
		playerCameraManager.changeCameraFov (flyModeTurboActive);
		//when the player accelerates his movement in the air, the camera shakes
		if (flyModeTurboActive) {
			playerCameraManager.shakeCamera ();
			playerCameraManager.accelerateShake (true);
		} else {
			playerCameraManager.accelerateShake (false);
			playerCameraManager.stopShakeCamera ();
		}
	}
	public void enableOrDisableAimingInFirstPerson(bool state){
		aimingInFirstPerson = state;
	}
}