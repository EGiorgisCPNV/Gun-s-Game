using UnityEngine;
using System.Collections;

public class setGravity : MonoBehaviour {
	public bool useWithPlayer;
	public bool useWithVehicles;
	public bool useWithAnyRigidbody;
	bool inside;
	GameObject player;
	GameObject character;
	grabObjects grabObjectsManager;

	//set a custom gravity for the player and the vehicles, in the direction of the arrow
	void Update () {
		GetComponent<Animation>().Play ("arrowAnim");
	}
	void OnTriggerEnter(Collider col){
		//if the player is not driving, stop the gravity power
		if (col.GetComponent<Collider> ().tag == "Player" && useWithPlayer) {
			if (!player) {
				player = col.gameObject;
			}
			if (!player.GetComponent<playerController> ().driving && !player.GetComponent<playerController>().jetPackEquiped && !player.GetComponent<playerController>().flyModeActive) {
				if (!character) {
					character = GameObject.Find ("Character");
				}
				character.GetComponent<playerStatesManager> ().checkPlayerStates ();
				if (col.gameObject.GetComponent<changeGravity> ()) {
					col.gameObject.GetComponent<changeGravity> ().changeOnTrigger (transform.up, transform.right);
				}
			}
		}  else if(col.GetComponent<Rigidbody>()){
			//if the player is driving, disable the gravity control in the vehicle
			if (col.gameObject.GetComponent<vehicleGravityControl> () && useWithVehicles) {
				if (col.gameObject.GetComponent<vehicleGravityControl> ().gravityControlEnabled) {
					col.gameObject.GetComponent<vehicleGravityControl> ().activateGravityPower (transform.TransformDirection (Vector3.up), transform.TransformDirection (Vector3.right));
				}
			} else {
				if (useWithAnyRigidbody) {
					if (!player) {
						player = GameObject.FindGameObjectWithTag ("Player");
						grabObjectsManager = player.GetComponent<grabObjects> ();
					}
					if (grabObjectsManager.objectHeld) {
						if (grabObjectsManager.objectHeld == col.gameObject) {
							grabObjectsManager.dropObject ();
						}
					}
					if (!col.gameObject.GetComponent<artificialObjectGravity> ()) {
						col.gameObject.AddComponent<artificialObjectGravity> ();
					} 
					col.gameObject.GetComponent<artificialObjectGravity> ().setCurrentGravity (transform.up);
				}
			}
		}
	}
}
