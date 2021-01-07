using UnityEngine;
using System.Collections;

public class vehicleLaser : MonoBehaviour {
	public float scrollSpeed = 0.09f;
	public float pulseSpeed = 0.28f;
	public float noiseSize = 0.19f;
	public float maxWidth = 0.1f;
	public float minWidth = 0.2f;
	public LayerMask layer;
	public GameObject hitParticles;
	public GameObject hitSparks;
	[HideInInspector] public float laserDamage=0.3f;
	LineRenderer lRenderer;
	float aniDir;
	float laserDistance;
	RaycastHit hit;
	bool working;
	GameObject player;

	void Start() {
		player=GameObject.Find("Player Controller");
		//it works like similar to the other lasers, but checking that the hitted object has a health or a vehicle damage receiver component
		lRenderer = gameObject.GetComponent<LineRenderer>();	
		changeLaserState (false);
	}

	IEnumerator laserAnimation () {
		//just a configuration to animate the laser beam 
		aniDir = aniDir * 0.9f + Random.Range (0.5f, 1.5f) * 0.1f;
		yield return null;
		minWidth = minWidth * 0.8f + Random.Range (0.1f, 1) * 0.2f;
		yield return new WaitForSeconds (1 + Random.value * 2 - 1);	
	}

	void Update () {
		if (working) {
			//check the hit collider of the raycast
			if (Physics.Raycast (Camera.main.transform.position,Camera.main.transform.TransformDirection (Vector3.forward), out hit, Mathf.Infinity, layer)) {
				transform.LookAt(hit.point);
				applyDamage.checkHealth (gameObject, hit.collider.gameObject, laserDamage, -transform.forward, (hit.point - (hit.normal / 4)), player, true);
				//set the sparks and .he smoke in the hit point
				laserDistance = hit.distance;
				hitSparks.SetActive (true);
				hitParticles.SetActive (true);
				hitParticles.transform.position = hit.point + (transform.position - hit.point) * 0.02f;
				hitParticles.transform.rotation = Quaternion.identity;
				hitSparks.transform.rotation = Quaternion.LookRotation (hit.normal, transform.up);
			} else {
				//if the laser does not hit anything, disable the particles and set the hit point
				hitParticles.SetActive (false);
				hitParticles.SetActive (false);
				laserDistance = 1000;	
			}
			//set the size of the laser, according to the hit position
			lRenderer.SetPosition (1, (laserDistance * Vector3.forward));
			animateLaser ();
		}
	}
	void animateLaser(){
		GetComponent<Renderer>().material.mainTextureOffset += new Vector2 (Time.deltaTime * aniDir * scrollSpeed, 0);
		float aniFactor = Mathf.PingPong (Time.time * pulseSpeed, 1);
		aniFactor = Mathf.Max (minWidth, aniFactor) * maxWidth;
		lRenderer.SetWidth (aniFactor, aniFactor);
		GetComponent<Renderer>().material.mainTextureScale = new Vector2(0.1f * (laserDistance),GetComponent<Renderer>().material.mainTextureScale.y);
	}
	//enable or disable the vehicle laser
	public void changeLaserState(bool state){
		lRenderer.enabled = state;
		working = state;
		if (state) {
			StartCoroutine (laserAnimation ());
		} else {
			hitSparks.SetActive (false);
			hitParticles.SetActive (false);
		}
	}
}
