using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class doorSystem : MonoBehaviour {
	public List<singleDoorInfo> doorsInfo = new List<singleDoorInfo> ();
	public doorMovementType movementType;
	public AudioClip openSound;
	public AudioClip closeSound;
	public doorType doorTypeInfo;
	public doorCurrentState doorState;
	public bool locked;
	public float openSpeed;
	public GameObject hologram;
	public bool showGizmo;
	//set if the door is rotated or translated
	public enum doorMovementType{translate, rotate};
	//set how the door is opened, using triggers, a button close to the door, using a hologram to press the interaction button close to the door 
	//and by shooting the door
	public enum doorType {trigger, button, hologram, shoot};
	//set the initial state of the door, opened or closed
	public enum doorCurrentState {closed, opened};
	bool enter;
	bool exit;
	[HideInInspector] public bool moving;
	int doorsNumber;
	int doorsInPosition=0;
	AudioSource soundSource;
	int i;

	void Start () {
		//get the original rotation and position of every panel of the door
		for (i=0; i<doorsInfo.Count; i++) {
			doorsInfo[i].originalPosition=doorsInfo[i].doorMesh.transform.localPosition;
			doorsInfo[i].originalRotation=doorsInfo[i].doorMesh.transform.localRotation;
		}
		//total number of panels
		doorsNumber = doorsInfo.Count;
		soundSource = GetComponent<AudioSource> ();
	}
	void Update () {
		//if the player enters or exits the door, move the door
		if ((enter || exit)) {
			moving = true;
			//for every panel in the door
			doorsInPosition=0;
			for (i=0; i<doorsInfo.Count; i++) {
				//if the panels are translated, then
				if (movementType == doorMovementType.translate) {
					//if the curren position of the panel is different from the target position, then
					if (doorsInfo [i].doorMesh.transform.localPosition != doorsInfo [i].currentTargetPosition) {
						//translate the panel
						doorsInfo [i].doorMesh.transform.localPosition =
							Vector3.MoveTowards (doorsInfo [i].doorMesh.transform.localPosition, doorsInfo [i].currentTargetPosition, Time.deltaTime * openSpeed);
					} 
					//if the panel has reached its target position, then
					else {
						doorsInfo [i].doorMesh.transform.localPosition=doorsInfo [i].currentTargetPosition;
						//increase the number of panels that are in its target position
						doorsInPosition++;
					}
				} 
				//if the panels are rotated, then
				else {
					//if the curren rotation of the panel is different from the target rotation, then
					if (doorsInfo [i].doorMesh.transform.localRotation != doorsInfo [i].currentTargetRotation) {
						//rotate from its current rotation to the target rotation
						doorsInfo [i].doorMesh.transform.localRotation = Quaternion.RotateTowards (doorsInfo [i].doorMesh.transform.localRotation, doorsInfo [i].currentTargetRotation, Time.deltaTime * openSpeed * 10);
					} 
					//if the panel has reached its target rotation, then
					else {
						//increase the number of panels that are in its target rotation
						doorsInPosition++;
						if (exit) {
							doorsInfo [i].doorMesh.transform.localRotation = Quaternion.identity;
						}
					}
				}
			}
			//if all the panels in the door are in its target position/rotation
			if (doorsInPosition == doorsNumber) {
				//if the door was opening, then the door is opened
				if (enter) {
					doorState = doorCurrentState.opened;
				}
				//if the door was closing, then the door is closed
				if (exit) {
					doorState = doorCurrentState.closed;
				}
				//reset the parameters
				enter = false;
				exit = false;
				doorsInPosition = 0;
				moving = false;
			}
		}
	}
	//if the door was unlocked, locked it
	public void lockDoor(){
		if (doorState==doorCurrentState.opened) {
			closeDoors();
		}
		//if the door is not a hologram type, then close the door
		if (doorTypeInfo != doorType.hologram && doorTypeInfo != doorType.button) {
			
		} else {
			//else, lock the hologram, so the door is closed
			if (hologram) {
				hologram.GetComponent<hologramDoor> ().lockHologram ();
			}
		}
		if(GetComponent<mapObjectInformation>()){
			GetComponent<mapObjectInformation>().addMapObject("Locked Door");
		}
		locked = true;
	}
	//if the door was locked, unlocked it
	public void unlockDoor(){
		locked = false;
		//if the door is not a hologram type, then open the door
		if (doorTypeInfo != doorType.hologram && doorTypeInfo != doorType.button) {
			changeDoorsStateByButton ();
		} else {
			//else, unlock the hologram, so the door can be opened when the hologram is used
			if (hologram) {
				hologram.GetComponent<hologramDoor> ().unlockHologram ();
			}
		}
		if(GetComponent<mapObjectInformation>()){
			GetComponent<mapObjectInformation>().addMapObject("Unlocked Door");
		}
	}
	//a button to open the door calls this function, so
	public void changeDoorsStateByButton(){
		//if the door is opened, close it
		// && !moving
		if (doorState==doorCurrentState.opened) {
			closeDoors();
		} 
		//if the door is closed, open it
		if (doorState==doorCurrentState.closed) {
			openDoors ();
		}
	}
	//open the doors
	void openDoors(){
		if (!locked) {
			enter = true;
			exit = false;
			//for every panel in the door, set that their target rotation/position are their opened/rotated positions
			for (i=0; i<doorsInfo.Count; i++) {
				if(movementType==doorMovementType.translate){
					doorsInfo [i].currentTargetPosition = doorsInfo [i].openedPosition.transform.localPosition;
				}
				else{
					doorsInfo [i].currentTargetRotation = doorsInfo [i].rotatedPosition.transform.localRotation;
				}
			}
			//play the open sound
			soundSource.PlayOneShot (openSound);
		}
	}
	//close the doors
	void closeDoors(){
		if (!locked) {
			enter = false;
			exit = true;
			//for every panel in the door, set that their target rotation/position are their original positions/rotations
			for (i=0; i<doorsInfo.Count; i++) {
				if(movementType==doorMovementType.translate){
					doorsInfo [i].currentTargetPosition = doorsInfo [i].originalPosition;
				}
				else{
					doorsInfo [i].currentTargetRotation = doorsInfo [i].originalRotation;
				}
			}
			//play the close sound
			soundSource.PlayOneShot (closeSound);
		}
	}
	void OnTriggerEnter(Collider col){
		//the player has entered in the door trigger, check if this door is a trigger door or a hologram door opened
		if(col.GetComponent<Collider>().tag == "Player" && (doorTypeInfo==doorType.trigger || (doorTypeInfo==doorType.hologram && doorState==doorCurrentState.opened))){
			openDoors();
		}
	}
	void OnTriggerExit(Collider col){
		//the player has gone of the door trigger, check if this door is a trigger door, a shoot door, or a hologram door and it is opened, to close it
		if(col.GetComponent<Collider>().tag == "Player" && (doorTypeInfo==doorType.trigger || (doorTypeInfo==doorType.shoot && doorState==doorCurrentState.opened))
			|| (doorTypeInfo==doorType.hologram && doorState==doorCurrentState.opened)){
			closeDoors();
		}
	}
	//the player has shooted this door, so
	void doorsShooted(GameObject projectile){
		//check if the object is a player's projectile
		if (projectile.GetComponent<powerProjectile> ()) {
			//and if the door is closed and a shoot type
			if (doorState==doorCurrentState.closed && !moving && doorTypeInfo==doorType.shoot) {
				//then, open the door
				openDoors ();
			}
		}
	}
	void OnDrawGizmos(){
		DrawGizmos();
	}
	void OnDrawGizmosSelected(){
		DrawGizmos();
	}
	//draw the pivot and the final positions of every door
	void DrawGizmos(){
		if (showGizmo) {
			//if (!Application.isPlaying) {
			for (i = 0; i < doorsInfo.Count; i++) {
				Gizmos.color = Color.yellow;
				Gizmos.DrawSphere (doorsInfo [i].doorMesh.transform.position, 0.3f);
				if (doorsInfo [i].openedPosition) {
					Gizmos.color = Color.green;
					Gizmos.DrawSphere (doorsInfo [i].openedPosition.transform.position, 0.3f);
					Gizmos.color = Color.white;
					Gizmos.DrawLine (doorsInfo [i].doorMesh.transform.position,doorsInfo [i].openedPosition.transform.position);
				}
				if (doorsInfo [i].rotatedPosition) {
					Gizmos.color = Color.green;
					//Gizmos.DrawCube (doorsInfo [i].rotatedPosition.transform.position, Vector3.one*0.7f);
					Matrix4x4 cubeTransform = Matrix4x4.TRS(doorsInfo [i].rotatedPosition.transform.position, doorsInfo [i].rotatedPosition.transform.rotation, Vector3.one*1.2f);
					Matrix4x4 oldGizmosMatrix = Gizmos.matrix;
					Gizmos.matrix *= cubeTransform;
					Gizmos.DrawCube(Vector3.zero, Vector3.one);
					Gizmos.matrix = oldGizmosMatrix;
					Gizmos.color = Color.white;
					Gizmos.DrawLine (doorsInfo [i].doorMesh.transform.position,doorsInfo [i].rotatedPosition.transform.position);
				}
			}
			//}
		}
	}
	//a clas to store every panel that make the door, the position to move when is opened or the object which has the rotation that the door has to make
	//and fields to store the current and original rotation and position
	[System.Serializable]
	public class singleDoorInfo{
		public GameObject doorMesh;
		public GameObject openedPosition;
		public GameObject rotatedPosition;
		[HideInInspector] public Vector3 originalPosition;
		[HideInInspector] public Quaternion originalRotation;
		[HideInInspector] public Vector3 currentTargetPosition;
		[HideInInspector] public Quaternion currentTargetRotation;
	}
}