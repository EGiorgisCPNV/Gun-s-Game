using UnityEngine;
using System.Collections;

public class laserDevice : MonoBehaviour {
	public float scrollSpeed = 0.09f;
	public float pulseSpeed = 0.28f;
	public float noiseSize = 0.19f;
	public float maxWidth = 0.1f;
	public float minWidth = 0.2f;
	public LayerMask layer;
	public bool assigned;
	public GameObject laserConnector;
	public laserType lasertype;
	public float damageAmount;
	public bool canDamagePlayer;
	public bool canDamageCharacters;
	public bool canDamageVehicles;
	public bool canDamageEverything;
	public bool canKillWithOneHit;
	GameObject player;
	LineRenderer lRenderer;
	bool forceFieldEnabled;
	RaycastHit hit;
	float aniDir;
	float laserDistance;
	Vector3 hitPointPosition;
	float rayDistance;
	float hitDistance;
	bool hittingSurface;
	bool damageCurrentSurface;
	bool laserEnabled=true;

	public enum laserType{
		simple,refraction,
	}
	
	void Start() {
		lRenderer = gameObject.GetComponent<LineRenderer> ();	
		StartCoroutine (laserAnimation ());
		player = GameObject.Find ("Player Controller");
		//get the initial raycast distance
		rayDistance = Mathf.Infinity;

	}

	void Update () {
		if (laserEnabled) {
			lRenderer.SetVertexCount (2);
			lRenderer.SetPosition (0, transform.position);
			//check if the hitted object is the player, enabling or disabling his shield
			if (Physics.Raycast (transform.position, transform.forward, out hit, rayDistance, layer)) {
				//if the laser has been deflected, then check if any object collides with it, to disable all the other reflections of the laser
				hittingSurface = true;
				laserDistance = hit.distance;
				hitPointPosition = hit.point;
			} else {
				//the laser does not hit anything, so disable the shield if it was enabled
				hittingSurface = false;
			}
			if (hittingSurface) {
				if (assigned) {
					forceFieldEnabled = false;
					player.SendMessage ("deactivateLaserForceField");
					rayDistance = Mathf.Infinity;
					laserConnector.GetComponent<laserConnector> ().setLaser ();
				} else {
					///the laser touchs the player, active his shield and set the laser that is touching him
					if (hit.transform.tag == "Player" && !hit.collider.isTrigger && !forceFieldEnabled) {
						player.GetComponent<otherPowers> ().setLaser (gameObject, lasertype);
						forceFieldEnabled = true;
					}
					if (forceFieldEnabled) {
						hitDistance = hit.distance;
						//set the position where this laser is touching the player
						Vector3 position = hit.point;
						player.SendMessage ("activateLaserForceField", position);
						//the laser has stopped to touch the player, so deactivate the player's shield
						if (hit.transform.tag != "Player") {
							forceFieldEnabled = false;
							player.SendMessage ("deactivateLaserForceField");
						}
					}
				}
				if (canDamagePlayer && hit.transform.tag == "Player") {
					damageCurrentSurface = true;
				} else if (canDamageCharacters && hit.transform.GetComponent<characterDamageReceiver> ()) {
					damageCurrentSurface = true;
				} else if (canDamageVehicles && hit.transform.GetComponent<vehicleDamageReceiver> ()) {
					damageCurrentSurface = true;
				}
				if (canDamageEverything) {
					damageCurrentSurface = true;
				}
				if (damageCurrentSurface) {
					if (canKillWithOneHit) {
						float remainingHealth = applyDamage.getRemainingHealth (hit.transform.gameObject);
						applyDamage.checkHealth (gameObject, hit.transform.gameObject, remainingHealth, transform.forward, hit.point, gameObject, true);
					} else {
						applyDamage.checkHealth (gameObject, hit.transform.gameObject, damageAmount, transform.forward, hit.point, gameObject, true);
					}
				}
				lRenderer.SetPosition (1, hitPointPosition);
			} else {
				if (!assigned) {
					if (forceFieldEnabled) {
						forceFieldEnabled = false;
						player.SendMessage ("deactivateLaserForceField");
						//set to infinite the raycast distance again
						rayDistance = Mathf.Infinity;
					}		
					laserDistance = 1000;	
					lRenderer.SetPosition (1, (laserDistance * transform.forward));
				}
			}

			animateLaser ();
		}
	}
	void OnDisable(){
		if (assigned) {
			forceFieldEnabled = false;
			if(player){
				player.SendMessage("deactivateLaserForceField");
				//set to infinite the raycast distance again
				rayDistance=Mathf.Infinity;
				//disable the laser connector
				laserConnector.GetComponent<laserConnector>().setLaser();
			}
		}
	}

	//set the laser that it is touching the player, to assign it to the laser connector
	void assignLaser(){
		assigned = true;
		rayDistance = hitDistance;
		player.SendMessage("deactivateLaserForceField");
	}

	void animateLaser(){
		GetComponent<Renderer>().material.mainTextureOffset += new Vector2 (Time.deltaTime * aniDir * scrollSpeed, 0);
		float aniFactor = Mathf.PingPong (Time.time * pulseSpeed, 1);
		aniFactor = Mathf.Max (minWidth, aniFactor) * maxWidth;
		lRenderer.SetWidth (aniFactor, aniFactor);
		GetComponent<Renderer>().material.mainTextureScale = new Vector2(0.1f * (laserDistance),GetComponent<Renderer>().material.mainTextureScale.y);
	}
	IEnumerator laserAnimation () {
		//just a configuration to animated the laser beam
		aniDir = aniDir * 0.9f + Random.Range (0.5f, 1.5f) * 0.1f;
		yield return null;
		minWidth = minWidth * 0.8f + Random.Range (0.1f, 1) * 0.2f;
		yield return new WaitForSeconds (1 + Random.value * 2 - 1);	
	}

	public void disableLaser(){
		laserEnabled = false;
		lRenderer.enabled = false;
	}
}