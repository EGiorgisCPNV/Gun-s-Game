using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IKDrivingSystem : MonoBehaviour
{
	public IKDrivingInformation IKDrivingInfo;
	public bool hidePlayerFromNPCs;
	public bool playerVisibleInVehicle = true;
	public bool ejectPlayerWhenDestroyed;
	public float ejectingPlayerForce;
	public bool useExplosionForceWhenDestroyed;
	public float explosionRadius;
	public float explosionForce;
	public float explosionDamage;
	public bool showGizmo;
	public Color gizmoLabelColor;
	public float gizmoRadius = 0.1f;
	[HideInInspector] public GameObject player;
	[HideInInspector] public bool controlsMenuOpened;
	GameObject pCamera;
	GameObject character;
	GameObject vCamera;
	GameObject vehicle;
	GameObject originalCameraParent;
	bool driving;
	Camera mainCamera;
	Quaternion currentCamerRotation;
	Vector3 currentCameraPosition;
	Vector3 previousCameraPos;
	Vector3 placeToShootOriginalPosition;
	int i;
	Coroutine moveCamera;
	bool vehicleDestroyed;
	inputActionManager actionManager;
	vehicleHUDInfo HUDInfo;
	menuPause pauseManager;
	vehicleCameraController vehicleCameraManager;
	playerCamera playerCameraManager;
	vehicleHUDManager HUDManager;
	List<Collider> colliders = new List<Collider> ();

	void Start ()
	{
		//get the neccessary elements, player, character and player camera
		player = GameObject.Find ("Player Controller");
		character = GameObject.Find ("Character");
		pCamera = GameObject.Find ("Player Camera");
		playerCameraManager = pCamera.GetComponent<playerCamera> ();
		mainCamera = Camera.main;
		//get the vehicle and its camera
		vehicleCameraManager = GetComponentInChildren<vehicleCameraController> ();
		vCamera = vehicleCameraManager.gameObject;
		HUDManager = GetComponentInChildren<vehicleHUDManager> ();
		vehicle = HUDManager.gameObject;
		//send the input manager component to the vehicle and its camera
		actionManager = GetComponent<inputActionManager> ();
		actionManager.getInputManager (character);
		vCamera.SendMessage ("getInputActionManager", actionManager);
		vehicle.SendMessage ("getInputActionManager", actionManager);
		HUDInfo = character.GetComponent<vehicleHUDInfo> ();
		pauseManager = character.GetComponent<menuPause> ();
	}
	//if the vehicle is destroyed, remove it from the scene
	public void destroyVehicle ()
	{
		Destroy (vCamera);
		Destroy (vehicle);
		Destroy (gameObject);
	}
	//if the vehicle is destroyed
	public void disableVehicle ()
	{
		//if the player was driving it
		if (driving) {
			vehicleDestroyed = true;
			//disable its components
			vehicle.GetComponent<Collider> ().enabled = false;
			//disable the option to get off from the vehicle if the player press that button
			player.GetComponent<usingDevicesSytem> ().removeVehicleFromList ();
			player.GetComponent<usingDevicesSytem> ().disableIcon ();
			//stop the vehicle
			startOrStopVehicle (false, null, transform.up, player.transform.position);
			if (ejectPlayerWhenDestroyed) {
				//eject the player from the car
				player.GetComponent<playerController> ().ejectPlayerFromVehicle (ejectingPlayerForce);
			} else {
				//kill him
				player.GetComponent<health> ().setDamage (player.GetComponent<health> ().healthAmount, vehicle.transform.up, vehicle.transform.position, vehicle, vehicle, false);
			}
			//disable the weapon system if the vehicle has it
			if (GetComponentInChildren<vehicleWeaponSystem> ()) {
				if (GetComponentInChildren<vehicleWeaponSystem> ().enabled) {
					GetComponentInChildren<vehicleWeaponSystem> ().changeWeaponState (false);
				}
			}
			if (!playerVisibleInVehicle) {
				player.GetComponent<changeGravity> ().settings.meshCharacter.enabled = true;
				enableOrDisablePlayerVisibleInVehicle (true);
			}
		}
		//disable the camera and the gravity control component
		vehicleCameraManager.enabled = false;
		vehicle.SendMessage ("disableVehicle");
		if (vehicle.GetComponent<vehicleGravityControl> ()) {
			vehicle.GetComponent<vehicleGravityControl> ().enabled = false;
		}
		if (useExplosionForceWhenDestroyed) {
			if (colliders.Count == 0) {
				colliders.AddRange (Physics.OverlapSphere (vehicle.transform.position, explosionRadius));
				foreach (Collider hit in colliders) {
					if (hit != null) {
						if (hit.gameObject.tag != "Player" && hit.gameObject.tag != "bullet" && !hit.gameObject.transform.IsChildOf (transform)) {
							if (hit.GetComponent<Rigidbody> ()) {
								if (!hit.GetComponent<Rigidbody> ().isKinematic) {
									hit.GetComponent<Rigidbody> ().AddExplosionForce (explosionForce, vehicle.transform.position, explosionRadius, 3, ForceMode.Impulse);
								}
							}
							if (explosionDamage > 0) {
								applyDamage.checkHealth (gameObject, hit.gameObject, explosionDamage, -vehicle.transform.forward, hit.gameObject.transform.position, gameObject, false);
							}
						}
					}
				}
			}
		}
	}
	//the player is getting in or off from the vehicle
	public void startOrStopVehicle (bool state, GameObject parent, Vector3 normal, Vector3 nextPlayerPos)
	{
		driving = state;
		//set the state driving as the current state of the player
		player.GetComponent<playerController> ().drivingState (driving, vehicle);
		pauseManager.usingDeviceState (driving);
		//enable or disable the collider and the rigidbody of the player
		player.GetComponent<Collider> ().isTrigger = driving;
		if (hidePlayerFromNPCs) {
			player.GetComponent<Collider> ().enabled = !driving;
		}
		player.GetComponent<Rigidbody> ().isKinematic = driving;
		//get the IK positions of the car to use them in the player
		player.GetComponent<IKSystem> ().drivingState (driving, IKDrivingInfo);

		//check if the camera in the player is in first or third view, to set the current view in the vehicle
		bool firstCameraEnabled = player.GetComponentInChildren<changeGravity> ().settings.firstPersonView;
		vehicleCameraManager.setCameraPosition (firstCameraEnabled);
		//enable and disable the player's HUD and the vehicle's HUD
		HUDInfo.playerHUD.SetActive (!driving);
		HUDInfo.vehicleHUD.SetActive (driving);

		//get the vehicle's HUD elements to show the current values of the vehicle, like health, energy, ammo....
		HUDManager.getHUDBars (HUDInfo.vehicleHealth, HUDInfo.vehicleBoost, HUDInfo.vehicleAmmo, HUDInfo.weaponName, HUDInfo.ammoInfo,
			HUDInfo.ammoContent, HUDInfo.currentSpeed);

		player.GetComponent<footStepManager> ().enableOrDisableFootSteps (!driving);

		if (actionManager) {
			actionManager.enableOrDisableInput (driving);
		}
		playerCameraManager.playOrPauseHeadBob (!driving);
		vehicleCameraManager.getPlayer (player);
		//if the player is driving it
		if (driving) {
			HUDInfo.setControlList (actionManager);
			HUDInfo.setCurrentVehicle (vehicle);
			placeToShootOriginalPosition = player.GetComponent<health> ().placeToShoot.transform.localPosition;
			player.GetComponent<health> ().placeToShoot.transform.SetParent (HUDManager.placeToShoot);
			player.GetComponent<health> ().placeToShoot.transform.localPosition = Vector3.zero;
			//disable or enable the vehicle camera
			vehicleCameraManager.changeCameraDrivingState (driving);
			//check the current state of the player, to check if he is carrying an object, aiming, etc... to disable that state
			character.GetComponent<playerStatesManager> ().checkPlayerStates ();
			//change the main camera from the player camera component to the vehicle's camera component
			originalCameraParent = playerCameraManager.pivot.gameObject;
			//store the previous camera position
			previousCameraPos = mainCamera.transform.localPosition;
			//if the first camera was enabled, set the current main camera position in the first camera position of the vehicle
			if (firstCameraEnabled) {
				//enable the player's body to see it
				player.GetComponent<changeGravity> ().settings.meshCharacter.enabled = true;
			}
			//else the main camera position in the third camera position of the vehicle
			mainCamera.transform.SetParent (vehicleCameraManager.currentState.cameraTransform);
			//set the player's position and parent inside the car
			player.transform.SetParent (parent.transform);
			player.transform.localPosition = Vector3.zero;
			player.transform.localRotation = Quaternion.identity;
			player.transform.position = IKDrivingInfo.bodyPosition.position;
			player.transform.rotation = IKDrivingInfo.bodyPosition.rotation;
			currentCameraPosition = Vector3.zero;
			//get the vehicle camera rotation
			currentCamerRotation = vehicleCameraManager.currentState.cameraTransform.localRotation;
			//disable the camera rotation of the player 
			playerCameraManager.pauseOrPlayCamera (!driving);
			//reset the player's camera rotation input values
			playerCameraManager.lookAngle = Vector2.zero;
			//set the player's camera rotation as the same in the vehicle
			pCamera.transform.rotation = vCamera.transform.rotation;
			//set the same rotation in the camera pivot
			playerCameraManager.pivot.transform.localRotation = vehicleCameraManager.currentState.pivotTransform.localRotation;
		} 
		//the player gets off from the vehicle
		else {
			HUDInfo.setCurrentVehicle (null);
			player.GetComponent<health> ().placeToShoot.transform.SetParent (player.transform);
			player.GetComponent<health> ().placeToShoot.transform.localPosition = placeToShootOriginalPosition;
			//if the first person was actived, disable the player's body
			if (firstCameraEnabled) {
				player.GetComponent<changeGravity> ().settings.meshCharacter.enabled = false;
			}
			//set the parent of the player as null
			player.transform.SetParent (null);
			//set the player's position at the correct side of the car
			player.transform.position = nextPlayerPos;
			//set the current gravity of the player's as the same in the vehicle
			player.GetComponent<changeGravity> ().setNormal (normal);
			//set the player's camera position in the correct place
			pCamera.transform.position = nextPlayerPos;
			currentCamerRotation = Quaternion.identity;
			currentCameraPosition = previousCameraPos;
			if (vehicleDestroyed && firstCameraEnabled) {
				playerCameraManager.pauseOrPlayCamera (!driving);
				vehicleCameraManager.changeCameraDrivingState (driving);
				return;
			}
			//change the main camera parent to player's camera
			mainCamera.transform.SetParent (originalCameraParent.transform);
			//disable or enable the vehicle camera
			vehicleCameraManager.changeCameraDrivingState (driving);
		}
		if (!playerVisibleInVehicle) {
			player.GetComponent<changeGravity> ().settings.meshCharacter.enabled = !driving;
			enableOrDisablePlayerVisibleInVehicle (driving);
		}
		//stop the current transition of the main camera from the player to the vehicle and viceversa if the camera is moving from one position to another
		checkCameraTranslation (driving);

	}

	public void enableOrDisablePlayerVisibleInVehicle(bool state){
		if (!player.GetComponent<changeGravity> ().settings.firstPersonView) {
			player.GetComponent<jetpackSystem> ().enableOrDisableJetPackMesh (!state);
			player.GetComponent<playerWeaponsManager>().enableOrDisableWeaponsMesh (!state);
			player.GetComponent<changeGravity> ().settings.arrow.SetActive (!state);
		} else {
			player.GetComponent<changeGravity> ().settings.meshCharacter.enabled = false;
		}
	}
	//stop the current coroutine and start it again
	void checkCameraTranslation (bool state)
	{
		if (moveCamera != null) {
			StopCoroutine (moveCamera);
		}
		moveCamera = StartCoroutine (adjustCamera (state));
	}
	//move the camera position and rotation from the player's camera to vehicle's camera and viceversa
	IEnumerator adjustCamera (bool state)
	{
		float i = 0;
		//store the current rotation of the camera
		Quaternion currentQ = mainCamera.transform.localRotation;
		//store the current position of the camera
		Vector3 currentPos = mainCamera.transform.localPosition;
		//translate position and rotation camera
		while (i < 1) {
			i += Time.deltaTime * 2;
			mainCamera.transform.localRotation = Quaternion.Lerp (currentQ, currentCamerRotation, i);
			mainCamera.transform.localPosition = Vector3.Lerp (currentPos, currentCameraPosition, i);
			yield return null;
		}
		//enable the camera rotation of the player if the vehicle is not being droven
		if (!state) {
			playerCameraManager.pauseOrPlayCamera (!state);
		}
	}

	public void openOrCloseControlsMenu (bool state)
	{
		if ((!pauseManager.playerMenuActive || controlsMenuOpened) && pauseManager.usingDevice) {
			controlsMenuOpened = state;
			pauseManager.openOrClosePlayerMenu (controlsMenuOpened);
			pauseManager.showOrHideCursor (controlsMenuOpened);
			//disable the touch controls
			pauseManager.checkTouchControls (!controlsMenuOpened);
			//disable the camera rotation
			pauseOrPlayVehicleCamera (controlsMenuOpened);
			pauseManager.usingSubMenuState (controlsMenuOpened);
			HUDInfo.openOrCloseControlsMenu (controlsMenuOpened);
		}
	}

	public void pauseOrPlayVehicleCamera (bool state)
	{
		vehicleCameraManager.pauseOrPlayVehicleCamera (state);
	}

	void OnDrawGizmos ()
	{
		DrawGizmos ();
	}

	void OnDrawGizmosSelected ()
	{
		DrawGizmos ();
	}
	//draw the pivot and the final positions of every door
	void DrawGizmos ()
	{
		if (showGizmo) {
			for (i = 0; i < IKDrivingInfo.IKDrivingPos.Count; i++) {
				Gizmos.color = Color.yellow;
				Gizmos.DrawSphere (IKDrivingInfo.IKDrivingPos [i].position.position, gizmoRadius);
			}
			for (i = 0; i < IKDrivingInfo.IKDrivingKneePos.Count; i++) {
				Gizmos.color = Color.blue;
				Gizmos.DrawSphere (IKDrivingInfo.IKDrivingKneePos [i].position.position, gizmoRadius);
			}
			if (IKDrivingInfo.bodyPosition) {
				Gizmos.color = Color.red;
				Gizmos.DrawSphere (IKDrivingInfo.bodyPosition.position, gizmoRadius);
			}
			if (IKDrivingInfo.steerDirecion) {
				Gizmos.color = Color.yellow;
				Gizmos.DrawSphere (IKDrivingInfo.steerDirecion.position, gizmoRadius);
			}
			if (useExplosionForceWhenDestroyed) {
				if (vehicle) {
					Gizmos.color = Color.red;
					Gizmos.DrawWireSphere (vehicle.transform.position, explosionRadius);
				} else {
					HUDManager = GetComponentInChildren<vehicleHUDManager> ();
					vehicle = HUDManager.gameObject;
				}
			}
		}
	}

	[System.Serializable]
	public class IKDrivingInformation
	{
		public List<IKDrivingPositions> IKDrivingPos = new List<IKDrivingPositions> ();
		public List<IKDrivingKneePositions> IKDrivingKneePos = new List<IKDrivingKneePositions> ();
		public Transform bodyPosition;
		public Transform steerDirecion;
	}

	[System.Serializable]
	public class IKDrivingPositions
	{
		public string Name;
		public AvatarIKGoal limb;
		public Transform position;
	}

	[System.Serializable]
	public class IKDrivingKneePositions
	{
		public string Name;
		public AvatarIKHint knee;
		public Transform position;
	}
}