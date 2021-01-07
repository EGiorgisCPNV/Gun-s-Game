using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class AIHidePositionsManager : MonoBehaviour {
	public List<Transform> hidePositionList =new List<Transform>();
	public bool showGizmo;
	public float gizmoRadius;
	int i;

	void Start () {
		
	}
	void Update () {

	}
	//add a new waypoint
	public void addNewWayPoint(){
		Vector3 newPosition = transform.position;
		if (hidePositionList.Count > 0) {
			newPosition = hidePositionList [hidePositionList.Count-1].position + hidePositionList [hidePositionList.Count-1].forward;
		}
		GameObject newWayPoint = new GameObject ();
		newWayPoint.transform.SetParent (transform);
		newWayPoint.transform.position = newPosition;
		newWayPoint.name=(hidePositionList.Count+1).ToString();
		hidePositionList.Add(newWayPoint.transform);
		#if UNITY_EDITOR
		EditorUtility.SetDirty (GetComponent<AIHidePositionsManager>());
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
			for (i = 0; i < hidePositionList.Count; i++) {
				if (hidePositionList [i]) {
					Gizmos.color = Color.yellow;
					Gizmos.DrawSphere (hidePositionList [i].position, gizmoRadius);
					Gizmos.color = Color.white;
					Gizmos.DrawLine (hidePositionList [i].position, transform.position);
				}
			}
		}
	}
}