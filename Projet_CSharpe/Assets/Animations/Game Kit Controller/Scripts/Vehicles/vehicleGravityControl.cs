using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class vehicleGravityControl : MonoBehaviour {
	public bool gravityControlEnabled;
	public bool OnGround;
	public bool powerActive;
	public bool recalculate;
	public bool searching;
	public bool searchNew;
	public bool searchAround;
	public bool rotating;
	public Vector3 currentNormal = new Vector3 (0, 1, 0); 
	public otherSettings settings;
	[HideInInspector] public List<Collider> collidersList = new List<Collider> ();
	bool sphere;
	bool conservateSpeed;
	bool accelerating;
	bool useGravity = true;
	Vector3 forwardAxisCamera;
	Vector3 rightAxisCamera;
	Vector3 forwardAxisMovement;
	Vector3 surfaceNormal; 
	Vector3 previousVelocity;
	float normalGravityMultiplier;
	float horizontalAxis;
	float verticalAxis;
	Transform pivot;
	GameObject father;
	Collider gravityCenterCollider;
	RaycastHit hit;
	Coroutine rotateCharacterState;
	Rigidbody mainRigidbody;
	GameObject vehicleController;
	inputActionManager actionManager;
	vehicleCameraController vehicleCameraManager;

	void Start () {
		//get every important component in the vehicle
		vehicleController = gameObject;
		Component[] components = GetComponentsInChildren(typeof(Collider));
		foreach (Component c in components) {
			collidersList.Add (c as Collider);
		}
		//get the gravity center collider located in the center of mass of the vehicle
		gravityCenterCollider = settings.centerOfMass.GetComponent<Collider> ();
		mainRigidbody=GetComponent<Rigidbody>();
		//set the current normal in the vehicle controller
		vehicleController.SendMessage("setNormal",currentNormal, SendMessageOptions.DontRequireReceiver);
		vehicleCameraManager = settings.vehicleCamera.GetComponent<vehicleCameraController> ();
	}
		
	void Update(){
		//if the vehicle is being driving
		if (gravityControlEnabled && settings.canUseGravityControl) {
			//activate the gravity control to search a new surface in the camera direction
			if (actionManager.getActionInput ("Enable Gravity Control")) {
				activateGravityPower (pivot.transform.TransformDirection (Vector3.forward), pivot.transform.TransformDirection (Vector3.right));
			}
			//deactivate the gravity control
			if (actionManager.getActionInput ("Disable Gravity Control")) {
				deactivateGravityPower ();
			}
			if (searching || searchNew || powerActive) {
				//if the vehicle is searching a new surface, increase its velocity, only when the gravity power is active
				if (actionManager.getActionInput ("Increase Gravity Speed")) {
					accelerating = true;
					vehicleCameraManager.usingBoost(true,"Quick Gravity Control");
				}
				//stop to increase the velocity of the vehiclee in the air
				if (actionManager.getActionInput ("Decrease Gravity Speed")) {
					accelerating = false;
					vehicleCameraManager.usingBoost(true,"Regular Gravity Control");
				}
			}
		}
		//if the vehicle is searching a new surface
		if (searching) {
			//set the vehicle movement direction in the air (local X and Y axis)
			forwardAxisCamera = pivot.transform.TransformDirection (Vector3.up);
			rightAxisCamera = pivot.transform.TransformDirection (Vector3.right);
			//new position and direction of a raycast to detect a surface
			Vector3 pos;
			Vector3 dir;
			//if the player has activate the gravity control in the vehicle, then apply force in the camera direction and set the raycas position in the center of mass
			if (!searchNew) {
				//this function apply force to the vehicle in the new direction and allows to move it in the air in the left, right, forward and backward direction using the direction
				//of the movement as local Y axis
				moveInTheAir (9.8f * (mainRigidbody.mass / settings.massDivider) * forwardAxisMovement * settings.speed, settings.speed, mainRigidbody.mass / settings.massDivider, forwardAxisMovement);
				pos = settings.centerOfMass.transform.position;
				dir = forwardAxisMovement;
			} 
			//else, the player is falling in the air while the gravity control was enabled, so the ray direction is the negavite local Y axis or -currentNormal and the position is the 
			//vehicle itself
			else {    
				pos = transform.position;
				dir = -currentNormal;
			}
			Debug.DrawRay (pos, dir * settings.vehicleRadius, Color.yellow);
			//if the raycast found a new surface, then
			if (Physics.Raycast (pos, dir, out hit, settings.vehicleRadius, settings.layer)) {
				//check is not a trigger or a rigidbody
				if (!hit.collider.isTrigger && !hit.rigidbody) {
					//a new valid surface has been detected, so stop the search of a new surface
					powerActive = false;
					searchNew = false;
					searching = false;
					searchAround = false;
					//set to 0 the vehicle velocity
					mainRigidbody.velocity = Vector3.zero;
					//disable the collider in the center of mass
					gravityCenterCollider.enabled = false;
					//if the surface can be circumnavigate
					if (hit.collider.gameObject.tag == "sphere") {
						sphere = true;
					}
					//if the surface is moving
					if (hit.collider.gameObject.tag == "moving") {
						addParent (hit.collider.gameObject);
					}	
					//if the new normal is different from the current normal, which means that is other surface inclination, then 
					if (hit.normal != currentNormal) {
						//rotate the vehicle according to that normal
						StartCoroutine (rotateToSurface (hit.normal, 2)); 
					}
					//disable the gravity control in the vehicle controller
					vehicleController.SendMessage ("changeGravityControlUse", false);
					vehicleCameraManager.usingBoost(false,"StopShake");
				}
			}
		}
		//if the gravity control is enabled, and the vehicle falls in its negative loxal Y axis, 
		//then check if there is any surface below the vehicle which will become the new ground surface
		//in case the vehicle reachs a certain velocity
		if (!OnGround && transform.InverseTransformDirection (mainRigidbody.velocity).y < -settings.velocityToSearch && !searchNew && !powerActive) {
			//check that the current normal is not the regular one
			if (currentNormal != new Vector3 (0, 1, 0)) {
				//enable the collider in the center of mass
				gravityCenterCollider.enabled = true;
				//searching a new surface
				searchNew = true;
				searching = true;
				//stop to recalculate the normal of the vehicle
				recalculate = false;
				sphere = false;
				//enable the gravity control in the vehicle controller
				vehicleController.SendMessage ("changeGravityControlUse", true);
				vehicleCameraManager.usingBoost(true,"Regular Gravity Control");
			}
		}
		//if the vehicle is above a circumnavigable object, then recalculate the current normal of the vehicle according to the surface normal under the vehicle
		if (!searching && (sphere || father) && recalculate) {
			//set the distance of the ray, if the vehicle is not on the ground, the distance is higher
			float distance = settings.rayDistance + 0.05f;
			if (!OnGround) {
				distance = 10;
			}
			//if the vehicle founds a surface below it, get its normal
			if (Physics.Raycast (settings.centerOfMass.transform.position, -transform.up, out hit, distance, settings.layer)) {
				if (!hit.collider.isTrigger && !hit.rigidbody) {
					if (hit.collider.gameObject.tag == "sphere") {
						surfaceNormal = hit.normal;
					}
					if (hit.collider.gameObject.tag == "moving") {
						surfaceNormal = hit.normal;
						if (!father) {
							addParent (hit.collider.gameObject);
						}
					} else {
						if (father) {
							removeParent ();
						}
					}
				}
			}
			//set the current normal according to the hit.normal
			currentNormal = Vector3.Lerp (currentNormal, surfaceNormal, 10 * Time.deltaTime);
			Vector3 myForward = Vector3.Cross (transform.right, currentNormal);
			Quaternion dstRot = Quaternion.LookRotation (myForward, currentNormal); 
			transform.rotation = Quaternion.Lerp (transform.rotation, dstRot, 10 * Time.deltaTime);
			Vector3 myForwardCamera = Vector3.Cross (settings.vehicleCamera.transform.right, currentNormal);
			Quaternion dstRotCamera = Quaternion.LookRotation (myForwardCamera, currentNormal);
			settings.vehicleCamera.transform.rotation = Quaternion.Lerp (settings.vehicleCamera.transform.rotation, dstRotCamera, 10 * Time.deltaTime);
			vehicleController.SendMessage ("setNormal", currentNormal);
		}
		//if the vehicle is being rotated to a new surface, set its velocity to 0
		if (rotating) {
			mainRigidbody.velocity = Vector3.zero;
		}
	}
	void FixedUpdate(){
		//if the gravity control is not being used
		if (!powerActive) {
			//apply force to the vehicle in the negavity local Y axis, so the vehicle has a regular gravity
			if (useGravity) {
				mainRigidbody.AddForce (-9.8f * mainRigidbody.mass * currentNormal * settings.gravityMultiplier);
			}
			//if the vehicle is search a new surface, and previously the gravity control was enabled, then 
			if (searchNew) {
				//apply force in the negavite local Y axis of the vehicle, and allow to move it left, right, backward and forward
				moveInTheAir (mainRigidbody.velocity, 1, 1, -currentNormal);
			} 
			//check if the vehicle is on the ground
			OnGround = false;
			//use a raycast to it, with the center of the mass as position and the negavity local Y axis as direction, using the ray distance configured in the inspector
			if (Physics.Raycast (settings.centerOfMass.transform.position, -transform.up, out hit, settings.rayDistance, settings.layer)) {
				Debug.DrawLine (settings.centerOfMass.transform.position, hit.point, Color.cyan);
				//if the hit is not a trigger
				if (!hit.collider.isTrigger) {
					//the vehicle is on the ground
					OnGround = true;
					//if the gravity control is enabled, and the surface below the vehicle can be circumnavigate or is a moving object, allow to recalculate the normal
					if (sphere || father) {
						recalculate = true;
					} else {
						recalculate = false;
					}
				}
			} 
			//else the vehicle is in the air
			else {
				OnGround = false;
			}
		} 
		//the vehicle is searching a new surface, so it is in the air
		else {
			OnGround = false;
		}
	}
	void moveInTheAir(Vector3 newVel, float speedMultiplier, float massMultiplier,Vector3 movementDirection){
		//get the new velocity to apply to the vehicle
		//print(transform.InverseTransformDirection( newVel).y);
		Vector3 newVelocity=newVel;
		//get the current values of the horizontal and vertical axis, from the input manager
		horizontalAxis = actionManager.input.getMovementAxis("keys").x;
		verticalAxis = actionManager.input.getMovementAxis("keys").y;
		//allow to move the vehicle in its local X and Y axis while he falls or move in the air using the gravity control
		Vector3 newmoveInput = verticalAxis * forwardAxisCamera + horizontalAxis * rightAxisCamera;
		if (newmoveInput.magnitude > 1) {
			newmoveInput.Normalize ();
		}
		//if the input axis are being used, set that movement to the vehicle
		if(newmoveInput.magnitude>0){
			newVelocity+=newmoveInput * settings.speed*(mainRigidbody.mass/settings.massDivider)*speedMultiplier;
		}
		//if the player is accelerating in the air, add more velocity to the vehicle
		if(accelerating){
			newVelocity+=forwardAxisMovement * settings.accelerateSpeed * massMultiplier;
		}
		//if the current local Y velocity is lower than the limit, clamp the velocity
		if (Mathf.Abs( transform.InverseTransformDirection (newVelocity).y) > 40) {
			newVelocity -= movementDirection * Mathf.Abs(transform.InverseTransformDirection (newVelocity).y);
			newVelocity += movementDirection * 40;
		}
		//set the new velocity to the vehicle
		mainRigidbody.velocity = Vector3.Lerp (mainRigidbody.velocity, newVelocity, Time.deltaTime * 2);
	}
	//draw the ray distance radius in the vehicle
	void OnDrawGizmosSelected() {
		if (settings.centerOfMass) {
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere (settings.centerOfMass.transform.position, settings.vehicleRadius);
		}
	}
	//when the colliders of the vehicle hits another surface
	void OnCollisionEnter(Collision col){
		//if the vehicle was searching a new surface using the gravity control, then
		if (searching && searchAround) {
			//check the type of collider
			if (col.gameObject.tag != "Player" && !col.rigidbody && !col.collider.isTrigger ) {
				print ("collision "+col.collider.name);
				//using the collision contact, get the direction of that new surface, so the vehicle change its movement to the position of that surface
				//like this the searching of a new surface is more accurate, since a raycast and collisions are used to detect the new surface
				Vector3 hitDirection= col.contacts[0].point-settings.centerOfMass.transform.position;
				//set the current direction of the vehicle
				hitDirection=hitDirection/hitDirection.magnitude;
				forwardAxisMovement = hitDirection;
				//stop to search collisions around the vehicle
				searchAround=false;
			}
		}
	}
	//set and remove the parent of the vehicle according to the type of surface
	void addParent(GameObject obj){
		father = obj;
		transform.SetParent (father.transform);
		settings.vehicleCamera.transform.SetParent (father.transform);
	}
	void removeParent(){
		transform.SetParent (null);
		settings.vehicleCamera.transform.SetParent (null);
		father = null;
	}
	//activate the collider in the center of mass of the vehicle to dectect collisions with a new surface when the gravity control is enabled
	IEnumerator activateCollider(){
		//wait to avoid enable the collider when the vehicle stills in the ground
		yield return new WaitForSeconds (0.2f);
		//if the vehicle is searching surface, then active the collider
		if (searching || searchNew || searchAround) {
			gravityCenterCollider.enabled = true;
		}
	}
	//enable the gravity control in the direction of the camera
	public void activateGravityPower(Vector3 dir, Vector3 right){
		StartCoroutine(activateCollider());
		searchNew = false;
		removeParent ();
		sphere = false;	
		searching = true;
		searchAround=true;
		powerActive = true;
		forwardAxisMovement = dir;
		rightAxisCamera=right;
		//enable the gravity control in the vehicle controller
		vehicleCameraManager.usingBoost(true,"Regular Gravity Control");
		vehicleController.SendMessage ("changeGravityControlUse", true, SendMessageOptions.DontRequireReceiver);
	}
	//disaable the gravity control, rotatin the vehicle to the regular gravity 
	public void deactivateGravityPower(){
		//check that the vehicle was searching a new surface, or that the gravity is different than vector3.up
		if (searching || searchNew || searchAround || currentNormal != new Vector3 (0, 1, 0)) {
			conservateSpeed = true;
			gravityCenterCollider.enabled = false;
			accelerating = false;
			sphere = false;	
			removeParent ();
			searchNew = false;
			searching = false;
			searchAround=false;
			recalculate = false;
			//if the current normal before reset the gravity to the regular one is different from vectore.up, then rotate the vehicle back to the regular state
			if (currentNormal != new Vector3 (0, 1, 0)) {
				StartCoroutine (rotateToSurface (new Vector3 (0, 1, 0), 2));
			}
			powerActive=false;
			//set the normal in the vehicle controller
			vehicleController.SendMessage("setNormal",Vector3.up);
			//disable the gravity control in the vehicle controller
			vehicleCameraManager.usingBoost(false,"stopShake");
			vehicleController.SendMessage ("changeGravityControlUse", false);
		}
	}
	//the player is getting on or off from the vehicle, so enable or disable the graivty control component
	public void changeGravityControlState(bool state){
		gravityControlEnabled = state;
		//if the vehicle is not being driving, and it wasn't in the ground, deactivate the gravity control
		if (!gravityControlEnabled && !OnGround) {
			deactivateGravityPower ();
			accelerating = false;
		}
	}
	//rotate the vehicle and its camera to the new found surface, using the normal of that surface
	public IEnumerator rotateToSurface(Vector3 normal, int rotSpeed){
		previousVelocity = mainRigidbody.velocity;
		//the vehicle is being rotate, so set its velocity to 0
		rotating = true;
		currentNormal = normal; 
		//set the new normal in the vehicle controller
		vehicleController.SendMessage("setNormal",normal);
		//get the current rotation of the vehicle and the camera
		Quaternion rotPlayer = transform.rotation;
		Quaternion rotCamera = settings.vehicleCamera.transform.rotation;
		Vector3 myForwardPlayer = Vector3.Cross (transform.right, normal);
		Quaternion dstRotPlayer = Quaternion.LookRotation (myForwardPlayer, normal);
		Vector3 myForwardCamera = Vector3.Cross (settings.vehicleCamera.transform.right, normal);
		Quaternion dstRotCamera = Quaternion.LookRotation (myForwardCamera, normal);
		//rotate from their rotation to thew new surface normal direction
		for (float t = 0; t < 1;) {
			t += Time.deltaTime * rotSpeed;
			settings.vehicleCamera.transform.rotation = Quaternion.Slerp (rotCamera,dstRotCamera, t);
			transform.rotation = Quaternion.Slerp (rotPlayer, dstRotPlayer, t);
			yield return null;
		}
		rotating = false;
		//store the current vehicle velocity to set it again once the rotation is finished, is only applied when the gravity control is disabled
		if (conservateSpeed) {
			mainRigidbody.velocity = previousVelocity;
		}
		conservateSpeed = false;
	}
	public IEnumerator rotateVehicleToLandSurface(Vector3 hitNormal, Vector3 hitPoint){
		rotating = true;
		currentNormal = hitNormal; 
		//set the new normal in the vehicle controller
		vehicleController.SendMessage("setNormal",hitNormal);
		//get the current rotation of the vehicle and the camera
		Quaternion rotPlayer = transform.rotation;
		Quaternion rotCamera = settings.vehicleCamera.transform.rotation;
		Vector3 myForwardPlayer = Vector3.Cross (transform.right, hitNormal);
		Quaternion dstRotPlayer = Quaternion.LookRotation (myForwardPlayer, hitNormal);
		Vector3 myForwardCamera = Vector3.Cross (settings.vehicleCamera.transform.right, hitNormal);
		Quaternion dstRotCamera = Quaternion.LookRotation (myForwardCamera, hitNormal);
		//rotate from their rotation to thew new surface normal direction
		for (float t = 0; t < 1;) {
			t += Time.deltaTime * 2;
			settings.vehicleCamera.transform.rotation = Quaternion.Slerp (rotCamera,dstRotCamera, t);
			transform.rotation = Quaternion.Slerp (rotPlayer, dstRotPlayer, t);
			yield return null;
		}
		rotating = false;
		pauseDownForce (false);
	}
	//get the current camera pivot, according to the vehicle view, in fisrt or third person
	public void getCurrentCameraPivot(Transform newPivot){
		pivot = newPivot;
	}
	public void pauseDownForce(bool state){
		useGravity = !state;
	}
	//get the input manager component
	public void getInputActionManager(inputActionManager manager){
		actionManager = manager;
	}
	[System.Serializable]
	public class otherSettings{
		public LayerMask layer;
		public float speed = 10;
		public float accelerateSpeed = 20;
		public float velocityToSearch = 10;
		public float gravityMultiplier = 1;
		public float rayDistance;
		public float vehicleRadius;
		public GameObject vehicleCamera;
		public Transform centerOfMass;
		public bool canUseGravityControl;
		public float massDivider = 1000;
	}
}