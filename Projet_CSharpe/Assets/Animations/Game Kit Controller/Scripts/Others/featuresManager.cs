using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
[System.Serializable]
public class featuresManager : MonoBehaviour {
	//this script allows to enable and disable all the features in this asset, so you can configure which of them you need and which you don't
	[Header ("Player Controller Features")]
	public pControllerSettings PlayerController;
	[Header ("Player Camera Features")]
	public pCameraSettings PlayerCamera;
	[Header ("Gravity Control Features")]
	public GravitySettings GravityControl;
	[Header ("Powers Features")]
	public PowersSettings PowersSystem;
	[Header ("Grab Object Features")]
	public GrabObjectSettings GrabObjects;
	[Header ("Devices System Features")]
	public DevicesSystemSettings DevicesSystem;
	[Header ("Close Combat System Features")]
	public CombatSytemSettings CombatSystem;
	[Header ("Scanner System Features")]
	public ScannerSytemSettings ScannerSystem;
	[Header ("Pick Ups Info Features")]
	public PickUpsScreenInfoSettings PickUps;
	[Header ("Powers Manager Features")]
	public PowersManagerSettings PowersManager;
	[Header ("Map Features")]
	public MapSettings Map;
	[Header ("TimeBullet Features")]
	public TimeBulletSettings TimeBullet;
	[Header ("Menu Features")]
	public menuPauseSettings MenuPause;

	//this script uses parameters inside the player, the camera, the map and the character (the parent of the player)
	GameObject pController;
	GameObject pCamera;
	GameObject map;
	GameObject mapContent;
	GameObject character;
	GameObject HUD;

	playerController playerControllerManager;
	playerCamera playerCameraManager;
	otherPowers powersManager;
	changeGravity gravityManager;
	grabObjects grabObjectsManager;
	usingDevicesSytem usingDevicesManager;
	closeCombatSystem combatManager;
	scannerSystem scannerManager;
	pickUpsScreenInfo pickUpsScreenInfoManager;
	mapSystem mapManager;
	timeBullet timeBulletManager;
	powersListManager powerListManager;
	menuPause menuPauseManager;

	void Start () {
		//setConfiguration ();
	}
	//set the options that the user has configured in the inspector
	public void setConfiguration(){
		//search the component that has the values to enable or disable
		searchComponent ();
		//Player Controller
		playerControllerManager.enabledDoubleJump = PlayerController.doubleJump;
		playerControllerManager.damageFallEnabled = PlayerController.fallDamage;
		playerControllerManager.holdJumpSlowDownFall = PlayerController.holdJumpToSlowDownFall;
		//Player Camera
		playerCameraManager.settings.zoomEnabled = PlayerCamera.zoomCamera;
		playerCameraManager.settings.moveAwayCameraEnabled = PlayerCamera.moveAwayCamera;
		playerCameraManager.settings.enableShakeCamera = PlayerCamera.shakeCamera;
		playerCameraManager.settings.enableMoveAwayInAir = PlayerCamera.moveAwayCameraInAir;
		playerCameraManager.settings.useAcelerometer = PlayerCamera.useAccelerometer;
		//Gravity System
		gravityManager.settings.gravityPowerEnabled = GravityControl.gravityPower;
		//Powers
		powersManager.settings.runPowerEnabled = PowersSystem.runPower;
		powersManager.settings.aimModeEnabled = PowersSystem.aimMode;
		powersManager.settings.shieldEnabled = PowersSystem.shield;
		powersManager.settings.grabObjectsEnabled = PowersSystem.grabObjectsThirdMode;
		powersManager.settings.changeCameraViewEnabled = PowersSystem.changeCameraView;
		powersManager.settings.shootEnabled = PowersSystem.shoot;
		powersManager.settings.changePowersEnabled = PowersSystem.changePowers;
		//Grab Objects
		grabObjectsManager.settings.grabObjectsEnabled = GrabObjects.grabObjectsAimMode;
		//Using Devices System
		usingDevicesManager.canUseDevices = DevicesSystem.devicesSystem;
		//Close Combat System
		combatManager.combatSystemEnabled = CombatSystem.combatSystem;
		//Scanner System
		scannerManager.scannerSystemEnabled = ScannerSystem.scannerSystem;
		//Pick Ups Screen Info
		pickUpsScreenInfoManager.pickUpScreenInfoEnabled = PickUps.pickUpScreenInfo;
		//Map System
		map.transform.GetChild (0).gameObject.SetActive (Map.mapActive);
		map.transform.GetChild (1).gameObject.SetActive (Map.mapActive);
		mapContent.transform.GetChild (0).gameObject.SetActive (Map.mapActive);
		mapManager.mapEnabled = Map.mapActive;
		//Time Bullet
		timeBulletManager.timeBulletEnabled = TimeBullet.timeBullet;
		//Power List Manager
		powerListManager.powerListManagerEnabled = PowersManager.powersManager;
		//Pause Menu
		menuPauseManager.menuPauseEnabled = MenuPause.menuPause;
		//Player Health Hud
		HUD.transform.GetChild (0).gameObject.SetActive (MenuPause.healthHUD);
		//Change between keyboard and touch controls
		menuPauseManager.changeControlsEnabled = MenuPause.changeKeyboardTouchControls;
		//upload every change object in the editor
		updateComponents();
	}
	//get the current values of the features, to check the if the booleans fields are correct or not
	public void getConfiguration(){
		searchComponent ();
		PlayerController.doubleJump = playerControllerManager.enabledDoubleJump;
		PlayerController.fallDamage = playerControllerManager.damageFallEnabled;
		PlayerCamera.zoomCamera = playerCameraManager.settings.zoomEnabled;
		PlayerCamera.moveAwayCamera = playerCameraManager.settings.moveAwayCameraEnabled;
		PlayerCamera.shakeCamera = playerCameraManager.settings.enableShakeCamera;
		PlayerCamera.moveAwayCameraInAir = playerCameraManager.settings.enableMoveAwayInAir; 
		PlayerCamera.useAccelerometer = playerCameraManager.settings.useAcelerometer;
		GravityControl.gravityPower = gravityManager.settings.gravityPowerEnabled;
		PowersSystem.runPower = powersManager.settings.runPowerEnabled;
		PowersSystem.aimMode = powersManager.settings.aimModeEnabled;
		PowersSystem.shield = powersManager.settings.shieldEnabled;
		PowersSystem.grabObjectsThirdMode = powersManager.settings.grabObjectsEnabled;
		PowersSystem.changeCameraView = powersManager.settings.changeCameraViewEnabled;
		PowersSystem.shoot = powersManager.settings.shootEnabled;
		PowersSystem.changePowers = powersManager.settings.changePowersEnabled;
		GrabObjects.grabObjectsAimMode = grabObjectsManager.settings.grabObjectsEnabled;
		DevicesSystem.devicesSystem = usingDevicesManager.canUseDevices;
		CombatSystem.combatSystem = combatManager.combatSystemEnabled;
		ScannerSystem.scannerSystem = scannerManager.scannerSystemEnabled;
		PickUps.pickUpScreenInfo = pickUpsScreenInfoManager.pickUpScreenInfoEnabled;

		Map.mapActive = map.transform.GetChild(0).gameObject.activeSelf;

		TimeBullet.timeBullet = timeBulletManager.timeBulletEnabled;
		PowersManager.powersManager = powerListManager.powerListManagerEnabled;
		MenuPause.menuPause = menuPauseManager.menuPauseEnabled;
		MenuPause.healthHUD = HUD.transform.GetChild (0).gameObject.activeSelf;
		MenuPause.changeKeyboardTouchControls = menuPauseManager.changeControlsEnabled;
	}

	public void updateComponents(){
		EditorUtility.SetDirty (pController.GetComponent<playerController> ());
		EditorUtility.SetDirty (pCamera.GetComponent<playerCamera> ());
		EditorUtility.SetDirty (pController.GetComponent<changeGravity> ());
		EditorUtility.SetDirty (pController.GetComponent<otherPowers> ());
		EditorUtility.SetDirty (pController.GetComponent<grabObjects> ());
		EditorUtility.SetDirty (pController.GetComponent<usingDevicesSytem> ());
		EditorUtility.SetDirty (pController.GetComponent<closeCombatSystem> ());
		EditorUtility.SetDirty (pController.GetComponent<scannerSystem> ());
		EditorUtility.SetDirty (pController.GetComponent<pickUpsScreenInfo> ());
		EditorUtility.SetDirty (character.GetComponent<timeBullet> ());
		EditorUtility.SetDirty (character.GetComponent<mapSystem> ());
		EditorUtility.SetDirty (character.GetComponent<powersListManager> ());
		EditorUtility.SetDirty (character.GetComponent<menuPause> ());
	}
		
	void searchComponent(){
		//if any of the component is not assigned, serach them
		if (!pController || !pCamera || !map || !character || !HUD || !mapContent) {
			pController = GameObject.Find ("Player Controller");
			pCamera = GameObject.Find ("Player Camera");
			map = GameObject.Find ("mapSystem");
			mapContent = GameObject.Find ("map");
			character = GameObject.Find ("Character");
			HUD = GameObject.Find ("playerInfo");
		} 

		if (pController && pCamera && map && character && HUD && mapContent) {
			playerControllerManager = pController.GetComponent<playerController> ();
			playerCameraManager = pCamera.GetComponent<playerCamera> ();
			gravityManager = pController.GetComponent<changeGravity> ();
			powersManager = pController.GetComponent<otherPowers> ();
			grabObjectsManager = pController.GetComponent<grabObjects> ();
			usingDevicesManager = pController.GetComponent<usingDevicesSytem> ();
			combatManager = pController.GetComponent<closeCombatSystem> ();
			scannerManager = pController.GetComponent<scannerSystem> ();
			pickUpsScreenInfoManager = pController.GetComponent<pickUpsScreenInfo> ();
			timeBulletManager = character.GetComponent<timeBullet> ();
			mapManager = character.GetComponent<mapSystem> ();
			powerListManager = character.GetComponent<powersListManager> ();
			menuPauseManager = character.GetComponent<menuPause> ();
		}
	}
	//every script has its own serializable class, so the changing of features is easier
	[System.Serializable]
	public class pControllerSettings{
		public bool doubleJump;
		public bool fallDamage;
		public bool holdJumpToSlowDownFall;
	}
	[System.Serializable]
	public class pCameraSettings{
		public bool zoomCamera;
		public bool moveAwayCamera;
		public bool shakeCamera;
		public bool moveAwayCameraInAir;
		public bool useAccelerometer;
	}
	[System.Serializable]
	public class GravitySettings{
		public bool gravityPower;
	}
	[System.Serializable]
	public class PowersSettings{
		public bool runPower;
		public bool aimMode;
		public bool shield;
		public bool grabObjectsThirdMode;
		public bool changeCameraView;
		public bool shoot;
		public bool changePowers;
	}
	[System.Serializable]
	public class GrabObjectSettings{
		public bool grabObjectsAimMode;
	}
	[System.Serializable]
	public class DevicesSystemSettings{
		public bool devicesSystem;
	}
	[System.Serializable]
	public class CombatSytemSettings{
		public bool combatSystem;
	}
	[System.Serializable]
	public class ScannerSytemSettings{
		public bool scannerSystem;
	}
	[System.Serializable]
	public class PickUpsScreenInfoSettings{
		public bool pickUpScreenInfo;
	}
	[System.Serializable]
	public class PowersManagerSettings{
		public bool powersManager;
	}
	[System.Serializable]
	public class MapSettings{
		public bool mapActive;
	}
	[System.Serializable]
	public class TimeBulletSettings{
		public bool timeBullet;
	}
	[System.Serializable]
	public class menuPauseSettings{
		public bool menuPause;
		public bool healthHUD;
		public bool changeKeyboardTouchControls;
	}
}
#endif