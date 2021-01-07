using UnityEngine;
using System.Collections;

public class sphereController : MonoBehaviour
{
	public otherVehicleParts vehicleParts;
	public vehicleSettings settings;
	public float currentSpeed;
	public bool anyOnGround;
	bool driving;
	bool jump;
	bool moving;
	bool usingBoost;
	bool usingGravityControl;
	bool rotating;
	int i, j;
	int collisionForceLimit = 5;
	float boostInput = 1;
	float horizontalAxis;
	float verticalAxis;
	float originalJumpPower;
	Vector3 moveInput;

	Vector3 normal;
	Rigidbody mainRigidbody;
	IKDrivingSystem IKManager;
	inputActionManager actionManager;
	vehicleCameraController vCamera;
	vehicleHUDManager hudManager;
	vehicleGravityControl gravityManager;

	void Start ()
	{
		mainRigidbody = GetComponent<Rigidbody> ();
		vCamera = settings.vehicleCamera.GetComponent<vehicleCameraController> ();
		hudManager = GetComponent<vehicleHUDManager> ();
		IKManager = transform.parent.GetComponent<IKDrivingSystem> ();
		gravityManager = GetComponent<vehicleGravityControl> ();
		originalJumpPower = settings.jumpPower;
	}

	void Update ()
	{
		if (driving && !usingGravityControl) {
			//get the current values from the input manager, keyboard and touch controls
			horizontalAxis = actionManager.input.getMovementAxis ("keys").x;
			verticalAxis = actionManager.input.getMovementAxis ("keys").y;
			if (settings.canJump && actionManager.getActionInput ("Jump")) {
				if (anyOnGround) {
					mainRigidbody.AddForce (normal * mainRigidbody.mass * settings.jumpPower);
				}
			}
			//boost input
			if (settings.canUseBoost && actionManager.getActionInput ("Enable Turbo")) {
				usingBoost = true;
				//set the camera move away action
				vCamera.usingBoost (true, "Boost");
			}
			//stop boost input
			if (actionManager.getActionInput ("Disable Turbo")) {
				usingBoost = false;
				//disable the camera move away action
				vCamera.usingBoost (false, "Boost");
				//disable the boost particles
				usingBoosting ();
				boostInput = 1;
			}
			//if the boost input is enabled, check if there is energy enough to use it
			if (usingBoost) {
				//if there is enough energy, enable the boost
				if (hudManager.useBoost (moving)) {
					boostInput = settings.maxBoostMultiplier;
					usingBoosting ();
				} 
				//else, disable the boost
				else {
					usingBoost = false;
					//if the vehicle is not using the gravity control system, disable the camera move away action
					if (!gravityManager.powerActive) {
						vCamera.usingBoost (false, "Boost");
					}
					usingBoosting ();
					boostInput = 1;
				}
			}
			//set the current speed in the HUD of the vehicle
			hudManager.getSpeed (currentSpeed, settings.maxForwardSpeed);

		} 
		//else, set the input values to 0
		else {
			horizontalAxis = 0;
			verticalAxis = 0;
		}
		moving = verticalAxis != 0;

		moveInput = verticalAxis * settings.vehicleCamera.transform.forward + horizontalAxis * settings.vehicleCamera.transform.right;	
		Vector3 force = moveInput * settings.moveSpeedMultiplier * boostInput;
		//substract the local Y axis velocity of the rigidbody
		//	force = force - settings.vehicleCamera.transform.up * transform.InverseTransformDirection (force).y;
		settings.maxForwardSpeed = mainRigidbody.velocity.magnitude;
		if (currentSpeed > settings.maxForwardSpeed) {
			mainRigidbody.AddForce (Vector3.zero);
		} else {
			mainRigidbody.AddForce (force);
		}
		anyOnGround = false;
		if (Physics.Raycast (vehicleParts.chassis.transform.position, -normal, gravityManager.settings.rayDistance, settings.layer)) {
			anyOnGround = true;
		}
	}

	void FixedUpdate ()
	{
		
	}
	//if the vehicle is using the gravity control, set the state in this component
	public void changeGravityControlUse (bool state)
	{
		usingGravityControl = state;
	}
	//the player is getting on or off from the vehicle, so
	public void changeVehicleState (Vector3 nextPlayerPos)
	{
		driving = !driving;
		//set the audio values if the player is getting on or off from the vehicle
		if (driving) {
			
		} else {
			
			boostInput = 1;
			//stop the boost
			if (usingBoost) {
				usingBoost = false;
				vCamera.usingBoost (false, "Boost");
				usingBoosting ();
				boostInput = 1;
			}
		}
		//set the same state in the IK driving and in the gravity control components
		IKManager.startOrStopVehicle (driving, vehicleParts.chassis, normal, nextPlayerPos);
		gravityManager.changeGravityControlState (driving);
	}
	//the vehicle has been destroyed, so disabled every component in it
	public void disableVehicle ()
	{
		//stop the boost
		if (usingBoost) {
			usingBoost = false;
			vCamera.usingBoost (false, "Boost");
			usingBoosting ();
			boostInput = 1;
		}

		//disable the controller
		GetComponent<sphereController> ().enabled = false;
	}
	//get the current normal in the gravity control component
	public void setNormal (Vector3 normalValue)
	{
		normal = normalValue;
	}
	//if any collider in the vehicle collides, then
	void OnCollisionEnter (Collision collision)
	{
		//check that the collision is not with the player
		if (collision.contacts.Length > 0 && collision.gameObject.tag != "Player") {	
			//if the velocity of the collision is higher that the limit
			if (collision.relativeVelocity.magnitude > collisionForceLimit) {
				//if the vehicle hits another vehicle, apply damage to both of them according to the velocity at the impact
				applyDamage.checkHealth (gameObject, collision.collider.gameObject, 
					collision.relativeVelocity.magnitude * GetComponent<vehicleHUDManager> ().damageMultiplierOnCollision, 
					collision.contacts [0].normal, collision.contacts [0].point, gameObject, false);
			}
		}
	}
	//get the input manager component
	public void getInputActionManager (inputActionManager manager)
	{
		actionManager = manager;
	}
	//if the vehicle is using the boost, set the boost particles
	public void usingBoosting ()
	{
		
	}
	//use a jump platform
	public void useVehicleJumpPlatform (Vector3 direction)
	{
		mainRigidbody.AddForce (mainRigidbody.mass * direction, ForceMode.Impulse);
	}

	public void setNewJumpPower (float newJumpPower)
	{
		settings.jumpPower = newJumpPower * 100;
	}

	public void setOriginalJumpPower ()
	{
		settings.jumpPower = originalJumpPower;
	}

	[System.Serializable]
	public class otherVehicleParts
	{
		public Transform COM;
		public GameObject chassis;
	}

	[System.Serializable]
	public class vehicleSettings
	{
		public LayerMask layer;
		public float maxForwardSpeed;
		public float maxBoostMultiplier;
		public float moveSpeedMultiplier;
		public GameObject vehicleCamera;
		public float jumpPower;
		public bool canJump;
		public bool canUseBoost;
	}
}