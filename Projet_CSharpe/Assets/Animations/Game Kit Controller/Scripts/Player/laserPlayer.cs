using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class laserPlayer : MonoBehaviour {
	public float scrollSpeed = 0.09f;
	public float pulseSpeed = 0.28f;
	public float noiseSize = 0.19f;
	public float maxWidth = 0.1f;
	public float minWidth = 0.2f;
	public float laserDamage = 0.3f;
	public LayerMask layer;
	public GameObject hitParticles;
	public GameObject hitSparks;
	public GameObject meshParticles;
	public bool useParticles;
	public laserDevice.laserType lasertype;
	public int reflactionLimit=10;
	LineRenderer lRenderer;
	float aniDir = 1;
	float laserDistance;
	Vector3 inDirection;
	Vector3 laserHitPosition;
	GameObject currentLaser;
	GameObject hitObject;
	GameObject player;
	RaycastHit hit;
	Ray ray;  
	int nPoints;

	void Start() {
		lRenderer = gameObject.GetComponent <LineRenderer>();	
		StartCoroutine( laserAnimation());
		player = GameObject.Find ("Player Controller");
		if (!useParticles) {
			meshParticles.SetActive(false);
		}
		reflactionLimit++;
	}
	IEnumerator laserAnimation () {
		//just a configuration to animate the laser beam
		aniDir = aniDir * 0.9f + Random.Range (0.5f, 1.5f) * 0.1f;
		yield return null;
		minWidth = minWidth * 0.8f + Random.Range (0.1f, 1) * 0.2f;
		yield return new WaitForSeconds (1 + Random.value * 2.0f - 1);	
	}
	void Update () {
		//the player's laser can be reflected, so the linerenderer has reflactionLimit vertex
		if (lasertype == laserDevice.laserType.refraction) {
			reflactionLimit = Mathf.Clamp(reflactionLimit,1,reflactionLimit);
			ray = new Ray(Camera.main.transform.position, Camera.main.transform.TransformDirection (Vector3.forward));  
			nPoints = reflactionLimit;  			 
			//make the lineRenderer have nPoints  
			lRenderer.SetVertexCount(reflactionLimit);  
			//set the first point of the line it its current positions
			lRenderer.SetPosition(0,transform.position);  
			for(int i=0;i<reflactionLimit;i++) {  
				//if the ray has not be reflected yet  
				if(i==0) {  
					//check if the ray has hit something  
					if(Physics.Raycast(ray.origin, ray.direction, out hit,Mathf.Infinity, layer)) {  
						//the reflection direction is the reflection of the current ray direction flipped at the hit normal  
						inDirection = Vector3.Reflect(ray.direction,hit.normal);  
						//cast the reflected ray, using the hit point as the origin and the reflected direction as the direction  
						ray = new Ray(hit.point,inDirection);  
						//if the number of reflections is set to 1  
						if(reflactionLimit==1) {  
							//add a new vertex to the line renderer  
							lRenderer.SetVertexCount(nPoints++);  
						}  
						//set the position of the next vertex at the line renderer to be the same as the hit point  
						lRenderer.SetPosition(i+1,hit.point);  
						laserDistance = hit.distance;
					}  
					else{
						//if the rays does not hit anything, set as a single straight line in the camera direction and disable the smoke
						laserDistance = 1000;
						transform.rotation = Camera.main.transform.rotation;
						hitParticles.SetActive(false);
						hitSparks.SetActive(false);
						lRenderer.SetVertexCount (2);
						lRenderer.SetPosition(0,transform.position);
						lRenderer.SetPosition (1, (laserDistance * transform.forward));
					}
				}  
				else if(i>0){  
					//check if the ray has hit something  
					if(Physics.Raycast(ray.origin,ray.direction, out hit, Mathf.Infinity, layer)){  
						//the refletion direction is the reflection of the ray's direction at the hit normal  
						inDirection = Vector3.Reflect(inDirection,hit.normal);  
						//cast the reflected ray, using the hit point as the origin and the reflected direction as the direction  
						ray = new Ray(hit.point,inDirection);  
						lRenderer.SetVertexCount(nPoints++);  
						//set the position of the next vertex at the line renderer to be the same as the hit point  
						lRenderer.SetPosition(i+1,hit.point); 
						if(i+1==reflactionLimit){
							//if this linerenderer vertex is the last, set the smoke in its position and check for a refraction cube or  a laser receiver
							hitSparks.SetActive(true);
							hitParticles.SetActive(true);
							hitParticles.transform.position = hit.point ;
							hitParticles.transform.rotation = Quaternion.identity;
							hitSparks.transform.rotation = Quaternion.LookRotation (hit.normal, transform.up);
							if (hit.collider.gameObject.name == "laserReceiver" || hit.collider.gameObject.name == "refractionCube") {
								hitObject = hit.collider.gameObject;
								connectLasers ();
							}
							//check if the laser hits an object with a health component different from the player
							if(hit.collider.gameObject!=player){
								applyDamage.checkHealth (gameObject, hit.collider.gameObject, laserDamage, -transform.forward, hit.point, player, true);
							}
						}
						laserDistance = hit.distance;
					}  
				}
			}  
		}

		//the player's laser cannot be reflected, so the linerenderer only has 2 vertex
		if (lasertype == laserDevice.laserType.simple) {
			animateLaser ();
			if (Physics.Raycast (Camera.main.transform.position, Camera.main.transform.TransformDirection (Vector3.forward), out hit, Mathf.Infinity, layer)) {
				//set the direction of the laser in the hit point direction
				transform.LookAt(hit.point);
				//check with a raycast if the laser hits a receiver, a refraction cube or a gameObject with a health component or a vehicle damage receiver
				//Debug.DrawRay (Camera.main.transform.position, Camera.main.transform.TransformDirection (Vector3.forward)*hit.distance, Color.yellow);
				if (hit.collider.gameObject.name == "laserReceiver" || hit.collider.gameObject.name == "refractionCube") {
					hitObject = hit.collider.gameObject;
					connectLasers ();
				}
				if(hit.collider.gameObject!=player){
					applyDamage.checkHealth (gameObject, hit.collider.gameObject, laserDamage, -transform.forward, hit.point, player, true);
				}
				//get the hit position to set the particles of smoke and sparks
				laserDistance = hit.distance;
				hitSparks.SetActive(true);
				hitParticles.SetActive(true);
				hitParticles.transform.position = hit.point - transform.forward * 0.02f;
				hitParticles.transform.rotation = Quaternion.identity;
				hitSparks.transform.rotation = Quaternion.LookRotation (hit.normal, transform.up);
				lRenderer.SetVertexCount (2);
				lRenderer.SetPosition(0,transform.position);
				lRenderer.SetPosition (1, hit.point);
			}
			else{
				//set the direction of the laser in the camera forward
				Quaternion lookDir = Quaternion.LookRotation(Camera.main.transform.TransformDirection (Vector3.forward));
				transform.rotation = lookDir;
				hitParticles.SetActive(false);
				hitSparks.SetActive(false);
				laserDistance = 1000;
				lRenderer.SetVertexCount (2);
				lRenderer.SetPosition(0,transform.position);
				lRenderer.SetPosition (1, (laserDistance * transform.forward));
			}
			//set the laser size 
			if (useParticles){
				if(!meshParticles.activeSelf){
					meshParticles.SetActive(true);
				}
				//set the size of the meshParticles and adjust its em
				meshParticles.transform.GetChild (0).GetComponent<ParticleEmitter> ().minEmission = laserDistance * 3;
				meshParticles.transform.GetChild (0).GetComponent<ParticleEmitter> ().maxEmission = laserDistance * 3;
				meshParticles.transform.localScale = new Vector3 (1, 1, laserDistance);
			}
		}
	}
	void connectLasers(){
		//check if the object touched with the laser is a laser receiver, to check if the current color of the laser is equal to the color needed
		//in the laser receiver
		if (hitObject.GetComponent<laserReceiver> ()) {
			if (hitObject.GetComponent<laserReceiver> ().colorNeeded == GetComponent<Renderer>().material.GetColor ("_TintColor")) {
				hitObject.GetComponent<laserReceiver> ().laserConnected (GetComponent<Renderer>().material.GetColor ("_TintColor"));

			} else {
				//else the laser is not reflected
				return;
			}
		}
		//if the object is not a laser receiver or a refraction cube, the laser is not refrated
		else if (hitObject.name != "refractionCube") {
			return;
		} 
		//deflect the laser and enable the laser connector 
		GameObject baseLaserConnector = currentLaser.GetComponent<laserDevice> ().laserConnector;
		baseLaserConnector.SetActive(true);
		baseLaserConnector.transform.position = laserHitPosition;
		baseLaserConnector.transform.LookAt (hitObject.transform.position);
		//if the hitted objects is a cube refraction, enable the laser inside it
		if (hitObject.GetComponent<refractionCube> ()) {
			GameObject cubeLaser = hitObject.transform.GetChild (0).gameObject;
				if (!cubeLaser.activeSelf) {
				baseLaserConnector.GetComponent<laserConnector> ().hitObject = hitObject;
				cubeLaser.transform.rotation = baseLaserConnector.transform.rotation;
			} 
			else {
				baseLaserConnector.SetActive(false);
				return;
			}
		}
		hitObject = null;
		//stop the laser that hits the player from detect any other collision, to deflect it
		currentLaser.SendMessage ("assignLaser");
		baseLaserConnector.GetComponent<laserConnector> ().currentLaser = currentLaser;
		baseLaserConnector.GetComponent<laserConnector> ().setColor ();
		currentLaser = null;
		gameObject.SetActive(false);
	}
	//set the color of the laser according to the color of the laser device
	void setColor(){
		if (currentLaser.GetComponent<Renderer>().material.HasProperty("_TintColor")) {
			Color c = currentLaser.GetComponent<Renderer>().material.GetColor ("_TintColor");
			GetComponent<Renderer>().material.SetColor ("_TintColor", c);
		}
	}
	
	public void setLaserInfo(laserDevice.laserType type, GameObject l, Vector3 pos){
		//get the position where the lasers hits the player, 
		laserHitPosition=pos;
		//get the laser that it is hitting the player
		currentLaser=l;
		setColor();
		//set if the laser reflects in other surfaces or not
		if (!lRenderer) {
			lRenderer = gameObject.GetComponent <LineRenderer>();	
		}
		lasertype = type;
		if (lasertype == laserDevice.laserType.refraction) {
			//lRenderer.useWorldSpace=true;
		} else {
			lRenderer.SetVertexCount (2);
			lRenderer.SetPosition(0,Vector3.zero);
			//lRenderer.useWorldSpace=false;
		}
	}
	//make the laser changes its width
	void animateLaser(){
		GetComponent<Renderer>().material.mainTextureOffset += new Vector2 (Time.deltaTime * aniDir * scrollSpeed, 0);
		float aniFactor = Mathf.PingPong (Time.time * pulseSpeed, 1);
		aniFactor = Mathf.Max (minWidth, aniFactor) * maxWidth;
		lRenderer.SetWidth (aniFactor, aniFactor);
		GetComponent<Renderer>().material.mainTextureScale = new Vector2(0.1f * (laserDistance),GetComponent<Renderer>().material.mainTextureScale.y);
	}
}