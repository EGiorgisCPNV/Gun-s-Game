using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class explosiveBarrel : MonoBehaviour {
	public GameObject brokenBarrel;
	public GameObject explosionParticles;
	public AudioClip explosionSound;
	public float explosionDamage;
	public float damageRadius;
	public float minVelocityToExplode;
	public float explosionDelay;
	public float explosionForce = 300;
	public bool breakInPieces;
	bool exploded;
	public bool canExplode=true;
	List<Material> rendererParts=new List<Material>();
	int i,j;
	float timeToRemove=3;
	GameObject player;
	GameObject barrelOwner;
	Rigidbody mainRigidbody;

	void Start () {
		mainRigidbody = GetComponent<Rigidbody> ();
	}
	void Update () {
		//if the barrel has exploded, wait a seconds and then 
		if (exploded) {
			if (timeToRemove > 0) {
				timeToRemove -= Time.deltaTime;
			} else {
				//change the alpha of the color in every renderer component in the fragments of the barrel
				for (i = 0; i < rendererParts.Count; i++) {
					Color alpha = rendererParts [i].color;
					alpha.a -= Time.deltaTime / 5;
					rendererParts [i].color = alpha;
					//once the alpha is 0, remove the gameObject
					if (rendererParts [i].color.a <= 0) {
						Destroy (gameObject);
					}
				}
			}
		}
	}
	//explode this barrel
	public void explodeBarrel(){
		//if the barrel has not been throwing by the player, the barrel owner is the barrel itself
		if (!barrelOwner) {
			barrelOwner = gameObject;
		}
		//disable the main mesh of the barrel and create the copy with the fragments of the barrel
		GetComponent<Collider> ().enabled = false;
		GetComponent<MeshRenderer> ().enabled = false;
		mainRigidbody.isKinematic = true;
		//check all the colliders inside the damage radius
		Collider[] objects = Physics.OverlapSphere (transform.position, damageRadius);
		foreach (Collider hits in objects) {
			//apply force to all the rigidbodies
			if (hits.GetComponent<Rigidbody> () && hits.tag != "Player") {
				hits.GetComponent<Rigidbody> ().AddExplosionForce (explosionForce * hits.GetComponent<Rigidbody> ().mass, transform.position, damageRadius);
			}
			//damage all the objects with a health or a vehicle damage receiver
			applyDamage.checkHealth (gameObject, hits.gameObject, explosionDamage, -transform.forward, transform.position, barrelOwner, false);
		}
		//create the explosion particles
		GameObject explosionParticlesClone = (GameObject)Instantiate (explosionParticles, transform.position, transform.rotation);
		explosionParticlesClone.transform.SetParent (transform);
		//if the option break in pieces is enabled, create the barrel broken
		if (breakInPieces) {
			GameObject brokenBarrelClone = (GameObject)Instantiate (brokenBarrel, transform.position, transform.rotation);
			brokenBarrelClone.transform.localScale = transform.localScale;
			brokenBarrelClone.transform.SetParent (transform);
			brokenBarrelClone.GetComponent<AudioSource> ().PlayOneShot (explosionSound);
			Component[] components = brokenBarrelClone.GetComponentsInChildren (typeof(MeshRenderer));
			foreach (Component c in components) {
				//add force to every piece of the barrel and add a box collider
				c.gameObject.AddComponent<Rigidbody> ();
				c.gameObject.AddComponent<BoxCollider> ();
				c.GetComponent<Rigidbody> ().AddExplosionForce (5, c.transform.position, 30, 1, ForceMode.Impulse);
				//change the shader of the fragments to fade them
				MeshRenderer renderPart = c.gameObject.GetComponent<MeshRenderer> ();
				for (j = 0; j < renderPart.materials.Length; j++) {
					renderPart.materials [j].shader = Shader.Find ("Legacy Shaders/Transparent/Diffuse");
					rendererParts.Add (renderPart.materials [j]);
				}
			}
		}
		//if the barrel has a drop pick ups component, call it to create the pickups
//		if (GetComponent<dropPickUpSystem> ()) {
//			GetComponent<dropPickUpSystem> ().createObjects ();
//		}
		//search the player in case he had grabbed the barrel when it exploded
		exploded = true;
		player = GameObject.Find ("Player Controller");
		player.GetComponent<grabObjects> ().checkIfDropObject (gameObject);
		player.GetComponent<otherPowers> ().checkIfDropObject (gameObject);
	}
	//if the player grabs this barrel, disable its explosion by collisions
	public void barrilCanExplodeState(bool state, GameObject newBarrelOwner){
		canExplode = state;
		barrelOwner = newBarrelOwner;
	}
	//if the barrel collides at enough speed, explode it
	void OnCollisionEnter(Collision col){
		if (mainRigidbody.velocity.magnitude > minVelocityToExplode && canExplode && !exploded) {
			//StartCoroutine (waitToExplode ());
			explodeBarrel ();
		}
	}
	public void waitToExplode(){
		StartCoroutine (waitToExplodeCorutine ());
	}
	//delay to explode the barrel
	IEnumerator waitToExplodeCorutine(){
		yield return new WaitForSeconds (explosionDelay);
		explodeBarrel ();
	}
	//set the explosion values from other component
	public void setExplosionValues(float force, float radius){
		explosionForce = force;
		damageRadius = radius;
	}
	//draw an sphere to show the damage radius
	void OnDrawGizmosSelected() {
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, damageRadius);
	}
}