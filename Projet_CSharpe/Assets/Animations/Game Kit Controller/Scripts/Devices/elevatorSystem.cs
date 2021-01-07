using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class elevatorSystem : MonoBehaviour {
	public List<floorInfo> floors = new List<floorInfo> ();
	public int currentFloor;
	public float speed;
	public GameObject insideElevatorDoor;
	public GameObject elevatorSwitchPrefab;
	public bool addSwitchInNewFloors;
	public GameObject elevatorDoorPrefab;
	public bool addDoorInNewFloors;
	public bool moving;
	public bool doorsClosed=true;
	public bool showGizmo;	
	public Color gizmoLabelColor;
	GameObject player;
	GameObject pCamera;
	bool inside;
	int i;
	bool lockedElevator;
	bool closingDoors;
	Coroutine elevatorMovement;

	void Start(){
		
	}
	void Update () {
		//check if there is doors in the elevator to close them and start the elevator movement when they are closed
		if (closingDoors) {
			if (insideElevatorDoor) {
				if (insideElevatorDoor.GetComponent<doorSystem> ().doorState == doorSystem.doorCurrentState.closed) {
					closingDoors = false;
					checkElevatorMovement ();
				}
			} else {
				closingDoors = false;
				checkElevatorMovement ();
			}
		}
	}
	//the player has press the button move up, so increase the current floor count
	public void nextFloor(){
		getFloorNumberToMove (1);
	}
	//the player has press the button move down, so decrease the current floor count
	public void previousFloor(){
		getFloorNumberToMove (-1);
	}
	//move to the floor, according to the direction selected by the player
	void getFloorNumberToMove(int direction){
		//if the player is inside the elevator and it is not moving, then 
		if (inside && !moving) {
			//change the current floor to the next or the previous
			int floorIndex = currentFloor + direction;
			//check that the floor exists, and start to move the elevator to that floor position
			if (floorIndex < floors.Count && floorIndex >= 0) {
				openOrCloseElevatorDoors ();
				currentFloor = floorIndex;
				closingDoors = true;
				setPlayerParent (transform);
			}
		}
	}
	//move to the floor, according to the direction selected by the player
	public bool goToNumberFloor(int floorNumber){
		bool canMoveToFloor = false;
		//if the player is inside the elevator and it is not moving, then 
		if (inside && !moving) {
			//check that the floor exists, and start to move the elevator to that floor position
			if (floorNumber < floors.Count && floorNumber >= 0 && floorNumber!=currentFloor) {
				openOrCloseElevatorDoors ();
				currentFloor = floorNumber;
				closingDoors = true;
				setPlayerParent (transform);
				canMoveToFloor = true;
			}
		}
		return canMoveToFloor;
	}
	//when a elevator button is pressed, move the elevator to that floor
	void callElevator(GameObject button){
		for (i = 0; i < floors.Count; i++) {
			if (floors [i].floorButton == button) {
				lockedElevator = false;
				if (floors [currentFloor].outsideElevatorDoor) {
					if (floors [currentFloor].outsideElevatorDoor.GetComponent<doorSystem> ().locked) {
						lockedElevator = true;
					}
				}
				if(!lockedElevator){
					if (currentFloor != i) {
						if (!doorsClosed) {
							openOrCloseElevatorDoors ();
						}
						currentFloor = i;
						closingDoors = true;
					} else {
						openOrCloseElevatorDoors ();
					}
				}
			}
		}
	}
	//open or close the inside and outside doors of the elevator if the elevator has every of this doors
	void openOrCloseElevatorDoors(){
		if (insideElevatorDoor) {
			if (insideElevatorDoor.GetComponent<doorSystem> ().doorState == doorSystem.doorCurrentState.closed) {
				doorsClosed = false;
			} else {
				doorsClosed = true;
			}
			insideElevatorDoor.GetComponent<doorSystem> ().changeDoorsStateByButton ();
		}
		if (floors [currentFloor].outsideElevatorDoor) {
			floors [currentFloor].outsideElevatorDoor.GetComponent<doorSystem> ().changeDoorsStateByButton ();
		}
	}
	//stop the current elevator movement and start it again
	void checkElevatorMovement(){
		if (elevatorMovement!=null) {
			StopCoroutine (elevatorMovement);
		}
		elevatorMovement = StartCoroutine (moveElevator ());
	}
	IEnumerator moveElevator(){
		moving = true;
		//move the elevator from its position to the currentfloor
		Vector3 currentElevatorPosition = transform.localPosition;
		for (float t = 0; t < 1;) {
			t += Time.deltaTime * speed;
			transform.localPosition = Vector3.Lerp (currentElevatorPosition, floors [currentFloor].floorPosition.localPosition, t);
			yield return null;
		}
		//if the elevator reachs the correct floor, stop its movement, and deattach the player of its childs
		moving = false;
		setPlayerParent (null);
		openOrCloseElevatorDoors ();
	}
	void OnTriggerEnter(Collider col){
		//the player has entered in the elevator trigger, stored it and set the evelator as his parent
		if(col.GetComponent<Collider>().tag == "Player"){
			if (!col.GetComponent<playerController> ().driving) {
				if (!player) {
					player = col.gameObject;
					pCamera = player.GetComponent<playerController> ().pCamera;
				}
				inside = true;
				setPlayerParent (transform);
			}
		}
	}
	void OnTriggerExit(Collider col){
		//the player has gone of the elevator trigger, remove the parent from the player
		if(col.GetComponent<Collider>().tag == "Player"){
			inside = false;
			setPlayerParent (null);
			if (!doorsClosed) {
				openOrCloseElevatorDoors ();
			}
		}
	}
	//attach and disattch the player and the camera inside the elevator
	void setPlayerParent(Transform father){
		player.transform.SetParent (father);
		pCamera.transform.SetParent (father);
	}
	//add a new floor, with a switch and a door, if they are enabled to add them
	public void addNewFloor(){
		floorInfo newFloorInfo = new floorInfo ();
		GameObject newFloor = new GameObject ();
		newFloor.transform.SetParent (transform.parent);
		Vector3 newFloorLocalposition = Vector3.zero;
		if (floors.Count>0) {
			newFloorLocalposition = floors [floors.Count - 1].floorPosition.position + floors [floors.Count - 1].floorPosition.up * 5;
		}
		newFloor.transform.position = newFloorLocalposition;
		newFloor.name = "New Floor";
		newFloorInfo.name = newFloor.name;
		newFloorInfo.floorNumber = floors.Count;
		newFloorInfo.floorPosition = newFloor.transform;
		//add a switch
		if (addSwitchInNewFloors) {
			GameObject newSwitch = (GameObject)Instantiate (elevatorSwitchPrefab, Vector3.zero, Quaternion.identity);
			newSwitch.transform.SetParent (transform.parent);
			newSwitch.transform.position = newFloorLocalposition + newFloorInfo.floorPosition.forward * 10;
			newSwitch.name = "elevatorSwitch";
			newFloorInfo.floorButton = newSwitch;
			newSwitch.transform.SetParent (newFloor.transform);
		}
		//add a door
		if (addDoorInNewFloors) {
			GameObject newDoor = (GameObject)Instantiate (elevatorDoorPrefab, Vector3.zero, Quaternion.identity);
			newDoor.transform.SetParent (transform.parent);
			newDoor.transform.position = newFloorLocalposition + newFloorInfo.floorPosition.forward * 5;
			newDoor.name = "elevatorDoor";
			newFloorInfo.outsideElevatorDoor = newDoor;
			newDoor.transform.SetParent (newFloor.transform);
		}

		floors.Add (newFloorInfo);
		#if UNITY_EDITOR
		EditorUtility.SetDirty (GetComponent<elevatorSystem>());
		#endif
	}
	//draw every floor position and a line between floors
	void OnDrawGizmos(){
		DrawGizmos();
	}
	void OnDrawGizmosSelected(){
		DrawGizmos();
	}
	//draw the pivot and the final positions of every door
	void DrawGizmos(){
		if (showGizmo) {
			if (!Application.isPlaying) {
				for (i = 0; i < floors.Count; i++) {
					Gizmos.color = Color.yellow;
					if (floors [i].floorNumber == currentFloor) {
						Gizmos.color = Color.red;
					}
					Gizmos.DrawSphere (floors [i].floorPosition.position, 0.6f);
					if (i + 1 < floors.Count) {
						Gizmos.color = Color.yellow;
						Gizmos.DrawLine (floors [i].floorPosition.position, floors [i + 1].floorPosition.position);
					}
					if (floors [i].floorButton) {
						Gizmos.color = Color.blue;
						Gizmos.DrawLine (floors [i].floorButton.transform.position, floors [i].floorPosition.position);
						Gizmos.color = Color.green;
						Gizmos.DrawSphere (floors [i].floorButton.transform.position, 0.3f);
						if (floors [i].outsideElevatorDoor) {
							Gizmos.color = Color.white;
							Gizmos.DrawLine (floors [i].floorButton.transform.position, floors [i].outsideElevatorDoor.transform.position);
						}
					}
				}
			}
		}
	}
	[System.Serializable]
	public class floorInfo{
		public string name;
		public int floorNumber;
		public Transform floorPosition;
		public GameObject floorButton;
		public GameObject outsideElevatorDoor;
	}
}