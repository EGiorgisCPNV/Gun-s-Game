using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//now the menu uses the UI from unity 4.6 with more options and a more complete interface which adjust to every screen size
public class menuPause : MonoBehaviour {
	public bool menuPauseEnabled;
	public bool changeControlsEnabled;
	public bool vanish = true;
	public GameObject hudAndMenus;
	public GameObject touchPanel;
	public GameObject pauseMenu;
	public GameObject controlsMenu;
	public GameObject touchOptionsMenu;
	public GameObject editInputControlMenu;
	public GameObject exitToHomeMenu;
	public GameObject exitToDesktopMenu;
	public GameObject dieMenu;
	public GameObject accelerometerSwitch;
	[HideInInspector] public bool pauseGame = false;
	[HideInInspector] public bool usingDevice;
	[HideInInspector] public bool usingSubMenu;
	[HideInInspector] public bool useTouchControls = false;
	[HideInInspector] public bool playerMenuActive;
	bool showGUI = false;
	bool dead;
	bool subMenuActive;
	Color alpha;
	GameObject blackBottom;
	GameObject player;
	GameObject cam;
	inputManager input;
	public cursorStateInfo cursorState;
	powersListManager powerList;
	Image blackBottomImage;
	playerController playerControllerManager;
	playerCamera playerCameraManager;
	editControlPosition editControlPositionManager;
	inventoryManager playerInventoryManager;
	friendListManager friendsManager;
	mapSystem mapManager;
	playerStatesManager playerStates;
	timeBullet timeManager;

	void Start () {
		AudioListener.pause = false;
		player=GameObject.Find("Player Controller");
		cam=GameObject.Find("Player Camera");
		playerCameraManager = cam.GetComponent<playerCamera> ();
		playerControllerManager = player.GetComponent <playerController> ();
		blackBottom=GameObject.Find("blackBottom");
		blackBottomImage = blackBottom.GetComponent<Image> ();
		blackBottomImage.enabled=true;
		input = GetComponent<inputManager> ();
		if (!useTouchControls) {
			showOrHideCursor (false);
		} else {
			enableOrDisableTouchControls(true);
		}
		editControlPositionManager = hudAndMenus.GetComponent<editControlPosition> ();
		editControlPositionManager.getTouchButtons(useTouchControls);
		alpha.a=1;
		Time.timeScale=1;
		//if the fade of the screen is disabled, just set the alpha of the black panel to 0
		if(!vanish){
			alpha.a =0;
			blackBottomImage.color = alpha;
			blackBottomImage.enabled=false;
		}
		pauseMenu.SetActive(false);
		controlsMenu.SetActive(false);
		exitToDesktopMenu.SetActive(false);
		touchOptionsMenu.SetActive(false);
		editInputControlMenu.SetActive(false);
		setCurrentCameraState (true);
		//if the accelerometer is disabled, set the value in the menu to disabled
		if (!playerCameraManager.settings.useAcelerometer) {
			accelerometerSwitch.GetComponent<Scrollbar>().value=0;
		}
		powerList = GetComponent<powersListManager> ();
		playerInventoryManager = player.GetComponent<inventoryManager> ();
		friendsManager = player.GetComponent<friendListManager> ();
		mapManager = GetComponent<mapSystem> ();
		playerStates = GetComponent<playerStatesManager> ();
		timeManager = GetComponent<timeBullet> ();
	}
	//save the previous and the current visibility of the cursor, to enable the mouse cursor correctly when the user enables the touch controls, or using a device
	//or editing the powers, or open the menu, or any action that enable and disable the mouse cursor
	void setCurrentCursorState(bool curVisible){
		cursorState.currentVisible = curVisible;
	}
	void setPreviousCursorState(bool prevVisible){
		cursorState.previousVisible = prevVisible;
	}
	//like the mouse, save the state of the camera, to prevent rotate it when a menu is enabled, or using a device, or the player is dead, etc...
	void setCurrentCameraState(bool curCamera){
		cursorState.currentCameraEnabled = curCamera;
	}
	void setPreviousCameraState(bool prevCamera){
		cursorState.previousCameraEnabled = prevCamera;
	}
	void Update () {
		//if the fade is enabled, decrease the value of alpha to get a nice fading effect at the beginning of the game
		if(vanish){
			alpha.a -=Time.deltaTime/2;
			blackBottomImage.color = alpha;
			if(alpha.a<=0){
				blackBottomImage.enabled=false;
				vanish=false;
			}
		}
		//if the pause key is pressed, pause the game
		if(input.checkInputButton ("Pause", inputManager.buttonType.getKeyDown)){
			if(!subMenuActive){
				//if the main pause menu is the current place, resuem the game
				pause();
				return;
			}
			else{
				//else, the current menu place is a submenu, so disable all the submenus and set the main menu window
				showGUI=true;
				subMenuActive=false;
				pauseMenu.SetActive(true);
				controlsMenu.SetActive(false);
				exitToHomeMenu.SetActive (false);
				exitToDesktopMenu.SetActive(false);
				touchOptionsMenu.SetActive(false);
				editInputControlMenu.SetActive(false);
			}
			//disable the edition of the touch button position if the player backs from that menu option
			editControlPositionManager.disableEdit();
		}
		//if the mouse is showed, press in the screen to lock it again
		if(!pauseGame){
			//check that the touch controls are disabled, the player is not dead, the powers is not being editing or selecting, the player is not using a device
			//or the cursor is visible
			if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && !useTouchControls && !dead && inGameWindowOpened()
				&& (Cursor.lockState == CursorLockMode.None || Cursor.visible )){
				showOrHideCursor(false);
			}
		}
		//change between touch controls and the keyboard
		if (input.checkInputButton ("Change Controls", inputManager.buttonType.getKeyDown)) {
			useTouchControls=!useTouchControls;
			showOrHideCursor(useTouchControls);
			enableOrDisableTouchControls(useTouchControls);
		}
	}
	public bool inGameWindowOpened(){
		bool opened = false;
		if(!powerList.editingPowers && !powerList.selectingPower && !usingDevice && !playerInventoryManager.inventoryOpened 
			&& !mapManager.mapOpened && !playerStates.menuOpened && !friendsManager.menuOpened){
			opened = true;
		}
		return opened;
	}
	//get the scroller value in the touch options menu if the player enables or disables the accelerometer
	public void getAccelerometerScrollerValue(Scrollbar info){
		if(info.value<0.5f){
			playerCameraManager.settings.useAcelerometer=false;
			if(info.value>0){
				info.value=0;
			}
		}
		else{
			playerCameraManager.settings.useAcelerometer=true;
			if(info.value<1){
				info.value=1;
			}
		}
	}
	//set the configuration of both joysticks in the touch options menu, so the joysticks can be configured ingame
	public void getToggleJoysticksValue(Toggle info){
		string name = info.name;
		switch (name) {
		case "leftSnap":
			input.touchMovementControl.snapsToFinger = info.isOn;
			break;
		case "leftHide":
			input.touchMovementControl.hideOnRelease = info.isOn;
			break;
		case "leftPad":
			input.touchMovementControl.touchPad = info.isOn;
			break;
		case "leftShow":
			input.touchMovementControl.showJoystick = info.isOn;
			break;
		case "rightSnap":
			input.touchCameraControl.snapsToFinger = info.isOn;
			break;
		case "rightHide":
			input.touchCameraControl.hideOnRelease = info.isOn;
			break;
		case "rightPad":
			input.touchCameraControl.touchPad = info.isOn;
			break;
		case "rightShow":
			input.touchCameraControl.showJoystick = info.isOn;
			break;
		}
	}
	//get the values from the touch joystick sensitivity in the touch options menu when the player adjust the joysticks sensitivity
	public void getRightSensitivityValue(Slider info){
		info.transform.GetChild (0).GetComponent<Text> ().text = info.value.ToString("0.#");
		//set the values in the input manager
		input.rightTouchSensitivity = info.value;
	}
	public void getLeftSensitivityValue(Slider info){
		info.transform.GetChild (0).GetComponent<Text> ().text = info.value.ToString("0.#");
		//set the values in the input manager
		input.leftTouchSensitivity = info.value;
	}
	//get the mouse sensitivity value when the player adjusts it in the edit input menu
	public void getMouseSensitivityValue(Slider info){
		info.transform.GetChild (0).GetComponent<Text> ().text = info.value.ToString("0.#");
		//set the values in the input manager
		input.mouseSensitivity = info.value;
	}
	//set in the player is using a device like a computer or a text device
	public void usingDeviceState(bool state){
		usingDevice = state;
		playerControllerManager.setLastTimeMoved ();
	}
	public void usingSubMenuState(bool state){
		usingSubMenu = state;
	}
	public void pause(){
		//check if the game is going to be paused or resumed
		if (!dead && menuPauseEnabled) {
			//if the player pauses the game and he is editing the powers or selecting them, disable the power manager menu
			if(powerList.editingPowers){
				powerList.editPowersSlots();
			}
			if(powerList.selectingPower){
				powerList.selectPowersSlots();
			}
			if (playerInventoryManager.inventoryOpened) {
				playerInventoryManager.openOrCloseInventory (false);
			}
			if(mapManager.mapOpened){
				mapManager.openOrCloseMap(false);
			}
			if (playerStates.menuOpened) {
				playerStates.openOrCloseControlMode (false);
			}
			if (friendsManager.menuOpened) {
				friendsManager.openOrCloseFriendMenu (false);
			}
			pauseGame = !pauseGame;
			showGUI = !showGUI;
			AudioListener.pause = pauseGame;
			//enable or disable the main pause menu
			pauseMenu.SetActive(!pauseMenu.activeSelf);
			//change the camera state
			changeCameraState (!pauseGame);
			//check if the touch controls were enabled
			if (!useTouchControls) {
				showOrHideCursor(pauseGame);
			}
			input.setPauseState (pauseGame);
			//pause game
			if (pauseGame) {
				timeManager.disableTimeBullet ();
				Time.timeScale = 0;
				alpha.a = 0.5f;
				//fade a little to black an UI panel
				blackBottomImage.enabled=true;
				blackBottomImage.color = alpha;
				//disable the event triggers in the touch buttons
				editControlPositionManager.changeButtonsState(false);
			}
			//resume game
			if (!pauseGame) {
				Time.timeScale = 1;	 	
				alpha.a = 0;
				//fade to transparent the UI panel
				blackBottomImage.enabled=false;
				blackBottomImage.color = alpha;
				//enable the event triggers in the touch buttons
				editControlPositionManager.changeButtonsState(true);
				timeManager.reActivateTime ();
			}
			vanish = false;
		}
	}
	//set the state of the cursor, according to if the touch controls are enabled, if the game is pause, if the powers manager menu is enabled, etc...
	//so the cursor is always locked and not visible correctly and vice versa
	public void showOrHideCursor(bool value){
		//use the correct code in unity 5
		#if UNITY_5
		if(cursorState.currentVisible && cursorState.previousVisible){
			setPreviousCursorState(false);
			setCurrentCursorState(true);
			return;
		}
		if(cursorState.currentVisible && useTouchControls){
			setPreviousCursorState(false);
			setCurrentCursorState(true);
			return;
		}
		if(value){
			Cursor.lockState = CursorLockMode.None;
		}
		else{
			Cursor.lockState=CursorLockMode.Confined;
			Cursor.lockState = CursorLockMode.Locked;
		}
		setPreviousCursorState(Cursor.visible);
		Cursor.visible = value;
		setCurrentCursorState(Cursor.visible);
		#else
		//use the code of unity 4
		//if both states of the cursor is true, it means that the player is resuming the game when the cursor was visible, so keep the mouse cursor visible
		if(cursorState.currentVisible && cursorState.previousVisible){
			setPreviousCursorState(false);
			setCurrentCursorState(true);
			return;
		}
		//else, the touchcontrols were enabled, so keep the cursor visible
		if(cursorState.currentVisible && useTouchControls){
			setPreviousCursorState(false);
			setCurrentCursorState(true);
			return;
		}
		//else, the cursor was invisible before pausing the game, so save the previous and the current state and change the cursor visibility
		setPreviousCursorState(Cursor.visible);
		Screen.lockCursor = !value;
		Cursor.visible = value;
		setCurrentCursorState(Cursor.visible);
		#endif
	}
	//check if the touch controls have to be enable and disable and change the cursor visibility according to that
	public void checkTouchControls(bool state){
		if (useTouchControls) {
			enableOrDisableTouchControls (state);
			showOrHideCursor(useTouchControls);
		}
	}
	//the player dies, so enable the death menu to ask the player to play again
	public void death(){
		dead = true;
		showOrHideCursor (true);
		dieMenu.SetActive(true);
		changeCameraState (false);
	}
	//the player chooses to play again
	public void getUp(){
		dead = false;
		if (!useTouchControls) {
			showOrHideCursor (false);
		}
		changeCameraState (true);
	}
	//restart the scene
	public void restart(){
		pause();
		SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex);
	}
	//change the camera state according to if the player pauses the game or uses a device, etc... so the camera is enabled correctly according to every situation
	public void changeCameraState(bool state){
		if (playerCameraManager) {
			//if the player paused the game using a device, then resume again with the camera disable to keep using that device
			if (!cursorState.currentCameraEnabled && !cursorState.previousCameraEnabled) {
				setPreviousCameraState (true);
			}
			//else save the current and previous state of the camera and set the state of the camera according to the current situation
			else {
				setPreviousCameraState (playerCameraManager.cameraCanBeUsed);
				playerCameraManager.pauseOrPlayCamera (state);
				setCurrentCameraState (playerCameraManager.cameraCanBeUsed);
			}
		}
	}
	public void openOrClosePlayerMenu(bool state){
		playerMenuActive = state;
	}
	//the player is in a submenu, so disable the main menu
	public void enterSubMenu(){
		showGUI=false;
		subMenuActive=true;
	}
	//the player backs from a submenu, so enable the main menu
	public void exitSubMenu(){
		showGUI=true;
		subMenuActive=false;
	}
	//switch between touch controls and the keyboard
	public void switchControls(){
		useTouchControls=!useTouchControls;
		enableOrDisableTouchControls(useTouchControls);
		pause ();
	}
	//exit from the game
	public void confirmExit(){
		Application.Quit();
	}
	public void confirmGoToHomeMenu(){
		SceneManager.LoadScene (0);
	}
	//enable or disable the joysticks and the touch buttons in the HUD
	public void enableOrDisableTouchControls(bool state){
		input.changeControlsType(state);
		touchPanel.SetActive(state);
	}
	public void reloadStart(){
		Start ();
	}
	//a class to save the current and previous state of the mouse visibility and the state of the camera, to enable and disable them correctly according to every
	//type of situation
	[System.Serializable]
	public class cursorStateInfo{
		public bool currentVisible;
		public bool previousVisible;
		public bool currentCameraEnabled;
		public bool previousCameraEnabled;
	}
}