using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class zipline : MonoBehaviour {
	public IKZiplineInfo IKZipline;
	public Transform finalPosition;
	public Transform initialPosition;
	public Transform movingTransform;
	public Transform middleLine;
	public Transform middleLinePivot;
	public float extraDistance;
	public bool usingZipline;
	public float speed;
	public float maxSpeed;
	public float minSpeed;
	public bool showGizmo;
	GameObject player;
	GameObject pCamera;
	GameObject character;
	Collider trigger;
	float originalSpeed;
	int i;
	bool stoppedByPlayer;
	inputManager input;

	void Start () {
		player = GameObject.Find ("Player Controller");
		pCamera = GameObject.Find ("Player Camera");
		character = player.transform.parent.gameObject;
		trigger = GetComponent<Collider> ();
		input = character.GetComponent<inputManager> ();
		originalSpeed = speed;
	}
	void Update () {
		//if the player is using the zipline, move his position from the current position to the final
		if (usingZipline) {
			if (input.getMovementAxis ("keys").y>0) {
				speed += Time.deltaTime * speed;
			}
			if (input.getMovementAxis ("keys").y<0) {
				speed -= Time.deltaTime * speed;
			}
			speed = Mathf.Clamp (speed, minSpeed, maxSpeed);
			movingTransform.transform.position = Vector3.MoveTowards (movingTransform.transform.position, finalPosition.transform.position, Time.deltaTime * speed);
			//if the player reachs the end of the zipline, disattach the player from the zipline and stop the movement
			if (Vector3.Distance (movingTransform.transform.position, finalPosition.transform.position) <= 0.1f) {
				usingZipline = false;
				changeZiplineState (usingZipline);
			}
		}
	}
	//function called when the player use the interaction button, to use the zipline
	public void activateDevice(){
		usingZipline = !usingZipline;
		//if the player press the interaction button while he stills using the zipline, stop his movement and released from the zipline
		if (!usingZipline) {
			stoppedByPlayer = true;
		}
		changeZiplineState (usingZipline);
	}
	public void changeZiplineState(bool state){
		//set the current state of the player in the IKSystem component, to enable or disable the ik positions
		player.GetComponent<IKSystem> ().ziplineState (state,IKZipline);
		//enable or disable the player's capsule collider
		player.GetComponent<Collider> ().isTrigger = state;
		//if the player is using the zipline, then
		if (state) {
			//disable the trigger of the zipline, to avoid the player remove this device from the compoenet usingDeviceSystem when he exits from its trigger
			trigger.enabled = false;
			//set the position of the object which moves throught the zipline
			movingTransform.transform.position = initialPosition.transform.position;
			//disable the player controller component
			player.GetComponent<playerController> ().changeScriptState (false);
			player.GetComponent<playerController> ().enableOrDisablePlayerControllerScript (false);
			//set that the player is using a device
			character.GetComponent<menuPause> ().usingDeviceState (state);
			//make the player and the camera a child of the object which moves in the zipline 
			player.transform.SetParent (movingTransform);
			player.transform.localRotation = Quaternion.identity;
			pCamera.transform.SetParent (movingTransform);
		} else {
			//the player stops using the zipline, so release him from it
			trigger.enabled = enabled;
			player.GetComponent<playerController> ().changeScriptState (true);
			player.GetComponent<playerController> ().enableOrDisablePlayerControllerScript (true);
			player.GetComponent<usingDevicesSytem> ().disableIcon ();
			character.GetComponent<menuPause> ().usingDeviceState (state);
			player.transform.SetParent (null);
			pCamera.transform.SetParent (null);
			movingTransform.transform.position = initialPosition.transform.position;
			//if the player has stopped his movement before he reaches the end of the zipline, add an extra force in the zipline direction
			if (stoppedByPlayer) {
				player.GetComponent<playerController> ().useJumpPlatform ((player.transform.forward - (player.transform.up*0.5f))*speed*2);
			}
			player.GetComponent<usingDevicesSytem> ().clearDeviceList ();
		}
		speed = originalSpeed;
		stoppedByPlayer = false;
	}
	//draw every ik position in the editor
	void OnDrawGizmos(){
		DrawGizmos();
	}
	void OnDrawGizmosSelected(){
		DrawGizmos();
	}
	void DrawGizmos(){
		if (showGizmo) {
			for (i = 0; i < IKZipline.IKGoals.Count; i++) {
				Gizmos.color = Color.yellow;
				Gizmos.DrawSphere (IKZipline.IKGoals [i].position.position, 0.1f);
			}
			for (i = 0; i < IKZipline.IKHints.Count; i++) {
				Gizmos.color = Color.blue;
				Gizmos.DrawSphere (IKZipline.IKHints [i].position.position, 0.1f);
			}
			Gizmos.color = Color.red;
			Gizmos.DrawSphere (IKZipline.bodyPosition.position, 0.1f);
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine (initialPosition.position, finalPosition.position);
			Gizmos.color = Color.red;
			Gizmos.DrawSphere (initialPosition.position, 0.2f);
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere (finalPosition.position, 0.2f);
			float scaleZ = Vector3.Distance (middleLine.position, finalPosition.position);
			middleLinePivot.transform.localScale = new Vector3 (1, 1, scaleZ + (scaleZ * extraDistance) + 0.5f);
			middleLine.LookAt (finalPosition);
		}
	}
	[System.Serializable]
	public class IKZiplineInfo{
		public List<IKGoalsZiplinePositions> IKGoals=new List<IKGoalsZiplinePositions>();
		public List<IKHintsZiplinePositions> IKHints=new List<IKHintsZiplinePositions>();
		public Transform bodyPosition;
	}
	[System.Serializable]
	public class IKGoalsZiplinePositions{
		public string Name;
		public AvatarIKGoal limb;
		public Transform position;
	}
	[System.Serializable]
	public class IKHintsZiplinePositions{
		public string Name;
		public AvatarIKHint limb;
		public Transform position;
	}
}