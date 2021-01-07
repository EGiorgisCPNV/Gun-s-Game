using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class playerWeaponBullet : MonoBehaviour {
	public float disableTimer=5;
	public LayerMask layer;
	public GameObject bulletMesh;
	List<Collider> colliders = new List<Collider> ();
	GameObject projectileParticles;
	GameObject bulletOwner;
	GameObject collision;
	GameObject scorch;
	bool projectileUsed;
	bool firedByRayCast;
	bool isRegularProjectile;
	bool isExplosive;
	float projectileDamage;
	float speed;
	float projectileForce;
	float explosionForce;
	float explosionRadius;
	RaycastHit hit;
	AudioClip hitSound;
	Rigidbody mainRigidbody;

	void Update () {
		if (disableTimer > 0) {
			disableTimer -= Time.deltaTime;
			if (disableTimer < 0) {
				Destroy (gameObject);
			}
		}
	}
	//when the bullet touchs a surface, then
	void OnTriggerEnter(Collider col){
		//if the layer of the object found is in the layers list, then
		//!col.isTrigger && 
		if (!projectileUsed && (1 << col.gameObject.layer & layer.value) == 1 << col.gameObject.layer) {
			projectileUsed = true;
			//set the bullet kinematic
			collision = col.GetComponent<Collider> ().gameObject;
			//the bullet fired is a simple bullet or a greanade, check the hit point with a raycast to set in it a scorch
			Debug.DrawLine (transform.position, transform.position - transform.forward * 100, Color.red);
			if (scorch) {
				if (Physics.Raycast (transform.position - transform.forward * 2, transform.forward, out hit, 10, layer)) {
					decalManager.setScorch (transform.rotation, scorch, hit, collision);
				}
			}
			//set the projectile particles when the collision happens
			if (projectileParticles) {
				Instantiate (projectileParticles, hit.point, transform.rotation);
			}
			if (hitSound) {
				GetComponent<AudioSource> ().PlayOneShot (hitSound);
			}
			applyDamage.checkHealth (gameObject, collision, projectileDamage, -transform.forward, transform.position, bulletOwner, false);
			disableBullet (hitSound.length);

			if (isRegularProjectile) {
				bool canReceiveForce = false;
				if (collision.GetComponent<Rigidbody> ()) {
					canReceiveForce = true;
					if (collision.GetComponent<characterDamageReceiver> ()) {
						if (collision.GetComponent<characterDamageReceiver> ().character.tag != "Player" ||
						    collision.GetComponent<characterDamageReceiver> ().character.tag != "enemy" ||
						    collision.GetComponent<characterDamageReceiver> ().character.tag != "friend") {
							canReceiveForce = false;
						}
					}
				}
				if (canReceiveForce) {
					Vector3 force = transform.forward * projectileForce;
					collision.GetComponent<Rigidbody> ().AddForce (force * collision.GetComponent<Rigidbody> ().mass, ForceMode.Impulse);
				}
			}
			if (isExplosive) {
				activateExplosion ();
			}
		}
	}
	public void activateExplosion(){
		if (colliders.Count == 0) {
			colliders.AddRange (Physics.OverlapSphere (transform.position, explosionRadius, layer));
			foreach (Collider hit in colliders) {
				if (hit != null) {
					if (hit.gameObject.tag != "Player" && hit.gameObject.tag != "enemy" && hit.gameObject.tag != "friend") {
						applyDamage.checkHealth (gameObject, hit.gameObject, projectileDamage, -transform.forward, hit.gameObject.transform.position, bulletOwner, false);
						if (applyDamage.canApplyForce (hit.gameObject)) {
							hit.GetComponent<Rigidbody> ().AddExplosionForce (explosionForce, transform.position, explosionRadius, 3, ForceMode.Impulse);
						}
					}
				}
			}
		}
	}
	//get the info of the current weapon selected, so the projectile has the correct behaviour
	public void getWeaponInfo(float damage, float bulletSpeed, GameObject particles,AudioClip soundEffect, GameObject owner, float force, GameObject scorchPrefab){
		projectileDamage = damage;
		speed = bulletSpeed;
		bulletOwner = owner;
		projectileForce = force;
		isRegularProjectile = true;
		scorch = scorchPrefab;
		if (particles) {
			projectileParticles = particles;
		}
		if (soundEffect) {
			hitSound = soundEffect;
		}
		startBullet ();
	}
	public void getGrenadeInfo(float force, float radius){
		isExplosive = true;
		isRegularProjectile = false;
		explosionForce = force;
		explosionRadius = radius;
	}
	//destroy the bullet according to the time value
	void disableBullet(float time){
		mainRigidbody.isKinematic = true;
		disableTimer = time;
		bulletMesh.SetActive(false);
	}
	//if the projectiles is placed directly in the raycast hit point, place the projectile in the correct position
	public void rayCastShoot(Collider surface,Vector3 position){
		transform.position = position;
		GetComponent<Collider> ().enabled = false;
		firedByRayCast = true;
		startBullet ();
		this.OnTriggerEnter (surface);
	}
	void startBullet(){
		GetComponent<TrailRenderer>().enabled=true;
		mainRigidbody = GetComponent<Rigidbody> ();
		if (!firedByRayCast) {
			mainRigidbody.velocity = transform.forward * speed;
		}
	}
}