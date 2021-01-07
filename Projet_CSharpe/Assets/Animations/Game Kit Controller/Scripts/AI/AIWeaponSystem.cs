using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class AIWeaponSystem : MonoBehaviour {
	public GameObject character;
	public weaponInfo weaponSettings;
	public AudioClip outOfAmmo;
	public GameObject weaponProjectile;
	public LayerMask layer;
	public bool reloading;
	public AIIKWeaponSystem IKWeaponManager;
	List<GameObject> shells=new List<GameObject>();
	float destroyShellsTimer=0;
	int i,j,k;
	RaycastHit hit;
	float lastShoot;
	AudioSource weaponsEffectsSource;
	bool animationForwardPlayed;
	bool animationBackPlayed;
	bool shellCreated;
	public bool aiming;
	float weaponSpeed = 1;
	float originalWeaponSpeed;
	Animation weaponAnimation;
	bool weaponHasAnimation;

	void Start () {
		weaponsEffectsSource = GetComponent<AudioSource> ();
		weaponSettings.ammoPerClip = weaponSettings.clipSize;
		originalWeaponSpeed = weaponSpeed;
		weaponAnimation = GetComponent<Animation> ();
		if (weaponSettings.animation != "") {
			weaponHasAnimation = true;
			weaponAnimation [weaponSettings.animation].speed = weaponSettings.animationSpeed; 
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
		if (aiming) {
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
	public void aimingWeapon(bool state){
		aiming = state;
		if (aiming && weaponSettings.clipSize == 0) {
			manualReload ();
		}
	}
	//fire the current weapon
	public void shootWeapon(){
		//if the weapon system is active and the clip size higher than 0
		if (weaponSettings.clipSize > 0) {
			//else, fire the current weapon according to the fire rate
			if (Time.time > lastShoot + weaponSettings.fireRate && ((!animationForwardPlayed && !animationBackPlayed && weaponHasAnimation) || !weaponHasAnimation)) {
				//recoil
				IKWeaponManager.startRecoil();
				//play the fire sound
				playWeaponSoundEffect (true);
				//create the muzzle flash
				createMuzzleFlash ();


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
						if (Physics.Raycast (IKWeaponManager.weaponInfo.aimPosition.position, transform.forward, out hit, Mathf.Infinity, layer) && !weaponSettings.fireWeaponForward) {
							if (!hit.collider.isTrigger) {
								//Debug.DrawLine (weaponSettings.projectilePosition [j].position, hit.point, Color.red, 2);
								projectile.transform.LookAt (hit.point);
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
						if (weaponSettings.useRayCastShoot) {
							Vector3 forwardDirection = transform.forward;
							Vector3 forwardPositon = IKWeaponManager.weaponInfo.aimPosition.position;
							if (weaponSettings.fireWeaponForward) {
								forwardDirection = weaponSettings.weapon.transform.forward;
								forwardPositon = weaponSettings.projectilePosition [j].position;
							}
							if (spreadAmount.magnitude != 0) {
								forwardDirection = Quaternion.Euler (spreadAmount) * forwardDirection;
							}
							if (Physics.Raycast (forwardPositon, forwardDirection, out hit, Mathf.Infinity, layer)) {
								projectile.GetComponent<playerWeaponBullet> ().rayCastShoot (hit.collider, hit.point);
								//print ("same object: " + hit.collider.name);

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
		float spreadAmount = weaponSettings.spreadAmount;
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
				Physics.IgnoreCollision (character.GetComponent<Collider> (), shellClone.transform.GetChild (0).GetComponent<Collider> ());
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
				weaponsEffectsSource.pitch = weaponSpeed;
				weaponsEffectsSource.Play ();
			}
		} else {
			if (Time.time > lastShoot + weaponSettings.fireRate) {
				weaponsEffectsSource.pitch = weaponSpeed;
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
				muzzleParticlesClone.transform.parent = weaponSettings.projectilePosition[j];
				weaponSettings.muzzleParticles.GetComponent<ParticleSystem> ().Play ();
			}
		}
	}
	//	//decrease the amount of ammo in the clip
	void useAmmo(){
		weaponSettings.clipSize--;
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
	}
	//a delay for reload the weapon
	IEnumerator waitToReload(float amount){
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
	//the vehicle has used an ammo pickup, so increase the correct weapon by name
	public void getAmmo(int amount){
		bool empty = false;
		if (weaponSettings.remainAmmo == 0 && weaponSettings.clipSize == 0) {
			empty = true;
		}
		weaponSettings.remainAmmo += amount;
		if (empty && aiming) {
			manualReload ();
		}
	}
	public void reduceVelocity(float multiplierValue){
		weaponSpeed *= multiplierValue;
	}
	public void normalVelocity(){
		weaponSpeed = originalWeaponSpeed;
	}
	[System.Serializable]
	public class weaponInfo{
		public string Name;
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
		public int ammoPerClip;
	}
}