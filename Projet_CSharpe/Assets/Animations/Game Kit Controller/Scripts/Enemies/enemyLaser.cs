using UnityEngine;
using System.Collections;

public class enemyLaser : MonoBehaviour {
	public float scrollSpeed = 0.09f;
	public float pulseSpeed = 0.28f;
	public float noiseSize = 0.19f;
	public float maxWidth = 0.1f;
	public float minWidth = 0.2f;
	public LayerMask layer;
	public GameObject hitParticles;
	public GameObject hitSparks;
	[HideInInspector] public GameObject bulletOwner;
	[HideInInspector] public float laserDamage=0.3f;
	LineRenderer lRenderer;
	float aniDir;
	float laserDistance;
	RaycastHit hit;
	
	void Start() {
		//it works like similar to the other lasers, but checking that the hitted object has a health component
		lRenderer = gameObject.GetComponent<LineRenderer>();	
		StartCoroutine( laserAnimation());
	}
	
	IEnumerator laserAnimation () {
		//just a configuration to animate the laser beam 
		aniDir = aniDir * 0.9f + Random.Range (0.5f, 1.5f) * 0.1f;
		yield return null;
		minWidth = minWidth * 0.8f + Random.Range (0.1f, 1) * 0.2f;
		yield return new WaitForSeconds (1 + Random.value * 2 - 1);	
	}
	
	void Update () {
		//check the hit collider of the raycast
		if (Physics.Raycast (transform.position, transform.forward, out hit, Mathf.Infinity, layer)) {
			applyDamage.checkHealth (gameObject, hit.collider.gameObject, laserDamage, -transform.forward, (hit.point - (hit.normal / 4)), bulletOwner, true);
			//set the sparks and .he smoke in the hit point
			laserDistance = hit.distance;
			hitSparks.SetActive (true);
			hitParticles.SetActive (true);
			hitParticles.transform.position = hit.point + (transform.position - hit.point) * 0.02f;
			hitParticles.transform.rotation = Quaternion.identity;
			hitSparks.transform.rotation = Quaternion.LookRotation (hit.normal, transform.up);
		}
		else {
			//if the laser does not hit anything, disable the particles and set the hit point
			hitParticles.SetActive(false);
			hitParticles.SetActive(false);
			laserDistance=1000;	
		}
		//set the size of the laser, according to the hit position
		lRenderer.SetPosition (1, (laserDistance * Vector3.forward));
		animateLaser ();
	}
	void animateLaser(){
		GetComponent<Renderer>().material.mainTextureOffset += new Vector2 (Time.deltaTime * aniDir * scrollSpeed, 0);
		float aniFactor = Mathf.PingPong (Time.time * pulseSpeed, 1);
		aniFactor = Mathf.Max (minWidth, aniFactor) * maxWidth;
		lRenderer.SetWidth (aniFactor, aniFactor);
		GetComponent<Renderer>().material.mainTextureScale = new Vector2(0.1f * (laserDistance),GetComponent<Renderer>().material.mainTextureScale.y);
	}
}