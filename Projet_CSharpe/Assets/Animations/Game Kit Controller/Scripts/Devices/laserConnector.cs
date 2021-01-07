using UnityEngine;
using System.Collections;

public class laserConnector : MonoBehaviour {
	public float scrollSpeed = 0.09f;
	public float pulseSpeed = 0.28f;
	public float noiseSize = 0.19f;
	public float maxWidth = 0.1f;
	public float minWidth = 0.2f;
	public LayerMask layer;
	[HideInInspector] public GameObject currentLaser;
	[HideInInspector] public GameObject hitObject;
	LineRenderer lRenderer;
	float aniDir = 1;
	float laserDistance;
	GameObject raycast;
	GameObject laser2;
	RaycastHit hit;
	GameObject receiver;

	//the laser connector is activated when a laser device is deflected
	void Start() {
		lRenderer = gameObject.GetComponent <LineRenderer>();	
		StartCoroutine( laserAnimation());
	}
	IEnumerator laserAnimation () {
		//just a configuration to animate the laser beam
		aniDir = aniDir * 0.9f + Random.Range (0.5f, 1.5f) * 0.1f;
		yield return null;
		minWidth = minWidth * 0.8f + Random.Range (0.1f, 1) * 0.2f;
		yield return new WaitForSeconds (1 + Random.value * 2.0f - 1);	
	}
	void Update () {
		//check if the laser connector hits a lasers receiver, or any other object to disable the laser connection
		if (Physics.Raycast (transform.position, transform.forward, out hit, Mathf.Infinity, layer)) {
			if (hit.collider.gameObject.name != "laserReceiver" && hit.collider.gameObject.name != "refractionCube") {
				setLaser ();
			} 
			else {
				if (!laser2 && hitObject) {
					//get the laser inside the refraction cube
					laser2 = hitObject.transform.GetChild (0).gameObject;
					laser2.SetActive(true);
					//set the color of the laser connector according to the laser beam deflected
					if(laser2.GetComponent<Renderer>()){
						laser2.GetComponent<Renderer>().material.SetColor ("_TintColor", hitObject.GetComponent<Renderer>().material.GetColor("_Color"));
					}
				}
				laserDistance = hit.distance;
				if(!receiver){
					receiver=hit.collider.gameObject;
				}
			}
		}
		else{
			laserDistance=1000;
		}
		//set the laser size according to the hit position
		lRenderer.SetPosition (1, (laserDistance * Vector3.forward));
		animateLaser ();
	}
	public void setLaser(){
		//if the player touchs the laser connector, disable the reflected laser
		if (laser2) {
			if (laser2.activeSelf) {
				laser2.SetActive(false);
				hitObject = null;
				laser2 = null;
			}
		}
		currentLaser.GetComponent<laserDevice> ().assigned = false;
		gameObject.SetActive (false);
		if (receiver) {
			if(receiver.GetComponent<laserReceiver>()){
				receiver.GetComponent<laserReceiver>().laserDisconnected();
			}
			receiver=null;
		}
	}
	//set the color of the laser beam
	public void setColor(){
		Color c=currentLaser.GetComponent<Renderer>().material.GetColor("_TintColor");
		GetComponent<Renderer>().material.SetColor("_TintColor",c);
	}
	void animateLaser(){
		GetComponent<Renderer>().material.mainTextureOffset += new Vector2 (Time.deltaTime * aniDir * scrollSpeed, 0);
		float aniFactor = Mathf.PingPong (Time.time * pulseSpeed, 1);
		aniFactor = Mathf.Max (minWidth, aniFactor) * maxWidth;
		lRenderer.SetWidth (aniFactor, aniFactor);
		GetComponent<Renderer>().material.mainTextureScale = new Vector2(0.1f * (laserDistance),GetComponent<Renderer>().material.mainTextureScale.y);
	}
}