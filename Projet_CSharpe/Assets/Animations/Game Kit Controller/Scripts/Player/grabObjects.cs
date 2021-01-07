using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class grabObjects : MonoBehaviour{
	public float holdDistance = 3;
	public float maxDistanceHeld = 4;
	public float maxDistanceGrab = 10;
	public float holdSpeed=10;
	public float alphaTransparency = 0.5f;
	public float rotationSpeed;
	public float rotateSpeed;
	public grabMode currentGrabMode;
	public realisticSettings realisticOptions;
	public grabSettings settings = new grabSettings();
	[HideInInspector] public bool aiming=false;
	[HideInInspector] public GameObject objectHeld;
	public enum grabMode{
		powers, realistic
	}
	Rigidbody objectHeldRigidbody;
	GameObject cam;
	GameObject smoke;
	GameObject shootZone;
	bool objectFocus;
	public bool grabbed;
	public bool gear;
	public bool rail;
	public bool regularObject;
	float holdTimer=0;
	float angle=0;
	float timer=0;
	RaycastHit hit;
	Shader dropShader;
	Vector3 grabCursorScale;
	Texture originalCursor;
	string grabbedObjectTag;
	List<string> ableToGrabTags=new List<string>();
	List<Renderer> rendererParts=new List<Renderer>();
	List<Shader> originalShader = new List<Shader> ();
	otherPowers powers;
	int i;
	int j;
	inputManager input;
	menuPause pauseManager;
	float orignalHoldDistance;
	playerCamera playerCameraManager;
	public bool rotatingObject;
	public bool usingDoor;
	RigidbodyConstraints objectHeldRigidbodyConstraints = RigidbodyConstraints.None;

	void Start(){
		input = transform.parent.GetComponent<inputManager> ();
		powers = GetComponent<otherPowers> ();
		cam = Camera.main.gameObject;
		shootZone = GameObject.Find ("shootZone");
		grabCursorScale = powers.settings.cursor.transform.localScale;
		originalCursor = powers.settings.cursor.GetComponent<RawImage> ().texture;
		ableToGrabTags = powers.settings.ableToGrabTags;
		pauseManager = transform.parent.GetComponent<menuPause> ();
		orignalHoldDistance = holdDistance;
		playerCameraManager = GameObject.Find ("Player Camera").GetComponent<playerCamera> ();
	}
	
	void Update(){
		//if the player is in aim mode, grab an object
		if (!pauseManager.usingDevice && aiming && !objectHeld && settings.grabObjectsEnabled && input.checkInputButton ("Grab Objects", inputManager.buttonType.getKeyDown)) {
			grabObject();
		}
		//if the drop button is being holding, add force to the final velocity of the drooped object
		if(grabbed && input.checkInputButton ("Grab Objects", inputManager.buttonType.getKey)) {
			if (regularObject && currentGrabMode == grabMode.powers) {
				if (holdTimer > 300) {
					//if the button is not released immediately, active the power slider
					if (!settings.powerSlider.gameObject.activeSelf) {
						settings.particles [1].SetActive (true);
						settings.powerSlider.gameObject.SetActive (true);
					}
				}
				if (holdTimer < settings.powerSlider.maxValue) {
					holdTimer += Time.deltaTime * 1500;
					settings.powerSlider.value += Time.deltaTime * 1500;
				}
			}
		}
		if (currentGrabMode == grabMode.realistic && grabbed && input.checkInputButton ("Shoot", inputManager.buttonType.getKeyDown)) {
			settings.powerSlider.gameObject.SetActive (false);
			GameObject objectToThrow = objectHeld;
			dropObject ();
			if (objectToThrow.GetComponent<Rigidbody> () && regularObject) {
				objectToThrow.GetComponent<Rigidbody> ().AddForce (cam.transform.forward * realisticOptions.throwPower * objectToThrow.GetComponent<Rigidbody> ().mass,ForceMode.Impulse);
			}
		}

		//when the button is released, check the amount of strength accumulated
		if (grabbed && input.checkInputButton ("Grab Objects", inputManager.buttonType.getKeyUp)) {
			settings.powerSlider.gameObject.SetActive (false);
			GameObject objectToThrow = objectHeld;
			dropObject ();
			if (objectToThrow.GetComponent<Rigidbody> ()) {
				//if the button has been pressed and released quickly, drop the object, else addforce to its rigidbody
				if (!objectToThrow.GetComponent<vehicleGravityControl> ()) {
					objectToThrow.GetComponent<Rigidbody> ().useGravity = true;
				}
				if (currentGrabMode == grabMode.powers) {
					if (holdTimer > 300) {
						objectToThrow.AddComponent<launchedObjects> ();
						GameObject launchParticles = (GameObject)Instantiate (settings.particles [2], shootZone.transform.position, cam.transform.rotation);
						launchParticles.transform.SetParent (null);
						launchParticles.SetActive (true);
						if (objectToThrow.GetComponent<CharacterJoint> ()) {
							checkJointsInObject (objectToThrow, holdTimer);
						} else {
							addForceToThrownRigidbody (objectToThrow, holdTimer);
						}
					}
				}
			}
		}
		if(objectHeld && !usingDoor){
			if(input.checkInputButton ("Secondary Button", inputManager.buttonType.getKeyDown)){
				playerCameraManager.changeCameraRotationState (false);
				rotatingObject = true;
			}
			if(input.checkInputButton ("Secondary Button", inputManager.buttonType.getKeyUp)) {
				playerCameraManager.changeCameraRotationState (true);
				rotatingObject = false;
			}
		}
		if (rotatingObject) {
			objectHeld.transform.RotateAroundLocal(cam.transform.up, -Mathf.Deg2Rad * rotateSpeed * input.getMovementAxis("mouse").x);
			objectHeld.transform.RotateAroundLocal(cam.transform.right, Mathf.Deg2Rad * rotateSpeed * input.getMovementAxis("mouse").y);
		}
		// if an object is grabbed, then move it from its original position, to the other in front of the camera
		if (objectHeld) {
			if (!grabbed) {
				timer += Time.deltaTime;
				if ((Vector3.Distance (objectHeld.transform.position, cam.transform.position) <= maxDistanceHeld || rail || gear || usingDoor) && timer > 0.5f) {
					grabbed = true;
					timer = 0;
				}
			}
			//if the object is not capable to move in front of the camera, because for example is being blocked for a wall, drop it
			if (Vector3.Distance (objectHeld.transform.position, cam.transform.position) > maxDistanceHeld && grabbed && regularObject) {
				dropObject ();
			} else {
				//if the object is a cube, a turret, or anything that can move freely, set its position in front of the camera
				if (regularObject) {
					Vector3 nextPos = cam.transform.position + cam.transform.forward * (holdDistance + objectHeld.transform.localScale.x);
					Vector3 currPos = objectHeld.transform.position;
					objectHeldRigidbody.velocity = (nextPos - currPos) * holdSpeed;
					if (!rotatingObject) {
						objectHeld.transform.rotation = Quaternion.Slerp (objectHeld.transform.rotation, cam.transform.rotation, Time.deltaTime * rotationSpeed);
					}
					if (input.checkInputButton ("Previous Power", inputManager.buttonType.negMouseWheel) && settings.canUseZoomWhileGrabbed) {
						changeGrabbedZoom (1);
					}
					if (input.checkInputButton ("Next Power", inputManager.buttonType.posMouseWheel) && settings.canUseZoomWhileGrabbed) {
						changeGrabbedZoom (-1);
					}
				} 
				//else if the object is on a rail get the angle between the forward of the camera and the object forward
				if (rail) {
					int dir = 0;
					float newAngle = Vector3.Angle (objectHeld.transform.forward, cam.transform.forward);
					if (newAngle >= angle + 5) {
						dir = -1;
					}
					if (newAngle <= angle - 5) {
						dir = 1;
					}
					//if the camera aims to the object, dont move it, else move in the direction the camera is looking in the local forward and back of the object
					if (Physics.Raycast (cam.transform.position, cam.transform.TransformDirection (Vector3.forward), out hit, maxDistanceGrab, settings.layer)) {
						if (hit.transform.gameObject == objectHeld) {
							dir = 0;
							angle = Vector3.Angle (objectHeld.transform.forward, cam.transform.forward);
						}
					}
					if (Mathf.Abs (newAngle - angle) < 10) {
						dir = 0;
					}
					objectHeld.transform.Translate (Vector3.forward * dir * Time.deltaTime * 2);
				}
				if (gear) {
					//else, the object is a gear, so rotate it
					objectHeld.transform.Rotate (0, 0, 150 * Time.deltaTime);
				}
				if (usingDoor) {
					float yAxis = objectHeld.GetComponent<ConfigurableJoint> ().axis.y * input.getMovementAxis ("mouse").y;
					Vector3 extraYRotation = objectHeld.transform.localEulerAngles+objectHeld.transform.up*yAxis;
					float angleY = extraYRotation.y;
					if (angleY > 180) {
						angleY = Mathf.Clamp (angleY, objectHeld.GetComponent<movableDoor> ().limitXAxis.y, 360);
					} else if(angleY >0){
						angleY = Mathf.Clamp (angleY, 0, objectHeld.GetComponent<movableDoor> ().limitXAxis.x);
					}
					extraYRotation = new Vector3 (extraYRotation.x, angleY, extraYRotation.z);
					//extraYRotation += objectHeld.transform.up*yAxis;
					Quaternion rot = Quaternion.Euler (extraYRotation);
					objectHeld.transform.localRotation = Quaternion.Slerp (objectHeld.transform.localRotation, rot, Time.deltaTime*objectHeld.GetComponent<movableDoor> ().rotationSpeed);
				}
				if (currentGrabMode == grabMode.powers) {
					if (smoke) {
						//activate the particles while the player is moving an object
						smoke.transform.transform.LookAt (shootZone.transform.position);
						smoke.GetComponent<ParticleSystem> ().startSpeed = Vector3.Distance (smoke.transform.position, shootZone.transform.position) / 2;
					}
				}
			}
		}
		//change cursor size to show that the player is aiming a grabbable object and set to its normal scale and get the object to hold in case the player could grab it
		if (aiming && !objectHeld) {
			if (Physics.Raycast (cam.transform.position, cam.transform.TransformDirection (Vector3.forward), out hit, maxDistanceGrab,settings.layer)){
				if(checkTypeObject(hit.collider) && !objectFocus){
					powers.settings.cursor.transform.localScale/=2;
					objectFocus=true;
				}
				if(!checkTypeObject(hit.collider) && objectFocus){
					powers.settings.cursor.transform.localScale=grabCursorScale;
					objectFocus=false;
				}
			}
		}
	}
	public void grabObject(){
		//if the object which the player is looking, grab it
		if (Physics.Raycast (cam.transform.position, cam.transform.TransformDirection (Vector3.forward), out hit, maxDistanceGrab,settings.layer) && objectFocus){
			if(checkTypeObject(hit.collider)){
				//reset the hold distance
				holdDistance = orignalHoldDistance;
				//if the located object is part of a vehicle, get the main vehicle object to grab it
				if (hit.collider.GetComponent<vehicleDamageReceiver> ()) {
					objectHeld = hit.collider.GetComponent<vehicleDamageReceiver> ().vehicle;
					if (!objectHeld.GetComponent<Rigidbody> ().isKinematic) {
						//get the extra grab distance configurable for every vehicle
						holdDistance += objectHeld.GetComponent<vehicleHUDManager> ().extraGrabDistance;
						objectHeld.GetComponent<vehicleGravityControl> ().pauseDownForce (true);
					} else {
						objectHeld = null;
						return;
					}
				} 
				//if the located object is part of a character, get the main character object to grab it
				else if (hit.collider.GetComponent<characterDamageReceiver> ()) {
					objectHeld = hit.collider.GetComponent<characterDamageReceiver> ().character;
				} 
				//else it is an object from the able to grab list
				else {
					objectHeld = hit.collider.gameObject;
				}
				//get its tag, to set it again to the object, when it is dropped
				if (!objectHeld.GetComponent<vehicleHUDManager>()) {
					grabbedObjectTag = objectHeld.tag.ToString ();
					objectHeld.tag = "Untagged";
				}
				if (objectHeld.GetComponent<Rigidbody>()) {
					objectHeldRigidbody = objectHeld.GetComponent<Rigidbody> ();
					objectHeldRigidbody.isKinematic = false;
					objectHeldRigidbody.useGravity = false;
					objectHeldRigidbody.velocity = Vector3.zero;
				}
				//if the object has its gravity modified, pause that script
				if (objectHeld.GetComponent<artificialObjectGravity> ()) {
					objectHeld.GetComponent<artificialObjectGravity> ().active = false;
				}
				if (objectHeld.GetComponent<explosiveBarrel> ()) {
					objectHeld.GetComponent<explosiveBarrel> ().barrilCanExplodeState(false,gameObject);
				}
				if (objectHeld.GetComponent<crate> ()) {
					objectHeld.GetComponent<crate> ().crateCanBeBrokenState(false);
				}
				if(objectHeld.GetComponent<pickUpObject>()){
					objectHeld.GetComponent<pickUpObject> ().activateObjectTrigger ();
				}
				if (currentGrabMode == grabMode.powers) {
					//if the objects is a mechanism, the object is above a rail, so the player could move it only in two directions
					if (objectHeld.name == "mechanism") {
						angle = Vector3.Angle (objectHeld.transform.forward, cam.transform.forward);
						rail = true;
					}
					//if the object is a gear, it only can be rotated
					else if (objectHeld.name == "rotatoryGear") {
						gear = true;
					} else if (!objectHeld.GetComponent<ConfigurableJoint> () && objectHeldRigidbody) {
						regularObject = true;
						objectHeldRigidbodyConstraints = objectHeldRigidbody.constraints;
						objectHeldRigidbody.freezeRotation = true;
					}
				} else {
					if (!objectHeld.GetComponent<ConfigurableJoint> () && objectHeldRigidbody) {
						regularObject = true;
						if (!objectHeld.GetComponent<vehicleHUDManager> ()) {
							objectHeldRigidbodyConstraints = objectHeldRigidbody.constraints;
							objectHeldRigidbody.freezeRotation = true;
						}
					}
				}

				if (objectHeld.GetComponent<ConfigurableJoint> ()) {
					usingDoor = true;
					objectHeldRigidbodyConstraints = objectHeldRigidbody.constraints;
					objectHeldRigidbody.freezeRotation=true;
					playerCameraManager.changeCameraRotationState (false);
				}
//				if (!rail && !gear && !usingDoor) {
//					objectHeld.transform.SetParent (null);
//				}
				if (objectHeld.GetComponent<pickUpObject>()) {
					objectHeld.transform.SetParent (null);
				}
				//if the transparency is enabled, chnage all the color of all the materials of the object
				if (settings.enableTransparency) {
					Component[] components=objectHeld.GetComponentsInChildren(typeof(Renderer));
					foreach (Renderer child in components){
						if (child.GetComponent<Renderer>().material.shader) {
							Renderer render = child.GetComponent<Renderer> ();
							for (i=0;i<render.materials.Length;i++){
								rendererParts.Add (render);
								originalShader.Add (render.materials[i].shader);
								render.materials[i].shader = settings.pickableShader;
								Color alpha = render.materials[i].color;
								alpha.a = alphaTransparency;
								render.materials[i].color = alpha;
							}
						}
					}
				}
				settings.powerSlider.value = 0;
				holdTimer = 0;
				powers.carryingObject = true;
				powers.settings.cursor.GetComponent<RawImage> ().texture = settings.grabbedObjectCursor;
				powers.settings.cursor.transform.localScale = grabCursorScale;
				if (currentGrabMode == grabMode.powers) {
					if (settings.useGrabbedParticles) {
						//enable particles and reset some powers values
						smoke = (GameObject)Instantiate (settings.particles [3], hit.collider.transform.position, objectHeld.transform.rotation);
						smoke.transform.SetParent (objectHeld.transform);
						smoke.SetActive (true);
						settings.particles [0].SetActive (true);
					}
				}
			}
		} 
	}
	//check if the object detected by the raycast is in the able to grab list or is a vehicle
	public bool checkTypeObject(Collider col){
		bool canBeGrabbed = false;
		if (ableToGrabTags.Contains (col.tag.ToString ())) {
			canBeGrabbed = true;
		}
		if (col.GetComponent<vehicleDamageReceiver> ()) {
			canBeGrabbed = true;
		}
		if (col.GetComponent<characterDamageReceiver> ()) {
			if (ableToGrabTags.Contains (col.GetComponent<characterDamageReceiver> ().character.tag.ToString ())) {
				canBeGrabbed = true;
			}
		}
		//return the value of the checking
		return canBeGrabbed;
	}
	//drop the object
	public void dropObject(){
		powers.settings.cursor.GetComponent<RawImage> ().texture = originalCursor;
		powers.settings.cursor.transform.localScale = grabCursorScale;
		powers.carryingObject = false;
		playerCameraManager.changeCameraRotationState (true);
		rotatingObject = false;
		usingDoor = false;
		if (objectHeld) {
			//set the tag of the object that had before grab it, and if the object has its own gravity, enable again
			if (!objectHeld.GetComponent<vehicleHUDManager> ()) {
				objectHeld.tag = grabbedObjectTag;
			}
			if (objectHeld.GetComponent<artificialObjectGravity> ()) {
				objectHeld.GetComponent<artificialObjectGravity> ().active = true;
			}
			if (objectHeld.GetComponent<explosiveBarrel> ()) {
				objectHeld.GetComponent<explosiveBarrel> ().barrilCanExplodeState (true, gameObject);
			}
			if (objectHeld.GetComponent<crate> ()) {
				objectHeld.GetComponent<crate> ().crateCanBeBrokenState (true);
			}
			if (objectHeldRigidbody) {
				if (!objectHeld.GetComponent<vehicleGravityControl> ()) {
					objectHeldRigidbody.useGravity = true;
				}
				if(!objectHeld.GetComponent<ConfigurableJoint>()){
					objectHeldRigidbody.freezeRotation = false;
					if (objectHeldRigidbodyConstraints!=RigidbodyConstraints.None) {
						objectHeldRigidbody.constraints = objectHeldRigidbodyConstraints;
						objectHeldRigidbodyConstraints = RigidbodyConstraints.None;
					}
				}
			}
			if (objectHeld.GetComponent<vehicleGravityControl> ()) {
				objectHeld.GetComponent<vehicleGravityControl> ().pauseDownForce (false);
			}
			//set the normal shader of the object 
			for (i = 0; i < rendererParts.Count; i++) {
				if (rendererParts [i]) {
					Renderer render = rendererParts [i].GetComponent<Renderer> ();
					for (j = 0; j < rendererParts [i].materials.Length; j++) {
						render.materials [j].shader = originalShader [i];
					}
				}
			}
			//if the grabbed object is a turret, call a function to make it kinematic again when it will touch a surface
			if (objectHeld.tag=="friend") {
				objectHeld.SendMessage ("dropCharacter", true, SendMessageOptions.DontRequireReceiver);
			}
		}
		grabbed = false;
		objectHeld = null;
		objectHeldRigidbody = null;
		rail = false;
		gear = false;
		regularObject = false;
		settings.particles [0].SetActive (false);
		settings.particles [1].SetActive (false);
		rendererParts.Clear ();
		originalShader.Clear ();
		Destroy (smoke);
	}
	public void checkJointsInObject(GameObject objectToThrow, float force){
		if (objectToThrow.GetComponent<CharacterJoint> ()) {
			checkJointsInObject (objectToThrow.GetComponent<CharacterJoint> ().connectedBody.gameObject, force);
		} else {
			addForceToThrownRigidbody (objectToThrow, force);
		}
	}

	public void addForceToThrownRigidbody (GameObject objectToThrow, float force){
		Component[] components = objectToThrow.GetComponentsInChildren (typeof(Rigidbody));
		foreach (Component c in components) {
			Rigidbody currentRigid = c as Rigidbody;
			if (!currentRigid.isKinematic && currentRigid.GetComponent<Collider> ()) {
				currentRigid.AddForce (cam.transform.forward * force * currentRigid.mass);
			}
		}
	}
	public void changeGrabbedZoom(int zoomType){
		if (zoomType > 0) {
			holdDistance += Time.deltaTime * settings.zoomSpeed;
		} else {
			holdDistance -= Time.deltaTime * settings.zoomSpeed;
		}
		if (holdDistance > settings.maxZoomDistance) {
			holdDistance = settings.maxZoomDistance;
		}
		if (holdDistance < settings.minZoomDistance) {
			holdDistance = settings.minZoomDistance;
		}
	}
	public void checkIfDropObject(GameObject objectToCheck){
		if (objectHeld == objectToCheck) {
			dropObject ();
		}
	}
	[System.Serializable]
	public class grabSettings{
		public bool grabObjectsEnabled;
		public Shader pickableShader;
		public Slider powerSlider;
		public Texture grabbedObjectCursor;
		public GameObject[] particles;
		public LayerMask layer;
		public bool enableTransparency = true;
		public bool useGrabbedParticles;
		public bool canUseZoomWhileGrabbed;
		public float zoomSpeed;
		public float maxZoomDistance;
		public float minZoomDistance;
	}
	[System.Serializable]
	public class realisticSettings{
		public float throwPower;
	}
}