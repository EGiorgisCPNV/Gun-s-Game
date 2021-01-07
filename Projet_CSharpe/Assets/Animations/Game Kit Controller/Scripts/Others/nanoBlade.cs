using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class nanoBlade : MonoBehaviour {
	public float disableTimer=5;
	public LayerMask layer;
	GameObject projectile;
	GameObject objectToDamage;
	GameObject player;
	GameObject shield;
	GameObject enemy;
	RaycastHit hit;
	bool projectileUsed;
	Rigidbody mainRigidbody;
	float projectileSpeed;
	float projectileDamage;
	AudioClip impactSound;

	void Start () {
		mainRigidbody = GetComponent<Rigidbody> ();
		projectile = gameObject;
		//ignore collision between the bullet and the player
		Physics.IgnoreCollision(player.GetComponent<Collider>(), GetComponentInChildren<Collider>());
		if(shield){
			Physics.IgnoreCollision(shield.GetComponentInChildren<Collider>(), GetComponentInChildren<Collider>());
		}
		GetComponent<TrailRenderer>().enabled=true;
		//the bullet moves on the camera direction
		mainRigidbody.velocity = transform.forward * projectileSpeed;
		//check what type of bullet has been fired, to activate some parameters, particles, etc...
	}
	void Update () {

	}
	//when the bullet touchs a surface, then
	void OnTriggerEnter(Collider col){
		if (!col.isTrigger && !projectileUsed && col.gameObject.layer != LayerMask.NameToLayer ("Ignore Raycast")) {
			if (impactSound) {
				GetComponent<AudioSource> ().PlayOneShot (impactSound);
			}
			projectileUsed = true;
			//set the bullet kinematic
			objectToDamage = col.GetComponent<Collider> ().gameObject;
			Vector3 previousVelocity = mainRigidbody.velocity;
			//print (objectToDamage.name);
			mainRigidbody.isKinematic = true;
			if (objectToDamage.GetComponent<Rigidbody> ()) {
				if (objectToDamage.GetComponent<AIRagdollActivator> ()) {
					List<AIRagdollActivator.BodyPart> bones = objectToDamage.GetComponent<AIRagdollActivator> ().bodyParts;
					float distance = Mathf.Infinity;
					int index = -1;
					for (int i = 0; i < bones.Count; i++) {
						float currentDistance = Vector3.Distance (bones [i].transform.position, transform.position);
						if (currentDistance < distance) {
							distance = currentDistance;
							index = i;
						}
					}
					if (index != -1) {
						transform.SetParent (bones [index].transform);
						//print (bones [index].transform.name);
						if (applyDamage.checkIfDead (objectToDamage)) {
							mainRigidbody.isKinematic = false;
							mainRigidbody.velocity = previousVelocity;
							projectileUsed = false;
						}
					}
				} else if (objectToDamage.GetComponent<characterDamageReceiver> ()) {
					transform.SetParent (objectToDamage.GetComponent<characterDamageReceiver> ().character.transform);
				} else {
					transform.SetParent (objectToDamage.transform);
				}
			} else if (objectToDamage.GetComponent<characterDamageReceiver> ()) {
				transform.SetParent (objectToDamage.transform);
			} else if (objectToDamage.GetComponent<vehicleDamageReceiver> ()) {
				transform.SetParent (objectToDamage.GetComponent<vehicleDamageReceiver> ().vehicle.transform);
			}
			//add velocity if the touched object has rigidbody
			applyDamage.checkHealth (projectile, objectToDamage, projectileDamage, -transform.forward, transform.position, player, false);
			if (applyDamage.canApplyForce (objectToDamage)) {
				Vector3 force = transform.forward * 20;
				objectToDamage.GetComponent<Rigidbody> ().AddForce (force * objectToDamage.GetComponent<Rigidbody> ().mass, ForceMode.Impulse);
			}

			//disableBullet (0.5f);
		}
	}
	//destroy the bullet according to the time value
	void disableBullet(float time){
		mainRigidbody.isKinematic = true;
		disableTimer = time;
	}
	public void setProjectileInfo(GameObject playerObj,GameObject shieldObj,float damage,AudioClip impactEffect, float speed){
		player = playerObj;
		shield = shieldObj;
		projectileDamage = damage;
		impactSound = impactEffect;
		projectileSpeed = speed;
	}
}