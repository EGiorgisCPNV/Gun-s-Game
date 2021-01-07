using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
public class playerStatesManager : MonoBehaviour {
	public Text currentPlayerModeText;
	public List<playerMode> playersMode=new List<playerMode>();
	public GameObject playerControlModeMenu;
	public RawImage normalModeImage;
	public RawImage flyModeImage;
	public RawImage jetpackImage;
	public RawImage sphereImage;
	public RawImage currentPlayerControlModeImage;
	public GameObject newVehiclePrefab;
	public bool menuOpened;
	public bool usingFlyMode;
	public bool usingJetpack;
	public bool usingSphereMode;
	otherPowers powersManager;
	grabObjects grabManager;
	scannerSystem scannerManager;
	playerController playerManager;
	GameObject player;
	changeGravity gravityManager;
	playerWeaponsManager weaponsManager;
	inputManager input;
	menuPause pauseManager;
	int currentState;
	int i;
	GameObject sphereVehicle;
	vehicleHUDManager vehicleManager;
	GameObject vehicleCamera;

	void Start () {
		//get the main components in the player
		player = GetComponentInChildren<playerController> ().gameObject;
		playerManager = player.GetComponent<playerController> ();
		powersManager = player.GetComponent<otherPowers> ();
		grabManager = player.GetComponent<grabObjects> ();
		scannerManager = player.GetComponent<scannerSystem> ();
		gravityManager = player.GetComponent<changeGravity> ();
		weaponsManager = player.GetComponent<playerWeaponsManager> ();
		input = GetComponent<inputManager> ();
		for (i = 0; i < playersMode.Count; i++) {
			if (playersMode [i].isCurrentState) {
				currentState = i;
			}
		}
		setNextPlayerMode ();
		playerControlModeMenu.SetActive (false);
		pauseManager = GetComponent<menuPause> ();
	}
	void Update(){
		if (!playerManager.driving) {
			if (input.checkInputButton ("Change Mode", inputManager.buttonType.getKeyDown)) {
				setModeIndex ();
			}
		}
		if (input.checkInputButton ("Change Player Control Mode", inputManager.buttonType.getKeyDown)) {
			openOrCloseControlMode (!menuOpened);
		}
	}
	public void openOrCloseControlMode(bool state){
		if ((!pauseManager.playerMenuActive || menuOpened) && ((!playerManager.driving && !usingSphereMode) || usingSphereMode)) {
			menuOpened = state;
			if (vehicleCamera) {
				vehicleCamera.GetComponent<vehicleCameraController> ().pauseOrPlayVehicleCamera (menuOpened);
			}
			pauseManager.openOrClosePlayerMenu (menuOpened);
			playerControlModeMenu.SetActive (state);
			pauseManager.showOrHideCursor (menuOpened);
			//disable the touch controls
			pauseManager.checkTouchControls (!menuOpened);
			//disable the camera rotation
			pauseManager.changeCameraState (!menuOpened);
			pauseManager.usingSubMenuState (menuOpened);
		}
	}
	public void openOrCLoseControlModeMenuFromTouch(){
		openOrCloseControlMode (!menuOpened);
	}
	public void getCurrentModeImage(GameObject button){
		currentPlayerControlModeImage.texture = button.transform.GetChild (0).GetComponent<RawImage> ().texture;
		RawImage currentModeSelected = button.transform.GetChild (0).GetComponent<RawImage> ();
		bool useJetPack = false;
		bool useSphereMode = false;
		bool useFlyMode = false;
		if (currentModeSelected == jetpackImage) {
			useJetPack = true;
		}else if (currentModeSelected == flyModeImage) {
			useFlyMode = true;
		} else if (currentModeSelected == sphereImage) {
			useSphereMode = true;
		}
		usingFlyMode = useFlyMode;
		usingJetpack = useJetPack;
		usingSphereMode = useSphereMode;
		player.GetComponent<jetpackSystem> ().enableOrDisableJetpack (useJetPack);
		player.GetComponent<flySystem> ().enableOrDisableFlyingMode (useFlyMode);
		if (playerManager.canUseSphereMode) {
			StartCoroutine (setVehicleState (useSphereMode));
		}
	}
	IEnumerator setVehicleState(bool state){
		if (state) {
			if (!sphereVehicle) {
				sphereVehicle = (GameObject)Instantiate (newVehiclePrefab, Vector3.one * 1000, Quaternion.identity);
				sphereVehicle.transform.GetChild (0).GetComponent<vehicleGravityControl> ().pauseDownForce (true);
				vehicleManager = sphereVehicle.transform.GetChild (0).GetComponent<vehicleHUDManager> ();
				vehicleCamera = sphereVehicle.transform.GetChild (1).gameObject;
				yield return new WaitForSeconds (0.00001f);
				sphereVehicle.transform.GetChild (0).GetComponent<Collider> ().enabled = false;
				sphereVehicle.transform.GetChild (0).GetComponent<vehicleGravityControl> ().pauseDownForce (false);
				Vector3 vehiclePosition = player.transform.position + player.transform.up;
				vehicleManager.gameObject.transform.position = vehiclePosition;
				vehicleCamera.transform.position = vehiclePosition;
				openOrCloseControlMode (false);
				playerManager.enableOrDisableSphereMode (true);
				vehicleManager.activateDevice ();
				yield return null;
			} else {
				if (!vehicleManager.gameObject.activeSelf) {
					Vector3 vehiclePosition = player.transform.position + player.transform.up;
					vehicleManager.gameObject.transform.position = vehiclePosition;
					vehicleCamera.transform.position = vehiclePosition;
					openOrCloseControlMode (false);
					playerManager.enableOrDisableSphereMode (true);
					vehicleManager.gameObject.SetActive (true);
					vehicleManager.activateDevice ();
					yield return null;
				}
			}
		} else {
			if (sphereVehicle) {
				if (vehicleManager.gameObject.activeSelf) {
					openOrCloseControlMode (false);
					vehicleManager.gameObject.transform.rotation = vehicleCamera.transform.rotation;
					playerManager.enableOrDisableSphereMode (false);
					vehicleManager.activateDevice ();
					vehicleManager.gameObject.SetActive (false);
					yield return null;
				} 
			}
		}
		player.GetComponent<usingDevicesSytem> ().driving = state;
		if (!gravityManager.settings.firstPersonView) {
			player.GetComponent<jetpackSystem> ().enableOrDisableJetPackMesh (!state);
			weaponsManager.enableOrDisableWeaponsMesh (!state);
			gravityManager.settings.arrow.SetActive (!state);
		} else {
			gravityManager.settings.meshCharacter.enabled = false;
		}
	}
	public void setNextPlayerMode(){
		if (!powersManager.aimsettings.aiming && !weaponsManager.carryingWeaponInThirdPerson && !weaponsManager.aimingInThirdPerson && !weaponsManager.carryingWeaponInFirstPerson && !weaponsManager.aimingInFirstPerson) {
			currentPlayerModeText.text = playersMode [currentState].nameMode;
			switch (playersMode [currentState].nameMode) {
			case "Powers":
				player.GetComponent<IKSystem> ().currentAimMode = IKSystem.aimMode.hands;
				player.GetComponent<closeCombatSystem> ().currentPlayerMode = false;
				weaponsManager.enableOrDisableWeaponsHUD (false);
				break;
			case "Weapons":
				player.GetComponent<IKSystem> ().currentAimMode = IKSystem.aimMode.weapons;
				player.GetComponent<closeCombatSystem> ().currentPlayerMode = false;
				break;
			case "Combat":
				player.GetComponent<IKSystem> ().currentAimMode = IKSystem.aimMode.hands;
				player.GetComponent<closeCombatSystem> ().currentPlayerMode = true;
				weaponsManager.enableOrDisableWeaponsHUD (false);
				break;
			}
			for (i = 0; i < playersMode.Count; i++) {
				if (i == currentState) {
					playersMode [i].isCurrentState = true;
				} else {
					playersMode [i].isCurrentState = false;
				}
			}
		}
	}
	public void setModeIndex(){
		currentState++;
		if (currentState > playersMode.Count - 1) {
			currentState = 0;
		}
		setNextPlayerMode ();
	}
	//check every possible state that must not keep enabled when the player is going to make a certain action, like drive
	public void checkPlayerStates(){
		//disable weapons
		disableWeapons();
		//disable the aim mode
		disableAimMode ();
		//disable the grab mode of one single object
		disableGrabMode ();
		//disable the grab mode when the player is carrying more than one object
		disableScannerMode ();
		//set the iddle state in the animator
		resetAnimatorState ();
		//disable the gravity power
		disableGravityPower();
		//disable powers states
		disablePowers();
		//set the normal mode for the player, to disable the jetpack and the sphere mode
		disablePlayerModes();
	}

	public void disableWeapons(){
		weaponsManager.disableCurrentWeapon ();
	}

	public void disableAimMode(){
		if (powersManager.aimsettings.aiming) {
			powersManager.deactivateAimMode ();
		}
	}

	public void disableGrabMode(){
		if (grabManager.objectHeld) {
			grabManager.dropObject ();
		}
		if (powersManager.carryingObjects) {
			powersManager.dropObjects ();
		}
	}

	public void disableScannerMode(){
		if (scannerManager.activate) {
			scannerManager.disableScanner ();
		}
	}
	public void resetAnimatorState(){
		if (playerManager.crouch) {
			playerManager.crouching ();
			playerManager.animator.SetBool ("Crouch", false);
		}
		playerManager.animator.SetFloat ("Jump", 0);
		playerManager.animator.SetFloat ("JumpLeg", 0);
		playerManager.animator.SetFloat ("Turn", 0);
		playerManager.animator.SetFloat ("Forward",0);
		playerManager.animator.SetBool ("OnGround", true);
	}
	public void disableGravityPower(){
		if (gravityManager.powerActive) {
			gravityManager.stopGravityPower ();
		}
	}
	public void disablePowers(){
		if(powersManager.running){
			powersManager.stopRun();
		}
		if (powersManager.activatedShield) {
			powersManager.activateShield ();
		}
	}
	public void disablePlayerModes(){
		if (!usingSphereMode) {
			getCurrentModeImage (normalModeImage.transform.parent.gameObject);
		}
	}
	[System.Serializable]
	public class playerMode{
		public string nameMode;
		public bool isCurrentState;
	}
}
