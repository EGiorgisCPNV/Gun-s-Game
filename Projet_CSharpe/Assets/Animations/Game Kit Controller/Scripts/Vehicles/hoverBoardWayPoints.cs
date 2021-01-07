using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class hoverBoardWayPoints : MonoBehaviour {
	public List<wayPointsInfo> wayPoints =new List<wayPointsInfo> ();
	public GameObject wayPointElement;
	public bool inside;
	public float movementSpeed;
	public bool moveInOneDirection;
	public float extraRotation;
	public float forceAtEnd;
	public float railsOffset;
	public float extraScale;
	public float triggerRadius;
	public bool showGizmo;
	public float gizmoRadius;
	int i;
	GameObject player;
	GameObject currentVehicle;
	GameObject currentvehicleCamera;
	Coroutine movement;
	public bool moving;

	void OnTriggerEnter(Collider col){
		if(col.gameObject.tag=="Player" && !inside){
			if (!player) {
				player = col.gameObject;
			} 
			if (player) {
				if (player.GetComponent<playerController> ().driving) {
					if (player.GetComponent<playerController> ().getCurrentVehicle().GetComponent<hoverBoardController> ()) {
						currentVehicle = player.GetComponent<playerController> ().getCurrentVehicle();
						currentvehicleCamera = currentVehicle.GetComponent<hoverBoardController> ().settings.vehicleCamera;
						pickOrReleaseVehicle (true,false);
						if (movement != null) {
							StopCoroutine (movement);
						}
						movement = StartCoroutine (moveThroughWayPoints ());
					}
				}
			}
		}
	}
	void OnTriggerExit(Collider col){
		if(col.gameObject.tag=="Player" && inside && !moving){
			pickOrReleaseVehicle (false,false);
		}
	}
	public void pickOrReleaseVehicle(bool state, bool auto){
		inside = state;
		currentVehicle.SendMessage ("enterOrExitFromWayPoint", inside);
		currentVehicle.SendMessage ("receiveWayPoints", GetComponent<hoverBoardWayPoints> ());
		currentvehicleCamera.SendMessage ("startOrStopFollowVehiclePosition", !inside);
		if (!inside) {
			if (movement != null) {
				StopCoroutine (movement);
			}
			if (auto) {
				currentVehicle.GetComponent<Rigidbody>().AddForce ( currentVehicle.transform.forward  * currentVehicle.GetComponent<Rigidbody>().mass * forceAtEnd, ForceMode.Impulse);
			}
			currentVehicle = null;
			currentvehicleCamera = null;
		}
	}
	IEnumerator moveThroughWayPoints(){
		moving = true;
		float closestDistance = Mathf.Infinity;
		int index = -1;
		for (i = 0; i < wayPoints.Count; i++) {
			if (Vector3.Distance (wayPoints [i].wayPoint.position, currentVehicle.transform.position) < closestDistance) {
				closestDistance = Vector3.Distance (wayPoints [i].wayPoint.position, currentVehicle.transform.position);
				index = i;
			}
		}
		Vector3 heading=currentVehicle.transform.position-wayPoints[index].wayPoint.position;
		float distance = heading.magnitude;
		Vector3 directionToPoint = heading / distance;
		print ("player: "+directionToPoint + "-direction: "+wayPoints [index].direction.forward);
		//check if the vectors point in the same direction or not
		float angle=Vector3.Dot (directionToPoint, wayPoints [index].direction.forward);
		print (angle);
		if (angle < 0) {
			print ("different direction");
		}
		//if the vectors point in different directions, it means that the player is close to a waypoint in the opposite forward direction of the hoverboard waypoints,
		//so increase the index in 1 to move the player to the correct waypoint position, according to the forward direction used to the waypoints
		if (angle > 0) {
			print ("same direcion");
			index++;
			if (index > wayPoints.Count - 1) {
				StopCoroutine (movement);
			}
		}

		List<Transform> currentPath =new List<Transform>();
		for (i = index; i < wayPoints.Count; i++) {
			currentPath.Add (wayPoints [i].direction);
		}
		if (index - 1 >= 0) {
			index--;
		} else {
			index = 0;
		}
		Vector3 extraYRotation = wayPoints [index].direction.eulerAngles+currentVehicle.transform.up*extraRotation;
		Quaternion rot = Quaternion.Euler (extraYRotation);
		foreach (Transform transformPath in  currentPath) {
			Vector3 pos = transformPath.transform.position;
			if (transformPath == currentPath [currentPath.Count - 1]) {
				pos += transformPath.forward*2;
			}
			while (Vector3.Distance (currentVehicle.transform.position, pos) > .01f) {
				currentVehicle.transform.position = Vector3.MoveTowards (currentVehicle.transform.position, pos, Time.deltaTime*movementSpeed);
				currentVehicle.transform.rotation = Quaternion.Slerp (currentVehicle.transform.rotation, rot, Time.deltaTime*movementSpeed);
				currentvehicleCamera.transform.position = Vector3.MoveTowards (currentvehicleCamera.transform.position, pos, Time.deltaTime*movementSpeed);
				yield return null;
			}
			extraYRotation =transformPath.eulerAngles+currentVehicle.transform.up*extraRotation;
			rot = Quaternion.Euler (extraYRotation);
		}
		moving = false;
		pickOrReleaseVehicle (false,true);
	}
	public void addNewWayPoint(){
		Vector3 newPosition = transform.position;
		if (wayPoints.Count > 0) {
			newPosition = wayPoints [wayPoints.Count-1].wayPoint.position + wayPoints [wayPoints.Count-1].wayPoint.forward;
		}
		GameObject newWayPoint = (GameObject)Instantiate (wayPointElement, newPosition, Quaternion.identity);
		newWayPoint.transform.SetParent (transform);
		newWayPoint.name=(wayPoints.Count+1).ToString();
		wayPointsInfo newWayPointInfo=new wayPointsInfo();
		newWayPointInfo.Name=newWayPoint.name;
		newWayPointInfo.wayPoint=newWayPoint.transform;
		newWayPointInfo.direction=newWayPoint.transform.GetChild(0);
		newWayPointInfo.trigger=newWayPoint.GetComponentInChildren<CapsuleCollider>();
		newWayPointInfo.railMesh = newWayPoint.GetComponentInChildren<MeshRenderer> ().gameObject;
		wayPoints.Add(newWayPointInfo);
		#if UNITY_EDITOR
				EditorUtility.SetDirty (GetComponent<hoverBoardWayPoints>());
		#endif
	}
	void OnDrawGizmos(){
		DrawGizmos();
	}
	void OnDrawGizmosSelected(){
		DrawGizmos();
	}
	//draw the pivot and the final positions of every door
	void DrawGizmos(){
		//&& !Application.isPlaying
		if (showGizmo ) {
			for (i = 0; i < wayPoints.Count; i++) {
				if (wayPoints [i].wayPoint && wayPoints[i].direction) {
					Gizmos.color = Color.yellow;
					Gizmos.DrawSphere (wayPoints [i].wayPoint.position, gizmoRadius);
					if (i + 1 < wayPoints.Count) {
						Gizmos.color = Color.white;
						Gizmos.DrawLine (wayPoints [i].wayPoint.position, wayPoints [i + 1].wayPoint.position);
						wayPoints [i].direction.LookAt (wayPoints [i + 1].wayPoint.position);
						float scaleZ = Vector3.Distance (wayPoints [i].wayPoint.position, wayPoints [i + 1].wayPoint.position);
						wayPoints [i].direction.localScale = new Vector3 (1, 1, scaleZ + scaleZ * extraScale);
						Gizmos.color = Color.green;
						Gizmos.DrawLine (wayPoints [i].wayPoint.position, wayPoints [i].wayPoint.position + wayPoints [i].direction.forward);
					}
					if (i == wayPoints.Count-1 && (i-1)>=0 && i!=0) {
						wayPoints [i].direction.rotation=Quaternion.LookRotation (wayPoints [i].wayPoint.position-wayPoints [i-1].wayPoint.position);
						Gizmos.color = Color.green;
						Gizmos.DrawLine (wayPoints [i].direction.position, wayPoints [i].direction.position + wayPoints [i].direction.forward);
					}
					if (i == wayPoints.Count - 1) {
						wayPoints [i].direction.localScale = Vector3.one;
					}
					wayPoints [i].trigger.radius = triggerRadius;
					wayPoints [i].railMesh.transform.localPosition = new Vector3 (wayPoints [i].railMesh.transform.localPosition.x, railsOffset, wayPoints [i].railMesh.transform.localPosition.z);
				}
			}
		}
	}
	[System.Serializable]
	public class wayPointsInfo{	
		public string Name;
		public Transform wayPoint;
		public Transform direction;
		public CapsuleCollider trigger;
		public GameObject railMesh;
	}
}