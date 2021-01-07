using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class waypointPlatform : MonoBehaviour {
	public List<Transform> wayPoints =new List<Transform>();
	public Transform waypointsParent;
	public bool repeatWaypoints;
	public bool moveInCircles;
	public bool stopIfPlayerOutSide;
	public float waitTimeBetweenPoints;
	public float movementSpeed;
	public bool movingForward=true;
	public bool showGizmo;
	public Color gizmoLabelColor=Color.black;
	public float gizmoRadius;
	List<Transform> forwardPath =new List<Transform>();
	List<Transform> inversePath = new List<Transform>();
	List<Transform> currentPath = new List<Transform>();
	GameObject player;
	GameObject pCamera;
	Coroutine movement;
	Transform currentWaypoint;
	int currentPlatformIndex;
	int i;
	bool inside;

	void Start () {
		forwardPath = new List<Transform> (wayPoints);
		inversePath = new List<Transform> (wayPoints);
		inversePath.Reverse ();
		if (!stopIfPlayerOutSide) {
			checkMovementCoroutine (true);
		}
	}
	void Update () {
		
	}
	void OnTriggerEnter(Collider col){
		//if the player enters inside the platform trigger, then
		if(col.gameObject.tag=="Player" && !inside){
			//store him
			if (!player) {
				player = col.gameObject;
				pCamera = player.GetComponent<playerController> ().pCamera;
			} 
			//if he is not driving, then attach the player and the camera inside the platform
			if (!player.GetComponent<playerController> ().driving) {
				setPlayerParent (transform);
				//if the platform stops when the player exits from it, then restart its movement
				if (stopIfPlayerOutSide) {
					checkMovementCoroutine (true);
				}
				inside = true;
			}
		}
	}
	void OnTriggerExit(Collider col){
		//if the player exits, then disattach the player
		if(col.gameObject.tag=="Player" && inside){
			setPlayerParent (null);
			//if the platform stops when the player exits from it, stop the platform
			if (stopIfPlayerOutSide) {
				checkMovementCoroutine (false);
			}
			inside = false;
		}
	}
	void setPlayerParent(Transform father){
		player.transform.SetParent (father);
		pCamera.transform.SetParent (father);
	}
	//stop the platform coroutine movement and play again
	public void checkMovementCoroutine(bool play){
		if(movement != null){
			StopCoroutine(movement);
		}
		if (play) {
			movement = StartCoroutine (moveThroughWayPoints ());
		}
	}
	IEnumerator moveThroughWayPoints(){
		currentPath.Clear ();
		//if the platform moves from waypoint to waypoint and it starts again, then
		if (moveInCircles) {
			//from the current waypoint to the last of them, add these waypoints
			for (i = currentPlatformIndex; i < forwardPath.Count; i++) {
				currentPath.Add (forwardPath [i]);
			}
		} else {
			//else, if only moves from the first waypoint to the last and then stop, then
			//if the platform moves between waypoins in the order list
			if (movingForward) {
				//from the current waypoint to the last of them, add these waypoints
				for (i = currentPlatformIndex; i < forwardPath.Count; i++) {
					currentPath.Add (forwardPath [i]);
				}
			} else {
				//from the current waypoint to the first of them, add these waypoints, making the reverse path
				for (i = currentPlatformIndex; i < inversePath.Count; i++) {
					currentPath.Add (inversePath [i]);
				}
			}
		}
		//if the current path to move has waypoints, then
		if (currentPath.Count > 0) {
			//move between every waypoint
			foreach (Transform point in  currentPath) {
				//wait the amount of time configured
				yield return new WaitForSeconds (waitTimeBetweenPoints);
				Vector3 pos = point.position;
				Quaternion rot = point.rotation;
				currentWaypoint = point;
				//while the platform moves from the previous waypoint to the next, then displace it
				while (Vector3.Distance (transform.position, pos) > .01f) {
					transform.position = Vector3.MoveTowards (transform.position, pos, Time.deltaTime * movementSpeed);
					transform.rotation = Quaternion.Slerp (transform.rotation, rot, Time.deltaTime * movementSpeed);
					yield return null;
				}
				//when the platform reaches the next waypoint
				currentPlatformIndex++;
				if (currentPlatformIndex > wayPoints.Count - 1) {
					currentPlatformIndex = 0;
					movingForward = !movingForward;
				}
			}
			//if the platform moves in every moment, then repeat the path
			if (repeatWaypoints) {
				checkMovementCoroutine (true);
			}
		} else {
			//else, stop the movement
			checkMovementCoroutine (false);
		}
	}
	//add a new waypoint
	public void addNewWayPoint(){
		Vector3 newPosition = transform.position;
		if (wayPoints.Count > 0) {
			newPosition = wayPoints [wayPoints.Count-1].position + wayPoints [wayPoints.Count-1].forward;
		}
		GameObject newWayPoint = new GameObject ();
		newWayPoint.transform.SetParent (waypointsParent);
		newWayPoint.transform.position = newPosition;
		newWayPoint.name=(wayPoints.Count+1).ToString();
		wayPoints.Add(newWayPoint.transform);
		#if UNITY_EDITOR
		EditorUtility.SetDirty (GetComponent<waypointPlatform>());
		#endif
	}
	void OnDrawGizmos(){
		DrawGizmos();
	}
	void OnDrawGizmosSelected(){
		DrawGizmos();
	}
	void DrawGizmos(){
		if (showGizmo) {
			for (i = 0; i < wayPoints.Count; i++) {
				if (wayPoints [i]) {
					Gizmos.color = Color.yellow;
					Gizmos.DrawSphere (wayPoints [i].position, gizmoRadius);
					if (i + 1 < wayPoints.Count) {
						Gizmos.color = Color.white;
						Gizmos.DrawLine (wayPoints [i].position, wayPoints [i + 1].position);
					}
					if (i == wayPoints.Count - 1 && moveInCircles) {
						Gizmos.color = Color.white;
						Gizmos.DrawLine (wayPoints [i].position, wayPoints [0].position);
					}
					if (currentWaypoint) {
						Gizmos.color = Color.red;
						Gizmos.DrawSphere (currentWaypoint.position, gizmoRadius);
					}
				}
			}
		}
	}
}