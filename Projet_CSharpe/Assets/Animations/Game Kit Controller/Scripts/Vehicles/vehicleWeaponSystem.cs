#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[SerializeField]
public class vehicleWeaponSystem : MonoBehaviour {
	public bool weaponsActivate;
	public AudioSource weaponsEffectsSource;
	public AudioClip outOfAmmo;
	public GameObject locatedEnemyIcon;
	public LayerMask layer;
	public float minimumX = 25;
	public float maximumX = 315;
	public float minimumY = 360;
	public float maximumY = 360;
	public GameObject baseX;
	public GameObject baseY;
	public Transform weaponLookDirection;
	public int weaponsSlotsAmount;
	public Vector2 touchZoneSize;
	public float minSwipeDist=20;
	public bool showGizmo;
	public Color gizmoColor;
	public bool reloading;
	public bool aimingCorrectly;
	public List<vehicleWeapons> weapons=new List<vehicleWeapons>();
	[HideInInspector] public GameObject vehicle;
	[HideInInspector] public vehicleWeapons currentWeapon;
	List<GameObject> locatedEnemiesIcons=new List<GameObject>();
	List<GameObject> locatedEnemies=new List<GameObject>();
	List<GameObject> shells=new List<GameObject>();
	float rotationY = 0;
	float rotationX = 0;
	float lastShoot=0;
	float destroyShellsTimer=0;
	int choosedWeapon;
	int i,j,k;
	bool homingProjectiles;
	bool usingLaser;
	bool launchingBarrel;
	bool objectiveFound;
	bool touchPlatform;
	bool touchEnabled;
	bool touching;
	GameObject explosiveBarrelClone;
	GameObject swipeCenterPosition;
	GameObject closestEnemy;
	Transform mainCamera;
	inputActionManager actionManager;
	vehicleHUDManager hudManager;
	RaycastHit hit;
	launchTrayectory parable;
	Touch currentTouch;
	Rect touchZoneRect;
	Vector3 swipeStartPos;
	Vector3 aimedZone;
	vehicleCameraController vehicleCameraManager;

	void Start (){
		//get every the ammo per clip of every weapon according to their initial clip size
		for (i = 0; i < weapons.Count; i++) {
			weapons [i].ammoPerClip = weapons [i].clipSize;
			if (weapons [i].weapon) {
				//get the parable in the barrel launcher
				if (weapons [i].weapon.GetComponentInChildren<launchTrayectory> ()) {
					parable = weapons [i].weapon.GetComponentInChildren<launchTrayectory> ();
				}
			}
		}
		//get vehicle hud manager
		hudManager = GetComponent<vehicleHUDManager>();
		//check the current type of platform
		touchPlatform=touchJoystick.checkTouchPlatform ();
		//set the touch zone in the right upper corner of the screen to swipe between weapons in vehicles when the platform is a touch device
		setHudZone ();
		vehicleCameraManager = transform.parent.GetComponentInChildren<vehicleCameraController> ();
	}
	void Update (){
		if (weaponsActivate) {
			//rotate the weapon to look in the camera direction
			Quaternion cameraDirection = Quaternion.LookRotation (mainCamera.forward);
			weaponLookDirection.transform.rotation = cameraDirection;
			float angleX = weaponLookDirection.transform.localEulerAngles.x;
			//clamp the angle of the weapon, to avoid a rotation higher that the camera
			//in X axis
			if (angleX >= 0 && angleX <= minimumX) {
				rotationX = angleX;
				aimingCorrectly = true;
			} else if (angleX >= maximumX && angleX <= 360) {
				rotationX = angleX;
				aimingCorrectly = true;
			} else {
				aimingCorrectly = false;
			}
			//in Y axis
			float angleY = weaponLookDirection.transform.localEulerAngles.y;
			if (angleY >= 0 && angleY <= minimumY) {
				rotationY = angleY;
			} else if (angleY >= maximumY && angleY <= 360) {
				rotationY = angleY;
			}
			//rotate every transform of the weapon base
			baseY.transform.localEulerAngles = new Vector3 (0, rotationY, 0);
			baseX.transform.localEulerAngles = new Vector3 (rotationX, 0, 0);
			//fire the current weapon
			if (actionManager.getActionInput ("Shoot Weapon")) {
				shootWeapon ();
			}
			//if the shoot button is released, reset the last shoot timer
			if (actionManager.getActionInput ("Stop Shoot Weapon")) {
				lastShoot = 0;
			}
			//check if a key number has been pressed, to change the current weapon for the key pressed, if there is a weapon using that key
			for (i = 0; i <weaponsSlotsAmount; i++) {
				if (Input.GetKeyDown ("" + (i + 1))) {
					for (k=0; k<weapons.Count; k++) {
						if (weapons[k].numberKey == (i + 1) && choosedWeapon != k) {
							if (weapons [k].enabled) {
								choosedWeapon = k;
								weaponChanged ();
							}
						}
					}
				}
			}
			//select the power using the mouse wheel or the change power buttons
			if (actionManager.getActionInput ("Next Weapon")) {
				chooseNextWeapon ();
			}
			if (actionManager.getActionInput ("Previous Weapon")) {
				choosePreviousWeapon ();
			}
			//if the touch controls are enabled, activate the swipe option
			if (touchEnabled) {
				//select the weapon by swiping the finger in the right corner of the screen, above the weapon info
				int touchCount = Input.touchCount;
				if (!touchPlatform) {
					touchCount++;
				}
				for (i = 0; i < touchCount; i++) {
					if (!touchPlatform) {
						currentTouch = touchJoystick.convertMouseIntoFinger ();
					}
					else{
						currentTouch = Input.GetTouch(i);
					}
					//get the start position of the swipe
					if (currentTouch.phase == TouchPhase.Began) {
						if (touchZoneRect.Contains (currentTouch.position) && !touching) {
							swipeStartPos = currentTouch.position;
							touching = true;
						}
					}
					//and the final position, and get the direction, to change to the previous or the next power
					if (currentTouch.phase == TouchPhase.Ended && touching) {
						float swipeDistHorizontal = (new Vector3 (currentTouch.position.x, 0, 0) - new Vector3 (swipeStartPos.x, 0, 0)).magnitude;
						if (swipeDistHorizontal > minSwipeDist) {
							float swipeValue = Mathf.Sign (currentTouch.position.x - swipeStartPos.x);
							if (swipeValue > 0) {
								//right swipe
								choosePreviousWeapon ();
							} else if (swipeValue < 0) {
								//left swipe
								chooseNextWeapon ();
							}
						}
						touching = false;
					}
				}
			} 
			//if the current wepapon is the homming missiles
			if (homingProjectiles) {
				//check if the amount of locoted enemies is equal or lower that the number of remaining projectiles
				if (weapons [choosedWeapon].clipSize >= locatedEnemies.Count*weapons [choosedWeapon].projectilePosition.Count) {
					//uses a ray to detect enemies, to locked them
					if (Physics.Raycast (Camera.main.transform.position, Camera.main.transform.TransformDirection (Vector3.forward), out hit, Mathf.Infinity, layer)) {
						if (hit.collider.GetComponent<characterDamageReceiver>()){
							GameObject enemyTarget = hit.collider.GetComponent<characterDamageReceiver> ().character;
							if (enemyTarget.tag == "enemy") {
								if (!locatedEnemies.Contains (enemyTarget)) {
									//if an enemy is detected, add it to the list of located enemies and instantiated an icon in screen to follow the enemy
									locatedEnemies.Add (enemyTarget);
									GameObject locatedEnemyIconClone = (GameObject)Instantiate (locatedEnemyIcon, Vector3.zero, Quaternion.identity);
									locatedEnemyIconClone.transform.parent = locatedEnemyIcon.transform.parent;
									locatedEnemyIconClone.SetActive (true);
									locatedEnemyIconClone.GetComponent<locatedEnemy> ().setTarget (hit.collider.gameObject);
									locatedEnemiesIcons.Add (locatedEnemyIconClone);
								}
							}
						}
					}
				} 
				//the clip is empty, so reload it
				else {
					StartCoroutine (waitToReload (weapons [choosedWeapon].reloadTime));
					//checkRemainAmmo ();
				}
				//if the button to shoot is released, shoot a homing projectile for every located enemy
				if (actionManager.getActionInput ("Stop Shoot Weapon")) {
					//check that the located enemies are higher that 0
					if (locatedEnemies.Count > 0) {
						//shoot the missiles
						createMuzzleFlash ();
						for (i=0; i<locatedEnemies.Count; i++) {
							for (j = 0; j < weapons [choosedWeapon].projectilePosition.Count; j++) {
								GameObject projectile = (GameObject)Instantiate ( weapons [choosedWeapon].projectileToShoot, weapons [choosedWeapon].projectilePosition [j].position, 
									weapons [choosedWeapon].projectilePosition[j].rotation);
								projectile.GetComponent<vehicleWeaponProjectile> ().getWeaponInfo (weapons [choosedWeapon], weapons [choosedWeapon].projectileDamage, 
									weapons [choosedWeapon].particles, weapons [choosedWeapon].projectileSoundEffect, weapons [choosedWeapon].projectileSpeed, 
									weapons [choosedWeapon].projectileForce, weapons [choosedWeapon].explosionForce, weapons [choosedWeapon].explosionRadius, 
									weapons [choosedWeapon].isExplosive, weapons [choosedWeapon].isHomming, weapons[choosedWeapon].scorch);
								projectile.GetComponent<vehicleWeaponProjectile> ().setEnemy (locatedEnemies [i]);
								//play the shoot sound and reduce the amount of ammo
								playWeaponSoundEffect (true);
								useAmmo ();
							}
						}
						locatedEnemies.Clear ();
						removeLocatedEnemiesIcons ();
						checkShotShake ();
					}
					homingProjectiles = false;
				}
			}
			//if the current weapon is the laser
			if (usingLaser) {
				//play the animation
				weapons [choosedWeapon].weapon.GetComponent<Animation> ().Play (weapons [choosedWeapon].animation);
				//reduce the amount of ammo
				useAmmo ();
				//play the sound 
				if (Time.time > lastShoot + weapons [choosedWeapon].fireRate) {
					playWeaponSoundEffect (true);
					lastShoot = Time.time;
				}
				//if the fire button is released, stop the laser
				if (actionManager.getActionInput ("Stop Shoot Weapon")) {
					weapons [choosedWeapon].weapon.GetComponentInChildren<vehicleLaser> ().changeLaserState(false);
					usingLaser = false;
				}
				checkShotShake ();
			}
			//if the current weapon is the barrel launcher
			if (launchingBarrel) {
				//if the launcher animation is not being player
				if (!weapons [choosedWeapon].weapon.GetComponent<Animation> ().IsPlaying (weapons [choosedWeapon].animation)) {
					//reverse it and play it again
					if (weapons [choosedWeapon].weapon.GetComponent<Animation> () [weapons [choosedWeapon].animation].speed == 1) {
						weapons [choosedWeapon].weapon.GetComponent<Animation> () [weapons [choosedWeapon].animation].speed = -1; 
						weapons [choosedWeapon].weapon.GetComponent<Animation> () [weapons [choosedWeapon].animation].time = weapons [choosedWeapon].weapon.GetComponent<Animation> () [weapons [choosedWeapon].animation].length;
						weapons [choosedWeapon].weapon.GetComponent<Animation> ().Play (weapons [choosedWeapon].animation);
						explosiveBarrelClone.transform.parent = null;
						//launche the barrel according to the velocity calculated according to the hit point of a raycast from the camera position
						explosiveBarrelClone.GetComponent<Rigidbody> ().isKinematic = false;
						Vector3 newVel = getParableSpeed (explosiveBarrelClone.transform.position, aimedZone);
						if (newVel == -Vector3.one) {
							newVel = explosiveBarrelClone.transform.forward * 100;
						}
						explosiveBarrelClone.GetComponent<Rigidbody> ().AddForce (newVel, ForceMode.VelocityChange);
						return;
					}
					//the launcher has throwed a barrel and the animation is over
					if (weapons [choosedWeapon].weapon.GetComponent<Animation> () [weapons [choosedWeapon].animation].speed == -1) {
						launchingBarrel = false;
						weapons [choosedWeapon].secundaryObject.SetActive (true);
					}
				}
			}
		}
		//if the amount of shells from the projectiles is higher than 0, check the time to remove then
		if (weapons [choosedWeapon].ejectShellOnShot) {
			if (shells.Count > 0) {
				destroyShellsTimer += Time.deltaTime;
				if (destroyShellsTimer > 3) {
					for (int i = 0; i < shells.Count; i++) {
						Destroy (shells [i]);
					}
					shells.Clear ();
					destroyShellsTimer = 0;
				}
			}
		}
	}
	//if the homing missile weapon has been fired or change when enemies were locked, remove the icons from the screen
	void removeLocatedEnemiesIcons(){
		if (locatedEnemiesIcons.Count > 0) {
			//remove the icons in the screen
			for (i = 0; i < locatedEnemiesIcons.Count; i++) {
				Destroy (locatedEnemiesIcons [i]);
			}
			locatedEnemiesIcons.Clear ();
		}
	}
	//play the fire sound or the empty clip sound
	void playWeaponSoundEffect(bool hasAmmo){
		if (hasAmmo) {
			if (weapons [choosedWeapon].soundEffect) {
				weaponsEffectsSource.clip = weapons [choosedWeapon].soundEffect;
				weaponsEffectsSource.Play ();
				//weaponsEffectsSource.PlayOneShot (weapons [choosedWeapon].soundEffect);
			}
		} else {
			if (Time.time > lastShoot + weapons [choosedWeapon].fireRate) {
				weaponsEffectsSource.PlayOneShot (outOfAmmo);
				lastShoot = Time.time;
			}
		}
	}
	//calculate the speed applied to the barrel to make a parable according to a hit point
	Vector3 getParableSpeed(Vector3 origin, Vector3 target) {
		//if a hit point is not found, return
		if (!objectiveFound) {
			return -Vector3.one;
		}
		//get the distance between positions
		Vector3 toTarget = target - origin;
		Vector3 toTargetXZ = toTarget;
		//remove the Y axis value
		toTargetXZ -= transform.InverseTransformDirection( toTargetXZ).y*transform.up;
		float y = transform.InverseTransformDirection( toTarget).y;
		float xz = toTargetXZ.magnitude;
		//get the velocity accoring to distance ang gravity
		float t = Vector3.Distance (origin, target)/20;
		float v0y = y / t + 0.5f * Physics.gravity.magnitude * t;
		float v0xz = xz / t;
		//create result vector for calculated starting speeds
		Vector3 result = toTargetXZ.normalized;        
		//get direction of xz but with magnitude 1
		result *= v0xz;                                
		// set magnitude of xz to v0xz (starting speed in xz plane), setting the local Y value
		result -= transform.InverseTransformDirection(result).y*transform.up;
		result += transform.up*v0y;
		return result;
	}
	//fire the current weapon
	public void shootWeapon(){
		//if the weapon system is active and the clip size higher than 0
		if (!weaponsActivate || hudManager.IKDrivingManager.controlsMenuOpened) {
			return;
		}
		if (weapons [choosedWeapon].clipSize > 0 ) {
			//if the current weapon is the homing missile, set to true and return
			if(weapons[choosedWeapon].Name == "Homming Missile"){
				homingProjectiles=true;
				return;
			}
			//if the current weapon is the laser, enable it and return
			if(weapons[choosedWeapon].Name == "Laser"){
				if (!usingLaser) {
					weapons [choosedWeapon].weapon.GetComponentInChildren<vehicleLaser> ().changeLaserState(true);
					usingLaser = true;
				}
				return;
			}
			//else, fire the current weapon according to the fire rate
			if (Time.time > lastShoot + weapons [choosedWeapon].fireRate) {
				checkShotShake ();
				//play the fire sound
				playWeaponSoundEffect (true);
				//create the muzzle flash
				createMuzzleFlash ();
				//if the current weapon is the barrel launcher
				if (weapons [choosedWeapon].Name == "Barrel Launcher") {
					//if a barrel is not being launched, then 
					if (!launchingBarrel) {
						//create a new barrel, set the launch info in it, so it can exploed at collision
						explosiveBarrelClone = (GameObject)Instantiate (weapons[choosedWeapon].projectileToShoot, weapons [choosedWeapon].projectilePosition[0].position, weapons [choosedWeapon].projectilePosition[0].rotation);
						explosiveBarrelClone.GetComponent<explosiveBarrel> ().barrilCanExplodeState(true,hudManager.IKDrivingManager.player);
						//set the parent and rotation inside the vehicle
						explosiveBarrelClone.transform.SetParent (weapons [choosedWeapon].projectilePosition[0].transform);
						explosiveBarrelClone.transform.localRotation = Quaternion.Euler (new Vector3 (0, 0, -90));
						explosiveBarrelClone.GetComponent<Rigidbody> ().isKinematic = true;
						//if the vehicle has a gravity control component, and the current gravity is not the regular one, add an artifical gravity component to the barrel
						//like this, the barrel can make a parable in any surface and direction, setting its gravity in the same of the vehicle
						if (GetComponent<vehicleGravityControl> ()) {
							if (GetComponent<vehicleGravityControl> ().currentNormal != Vector3.up) {
								explosiveBarrelClone.AddComponent<artificialObjectGravity> ().setCurrentGravity (-GetComponent<vehicleGravityControl> ().currentNormal);
							}
						}
						explosiveBarrelClone.GetComponent<explosiveBarrel> ().setExplosionValues (weapons [choosedWeapon].explosionForce, weapons [choosedWeapon].explosionRadius);
						//play the launch barrel animation
						weapons [choosedWeapon].weapon.GetComponent<Animation>()[weapons [choosedWeapon].animation].speed =1; 
						weapons [choosedWeapon].weapon.GetComponent<Animation> ().Play (weapons [choosedWeapon].animation);
						launchingBarrel = true;
						//disable the barrel model in the weapon
						weapons [choosedWeapon].secundaryObject.SetActive (false);
						lastShoot = Time.time;
						//get the ray hit point where the barrel will fall
						if (Physics.Raycast (Camera.main.transform.position, Camera.main.transform.TransformDirection (Vector3.forward), out hit, Mathf.Infinity, layer)) {
							aimedZone = hit.point;
							objectiveFound = true;
						} else {
							objectiveFound = false;
						}
						useAmmo ();
					}
					return;
				}
				//play the fire animation
				if (weapons [choosedWeapon].weapon && weapons [choosedWeapon].animation != "") {
					weapons [choosedWeapon].weapon.GetComponent<Animation> ().Play (weapons [choosedWeapon].animation);
				}
				//every weapon can shoot 1 or more projectiles at the same time, so for every projectile position to instantiate
				for (j = 0; j < weapons [choosedWeapon].projectilePosition.Count; j++) {
					for (int l = 0; l < weapons [choosedWeapon].projectilesPerShoot; l++) {
						//create the projectile
						GameObject projectile = (GameObject)Instantiate (weapons [choosedWeapon].projectileToShoot, weapons [choosedWeapon].projectilePosition [j].position, weapons [choosedWeapon].projectilePosition [j].rotation);
						//set its direction in the weapon forward or the camera forward according to if the weapon is aimed correctly or not
						if (Physics.Raycast (Camera.main.transform.position, Camera.main.transform.TransformDirection (Vector3.forward), out hit, Mathf.Infinity, layer) && aimingCorrectly && !weapons [choosedWeapon].fireWeaponForward) {
							if (!hit.collider.isTrigger) {
								projectile.transform.LookAt (hit.point);
							}
						}
						//add spread to the projectile
						Vector3 spreadAmount = Vector3.zero;
						if (weapons[choosedWeapon].useProjectileSpread) {
							spreadAmount = setProjectileSpread ();
							projectile.transform.Rotate (spreadAmount);
						}
						//set the info in the projectile, like the damage, the type of projectile, bullet or missile, etc...
						projectile.GetComponent<vehicleWeaponProjectile> ().getWeaponInfo (weapons [choosedWeapon], weapons [choosedWeapon].projectileDamage, 
							weapons [choosedWeapon].particles, weapons [choosedWeapon].projectileSoundEffect, weapons [choosedWeapon].projectileSpeed, 
							weapons [choosedWeapon].projectileForce, weapons [choosedWeapon].explosionForce, weapons [choosedWeapon].explosionRadius, 
							weapons [choosedWeapon].isExplosive, weapons [choosedWeapon].isHomming, weapons[choosedWeapon].scorch);
						//if the weapon shoots setting directly the projectile in the hit point, place the current projectile in the hit point position
						if (weapons [choosedWeapon].useRayCastShoot) {
							Vector3 forwardDirection = Camera.main.transform.TransformDirection (Vector3.forward);
							Vector3 forwardPositon = Camera.main.transform.position;
							if (!aimingCorrectly || weapons [choosedWeapon].fireWeaponForward) {
								forwardDirection = weapons [choosedWeapon].weapon.transform.forward;
								forwardPositon = weapons [choosedWeapon].projectilePosition [j].position;
							}
							if (spreadAmount.magnitude != 0) {
								forwardDirection = Quaternion.Euler (spreadAmount) * forwardDirection;
							}

							if (Physics.Raycast (forwardPositon, forwardDirection, out hit, Mathf.Infinity, layer)) {
								projectile.GetComponent<vehicleWeaponProjectile> ().rayCastShoot (hit.collider, hit.point + hit.normal * 0.2f);
							}
						}
						//if the current weapon is the seeker missile
						if (weapons [choosedWeapon].Name == "Seeker Missiles") {
							//get all the enemies in the scene
							List<GameObject> enemiesInFront = new List<GameObject> ();
							GameObject[] enemiesList = GameObject.FindGameObjectsWithTag ("enemy");
							for (i = 0; i < enemiesList.Length; i++) {
								//get those enemies which are not dead and in front of the camera
								if (enemiesList [i].GetComponent<health> ()) {
									if (!enemiesList [i].GetComponent<health> ().dead) {
										Vector3 screenPoint = Camera.main.WorldToScreenPoint (enemiesList [i].transform.position);
										//the target is visible in the screen
										if (screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < Screen.width && screenPoint.y > 0 && screenPoint.y < Screen.height) {
											enemiesInFront.Add (enemiesList [i]);
										}
									}
								}
							}
							for (i = 0; i < enemiesInFront.Count; i++) {
								//for every enemy in front of the camera, use a raycast, if it finds an obstacle between the enemy and the camera, the enemy is removed from the list
								Vector3 direction = enemiesInFront [i].transform.position - weapons [choosedWeapon].projectilePosition [j].position;
								direction = direction / direction.magnitude;
								float distance = Vector3.Distance (enemiesInFront [i].transform.position, Camera.main.transform.position);
								if (Physics.Raycast (enemiesInFront [i].transform.position, -direction, out hit, distance, layer)) {
									enemiesInFront.RemoveAt (i);
								}
							}
							//finally, get the enemy closest to the vehicle
							float minDistance = Mathf.Infinity;
							for (i = 0; i < enemiesInFront.Count; i++) {
								if (Vector3.Distance (enemiesInFront [i].transform.position, transform.position) < minDistance) {
									minDistance = Vector3.Distance (enemiesInFront [i].transform.position, transform.position);
									closestEnemy = enemiesInFront [i];
									print (closestEnemy.name);
								}
							}
							projectile.GetComponent<vehicleWeaponProjectile> ().setEnemy (closestEnemy);
						}
					}
					//if the current weapon drops shells, create them
					if (weapons [choosedWeapon].ejectShellOnShot) {
						if (weapons [choosedWeapon].shell) {
							GameObject shellClone = (GameObject)Instantiate (weapons [choosedWeapon].shell, weapons [choosedWeapon].shellPosition [j].position, weapons [choosedWeapon].shellPosition [j].rotation);
							shellClone.GetComponent<Rigidbody> ().AddForce (weapons [choosedWeapon].shellPosition [j].right * weapons [choosedWeapon].shellEjectionForce);
							shells.Add (shellClone);
							if (weapons [choosedWeapon].shellDropSoundList.Count > 0) {
								shellClone.GetComponent<AudioSource> ().clip = weapons [choosedWeapon].shellDropSoundList [Random.Range (0, weapons [choosedWeapon].shellDropSoundList.Count - 1)];
							}
							if (shells.Count > 15) {
								GameObject shellToRemove = shells [0];
								shells.RemoveAt (0);
								Destroy (shellToRemove);
							}
						}
						destroyShellsTimer = 0;
					}
					useAmmo ();
					lastShoot = Time.time;
				}
			}
		} 
		//else, the clip in the weapon is over, so check if there is remaining ammo
		else {
			//disable the laser
			if (usingLaser) {
				weapons [choosedWeapon].weapon.GetComponentInChildren<vehicleLaser> ().changeLaserState (false);
				usingLaser = false;
			}
			//if the weapon is not being reloaded, do it
			if (!reloading) {
				StartCoroutine (waitToReload (weapons [choosedWeapon].reloadTime));
			}
			//checkRemainAmmo ();
		}
	}
	public void checkShotShake(){
		if (vehicleCameraManager.currentState.firstPersonCamera) {
			if (weapons [choosedWeapon].shootShakeInfo.useDamageShakeInFirstPerson) {
				vehicleCameraManager.setCameraExternalShake (weapons [choosedWeapon].shootShakeInfo.firstPersonDamageShake);
			}
		} else {
			if (weapons [choosedWeapon].shootShakeInfo.useDamageShakeInThirdPerson) {
				vehicleCameraManager.setCameraExternalShake (weapons [choosedWeapon].shootShakeInfo.thirdPersonDamageShake);
			}
		}
	}
	//create the muzzle flash particles if the weapon has it
	void createMuzzleFlash (){
		if (weapons [choosedWeapon].muzzleParticles) {
			for (j = 0; j < weapons [choosedWeapon].projectilePosition.Count; j++) {
				GameObject muzzleParticlesClone = (GameObject)Instantiate (weapons [choosedWeapon].muzzleParticles, weapons [choosedWeapon].projectilePosition [j].position, weapons [choosedWeapon].projectilePosition [j].rotation);
				Destroy (muzzleParticlesClone, 1);	
				muzzleParticlesClone.transform.parent = weapons [choosedWeapon].projectilePosition [j];
				weapons [choosedWeapon].muzzleParticles.GetComponent<ParticleSystem> ().Play ();
			}
		}
	}
	//decrease the amount of ammo in the clip
	void useAmmo(){
		weapons [choosedWeapon].clipSize--;
		updateAmmo ();
	}
	void updateAmmo(){
		if (!weapons [choosedWeapon].infiniteAmmo) {
			hudManager.useAmmo (weapons [choosedWeapon].clipSize, weapons [choosedWeapon].remainAmmo.ToString());
		} else {
			hudManager.useAmmo (weapons [choosedWeapon].clipSize, "Infinite");
		}
	}
	//check the amount of ammo
	void checkRemainAmmo(){
		//if the weaopn has not infinite ammo
		if (!weapons [choosedWeapon].infiniteAmmo) {
			//if the remaining ammo is lower that the ammo per clip, set the final projectiles in the clip 
			if (weapons [choosedWeapon].remainAmmo < weapons [choosedWeapon].ammoPerClip) {
				weapons [choosedWeapon].clipSize = weapons [choosedWeapon].remainAmmo;
			} 
			//else, refill it
			else {
				weapons [choosedWeapon].clipSize = weapons [choosedWeapon].ammoPerClip;
			}
			//if the remaining ammo is higher than 0, remove the current projectiles added in the clip
			if (weapons [choosedWeapon].remainAmmo > 0) {
				weapons [choosedWeapon].remainAmmo -= weapons [choosedWeapon].clipSize;
			} 
		} else {
			//else, the weapon has infinite ammo, so refill it
			weapons [choosedWeapon].clipSize = weapons [choosedWeapon].ammoPerClip;
		}
	}
	//a delay for reload the weapon
	IEnumerator waitToReload(float amount){
		//if the remmaining ammo is higher than 0 or infinite
		if(weapons [choosedWeapon].remainAmmo > 0 || weapons [choosedWeapon].infiniteAmmo){
			//reload
			reloading = true;
			//play the reload sound
			if (weapons [choosedWeapon].reloadSoundEffect) {
				weaponsEffectsSource.PlayOneShot (weapons [choosedWeapon].reloadSoundEffect);
			}
			//wait an amount of time
			yield return new WaitForSeconds (amount);
			//check the ammo values
			checkRemainAmmo ();
			//stop reload
			reloading = false;
			updateAmmo ();
		}
		else{
			//else, the ammo is over, play the empty weapon sound
			playWeaponSoundEffect (false);
		}
		yield return null;
	}
	//the vehicle has used an ammo pickup, so increase the correct weapon by name
	public void getAmmo(string ammoName, int amount){
		for (int i = 0; i < weapons.Count; i++) {
			if (weapons [i].Name == ammoName) {
				weapons [i].remainAmmo += amount;
				weaponChanged ();
				return;
			}
		}
	}
	//select next or previous weapon
	void chooseNextWeapon(){
		//check the index and get the correctly weapon 
		int max = 0;
		int currentWeapon = weapons [choosedWeapon].numberKey;
		currentWeapon++;
		if (currentWeapon > weaponsSlotsAmount) {
			currentWeapon = 1;
		}
		bool exit = false;
		while (!exit) {
			for (k = 0; k < weapons.Count; k++) {
				if (weapons [k].enabled && weapons [k].numberKey == currentWeapon) {
					choosedWeapon = k;
					exit = true;
				}
			}
			max++;
			if (max > 100) {
				return;
			}
			//get the current weapon index
			currentWeapon++;
			if (currentWeapon > weaponsSlotsAmount) {
				currentWeapon = 1;
			}
		}
		//set the current weapon 
		weaponChanged ();
	}
	void choosePreviousWeapon(){
		int max = 0;
		int currentWeapon = weapons[choosedWeapon].numberKey;
		currentWeapon--;
		if (currentWeapon < 1) {
			currentWeapon = weaponsSlotsAmount;
		}
		bool exit = false;
		while (!exit) {
			for (k = weapons.Count - 1; k >= 0; k--) {
				if (weapons [k].enabled && weapons [k].numberKey == currentWeapon) {
					choosedWeapon = k;
					exit = true;
				}
			}
			max++;
			if (max > 100) {
				return;
			}
			currentWeapon--;
			if (currentWeapon < 1) {
				currentWeapon = weaponsSlotsAmount;
			}
		}
		weaponChanged ();
	}
	//set the info of the selected weapon in the hud 
	void weaponChanged(){
		hudManager.setWeaponName (weapons [choosedWeapon].Name,weapons[choosedWeapon].ammoPerClip,weapons[choosedWeapon].clipSize);
		if (!weapons [choosedWeapon].infiniteAmmo) {
			hudManager.useAmmo (weapons [choosedWeapon].clipSize, weapons [choosedWeapon].remainAmmo.ToString());
		} else {
			hudManager.useAmmo (weapons [choosedWeapon].clipSize, "Infinite");
		}
		//enable or disable the parable linerenderer
		if (weapons [choosedWeapon].Name == "Barrel Launcher") {
			if (parable) {
				parable.changeParableState (true);
			}
		} else {
			if (parable) {
				parable.changeParableState (false);
			}
		}
		//remove the located enemies icon
		removeLocatedEnemiesIcons ();
		currentWeapon = weapons [choosedWeapon];
	}
	//enable or disable the weapons in the vehicle according to if it is being droven or not
	public void changeWeaponState(bool state){
		weaponsActivate = state;
		//the player gets off
		if (!weaponsActivate) {
			rotationX = 0;
			rotationY = 0;
			StartCoroutine (rotateWeapon ());
			//disable the parable linerenderer
			if (parable) {
				parable.changeParableState (false);
			}
			//if the laser is being used, disable it
			if (usingLaser) {
				weapons [choosedWeapon].weapon.GetComponentInChildren<vehicleLaser> ().changeLaserState (false);
				usingLaser = false;
			}
		} 
		//if the player gets in, set the info in the hud
		else {
			touchEnabled = actionManager.input.touchControlsCurrentlyEnabled;
			weaponChanged ();
		}
	}
	//get the input manager
	public void getInputActionManager(inputActionManager manager){
		actionManager = manager;
	}
	//get the camera info of the vehicle
	public void getCameraInfo(Transform camera){
		mainCamera = camera;
	}
	//reset the weapon rotation when the player gets off
	IEnumerator rotateWeapon(){
		Quaternion currentBaseXRotation = baseX.transform.localRotation;
		Quaternion currentBaseYRotation = baseY.transform.localRotation;
		for (float t = 0; t < 1;) {
			t += Time.deltaTime * 3;
			baseX.transform.localRotation = Quaternion.Slerp (currentBaseXRotation,Quaternion.identity, t);
			baseY.transform.localRotation = Quaternion.Slerp (currentBaseYRotation,Quaternion.identity, t);
			yield return null;
		}
	}
	public Vector3 setProjectileSpread(){
		float spreadAmount = weapons[choosedWeapon].spreadAmount;
		if (spreadAmount > 0) {
			Vector3 randomSpread = Vector3.zero;
			randomSpread.x = Random.Range (-spreadAmount, spreadAmount);
			randomSpread.y = Random.Range (-spreadAmount, spreadAmount);
			randomSpread.z = Random.Range (-spreadAmount, spreadAmount);
			return randomSpread;
		}
		return Vector3.zero;
	}
	#if UNITY_EDITOR
	void OnDrawGizmos(){
		DrawGizmos ();
	}
	void OnDrawGizmosSelected(){
		DrawGizmos ();
	}
	void DrawGizmos(){
		if (showGizmo) {
			//set the change weapon touch zone in the right upper corner of the scren, visile as gizmo
			if (!EditorApplication.isPlaying) {
				setHudZone ();
			}
			Gizmos.color = gizmoColor;
			Vector3 touchZone = new Vector3 (touchZoneRect.x + touchZoneRect.width / 2f, touchZoneRect.y + touchZoneRect.height / 2f, swipeCenterPosition.transform.position.z);
			Gizmos.DrawWireCube (touchZone, new Vector3 (touchZoneSize.x, touchZoneSize.y, 0f));
		}
	}
	#endif
	//get the correct size of the rect
	void setHudZone(){
		if (!swipeCenterPosition) {
			swipeCenterPosition = GameObject.Find ("vehicleWeaponsSwipePosition");
		}
		touchZoneRect = new Rect (swipeCenterPosition.transform.position.x - touchZoneSize.x / 2f, swipeCenterPosition.transform.position.y - touchZoneSize.y / 2f, touchZoneSize.x, touchZoneSize.y);
	}
	[System.Serializable]
	public class vehicleWeapons{
		public string Name;
		public int numberKey;
		public bool useRayCastShoot;
		public bool fireWeaponForward;
		public bool enabled;
		public bool infiniteAmmo;
		public int clipSize;
		public int remainAmmo;
		public float projectileSpeed;
		public float projectileForce;
		public bool isExplosive;
		public float explosionForce;
		public float explosionRadius;
		public bool isHomming;
		public float fireRate;
		public float reloadTime;
		public float projectileDamage;
		public int projectilesPerShoot;
		public bool useProjectileSpread;
		public float spreadAmount;
		public GameObject projectileToShoot;
		public List<Transform> projectilePosition =new List<Transform>();
		public bool ejectShellOnShot;
		public GameObject shell;
		public List<Transform> shellPosition =new List<Transform>();
		public float shellEjectionForce=200;
		public List<AudioClip> shellDropSoundList =new List<AudioClip>();
		public GameObject secundaryObject;
		public GameObject scorch;
		public GameObject weapon;
		public string animation;
		public GameObject particles;
		public GameObject muzzleParticles;
		public AudioClip soundEffect;
		public AudioClip projectileSoundEffect;
		public AudioClip reloadSoundEffect;
		[HideInInspector] public int ammoPerClip;
		public vehicleCameraController.shakeSettingsInfo shootShakeInfo;
		public bool showShakeSettings;
	}
}