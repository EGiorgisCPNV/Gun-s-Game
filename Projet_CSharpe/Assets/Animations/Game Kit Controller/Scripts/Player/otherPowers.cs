using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Reflection;

[System.Serializable]
public class otherPowers : MonoBehaviour
{
	public int choosedPower;
	public bool carryingObjects;
	public bool carryingObject;
	public bool wallWalk;
	public bool running;
	public bool activatedShield;
	public bool laserActive;
	public powersSettings settings = new powersSettings ();
	public aimSettings aimsettings = new aimSettings ();
	public shootSettings shootsettings = new shootSettings ();
	public shakeSettingsInfo shakeSettings = new shakeSettingsInfo ();
	public bool aim = false;
	public GameObject laser;
	public Rect touchZoneRect;
	public float auxPowerAmount;
	public float auxHealthAmount;
	public bool usingWeapons;
	changeGravity gravity;
	playerController pController;
	health healthManager;
	grabObjects grabObjectsManager;
	TrailRenderer[] trails;
	GameObject carryObjects;
	GameObject shell;
	GameObject currentLaser;
	GameObject jointObject1;
	GameObject jointObject2;
	GameObject jointParticles1;
	GameObject jointParticles2;
	int i, j, k;
	int amountPowersEnabled;
	Vector3 normalOrig;
	Vector3 laserPosition;
	Vector3 jointDirection1;
	Vector3 jointDirection2;
	Vector3 jointPosition1;
	Vector3 jointPosition2;
	Vector3 swipeStartPos;
	Material[] mats;
	Material[] auxMats;
	List<grabbedObject> grabbedObjectList = new List<grabbedObject> ();
	List<GameObject> locatedEnemies = new List<GameObject> ();
	List<GameObject> locatedEnemiesIcons = new List<GameObject> ();
	RaycastHit hit;
	float force = 0;
	float trailTimer = -1;
	float time = 0;
	float buttonTimer = 0;
	float normalVelocity;
	float normalJumpPower;
	float normalAirSpeed;
	float normalAirControl;
	float powerSelectionTimer;
	float pushCenterDistance;
	float lastTimeUsed;
	float lastTimeFired;
	float jointTimer;
	bool jointKinematic1;
	bool jointKinematic2;
	bool selection;
	bool jointObjects;
	bool dead;
	bool checkRunGravity;
	bool homingProjectiles;
	bool touchPlatform;
	inputManager input;
	powersListManager powersManager;
	menuPause pauseManager;
	Touch currentTouch;
	laserDevice.laserType lasertype;
	AudioSource shootZoneAudioSource;
	Transform mainCameraTransform;
	Camera mainCamera;

	void Start ()
	{
		shootZoneAudioSource = shootsettings.shootZone.GetComponent<AudioSource> ();
		//get the trail renderers in the player's model
		trails = GetComponentsInChildren<TrailRenderer> ();
		//get the side of the player to carry objects
		carryObjects = GameObject.Find ("carryObjects");
		//get the player camera object
		aimsettings.cam = GameObject.Find ("Player Camera");
		//get other components in the player
		gravity = gameObject.GetComponent<changeGravity> ();
		pController = gameObject.GetComponent<playerController> ();
		powersManager = transform.parent.GetComponent<powersListManager> ();
		pauseManager = transform.parent.GetComponent<menuPause> ();
		//get the skinned mesh renderer of the player
		Component component = GetComponentInChildren (typeof(SkinnedMeshRenderer));
		settings.meshCharacter = component as SkinnedMeshRenderer;
		//get the materials of the mesh
		mats = settings.meshCharacter.GetComponent<Renderer> ().materials;
		auxMats = mats;
		//get the slider used when the player launchs objects
		if (settings.slider) {
			settings.slider.gameObject.SetActive (false);
		}
		//get all the important parameters of player controller
		normalVelocity = pController.moveSpeedMultiplier;
		normalJumpPower = pController.jumpPower;
		normalAirSpeed = pController.airSpeed;
		normalAirControl = pController.airControl;

		//get the distance from the empty object in the player to push objects, close to it
		pushCenterDistance = Vector3.Distance (transform.position, shootsettings.pushObjectsCenter.transform.position);
		//set the texture of the current selected power
		shootsettings.selectedPowerHud.texture = shootsettings.powersList [choosedPower].texture;
		//by default the aim mode stays in the right side of the player, but it is checked in the start
		setAimModeSide (false);
		//set a touch zone in the upper left corner of the screen, to change betweeen powers by swiping
		setHudZone ();
		//check if the first mode is enabled when the game is started
		if (gravity.settings.firstPersonView) {
			//set the shoot texture in the touch buttons
			settings.buttonShoot.GetComponent<RawImage> ().texture = settings.buttonShootTexture;
		} else {
			//set the kick texture in the touch buttons
			settings.buttonShoot.GetComponent<RawImage> ().texture = settings.buttonKickTexture;
		}
		//get the input manager
		input = transform.parent.GetComponent<inputManager> ();
		//set the amount of current powers enabled
		for (i = 0; i < shootsettings.powersList.Count; i++) {
			if (shootsettings.powersList [i].enabled) {
				if (amountPowersEnabled + 1 <= shootsettings.powersSlotsAmount) {
					amountPowersEnabled++;
				}
			}
		}
		//check if the platform is a touch device or not
		touchPlatform = touchJoystick.checkTouchPlatform ();
		//set the value of energy avaliable at the beginning of the game
		settings.powerBar.maxValue = shootsettings.powerAmount;
		settings.powerBar.value = shootsettings.powerAmount;
		//store the max amount of energy and health in auxiliar variables, used for the pick ups to check that the player doesn't use more pickups that the neccessary
		auxPowerAmount = shootsettings.powerAmount;
		auxHealthAmount = settings.healthBar.maxValue;
		healthManager = GetComponent<health> ();
		grabObjectsManager = GetComponent<grabObjects> ();
		mainCamera = Camera.main;
		mainCameraTransform = mainCamera.transform;
	}
	//if the player is in aim mode, set his arm horizontally and enable the upper body to rotate with the camera movement
	public float extraRotation;
	public float targetRotation;
	GameObject currentWeapon;
	Coroutine changeExtraRotation;

	void LateUpdate ()
	{
		if (aimsettings.aiming) {


			if (aimsettings.spineVector == Vector3.up) {
				Quaternion rotationX = Quaternion.FromToRotation (aimsettings.spine.transform.InverseTransformDirection (transform.up), mainCameraTransform.InverseTransformDirection (transform.forward));
				Vector3 directionX = rotationX.eulerAngles;
				Quaternion rotationZ = Quaternion.FromToRotation (aimsettings.spine.transform.InverseTransformDirection (transform.forward), mainCameraTransform.InverseTransformDirection (transform.up));
				Vector3 directionZ = rotationZ.eulerAngles;
				aimsettings.spine.transform.localEulerAngles = new Vector3 (directionX.x, aimsettings.spine.transform.localEulerAngles.y, -directionZ.z + extraRotation);
			}
			if (aimsettings.spineVector == Vector3.forward) {
				Quaternion rotationX = Quaternion.FromToRotation (aimsettings.spine.transform.InverseTransformDirection (transform.up), mainCameraTransform.InverseTransformDirection (transform.right));
				Vector3 directionX = rotationX.eulerAngles;
				Quaternion rotationZ = Quaternion.FromToRotation (aimsettings.spine.transform.InverseTransformDirection (transform.up), mainCameraTransform.InverseTransformDirection (transform.up));
				Vector3 directionZ = rotationZ.eulerAngles;
				aimsettings.spine.transform.localEulerAngles = new Vector3 (aimsettings.spine.transform.localEulerAngles.x- extraRotation, aimsettings.spine.transform.localEulerAngles.y, directionZ.y);
			}
			//aimsettings.chest.transform.localEulerAngles = new Vector3 (aimsettings.chest.transform.localEulerAngles.x, aimsettings.chest.transform.localEulerAngles.y, -directionZ.z + extraRotation);

			//hacer un clamp entre -40 y 60
		} else if (extraRotation < targetRotation && extraRotation > 0) {
			if (aimsettings.spineVector == Vector3.up) {
				aimsettings.chest.transform.localEulerAngles = new Vector3 (aimsettings.chest.transform.localEulerAngles.x, aimsettings.chest.transform.localEulerAngles.y, aimsettings.chest.transform.localEulerAngles.z + extraRotation);
		
			}
			if (aimsettings.spineVector == Vector3.forward) {
				aimsettings.chest.transform.localEulerAngles = new Vector3 (aimsettings.chest.transform.localEulerAngles.x - extraRotation, aimsettings.chest.transform.localEulerAngles.y, aimsettings.chest.transform.localEulerAngles.z);
			}
		}
	}

	void checkSetExtraRotationCoroutine (bool state)
	{
		if (changeExtraRotation != null) {
			StopCoroutine (changeExtraRotation);
		}
		changeExtraRotation = StartCoroutine (setExtraRotation (state));
	}

	IEnumerator setExtraRotation (bool state)
	{
		if (targetRotation != 0) {
			for (float t = 0; t < 1;) {
				t += Time.deltaTime;
				if (state) {
					extraRotation = Mathf.Lerp (extraRotation, targetRotation, t);
				} else {
					extraRotation = Mathf.Lerp (extraRotation, 0, t);
				}
				currentWeapon.transform.localEulerAngles = new Vector3 (0, -extraRotation, 0);
				yield return null;
			}
		}
	}

	public void getCurrentWeapon (IKWeaponSystem weapon)
	{
		currentWeapon = weapon.gameObject;
		targetRotation = weapon.extraRotation;
	}
	//use the remaining power of the player, to use any of his powers
	void usePowerBar (float amount)
	{
		shootsettings.powerAmount -= amount;
		auxPowerAmount = shootsettings.powerAmount;
		lastTimeUsed = Time.time;
	}
	//if the player pick a health object, increase his health value
	public void getHealth (float amount)
	{
		if (!healthManager.dead) {
			healthManager.healthAmount += amount;
			//check that the health amount is not higher that the health max value of the slider
			if (healthManager.healthAmount >= settings.healthBar.maxValue) {
				healthManager.healthAmount = settings.healthBar.maxValue;
			}
			auxHealthAmount = healthManager.healthAmount;
			if (healthManager.healthAmount < settings.healthBar.maxValue) {
				healthManager.getHealth (amount);
			}
		}
	}
	//if the player pick a enegy object, increase his energy value
	public void getEnergy (float amount)
	{
		shootsettings.powerAmount += amount;
		//check that the energy amount is not higher that the energy max value of the slider
		if (shootsettings.powerAmount >= settings.powerBar.maxValue) {
			shootsettings.powerAmount = settings.powerBar.maxValue;
		}
		auxPowerAmount = shootsettings.powerAmount;
	}

	void Update ()
	{
//		if (aimsettings.aiming ) {
//			if (!GetComponent<HeadLookController> ().enabled) {
//				GetComponent<HeadLookController> ().enabled = true;
//			}
//		} else {
//			if (GetComponent<HeadLookController> ().enabled) {
//				GetComponent<HeadLookController> ().enabled = false;
//			}
//		}
		//set the health value of the player
		settings.healthBar.value = healthManager.healthAmount;
		settings.powerBar.value = shootsettings.powerAmount;
		//the power is regenerated if the player is not using it, and if the powerRegenerateSpeed is higher than 0
		if (shootsettings.powerRegenerateSpeed > 0 && lastTimeUsed != 0 && !dead) {
			if (Time.time > lastTimeUsed + 1) {
				shootsettings.powerAmount += shootsettings.powerRegenerateSpeed * Time.deltaTime;
				if (shootsettings.powerAmount >= settings.powerBar.maxValue) {
					shootsettings.powerAmount = settings.powerBar.maxValue;
					lastTimeUsed = 0;
				}
				auxPowerAmount = shootsettings.powerAmount;
			}
		} 
		//check that the player is not using a device, so all the key input can be checked
		if (!pauseManager.usingDevice && !pauseManager.usingSubMenu) {
			//enable shield when the player touch a laser
			if (settings.shield.activeSelf && currentLaser) {
				Vector3 targetDir = currentLaser.transform.position - settings.shield.transform.position;
				Quaternion qTo = Quaternion.LookRotation (targetDir);
				settings.shield.transform.rotation = Quaternion.Slerp (settings.shield.transform.rotation, qTo, 10 * Time.deltaTime);
			}
			//enable or disable the shield
			if (input.checkInputButton ("Use Shield", inputManager.buttonType.getKeyDown)) {
				activateShield ();
			}
			//if the shield is enabled, the power decreases
			if (settings.shield.activeSelf && activatedShield && !laserActive) {
				//also, rotates the shield towards the camera direction
				if (mainCameraTransform.parent.transform.localRotation.x < 0) {
					settings.shield.transform.rotation = mainCameraTransform.parent.transform.rotation;
				} else {
					settings.shield.transform.rotation = Quaternion.Euler (aimsettings.cam.transform.eulerAngles);
				}
				usePowerBar (Time.deltaTime * shootsettings.powerUsedByShield);
				if (settings.powerBar.value <= 0) {
					settings.shield.SetActive (false);
					activatedShield = false;
				}
				//the bullets and missiles from the enemies are stored in the shield, so if the player press the right button of the mouse
				//the shoots are sent to its owners if they still alive, else, the shoots are launched in the camera direction
				if (input.checkInputButton ("Secondary Button", inputManager.buttonType.getKeyDown)) {
					shootEnemyProjectiles ();
				}
			}
			if (GetComponent<IKSystem> ().currentAimMode != IKSystem.aimMode.weapons) {
				//check if any keyboard number is preseed, and in that case, check which of it and if a power has that number associated
				for (int i = 0; i < shootsettings.powersSlotsAmount; i++) {
					if (Input.GetKeyDown ("" + (i + 1))) {
						for (k = 0; k < shootsettings.powersList.Count; k++) {
							if (shootsettings.powersList [k].numberKey == (i + 1) && choosedPower != k) {
								if (shootsettings.powersList [k].enabled) {
									choosedPower = k;
									powerChanged ();
								}
							}
						}
					}
				}
				//select the power using the mouse wheel or the change power buttons
				if (input.checkInputButton ("Next Power", inputManager.buttonType.posMouseWheel) &&
				    ((!grabObjectsManager.settings.canUseZoomWhileGrabbed && grabObjectsManager.objectHeld) || !grabObjectsManager.objectHeld)) {
					chooseNextPower ();
				}
				if (input.checkInputButton ("Previous Power", inputManager.buttonType.negMouseWheel) &&
				    ((!grabObjectsManager.settings.canUseZoomWhileGrabbed && grabObjectsManager.objectHeld) || !grabObjectsManager.objectHeld)) {
					choosePreviousPower ();
				}
				if (input.checkInputButton ("Next Power", inputManager.buttonType.getKeyDown)) {
					chooseNextPower ();
				}
				if (input.checkInputButton ("Previous Power", inputManager.buttonType.getKeyDown)) {
					choosePreviousPower ();
				}
			}
			//if the wheel of the mouse rotates, the selected power is showed in the center of the screen a few seconds, and also changed in the hud
			if (selection) {
				powerSelectionTimer -= Time.deltaTime;
				if (powerSelectionTimer < 0) {
					powerSelectionTimer = 0.5f;
					selection = false;
					shootsettings.selectedPowerIcon.gameObject.SetActive (false);
				}
			}
			//if the touch controls are enabled, activate the swipe option
			if (input.touchControlsCurrentlyEnabled) {
				//select the power by swiping the finger in the left corner of the screen, above the selected power icon
				int touchCount = Input.touchCount;
				if (!touchPlatform) {
					touchCount++;
				}
				for (int i = 0; i < touchCount; i++) {
					if (!touchPlatform) {
						currentTouch = touchJoystick.convertMouseIntoFinger ();
					} else {
						currentTouch = Input.GetTouch (i);
					}
					//get the start position of the swipe
					if (currentTouch.phase == TouchPhase.Began) {
						if (touchZoneRect.Contains (currentTouch.position) && !shootsettings.touching) {
							swipeStartPos = currentTouch.position;
							shootsettings.touching = true;
						}
					}
					//and the final position, and get the direction, to change to the previous or the next power
					if (currentTouch.phase == TouchPhase.Ended && shootsettings.touching) {
						float swipeDistHorizontal = (new Vector3 (currentTouch.position.x, 0, 0) - new Vector3 (swipeStartPos.x, 0, 0)).magnitude;
						if (swipeDistHorizontal > shootsettings.minSwipeDist) {
							float swipeValue = Mathf.Sign (currentTouch.position.x - swipeStartPos.x);
							if (swipeValue > 0) {
								//right swipe
								choosePreviousPower ();
							} else if (swipeValue < 0) {
								//left swipe
								chooseNextPower ();
							}
						}
						shootsettings.touching = false;
					}
				}
			} 
			//if the player is editing the power list using the power manager, disable the swipe checking
			else if (powersManager.editingPowers) {
				shootsettings.touching = false;
				return;
			}
			//according to the selected power, when the left button of the mouse is pressed, that power is activated
			if (input.checkInputButton ("Shoot", inputManager.buttonType.getKeyDown)) {
				if (GetComponent<IKSystem> ().currentAimMode == IKSystem.aimMode.weapons) {
					return;
				}
				powerShoot ();
			}
			//two objects are going to be attracted each other
			if (jointObjects) {
				jointTimer -= Time.deltaTime;
				if (jointTimer < 0) {
					removeObjectsJoint ();
					return;
				}
				//when both objects are stored, then it is checked if any of them have a rigidbody, to add force to them or not
				//to this, it is used checkCollisionType, a script that allows to check any type of collision with collider or triggers and enter or exit
				//and also can be configurated if the player want to check if the collision is with a particular object, in this case to both joint object
				//the collision to check is the opposite object
				if (!jointObject1.GetComponent<checkCollisionType> () && !jointObject2.GetComponent<checkCollisionType> ()) {
					jointObject1.AddComponent<checkCollisionType> ();
					jointObject2.AddComponent<checkCollisionType> ();
					jointObject1.GetComponent<checkCollisionType> ().onCollisionEnter = true;
					jointObject2.GetComponent<checkCollisionType> ().onCollisionEnter = true;
					jointObject1.GetComponent<checkCollisionType> ().objectToCollide = jointObject2;
					jointObject2.GetComponent<checkCollisionType> ().objectToCollide = jointObject1;
					if (jointObject1.GetComponent<Rigidbody> ()) {
						jointObject1.GetComponent<Rigidbody> ().useGravity = false;
					}
					if (jointObject2.GetComponent<Rigidbody> ()) {
						jointObject2.GetComponent<Rigidbody> ().useGravity = false;
					}
					//a joint object can be used to be launched to an enemy, hurting him, to check this, it is used launchedObjects
					if (!jointKinematic1) {
						jointObject1.AddComponent<launchedObjects> ();
					}
					if (!jointKinematic2) {
						jointObject2.AddComponent<launchedObjects> ();
					}
				}
				//once the script is added to every object, then the direction of the force to applied is calculated, and checking to which object can be applied
				if (jointObject1.GetComponent<checkCollisionType> () && jointObject2.GetComponent<checkCollisionType> ()) {
					Vector3 heading;
					//check if the player has not rigidbody, so the position to follow is the hit point
					if (jointKinematic1) {
						jointParticles2.transform.transform.LookAt (jointPosition1);
						heading = jointObject2.transform.position - jointPosition1;
					}
				//else, the position of the direction is the position of the object 
				else {
						//also a couple of particles are added
						jointParticles2.transform.transform.LookAt (jointObject1.transform.position);
						heading = jointObject2.transform.position - jointObject1.transform.position;
					}
					jointDirection1 = heading / heading.magnitude;
					Vector3 heading2;
					if (jointKinematic2) {
						jointParticles1.transform.transform.LookAt (jointPosition2);
						heading2 = jointObject1.transform.position - jointPosition2;
					} else {
						jointParticles1.transform.transform.LookAt (jointObject2.transform.position);
						heading2 = jointObject1.transform.position - jointObject2.transform.position;
					}

					jointDirection2 = heading2 / heading2.magnitude; 

					jointParticles1.GetComponent<ParticleSystem> ().startSpeed = Vector3.Distance (jointParticles1.transform.position, jointObject2.transform.position) / 2;
					jointParticles2.GetComponent<ParticleSystem> ().startSpeed = Vector3.Distance (jointParticles2.transform.position, jointObject1.transform.position) / 2;
					//add force to the object, according to the direction of the other object
					if (jointObject1.GetComponent<Rigidbody> () && jointObject2.GetComponent<Rigidbody> ()) {
						jointObject1.GetComponent<Rigidbody> ().AddForce (-jointDirection2 * settings.jointObjectsForce);
						jointObject2.GetComponent<Rigidbody> ().AddForce (-jointDirection1 * settings.jointObjectsForce);
					} else if (jointObject1.GetComponent<Rigidbody> () && !jointObject2.GetComponent<Rigidbody> ()) {
						jointObject1.GetComponent<Rigidbody> ().AddForce (-jointDirection2 * settings.jointObjectsForce);
					} else if (!jointObject1.GetComponent<Rigidbody> () && jointObject2.GetComponent<Rigidbody> ()) {
						jointObject2.GetComponent<Rigidbody> ().AddForce (-jointDirection1 * settings.jointObjectsForce);
					} else {
						//if both objects have not rigidbodies, then cancel the joint
						removeObjectsJoint ();
						return;
					}
					//if the collision happens, the scripts are removed, and every object return to their normal situation
					if (jointObject1.GetComponent<checkCollisionType> ().active || jointObject2.GetComponent<checkCollisionType> ().active) {
						removeObjectsJoint ();
					}
				}
			}
			//if the homing projectiles are being using, then
			if (homingProjectiles) {
				//while the number of located enemies is lowers that the max enemies amount, then
				if (locatedEnemies.Count < shootsettings.homingProjectilesMaxAmount) {
					//uses a ray to detect enemies, to locked them
					if (Physics.Raycast (mainCameraTransform.position, mainCameraTransform.forward, out hit, Mathf.Infinity, settings.layer)) {
						if (hit.collider.GetComponent<characterDamageReceiver> ()) {
							if (!locatedEnemies.Contains (hit.collider.GetComponent<characterDamageReceiver> ().character)
							   && hit.collider.GetComponent<characterDamageReceiver> ().character.tag == "enemy") {
								//if an enemy is detected, add it to the list of located enemies and instantiated an icon in screen to follow the enemy
								locatedEnemies.Add (hit.collider.GetComponent<characterDamageReceiver> ().character);
								GameObject locatedEnemyIconClone = (GameObject)Instantiate (shootsettings.locatedEnemyIcon, Vector3.zero, Quaternion.identity);
								locatedEnemyIconClone.transform.SetParent (shootsettings.locatedEnemyIcon.transform.parent);
								locatedEnemyIconClone.SetActive (true);
								locatedEnemyIconClone.GetComponent<locatedEnemy> ().setTarget (hit.collider.gameObject);
								locatedEnemiesIcons.Add (locatedEnemyIconClone);
							}
						}
					}
				}
				//if the button to shoot is released, shoot a homing projectile for every located enemy
				if (input.checkInputButton ("Shoot", inputManager.buttonType.getKeyUp)) {
					//check that the located enemies are higher that 0
					if (locatedEnemies.Count > 0) {
						createShootParticles ();
						//shoot the missiles
						for (i = 0; i < locatedEnemies.Count; i++) {
							Quaternion rot = mainCameraTransform.rotation;
							shell = (GameObject)Instantiate (shootsettings.bullet, shootsettings.shootZone.position, rot);
							shell.GetComponent<powerProjectile> ().setProjectileInfo (gameObject, settings.shield, shootsettings.powersList [choosedPower],
								shootsettings.powersList [choosedPower].projectileDamage, shootsettings.powersList [choosedPower].impactSoundEffect,
								shootsettings.powersList [choosedPower].scorch, shootsettings.powersList [choosedPower].projectileSpeed);
							shell.GetComponent<powerProjectile> ().setEnemy (locatedEnemies [i]);
							if (shootsettings.powersList [choosedPower].useRayCastShoot) {
								Vector3 forwardDirection = mainCameraTransform.TransformDirection (Vector3.forward);
								Vector3 forwardPositon = mainCameraTransform.position;
								if (Physics.Raycast (forwardPositon, forwardDirection, out hit, Mathf.Infinity, settings.layer)) {
									shell.GetComponent<powerProjectile> ().rayCastShoot (hit.collider, hit.point);
								}
							}
						}
						//remove the icons in the screen
						removeLocatedEnemiesIcons ();
						//decrease the value of the power bar
						shootZoneAudioSource.PlayOneShot (shootsettings.powersList [choosedPower].shootSoundEffect);
						usePowerBar (shootsettings.powersList [choosedPower].amountPowerNeeded);
						locatedEnemies.Clear ();
						GetComponent<IKSystem> ().startRecoil ();
					}
					homingProjectiles = false;
				}
			}
			//activate or deactivate the aim mode, checking that the gravity power is active and nither the first person mode
			if (input.checkInputButton ("Aim", inputManager.buttonType.getKeyDown) && !dead && settings.aimModeEnabled) {
				if (!pController.powerActive && !gravity.settings.firstPersonView && pController.onGround && !pController.sphereModeActive) {
					//check if the player is crouched, to prevent that the player enables the aim mode in a place where he can not get up
					if (GetComponent<IKSystem> ().currentAimMode == IKSystem.aimMode.weapons) {
						return;
					}
					if (pController.crouch) {
						pController.crouching ();
					}
					//if the player can get up, or was not crouched, allow to enable or disable the aim mode
					if (!pController.crouch) {
						aimsettings.aiming = !aimsettings.aiming;
						if (aimsettings.aiming) {
							activateAimMode ();
						} else {
							deactivateAimMode ();
						}
					}
				}
			}
			//change the view of the camera according to the situation
			if (input.checkInputButton ("Change Camera", inputManager.buttonType.getKeyDown) && !dead && settings.changeCameraViewEnabled) {
				changeTypeView ();
			}
			//check if the player is moving and he is not using the gravity power
			//in that case according to the duration of the press key, the player will only run or run and change his gravity
			//also in the new version of the asset also check if the touch control is being used
			if (input.checkInputButton ("Run", inputManager.buttonType.getKey) && !dead && settings.runPowerEnabled) {
				if ((pController.moveInput.magnitude > 0 && !pController.powerActive && pController.onGround && !gravity.searching) || running) {
					if (!wallWalk) {
						//check the amount of time that the button is being pressed
						buttonTimer += Time.deltaTime;
						if (!running) {
							run ();
							time = buttonTimer;
							normalOrig = gravity.currentNormal;
						}
						if (buttonTimer > 0.5) {
							if (!gravity.sphere) {
								wallWalk = true;	
							}
							return;
						}
					}
				}
			}
			//if the run button is released, stop the run power, if the power was holding the run button to adhere to other surfaces, else the 
			//run button has to be pressed again to stop the run power
			if (input.checkInputButton ("Run", inputManager.buttonType.getKeyUp) && !dead) {
				if (running && (buttonTimer > 0.5 || buttonTimer - time > 0.12)) {
					stopRun ();
					buttonTimer = 0;
					time = 0;
				}
			}
			//when the player touchs a new surface, he is rotated to it while he stills running
			if (wallWalk) {
				//check a surface in front of the player, to rotate to it
				if (Physics.Raycast (transform.position + transform.up, transform.forward, out hit, 2, settings.layer)) {
					if (!hit.collider.isTrigger) {
						StartCoroutine (gravity.rotateToSurface (hit.normal, 10));
						pController.setNormalCharacter (hit.normal);
					}
				}
				//check if the player is too far from his current ground, to rotate to his previous normal
				if (!Physics.Raycast (transform.position + transform.up, -transform.up, out hit, 5, settings.layer)) {
					if (gravity.currentNormal != normalOrig && !checkRunGravity) {
						checkRunGravity = true;
						StartCoroutine (gravity.rotateToSurface (normalOrig, 2));
						pController.setNormalCharacter (normalOrig);
					}
					if (checkRunGravity && gravity.currentNormal == normalOrig) {
						checkRunGravity = false;
					}
				}
			}
			//grab and carry objets in both sides of the player, every objetc will translate to the closest side of the player, left or right
			//this mode is only when the player press E in the normal mode, in the aim mode, the player only will grab one object at the same time
			if (!carryingObjects && !aimsettings.aiming && !dead && settings.grabObjectsEnabled && !aim && input.checkInputButton ("Grab Objects", inputManager.buttonType.getKeyDown)) {
				carryObjects.GetComponent<Animation> ().Play ("grabObjects");
				carryingObjects = true;
				force = 0;
			}
			if (carryingObjects) {
				//if the player has not grabbedObjects, store them
				if (grabbedObjectList.Count == 0) {
					//check in a radius, the close objects which can be grabbed
					Collider[] objetos = Physics.OverlapSphere (carryObjects.transform.position + transform.up, settings.grabRadius);
					foreach (Collider hits in objetos) {
						if (settings.ableToGrabTags.Contains (hits.GetComponent<Collider> ().tag.ToString ()) && hits.GetComponent<Rigidbody> ()) {
							if (hits.GetComponent<Rigidbody> ().isKinematic) {
								hits.GetComponent<Rigidbody> ().isKinematic = false;
							}
							grabbedObject newGrabbedObject = new grabbedObject ();
							//removed tag and layer after store them, so the camera can still use raycast properly
							GameObject currentObject = hits.gameObject;
							newGrabbedObject.objectToMove = currentObject;
							newGrabbedObject.objectTag = currentObject.tag;
							newGrabbedObject.objectLayer = currentObject.layer;
							currentObject.SendMessage ("pauseAI", true, SendMessageOptions.DontRequireReceiver);
							currentObject.tag = "Untagged";
							currentObject.layer = LayerMask.NameToLayer ("Ignore Raycast");
							currentObject.GetComponent<Rigidbody> ().useGravity = false;
							//get the distance from every object to left and right side of the player, to set every side as parent of every object
							//disable collisions between the player and the objects, to avoid issues
							Physics.IgnoreCollision (currentObject.GetComponent<Collider> (), gameObject.GetComponent<Collider> ());
							if (Vector3.Distance (currentObject.transform.position, carryObjects.transform.GetChild (0).gameObject.transform.position) <
							    Vector3.Distance (currentObject.transform.position, carryObjects.transform.GetChild (1).gameObject.transform.position)) {
								currentObject.transform.SetParent(carryObjects.transform.GetChild (0).gameObject.transform);
								newGrabbedObject.objectToFollow = carryObjects.transform.GetChild (0).gameObject.transform;
							} else {
								currentObject.transform.SetParent (carryObjects.transform.GetChild (1).gameObject.transform);
								newGrabbedObject.objectToFollow = carryObjects.transform.GetChild (1).gameObject.transform;
							}
							//if any object grabbed has its own gravity, paused the script to move the object properly
							if (currentObject.GetComponent<artificialObjectGravity> ()) {
								currentObject.GetComponent<artificialObjectGravity> ().active = false;
							}
							if (currentObject.GetComponent<explosiveBarrel> ()) {
								currentObject.GetComponent<explosiveBarrel> ().barrilCanExplodeState (false, gameObject);
							}
							if (currentObject.GetComponent<crate> ()) {
								currentObject.GetComponent<crate> ().crateCanBeBrokenState(false);
							}
							grabbedObjectList.Add (newGrabbedObject);
						}
					}
					//if there are not any object close to the player, cancel 
					if (grabbedObjectList.Count == 0) {
						carryingObjects = false;
						return;
					}
				} 
				//else, move close to him
				else {
					//when all the objects are stored, then set their position close to the player
					for (k = 0; k < grabbedObjectList.Count; k++) {
						if (grabbedObjectList [k].objectToMove) {
							//if any object is pickable and is inside an opened chest, activate its trigger or if it has been grabbed by the player, remove of the list
							if (grabbedObjectList [k].objectToMove.GetComponent<pickUpObject> ()) {
								grabbedObjectList [k].objectToMove.GetComponent<pickUpObject> ().activateObjectTrigger ();
							}
							float distance = Vector3.Distance (grabbedObjectList [k].objectToMove.transform.localPosition, Vector3.zero);
							if (distance > 0.8f) {
								Vector3 nextPos = grabbedObjectList [k].objectToFollow.position;
								Vector3 currPos = grabbedObjectList [k].objectToMove.transform.position;
								grabbedObjectList [k].objectToMove.GetComponent<Rigidbody> ().velocity = (nextPos - currPos) * 5;
							} else {
								grabbedObjectList [k].objectToMove.GetComponent<Rigidbody> ().velocity = Vector3.zero;
							}
						} else {
							grabbedObjectList.RemoveAt (k);
						}
					}
				}
			}
			//the objects can be dropped or launched, according to the duration of the key press, in the camera direction
			if (carryingObjects && !dead && input.checkInputButton ("Grab Objects", inputManager.buttonType.getKey)) {
				if (!carryObjects.GetComponent<Animation> ().IsPlaying ("grabObjects")) {
					if (force < 3500) {
						//enable the power slider in the center of the screen
						force += Time.deltaTime * 1200;
						if (settings.slider && force > 300) {
							settings.slider.value = force;
							if (!settings.slider.gameObject.activeSelf) {
								settings.slider.gameObject.SetActive (true);
							}
							aim = true;
						}
					}
				}
			}
			//drop or thrown the objects
			if (carryingObjects && !dead && input.checkInputButton ("Grab Objects", inputManager.buttonType.getKeyUp)) {
				dropObjects ();
			}
			//show a cursor in the center of the screen to aim when the player is going to launch some objects
			if ((aim || usingWeapons) && settings.cursor) {
				if (!settings.cursor.activeSelf) {
					settings.cursor.SetActive (true);
				}
			}
			if (!aim && !usingWeapons && !aimsettings.aiming && settings.cursor) {
				if (settings.cursor.activeSelf) {
					settings.cursor.SetActive (false);
				}
			}
			//stop the running action if the player is not moving
			if (pController.moveInput.magnitude == 0) {
				if (running) {
					stopRun ();
				}
			}
		}
		//just a configuration to the trails in the player
		if (settings.trailsActive) {
			if (trailTimer > 0) {
				trailTimer -= Time.deltaTime;
				for (j = 0; j < trails.Length; j++) {
					trails [j].time -= Time.deltaTime;
				}
			}
			if (trailTimer <= 0 && trailTimer > -1) {
				for (j = 0; j < trails.Length; j++) {
					trails [j].enabled = false;
				}
				trailTimer = -1;
			}
		}
	}
	//remove the localte enemies icons
	void removeLocatedEnemiesIcons ()
	{
		if (locatedEnemiesIcons.Count > 0) {
			for (i = 0; i < locatedEnemiesIcons.Count; i++) {
				Destroy (locatedEnemiesIcons [i]);
			}
			locatedEnemiesIcons.Clear ();
		}
	}
	//set the choosed power value in the next, changing the type of shoot action
	public void chooseNextPower ()
	{
		//if the wheel mouse or the change power button have been used and the powers can be changed, then
		if (amountPowersEnabled > 1 && settings.changePowersEnabled) {
			//increase the index
			int max = 0;
			int currentPower = shootsettings.powersList [choosedPower].numberKey;
			currentPower++;
			//if the index is higher than the current powers slots, reset the index
			if (currentPower > shootsettings.powersSlotsAmount) {
				currentPower = 1;
			}
			bool exit = false;
			while (!exit) {
				//get which is the next power in the list, checking that it is enabled
				for (k = 0; k < shootsettings.powersList.Count; k++) {
					if (shootsettings.powersList [k].enabled && shootsettings.powersList [k].numberKey == currentPower) {
						choosedPower = k;
						exit = true;
					}
				}
				max++;
				if (max > 100) {
					//print ("forward error in index");
					return;
				}
				//set the current power
				currentPower++;
				if (currentPower > shootsettings.powersSlotsAmount) {
					currentPower = 1;
				}
			}
			//enable the power icon in the center of the screen
			powerChanged ();
		}
	}
	//set the choosed power value in the previous, changing the type of shoot action
	public void choosePreviousPower ()
	{
		//if the wheel mouse or the change power button have been used and the powers can be changed, then
		if (amountPowersEnabled > 1 && settings.changePowersEnabled) {
			//decrease the index
			int max = 0;
			int currentPower = shootsettings.powersList [choosedPower].numberKey;
			currentPower--;
			//if the index is lower than 0, reset the index
			if (currentPower < 1) {
				currentPower = shootsettings.powersSlotsAmount;
			}
			bool exit = false;
			while (!exit) {
				//get which is the next power in the list, checking that it is enabled
				for (k = shootsettings.powersList.Count - 1; k >= 0; k--) {
					if (shootsettings.powersList [k].enabled && shootsettings.powersList [k].numberKey == currentPower) {
						choosedPower = k;
						exit = true;
					}
				}
				max++;
				if (max > 100) {
					//print ("backward error in index");
					return;
				}
				//set the current power
				currentPower--;
				if (currentPower < 1) {
					currentPower = shootsettings.powersSlotsAmount;
				}
			}
			//enable the power icon in the center of the screen
			powerChanged ();
		}
	}
	//every time that a power is selected, the icon of the power is showed in the center of the screen
	//and changed if the upper left corner of the screen
	void powerChanged ()
	{
		if (settings.changePowersEnabled) {
			selection = true;
			powerSelectionTimer = 0.5f;
			grabObjectsManager.dropObject ();
			shootsettings.selectedPowerHud.texture = shootsettings.powersList [choosedPower].texture;
			shootsettings.selectedPowerIcon.texture = shootsettings.powersList [choosedPower].texture;
			shootsettings.selectedPowerIcon.gameObject.SetActive (true);
			removeLocatedEnemiesIcons ();
		}
	}
	//drop or throw the current grabbed objects
	public void dropObjects ()
	{
		if (!carryObjects.GetComponent<Animation> ().IsPlaying ("grabObjects")) {
			//get the point at which the camera is looking, to throw the objects in that direction
			Vector3 hitDirection = Vector3.zero;
			if (Physics.Raycast (mainCameraTransform.position, mainCameraTransform.TransformDirection (Vector3.forward), out hit, Mathf.Infinity, settings.layer)) {
				if (!hit.collider.isTrigger) {
					hitDirection = hit.point;
				}
			}
			for (j = 0; j < grabbedObjectList.Count; j++) {
				grabbedObjectList [j].objectToMove.SendMessage ("pauseAI", false, SendMessageOptions.DontRequireReceiver);
				grabbedObjectList [j].objectToMove.transform.SetParent (null);
				grabbedObjectList [j].objectToMove.GetComponent<Rigidbody> ().useGravity = true;
				grabbedObjectList [j].objectToMove.tag = grabbedObjectList [j].objectTag.ToString ();
				grabbedObjectList [j].objectToMove.layer = grabbedObjectList [j].objectLayer;
				//drop the objects, because the grab objects button has been pressed quickly
				if (force < 300) {
					Physics.IgnoreCollision (grabbedObjectList [j].objectToMove.GetComponent<Collider> (), GetComponent<Collider> (), false);
				}
				//launch the objects according to the amount of time that the player has held the buttton
				if (force > 300) {
					//if the objects are launched, add the script launchedObject, to damage any enemy that the object would touch
					grabbedObjectList [j].objectToMove.AddComponent<launchedObjects> ();
					//if there are any collider in from of the camera, use the hit point, else, use the camera direciton
					if (hitDirection != Vector3.zero) {
						Vector3 throwDirection = hitDirection - grabbedObjectList [j].objectToMove.transform.position;
						throwDirection = throwDirection / throwDirection.magnitude;
						grabbedObjectList [j].objectToMove.GetComponent<Rigidbody> ().AddForce (throwDirection * force * grabbedObjectList [j].objectToMove.GetComponent<Rigidbody> ().mass);
					} else {
						grabbedObjectList [j].objectToMove.GetComponent<Rigidbody> ().AddForce (mainCameraTransform.TransformDirection (Vector3.forward) * force * grabbedObjectList [j].objectToMove.GetComponent<Rigidbody> ().mass);
					}
				}
				//set again the custom gravity of the object
				if (grabbedObjectList [j].objectToMove.GetComponent<artificialObjectGravity> ()) {
					grabbedObjectList [j].objectToMove.GetComponent<artificialObjectGravity> ().active = true;
				}
				if (grabbedObjectList [j].objectToMove.GetComponent<explosiveBarrel> ()) {
					grabbedObjectList [j].objectToMove.GetComponent<explosiveBarrel> ().barrilCanExplodeState (true, gameObject);
				}
				if (grabbedObjectList [j].objectToMove.GetComponent<crate> ()) {
					grabbedObjectList [j].objectToMove.GetComponent<crate> ().crateCanBeBrokenState (true);
				}
			}
			carryingObjects = false;
			grabbedObjectList.Clear ();
			aim = false;
			if (settings.slider) {
				settings.slider.gameObject.SetActive (false);
				settings.slider.value = 0;
			}
		}
	}
	//if the player edits the current powers in the wheel, when a power is changed of place, removed, or added, change its key number to change
	//and the order in the power list
	public void changePowerState (Powers power, int numberKey, bool value, int index)
	{
		//change the state of the power sent
		for (k = 0; k < shootsettings.powersList.Count; k++) {
			if (shootsettings.powersList [k].Name == power.Name) {
				shootsettings.powersList [k].numberKey = numberKey;
				shootsettings.powersList [k].enabled = value;
			}
		}
		//increase or decrease the amount of powers enabled
		amountPowersEnabled += index;
		//if the current power is removed, select the previous
		if (!value && shootsettings.powersList [choosedPower].Name == power.Name) {
			choosePreviousPower ();
		}
		//if all the powers are disabled, disable the icon in the upper left corner of the screen
		if (amountPowersEnabled == 0) {
			shootsettings.selectedPowerHud.texture = null;
			shootsettings.selectedPowerHud.gameObject.SetActive (false);
			shootsettings.selectedPowerIcon.texture = null;
		} 
		//if only a power still enabled and the power is not selected, search and set it.
		else if (amountPowersEnabled == 1) {
			for (k = 0; k < shootsettings.powersList.Count; k++) {
				if (shootsettings.powersList [k].enabled) {
					choosedPower = k;
					shootsettings.selectedPowerHud.gameObject.SetActive (true);
					shootsettings.selectedPowerHud.texture = shootsettings.powersList [choosedPower].texture;
				}
			}
		}
	}
	//if the player selects a power using the wheel and the mouse, set the power closed to the mouse
	public void setPower (Powers power)
	{
		for (k = 0; k < shootsettings.powersList.Count; k++) {
			if (shootsettings.powersList [k].enabled && shootsettings.powersList [k].Name == power.Name) {
				choosedPower = k;
				shootsettings.selectedPowerHud.gameObject.SetActive (true);
				shootsettings.selectedPowerHud.texture = shootsettings.powersList [choosedPower].texture;
			}
		}
	}
	//when the player is in aim mode, and press shoot, it is checked which power is selected, to create a bullet, push objects, etc...
	public void powerShoot ()
	{
		if ((aimsettings.aiming || gravity.settings.firstPersonView) && !pController.powerActive && !laserActive && settings.shootEnabled && !usingWeapons && !dead) {
			if (settings.powerBar.value >= shootsettings.powersList [choosedPower].amountPowerNeeded && !carryingObject && amountPowersEnabled > 0) {
				setLastTimeFired ();
				//this powers search and shoot homing projectiles
				if (shootsettings.powersList [choosedPower].Name == "Shut Down") {
					homingProjectiles = true;
					return;
				}
				shootZoneAudioSource.PlayOneShot (shootsettings.powersList [choosedPower].shootSoundEffect);
				//every power uses a certain amount of the power bar	
				usePowerBar (shootsettings.powersList [choosedPower].amountPowerNeeded);
				//change level's gravity
				if (shootsettings.powersList [choosedPower].Name == "Change Global Gravity") {
					if (Physics.Raycast (mainCameraTransform.position, mainCameraTransform.TransformDirection (Vector3.forward), out hit, Mathf.Infinity, settings.layer)) {
						if (!hit.collider.isTrigger && !hit.collider.gameObject.GetComponent<Rigidbody> ()) {
							createShootParticles ();
							Physics.gravity = -hit.normal * 9.8f;
						}
					}
					GetComponent<IKSystem> ().startRecoil ();
					return;
				}
				//the power number 5 is joint objects, so none bullet is created
				if (shootsettings.powersList [choosedPower].Name == "Join Objects") {
					//this power allows the player to joint two objects, and add force to both in the position of the other, checking if any of the objects 
					//has rigidbody or not, and when both objects collide, the joint is disabled
					if (!jointObject1 || !jointObject2) {
						createShootParticles ();
						//get every object using a raycast
						if (Physics.Raycast (mainCameraTransform.position, mainCameraTransform.forward, out hit, Mathf.Infinity, settings.layer)) {
							if (!jointObject1) {
								jointObject1 = hit.collider.gameObject;
								if (!jointObject1.GetComponent<Rigidbody> ()) {
									jointKinematic1 = true;
									jointPosition1 = hit.point;
								}
								jointParticles1 = (GameObject)Instantiate (shootsettings.powersList [choosedPower].secundaryParticles, hit.point, Quaternion.LookRotation (hit.normal));
								jointParticles1.transform.SetParent (jointObject1.transform);
								jointParticles1.SetActive (true);
								return;
							}
							if (!jointObject2 && jointObject1 != hit.collider.gameObject) {
								jointObject2 = hit.collider.gameObject;
								if (!jointObject2.GetComponent<Rigidbody> ()) {
									jointKinematic2 = true;
									jointPosition2 = hit.point;
								}
								jointParticles2 = (GameObject)Instantiate (shootsettings.powersList [choosedPower].secundaryParticles, hit.point, Quaternion.LookRotation (hit.normal));
								jointParticles2.transform.SetParent (jointObject2.transform);
								jointParticles2.SetActive (true);
							}
						}
					} 
					if (jointObject1 && jointObject2) {
						jointTimer = 5;
						jointObjects = true;
					}
					GetComponent<IKSystem> ().startRecoil ();
					return;
				}
				//this power changes the player's position with the object located with a ray when the player fires
				if (shootsettings.powersList [choosedPower].Name == "Change Objects Position") {
					if (Physics.Raycast (mainCameraTransform.position, mainCameraTransform.TransformDirection (Vector3.forward), out hit, Mathf.Infinity, settings.layer)) {
						if (!hit.collider.isTrigger && hit.collider.gameObject.GetComponent<Rigidbody> ()) {
							createShootParticles ();
							GameObject objectToMove = hit.collider.gameObject;
							Vector3 newPlayerPosition = objectToMove.transform.position;
							Vector3 newObjectPosition = transform.position;
							objectToMove.transform.position = Vector3.one * 100;
							transform.position = Vector3.zero;
							if (Physics.Raycast (newPlayerPosition, -gravity.currentNormal, out hit, Mathf.Infinity, settings.layer)) {
								newPlayerPosition = hit.point;
							}
							if (Physics.Raycast (newObjectPosition + transform.up, -gravity.currentNormal, out hit, Mathf.Infinity, settings.layer)) {
								newObjectPosition = hit.point + transform.up;
							}
							objectToMove.transform.position = newObjectPosition;
							transform.position = newPlayerPosition;
							aimsettings.cam.transform.position = newPlayerPosition;
							//transform.LookAt (hit.collider.transform.position);
							//hit.collider.transform.LookAt (transform.position);
							GetComponent<IKSystem> ().startRecoil ();
						}
					}
					return;
				}
				//the power number 11 is a nano blade
				if (shootsettings.powersList [choosedPower].Name == "Nano Blade") {
					//if the player shoots, instantate the blade and set its direction, velocity, etc...
					createShootParticles ();
					//use a raycast to check if there is any collider in the forward of the camera
					//if hit exits, then rotate the bullet in that direction, else launch the bullet in the camera direction
					GameObject newNanoBlade = (GameObject)Instantiate (shootsettings.nanoBladeProjectile, shootsettings.shootZone.position, mainCameraTransform.rotation);
					newNanoBlade.GetComponent<nanoBlade> ().setProjectileInfo (gameObject, settings.shield, shootsettings.powersList [choosedPower].projectileDamage,
						shootsettings.powersList [choosedPower].impactSoundEffect, shootsettings.powersList [choosedPower].projectileSpeed);
					if (Physics.Raycast (mainCameraTransform.position, mainCameraTransform.TransformDirection (Vector3.forward), out hit, Mathf.Infinity, settings.layer)) {
						if (!hit.collider.isTrigger) {
							newNanoBlade.transform.LookAt (hit.point);
						}
					}
					GetComponent<IKSystem> ().startRecoil ();
					return;
				}
				//the power number 2 is push objects, so any bullet is created
				if (shootsettings.powersList [choosedPower].Name == "Push Objects") {
					//if the power selected is push objects, check the objects close to pushObjectsCenter and add force to them in camera forward direction
					Collider[] colliders = Physics.OverlapSphere (shootsettings.pushObjectsCenter.transform.position, pushCenterDistance);
					for (i = 0; i < colliders.Length; i++) {
						if (colliders [i].GetComponent<Rigidbody> () && colliders [i].gameObject.tag != "Player") {
							colliders [i].SendMessage ("pushCharacter", transform.forward, SendMessageOptions.DontRequireReceiver);
							if (!colliders [i].GetComponent<Rigidbody> ().isKinematic) {
								colliders [i].GetComponent<Rigidbody> ().AddForce (mainCameraTransform.TransformDirection (Vector3.forward) * 4000 * colliders [i].GetComponent<Rigidbody> ().mass);
							}
						}
					}
					GameObject pushParticles = (GameObject)Instantiate (shootsettings.powersList [choosedPower].shootParticles, shootsettings.shootZone.position, mainCameraTransform.rotation);
					pushParticles.SetActive (true);
					GetComponent<IKSystem> ().startRecoil ();
				}
				//in any other current powers, a bullet is istantiated
				else {
					//if the player shoots, instantate the bullet and set its direction, velocity, etc...
					createShootParticles ();
					//use a raycast to check if there is any collider in the forward of the camera
					//if hit exits, then rotate the bullet in that direction, else launch the bullet in the camera direction
					shell = (GameObject)Instantiate (shootsettings.bullet, shootsettings.shootZone.position, mainCameraTransform.rotation);
					shell.GetComponent<powerProjectile> ().setProjectileInfo (gameObject, settings.shield, shootsettings.powersList [choosedPower],
						shootsettings.powersList [choosedPower].projectileDamage, shootsettings.powersList [choosedPower].impactSoundEffect,
						shootsettings.powersList [choosedPower].scorch, shootsettings.powersList [choosedPower].projectileSpeed);
					if (Physics.Raycast (mainCameraTransform.position, mainCameraTransform.TransformDirection (Vector3.forward), out hit, Mathf.Infinity, settings.layer)) {
						if (!hit.collider.isTrigger) {
							Debug.DrawRay (mainCameraTransform.position, mainCameraTransform.TransformDirection (Vector3.forward) * hit.distance, Color.red);
							shell.transform.LookAt (hit.point);

							if (shootsettings.powersList [choosedPower].useRayCastShoot) {
								shell.GetComponent<powerProjectile> ().rayCastShoot (hit.collider, hit.point);
							}
						}
					}

					GetComponent<IKSystem> ().startRecoil ();
				}
			}
			// if the player is holding an object in the aim mode (not many in the normal mode) and press left button of the mouse
			//the gravity of this object is changed, sending the object in the camera direction, and the normal of the first surface that it touchs
			//will be its new gravity
			//to enable previous gravity of that object, grab again and change its gravity again, but this time aim to the actual ground with normal (0,1,0)
			if (carryingObject && grabObjectsManager.currentGrabMode == grabObjects.grabMode.powers) {
				GameObject grabbedObject = grabObjectsManager.objectHeld.gameObject;
				if (grabbedObject.GetComponent<Rigidbody> ()) {
					grabObjectsManager.dropObject ();
					//if the current object grabbed is a vehicle, enable its own gravity control component
					if (grabbedObject.GetComponent<vehicleGravityControl> ()) {
						grabbedObject.GetComponent<vehicleGravityControl> ().activateGravityPower (mainCameraTransform.TransformDirection (Vector3.forward), mainCameraTransform.TransformDirection (Vector3.right));
					} 
					//else, it is a regular object
					else {
						//change the layer, because the object will use a raycast to check the new normal when a collision happens
						grabbedObject.layer = LayerMask.NameToLayer ("gravityObjects");
						//if the object has a regular gravity, attach the scrip and set its values
						if (!grabbedObject.GetComponent<artificialObjectGravity> ()) {
							grabbedObject.AddComponent<artificialObjectGravity> ();
						} 
						grabbedObject.GetComponent<artificialObjectGravity> ().enableGravity (settings.gravityObjectsLayer, settings.highFrictionMaterial);
					}
				}
			}
		}
	}

	public void createShootParticles(){
		if (shootsettings.powersList [choosedPower].shootParticles) {
			GameObject shootParticles = (GameObject)Instantiate (shootsettings.powersList [choosedPower].shootParticles, shootsettings.shootZone.position, Quaternion.LookRotation (mainCamera.transform.forward));
			shootParticles.transform.SetParent ( shootsettings.shootZone);
		}
	}

	public void setLastTimeFired ()
	{
		lastTimeFired = Time.time;
	}

	public float getLastTimeFired ()
	{
		return lastTimeFired;
	}
	//enable and disable the shield when the player want to stop attacks or when he touchs a laser
	public void activateShield ()
	{
		if (settings.powerBar.value > 0 && !laserActive && !dead && settings.shieldEnabled) {
			settings.shield.SetActive (!settings.shield.activeSelf);
			activatedShield = !activatedShield;
		}
	}
	//shoot the bullets and missiles catched by the shield
	public void shootEnemyProjectiles ()
	{
		if (aim) {
			//check if a raycast hits a surface from the center of the screen to forward
			//to set the direction of the projectiles in the shield
			Vector3 direction = mainCameraTransform.TransformDirection (Vector3.forward);
			if (Physics.Raycast (mainCameraTransform.position, mainCameraTransform.TransformDirection (Vector3.forward), out hit, Mathf.Infinity, settings.layer)) {
				direction = hit.point;
			}
			Component[] components = settings.shield.GetComponentsInChildren (typeof(Rigidbody));
			foreach (Component c in components) {
				c.GetComponent<enemyBullet> ().returnBullet (direction, gameObject);
			}
		}
	}
	//if none of the objects joint have rigidbody, the joint is cancelled
	public void removeObjectsJoint ()
	{
		if (jointObject1.GetComponent<checkCollisionType> ()) {
			Destroy (jointObject1.GetComponent<checkCollisionType> ());
		}
		if (jointObject2.GetComponent<checkCollisionType> ()) {
			Destroy (jointObject2.GetComponent<checkCollisionType> ());
		}
		if (jointObject1.GetComponent<launchedObjects> ()) {
			Destroy (jointObject1.GetComponent<launchedObjects> ());
		}
		if (jointObject2.GetComponent<launchedObjects> ()) {
			Destroy (jointObject2.GetComponent<launchedObjects> ());
		}
		if (jointObject1.GetComponent<Rigidbody> ()) {
			jointObject1.GetComponent<Rigidbody> ().useGravity = true;
		}
		if (jointObject2.GetComponent<Rigidbody> ()) {
			jointObject2.GetComponent<Rigidbody> ().useGravity = true;
		}
		jointObjects = false;
		jointObject1 = null;
		jointObject2 = null;
		jointKinematic1 = false;
		jointKinematic2 = false;
		Destroy (jointParticles1);
		Destroy (jointParticles2);
	}
	//if the player dies, check if the player was aiming, grabbing and object, etc... and disable any necessary parameter
	public void death (bool state)
	{
		dead = state;
		if (state) {
			if (aimsettings.cam.GetComponent<playerCamera> ().moveAwayActive) {
				aimsettings.cam.GetComponent<playerCamera> ().moveAwayCamera ();
			} else {
				//check that the player is not in first person view to disable the aim mode
				if (!gravity.settings.firstPersonView) {
					deactivateAimMode ();
				}
			}
			if (pController.crouch) {
				pController.crouching ();
			}
			deactivateLaserForceField ();
			stopRun ();
		}
	}
	//set the direction of the damage arrow to see the enemy that injured the player
	public void setDamageDir (GameObject enemy)
	{
		auxHealthAmount = healthManager.healthAmount;
		if (shakeSettings.useDamageShake) {
			if (!pController.driving) {
				if (shakeSettings.useDamageShakeInThirdPerson && !gravity.settings.firstPersonView) {
					mainCameraTransform.GetComponent<headBob> ().setExternalShakeState (shakeSettings.thirdPersonDamageShake);
				}
				if (shakeSettings.useDamageShakeInFirstPerson && gravity.settings.firstPersonView) {
					mainCameraTransform.GetComponent<headBob> ().setExternalShakeState (shakeSettings.firstPersonDamageShake);
				}
			}
		}
		//settings.damageIcon.transform.GetChild(0).gameObject.active=true;
	}
	//functions to enable or disable the aim mode
	public void activateAimMode ()
	{
		settings.buttonShoot.GetComponent<RawImage> ().texture = settings.buttonShootTexture;
		aimsettings.cam.GetComponent<playerCamera> ().activateAiming (aimsettings.aimSide); 	
		pController.aiming = true;	
		if (GetComponent<IKSystem> ().currentAimMode == IKSystem.aimMode.hands) {				
			aim = true;
			//enable the grab objects mode in aim mode
			grabObjectsManager.aiming = true;
			//if the player is touching by a laser device, enable the laser in the player
			if (laserActive && !laser.activeSelf) {
				laser.SetActive (true);
			}
			//else disable the laser
			if (!laserActive && laser.activeSelf) {
				laser.SetActive (false);
			}
			usingWeapons = false;
			GetComponent<IKSystem> ().changeArmState (1);
		}
		if (GetComponent<IKSystem> ().currentAimMode == IKSystem.aimMode.weapons) {
			usingWeapons = true;
			checkSetExtraRotationCoroutine (true);
		}
	}

	public void deactivateAimMode ()
	{
		settings.buttonShoot.GetComponent<RawImage> ().texture = settings.buttonKickTexture;
		aimsettings.cam.GetComponent<playerCamera> ().deactivateAiming ();
		pController.aiming = false;
		if (GetComponent<IKSystem> ().currentAimMode == IKSystem.aimMode.hands) {
			aim = false;
			//disable the grab objects mode in aim mode, and drop any object that the player has grabbed
			grabObjectsManager.aiming = false;
			grabObjectsManager.dropObject ();
			laser.SetActive (false);
			GetComponent<IKSystem> ().changeArmState (0);
		}
		if (GetComponent<IKSystem> ().currentAimMode == IKSystem.aimMode.weapons) {
			usingWeapons = false;
			checkSetExtraRotationCoroutine (false);
		}
		aimsettings.aiming = false;
	}
	//change the camera view according to the situation
	public void changeTypeView ()
	{
		//in the aim mode, the player can choose which side to aim, left or right
		if (pController.aiming && !gravity.settings.firstPersonView) {
			setAimModeSide (true);
		}
		//in the normal mode, change camera from third to first and viceversa
		if (!pController.aiming && !GetComponent<scannerSystem> ().activate) {
			deactivateAimMode ();
			gravity.changeCameraView ();
			if (!GetComponent<changeGravity> ().settings.firstPersonView) {
				if (GetComponent<scannerSystem> ().activate) {
					GetComponent<scannerSystem> ().disableScanner ();
				}
				//change the place where the projectiles are instantiated back to the hand of the player
				shootsettings.shootZone.SetParent (aimsettings.handActive.transform);
				shootsettings.shootZone.localPosition = Vector3.zero;
			} else {
				//change the place where the projectiles are instantiated to a place below the camera
				shootsettings.shootZone.SetParent (Camera.main.transform);
				shootsettings.shootZone.localPosition = shootsettings.firstPersonShootPosition.localPosition;
			}
			shootsettings.shootZone.localRotation = Quaternion.identity;
		}
	}
	//in the aim mode, the player can change the side to aim, left or right, moving the camera and changing the arm,
	//to configure the gameplay with the style of the player
	public void setAimModeSide (bool state)
	{
		int value;
		if (state) {
			value = (int)aimsettings.aimSide * (-1);
		} else {
			value = (int)aimsettings.aimSide;
		}
		//change to the right side, enabling the right arm
		if (value == 1) {
			aimsettings.handActive = aimsettings.rightHand;
			aimsettings.aimSide = sideToAim.Right;
			GetComponent<IKSystem> ().changeArmSide (true);
		}
		//change to the left side, enabling the left arm
		else {
			aimsettings.handActive = aimsettings.leftHand;
			aimsettings.aimSide = sideToAim.Left;
			GetComponent<IKSystem> ().changeArmSide (false);
		}
		//change the place, in this case, the hand, where the projectiles are instantiated
		if (state) {
			aimsettings.cam.GetComponent<playerCamera> ().changeAimSide (value);
		}
		if (!aimsettings.cam.GetComponent<playerCamera> ().firstPersonActive) {
			shootsettings.shootZone.SetParent (aimsettings.handActive.transform);
			shootsettings.shootZone.localPosition = Vector3.zero;
			shootsettings.shootZone.localRotation = Quaternion.identity;
		}
	}
	//enable disable the laser in the hand of the player, when he is in the range of one
	void activateLaserForceField (Vector3 pos)
	{
		activatedShield = false;
		laserActive = true;
		settings.shield.SetActive (true);
		laserPosition = pos;
		if (aim) {
			laser.SetActive (true);
		}
		if (laser.activeSelf) {
			laser.GetComponent<laserPlayer> ().setLaserInfo (lasertype, currentLaser, laserPosition);
		}
	}

	void deactivateLaserForceField ()
	{
		laserActive = false;
		settings.shield.SetActive (false);  
		laser.SetActive (false);
	}
	//get the laser device that touch the player, not enemy lasers, and if the laser reflects in other surfaces or not
	public void setLaser (GameObject l, laserDevice.laserType type)
	{
		currentLaser = l;
		lasertype = type;
	}
	//set the number of refractions in the laser in another function
	public void setValue (int value)
	{
		laser.GetComponent<laserPlayer> ().reflactionLimit = value + 1;
	}
	//if the player runs, a set of parameters are changed, like the speed of movement, animation, jumppower....
	public void run ()
	{
		if (settings.trailsActive && !gravity.settings.firstPersonView) {
			for (j = 0; j < trails.Length; j++) {
				trails [j].enabled = true;
				trails [j].time = 1;
			}
			trailTimer = -1;
		}
		if (settings.meshCharacter && settings.runMat) {
			Material[] allMats = settings.meshCharacter.GetComponent<Renderer> ().materials;
			for (int m = 0; m < mats.Length; m++) {
				allMats [m] = settings.runMat;
			}
			settings.meshCharacter.GetComponent<Renderer> ().materials = allMats;
		}
		pController.moveSpeedMultiplier = settings.runVelocity;
		pController.animSpeedMultiplier = settings.runVelocity;
		pController.jumpPower = settings.runJumpPower;
		pController.airSpeed = settings.runAirSpeed;
		pController.airControl = settings.runAirControl;
		running = true;
	}
	//when the player stops running, those parameters back to their normal values
	public void stopRun ()
	{
		if (settings.trailsActive) {
			trailTimer = 2;
		}
		if (settings.meshCharacter) {
			settings.meshCharacter.GetComponent<Renderer> ().materials = auxMats;
		}
		pController.moveSpeedMultiplier = normalVelocity;
		pController.animSpeedMultiplier = normalVelocity;
		pController.jumpPower = normalJumpPower;
		pController.airSpeed = normalAirSpeed;
		pController.airControl = normalAirControl;
		if (wallWalk) {
			if (gravity.currentNormal != normalOrig) {
				StartCoroutine (gravity.rotateToSurface (normalOrig, 2));
			}
			pController.setNormalCharacter (normalOrig);
			wallWalk = false;
		}
		running = false;
	}

	public void checkIfDropObject (GameObject objectToCheck)
	{
		for (int j = 0; j < grabbedObjectList.Count; j++) {
			if (grabbedObjectList [j].objectToMove == objectToCheck) {
				print (grabbedObjectList [j].objectToMove.name);
				grabbedObjectList.RemoveAt (j);
				objectToCheck.transform.SetParent (null);
			}
		}
	}
	//draw the touch zone of the panel that allow to change the choosed power, located in the upper left corner
	//you can see it in the left upper corner of hudAndMenus object in the hierachyby selecting the player controller and set the scene window
	//also you can check its size
	#if UNITY_EDITOR
	//draw the lines of the pivot camera in the editor
	void OnDrawGizmos ()
	{
		DrawGizmos ();
	}

	void OnDrawGizmosSelected ()
	{
		DrawGizmos ();
	}

	void DrawGizmos ()
	{
		if (shootsettings.showGizmo) {
			if (!EditorApplication.isPlaying) {
				setHudZone ();
			}
			Gizmos.color = shootsettings.gizmoColor;
			Vector3 touchZone = new Vector3 (touchZoneRect.x + touchZoneRect.width / 2f, touchZoneRect.y + touchZoneRect.height / 2f, shootsettings.hudZone.transform.position.z);
			Gizmos.DrawWireCube (touchZone, new Vector3 (shootsettings.touchZoneSize.x, shootsettings.touchZoneSize.y, 0f));
		}
	}
	#endif
	//get the correct size of the rect
	void setHudZone ()
	{
		touchZoneRect = new Rect (shootsettings.hudZone.transform.position.x - shootsettings.touchZoneSize.x / 2f, shootsettings.hudZone.transform.position.y - shootsettings.touchZoneSize.y / 2f, shootsettings.touchZoneSize.x, shootsettings.touchZoneSize.y);
	}

	[System.Serializable]
	public class powersSettings
	{
		public bool runPowerEnabled;
		public bool aimModeEnabled;
		public bool shieldEnabled;
		public bool grabObjectsEnabled;
		public bool changeCameraViewEnabled;
		public bool shootEnabled;
		public bool changePowersEnabled;
		//if runmat and body are not set, the player will not change his materials, but everything still working properly
		//also if trailsactive is false, the trails will not be activated
		public GameObject cursor;
		public Material runMat;
		public SkinnedMeshRenderer meshCharacter;
		public LayerMask layer;
		public bool trailsActive = true;
		public Slider slider;
		public Slider healthBar;
		public Slider powerBar;
		public float runVelocity = 1.5f;
		public float runJumpPower = 15;
		public float runAirSpeed = 20;
		public float runAirControl = 4;
		public float grabRadius = 10;
		public float jointObjectsForce = 40;
		public List< string> ableToGrabTags = new List< string> ();
		public Texture buttonKickTexture;
		public Texture buttonShootTexture;
		public GameObject buttonShoot;
		public PhysicMaterial highFrictionMaterial;
		public LayerMask gravityObjectsLayer;
		public GameObject shield;
	}

	[System.Serializable]
	public class aimSettings
	{
		public bool aiming;
		public sideToAim aimSide;
		public GameObject chest;
		public GameObject spine;
		public GameObject leftHand;
		public GameObject rightHand;
		public bool aimSideLeft;
		public GameObject cam;
		//due to some models has a different rotation, set in this vector the axis to rotate properly the character spine
		public Vector3 spineVector = new Vector3 (-1, 0, 0);
		public GameObject handActive;
	}

	public enum sideToAim
	{
		Left = -1,
		Right = 1
	}

	[System.Serializable]
	public class shootSettings
	{
		public List<Powers> powersList = new List<Powers> ();
		public float powerAmount;
		public int powersSlotsAmount;
		public float powerUsedByShield;
		public float powerRegenerateSpeed;
		public int homingProjectilesMaxAmount;
		public GameObject locatedEnemyIcon;
		public Color slowObjectsColor;
		[Range (0, 1)] public float slowValue;
		public RawImage selectedPowerIcon;
		public RawImage selectedPowerHud;
		public Transform shootZone;
		public Transform firstPersonShootPosition;
		public GameObject bullet;
		public GameObject nanoBladeProjectile;
		public GameObject pushObjectsCenter;
		public Vector2 touchZoneSize = new Vector2 (3, 3);
		public float minSwipeDist = 20;
		public bool touching;
		public GameObject hudZone;
		public bool showGizmo;
		public Color gizmoColor;
	}

	[System.Serializable]
	public class Powers
	{
		public string Name;
		public int numberKey;
		public Texture texture;
		public bool enabled;
		public bool useRayCastShoot;
		public float amountPowerNeeded;
		public float projectileDamage;
		public float projectileSpeed;
		public AudioClip shootSoundEffect;
		public AudioClip impactSoundEffect;
		public GameObject scorch;
		public GameObject shootParticles;
		public GameObject secundaryParticles;
	}

	[System.Serializable]
	public class grabbedObject
	{
		public GameObject objectToMove;
		public Transform objectToFollow;
		public string objectTag;
		public int objectLayer;
	}

	[System.Serializable]
	public class shakeSettingsInfo
	{
		public bool useDamageShake;
		public bool useDamageShakeInThirdPerson;
		public headBob.externalShakeInfo thirdPersonDamageShake;
		public bool useDamageShakeInFirstPerson;
		public headBob.externalShakeInfo firstPersonDamageShake;
	}
}