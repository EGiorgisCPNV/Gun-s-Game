using UnityEngine;
using System.Collections;
public class turretController : MonoBehaviour {
	public otherVehicleParts vehicleParts;
	public vehicleSettings settings;
	bool driving;
	IKDrivingSystem IKManager;
	inputActionManager actionManager;
	float horizontalAxis;
	float lookAngle;

	void Start () {
		IKManager = transform.parent.GetComponent<IKDrivingSystem> ();
	}
	void Update () {
		if (driving && settings.turretCanRotate) {
			horizontalAxis = actionManager.input.getMovementAxis ("keys").x;
			lookAngle -= horizontalAxis * settings.rotationSpeed;
			if (settings.rotationLimited) {
				lookAngle = Mathf.Clamp (lookAngle, -settings.clampTiltXTurret.x, settings.clampTiltXTurret.y);
			} 
			vehicleParts.chassis.transform.localRotation = Quaternion.Euler (0, -lookAngle, 0);
		}
	}
	//the player is getting on or off from the vehicle, so
	public void changeVehicleState(Vector3 nextPlayerPos){
		driving = !driving;
		if (!driving) {
			StartCoroutine (resetTurretRotation ());
			lookAngle = 0;
		}
		//set the same state in the IK driving and in the gravity control components
		IKManager.startOrStopVehicle (driving,vehicleParts.chassis,transform.up,nextPlayerPos);
	}
	//the vehicle has been destroyed, so disabled every component in it
	public void disableVehicle(){
		//disable the controller
		GetComponent<turretController> ().enabled = false;
	}
	//get the input manager component
	public void getInputActionManager(inputActionManager manager){
		actionManager = manager;
	}
	//reset the weapon rotation when the player gets off
	IEnumerator resetTurretRotation(){
		Quaternion currentBaseYRotation = vehicleParts.chassis.transform.localRotation;
		for (float t = 0; t < 1;) {
			t += Time.deltaTime * 3;
			vehicleParts.chassis.transform.localRotation = Quaternion.Slerp (currentBaseYRotation,Quaternion.identity, t);
			yield return null;
		}
	}
	[System.Serializable]
	public class otherVehicleParts{
		public GameObject chassis;
	}
	[System.Serializable]
	public class vehicleSettings{
		public bool turretCanRotate;
		public bool rotationLimited;
		public float rotationSpeed;
		public Vector2 clampTiltXTurret;
		public GameObject vehicleCamera;
	}
}
