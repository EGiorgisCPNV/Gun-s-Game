using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
public class changeGravity : MonoBehaviour {
	public bool powerActive;
	public bool recalculate;
	public bool searching;
	public bool searchNew;
	public bool searchAround;
	public Vector3 currentNormal = new Vector3 (0, 1, 0); 
	public otherSettings settings = new otherSettings();
	[HideInInspector] public bool rotating = false;
	[HideInInspector] public bool sphere;
	[HideInInspector] public bool dead;
	[HideInInspector] public Vector3 direction;
	[HideInInspector] public Vector3 rightAxis;
	float timer = 0.75f;
	float rotateAmount = 40;
	float normalGravityMultiplier;
	Vector3 surfaceNormal; 
	Vector3 turnDirection;
	GameObject cam;
	GameObject father;
	SphereCollider gravityCenterCollider;
	CapsuleCollider playerCollider;
	RaycastHit hit;
	playerController pController;
	playerCamera pCamera;
	otherPowers powers;
	bool choose;
	bool grounded;
	bool lift; 
	bool hover;
	bool turn;
	inputManager input;
	Transform pivot;
	Camera mainCamera;
	menuPause pauseManager;
	playerWeaponsManager weaponsManager;
	Coroutine rotateCharacterState;
	Rigidbody mainRigidbody;
	headBob headBobManager;

	void Start () {
		mainRigidbody = GetComponent<Rigidbody> ();
		//get the main camera
		mainCamera = Camera.main;
		//get the pivot of the camera
		pivot = mainCamera.transform.parent.transform;
		//and the player camera
		cam = pivot.transform.parent.gameObject;
		pCamera = cam.GetComponent<playerCamera> ();
		mainRigidbody.freezeRotation = true; 
		//the gravity center has a sphere collider, that surrounds completly the player to use it when the player searchs a new surface, 
		//detecting the collision with any object to rotate the player to that surface
		//this is done like this to avoid that the player cross a collider when he moves while he searchs a new surface
		//get all the neccessary components in the player
		gravityCenterCollider = settings.gravityCenter.GetComponent<SphereCollider> ();
		playerCollider = GetComponent<CapsuleCollider> ();
		pController = GetComponent<playerController> ();
		powers = GetComponent<otherPowers> ();
		//get the original value of some parameters
		normalGravityMultiplier =pController.gravityMultiplier;
		settings.originalPowerColor = settings.powerColor;
		//get the model of the player
		Component component=GetComponentInChildren(typeof(SkinnedMeshRenderer));
		settings.meshCharacter = component as SkinnedMeshRenderer;
		//get the arrow in the player
		settings.arrow=GameObject.Find("characterArrow");
		//get the input and pause manager
		input = transform.parent.GetComponent<inputManager> ();
		pauseManager = transform.parent.GetComponent<menuPause> ();
		weaponsManager = GetComponent<playerWeaponsManager> ();
		headBobManager = mainCamera.GetComponent<headBob> ();
	}

	//playerController set the values of ground in this script and in the camera code
	public void onGroundOrOnAir(bool state){
		grounded = state;
		if (grounded) {
			//the player is on the ground
			//set the states in the camera, on ground, stop any shake of the camera, and back the camera to its regular position if it has been moved
			pCamera.onGroundOrOnAir (true);
			pCamera.stopShakeCamera ();
			pCamera.changeCameraFov (false);
			//stop rotate the player
			turn = false;
			//if the surface where the player lands can be circumnavigated or an moving/rotating object, then keep recalculating the player throught the normal surface
			if (sphere || father) {
				recalculate = true;
			} 
			//else disable this state
			else {
				recalculate = false;
			}
			//set the gravity force applied to the player to its regular state
			pController.gravityMultiplier = normalGravityMultiplier;
			//set the model rotation to the regular state
			checkRotateCharacter (Vector3.zero);
			settings.accelerating = false;
		} else {
			//the player is on the air
			pCamera.onGroundOrOnAir (false);
		}
	}

	void Update () {
		//the arrow in the back of the player looks to the direction of the real gravity
		if (settings.arrow) {
			settings.arrow.transform.rotation = Quaternion.LookRotation (new Vector3 (transform.position.x, 0, transform.position.z));
		}
		//activate the power of change gravity
		//one press=the player elevates above the surface if he was in the ground or stops him in the air if he was not in the ground
		//two press=make the player moves in straight direction of the camera, looking a new surface
		//three press=stops the player again in the air
		if (input.checkInputButton ("Gravity Power On", inputManager.buttonType.getKeyDown) && !dead && !pauseManager.usingDevice && settings.gravityPowerEnabled) {
			activateGravityPower ();
		}
		//acelerate the movement of the character when he is moving in the air or falling
		if (input.checkInputButton ("Run", inputManager.buttonType.getKeyDown) && !dead) {
			changeMovementVelocity (true);
		}
		if (input.checkInputButton ("Run", inputManager.buttonType.getKeyUp) && !dead) {
			changeMovementVelocity (false);
		}
		//back gravity to normal, deactivate the power gravity wherever the player is
		if (input.checkInputButton ("Gravity Power Off", inputManager.buttonType.getKeyDown) && !dead && settings.gravityPowerEnabled) {
			deactivateGravityPower ();
		}
		//elevate the player above the ground when the gravity power is enabled and the player was in the ground before it
		if (lift) {
			bool surfaceAbove=false;
			//check if there is any obstacle above the player while he is being elevated, to prevent he can cross any collider
			Ray crouchRay = new Ray (transform.position + transform.up*1.5f, transform.up);
			if (Physics.SphereCast (crouchRay, 0.4f,out hit, 0.5f, settings.layer)) {
				surfaceAbove=true;
			}
			//if the ray doesn't found any surface, keep lifting the player until the timer reachs its target value
			else{
				timer -= Time.deltaTime;
				transform.Translate (Vector3.up * (Time.deltaTime * 4));
				cam.transform.Translate (Vector3.up * (Time.deltaTime * 4));
			}
			//if the timer ends or a surface is found, stop the lifting and start rotate the player to float in the air
			if(surfaceAbove || timer<0){
				lift = false;
				timer = 0.75f;
				searching = false;
				searchAround=false;
				rotateMeshPlayer ();
				setHoverState (true);
			}
		}	
		//moving in the air with the power gravity activated looking for a new surface
		if (searching) {
			//parameters to store the position and the direction of the raycast that checks any close surface to the player
			Vector3 pos;
			Vector3 dir;
			//set the size of the ray
			float distance;
			//if the player has set the direction of the air movement, the raycast starts in camera pivot position
			//else the player is falling and reach certain amount of velocity, so the next surface that he will touch becomes in his new ground
			//and the raycast starts in the player position
			if (!searchNew) {
				distance = 2;
				Vector3 newVelocity = 9.8f * mainRigidbody.mass * direction * settings.speed;
				//when the player searchs a new surface using the gravity force, the player can moves like when he falls
				if (!powers.running) {
					//get the global input and convert it to the local direction, using the axis in the changegravity script
					Vector3 forwardAxis = Vector3.Cross (direction, rightAxis);
					Vector3 newmoveInput = pController.v * forwardAxis + pController.h * rightAxis;
					if (newmoveInput.magnitude > 1) {
						newmoveInput.Normalize ();
					}
					if (newmoveInput.magnitude > 0) {
						newVelocity += newmoveInput * settings.speed * 5;
					}
				}
				//apply and extra force if the player increase his movement
				if (settings.accelerating) {
					newVelocity += direction * settings.accelerateSpeed;
				}
				//make a lerp of the velocity applied to the player to move him smoothly
				mainRigidbody.velocity = Vector3.Lerp (mainRigidbody.velocity, newVelocity, Time.deltaTime * 2);
				//set the direction of the ray that checks any surface
				pos = pivot.transform.position;
				dir = direction;
			} 
			//else, the player is falling in his ground direction, so the ray to check a new surface is below his feet
			else {    
				pos = transform.position;
				dir = -transform.up;
				distance = 0.6f;
			}
			//launch a raycast to check any surface
			Debug.DrawRay (pos, dir * distance, Color.yellow);
			if (Physics.Raycast (pos, dir, out hit, distance, settings.layer)) {
				//if the object detected has not trigger and rigidbody, then
				if (!hit.collider.isTrigger && !hit.rigidbody) {
					//disable the search of the surface and rotate the player to that surface
					pController.powerActive = false;
					powerActive = false;
					searchNew = false;
					searching = false;
					searchAround = false;
					mainRigidbody.velocity = Vector3.zero;
					//disable the collider in the gravity center and enable the capsule collider in the player
					gravityCenterCollider.enabled = false;
					playerCollider.isTrigger = false;
					//check if the object detected can be circumnavigate
					if (hit.collider.gameObject.tag == "sphere") {
						sphere = true;
					}
					//check if the object is moving to parent the player inside it
					if (hit.collider.gameObject.tag == "moving") {
						addParent (hit.collider.gameObject);
					}	
					//set the camera in its regular position
					pCamera.changeCameraFov (false);
					//if the new normal is different from the previous normal in the gravity power, then rotate the player
					if (hit.normal != currentNormal) {
						StartCoroutine (rotateToSurface (hit.normal, 2)); 
					}
					//if the player back to the regular gravity value, change its color to the regular state
					if (hit.normal == new Vector3 (0, 1, 0)) {
						StartCoroutine (changeColor (false));
					}	
				}
			}
		}
		//if the player falls and reachs certain velocity, the camera shakes and the mesh of the player rotates
		//also, if the gravity power is activated, look a new surface to change the gravity to the found surface
		if (!grounded && transform.InverseTransformDirection (mainRigidbody.velocity).y < -15 && !searchNew && !choose && !powerActive && 
			!pController.usingJetpack && !pController.jetPackEquiped && !pController.flyModeActive) {
			pCamera.shakeCamera ();
			if (!weaponsManager.carryingWeaponInThirdPerson) {
				rotateMeshPlayer ();
			}
			//if the gravity of the player is different from the regular gravity, start searchin the new surface
			if (currentNormal != new Vector3 (0, 1, 0)) {
				searchNew = true;
				searching = true;
				recalculate = false;
				sphere = false;
			}
		}
		//walk in spheres and moving objects, recalculating his new normal and lerping the player to the new rotation
		if (!lift && !searching && (sphere || father) && recalculate) {
			float distance = 0.5f;
			if (!grounded) {
				distance = 10;
			}
			//get the normal direction of the object below the player, to recalculate the rotation of the player
			if (Physics.Raycast (transform.position, -transform.up, out hit, distance, settings.layer)) {
				if (!hit.collider.isTrigger && !hit.rigidbody) {
					//the object detected can be circumnavigate, so get the normal direction
					if (hit.collider.gameObject.tag == "sphere") {
						surfaceNormal = hit.normal;
					}
					//the object is moving, so get the normal direction and set the player as a children of the moving obejct
					if (hit.collider.gameObject.tag == "moving") {
						surfaceNormal = hit.normal;
						if (!father) {
							addParent ( hit.collider.gameObject);
						}
					} 
					//else remove the parent of the player
					else {
						if (father) {
							removeParent ();
						}
					}
				}
			}
			//recalculate the rotation of the player and the camera according to the normal of the surface under the player
			currentNormal = Vector3.Lerp (currentNormal, surfaceNormal, 10 * Time.deltaTime);
			Vector3 myForward = Vector3.Cross (transform.right, currentNormal);
			Quaternion dstRot = Quaternion.LookRotation (myForward, currentNormal); 
			transform.rotation = Quaternion.Lerp (transform.rotation, dstRot, 10 * Time.deltaTime);
			Vector3 myForwardCamera = Vector3.Cross (cam.transform.right, currentNormal);
			Quaternion dstRotCamera = Quaternion.LookRotation (myForwardCamera, currentNormal);
			cam.transform.rotation = Quaternion.Lerp (cam.transform.rotation, dstRotCamera, 10 * Time.deltaTime);
			//set the normal in the playerController component
			pController.setNormalCharacter (currentNormal);
		}
		//set a cursor in the screen when the character can choose a direction to change his gravity
		if (choose && settings.cursor) {
			if (!settings.cursor.activeSelf) {
				settings.cursor.SetActive (true);
			}
		}
		if (!choose && settings.cursor) {
			if (settings.cursor.activeSelf) {
				settings.cursor.SetActive (false);
			}
		}
		//if the player can choosed a direction, lerp his velocity to zero
		if (choose) {
			mainRigidbody.velocity=Vector3.Lerp(mainRigidbody.velocity,Vector3.zero,Time.deltaTime*2);
		}
		if (rotating && !powers.running) {
			mainRigidbody.velocity=Vector3.zero;
		}
	}
	//rotate randomly the mesh of the player in the air, also make that mesh float while chooses a direction in the air
	void FixedUpdate(){
		if (turn) {
			if (settings.randomRotationOnAirEnabled || powerActive) {
				settings.gravityCenter.transform.Rotate (turnDirection * rotateAmount * Time.deltaTime);
			}
			if (hover) {
				float posTargetY = Mathf.Sin (Time.time * settings.hoverSpeed) * settings.hoverAmount;
				mainRigidbody.position = Vector3.MoveTowards (mainRigidbody.position, mainRigidbody.position+posTargetY*transform.up, Time.deltaTime * settings.hoverSmooth);
			}
		}
	}
	//when the player searchs a new surface using the gravity power on button, check the collisions in the gravity center sphere collider, to change the 
	//gravity of the player to the detected normal direction
	void OnCollisionEnter(Collision col){
		//check that the player is searchin a surface, the player is not running, and that he is searching around
		if (searching && !powers.running && turn && searchAround) {
			//check that the detected object is not a trigger or the player himself
			if (col.gameObject.tag != "Player" && !col.rigidbody && !col.collider.isTrigger) {
				//get the collision contant point to change the direction of the ray that searchs a new direction, setting the direction from the player 
				//to the collision point as the new direction
				Vector3 hitDirection= col.contacts[0].point-pivot.transform.position;
				hitDirection=hitDirection/hitDirection.magnitude;
				direction = hitDirection;
				searchAround=false;
			}
		}
	}
	//now the gravity power is in a function, so it can be called from keyboard and a touch button
	public void activateGravityPower(){
		if (GetComponent<IKSystem> ().currentAimMode == IKSystem.aimMode.weapons) {
			return;
		}
		//if the option to lift the player when he uses the gravity power is disable, then searchs an new surface in the camera direction 
		if (!settings.liftToSearchEnabled) {
			changeOnTrigger(mainCamera.transform.TransformDirection (Vector3.forward), mainCamera.transform.TransformDirection (Vector3.right));
		} 
		//else lift the player, and once that he has been lifted, then press again the gravit power on button to search an new surface
		//or disable the gravity power
		else {
			//enable the sphere collider in the gravity center
			gravityCenterCollider.enabled = true;
			//disable the capsule collider in the player
			playerCollider.isTrigger = true;
			//get the last time that the player was in the air
			pController.lastTimeFalling = Time.time;
			recalculate = false;
			settings.accelerating = false;
			//change the color of the player's textures
			StartCoroutine (changeColor (true));
			searchNew = false;
			removeParent ();
			sphere = false;	
			pController.powerActive = true;
			powerActive = true;
			//calibrate the accelerometer to rotate the camera in this mode
			pCamera.calibrateAccelerometer ();
			//drop any object that the player is holding and disable aim mode
			if (powers.aimsettings.aiming) {
				powers.deactivateAimMode ();
			}
			if (weaponsManager.aimingInThirdPerson || weaponsManager.carryingWeaponInThirdPerson) {
				weaponsManager.drawOrKeepWeapon (false);
			}
			GetComponent<grabObjects> ().dropObject ();
			//the player is in the ground, so he is elevated above it
			if (grounded) {
				lift = true;
				choose = true;
			}
			//the player set the direction of the movement in the air to search a new surface
			if (!lift && choose) {
				pCamera.shakeCamera ();
				setHoverState (false);
				rotateMeshPlayer ();
				searching = true;
				sphere = false;	
				removeParent ();
				choose = false;
				searchAround = true;
				direction = mainCamera.transform.forward;
				//get direction and right axis of the camera, so when the player searchs a new surface, this is used to get the local movement, 
				//which allows to move the player in his local right, left, forward and back while he also displaces in the air
				rightAxis = mainCamera.transform.right;
				checkRotateCharacter (-direction);
				return;
			} 
			//the player is in the air, so he is stopped in it to choose a direction
			if (!grounded && !choose && !lift) {
				pCamera.stopShakeCamera (); 
				pCamera.changeCameraFov (false);
				setHoverState (true); 
				rotateMeshPlayer ();
				choose = true; 	
				searching = false;	
				searchAround = false;
			}
		}
	}

	//now the gravity power is in a function, so it can be called from keyboard and a touch button
	public void deactivateGravityPower(){
		//check that the power gravity is already enabled
		if (choose || searching || currentNormal != new Vector3 (0, 1, 0) && !pauseManager.usingDevice) {
			//disable the sphere collider in the gravity center
			gravityCenterCollider.enabled = false;
			//enable the capsule collider in the player
			playerCollider.isTrigger = false;
			//get the last time that the player was in the air
			pController.lastTimeFalling = Time.time;
			//deactivate aim mode if it was enabled
			if (powers.aimsettings.aiming) {
				powers.deactivateAimMode ();
			}
			if (weaponsManager.aimingInThirdPerson || weaponsManager.carryingWeaponInThirdPerson) {
				weaponsManager.drawOrKeepWeapon (false);
			}
			settings.accelerating = false;
			//set the force of the gravity in the player to its regular state
			pController.gravityMultiplier = normalGravityMultiplier;
			//change the color of the player
			StartCoroutine( changeColor(false));
			choose = false;
			sphere = false;	
			removeParent ();
			setHoverState(false);
			turn = false;
			searching = false;
			searchAround=false;
			lift = false;
			recalculate = false;
			timer = 0.75f;
			//stop to shake the camera and set its position to the regular state
			pCamera.stopShakeCamera();
			pCamera.changeCameraFov (false);
			//if the normal of the player is different from the regular gravity, rotate the player
			if (currentNormal != new Vector3 (0, 1, 0)) {
				StartCoroutine (rotateToSurface (new Vector3 (0, 1, 0), 2));
			}
			//rotate the mesh of the player also
			checkRotateCharacter(new Vector3 (0, 1, 0));
			pController.powerActive = false;
			powerActive=false;
			//set the value of the normal in the playerController component to its regular state
			pController.setNormalCharacter (Vector3.up);
		}
	}

	//now the change of velocity is in a function, so it can be called from keyboard and a touch button
	public void changeMovementVelocity(bool value){
		//if the player is not choosing a gravity direction and he is searching a surface or the player is not in the ground and with a changed normal, then
		if (!choose && (powerActive || (!pController.onGround && currentNormal!=Vector3.up))){
			settings.accelerating = value;
			//move the camera to a further away position, and add extra force to the player's velocity
			if (settings.accelerating) {
				pCamera.changeCameraFov (true);
				pController.gravityMultiplier = settings.highGravityMultiplier;
				//when the player accelerates his movement in the air, the camera shakes
				//if the player accelerates his movement in the air and shake camera is enabled
				if (pCamera.settings.enableShakeCamera) {
					pCamera.accelerateShake(true);			
				}
			} 
			//else, set the camera to its regular position, reset the force applied to the player's velocity
			else {
				pCamera.changeCameraFov (false);
				pController.gravityMultiplier = normalGravityMultiplier;
				pCamera.accelerateShake(false);
			}
		}
	}

	//convert the character in a child of the moving object
	void addParent(GameObject obj){
		father = obj;
		transform.parent = father.transform;
		cam.transform.parent = father.transform;
	}
	//remove the parent of the player, so he moves freely again
	void removeParent(){
		transform.parent = null;
		cam.transform.parent = null;
		father = null;
	}
	//the funcion to change camera view, to be called from a key or a touch button
	public void changeCameraView(){
		settings.firstPersonView = !settings.firstPersonView;
		settings.meshCharacter.enabled = !settings.firstPersonView;
		//disable or enable the mesh of the player
		settings.arrow.transform.GetChild (0).GetComponent<MeshRenderer> ().enabled = !settings.arrow.transform.GetChild (0).GetComponent<MeshRenderer> ().enabled;
		//change to first person view
		if (settings.firstPersonView) {
			GetComponent<damageInScreen> ().pauseOrPlayDamageInScreen (true);
			pCamera.activateFirstPersonCamera ();
			GetComponent<grabObjects> ().aiming = true;
			powers.aim = true;
			//change the icons in the touch controls
			powers.settings.buttonShoot.GetComponent<RawImage> ().texture = powers.settings.buttonShootTexture;
		}
		//change to third person view
		else {
			GetComponent<damageInScreen> ().pauseOrPlayDamageInScreen (false);
			pCamera.deactivateFirstPersonCamera ();
			GetComponent<grabObjects> ().aiming = false;
			GetComponent<grabObjects> ().dropObject ();
			powers.aim = false;
			powers.aimsettings.aiming = false;
			powers.settings.buttonShoot.GetComponent<RawImage> ().texture = powers.settings.buttonKickTexture;
		}
		GetComponent<jetpackSystem> ().enableOrDisableJetPackMesh (!settings.firstPersonView);
		weaponsManager.setCurrentWeaponsParent (settings.firstPersonView);
		headBobManager.setFirstOrThirdHeadBobView (settings.firstPersonView);
		pController.setLastTimeMoved ();
	}

	//set a random direction to rotate the character
	void rotateMeshPlayer(){
		if (!turn) {
			turn = true;
			turnDirection = new Vector3 (Random.Range (-1, 1), Random.Range (-1, 1), Random.Range (-1, 1));
			if (turnDirection.magnitude == 0) {
				turnDirection.x = 1;
			}
		}
	}
	//set if the player is hovering or not
	void setHoverState(bool state){
		hover = state;
	}

	//change the gravity of the player when he touchs the arrow trigger
	public void changeOnTrigger(Vector3 dir, Vector3 right){
		//set the parameters needed to change the player's gravity without using the gravity power buttons
		searchNew = false;
		removeParent ();
		sphere = false;	
		searching = true;
		searchAround=true;
		StartCoroutine( changeColor(true));
		pController.gravityMultiplier = normalGravityMultiplier;
		pController.powerActive = true;
		powerActive = true;
		pCamera.calibrateAccelerometer ();
		rotateMeshPlayer ();
		pCamera.shakeCamera ();
		direction = dir;
		rightAxis=right;
		checkRotateCharacter (-direction);
	}

	//stop the gravity power when the player is going to drive a vehicle
	public void stopGravityPower(){
		//disable the sphere collider in the gravity center
		gravityCenterCollider.enabled = false;
		//get the last time that the player was in the air
		pController.lastTimeFalling = Time.time;
		settings.accelerating = false;
		//set the force of the gravity in the player to its regular state
		pController.gravityMultiplier = normalGravityMultiplier;
		choose = false;
		sphere = false;	
		removeParent ();
		setHoverState(false);
		turn = false;
		searching = false;
		searchAround=false;
		lift = false;
		recalculate = false;
		timer = 0.75f;
		//stop to shake the camera and set its position to the regular state
		pCamera.stopShakeCamera();
		pCamera.changeCameraFov (false);
		pController.powerActive = false;
		powerActive=false;
		//reset the player's rotation
		transform.rotation = Quaternion.identity;
		settings.gravityCenter.transform.localRotation = Quaternion.identity;
		//set to 0 the current velocity of the player
		mainRigidbody.velocity = Vector3.zero;
	}

	//rotate the player, camera and mesh of the player to the new surface orientation
	public IEnumerator rotateToSurface(Vector3 normal, int rotSpeed){
		rotating = true;
		Quaternion rotPlayer = transform.rotation;
		Quaternion rotCamera = cam.transform.rotation;
		Quaternion rotCenter = settings.gravityCenter.transform.localRotation;
		Vector3 myForwardPlayer = Vector3.Cross (transform.right, normal);
		Quaternion dstRotPlayer = Quaternion.LookRotation (myForwardPlayer, normal);
		Vector3 myForwardCamera = Vector3.Cross (cam.transform.right, normal);
		Quaternion dstRotCamera = Quaternion.LookRotation (myForwardCamera, normal);
		Quaternion dstRotCenter = new Quaternion (0, 0, 0, 1);
		for (float t = 0; t < 1;) {
			t += Time.deltaTime * rotSpeed;
			cam.transform.rotation = Quaternion.Slerp (rotCamera,dstRotCamera, t);
			transform.rotation = Quaternion.Slerp (rotPlayer, dstRotPlayer, t);
			settings.gravityCenter.transform.localRotation = Quaternion.Slerp (rotCenter, dstRotCenter, t);
			yield return null;
		}
		currentNormal = normal; 
		pController.gravityMultiplier = normalGravityMultiplier;
		pController.setNormalCharacter (normal);
		rotating = false;
	}

	public void setNormal(Vector3 normal){
		Vector3 myForwardPlayer = Vector3.Cross (transform.right, normal);
		Vector3 myForwardCamera = Vector3.Cross (cam.transform.right, normal);
		Quaternion dstRotPlayer = Quaternion.LookRotation (myForwardPlayer, normal);		
		Quaternion dstRotCamera = Quaternion.LookRotation (myForwardCamera, normal);
		transform.rotation = dstRotPlayer;
		cam.transform.rotation = dstRotCamera;
		currentNormal = normal;
		pController.setNormalCharacter (normal);
	}
	
	//rotate the mesh of the character in the direction of the camera when he selects a gravity direction in the air
	// and to the quaternion identity when he is on ground
	public void checkRotateCharacter(Vector3 normal){
		//get the coroutine, stop it and play it again
		if (rotateCharacterState != null) {
			StopCoroutine (rotateCharacterState);
		}
		rotateCharacterState = StartCoroutine(rotateCharacter(normal));
	}
	public IEnumerator rotateCharacter(Vector3 normal){
		Quaternion orgRotCenter = settings.gravityCenter.transform.localRotation;
		Quaternion dstRotCenter = new Quaternion (0, 0, 0, 1);
		//check that the normal is different from zero, to rotate the player's mesh in the direction of the new gravity when he use the gravity power button
		//and select the camera direction to search a new surface
		//else, the player's mesh is rotated to its regular state
		if (normal != Vector3.zero) {
			orgRotCenter = settings.gravityCenter.transform.rotation;
			Vector3 myForward = Vector3.Cross (settings.gravityCenter.transform.right, normal);
			dstRotCenter = Quaternion.LookRotation (myForward, normal);
		}
		for (float t = 0; t < 1;) {
			t += Time.deltaTime * 3;
			if (normal == Vector3.zero) {
				settings.gravityCenter.transform.localRotation = Quaternion.Slerp (orgRotCenter, dstRotCenter, t);
			} else {
				settings.gravityCenter.transform.rotation = Quaternion.Slerp (orgRotCenter, dstRotCenter, t);
			}
			yield return null;
		}
	}
	//change the mesh color of the character according to the gravity power
	public IEnumerator changeColor(bool value){
		if (settings.meshCharacter) {
			if (value) {
				settings.powerColor=settings.originalPowerColor;
			}
			else{
				settings.powerColor=Color.white;
			}
			SkinnedMeshRenderer skinnedMesh=settings.meshCharacter;
			Renderer skinned = skinnedMesh.GetComponent<Renderer> ();
			for (float t = 0; t < 1;) {
				t += Time.deltaTime;
				for (int i=0; i<settings.materialToChange.Length; i++) {
					if(skinned.materials.Length >= settings.materialToChange.Length){
						if (settings.materialToChange [i] == 1 && skinned.materials[i].HasProperty("_Color")) {
							skinned.materials [i].color = 
								Color.Lerp (skinned.materials [i].color, settings.powerColor, t);
						}
					}
				}
				yield return null;
			}
		}
	}
	//change the object which the camera follows and disable or enabled the powers according to the player state
	public void death(bool state){
		dead = state;
		if (state) {
			deactivateGravityPower();
			turn=false;
			hover=false;
			checkRotateCharacter(Vector3.zero);
		} 
	}
	//a group of parameters to configure
	[System.Serializable]
	public class otherSettings{
		public Transform gravityCenter;
		public bool gravityPowerEnabled;
		public bool liftToSearchEnabled;
		public bool randomRotationOnAirEnabled;
		public GameObject cursor;
		public LayerMask layer;
		public float speed = 10;
		public float accelerateSpeed = 20;
		public float highGravityMultiplier;
		public int[] materialToChange;
		public Color powerColor;
		public float hoverSpeed;
		public float hoverAmount;
		public float hoverSmooth;
		[HideInInspector] public Color originalPowerColor;
		[HideInInspector] public bool accelerating;
		[HideInInspector] public bool firstPersonView;
		[HideInInspector] public SkinnedMeshRenderer meshCharacter;  
		[HideInInspector] public GameObject arrow;
	}
}