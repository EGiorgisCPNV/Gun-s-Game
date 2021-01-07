using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class destroyableObject : MonoBehaviour
{
	public GameObject destroyedParticles;
	public AudioClip destroyedSound;
	public bool useExplosionForceWhenDestroyed;
	public float explosionRadius;
	public float explosionForce;
	public float explosionDamage;
	public float timeToFadePieces;
	public bool destroyed;
	public bool showGizmo;
	List<Material> rendererParts = new List<Material> ();
	List<Collider> collidersAround = new List<Collider> ();
	Rigidbody mainRigidbody;
	mapObjectInformation mapInformationManager;
	AudioSource destroyedSource;

	void Start ()
	{
		mainRigidbody = GetComponent<Rigidbody> ();
		mapInformationManager = GetComponent<mapObjectInformation> ();
		destroyedSource = GetComponent<AudioSource> ();
	}

	void Update ()
	{
		if (destroyed) {
			if (timeToFadePieces > 0) {
				timeToFadePieces -= Time.deltaTime;
			}
			if (timeToFadePieces <= 0) {
				int piecesAmountFade = 0;
				for (int i = 0; i < rendererParts.Count; i++) {
					Color alpha = rendererParts [i].color;
					alpha.a -= Time.deltaTime / 5;
					rendererParts [i].color = alpha;
					if (alpha.a <= 0) {
						piecesAmountFade++;
					}
				}
				if (piecesAmountFade == rendererParts.Count) {
					Destroy (gameObject);
				}
			}
		}
	}
	//Destroy the object
	public void destroyObject ()
	{
		//instantiated an explosiotn particles
		GameObject destroyedParticlesClone = (GameObject)Instantiate (destroyedParticles, transform.position, transform.rotation);
		destroyedParticlesClone.transform.SetParent (transform);
		if (destroyedSource) {
			destroyedSource.PlayOneShot (destroyedSound);
		}
		//set the velocity of the object to zero
		if (mainRigidbody) {
			mainRigidbody.velocity = Vector3.zero;
			mainRigidbody.isKinematic = true;
		}
		//get every renderer component if the object
		Component[] components = GetComponentsInChildren (typeof(MeshRenderer));
		foreach (Component c in components) {
			if (c.GetComponent<Renderer> () && c.gameObject.layer != LayerMask.NameToLayer ("Scanner")) {
				if (c.GetComponent<Renderer> ().enabled) {
					//for every renderer object, change every shader in it for a transparent shader 
					for (int j = 0; j < c.gameObject.GetComponent<MeshRenderer> ().materials.Length; j++) {
						c.GetComponent<Renderer> ().materials [j].shader = Shader.Find ("Legacy Shaders/Transparent/Diffuse");
						rendererParts.Add (c.GetComponent<MeshRenderer> ().materials [j]);
					}
					//set the layer ignore raycast to them
					c.gameObject.layer = LayerMask.NameToLayer ("Ignore Raycast");
					//add rigidbody and box collider to them
					if (!c.gameObject.GetComponent<Rigidbody> ()) {
						c.gameObject.AddComponent<Rigidbody> ();
					}
					if (!c.gameObject.GetComponent<BoxCollider> ()) {
						c.gameObject.AddComponent<BoxCollider> ();
					}
					//apply explosion force
					c.gameObject.GetComponent<Rigidbody> ().AddExplosionForce (500, transform.position, 50, 3);
				}
			} 
		}
		//any other object with a collider but with out renderer, is disabled
		Component[] collidersInObject = GetComponentsInChildren (typeof(Collider));
		foreach (Component c in collidersInObject) {
			if (c.gameObject.GetComponent<Collider> () && !c.GetComponent<Renderer> ()) {
				c.gameObject.GetComponent<Collider> ().enabled = false;
			}
		}
		if (mapInformationManager) {
			mapInformationManager.removeMapObject ();
		}
		if (useExplosionForceWhenDestroyed) {
			if (collidersAround.Count == 0) {
				collidersAround.AddRange (Physics.OverlapSphere (transform.position, explosionRadius));
				foreach (Collider hit in collidersAround) {
					if (hit != null) {
						if (hit.gameObject.tag != "Player" && hit.gameObject.tag != "bullet" && !hit.gameObject.transform.IsChildOf (transform)) {
							if (hit.GetComponent<Rigidbody> ()) {
								if (!hit.GetComponent<Rigidbody> ().isKinematic) {
									hit.GetComponent<Rigidbody> ().AddExplosionForce (explosionForce, transform.position, explosionRadius, 3, ForceMode.Impulse);
								}
							}
							if (explosionDamage > 0) {
								applyDamage.checkHealth (gameObject, hit.gameObject, explosionDamage, -transform.forward, hit.gameObject.transform.position, gameObject, false);
							}
						}
					}
				}
			}
		}
		destroyed = true;
	}

	void OnDrawGizmos ()
	{
		DrawGizmos ();
	}

	void OnDrawGizmosSelected ()
	{
		DrawGizmos ();
	}

	void DrawGizmos ()
	{
		if (showGizmo) {
			if (useExplosionForceWhenDestroyed) {
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere (transform.position, explosionRadius);
			}
		}
	}
}
