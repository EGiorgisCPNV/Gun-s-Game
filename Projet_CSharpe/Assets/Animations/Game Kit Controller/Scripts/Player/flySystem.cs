using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class flySystem : MonoBehaviour {
	public bool flyModeEnabled;
	public IKFlyInfo IKInfo;
	public float flyForce;
	public float flyAirSpeed;
	public float flyAirControl;
	public float flyTurboSpeed;
	public float limbsMovementSpeed;
	public float limbsMovementSmooth;
	public bool showGizmo;
	inputManager input;
	playerController playerManager;
	int i;
	bool turboEnabled;

	void Start () {
		input = transform.parent.GetComponent<inputManager> ();
		playerManager = GetComponent<playerController> ();
		for (i = 0; i < IKInfo.IKGoals.Count; i++) {
			IKInfo.IKGoals [i].originalLocalPosition = IKInfo.IKGoals [i].position.localPosition;
		}
	}
	void Update(){
		if (flyModeEnabled) {
			if (input.checkInputButton ("Run", inputManager.buttonType.getKeyDown)) {
				enableOrDisableTurbo (true);
			}
			if (input.checkInputButton ("Run", inputManager.buttonType.getKeyUp)) {
				enableOrDisableTurbo (false);
			}
		}
	}
	void FixedUpdate() {
		if (flyModeEnabled) {
			for (i = 0; i < IKInfo.IKGoals.Count; i++) {
				float posTargetY = Mathf.Sin (Time.time * limbsMovementSpeed) * IKInfo.IKGoals [i].limbMovementAmount;
				IKInfo.IKGoals [i].position.position = Vector3.MoveTowards (IKInfo.IKGoals [i].position.position, IKInfo.IKGoals [i].position.position + posTargetY * transform.up, Time.deltaTime * limbsMovementSmooth);
			}
		}
	}
	public void resetLimbsPositons(){
		for (i = 0; i < IKInfo.IKGoals.Count; i++) {
			IKInfo.IKGoals [i].position.localPosition = IKInfo.IKGoals [i].originalLocalPosition;
		}
	}
	public void enableOrDisableFlyingMode(bool state){
		flyModeEnabled = state;
		playerManager.enableOrDisableFlyingMode (state,flyForce,flyAirSpeed,flyAirControl, flyTurboSpeed);
		GetComponent<IKSystem> ().flyingModeState (state, IKInfo);
	}
	public void enableOrDisableTurbo(bool state){
		turboEnabled = state;
		playerManager.enableOrDisableFlyModeTurbo (turboEnabled);
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
			for (i = 0; i < IKInfo.IKGoals.Count; i++) {
				Gizmos.color = Color.yellow;
				Gizmos.DrawSphere (IKInfo.IKGoals [i].position.position, 0.05f);
			}
			for (i = 0; i < IKInfo.IKHints.Count; i++) {
				Gizmos.color = Color.blue;
				Gizmos.DrawSphere (IKInfo.IKHints [i].position.position, 0.05f);
			}
		}
	}
	[System.Serializable]
	public class IKFlyInfo{
		public List<IKGoalsFlyPositions> IKGoals=new List<IKGoalsFlyPositions>();
		public List<IKHintsFlyPositions> IKHints=new List<IKHintsFlyPositions>();
	}
	[System.Serializable]
	public class IKGoalsFlyPositions{
		public string Name;
		public AvatarIKGoal limb;
		public Transform position;
		public float limbMovementAmount;
		[HideInInspector] public Vector3 originalLocalPosition;
	}
	[System.Serializable]
	public class IKHintsFlyPositions{
		public string Name;
		public AvatarIKHint limb;
		public Transform position;
	}
}