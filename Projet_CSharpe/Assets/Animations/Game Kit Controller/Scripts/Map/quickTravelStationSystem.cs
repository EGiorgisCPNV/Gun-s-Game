using UnityEngine;
using System.Collections;
public class quickTravelStationSystem : MonoBehaviour {
	public Transform quickTravelTransform;
	public LayerMask layer;
	public string animationName;
	public AudioClip enterAudioSound;
	[TextArea(3,10)] public string beaconDescription;
	public int floorNumber;
	public bool stationActivated;
	Animation stationAnimation;
	AudioSource audioSource;
	GameObject player;
	RaycastHit hit;
	mapObjectInformation mapObjectInformationManager;

	void Start () {
		stationAnimation = GetComponent<Animation> ();
		audioSource = GetComponent<AudioSource> ();
	}
	void Update () {

	}
	public void travelToThisStation(){
		Vector3 positionToTravel = quickTravelTransform.position;
		if (Physics.Raycast (quickTravelTransform.position, -transform.up, out hit, Mathf.Infinity, layer)) {
			positionToTravel = hit.point + transform.up * 0.3f;
		}
		player.transform.position = positionToTravel;
		player.transform.rotation = quickTravelTransform.rotation;
	}
	public void OnTriggerEnter(Collider col){
		if (!stationActivated && col.tag == "Player") {
			player = col.gameObject;
			if (stationAnimation!=null && animationName!="") {
				stationAnimation [animationName].speed = 1;
				stationAnimation.Play (animationName);
			}
			audioSource.PlayOneShot (enterAudioSound);
			gameObject.AddComponent<mapObjectInformation> ().name = "Beacon";
			mapObjectInformationManager = GetComponent<mapObjectInformation> ();
			mapObjectInformationManager.floorIndex = floorNumber;
			mapObjectInformationManager.getMapObjectInformation ();
			mapObjectInformationManager.getIconTypeIndexByName ("Beacon");
			mapObjectInformationManager.description = beaconDescription;
			stationActivated = true;
		}
	}
	public void OnTriggerExit(Collider col){

	}
}