using UnityEngine;
using System.Collections;
public class jumpPlatform : MonoBehaviour {
	public float jumpForce;
	public string platformAnimation;
	public bool useWithPlayer;
	public bool useWithVehicles;
	public bool useWithAnyRigidbody;
	public bool useKeyToJumpWithPlayer;
	public bool useKeyToJumpWithVehicles;
	GameObject player;
	GameObject character;
	grabObjects grabObjectsManager;

	void Start () {
	
	}
	void Update () {
		//play the platform animation
		GetComponent<Animation>().Play (platformAnimation);
	}
	void OnTriggerEnter(Collider col){
		//if the player is inside the trigger and the platform can be used with him, then
		if (col.gameObject.tag == "Player" && useWithPlayer) {
			//store the player
			if (!player) {
				player = col.gameObject;
			}
			if (player) {
				//if the player is not driving
				if (!player.GetComponent<playerController> ().driving) {
					if (!character) {
						character = GameObject.Find ("Character");
					}
					//the platform increase the jump force in the player, and only the jump button will make the player to jump
					if (useKeyToJumpWithPlayer) {
						player.GetComponent<playerController> ().useJumpPlatformWithKeyButton (true, jumpForce);
					} else {
						//else make the player to jump
						character.GetComponent<playerStatesManager> ().checkPlayerStates ();
						player.GetComponent<playerController> ().useJumpPlatform (jumpForce * transform.up);
					}
				}
			}
		} 
		//if any other rigidbody enters the trigger, then
		else if (col.gameObject.GetComponent<Rigidbody> ()) {
			//if a vehicle enters inside the trigger and the platform can be used with vehicles, then
			if (col.gameObject.tag == "device" && col.gameObject.GetComponent<vehicleHUDManager> () && useWithVehicles) {
				//the platform increases the jump force in the vehicle, and only the jump button will make the vehicle to jump
				if (useKeyToJumpWithVehicles) {
					col.gameObject.GetComponent<vehicleHUDManager> ().useJumpPlatformWithKeyButton (true, jumpForce);
				} else {
					//else make the vehicle to jump
					col.gameObject.GetComponent<vehicleHUDManager> ().useJumpPlatform (jumpForce * transform.up);
				}
			} else {
				//if any other type of rigidbody enters the trigger, then
				if (useWithAnyRigidbody) {
					if (!player) {
						player = GameObject.FindGameObjectWithTag ("Player");
					}
					//if the object is being carried by the player, make him drop it
					if (player && !grabObjectsManager) {
						grabObjectsManager = player.GetComponent<grabObjects> ();
					}
					if (grabObjectsManager) {
						if (grabObjectsManager.objectHeld) {
							if (grabObjectsManager.objectHeld == col.gameObject) {
								grabObjectsManager.dropObject ();
							}
						}
					}
					//add force to that rigidbody
					col.gameObject.GetComponent<Rigidbody> ().AddForce (transform.up * (jumpForce / 2) * col.gameObject.GetComponent<Rigidbody> ().mass, ForceMode.Impulse);
				}
			}
		}
	}
	void OnTriggerExit(Collider col){
		//restore the original jump force in the player of the vehicle is the jump button is needed
		if(col.gameObject.tag == "Player"){
			if (useKeyToJumpWithPlayer) {
				player.GetComponent<playerController> ().useJumpPlatformWithKeyButton (false, jumpForce);
			}
		}
		if (col.gameObject.tag == "device") {
			if (useKeyToJumpWithVehicles) {
				col.gameObject.GetComponent<vehicleHUDManager> ().useJumpPlatformWithKeyButton (false, jumpForce);
			}
		}
	}
}