using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class hoverBoardController : MonoBehaviour {
	public List<hoverEngineSettings> hoverEngineList =new List<hoverEngineSettings> ();
	public OtherCarParts otherCarParts;
	public hoverCraftSettings settings;
	public playerMovementSettings playerSettings;
	Rigidbody mainRigidbody;
	Vector3 normal;
	float audioPower=0;
	float maxEnginePower;
	float boostInput=1;
	float resetTimer;
	float horizontalAxis;
	float verticalAxis;
	float currentSpeed;
	float gravityCenterAngleX;
	float gravityCenterAngleZ;
	float angleZ;
	float currentExtraBodyRotation;
	float currentExtraSpineRotation;
	float originalJumpPower;
	int i;
	int collisionForceLimit = 5;
	bool driving;
	bool jump=false;
	bool moving;
	bool usingBoost;
	bool usingGravityControl;
	bool anyOnGround;
	bool rotating;
	bool usingHoverBoardWaypoint;
	IKDrivingSystem IKManager;
	inputActionManager actionManager;
	vehicleCameraController vCamera;
	vehicleHUDManager hudManager;
	Animator animator;
	Transform playerplayerSpine;
	Transform playerHead;
	Transform rightArm;
	Transform leftArm;
	hoverBoardWayPoints wayPointsManager;

	void Start(){
		mainRigidbody = GetComponent<Rigidbody>();
		IKManager = transform.parent.GetComponent<IKDrivingSystem> ();
		vCamera = settings.vehicleCamera.GetComponent<vehicleCameraController> ();
		hudManager = GetComponent<vehicleHUDManager> ();
		//get the boost particles inside the vehicle
		for (i = 0; i < otherCarParts.boostingParticles.Count; i++) {
			otherCarParts.boostingParticles [i].gameObject.SetActive (false);
		}
		setAudioState(otherCarParts.engineAudio, 5, 0, otherCarParts.engineClip, true, false,false);
		currentExtraBodyRotation = playerSettings.extraBodyRotation;
		currentExtraSpineRotation = playerSettings.extraSpineRotation;
		otherCarParts.gravityCenterCollider.enabled = false;
		originalJumpPower = settings.jumpPower;
	}
	void Update(){
		angleZ = Mathf.Asin(transform.InverseTransformDirection( Vector3.Cross(normal.normalized, transform.up)).z) * Mathf.Rad2Deg;
		float angleX = Mathf.Asin (transform.InverseTransformDirection (Vector3.Cross (normal.normalized, transform.up)).x) * Mathf.Rad2Deg;
//		if (playerSettings.balanceInAirEnabled) {
//			transform.eulerAngles -= transform.InverseTransformDirection (transform.forward) * angleZ * Time.deltaTime * 3;
//			if (!anyOnGround && currentSpeed > 5) {
//				float velocityDirection = Vector3.Dot (mainRigidbody.velocity, normal);
//				if (velocityDirection > -20) {
//					transform.eulerAngles -= transform.InverseTransformDirection (transform.right) * angleX * Time.deltaTime * 3;
//				}
//			}
//		}
		float gravityAngleZ = 0;
		if (Mathf.Abs (angleZ) > 1) {
			gravityAngleZ = -angleZ;
		}
		else{
			gravityAngleZ = 0;
		}
		float gravityAngleX = 0;
		if (Mathf.Abs (angleX) > 1) {
			gravityAngleX = -angleX;
		}
		else{
			gravityAngleX = 0;
		}
		gravityCenterAngleX=Mathf.Lerp (gravityCenterAngleX, gravityAngleX, Time.deltaTime * 5);
		gravityCenterAngleZ=Mathf.Lerp (gravityCenterAngleZ, gravityAngleZ, Time.deltaTime * 5);
		gravityCenterAngleX = Mathf.Clamp (gravityCenterAngleX, -playerSettings.limitBodyRotationX, playerSettings.limitBodyRotationX);
		gravityCenterAngleZ = Mathf.Clamp (gravityCenterAngleZ, -playerSettings.limitBodyRotationZ, playerSettings.limitBodyRotationZ);
		otherCarParts.playerGravityCenter.transform.localEulerAngles = new Vector3 (gravityCenterAngleX, currentExtraBodyRotation, gravityCenterAngleZ);
		float forwardSpeed = (mainRigidbody.transform.InverseTransformDirection(mainRigidbody.velocity).z) * 3f;
		float bodyRotation = playerSettings.extraBodyRotation;
		float spineRotation = playerSettings.extraSpineRotation;
		if (forwardSpeed < -2) {
			bodyRotation = -playerSettings.extraBodyRotation;
			spineRotation = -playerSettings.extraSpineRotation;
		} 
		currentExtraBodyRotation=Mathf.Lerp (currentExtraBodyRotation, bodyRotation, Time.deltaTime * 5);
		currentExtraSpineRotation=Mathf.Lerp (currentExtraSpineRotation, spineRotation, Time.deltaTime * 5);

		mainRigidbody.centerOfMass = settings.centerOfMassOffset;
		if (driving && !usingGravityControl) {
			horizontalAxis = actionManager.input.getMovementAxis ("keys").x;
			verticalAxis = actionManager.input.getMovementAxis ("keys").y;
			moving = verticalAxis != 0;
			if (settings.canJump && actionManager.getActionInput ("Jump") ) {
				if (anyOnGround) {
					if (!jump && anyOnGround) {
						StartCoroutine (jumpCoroutine ());
						mainRigidbody.AddForce (normal * mainRigidbody.mass * settings.jumpPower, ForceMode.Impulse);
					}
				}
				if (usingHoverBoardWaypoint) {
					StartCoroutine (jumpCoroutine ());
					wayPointsManager.pickOrReleaseVehicle (false, false);
					mainRigidbody.AddForce ((normal + transform.forward) * mainRigidbody.mass * settings.jumpPower, ForceMode.Impulse);
				}
			}
			if (!usingHoverBoardWaypoint) {
				//boost input
				if (settings.canUseBoost && actionManager.getActionInput ("Enable Turbo")) {
					usingBoost = true;
					//set the camera move away action
					vCamera.usingBoost (true,"Boost");
				}
				//stop boost
				if (actionManager.getActionInput ("Disable Turbo")) {
					usingBoost = false;
					//disable the camera move away action
					vCamera.usingBoost (false,"Boost");
					//disable the boost particles
					usingBoosting ();
					boostInput = 1;
				}
			}
			//if the boost input is enabled, check if there is energy enough to use it
			if (usingBoost) {
				//if there is enough energy, enable the boost
				if (hudManager.useBoost (moving)) {
					boostInput = settings.maxBoostMultiplier;
					usingBoosting ();
				} 
				//else, disable the boost
				else {
					usingBoost = false;
					//if the vehicle is not using the gravity control system, disable the camera move away action
					if (!GetComponent<vehicleGravityControl> ().powerActive) {
						vCamera.usingBoost (false,"Boost");
					}
					usingBoosting ();
					boostInput = 1;
				}
			}
			//set the current speed in the HUD of the vehicle
			hudManager.getSpeed (currentSpeed, settings.maxSpeed);
		} else {
			horizontalAxis = 0;
			verticalAxis = 0;
		}
		maxEnginePower = 0;
		for (i = 0; i < hoverEngineList.Count; i++) {
			if (hoverEngineList [i].maxEnginePower > maxEnginePower) {
				maxEnginePower = hoverEngineList [i].maxEnginePower;
			}
			//configure every particle system according to the engine state
			float rpm = Mathf.Lerp(hoverEngineList [i].minRPM, hoverEngineList [i].maxRPM, hoverEngineList[i].maxEnginePower);
			if (hoverEngineList [i].turbine) {
				hoverEngineList [i].turbine.transform.Rotate (0, rpm * Time.deltaTime * 6, 0);
			}
			if (hoverEngineList [i].ParticleSystem) {
				hoverEngineList [i].ParticleSystem.emissionRate = hoverEngineList [i].maxEmission * hoverEngineList [i].maxEnginePower;
				hoverEngineList [i].ParticleSystem.transform.position = hoverEngineList [i].hit.point + hoverEngineList [i].hit.normal * hoverEngineList [i].dustHeight;
				hoverEngineList [i].ParticleSystem.transform.LookAt (hoverEngineList [i].hit.point + hoverEngineList [i].hit.normal * 10);
			}
		}
		audioPower = Mathf.Lerp (maxEnginePower, verticalAxis, settings.audioEngineSpeed);
		otherCarParts.engineAudio.volume = Mathf.Lerp (settings.engineMinVolume, settings.engineMaxVolume, audioPower);
		otherCarParts.engineAudio.pitch = Mathf.Lerp (settings.minAudioPitch, settings.maxAudioPitch, audioPower);
		//reset the vehicle rotation if it is upside down 
		if(currentSpeed < 5 ){
			//check the current rotation of the vehicle with respect to the normal of the gravity normal component, which always point the up direction
			float angle= Vector3.Angle(normal,transform.up);
			if (angle > 60 && !rotating) {
				resetTimer += Time.deltaTime;
				if (resetTimer > settings.timeToFlip) {
					resetTimer = 0;
					StartCoroutine (rotateVehicle ());
				}
			}
		}
	}
	void FixedUpdate(){
		currentSpeed = mainRigidbody.velocity.magnitude;
		//apply turn
		if (usingHoverBoardWaypoint) {
			return;
		}
		if (Mathf.Approximately (horizontalAxis, 0)) {
			float localR = Vector3.Dot (mainRigidbody.angularVelocity, transform.up);
			mainRigidbody.AddRelativeTorque (0, -localR * settings.brakingTorque, 0);
		} else {
			float targetRoll = -settings.rollOnTurns * horizontalAxis;
			float roll = Mathf.Asin (transform.right.y) * Mathf.Rad2Deg;
			// only apply additional roll if we're not "overrolled"
			if (Mathf.Abs (roll) > Mathf.Abs (targetRoll)) {
				roll = 0;
			} else {
				roll = Mathf.DeltaAngle (roll, targetRoll);
			}
			mainRigidbody.AddRelativeTorque (0, horizontalAxis * settings.steeringTorque, roll * settings.rollOnTurnsTorque);
		}
		if (!usingGravityControl && !jump) {
			Vector3 localVelocity = transform.InverseTransformDirection (mainRigidbody.velocity);
			Vector3 extraForce = Vector3.Scale(settings.extraRigidbodyForce, localVelocity);
			mainRigidbody.AddRelativeForce (-extraForce * mainRigidbody.mass);
			//use every engine to keep the vehicle in the air
			for (i = 0; i < hoverEngineList.Count; i++) {
				if (!hoverEngineList [i].mainEngine) {
					//find force direction by rotating local up vector towards world up
					Vector3 engineUp = hoverEngineList [i].engineTransform.up;
					Vector3 gravityForce = (normal * 9.8f).normalized;
					engineUp = Vector3.RotateTowards (engineUp, gravityForce, hoverEngineList [i].maxEngineAngle * Mathf.Deg2Rad, 1);
					//check if the vehicle is on ground
					hoverEngineList [i].maxEnginePower = 0;
					if (Physics.Raycast (hoverEngineList [i].engineTransform.position, -engineUp, out hoverEngineList [i].hit, hoverEngineList [i].maxHeight, settings.layer)) {
						//calculate down force
						hoverEngineList [i].maxEnginePower = Mathf.Pow ((hoverEngineList [i].maxHeight - hoverEngineList [i].hit.distance) / hoverEngineList [i].maxHeight, hoverEngineList [i].Exponent);
						float force = hoverEngineList [i].maxEnginePower * hoverEngineList [i].engineForce;
						float velocityUp = Vector3.Dot (mainRigidbody.GetPointVelocity (hoverEngineList [i].engineTransform.position), engineUp);
						float drag = -velocityUp * Mathf.Abs (velocityUp) * hoverEngineList [i].damping;
						mainRigidbody.AddForceAtPosition (engineUp * (force + drag), hoverEngineList [i].engineTransform.position);
					}
				} else {
					if (playerSettings.balanceInAirEnabled) {
						//find current local pitch and roll
						Vector3 gravityForce = (normal * 9.8f).normalized;
						float pitch = Mathf.Asin (Vector3.Dot (transform.forward, gravityForce)) * Mathf.Rad2Deg;
						float roll = Mathf.Asin (Vector3.Dot (transform.right, gravityForce)) * Mathf.Rad2Deg;
						pitch = Mathf.DeltaAngle (pitch, 0); 
						roll = Mathf.DeltaAngle (roll, 0);
						//apply compensation torque
						float auxPitch = -pitch * settings.pitchCompensationTorque;
						float auxRoll = roll * settings.rollCompensationTorque;
						mainRigidbody.AddRelativeTorque (auxPitch, 0, auxRoll);
					}
				}
			}
			if (actionManager.getActionInput ("Brake")) {
				for (i = 0; i < hoverEngineList.Count; i++) {
					if (hoverEngineList [i].mainEngine) {
						mainRigidbody.velocity = Vector3.Lerp (mainRigidbody.velocity, Vector3.zero, Time.deltaTime);
					}
				}
			} else {
				for (i = 0; i < hoverEngineList.Count; i++) {
					if (hoverEngineList [i].mainEngine) {
						float movementMultiplier = settings.inAirMovementMultiplier;
						if (Physics.Raycast (hoverEngineList [i].engineTransform.position, -transform.up, out hoverEngineList [i].hit, hoverEngineList [i].maxHeight, settings.layer)) {
							movementMultiplier = 1;
						} 
						Vector3 gravityForce = (normal * 9.8f).normalized;
						//current speed along forward axis
						float speed = Vector3.Dot (mainRigidbody.velocity, transform.forward);
						//if the vehicle doesn't move by input, apply automatic brake 
						bool isAutoBraking = Mathf.Approximately (verticalAxis, 0) && settings.autoBrakingDeceleration > 0;
						float thrust = verticalAxis;
						if (isAutoBraking) {
							thrust = -Mathf.Sign (speed) * settings.autoBrakingDeceleration / settings.maxBrakingDeceleration;
						}
						//check if it is braking, for example speed and thrust have opposing signs
						bool isBraking = verticalAxis * speed < 0;
						//don't apply force if speed is max already
						if (Mathf.Abs (speed) < settings.maxSpeed || isBraking) {
							//position on speed curve
							float normSpeed = Mathf.Sign (verticalAxis) * speed / settings.maxSpeed;
							//apply acceleration curve and select proper maximum value
							float acc = settings.accelerationCurve.Evaluate (normSpeed) * (isBraking ? settings.maxBrakingDeceleration : thrust > 0 ? settings.maxForwardAcceleration : settings.maxReverseAcceleration);
							//drag should be added to the acceleration
							float sdd = speed * settings.extraRigidbodyForce.z;
							float dragForce = sdd + mainRigidbody.drag * speed;
							float force = acc * thrust + dragForce;
							//reduce acceleration if the vehicle is close to vertical orientation and is trrying to go higher
							float y = Vector3.Dot ( transform.forward, gravityForce);
							if (settings.maxSurfaceAngle < 90 && y * thrust > 0) {
								if (!isAutoBraking) {
									float pitch2 = Mathf.Asin (Mathf.Abs (y)) * Mathf.Rad2Deg;
									if (pitch2 > settings.maxSurfaceAngle) {
										float forceDecrease = (pitch2 - settings.maxSurfaceAngle) / (90 - settings.maxSurfaceAngle) * settings.maxSurfaceVerticalReduction;
										force /= 1 + forceDecrease;
									}
								}
							}
							mainRigidbody.AddForce ( transform.forward * force * boostInput * movementMultiplier, ForceMode.Acceleration);
						}
					}
				}
			}
		}
		anyOnGround = true;
		int totalWheelsOnAir = 0;
		for (i = 0; i < hoverEngineList.Count; i++) {
			if (!Physics.Raycast (hoverEngineList [i].engineTransform.position, -hoverEngineList [i].engineTransform.up, out hoverEngineList [i].hit, hoverEngineList [i].maxHeight, settings.layer)) {
				totalWheelsOnAir++;
			}
		}
		//if the total amount of wheels in the air is equal to the number of wheel sin the vehicle, anyOnGround is false
		if (totalWheelsOnAir == hoverEngineList.Count && anyOnGround) {
			anyOnGround = false;
		}
	}
	void LateUpdate(){
		if (driving) {
			if (playerplayerSpine) {
				Quaternion rotationX = Quaternion.FromToRotation (playerplayerSpine.transform.InverseTransformDirection (transform.right), playerplayerSpine.transform.InverseTransformDirection (transform.forward));
				Vector3 directionX = rotationX.eulerAngles;
				Quaternion rotationZ = Quaternion.FromToRotation (playerplayerSpine.transform.InverseTransformDirection (transform.forward), playerplayerSpine.transform.InverseTransformDirection (transform.forward));
				Vector3 directionZ = rotationZ.eulerAngles;
				float angleX = directionX.x;
				if (angleX > 180) {
					angleX = Mathf.Clamp (angleX, playerSettings.maxSpineRotationX, 360);
				} else {
					angleX = Mathf.Clamp (angleX, 0, playerSettings.minSpineRotationX);
				}
				playerplayerSpine.transform.localEulerAngles = new Vector3 (angleX - angleZ, playerplayerSpine.transform.localEulerAngles.y, directionZ.z - currentExtraSpineRotation );

				float armRotation = angleZ;
				armRotation = Mathf.Clamp (armRotation, -playerSettings.maxArmsRotation, playerSettings.maxArmsRotation);
				float rightArmRotationX = rightArm.transform.localEulerAngles.x - armRotation ;
				rightArm.transform.localEulerAngles =new Vector3 (rightArmRotationX, rightArm.transform.localEulerAngles.y, rightArm.transform.localEulerAngles.z );
				float leftArmRotationX = leftArm.transform.localEulerAngles.x + armRotation;
				leftArm.transform.localEulerAngles =new Vector3 (leftArmRotationX, leftArm.transform.localEulerAngles.y, leftArm.transform.localEulerAngles.z );

				Quaternion headRotationX = Quaternion.FromToRotation (playerHead.transform.InverseTransformDirection (transform.up), transform.InverseTransformDirection (transform.forward));
				Vector3 headDirectionX = headRotationX.eulerAngles;
				Quaternion headRotationY = Quaternion.FromToRotation (playerHead.transform.InverseTransformDirection (transform.forward), transform.InverseTransformDirection (transform.forward));
				Vector3 headDirectionY = headRotationY.eulerAngles;
				playerHead.transform.localEulerAngles = new Vector3 (headDirectionX.x - angleZ, playerHead.transform.localEulerAngles.y,headDirectionY.z - playerSettings.extraHeadRotation);
			}
		}
	}
	IEnumerator jumpCoroutine(){
		jump = true;
		yield return new WaitForSeconds (0.5f);
		jump=false;
	}
	public void enterOrExitFromWayPoint(bool state){
		usingHoverBoardWaypoint = state;
		GetComponent<vehicleGravityControl> ().enabled = !state;
		mainRigidbody.isKinematic = state;
	}
	public void receiveWayPoints(hoverBoardWayPoints wayPoints){
		wayPointsManager = wayPoints;
	}
	//if the vehicle is using the gravity control, set the state in this component
	public void changeGravityControlUse(bool state){
		usingGravityControl = state;
	}
	//the player is getting on or off from the vehicle, so
	public void changeVehicleState(Vector3 nextPlayerPos){
		driving = !driving;
		//set the audio values if the player is getting on or off from the vehicle
		if (driving) {
			setAudioState (otherCarParts.engineAudio, 5, 0, otherCarParts.engineClip, true, true,false);
		} else {
			setAudioState(otherCarParts.engineAudio, 5, 0, otherCarParts.engineClip, false, false,true);
			boostInput = 1;
			//stop the boost
			if (usingBoost) {
				usingBoost = false;
				vCamera.usingBoost (false,"Boost");
				usingBoosting ();
				boostInput = 1;
			}
		}
		otherCarParts.gravityCenterCollider.enabled = driving;
		//set the same state in the IK driving and in the gravity control components
		IKManager.startOrStopVehicle (driving,otherCarParts.chassis,normal,nextPlayerPos);
		if (!animator) {
			animator = GetComponentInChildren<Animator> ();
		}
		if (animator) {
			playerplayerSpine = animator.GetBoneTransform (HumanBodyBones.Chest).transform.parent;
			playerHead = animator.GetBoneTransform (HumanBodyBones.Head);
			rightArm = animator.GetBoneTransform (HumanBodyBones.RightUpperArm);
			leftArm = animator.GetBoneTransform (HumanBodyBones.LeftUpperArm);
		}
		GetComponent<vehicleGravityControl>().changeGravityControlState(driving);
	}
	//the vehicle has been destroyed, so disabled every component in it
	public void disableVehicle(){
		//stop the audio sources
		setAudioState (otherCarParts.engineAudio, 5, 0, otherCarParts.engineClip, false, false,false);
		//stop the boost
		if (usingBoost) {
			usingBoost = false;
			vCamera.usingBoost (false,"Boost");
			usingBoosting ();
			boostInput = 1;
		}
		otherCarParts.gravityCenterCollider.enabled = false;
		//disable the controller
		GetComponent<hoverBoardController> ().enabled = false;
	}
	//get the current normal in the gravity control component
	public void setNormal(Vector3 normalValue){
		normal = normalValue;
	}
	//reset the vehicle rotation if it is upside down
	IEnumerator rotateVehicle(){
		rotating = true;
		Quaternion currentRotation = transform.rotation;
		//rotate in the forward direction of the vehicle
		Quaternion dstRotPlayer = Quaternion.LookRotation (transform.forward, normal);
		for (float t = 0; t < 1;) {
			t += Time.deltaTime * 3;
			transform.rotation = Quaternion.Slerp (currentRotation,dstRotPlayer, t);
			mainRigidbody.velocity = Vector3.zero;
			yield return null;
		}
		rotating = false;
	}
	//play or stop every audio component in the vehicle, like engine, skid, etc.., configuring also volume and loop according to the movement of the vehicle
	public void setAudioState(AudioSource source, float distance, float volume, AudioClip clip, bool loop, bool play, bool stop){
		source.minDistance = distance;
		source.volume = volume;
		source.clip = clip;
		source.loop = loop;
		source.spatialBlend = 1;
		if (play) {
			source.GetComponent<AudioSource> ().Play ();
		}
		if (stop){
			source.GetComponent<AudioSource> ().Stop ();
		}
	}
	//if any collider in the vehicle collides, then
	void OnCollisionEnter (Collision collision){
		//check that the collision is not with the player
		if (collision.contacts.Length > 0 && collision.gameObject.tag!="Player"){
			//if the velocity of the collision is higher that the limit
			if(collision.relativeVelocity.magnitude > collisionForceLimit){
				//set the collision audio with a random collision clip
				if (otherCarParts.crashClips.Length > 0) {
					setAudioState (otherCarParts.crashAudio, 5, 1, otherCarParts.crashClips [UnityEngine.Random.Range (0, otherCarParts.crashClips.Length)], false, true, false);
				}
				//if the vehicle hits another vehicle, apply damage to both of them according to the velocity at the impact
				applyDamage.checkHealth (gameObject, collision.collider.gameObject, 
					collision.relativeVelocity.magnitude * GetComponent<vehicleHUDManager> ().damageMultiplierOnCollision, 
					collision.contacts [0].normal, collision.contacts [0].point, gameObject, false);
			}
		}
	}
	//get the input manager component
	public void getInputActionManager(inputActionManager manager){
		actionManager = manager;
	}
	//if the vehicle is using the boost, set the boost particles
	public void usingBoosting(){
		for (int i = 0; i < otherCarParts.boostingParticles.Count; i++) {
			if (usingBoost) {
				if (!otherCarParts.boostingParticles [i].isPlaying) {
					otherCarParts.boostingParticles [i].gameObject.SetActive (true);
					otherCarParts.boostingParticles [i].Play ();
					otherCarParts.boostingParticles [i].loop = true;
				}
			} else {
				otherCarParts.boostingParticles [i].loop = false;
			}
		}
	}
	//use a jump platform
	public void useVehicleJumpPlatform(Vector3 direction){
		StartCoroutine(jumpCoroutine());
		mainRigidbody.AddForce (mainRigidbody.mass * direction, ForceMode.Impulse);
	}
	public void setNewJumpPower (float newJumpPower) {
		settings.jumpPower = newJumpPower;
	} 
	public void setOriginalJumpPower(){
		settings.jumpPower = originalJumpPower;
	}
	[System.Serializable]
	public class hoverEngineSettings{
		public string Name;	
		public Transform engineTransform;
		public ParticleSystem ParticleSystem;
		public float maxEmission=100;
		public float dustHeight = 0.1f;
		public float maxHeight=2;
		public float engineForce=300;
		public float damping=10;
		public float Exponent=2;
		public float maxEngineAngle=15;
		public bool mainEngine;
		public float minRPM=100;
		public float maxRPM=200;
		public Transform turbine;
		[HideInInspector] public RaycastHit hit;
		[HideInInspector] public float maxEnginePower;
	}
	[System.Serializable]
	public class OtherCarParts{
		public Transform COM;
		public Transform playerGravityCenter;
		public GameObject chassis;
		public AudioClip engineClip;
		public AudioClip[] crashClips;
		public AudioSource engineAudio;
		public AudioSource crashAudio;
		public List<ParticleSystem> boostingParticles=new List<ParticleSystem>();
		public Collider gravityCenterCollider;
	}
	[System.Serializable]
	public class hoverCraftSettings{	
		public LayerMask layer;
		public float steeringTorque = 120;
		public float brakingTorque=200;
		public float maxSpeed = 30;
		public float maxForwardAcceleration = 20;
		public float maxReverseAcceleration = 15;
		public float maxBrakingDeceleration = 30;
		public float autoBrakingDeceleration = 20;
		public float rollOnTurns=10;
		public float rollOnTurnsTorque=10;
		public float rollCompensationTorque=1;
		public float pitchCompensationTorque = 1;
		public float timeToFlip = 2;
		public float audioEngineSpeed=0.5f;
		public float engineMinVolume = 0.5f;
		public float engineMaxVolume = 1;
		public float minAudioPitch = 0.4f;
		public float maxAudioPitch = 1;
		public AnimationCurve accelerationCurve;
		public float maxSurfaceVerticalReduction = 10;
		public float maxSurfaceAngle = 110;
		public Vector3 extraRigidbodyForce= new Vector3(2,0.1f,0.2f);
		public Vector3 centerOfMassOffset;
		public float maxBoostMultiplier;
		[Range(0,1)] public float inAirMovementMultiplier;
		public GameObject vehicleCamera;
		public float jumpPower;
		public bool canJump;
		public bool canUseBoost;
	}
	[System.Serializable]
	public class playerMovementSettings{	
		public float extraBodyRotation=-20;
		public float extraSpineRotation=30;
		public float extraHeadRotation;
		public float limitBodyRotationX=30;
		public float limitBodyRotationZ=30;
		public float minSpineRotationX=20;
		public float maxSpineRotationX=330;
		public float maxArmsRotation=25;
		public bool balanceInAirEnabled;
	}
}