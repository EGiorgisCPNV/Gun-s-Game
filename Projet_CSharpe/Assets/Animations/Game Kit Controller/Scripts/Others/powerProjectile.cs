using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class powerProjectile : MonoBehaviour {
	public float disableTimer=5;
	public float stopBulletTimer=1;
	public LayerMask layer;
	public GameObject bulletMesh;
	public particlesSettings particles = new particlesSettings ();
	List<Collider> colliders = new List<Collider> ();
	otherPowers.Powers currentPower;
	GameObject projectile;
	GameObject objectToDamage;
	GameObject player;
	GameObject shield;
	GameObject enemy;
	GameObject scorch;
	bool touched;
	bool stopBullet;
	bool homingProjectile;
	bool projectileUsed;
	bool firedByRayCast;
	float projectileSpeed;
	float blackHoleTimer=-1;
	float projectileDamage;
	RaycastHit hit;
	Rigidbody mainRigidbody; 
	string powerName;
	AudioClip impactSound;

	void Start(){
		startBullet ();
	}
	void Update () {
		//if the bullet touchs a surface, then check the power selected when it was fired
		if (touched) {
			switch (powerName) {
			case "Regular Shoot":
				//regular bullet
				//add velocity if the touched object has rigidbody
				applyDamage.checkHealth (projectile, objectToDamage, projectileDamage, -transform.forward, transform.position, player, false);
				if (applyDamage.canApplyForce (objectToDamage)) {
					Vector3 force = transform.forward * 20;
					objectToDamage.GetComponent<Rigidbody> ().AddForce (force * objectToDamage.GetComponent<Rigidbody> ().mass, ForceMode.Impulse);
				}
				disableBullet (0.5f);
				break;
			case "Bomb":
				//grenade
				//get all the objects inside a radius in the impact position, applying to them an explosion force
				if (colliders.Count == 0) {
					colliders.AddRange (Physics.OverlapSphere (transform.position, 15, layer));
					particles.particles [0].SetActive (true);
					foreach (Collider hit in colliders) {
						if (hit != null) {
							if (hit.gameObject.tag != "Player" && hit.gameObject.tag != "bullet") {
								applyDamage.checkHealth (projectile, hit.gameObject, projectileDamage, -transform.forward, hit.gameObject.transform.position, player, false);
								if (applyDamage.canApplyForce (hit.gameObject)) {
									hit.GetComponent<Rigidbody> ().AddExplosionForce (800, transform.position, 20, 3);
								}
							}
						}
					}
				}
				disableBullet (1.8f);
				break;
			case "Implosion Grenade":
				//Implosion grenade
				//get all the objects inside a radius in the impact position, applying to them an implosion force
				if (colliders.Count == 0) {
					colliders.AddRange (Physics.OverlapSphere (transform.position, 30, layer));
					particles.particles [1].SetActive (true);
				}
				if (colliders.Count > 0) {
					foreach (Collider hit in colliders) {
						if (hit != null) {
							if (hit.gameObject.tag != "Player" && hit.gameObject.tag != "bullet") {
								if (hit.GetComponent<Rigidbody> ()) {
									if (!hit.GetComponent<Rigidbody> ().isKinematic) {
										Vector3 Dir = transform.position - hit.gameObject.transform.position;
										Vector3 Dirscale = Vector3.Scale (Dir.normalized, gameObject.transform.localScale);
										hit.GetComponent<Rigidbody> ().AddForce (Dirscale * 1750 * hit.GetComponent<Rigidbody> ().mass, ForceMode.Acceleration);
									}
								}
								applyDamage.checkHealth (projectile, hit.gameObject, projectileDamage, -transform.forward, hit.gameObject.transform.position, player, false);
							}
						}
					}
				}
				disableBullet (1.8f);
				break;
			case "Push Objects":
				//this powers push objects, but none bullet is fired
				break;
			case "Black Hole":
				//black hole
				//when the bullet touchs a surface, or the timer reachs the limit, set the bullet kinematic, and activate the black hole
				if (blackHoleTimer > 0.5 && mainRigidbody.isKinematic) {
					particles.particles [2].SetActive (true);
					//get all the objects inside a radius
					if (colliders.Count == 0) {
						colliders.AddRange (Physics.OverlapSphere (transform.position, 40, layer));
						foreach (Collider hit in colliders) {
							if (hit != null) {
								if (hit.gameObject.tag != "Player" && hit.gameObject.tag != "bullet") {
									if (hit.GetComponent<Rigidbody> ()) {
										hit.GetComponent<Rigidbody> ().velocity = Vector3.zero;
									}
								}
							}
						}
					}
					foreach (Collider hit in colliders) {
						if (hit != null) {
							if (hit.gameObject.tag != "Player" && hit.gameObject.tag != "bullet") {
								if (hit.GetComponent<Rigidbody> ()) {
									//se the kinematic rigigbody of the enemies to false, to attract them
									if (hit.gameObject.tag == "enemy") {
										if (hit.GetComponent<Rigidbody> ().isKinematic) {
											hit.GetComponent<Rigidbody> ().isKinematic = false;
										}
										hit.gameObject.SendMessage ("pauseAI", true);
									}
									//if the object distance to the black hole is higher than a certain amount, attrac to it
									//else stop its movement
									if (!hit.GetComponent<Rigidbody> ().isKinematic) {
										if (Vector3.Distance (transform.position, hit.gameObject.transform.position) < 4) {
											hit.GetComponent<Rigidbody> ().velocity = Vector3.zero;
											hit.GetComponent<Rigidbody> ().useGravity = false;
										} else {
											Vector3 Dir = transform.position - hit.gameObject.transform.position;
											Vector3 Dirscale = Vector3.Scale (Dir.normalized, gameObject.transform.localScale);
											hit.GetComponent<Rigidbody> ().AddForce (Dirscale * 150 * hit.GetComponent<Rigidbody> ().mass, ForceMode.Acceleration);
										}
									}
								}
								applyDamage.checkHealth (projectile, hit.gameObject, projectileDamage, -transform.forward, hit.gameObject.transform.position, player, false);
							}
						}
					}
				}
				//activate the particles, they are activated and deactivated according to the timer value
				if (blackHoleTimer < 4 && !particles.particles [3].activeSelf) {
					particles.particles [3].SetActive (true);
				}
				if (blackHoleTimer < 3.5 && particles.particles [2].activeSelf) {
					particles.particles [2].SetActive (false);
				}
				//when the time is finishing, apply an explosion force to all the objects inside the black hole, and make an extra damage to all of them
				if (blackHoleTimer < 0.5 && mainRigidbody.isKinematic) {
					foreach (Collider hit in colliders) {
						if (hit != null) {
							if (hit.gameObject.tag != "Player" && hit.gameObject.tag != "bullet") {
								if (hit.GetComponent<Rigidbody> ()) {
									if (!hit.GetComponent<Rigidbody> ().isKinematic) {
										hit.GetComponent<Rigidbody> ().useGravity = true;
										hit.GetComponent<Rigidbody> ().AddExplosionForce (200, transform.position, 30, 3);	
									}
								}
								if (hit.gameObject.tag == "enemy") {
									hit.gameObject.SendMessage ("pauseAI", false);
								}
								applyDamage.checkHealth (projectile, hit.gameObject, projectileDamage * 10, -transform.forward, hit.gameObject.transform.position, player, false);
							}
						}
					}
				}
				break;
			case "Time Slower":
				//slow down enemies and a type of objects
				bool canbeSlowed = true;
				if (objectToDamage.GetComponent<characterDamageReceiver> ()) {
					objectToDamage = objectToDamage.GetComponent<characterDamageReceiver> ().character;
				} 
				if (!objectToDamage.GetComponent<slowObject> ()) {
					canbeSlowed = false;
				}
				if (canbeSlowed && !objectToDamage.GetComponent<slowObjectsColor> ()) {
					objectToDamage.AddComponent<slowObjectsColor> ().startSlowObject (player.GetComponent<otherPowers> ().shootsettings.slowObjectsColor, player.GetComponent<otherPowers> ().shootsettings.slowValue);
				}
				disableBullet (0.1f);
				break;
			case "Shut Down":
				//regular bullet
				//add velocity if the touched object has rigidbody
				applyDamage.checkHealth (projectile, objectToDamage, projectileDamage, -transform.forward, transform.position, player, false);
				disableBullet (1.8f);
				break;
			}
			//when a black hole bullet is shooted, if it does not touch anything in a certain amount of time, set it kinematic and open the black hole
			if (stopBullet) {
				stopBulletTimer -= Time.deltaTime;
				if (stopBulletTimer < 0) {
					stopBulletTimer = 1;
					mainRigidbody.isKinematic = true;
					mainRigidbody.useGravity = false;
					stopBullet = false;
				}
			}
			//destroy the black hole bullet
			if (blackHoleTimer > 0) {
				blackHoleTimer -= Time.deltaTime;
				if (blackHoleTimer < 0) {
					Destroy (projectile);
				}
			}
		} else if (homingProjectile && !mainRigidbody.isKinematic) {
			mainRigidbody.velocity = transform.forward * projectileSpeed;
			if (enemy) {
				Quaternion rotation = Quaternion.LookRotation (enemy.transform.position + enemy.transform.up - transform.position);
				transform.rotation = Quaternion.Slerp (transform.rotation, rotation, Time.deltaTime * projectileSpeed);
			}
		}
		//destroy the bullet 
		if (disableTimer > 0) {
			disableTimer -= Time.deltaTime;
			if (disableTimer < 0) {
				Destroy (projectile);
			}
		}
	}
	public void setEnemy(GameObject obj){
		StartCoroutine (missileWaiting (obj));
	}
	IEnumerator missileWaiting(GameObject obj){
		yield return new WaitForSeconds (0.3f);
		enemy = obj;
	}
	//set these bools when the bullet is a black hole 
	public void stopBlackHole(){
		stopBullet = true;
		touched = true;
	}
	//when the bullet touchs a surface, then
	void OnTriggerEnter(Collider col){
		if (!col.isTrigger && !projectileUsed && col.gameObject.layer != LayerMask.NameToLayer ("Ignore Raycast")) {
			if (impactSound) {
				GetComponent<AudioSource> ().PlayOneShot (impactSound);
			}
			projectileUsed = true;
			touched = true;
			//set the bullet kinematic
			objectToDamage = col.GetComponent<Collider> ().gameObject;
			if (!objectToDamage.GetComponent<Rigidbody> () && stopBullet) {
				mainRigidbody.isKinematic = true;
			}
			if (scorch) {
				//the bullet fired is a simple bullet or a greanade, check the hit point with a raycast to set in it a scorch
				if (powerName == "Regular Shoot"
				   || powerName == "Bomb"
				   || powerName == "Implosion Grenade") {
					if (Physics.Raycast (transform.position - transform.forward * 0.7f, transform.forward, out hit, 200, layer)) {
						decalManager.setScorch (transform.rotation, scorch, hit, objectToDamage);
					}
				}
			}
		}
	}
	void dropBlackHoleObjects(){
		for (int i = 0; i < colliders.Count; i++) {
			if (colliders [i].tag != "Player" && colliders [i].tag != "bullet") {
				if (colliders [i].GetComponent<Rigidbody> ()) {
					colliders [i].GetComponent<Rigidbody> ().useGravity = true;
					colliders [i].GetComponent<Rigidbody> ().AddExplosionForce (200, transform.position, 30, 3);	
				}
			}
		}
	}
	//destroy the bullet according to the time value
	void disableBullet(float time){
		mainRigidbody.isKinematic = true;
		touched = false;
		disableTimer = time;
		bulletMesh.SetActive (false);
	}
	public void setProjectileInfo(GameObject playerObj,GameObject shieldObj, otherPowers.Powers currentPowerUsed,float damage,AudioClip impactEffect, GameObject scorchPrefab, float speed){
		player = playerObj;
		shield = shieldObj;
		currentPower = currentPowerUsed;
		projectileDamage = damage;
		impactSound = impactEffect;
		scorch = scorchPrefab;
		projectileSpeed = speed;
	}
	//if the projectiles is placed directly in the raycast hit point, place the projectile in the correct position
	public void rayCastShoot(Collider surface,Vector3 position){
		transform.position = position;
		GetComponent<Collider> ().enabled = false;
		firedByRayCast = true;
		startBullet ();
		this.OnTriggerEnter (surface);
		//StartCoroutine (callTrigger (surface));
	}
	public void startBullet(){
		mainRigidbody = GetComponent<Rigidbody> ();
		projectile = gameObject;
		powerName = currentPower.Name;
		//ignore collision between the bullet and the player
		Physics.IgnoreCollision (player.GetComponent<Collider> (), GetComponent<Collider> ());
		if (shield) {
			Physics.IgnoreCollision (shield.GetComponentInChildren<Collider> (), GetComponent<Collider> ());
		}
		GetComponent<TrailRenderer> ().enabled = true;
		//the bullet moves on the camera direction
		if (!firedByRayCast) {
			mainRigidbody.velocity = transform.forward * projectileSpeed;
		}
		//check what type of bullet has been fired, to activate some parameters, particles, etc...
		switch (powerName) {
		case "Black Hole":
			//if the bullet type is a black hole, remove any other black hole in the scene and set the parameters in the bullet script 
			//the bullet with the black hole has activated the option useGravity in its rigidbody
			GameObject searchBlackHole = GameObject.FindGameObjectWithTag ("blackHole");
			if (searchBlackHole) {
				searchBlackHole.SendMessage ("dropBlackHoleObjects");
				Destroy (searchBlackHole);
			}
			mainRigidbody.useGravity = true;
			tag = "blackHole";
			stopBlackHole ();
			//the black hole bullet has another timer
			disableTimer = -1;
			blackHoleTimer = 10;
			GetComponent<SphereCollider> ().radius *= 5;
			GetComponent<TrailRenderer> ().startWidth = 4;
			GetComponent<TrailRenderer> ().time = 2;
			GetComponent<TrailRenderer> ().endWidth = 3;
			bulletMesh.SetActive (false);
			break;
		case "Time Slower":
			particles.particles [4].SetActive (true);
			break;
		case "Shut Down":
			disableTimer = 10;
			mainRigidbody.velocity = Vector3.zero;
			homingProjectile = true;
			break;
		}
	}
	//an array with every type of particle, according to the byllet type
	[System.Serializable]
	public class particlesSettings{
		public GameObject[] particles;
	}
}