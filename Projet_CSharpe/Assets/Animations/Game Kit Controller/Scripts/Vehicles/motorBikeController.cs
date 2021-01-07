using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class motorBikeController : MonoBehaviour {
	public List<Wheels> wheelsList =new List<Wheels> ();
	public List<Gears> gearsList = new List<Gears> ();
	public OtherCarParts otherCarParts;
	public motorBikeSettings settings;
	public int currentGear;
	public float currentSpeed;
	public float currentRPM = 0;
	public bool anyOnGround;
	List<ParticleSystem> boostingParticles=new List<ParticleSystem>();
	bool driving;
	bool reversing; 
	bool changingGear;
	bool jump;
	bool moving;
	bool usingBoost;
	bool vehicleDestroyed;
	bool usingGravityControl;
	int i;
	int collisionForceLimit = 10;
	float horizontalLean = 0;
	float verticalLean = 0;
	float steerInput = 0;
	float motorInput = 0;
	float defSteerAngle = 0;
	float boostInput=1;
	float horizontalAxis=0;
	float verticalAxis=0;
	float originalJumpPower;
	Wheels frontWheel;
	Wheels rearWheel;
	RaycastHit hit;
	Vector3 normal;
	Rigidbody mainRigidbody;
	IKDrivingSystem IKManager;
	inputActionManager actionManager;
	vehicleCameraController vCamera;
	vehicleHUDManager hudManager;
	skidsManager skidMarksManager;

	void Start (){
		//set every wheel slip smoke particles and get the front and the rear wheel
		for (i = 0; i < wheelsList.Count; i++) {
			if (otherCarParts.wheelSlipPrefab) {
				GameObject newSmoke = (GameObject)Instantiate (otherCarParts.wheelSlipPrefab, transform.position, transform.rotation);
				newSmoke.transform.position = wheelsList [i].wheelCollider.transform.position;
				newSmoke.transform.parent = wheelsList [i].wheelCollider.transform;
				wheelsList [i].wheelParticles = newSmoke;
			}
			if (wheelsList [i].wheelSide == wheelType.front) {
				frontWheel = wheelsList [i];
			}
			if (wheelsList [i].wheelSide == wheelType.rear) {
				rearWheel = wheelsList [i];
			}
		}
		//set the sound components
		setAudioState(otherCarParts.engineAudio, 5, 0, otherCarParts.engineClip, true, false,false);
		setAudioState(otherCarParts.skidAudio, 5, 0, otherCarParts.skidClip, true, false,false);
		setAudioState(otherCarParts.engineStartAudio, 5, 0.7f, otherCarParts.engineStartClip, false, false,false);
		//get the vehicle rigidbody
		mainRigidbody = GetComponent<Rigidbody>();
		mainRigidbody.constraints = RigidbodyConstraints.FreezeRotationZ;
		mainRigidbody.centerOfMass = new Vector3(otherCarParts.COM.localPosition.x * transform.localScale.x , otherCarParts.COM.localPosition.y * transform.localScale.y , otherCarParts.COM.localPosition.z * transform.localScale.z);
		mainRigidbody.maxAngularVelocity = 2;
		//store the max steer angle
		defSteerAngle = settings.steerAngleLimit;
		//get the ik driving system from the parent
		IKManager = transform.parent.GetComponent<IKDrivingSystem> ();
		//get the boost particles inside the vehicle
		if (otherCarParts.boostParticles) {
			Component[] boostParticlesComponents = otherCarParts.boostParticles.GetComponentsInChildren (typeof(ParticleSystem));
			foreach (Component c in boostParticlesComponents) {
				boostingParticles.Add (c.GetComponent<ParticleSystem> ());
				c.gameObject.SetActive (false);
			}
		}
		vCamera = settings.vehicleCamera.GetComponent<vehicleCameraController> ();
		hudManager = GetComponent<vehicleHUDManager> ();
		originalJumpPower = settings.jumpPower;
		skidMarksManager = GetComponentInParent<skidsManager> ();
	}
	void Update(){
		//if the player is driving this vehicle, then
		if (driving && !usingGravityControl) {
			//jump input
			if (settings.canJump && actionManager.getActionInput ("Jump") && anyOnGround) {
				jump = true;
			}
			//boost input
			if (settings.canUseBoost && actionManager.getActionInput ("Enable Turbo")) {
				usingBoost = true;
				//set the camera move away action
				vCamera.usingBoost (true, "Boost");
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
			hudManager.getSpeed (currentSpeed, settings.maxForwardSpeed);
		}
		//change gear
		if (!changingGear && !usingGravityControl) {
			if (currentGear + 1 < gearsList.Count) {
				if (currentSpeed >= gearsList [currentGear].gearSpeed && rearWheel.wheelCollider.rpm >= 0) {
					StartCoroutine (changeGear (currentGear + 1));
				}
			}
			if (currentGear - 1 >= 0) {
				if (currentSpeed < gearsList [currentGear - 1].gearSpeed) {
					StartCoroutine (changeGear (currentGear - 1));
				}
			}
			//set the current gear to 0 if the velocity is too low
			if (currentSpeed < 5 && currentGear > 1) {
				StartCoroutine (changeGear (0));
			}
		}
		//check every wheel collider of the vehicle, to move it and apply rotation to it correctly using raycast
		WheelHit wheelGroundHit;
		for (i = 0; i < wheelsList.Count; i++) {
			//get the center position of the wheel
			Vector3 ColliderCenterPoint = wheelsList[i].wheelCollider.transform.TransformPoint (wheelsList[i].wheelCollider.center);
			//use a raycast in the ground direction
			wheelsList[i].wheelCollider.GetGroundHit (out wheelGroundHit);
			//if the wheel is close enough to the ground, then
			if (Physics.Raycast (ColliderCenterPoint, -wheelsList[i].wheelCollider.transform.up, out hit,(wheelsList[i].wheelCollider.suspensionDistance + wheelsList[i].wheelCollider.radius) * transform.localScale.y,settings.layer)) {
				//set the wheel mesh position according to the values of the wheel collider
				wheelsList [i].wheelMesh.transform.position = hit.point + (wheelsList [i].wheelCollider.transform.up * wheelsList [i].wheelCollider.radius) * transform.localScale.y;
			} 
			//the wheel is in the air
			else {
				//set the wheel mesh position according to the values of the wheel collider
				wheelsList[i].wheelMesh.transform.position = ColliderCenterPoint - (wheelsList[i].wheelCollider.transform.up * wheelsList[i].wheelCollider.suspensionDistance) * transform.localScale.y;
			}
			//if the current wheel is the front one,rotate the steering handlebar according to the wheel collider steerAngle
			if (wheelsList [i].wheelSide == wheelType.front) {
				otherCarParts.steeringHandlebar.transform.rotation = wheelsList[i].wheelCollider.transform.rotation * Quaternion.Euler (0, wheelsList[i].wheelCollider.steerAngle, wheelsList[i].wheelCollider.transform.rotation.z);
			}
			//if the wheel has a mudguard
			if (wheelsList [i].mudGuard) {
				//rotate the mudguard according to that rotation
				wheelsList [i].mudGuard.transform.position =wheelsList[i].wheelMesh.transform.position;
			}
			//if the wheel has suspension, set its rotation according to the wheel position
			if (wheelsList [i].suspension) {
				Quaternion newRotation = Quaternion.LookRotation (wheelsList[i].suspension.transform.position-wheelsList[i].wheelMesh.transform.position,wheelsList[i].suspension.transform.up);
				wheelsList[i].suspension.transform.rotation=newRotation;
			}
			//set the rotation value in the wheel collider
			wheelsList[i].rotationValue += wheelsList[i].wheelCollider.rpm * (6) * Time.deltaTime;
			//rotate the wheel mesh only according to the current speed 
			wheelsList[i].wheelMesh.transform.rotation = wheelsList[i].wheelCollider.transform.rotation * Quaternion.Euler (wheelsList[i].rotationValue, wheelsList[i].wheelCollider.steerAngle, wheelsList[i].wheelCollider.transform.rotation.z);
		}
		//rotate the vehicle chassis when the gear is being changed
		//get the vertical lean value
		verticalLean = Mathf.Clamp(Mathf.Lerp (verticalLean, transform.InverseTransformDirection(mainRigidbody.angularVelocity).x * settings.chassisLean.y, Time.deltaTime * 5), -settings.chassisLeanLimit.y, settings.chassisLeanLimit.y);
		frontWheel.wheelCollider.GetGroundHit(out wheelGroundHit);
		float normalizedLeanAngle = Mathf.Clamp(wheelGroundHit.sidewaysSlip, -1, 1);	
		if (transform.InverseTransformDirection (mainRigidbody.velocity).z > 0) {
			normalizedLeanAngle = -1;
		} else {
			normalizedLeanAngle = 1;
		}
		//get the horizontal lean value
		horizontalLean = Mathf.Clamp(Mathf.Lerp (horizontalLean, (transform.InverseTransformDirection(mainRigidbody.angularVelocity).y * normalizedLeanAngle) * settings.chassisLean.x, Time.deltaTime * 3), -settings.chassisLeanLimit.x, settings.chassisLeanLimit.x);
		Quaternion target = Quaternion.Euler(verticalLean, otherCarParts.chassis.transform.localRotation.y + (mainRigidbody.angularVelocity.z), horizontalLean);
		//set the lean rotation value in the chassis transform
		otherCarParts.chassis.transform.localRotation = target;
		//set the vehicle mass center
		mainRigidbody.centerOfMass = new Vector3((otherCarParts.COM.localPosition.x) * transform.lossyScale.x , (otherCarParts.COM.localPosition.y) * transform.lossyScale.y , (otherCarParts.COM.localPosition.z) * transform.localScale.z);
	}
	void FixedUpdate (){
		//get the current speed value
		currentSpeed = mainRigidbody.velocity.magnitude * 3.6f;
		//stabilize the vehicle it is forward direction
		float angleZ = Mathf.Asin(transform.InverseTransformDirection( Vector3.Cross(normal.normalized, transform.up)).z) * Mathf.Rad2Deg;
		transform.eulerAngles -= transform.InverseTransformDirection (transform.forward) * angleZ * Time.deltaTime;
		//allows vehicle to remain roughly pointing in the direction of travel
		if (!anyOnGround && settings.preserveDirectionWhileInAir && currentSpeed>5 ) {
			float velocityDirection = Vector3.Dot (mainRigidbody.velocity, normal);
			if(velocityDirection>-20){
				float angleX = Mathf.Asin(transform.InverseTransformDirection( Vector3.Cross(normal.normalized, transform.up)).x) * Mathf.Rad2Deg;
				transform.eulerAngles-=transform.InverseTransformDirection( transform.right)*angleX*Time.deltaTime;
			}
		}
		//if the player is driving this vehicle and the gravity control is not being used, then
		if (driving && !usingGravityControl) {
			//get the current values from the input manager, keyboard and touch controls
			horizontalAxis = actionManager.input.getMovementAxis ("keys").x;
			verticalAxis = actionManager.input.getMovementAxis ("keys").y;
		} 
		//else, set the input values to 0
		else {
			horizontalAxis = 0;
			verticalAxis = 0;
		}
		//set the current axis input in the motor input
		if (!changingGear) {
			motorInput = verticalAxis;
		} else {
			motorInput = Mathf.Clamp (verticalAxis, -1, 0);
		}
		steerInput = Mathf.Lerp (steerInput, horizontalAxis, Time.deltaTime * 10);
		moving = verticalAxis != 0;
		//set the steer limit
		settings.steerAngleLimit = Mathf.Lerp(defSteerAngle, settings.highSpeedSteerAngle, (currentSpeed / settings.highSpeedSteerAngleAtSpeed));
		//set the steer angle in the fron wheel
		frontWheel.wheelCollider.steerAngle = settings.steerAngleLimit * steerInput;
		//set the current RPM
		currentRPM = Mathf.Clamp((((Mathf.Abs((frontWheel.wheelCollider.rpm + rearWheel.wheelCollider.rpm)) * settings.gearShiftRate) + settings.minRPM)) / (currentGear + 1), settings.minRPM, settings.maxRPM);
		//check if the vehicle is moving forwards or backwards
		if (motorInput < 0) { 
			reversing = true;
		} else {
			reversing = false;
		}
		//set the engine audio volume and pitch according to input and current RPM
		if (!vehicleDestroyed) {
			otherCarParts.engineAudio.volume = Mathf.Lerp (otherCarParts.engineAudio.volume, Mathf.Clamp (motorInput, 0.35f, 0.85f), Time.deltaTime * 5);
			otherCarParts.engineAudio.pitch = Mathf.Lerp (otherCarParts.engineAudio.pitch, Mathf.Lerp (1, 2, (currentRPM - (settings.minRPM / 1.5f)) / (settings.maxRPM + settings.minRPM)), Time.deltaTime * 5);
		}
		if (otherCarParts.engineStartAudio) {
			otherCarParts.engineStartAudio.GetComponent<AudioSource> ().volume -= Time.deltaTime / 5;
		}
		//if the current speed is higher that the max speed, stop apply motor torque to the powered wheel
		if(currentSpeed > settings.maxForwardSpeed || usingGravityControl){
			rearWheel.wheelCollider.motorTorque = 0;
		}
		//else if the vehicle is moving in fowards direction, apply motor torque to the powered wheel using the gear animation curve
		else if(!reversing && !changingGear){
			float speedMultiplier=1;
			if (settings.useCurves) {
				speedMultiplier = gearsList [currentGear].engineTorqueCurve.Evaluate (currentSpeed);
			}
			rearWheel.wheelCollider.motorTorque = settings.engineTorque  * Mathf.Clamp(motorInput, 0, 1) * boostInput * speedMultiplier;
		}
		//if the vehicle is moving backwards, apply motor torque to every powered wheel
		if(reversing){
			//if the current speed is lower than the maxBackWardSpeed, apply motor torque
			if (currentSpeed < settings.maxBackwardSpeed && Mathf.Abs (rearWheel.wheelCollider.rpm / 2) < 3000) {
				rearWheel.wheelCollider.motorTorque = settings.engineTorque * motorInput;
			} 
			//else, stop adding motor torque
			else {
				rearWheel.wheelCollider.motorTorque = 0;
			}
		}
		//if the handbrake is pressed, set the brake torque value in every wheel
		if (actionManager.getActionInput ("Brake")) {
			for (i = 0; i < wheelsList.Count; i++) {
				if (wheelsList [i].wheelSide == wheelType.front) {
					wheelsList [i].wheelCollider.brakeTorque = settings.brake * 5;
				} if (wheelsList [i].wheelSide == wheelType.rear) {
					wheelsList [i].wheelCollider.brakeTorque = settings.brake * 25;
				}
			}
		} 
		//else, check if the vehicle input is in forward or in backward direction
		else {
			for (i = 0; i < wheelsList.Count; i++) {
				//the vehicle is decelerating
				if (Mathf.Abs (motorInput) <= 0.05f && !changingGear) {
					wheelsList [i].wheelCollider.brakeTorque = settings.brake / 25;
				} 
				//the vehicle is braking
				else if (motorInput < 0 && !reversing) {
					if (wheelsList [i].wheelSide == wheelType.front) {
						wheelsList [i].wheelCollider.brakeTorque = settings.brake * (Mathf.Abs (motorInput) / 5);
					} if (wheelsList [i].wheelSide == wheelType.rear) {
						wheelsList [i].wheelCollider.brakeTorque = settings.brake * (Mathf.Abs (motorInput));
					}
				} else {
					wheelsList [i].wheelCollider.brakeTorque = 0;
				}
			}
		}
		//check the right front and right rear wheel to play the skid audio according to their state
		WheelHit wheelGroundHitFront;
		WheelHit wheelGroundHitRear;
		frontWheel.wheelCollider.GetGroundHit (out wheelGroundHitFront);
		rearWheel.wheelCollider.GetGroundHit (out wheelGroundHitRear);
		//if the values in the wheel hit are higher that
		if (Mathf.Abs (wheelGroundHitFront.sidewaysSlip) > 0.25f || Mathf.Abs (wheelGroundHitRear.forwardSlip) > 0.5f || Mathf.Abs (wheelGroundHitFront.forwardSlip) > 0.5f) {
			//and the vehicle is moving, then 
			if (mainRigidbody.velocity.magnitude > 1) {
				//set the skid volume value according to the vehicle skid
				otherCarParts.skidAudio.volume = Mathf.Abs (wheelGroundHitFront.sidewaysSlip) + ((Mathf.Abs (wheelGroundHitFront.forwardSlip) + Mathf.Abs (wheelGroundHitRear.forwardSlip)) / 4);
			} else {
				//set the skid volume value to 0
				otherCarParts.skidAudio.volume -= Time.deltaTime;
			}
		} else {
			//set the skid volume value to 0
			otherCarParts.skidAudio.volume -= Time.deltaTime;
		}
		//set the smoke skid particles in every wheel
		WheelHit wheelGroundHit;
		for (i = 0; i < wheelsList.Count; i++) {
			wheelsList[i].wheelCollider.GetGroundHit( out wheelGroundHit );

			//set the skid marks under every wheel
			wheelsList[i].wheelSlipAmountSideways = Mathf.Abs(wheelGroundHit.sidewaysSlip);
			wheelsList[i].wheelSlipAmountForward = Mathf.Abs(wheelGroundHit.forwardSlip);
			if ( wheelsList[i].wheelSlipAmountSideways > 0.25f || wheelsList[i].wheelSlipAmountForward > 0.5f){
				Vector3 skidPoint = wheelGroundHit.point + 2 * (mainRigidbody.velocity) * Time.deltaTime;
				if (mainRigidbody.velocity.magnitude > 1) {
					wheelsList[i].lastSkidmark = skidMarksManager.AddSkidMark (skidPoint, wheelGroundHit.normal, 
						(wheelsList[i].wheelSlipAmountSideways / 2) + (wheelsList[i].wheelSlipAmountForward / 2.5f), wheelsList[i].lastSkidmark);
				} else {
					wheelsList[i].lastSkidmark = -1;
				}
			}
			else{
				wheelsList[i].lastSkidmark = -1;
			}


			if(Mathf.Abs(wheelGroundHit.sidewaysSlip) > 0.25f || Mathf.Abs(wheelGroundHit.forwardSlip) > 0.5f){
				wheelsList[i].wheelParticles.GetComponent<ParticleEmitter>().emit = true;
			}else{ 
				wheelsList[i].wheelParticles.GetComponent<ParticleEmitter>().emit = false;
			}
		}
		//set the exhaust particles state
		for (i = 0; i < otherCarParts.normalExhaust.Count; i++) {
			if (driving && currentSpeed < 20) {
				otherCarParts.normalExhaust [i].emit = true;
			} else {
				otherCarParts.normalExhaust [i].emit = false;
			}
		}
		for (i = 0; i < otherCarParts.heavyExhaust.Count; i++) {
			if (driving && currentSpeed < 20 && motorInput > 0.5f) {
				otherCarParts.heavyExhaust [i].emit = true;
			} else {
				otherCarParts.heavyExhaust [i].emit = false;
			}
		}
		//check if the car is in the ground or not
		anyOnGround = true;
		int totalWheelsOnAir = 0;
		for (i = 0; i < wheelsList.Count; i++) {
			if (!wheelsList [i].wheelCollider.isGrounded){
				//if the current wheel is in the air, increase the number of wheels in the air
				totalWheelsOnAir++;
			}
		}
		//if the total amount of wheels in the air is equal to the number of wheel sin the vehicle, anyOnGround is false
		if (totalWheelsOnAir == wheelsList.Count && anyOnGround) {
			anyOnGround = false;
		}
		//if any wheel is in the ground rear, then 
		if (anyOnGround) {
			//check if the jump input has been presses
			if (jump) {
				//apply force in the up direction
				mainRigidbody.AddForce(transform.up * mainRigidbody.mass*settings.jumpPower);
				jump = false;
			}
		}
	}
	//if the vehicle is using the gravity control, set the state in this component
	public void changeGravityControlUse(bool state){
		usingGravityControl = state;
		if (usingGravityControl) {
			StartCoroutine (changeGear (0));
		}
	}
	//the player is getting on or off from the vehicle, so
	public void changeVehicleState(Vector3 nextPlayerPos){
		driving = !driving;
		//set the audio values if the player is getting on or off from the vehicle
		if (driving) {
			setAudioState (otherCarParts.engineAudio, 5, 0, otherCarParts.engineClip, true, true,false);
			setAudioState (otherCarParts.skidAudio, 5, 0, otherCarParts.skidClip, true, true,false);
			setAudioState (otherCarParts.engineStartAudio, 5, 0.7f, otherCarParts.engineStartClip, false, true,false);
		} else {
			setAudioState(otherCarParts.engineAudio, 5, 0, otherCarParts.engineClip, false, false,true);
			setAudioState(otherCarParts.engineAudio, 5, 1, otherCarParts.engineEndClip, false, true,false);
			motorInput = 0;
			steerInput = 0;
			boostInput = 1;
			//stop the boost
			if (usingBoost) {
				usingBoost = false;
				vCamera.usingBoost (false,"Boost");
				usingBoosting ();
				boostInput = 1;
			}
		}
		//set the same state in the IK driving and in the gravity control components
		IKManager.startOrStopVehicle (driving,otherCarParts.chassis,normal,nextPlayerPos);
		GetComponent<vehicleGravityControl>().changeGravityControlState(driving);
	}
	//the vehicle has been destroyed, so disabled every component in it
	public void disableVehicle(){
		//stop the audio sources
		setAudioState (otherCarParts.engineAudio, 5, 0, otherCarParts.engineClip, false, false,false);
		setAudioState (otherCarParts.skidAudio, 5, 0, otherCarParts.skidClip, false, false,false);
		setAudioState (otherCarParts.engineStartAudio, 5, 0.7f, otherCarParts.engineStartClip, false, false,false);
		vehicleDestroyed = true;
		//stop the boost
		if (usingBoost) {
			usingBoost = false;
			vCamera.usingBoost (false,"Boost");
			usingBoosting ();
			boostInput = 1;
		}
		//disable the skid particles
		for (i = 0; i < wheelsList.Count; i++) {
			wheelsList[i].wheelParticles.GetComponent<ParticleEmitter>().emit = false;
		}
		//disable the exhausts particles
		for (i = 0; i < otherCarParts.normalExhaust.Count; i++) {
			otherCarParts.normalExhaust [i].emit = false;
		}
		for (i = 0; i < otherCarParts.heavyExhaust.Count; i++) {
			otherCarParts.heavyExhaust[i].emit = false;
		}
		//disable the controller
		GetComponent<motorBikeController> ().enabled = false;
	}
	//get the current normal in the gravity control component
	public void setNormal(Vector3 normalValue){
		normal = normalValue;
	}
	//change the gear in the vehicle
	IEnumerator changeGear(int gear){
		changingGear = true;
		setAudioState (otherCarParts.gearShiftingSound, 5, 0.3f, gearsList [gear].gearShiftingClip, false, true, false);	
		yield return new WaitForSeconds(0.5f);
		changingGear = false;
		currentGear = gear;
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
		if (collision.contacts.Length > 0 && collision.gameObject.tag != "Player") {
			//if the velocity of the collision is higher that the limit
			if (collision.relativeVelocity.magnitude > collisionForceLimit) {
				//set the collision audio with a random collision clip
				if (otherCarParts.crashClips.Length > 0) {
					setAudioState (otherCarParts.crashAudio, 5, 1, otherCarParts.crashClips [UnityEngine.Random.Range (0, otherCarParts.crashClips.Length)], false, true, false);
				}
				//if the vehicle hits another vehicle, apply damage to both of them according to the velocity at the impact
				applyDamage.checkHealth (gameObject, collision.collider.gameObject, 
					collision.relativeVelocity.magnitude * GetComponent<vehicleHUDManager> ().damageMultiplierOnCollision, 
					collision.contacts [0].normal, collision.contacts [0].point, gameObject, false);
			}
			if (collision.relativeVelocity.magnitude > 20) {
				//Vector3 collisionDirection=tra
			}
		}
	}
	//get the input manager component
	public void getInputActionManager(inputActionManager manager){
		actionManager = manager;
	}
	//if the vehicle is using the boost, set the boost particles
	public void usingBoosting(){
		if (otherCarParts.boostParticles) {
			for (int i = 0; i < boostingParticles.Count; i++) {
				if (usingBoost) {
					if (!boostingParticles [i].isPlaying) {
						boostingParticles [i].gameObject.SetActive (true);
						boostingParticles [i].Play ();
						boostingParticles [i].loop = true;
					}
				} else {
					boostingParticles [i].loop = false;
				}
			}
		}
	}
	//use a jump platform
	public void useVehicleJumpPlatform(Vector3 direction){
		mainRigidbody.AddForce (mainRigidbody.mass * direction, ForceMode.Impulse);
	}
	public void setNewJumpPower (float newJumpPower) {
		settings.jumpPower = newJumpPower*100;
	} 
	public void setOriginalJumpPower(){
		settings.jumpPower = originalJumpPower;
	}
	[System.Serializable]
	public class Wheels{
		public string Name;
		public WheelCollider wheelCollider;
		public GameObject wheelMesh;
		public GameObject mudGuard;
		public GameObject suspension;
		public wheelType wheelSide;
		[HideInInspector] public GameObject wheelParticles;
		[HideInInspector] public float suspensionSpringPos;
		[HideInInspector] public float rotationValue;
		[HideInInspector] public float wheelSlipAmountSideways;
		[HideInInspector] public float wheelSlipAmountForward;
		[HideInInspector] public int lastSkidmark = -1;
	}
	public enum wheelType{
		front, rear,
	}
	[System.Serializable]
	public class OtherCarParts{
		public Transform steeringHandlebar;
		public Transform COM;
		public GameObject wheelSlipPrefab;
		public GameObject chassis;
		public AudioClip engineStartClip;
		public AudioClip engineClip;
		public AudioClip engineEndClip;
		public AudioClip skidClip;
		public AudioClip[] crashClips;
		public AudioSource engineStartAudio;
		public AudioSource engineAudio;
		public AudioSource skidAudio;
		public AudioSource crashAudio;
		public AudioSource gearShiftingSound;
		public GameObject boostParticles; 
	}
	[System.Serializable]
	public class Gears{	
		public string Name;
		public AnimationCurve engineTorqueCurve;
		public float gearSpeed;
		public AudioClip gearShiftingClip;
	}
	[System.Serializable]
	public class motorBikeSettings{	
		public LayerMask layer;
		public float engineTorque = 1500;
		public float maxRPM = 6000;
		public float minRPM = 1000;
		public float steerAngleLimit;
		public float highSpeedSteerAngle = 5;
		public float highSpeedSteerAngleAtSpeed = 80;
		public float brake;
		public float maxForwardSpeed;
		public float maxBackwardSpeed;
		public float maxBoostMultiplier;
		public float gearShiftRate = 10;
		public Vector2 chassisLean;
		public Vector2 chassisLeanLimit;
		public GameObject vehicleCamera;
		public bool preserveDirectionWhileInAir;
		public float jumpPower;
		public bool canJump;
		public bool canUseBoost;
		public bool useCurves;
	}
}