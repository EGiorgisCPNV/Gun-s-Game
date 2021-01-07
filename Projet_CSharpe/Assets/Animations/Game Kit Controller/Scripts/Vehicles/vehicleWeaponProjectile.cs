using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class vehicleWeaponProjectile : MonoBehaviour {
	public float disableTimer=5;
	public LayerMask layer;
	public GameObject bulletMesh;
	public GameObject missileMesh;
	List<Collider> colliders=new List<Collider>();
	vehicleWeaponSystem.vehicleWeapons weapon;
	GameObject projectile;
	GameObject objectToDamage;
	GameObject player;
	GameObject enemy;
	GameObject projectileParticles;
	GameObject scorch;
	bool touched;
	bool isHomingProjectile;
	bool projectileUsed;
	bool isExplosive;
	bool exploded;
	bool firedByRayCast;
	float projectileForce;
	float projectileSpeed;
	float projectileDamage;
	float explosionForce;
	float explosionRadius;
	string weaponName;
	RaycastHit hit;
	AudioClip hitSound;
	Rigidbody mainRigidbody;
	Rigidbody collisionRigid;

	//this script is for the projectiles fired by the player
	void Start () {
		mainRigidbody = GetComponent<Rigidbody> ();
		projectile = gameObject;
		player=GameObject.Find("Player Controller");
		GetComponent<TrailRenderer>().enabled=true;
		//the bullet moves on the camera direction
		if (!firedByRayCast) {
			mainRigidbody.velocity = transform.forward * projectileSpeed;
		}
		weaponName = weapon.Name;
		if (isExplosive) {
			bulletMesh.SetActive (false);
			missileMesh.SetActive (true);
			GetComponent<TrailRenderer> ().endWidth *= 2;
			GetComponent<TrailRenderer> ().startWidth *= 2;
		}
		if(isHomingProjectile){
			disableTimer=10;
			mainRigidbody.velocity=Vector3.zero;
		}
	}

	void Update () {
		//if the bullet touchs a surface, then check the power selected when it was fired
		if (touched) {
			switch (weaponName) {
			case "Machine Gun":
				//regular bullet
				//add velocity if the touched object has rigidbody
				if (applyDamage.canApplyForce (objectToDamage)) {
					Vector3 force = transform.forward * projectileForce;
					collisionRigid.AddForce (force * collisionRigid.mass, ForceMode.Impulse);
				}
				applyDamage.checkHealth (projectile, objectToDamage, projectileDamage, -transform.forward, transform.position, player, false);
				disableBullet (0.5f);
				break;
			case "Cannon":
				//grenade
				//get all the objects inside a radius in the impact position, applying to them an explosion force
				if (colliders.Count == 0) {
					colliders.AddRange (Physics.OverlapSphere (transform.position, explosionRadius, layer));
					foreach (Collider hit in colliders) {
						if (hit != null) {
							if (hit.gameObject.tag != "Player" && hit.gameObject.tag != "bullet") {
								if (hit.GetComponent<Rigidbody> ()) {
									if (!hit.GetComponent<Rigidbody> ().isKinematic) {
										hit.GetComponent<Rigidbody> ().AddExplosionForce (explosionForce, transform.position, explosionRadius, 3, ForceMode.Impulse);
									}
								}
								applyDamage.checkHealth (projectile, hit.gameObject, projectileDamage, -transform.forward, hit.gameObject.transform.position, player, false);
							}
						}
					}
				}
				disableBullet (1.8f);
				break;
			case "Homming Missile":
				if (objectToDamage) {
					applyDamage.checkHealth (projectile, objectToDamage, projectileDamage, -transform.forward, transform.position, player, false);
				}
				disableBullet (1.8f);
				break;
			case "Implosion Grenade":
				//Implosion grenade
				//get all the objects inside a radius in the impact position, applying to them an implosion force
				if (colliders.Count == 0) {
					colliders.AddRange (Physics.OverlapSphere (transform.position, explosionRadius, layer));
				}
				if (colliders.Count > 0) {
					foreach (Collider hit in colliders) {
						if (hit != null) {
							if (hit.gameObject.tag != "Player" && hit.gameObject.tag != "bullet") {
								if (hit.GetComponent<Rigidbody> ()) {
									if (!hit.GetComponent<Rigidbody> ().isKinematic) {
										Vector3 Dir = transform.position - hit.gameObject.transform.position;
										Vector3 Dirscale = Vector3.Scale (Dir.normalized, gameObject.transform.localScale);
										hit.GetComponent<Rigidbody> ().AddForce (Dirscale * explosionForce * hit.GetComponent<Rigidbody> ().mass, ForceMode.Acceleration);
									}
								}
								applyDamage.checkHealth (projectile, hit.gameObject, projectileDamage, -transform.forward, hit.gameObject.transform.position, player, false);
							}
						}
					}
				}
				disableBullet (1.8f);
				break;
			case "Double Machine Gun":
				//add velocity if the touched object has rigidbody
				if (collisionRigid) {
					Vector3 force = transform.forward * projectileForce;
					collisionRigid.AddForce (force * collisionRigid.mass, ForceMode.Impulse);
				}
				applyDamage.checkHealth (projectile, objectToDamage, projectileDamage, -transform.forward, transform.position, player, false);
				disableBullet (0.5f);
				break;
			case "Seeker Missiles":
				//get all the objects inside a radius in the impact position, applying to them an implosion force
				if (colliders.Count == 0) {
					colliders.AddRange (Physics.OverlapSphere (transform.position, explosionRadius, layer));
					foreach (Collider hit in colliders) {
						if (hit != null) {
							if (hit.gameObject.tag != "Player" && hit.gameObject.tag != "bullet") {
								if (hit.GetComponent<Rigidbody> ()) {
									if (!hit.GetComponent<Rigidbody> ().isKinematic) {
										hit.GetComponent<Rigidbody> ().AddExplosionForce (explosionForce, transform.position, explosionRadius, 3);
									}
								}
								applyDamage.checkHealth (projectile, hit.gameObject, projectileDamage, -transform.forward, hit.gameObject.transform.position, player, false);
							}
						}
					}
				}
				disableBullet (1.8f);
				break;
			case "Shotgun":
				//regular bullet
				//add velocity if the touched object has rigidbody
				if (applyDamage.canApplyForce (objectToDamage)) {
					Vector3 force = transform.forward * projectileForce;
					collisionRigid.AddForce (force * collisionRigid.mass, ForceMode.Impulse);
				}
				applyDamage.checkHealth (projectile, objectToDamage, projectileDamage, -transform.forward, transform.position, player, false);
				disableBullet (0.5f);
				break;
			}
		} 
		//if the current projectile is a homming or a seeker missile, apply velocity in the missile forward and rotate it to the target
		else if (isHomingProjectile && !mainRigidbody.isKinematic) {
			mainRigidbody.velocity = transform.forward * projectileSpeed;
			if (enemy) {
				Quaternion rotation = Quaternion.LookRotation (enemy.transform.position + enemy.transform.up - transform.position);
				transform.rotation = Quaternion.Slerp (transform.rotation, rotation, Time.deltaTime * projectileSpeed);
			}
		}
		//destroy the bullet when the time is over
		if (disableTimer > 0) {
			disableTimer -= Time.deltaTime;
			if (disableTimer < 0) {
				if (isExplosive && !touched && !exploded) {
					explodeMissile ();
					return;
				}
				Destroy (gameObject);
			}
		}
	}
	//when the bullet touchs a surface, then
	void OnTriggerEnter(Collider col){
		//compare if the layer of the hitted object is not in the layer configured in the inspector
//		if((1<<col.gameObject.layer & layer.value)!=1<<col.gameObject.layer){
//			//print (LayerMask.LayerToName (col.gameObject.layer));
//		}
		//if the layer of the object found is in the layers list, then
		if (!col.isTrigger && !projectileUsed && (1 << col.gameObject.layer & layer.value) == 1 << col.gameObject.layer) {
			projectileUsed = true;
			touched = true;
			//set the bullet kinematic
			objectToDamage = col.GetComponent<Collider> ().gameObject;
			if (objectToDamage.GetComponent<Rigidbody> ()) {
				collisionRigid = objectToDamage.GetComponent<Rigidbody> ();
			}
			//the bullet fired is a simple bullet or a greanade, check the hit point with a raycast to set in it a scorch
			if (scorch) {
				if (weaponName == "Machine Gun"
				   || weaponName == "Cannon"
				   || weaponName == "Homming Missile"
				   || weaponName == "Implosion Grenade"
				   || weaponName == "Double Machine Gun"
				   || weaponName == "Shotgun") {
					gameObject.layer = LayerMask.NameToLayer ("Ignore Raycast");
					Debug.DrawLine (transform.position, transform.position - transform.forward * 100, Color.red);
					if (Physics.Raycast (transform.position - transform.forward * 2, transform.forward, out hit, 10, layer)) {
						decalManager.setScorch (transform.rotation, scorch, hit, objectToDamage);
					}
				}
			}
			//set the projectile particles when the collision happens
			if (projectileParticles) {
				GameObject particlesClone = (GameObject)Instantiate (projectileParticles, transform.position, transform.rotation);
				particlesClone.transform.SetParent (transform);
			}
			if (hitSound) {
				GetComponent<AudioSource> ().PlayOneShot (hitSound);
			}
		}
	}
	//if the time to disable the projectile is over, and the projectile is a missile, explode it in the air, checking any possible collision in its radius
	void explodeMissile(){
		if (projectileParticles) {
			GameObject particlesClone = (GameObject)Instantiate (projectileParticles, transform.position, transform.rotation);
			particlesClone.transform.SetParent (transform);
		}
		if (hitSound) {
			GetComponent<AudioSource> ().PlayOneShot (hitSound);
		}
		if (colliders.Count == 0) {
			//get all the objects inside a radius in the impact position, applying to them an implosion force, and checking for health components
			colliders.AddRange (Physics.OverlapSphere (transform.position, explosionRadius, layer));
			foreach (Collider hit in colliders) {
				if (hit != null) {
					if (hit.gameObject.tag != "Player" && hit.gameObject.tag != "bullet") {
						if (hit.GetComponent<Rigidbody> ()) {
							if (!hit.GetComponent<Collider> ().gameObject.GetComponent<Rigidbody> ().isKinematic) {
								if (weaponName != "Implosion Grenade") {
									hit.GetComponent<Rigidbody> ().AddExplosionForce (explosionForce, transform.position, explosionRadius, 3);
								} else {
									Vector3 Dir = transform.position - hit.gameObject.transform.position;
									Vector3 Dirscale = Vector3.Scale (Dir.normalized, gameObject.transform.localScale);
									hit.GetComponent<Rigidbody> ().AddForce (Dirscale * explosionForce * hit.GetComponent<Rigidbody> ().mass, ForceMode.Acceleration);
								}
							}
						}
						applyDamage.checkHealth (projectile, hit.gameObject, projectileDamage, -transform.forward, hit.gameObject.transform.position, player, false);
					}
				}
			}
		}
		exploded = true;
		disableBullet (1.8f);
	}
	//destroy the bullet according to the time value
	void disableBullet(float time){
		mainRigidbody.isKinematic = true;
		touched = false;
		disableTimer = time;
		bulletMesh.SetActive(false);
		missileMesh.SetActive (false);
		exploded = true;
	}
	//if the projectiles is placed directly in the raycast hit point, place the projectile in the correct position
	public void rayCastShoot(Collider surface,Vector3 position){
		transform.position = position;
		GetComponent<Collider> ().enabled = false;
		firedByRayCast = true;
		StartCoroutine (callTrigger (surface));
	}
	//cal the on trigger enter function when the projectile uses a raycastshoot
	IEnumerator callTrigger(Collider surface){
		yield return new WaitForSeconds(0.1f);
		this.OnTriggerEnter (surface);
	}
	//set the enemy as the target for a missile
	public void setEnemy(GameObject obj){
		StartCoroutine (missileWaiting (obj));
	}
	//wait some seconds to set the enemy target, so the missile moves in its forward direction somes seconds
	IEnumerator missileWaiting(GameObject obj){
		yield return new WaitForSeconds (0.3f);
		enemy = obj;
	}
	//get the info of the current weapon selected, so the projectile has the correct behaviour
	public void getWeaponInfo(vehicleWeaponSystem.vehicleWeapons currentWeapon, float damage, GameObject particles, AudioClip soundEffect, float speed, 
		float force, float explosionForceAmount, float explosionRadiusAmount, bool isExplosiveValue, bool isHomingValue, GameObject scorchPrefab){
		weapon = currentWeapon;
		projectileDamage = damage;
		if (particles) {
			projectileParticles = particles;
		}
		if (soundEffect) {
			hitSound = soundEffect;
		}
		projectileSpeed = speed;
		projectileForce = force;
		explosionForce = explosionForceAmount;
		explosionRadius = explosionRadiusAmount;
		isExplosive = isExplosiveValue;
		isHomingProjectile = isHomingValue;
		scorch = scorchPrefab;
	}
	//draw an sphere to show the damage radius
	void OnDrawGizmosSelected() {
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, explosionRadius);
	}
}
