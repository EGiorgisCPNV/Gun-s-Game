using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class findObjectivesSystem : MonoBehaviour
{
	public float timeToCheckSuspect;
	//public float checkPartnerDistance;
	public List<GameObject> enemies = new List<GameObject> ();
	public LayerMask layerMask;
	public tagsToShoot tagToShoot;
	public AIAttackType attackType;
	public bool avoidEnemies;
	public bool onSpotted;
	public bool runningAway;
	public GameObject enemyToShoot;
	public bool paused;
	public Transform rayCastPosition;

	public enum tagsToShoot
	{
		Player,
		enemy
	}

	public enum AIAttackType
	{
		none,
		weapons,
		melee,
		both
	}

	GameObject posibleThreat;
	public GameObject partner;
	health enemyHealth;
	SphereCollider sphereTrigger;
	checkCollisionType viewTrigger;
	RaycastHit hit;
	float originalFOVRaduis;
	float timeToCheck = 0;
	float speedMultiplier = 1;
	bool checkingThreat;
	bool moveBack;

	void Start ()
	{
		sphereTrigger = GetComponentInChildren<SphereCollider> ();
		originalFOVRaduis = sphereTrigger.radius;
		viewTrigger = GetComponentInChildren<checkCollisionType> ();
		if (tag == "enemy") {
			tagToShoot = tagsToShoot.Player;
		} else if (tag == "friend") {
			tagToShoot = tagsToShoot.enemy;
		}
	}

	void Update ()
	{
		if (!paused) { 
			closestTarget ();
			if (attackType != AIAttackType.none) {
				if (onSpotted) {
					followTarget (enemyHealth.placeToShoot);
					if (Physics.Raycast (rayCastPosition.transform.position, rayCastPosition.transform.forward, out hit, Mathf.Infinity, layerMask)) {
						if (hit.collider.gameObject == enemyToShoot || hit.collider.gameObject.transform.IsChildOf (enemyToShoot.transform)) {
							if (attackType == AIAttackType.weapons) {
								BroadcastMessage ("shootWeapon");
							}
						}
					}
				}
				//if the turret detects a target, it will check if it is an enemy, and this will take 2 seconds, while the enemy choose to leave or stay in the place
				else if (checkingThreat) {
					if (!enemyHealth) {
						//every object with a health component, has a place to be shoot, to avoid that a enemy shoots the player in his foot, so to center the shoot
						//it is used the gameObject placetoshoot in the health script
						Component component = posibleThreat.GetComponent (typeof(health));
						//get the position of the enemy to shoot
						enemyHealth = component as health;
						if (enemyHealth.dead) {
							cancelCheckSuspect (posibleThreat);
							return;
						}
					}
					//look at the target position
					followTarget (enemyHealth.placeToShoot);

					//uses a raycast to check the posible threat
					if (Physics.Raycast (rayCastPosition.position, rayCastPosition.forward, out hit, Mathf.Infinity, layerMask)) {
						if (hit.collider.gameObject == posibleThreat || hit.collider.gameObject.transform.IsChildOf (posibleThreat.transform)) {
							timeToCheck += Time.deltaTime * speedMultiplier;
						}
						//when the turret look at the target for a while, it will open fire 
						if (timeToCheck > timeToCheckSuspect) {
							timeToCheck = 0;
							checkingThreat = false;
							addEnemy (posibleThreat);
							posibleThreat = null;
						}
					}
				}
				//			if (enemies.Count == 0 && !enemyToShoot && partner && checkPartnerDistance>0) {
				//				if (Physics.Raycast (rayCastPosition.position, rayCastPosition.forward, out hit, checkPartnerDistance, layerMask)) {
				//					if (hit.collider.gameObject == partner && !moveBack) {
				//						Vector3 direction = transform.position - partner.transform.position;
				//						direction = direction / direction.magnitude;
				//						SendMessage ("setTargetOffset", direction * 2, SendMessageOptions.DontRequireReceiver);
				//						moveBack = true;
				//					}
				//				} else {
				//					if (moveBack) {
				//						SendMessage ("setTargetOffset", Vector3.zero, SendMessageOptions.DontRequireReceiver);
				//						moveBack = false;
				//					}
				//				}
				//			}
			}
		}
	}
	//follow the enemy position, to rotate torwards his direction
	void followTarget (Transform objective)
	{
		//Debug.DrawRay (rayCastPosition.position, rayCastPosition.forward, Color.red, 100);
		Vector3 targetDir = objective.position - rayCastPosition.position;
		Quaternion targetRotation = Quaternion.LookRotation (targetDir, transform.up);
		rayCastPosition.rotation = Quaternion.Slerp (rayCastPosition.rotation, targetRotation, 10 * Time.deltaTime);
	}

	public bool checkCharacterTag (GameObject character)
	{
		return ((tagToShoot == tagsToShoot.Player && character.tag == "friend") || character.tag == tagToShoot.ToString ());
	}
	//check if the object which has collided with the viewTrigger (the capsule collider in the head of the turret) is an enemy checking the tag of that object
	void checkSuspect (GameObject col)
	{
		if (checkCharacterTag (col) && !onSpotted && !posibleThreat) {
			posibleThreat = col.gameObject;
			checkingThreat = true;
		}
	}
	//in the object exits from the viewTrigger, the turret rotates again to search more enemies
	void cancelCheckSuspect (GameObject col)
	{
		if (checkCharacterTag (col) && !onSpotted && posibleThreat) {
			enemyHealth = null;
			posibleThreat = null;
			checkingThreat = false;
			timeToCheck = 0;
		}
	}
	//the sphere collider with the trigger of the turret has detected an enemy, so it is added to the list of enemies
	void enemyDetected (GameObject col)
	{
		if (checkCharacterTag (col)) {
			addEnemy (col.gameObject);
		}
	}
	//one of the enemies has left, so it is removed from the enemies list
	void enemyLost (GameObject col)
	{
		if (checkCharacterTag (col) && onSpotted) {
			removeEnemy (col.gameObject);
		}
	}
	//if anyone shoot the turret, increase its field of view to search any enemy close to it
	void checkShootOrigin (GameObject bulletOwner)
	{
		if (!onSpotted) {
			enemyDetected (bulletOwner);
		}
	}
	//add an enemy to the list, checking that that enemy is not already in the list
	void addEnemy (GameObject enemy)
	{
		if (!enemies.Contains (enemy)) {
			enemies.Add (enemy);
		}
	}
	//remove an enemy from the list
	void removeEnemy (GameObject enemy)
	{
		enemies.Remove (enemy);
	}
	//when there is one enemy or more, check which is the closest to shoot it.
	void closestTarget ()
	{
		if (enemies.Count > 0) {
			float min = Mathf.Infinity;
			int index = -1;
			for (int i = 0; i < enemies.Count; i++) {
				if (enemies [i]) {
					if (Vector3.Distance (enemies [i].transform.position, transform.position) < min) {
						min = Vector3.Distance (enemies [i].transform.position, transform.position);
						index = i;
					}
				}
			}
			enemyToShoot = enemies [index];
			Component component = enemyToShoot.GetComponent (typeof(health));
			enemyHealth = component as health;
			if (enemyHealth.dead) {
				removeEnemy (enemyToShoot);
				return;
			}
			if (avoidEnemies) {
				SendMessage ("avoidTarget", enemyToShoot.transform, SendMessageOptions.DontRequireReceiver);
				SendMessage ("setAvoidTargetState", true, SendMessageOptions.DontRequireReceiver);
				runningAway = true;
				onSpotted = true;
			} else {
				SendMessage ("setTarget", enemyToShoot.transform, SendMessageOptions.DontRequireReceiver);
			}
			SendMessage ("setPatrolState", false);
			if (!onSpotted) {
				//the player can hack the turrets, but for that he has to crouch, so he can reach the back of the turret and activate the panel
				// if the player fails in the hacking or he gets up, the turret will detect the player and will start to fire him
				if (tagToShoot == tagsToShoot.Player) {
					//check if the player fails or get up
					if (enemyToShoot.GetComponent<playerController> ()) {
						if (!enemyToShoot.GetComponent<playerController> ().crouch) {
							if (attackType == AIAttackType.weapons) {
								shootTarget ();
							}
						}
					}
					//else, the target is a friend of the player, so shoot him
					else {
						if (attackType == AIAttackType.weapons) {
							shootTarget ();
						}
					}
				} else {
					if (attackType == AIAttackType.weapons) {
						shootTarget ();
					}
				}
			}
		} 
		//if there are no enemies
		else {
			if (onSpotted || runningAway) {
				enemyHealth = null;
				enemyToShoot = null;
				onSpotted = false;
				sphereTrigger.radius = originalFOVRaduis;
				viewTrigger.gameObject.SetActive (true);
				SendMessage ("removeTarget", SendMessageOptions.DontRequireReceiver);
				if (avoidEnemies) {
					SendMessage ("setAvoidTargetState", false, SendMessageOptions.DontRequireReceiver);
					runningAway = false;
				}
				if (attackType == AIAttackType.weapons) {
					BroadcastMessage ("startOrStopUseWeapons", false);
				}
				SendMessage ("lookAtTaget", false);
				if (partner) {
					SendMessage ("setTarget", partner.transform, SendMessageOptions.DontRequireReceiver);
					SendMessage ("setPatrolState", false);
				} else {
					if (GetComponent<AIPatrolSystem> ()) {
						GetComponent<AIPatrolSystem> ().setClosestWayPoint ();
					}
				}
			}
		}
	}
	//active the fire mode
	void shootTarget ()
	{
		onSpotted = true;
		sphereTrigger.radius = Vector3.Distance (enemyToShoot.transform.position, transform.position) + 2;
		viewTrigger.gameObject.SetActive (false);
		BroadcastMessage ("startOrStopUseWeapons", true);
		SendMessage ("lookAtTaget", true);
	}

	public void pauseAction (bool state)
	{
		paused = state;
	}

	void OnTriggerEnter (Collider col)
	{
		if (!paused) {
			if (checkCharacterTag(col.gameObject)) {
//				if (attackType == AIAttackType.none) {
//					print ("run away");
//				} else {
//					enemyDetected (col.gameObject);
//				}
				enemyDetected (col.gameObject);
			} else if (col.gameObject.tag == "Player" && !partner) {
				partner = col.gameObject;
				SendMessage ("partnerFound", partner.transform, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	void OnTriggerExit (Collider col)
	{
		if (col.gameObject.tag == tagToShoot.ToString ()) {
			enemyLost (col.gameObject);
		}
	}
}