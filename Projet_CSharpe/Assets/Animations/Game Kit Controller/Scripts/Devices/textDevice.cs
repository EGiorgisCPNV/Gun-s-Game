using UnityEngine;
using System.Collections;

public class textDevice : MonoBehaviour {
	bool check;
	bool moving;
	Vector3 originalPosition;
	Vector3 finalPosition;
	GameObject player;
	GameObject character;
	Quaternion originalRotation;

	void Start () {
		originalPosition = transform.position;
		originalRotation = transform.rotation;
		character = GameObject.Find ("Character");
	}

	void Update () {
		if (moving) {
			//move the device from a position to another, under the camera or in front of it
			transform.localPosition=Vector3.MoveTowards(transform.localPosition,finalPosition,Time.deltaTime*2);
			//if the final position is reached, disable the movement
			if(transform.localPosition==finalPosition){
				if(!check){
					//remove the parent of the device
					transform.parent=null;
					//put again the device in the original position
					transform.position=originalPosition;
					//set its original rotation
					transform.rotation=originalRotation;
					//enable again the colliders of the device
					transform.GetChild(1).GetComponent<Collider>().enabled=true;
					GetComponent<Collider>().enabled=true;
				}
				moving=false;
			}
		}
	}
	//enable or disable the device
	public void activateDevice(){
		check = !check;
		moving = true;
		//make visible the cursor, set the usingDeviceState to pause the player's controls and disable the camera
		character.GetComponent<menuPause> ().showOrHideCursor (check);
		character.GetComponent<menuPause> ().usingDeviceState (check);
		character.GetComponent<menuPause> ().changeCameraState(!check);
		if (check) {
			//disable the player controller script
			player.GetComponent<playerController> ().changeScriptState(!check);
			//disable the collider inside the device
			transform.GetChild(1).GetComponent<Collider>().enabled=false;
			//set the camera as the parent of the device 
			transform.parent=Camera.main.transform;
			//set the position of the device under the camera
			transform.localPosition = Vector3.forward-Vector3.up;
			//set the final position of the device, in front of the camera
			finalPosition = transform.localPosition+Vector3.up;
			//reset the rotation of the device
			transform.localRotation=Quaternion.identity;
			//disable the trigger of the device
			GetComponent<Collider>().enabled=false;
		} else {
			//enable the player controller again
			player.GetComponent<playerController> ().changeScriptState(!check);
			//set the final position under the camera
			finalPosition = transform.localPosition-Vector3.up;
			//hide the cursor again according to its state
			character.GetComponent<menuPause> ().showOrHideCursor (false);
		}
	}
	//check when the player is inside the trigger of the device
	void OnTriggerEnter(Collider col){
		if(col.GetComponent<Collider>().tag == "Player"){
			if(!player){
				player=col.GetComponent<Collider>().gameObject;
			}
		}
	}
}