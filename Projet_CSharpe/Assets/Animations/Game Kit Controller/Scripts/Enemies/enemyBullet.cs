using UnityEngine;
using System.Collections;

public class enemyBullet : MonoBehaviour {
	public float lifeTime  = 5;
	public GameObject bulletMesh;
	public GameObject missileMesh;
	public float speed = 15;
	public LayerMask layer;
	public AudioClip cannonSound;
	public GameObject explosionParticles;
	[HideInInspector] public float damage;
	[HideInInspector] public bool bullet;
	[HideInInspector] public bool missile;
	[HideInInspector] public float speedMultiplier=1;
	[HideInInspector] public GameObject bulletOwner;
	[HideInInspector] public GameObject enemy;
	float spawnTime = 0;
	bool hacked;
	bool paused;
	int i;
	TrailRenderer trail;
	Rigidbody mainRigidbody;

	void Start(){
		trail = GetComponent<TrailRenderer> ();
		//check which type of projectile has been fired, missile or bullet, and disable the opposite
		if (missile) {
			bulletMesh.SetActive(false);
		}
		if (bullet) {
			trail.enabled=false;
			missileMesh.SetActive(false);
		}
		mainRigidbody = GetComponent<Rigidbody> ();
	}
	void Update () {
		//the projectile is kinematic if it touches the player shield
		if (!mainRigidbody.isKinematic) {
			//set a time to destroy the projectile if it does not hit any surface
			spawnTime += Time.deltaTime;
			if (spawnTime > lifeTime) {
				Destroy (gameObject);
			}
			//if the projectile is a missile, it follows the player, even if he moves 
			if (missile) {		
				//enemy is the target of the missile, if the missile hits the shield, the enemy will be the turret which fired the missile, like the bullet
				//if the turret which fired the missile is destroyed before the player shoots it, the missile is launched in straight line
				//but it will check any enemy close to it and follow him until it reaches him
				if(enemy){
					if(!trail.enabled){
						trail.enabled=true;
					}
					//set the position of the missile to follow smoothly to its target
					mainRigidbody.velocity=transform.forward * speed;
					Quaternion rotation = Quaternion.LookRotation(enemy.transform.position + enemy.transform.up - transform.position);
					transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime *speed);
				}
				else{
					missile=false;
					bullet=true;
				}
			}
			//if the projectile is a bullet, then move in straight line
			if (bullet) {
				mainRigidbody.velocity = transform.forward * speed * speedMultiplier; 
			}
			//if the missile has been catched by the player shield, and then fired, but the turret which fired it was destroyed, check any close enemy while
			//the missile moves 
			if(hacked && !enemy){
				Collider[] colliders = Physics.OverlapSphere (transform.position,5,layer);
				for (i=0; i<colliders.Length; i++) {
					if(colliders[i].tag=="enemy"){
						enemy=colliders[i].gameObject;
						if(missileMesh.activeSelf){
							bullet=false;
							missile=true;
						}
						return;
					}
				}
			}
		}
	}
	//check if the projectile hits anything 
	void OnTriggerEnter(Collider col){
		//avoid that the projectile hits the same turret that fired it, but only in case that the bullet has not been hacked
		if (!col.GetComponent<Collider>().isTrigger && !paused && (col.gameObject!=bulletOwner.transform.GetChild(0).gameObject || hacked )) {
			//if the projectile is a missle, it will damage all in a determined radius that has a health script 
			if(missileMesh.activeSelf){
				GameObject particlesClone = (GameObject)Instantiate (explosionParticles, transform.position, Quaternion.identity);
				particlesClone.AddComponent<AudioSource>().PlayOneShot(cannonSound);
				particlesClone.AddComponent<destroyGameObject> ().timer = 2;
				Collider[] colliders = Physics.OverlapSphere (transform.position, 7);
				for (i=0; i<colliders.Length; i++) {
					if(colliders [i].GetComponent<Rigidbody>()){
						Rigidbody colliderRigidbody = colliders [i].GetComponent<Rigidbody> ();
						if (colliders [i].tag != "Player") {
							//also apply force to any rigidbody in that radius
							if (!colliderRigidbody.isKinematic) {
								colliderRigidbody.AddExplosionForce (500, transform.position, 15, 3);
							}
						}
					}
					applyDamage.checkHealth (gameObject, colliders[i].gameObject, damage, -transform.forward, colliders[i].transform.position+colliders[i].transform.up, bulletOwner, false);
				}
			}
			else{
				applyDamage.checkHealth (gameObject, col.gameObject, damage, -transform.forward, transform.position, bulletOwner, false);
			}
			Destroy (gameObject);
		}
		//if the projectile touchs the shield, set all the parameters to allow the player shoot back to the turrets 
		//also if the bullet is fired by the player, the shield is no longer checked, to avoid issues
		if (col.GetComponent<Collider>().gameObject.layer == LayerMask.NameToLayer ("shield") && bulletOwner){
			if(bulletOwner.tag!="Player"){
				trail.enabled=false;
				gameObject.transform.parent=col.GetComponent<Collider>().gameObject.transform.parent;
				speed=0;
				mainRigidbody.isKinematic=true;
				spawnTime=0;
				paused=true;
			}
		}
	}
	//if the player press the right button of the mouse or the touch controls, the projectile is sending back to the turret which fired it
	public void returnBullet(Vector3 direction,GameObject owner){
		paused = false;
		speed=15;
		mainRigidbody.isKinematic=false;
		gameObject.transform.parent = null;
		hacked = true;
		//check if the turret is not destroyed
		if (bulletOwner) {
			if(!bulletOwner.GetComponent<AITurret>().dead){
				transform.LookAt (bulletOwner.transform);
				enemy = bulletOwner;
			}
			else{
				transform.LookAt(direction);
				enemy = null;
			}
		} 
		else {
			transform.LookAt(direction);
			enemy = null;
		}
		//now the owner of the projectile is the player
		bulletOwner=owner;
	}
}