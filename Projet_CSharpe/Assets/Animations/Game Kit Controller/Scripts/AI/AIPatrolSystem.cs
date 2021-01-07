using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class AIPatrolSystem : MonoBehaviour {
	public AIWayPointPatrol patrolPath;
	public GameObject AIUsed;
	public float minDistanceToNextPoint;
	public float patrolSpeed;
	public bool useGeneralWaitTime;
	public float generalWaitTimeBetweenPoints;
	public bool moveBetweenPatrolsInOrder;
	public bool changeBetweenPointRandomly;
	[Range(0,10)] public int changeRandomlyProbability;
	public bool useTimeToChangeBetweenPointRandomly;
	public float fixedTimeToChangeBetweenPatrols;
	public bool useRandomTimeToChangePatrol;
	public Vector2 randomTimeLimits;
	bool patrolAssigned; 
	public bool paused;
	public bool showGizmo;
	public float gizmoRadius;
	Transform currentWayPoint;
	int currentPatrolIndex=0;
	int currentWaypointIndex=0;
	Coroutine movement;
	bool settingNextPoint;
	float lastTimeChanged;

	void Start () {
		if (patrolPath) {
			setClosestWayPoint ();
		}
	}
	void Update () {
		if (patrolAssigned && !settingNextPoint) {
			float distanceToPoint = Vector3.Distance (AIUsed.transform.position, currentWayPoint.position);
			if (distanceToPoint < minDistanceToNextPoint) {
				bool setRandomWayPoint = false;
				if (changeBetweenPointRandomly) {
					int changeOrNotBool = Random.Range (0, (changeRandomlyProbability+1));
					if (changeOrNotBool == 0) {
						setRandomWayPoint = true;
						//print ("random waypoint");
					}
				}
				if (setRandomWayPoint) {
					setNextRandomWaypoint ();
				} else {
					setNextWaypoint ();
					//print ("in order");
				}
			}
			if (changeBetweenPointRandomly) {
				if (useTimeToChangeBetweenPointRandomly) {
					if (useRandomTimeToChangePatrol) {


					} else {
						if (Time.time > fixedTimeToChangeBetweenPatrols + lastTimeChanged) {
							lastTimeChanged = Time.time;
							setNextRandomWaypoint ();
							//print ("random waypoint");
						}
					}
				}
			}
		}
	}
	public void pauseOrPlayPatrol(bool state){
		paused = state;
	}
	public Transform closestWaypoint(Vector3 currentPosition){
		Transform wayPoint;
		float distance = Mathf.Infinity;
		for (int i = 0; i < patrolPath.patrolList.Count; i++) {
			for (int j = 0; j < patrolPath.patrolList [i].wayPoints.Count; j++) {
				float currentDistance = Vector3.Distance (currentPosition, patrolPath.patrolList [i].wayPoints[j].position);
				if (currentDistance < distance) {
					distance = currentDistance;
					currentPatrolIndex = i;
					currentWaypointIndex = j;
				}
			}
		}
		wayPoint=patrolPath.patrolList [currentPatrolIndex].wayPoints [currentWaypointIndex];
		return wayPoint;
	}
	public void setNextWaypoint(){
		if(movement != null){
			StopCoroutine(movement);
		}
		movement = StartCoroutine (setNextWayPointCoroutine ());
	}
	IEnumerator setNextWayPointCoroutine(){
		AIUsed.SendMessage ("removeTarget", SendMessageOptions.DontRequireReceiver);
		settingNextPoint = true;
		if (useGeneralWaitTime) {
			yield return new WaitForSeconds (generalWaitTimeBetweenPoints);
		} else {
			yield return new WaitForSeconds (patrolPath.waitTimeBetweenPoints);
		}
		currentWaypointIndex++;
		if (currentWaypointIndex > patrolPath.patrolList[currentPatrolIndex].wayPoints.Count - 1) {
			currentWaypointIndex = 0;
			if (moveBetweenPatrolsInOrder) {
				currentPatrolIndex++;
				if (currentPatrolIndex > patrolPath.patrolList.Count - 1) {
					currentPatrolIndex = 0;
				}
			}
		}
		currentWayPoint = patrolPath.patrolList[currentPatrolIndex].wayPoints [currentWaypointIndex];
		AIUsed.SendMessage ("setTarget", currentWayPoint, SendMessageOptions.DontRequireReceiver);
		settingNextPoint = false;
	}
	public void setNextRandomWaypoint(){
		if(movement != null){
			StopCoroutine(movement);
		}
		movement = StartCoroutine (setNextRandomWayPointCoroutine ());
	}
	IEnumerator setNextRandomWayPointCoroutine(){
		AIUsed.SendMessage ("removeTarget", SendMessageOptions.DontRequireReceiver);
		settingNextPoint = true;
		if (useGeneralWaitTime) {
			yield return new WaitForSeconds (generalWaitTimeBetweenPoints);
		} else {
			yield return new WaitForSeconds (patrolPath.waitTimeBetweenPoints);
		}
		int currentWaypointIndexCopy = currentWaypointIndex;
		int currentPatrolIndexCopy = currentPatrolIndex;
		int checkBucle = 0;
		if (patrolPath.patrolList.Count > 1) {
			while (currentPatrolIndexCopy == currentPatrolIndex) {
				currentPatrolIndex = Random.Range (0, patrolPath.patrolList.Count);
				checkBucle++;
				if (checkBucle > 100) {
				//	print ("bucle error");
					break;
				}
			}
		}
		checkBucle = 0;
		while (currentWaypointIndexCopy == currentWaypointIndex) {
			currentWaypointIndex= Random.Range (0, patrolPath.patrolList[currentPatrolIndex].wayPoints.Count);
			checkBucle++;
			if (checkBucle > 100) {
				//print ("bucle error");
				break;
			}
		}
		//print ("Next patrol: " + (currentPatrolIndex+1) + " and next waypoint: " + (currentWaypointIndex+1));
		currentWayPoint = patrolPath.patrolList[currentPatrolIndex].wayPoints [currentWaypointIndex];
		AIUsed.SendMessage ("setTarget", currentWayPoint, SendMessageOptions.DontRequireReceiver);
		settingNextPoint = false;
	}
	public void setClosestWayPoint(){
		patrolAssigned = true;
		currentWayPoint = closestWaypoint (AIUsed.transform.position);
		AIUsed.SendMessage ("setTarget", currentWayPoint, SendMessageOptions.DontRequireReceiver);
		AIUsed.SendMessage ("setPatrolState", true);
		AIUsed.SendMessage ("setPatrolSpeed", patrolSpeed);
	}

	void OnDrawGizmos(){
		DrawGizmos();
	}
	void OnDrawGizmosSelected(){
		DrawGizmos();
	}
	void DrawGizmos(){
		if (showGizmo) {
			Gizmos.color = Color.red;
			Gizmos.DrawSphere (patrolPath.patrolList[currentPatrolIndex].wayPoints[currentWaypointIndex].transform.position, gizmoRadius);
		}
	}
}