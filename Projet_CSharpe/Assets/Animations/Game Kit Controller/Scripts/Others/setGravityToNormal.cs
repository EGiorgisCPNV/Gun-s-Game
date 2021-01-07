using UnityEngine;
using System.Collections;

public class setGravityToNormal : MonoBehaviour {
	public bool enter=false;
	public bool exit=false;

	//a simple script to set the gravity of the player and vehicles to the regular state, using triggers
	void OnTriggerExit(Collider col){
		if (exit) {
			disableGravity (col);
		}
	}
	void OnTriggerEnter(Collider col ){
		if (enter) {
			disableGravity (col);
		}

	}

	void disableGravity(Collider col){
		//if the player is not driving, stop the gravity power
		if(col.GetComponent<Collider>().tag == "Player"){
			if (!col.GetComponent<playerController> ().driving) {
				if (col.gameObject.GetComponent<changeGravity> ()) {
					col.gameObject.GetComponent<changeGravity> ().deactivateGravityPower ();
				}
			}
		}
		else {
			//if the player is driving, disable the gravity control in the vehicle
			if (col.gameObject.GetComponent<vehicleGravityControl> ()) {
				if (col.gameObject.GetComponent<vehicleGravityControl> ().gravityControlEnabled) {
					col.gameObject.GetComponent<vehicleGravityControl> ().deactivateGravityPower ();
				}
			}
		}
	}
}
