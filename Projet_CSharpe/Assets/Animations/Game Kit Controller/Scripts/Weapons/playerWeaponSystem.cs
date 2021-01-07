using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class playerWeaponSystem : MonoBehaviour {
	public GameObject character;
	public weaponInfo weaponSettings;
	public AudioClip outOfAmmo;
	public GameObject weaponProjectile;
	public LayerMask layer;
	public bool reloading;
	public bool carryingWeaponInThirdPerson;
	public bool carryingWeaponInFirstPerson;
	public bool aimingInThirdPerson;
	public bool aimingInFirstPerson;
	List<GameObject> shells=new List<GameObject>();
	float destroyShellsTimer=0;
	int i,j,k;
	RaycastHit hit;
	float lastShoot;
	AudioSource weaponsEffectsSource;
	bool animationForwardPlayed;
	bool animationBackPlayed;
	Transform originalParent;
	IKWeaponSystem IKWeaponManager;
	playerWeaponsManager weaponsManager;
	bool shellCreated;
	Camera mainCamera;
	Transform mainCameraTransform;
	headBob headBobManager;
	Animation weaponAnimation;
	bool weaponHasAnimation;

	void Start () {
		weaponsManager = GameObject.Find ("Player Controller").GetComponent<playerWeaponsManager> ();
		if (weaponsManager.checkIfWeaponAvaliable (weaponSettings.Name)) {
			weaponsEffectsSource = GetComponent<AudioSource> ();
			weaponSettings.ammoPerClip = weaponSettings.clipSize;
			originalParent = transform.parent;
			IKWeaponManager = originalParent.GetComponent<IKWeaponSystem> ();
			weaponSettings.weapon.transform.SetParent (weaponSettings.weaponParent);
			IKWeaponManager.weaponInfo.keepPosition.transform.SetParent (weaponSettings.weaponParent);
			enableHUD (false);
			mainCamera = Camera.main;
			mainCameraTransform = mainCamera.transform;
			headBobManager = mainCameraTransform.GetComponent<headBob> ();
			weaponAnimation = GetComponent<Animation> ();
			if (weaponSettings.animation != "") {
				weaponHasAnimation = true;
				weaponAnimation [weaponSettings.animation].speed = weaponSettings.animationSpeed; 
			}
		}
	}
	void Update () {
		//if the amount of shells from the projectiles is higher than 0, check the time to remove then
		if (shells.Count > 0) {
			destroyShellsTimer += Time.deltaTime;
			if (destroyShellsTimer > 3) {
				for (int i=0; i<shells.Count; i++) {
					Destroy (shells [i]);
				}
				shells.Clear ();
				destroyShellsTimer = 0;
			}
		}
		if (aimingInThirdPerson || carryingWeaponInFirstPerson) {
//			for (j = 0; j < weaponSettings.projectilePosition.Count; j++) {
//				if (Physics.Raycast (mainCameraTransform.position, mainCameraTransform.TransformDirection (Vector3.forward), out hit, Mathf.Infinity, layer)) {
//					Debug.DrawLine (weaponSettings.projectilePosition [j].position, hit.point, Color.red, 0.01f);
//				}
//			}
			if (!shellCreated && ((weaponHasAnimation && animationForwardPlayed && !weaponAnimation.IsPlaying (weaponSettings.animation)) || !weaponHasAnimation)) {
				createShells ();
			}
			if (!reloading) {
				if (weaponHasAnimation) {
					if (weaponSettings.clipSize > 0) {
						if (animationForwardPlayed && !weaponAnimation.IsPlaying (weaponSettings.animation)) {
							animationForwardPlayed = false;
							animationBackPlayed = true;
							weaponAnimation [weaponSettings.animation].speed = -weaponSettings.animationSpeed; 
							weaponAnimation [weaponSettings.animation].time = weaponAnimation [weaponSettings.animation].length;
							weaponAnimation.Play (weaponSettings.animation);
						}
						if (animationBackPlayed && !weaponAnimation.IsPlaying (weaponSettings.animation)) {
							animationBackPlayed = false;
						}
					} else if (weaponSettings.remainAmmo > 0) {
						StartCoroutine (waitToReload (weaponSettings.reloadTime));
					}
				} 
			} 
		}
	}
	public void setWeaponCarryState(bool thirdPersonCarry, bool firstPersonCarry){
		carryingWeaponInThirdPerson = thirdPersonCarry;
		carryingWeaponInFirstPerson = firstPersonCarry;
	}
	public void setWeaponAimState(bool thirdPersonAim, bool firstPersonAim){
		aimingInThirdPerson = thirdPersonAim;
		aimingInFirstPerson = firstPersonAim;
		if ((aimingInThirdPerson || aimingInFirstPerson) && weaponSettings.clipSize == 0) {
			manualReload ();
		}
	}
	//fire the current weapon
	public void shootWeapon(bool isThirdPersonView){
		//if the weapon system is active and the clip size higher than 0
		if (weaponSettings.clipSize > 0) {
			//else, fire the current weapon according to the fire rate
			if (Time.time > lastShoot + weaponSettings.fireRate && ((!animationForwardPlayed && !animationBackPlayed && weaponHasAnimation) || !weaponHasAnimation)) {
				//camera shake
				if (weaponsManager.carryingWeaponInFirstPerson && IKWeaponManager.useShotShakeInFirstPerson) {
					headBobManager.setShotShakeState (IKWeaponManager.firstPersonshotShakeInfo);
				}
				if (weaponsManager.carryingWeaponInThirdPerson && IKWeaponManager.useShotShakeInThirdPerson) {
					headBobManager.setShotShakeState (IKWeaponManager.thirdPersonshotShakeInfo);
				}
				//recoil
				IKWeaponManager.startRecoil (isThirdPersonView);
				IKWeaponManager.setLastTimeMoved ();
				//play the fire sound
				playWeaponSoundEffect (true);
				//create the muzzle flash
				createMuzzleFlash ();
				bool weaponCrossingSurface = false;
				if (!isThirdPersonView) {
					RaycastHit hitCamera, hitWeapon;
					if (Physics.Raycast (mainCameraTransform.position, mainCameraTransform.TransformDirection (Vector3.forward), out hitCamera, Mathf.Infinity, layer)
						&& Physics.Raycast (weaponSettings.projectilePosition [0].position, mainCameraTransform.TransformDirection (Vector3.forward), out hitWeapon, Mathf.Infinity, layer)) {
						if (hitCamera.collider != hitWeapon.collider) {
							//print ("too close surface");
							weaponCrossingSurface = true;
						} 
					}
				}
				//play the fire animation
				if (weaponSettings.weapon) {
					if (weaponHasAnimation) {
						weaponAnimation [weaponSettings.animation].speed = weaponSettings.animationSpeed;
						weaponAnimation.Play (weaponSettings.animation);
						animationForwardPlayed = true;
						if (weaponSettings.cockSound) {
							weaponsEffectsSource.PlayOneShot (weaponSettings.cockSound);
						}
					} 
					shellCreated = false;
				}
				//every weapon can shoot 1 or more projectiles at the same time, so for every projectile position to instantiate
				for (j = 0; j < weaponSettings.projectilePosition.Count; j++) {
					for (int l = 0; l < weaponSettings.projectilesPerShoot; l++) {
						//create the projectile
						GameObject projectile = (GameObject)Instantiate (weaponProjectile, weaponSettings.projectilePosition [j].position, weaponSettings.projectilePosition [j].rotation);
						//set its direction in the weapon forward or the camera forward according to if the weapon is aimed correctly or not
						if (!weaponCrossingSurface) {
							if (Physics.Raycast (mainCameraTransform.position, mainCameraTransform.TransformDirection (Vector3.forward), out hit, Mathf.Infinity, layer) && !weaponSettings.fireWeaponForward) {
								if (!hit.collider.isTrigger) {
									//Debug.DrawLine (weaponSettings.projectilePosition [j].position, hit.point, Color.red, 2);
									projectile.transform.LookAt (hit.point);
								}
							}
						}
						//add spread to the projectile
						Vector3 spreadAmount = Vector3.zero;
						if (weaponSettings.useProjectileSpread) {
							spreadAmount = setProjectileSpread ();
							projectile.transform.Rotate (spreadAmount);
						}
						//set the info in the projectile, like the damage, the type of projectile, bullet or missile, etc...
						projectile.GetComponent<playerWeaponBullet> ().getWeaponInfo (weaponSettings.projectileDamage, weaponSettings.projectileSpeed, 
							weaponSettings.particles, weaponSettings.projectileSoundEffect, character, weaponSettings.projectileForce, weaponSettings.scorch);
						if (weaponSettings.isExplosive) {
							projectile.GetComponent<playerWeaponBullet> ().getGrenadeInfo (weaponSettings.explosionForce, weaponSettings.explosionRadius);
						}
						//if the weapon shoots setting directly the projectile in the hit point, place the current projectile in the hit point position
						if (weaponSettings.useRayCastShoot || weaponCrossingSurface) {
							Vector3 forwardDirection = mainCameraTransform.TransformDirection (Vector3.forward);
							Vector3 forwardPositon = mainCameraTransform.position;
							if (weaponSettings.fireWeaponForward && !weaponCrossingSurface) {
								forwardDirection = weaponSettings.weapon.transform.forward;
								forwardPositon = weaponSettings.projectilePosition [j].position;
							}
							if (spreadAmount.magnitude != 0) {
								forwardDirection = Quaternion.Euler (spreadAmount) * forwardDirection;
							}
							if (Physics.Raycast (forwardPositon, forwardDirection, out hit, Mathf.Infinity, layer)) {
								projectile.GetComponent<playerWeaponBullet> ().rayCastShoot (hit.collider, hit.point);
								//print ("same object fired: " + hit.collider.name);
							}
						}
					}
					useAmmo ();
					lastShoot = Time.time;
					destroyShellsTimer = 0;
				}
			}
		} 
		//else, the clip in the weapon is over, so check if there is remaining ammo
		else {
			if (weaponSettings.remainAmmo == 0) {
				playWeaponSoundEffect (false);
			}
//			//if the weapon is not being reloaded, do it
//			if (!reloading) {
//				StartCoroutine (waitToReload (weaponSettings.reloadTime));
//			}
		}
	}
	public Vector3 setProjectileSpread(){
		float spreadAmount = 0;
		if (carryingWeaponInFirstPerson) {
			spreadAmount = weaponSettings.spreadAmount;
		}
		if (carryingWeaponInThirdPerson) {
			if (weaponSettings.sameSpreadInThirdPerson) {
				spreadAmount = weaponSettings.spreadAmount;
			} else {
				spreadAmount = weaponSettings.thirdPersonSpreadAmount;
			}
		}
		if (aimingInFirstPerson) {
			//print ("aiming");
			if (weaponSettings.useSpreadAming) {
				if (weaponSettings.useLowerSpreadAiming) {
					spreadAmount = weaponSettings.lowerSpreadAmount;
					//print ("lower spread");
				} else {
					//print ("same spread");
				}
			} else {
				//print ("no spread");
				spreadAmount = 0;
			}
		} else {
			//print ("no aiming");
		}
		if (spreadAmount > 0) {
			Vector3 randomSpread = Vector3.zero;
			randomSpread.x = Random.Range (-spreadAmount, spreadAmount);
			randomSpread.y = Random.Range (-spreadAmount, spreadAmount);
			randomSpread.z = Random.Range (-spreadAmount, spreadAmount);
			return randomSpread;
		}
		return Vector3.zero;
	}
	void createShells(){
		for (j = 0; j < weaponSettings.shellPosition.Count; j++) {
			//if the current weapon drops shells, create them
			if (weaponSettings.shell) {
				GameObject shellClone = (GameObject)Instantiate (weaponSettings.shell, weaponSettings.shellPosition [j].position, weaponSettings.shellPosition [j].rotation);
				shellClone.GetComponent<Rigidbody> ().AddForce (weaponSettings.shellPosition [j].right * weaponSettings.shellEjectionForce);
				Physics.IgnoreCollision (weaponsManager.gameObject.GetComponent<Collider> (), shellClone.transform.GetChild (0).GetComponent<Collider> ());
				if (weaponSettings.shellDropSoundList.Count > 0) {
					shellClone.GetComponent<AudioSource> ().clip = weaponSettings.shellDropSoundList [Random.Range (0, weaponSettings.shellDropSoundList.Count - 1)];
				}
				shells.Add (shellClone);
				if (shells.Count > 15) {
					GameObject shellToRemove = shells [0];
					shells.RemoveAt (0);
					Destroy (shellToRemove);
				}
				shellCreated = true;
			}
		}
	}
	//play the fire sound or the empty clip sound
	void playWeaponSoundEffect(bool hasAmmo){
		if (hasAmmo) {
			if (weaponSettings.soundEffect) {
				weaponsEffectsSource.clip = weaponSettings.soundEffect;
				weaponsEffectsSource.Play ();
			}
		} else {
			if (Time.time > lastShoot + weaponSettings.fireRate) {
				weaponsEffectsSource.PlayOneShot (outOfAmmo);
				lastShoot = Time.time;
			}
		}
	}
	//create the muzzle flash particles if the weapon has it
	void createMuzzleFlash (){
		if (weaponSettings.muzzleParticles) {
			for (j = 0; j < weaponSettings.projectilePosition.Count; j++) {
				GameObject muzzleParticlesClone = (GameObject)Instantiate (weaponSettings.muzzleParticles, weaponSettings.projectilePosition[j].position, weaponSettings.projectilePosition[j].rotation);
				Destroy (muzzleParticlesClone, 1);	
				muzzleParticlesClone.transform.SetParent (weaponSettings.projectilePosition [j]);
				weaponSettings.muzzleParticles.GetComponent<ParticleSystem> ().Play ();
			}
		}
	}
//	//decrease the amount of ammo in the clip
	void useAmmo(){
		weaponSettings.clipSize--;
		updateAmmoInfo ();
		//update hud ammo info
		weaponsManager.updateAmmo();
	}
	void updateAmmoInfo(){
		if(weaponSettings.HUD){
			weaponSettings.clipSizeText.text = weaponSettings.clipSize.ToString ();
			if (!weaponSettings.infiniteAmmo) {
				weaponSettings.remainAmmoText.text = weaponSettings.remainAmmo.ToString ();
			} else {
				weaponSettings.remainAmmoText.text = "Inf";
			}
		}
	}
	//check the amount of ammo
	void checkRemainAmmo(){
		//if the weaopn has not infinite ammo
		if (!weaponSettings.infiniteAmmo) {
			//the clip is empty
			if (weaponSettings.clipSize == 0) {
				//if the remaining ammo is lower that the ammo per clip, set the final projectiles in the clip 
				if (weaponSettings.remainAmmo < weaponSettings.ammoPerClip) {
					weaponSettings.clipSize = weaponSettings.remainAmmo;
				} 
				//else, refill it
				else {
					weaponSettings.clipSize = weaponSettings.ammoPerClip;
				}
				//if the remaining ammo is higher than 0, remove the current projectiles added in the clip
				if (weaponSettings.remainAmmo > 0) {
					weaponSettings.remainAmmo -= weaponSettings.clipSize;
				} 
			} 
			//the clip has some bullets in it yet
			else {
				int usedAmmo = 0;
				if (weaponSettings.remainAmmo < (weaponSettings.ammoPerClip - weaponSettings.clipSize)) {
					usedAmmo = weaponSettings.remainAmmo;
				} else {
					usedAmmo = weaponSettings.ammoPerClip - weaponSettings.clipSize;
				}
				weaponSettings.remainAmmo -= usedAmmo;
				weaponSettings.clipSize +=usedAmmo;
			}
		} else {
			//else, the weapon has infinite ammo, so refill it
			weaponSettings.clipSize = weaponSettings.ammoPerClip;
		}
		updateAmmoInfo ();
		weaponsManager.updateAmmo ();
	}
	//a delay for reload the weapon
	IEnumerator waitToReload(float amount){
		//print ("reload");
		//if the remmaining ammo is higher than 0 or infinite
		if(weaponSettings.remainAmmo > 0 || weaponSettings.infiniteAmmo){
			//reload
			reloading = true;
			//play the reload sound
			if (weaponSettings.reloadSoundEffect) {
				weaponsEffectsSource.PlayOneShot (weaponSettings.reloadSoundEffect);
			}
			//wait an amount of time
			yield return new WaitForSeconds (amount);
			//check the ammo values
			checkRemainAmmo ();
			//stop reload
			reloading = false;
		}
		else{
			//else, the ammo is over, play the empty weapon sound
			playWeaponSoundEffect (false);
		}
		yield return null;
	}
	public void manualReload(){
		if (!reloading) {
			if (weaponSettings.clipSize < weaponSettings.ammoPerClip) {
				StartCoroutine (waitToReload (0));
			}
		}
	}
	public void enableHUD(bool state){
		if (weaponSettings.HUD && weaponSettings.useHUD) {
			weaponSettings.HUD.SetActive (state);
			updateAmmoInfo ();
		}
	}
	public void changeHUDPosition(bool thirdPerson){
		if (weaponSettings.HUD && weaponSettings.useHUD) {
			if (weaponSettings.changeHUDPosition) {
				if (thirdPerson) {
					weaponSettings.ammoInfoHUD.transform.position = weaponSettings.HUDTransformInThirdPerson.position;
					//weaponSettings.HUD.transform.SetParent (weaponSettings.HUDTransformInThirdPerson);
				} else {
					weaponSettings.ammoInfoHUD.transform.position = weaponSettings.HUDTransformInFirstPerson.position;
					//weaponSettings.HUD.transform.SetParent (weaponSettings.HUDTransformInFirstPerson);
				}
				//weaponSettings.ammoInfoHUD.transform.localPosition = Vector3.zero;
			}
		}
	}
	//the vehicle has used an ammo pickup, so increase the correct weapon by name
	public void getAmmo(int amount){
		bool empty = false;
		if (weaponSettings.remainAmmo == 0 && weaponSettings.clipSize == 0) {
			empty = true;
		}
		weaponSettings.remainAmmo += amount;
		if (empty && (carryingWeaponInFirstPerson || aimingInThirdPerson)) {
			manualReload ();
		}
		updateAmmoInfo ();
	}
	public Transform getWeaponParent(){
		return originalParent;
	}

	public void setCharacter(GameObject obj){
		character = obj;
		#if UNITY_EDITOR
		EditorUtility.SetDirty (GetComponent<playerWeaponSystem> ());
		#endif
	}

	[System.Serializable]
	public class weaponInfo{
		public string Name;
		public int numberKey;
		public bool useRayCastShoot;
		public bool fireWeaponForward;
		public bool infiniteAmmo;
		public bool automatic;
		public int clipSize;
		public int remainAmmo;
		public float fireRate;
		public float reloadTime;
		public float projectileDamage;
		public float projectileSpeed;
		public float projectileForce=20;
		public int projectilesPerShoot;
		public bool useProjectileSpread;
		public float spreadAmount;
		public bool sameSpreadInThirdPerson;
		public float thirdPersonSpreadAmount;
		public bool useSpreadAming;
		public bool useLowerSpreadAiming;
		public float lowerSpreadAmount;
		public bool isExplosive;
		public float explosionForce;
		public float explosionRadius;
		public List<Transform> projectilePosition =new List<Transform>();
		public GameObject shell;
		public List<Transform> shellPosition =new List<Transform>();
		public float shellEjectionForce = 100;
		public GameObject weapon;
		public GameObject weaponMesh;
		public Transform weaponParent;
		public string animation;
		public float animationSpeed=1;
		public GameObject scorch;
		public GameObject particles;
		public GameObject muzzleParticles;
		public AudioClip soundEffect;
		public AudioClip projectileSoundEffect;
		public AudioClip reloadSoundEffect;
		public AudioClip cockSound;
		public List<AudioClip> shellDropSoundList =new List<AudioClip>();
		public Text clipSizeText;
		public Text remainAmmoText;
		public GameObject HUD;
		public GameObject ammoInfoHUD;
		public bool useHUD;
		public bool changeHUDPosition;
		public bool disableHUDInFirstPersonAim;
		public Transform HUDTransformInThirdPerson;
		public Transform HUDTransformInFirstPerson;
		public int ammoPerClip;
	}
}