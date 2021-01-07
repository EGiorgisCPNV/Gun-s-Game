using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class playerWeaponsManager : MonoBehaviour
{
	public bool carryingWeaponInThirdPerson;
	public bool carryingWeaponInFirstPerson;
	public bool aimingInThirdPerson;
	public bool aimingInFirstPerson;
	public bool shooting;
	public int weaponsSlotsAmount;
	public GameObject weaponsHUD;
	public Text currentWeaponNameText;
	public Text currentWeaponAmmoText;
	public Slider ammoSlider;
	public string currentWeaponName;
	public Transform weaponsParent;
	public Transform weaponsTransformInFirstPerson;
	public Transform weaponsTransformInThirdPerson;
	public Transform thirdPersonParent;
	public Transform firstPersonParent;
	public Transform cameraController;
	public Camera weaponsCamera;
	public string weaponsLayer;
	public Vector2 touchZoneSize;
	public float minSwipeDist;
	public bool touching;
	public bool showGizmo;
	public Color gizmoColor;
	public bool anyWeaponAvaliable;
	public List<IKWeaponSystem> weaponsList = new List<IKWeaponSystem> ();
	public IKWeaponSystem currentIKWeapon;
	public playerWeaponSystem currentWeaponSystem;
	GameObject swipeCenterPosition;
	Vector3 swipeStartPos;
	Quaternion originalWeaponsParentRotation;
	float originalFov;
	float originalWeaponsCameraFov;
	float lastTimeFired;
	public int choosedWeapon = 0;
	bool changingWeapon;
	bool keepingWeapon;
	bool isThirdPersonView;
	bool touchPlatform;
	changeGravity gravityManager;
	inputManager input;
	IKSystem IKManager;
	playerController playerManager;
	otherPowers powersManager;
	menuPause pauseManager;
	playerCamera playerCameraManager;
	Coroutine cameraFovCoroutine;
	Touch currentTouch;
	Rect touchZoneRect;

	void Start ()
	{
		input = transform.parent.GetComponent<inputManager> ();
		IKManager = GetComponent<IKSystem> ();
		powersManager = GetComponent<otherPowers> ();
		playerManager = GetComponent<playerController> ();
		pauseManager = transform.parent.GetComponent<menuPause> ();
		playerCameraManager = cameraController.GetComponent<playerCamera> ();
		gravityManager = GetComponent<changeGravity> ();
		anyWeaponAvaliable = checkWeaponsAvaliable ();
		if (anyWeaponAvaliable) {
			getCurrentWeapon ();
			powersManager.getCurrentWeapon (currentIKWeapon);
		}
		originalFov = Camera.main.fieldOfView;
		touchPlatform = touchJoystick.checkTouchPlatform ();
		setHudZone ();
		originalWeaponsCameraFov = weaponsCamera.fieldOfView;
	}

	void Update ()
	{
		if (IKManager.currentAimMode == IKSystem.aimMode.weapons) {
			isThirdPersonView = !gravityManager.settings.firstPersonView;
			if (anyWeaponAvaliable) {
				if (currentIKWeapon.currentWeapon) {
					if (carryingWeaponInThirdPerson || carryingWeaponInFirstPerson) {
						if (input.checkInputButton ("Aim", inputManager.buttonType.getKeyDown)) {
							if (isThirdPersonView) {
								aimCurrentWeapon (!aimingInThirdPerson);
							} else {
								aimCurrentWeapon (!aimingInFirstPerson);
							}
						}
						if (input.checkInputButton ("Secondary Button", inputManager.buttonType.getKeyDown)) {
							aimCurrentWeapon (true);
						}
						if (input.checkInputButton ("Secondary Button", inputManager.buttonType.getKeyUp)) {
							aimCurrentWeapon (false);
						}
						if (input.checkInputButton ("Drop Weapon", inputManager.buttonType.getKeyUp)) {
							dropWeapon ();
						}
					}
					if (aimingInThirdPerson || carryingWeaponInFirstPerson) {
						if (input.checkInputButton ("Gravity Power On", inputManager.buttonType.getKeyDown)) {
							currentWeaponSystem.manualReload ();
						}
						if (currentWeaponSystem.weaponSettings.automatic) {
							if (input.checkInputButton ("Shoot", inputManager.buttonType.getKey)) {
								shootWeapon (true);
							}
						} else {
							if (input.checkInputButton ("Shoot", inputManager.buttonType.getKeyDown)) {
								shootWeapon (true);
							}
						}
						if (input.checkInputButton ("Shoot", inputManager.buttonType.getKeyUp)) {
							shootWeapon (false);
						}
					}
					if (input.checkInputButton ("Draw Weapon", inputManager.buttonType.getKeyDown)) {
						if (isThirdPersonView) {
							drawOrKeepWeapon (!carryingWeaponInThirdPerson);
						} else {
							drawOrKeepWeapon (!carryingWeaponInFirstPerson);
						}
					}
					if (carryingWeaponInFirstPerson || carryingWeaponInThirdPerson) {
						//if the touch controls are enabled, activate the swipe option
						if (input.touchControlsCurrentlyEnabled) {
							//select the weapon by swiping the finger in the right corner of the screen, above the weapon info
							int touchCount = Input.touchCount;
							if (!touchPlatform) {
								touchCount++;
							}
							for (int i = 0; i < touchCount; i++) {
								if (!touchPlatform) {
									currentTouch = touchJoystick.convertMouseIntoFinger ();
								} else {
									currentTouch = Input.GetTouch (i);
								}
								//get the start position of the swipe
								if (currentTouch.phase == TouchPhase.Began) {
									if (touchZoneRect.Contains (currentTouch.position) && !touching) {
										swipeStartPos = currentTouch.position;
										touching = true;
									}
								}
								//and the final position, and get the direction, to change to the previous or the next power
								if (currentTouch.phase == TouchPhase.Ended && touching) {
									float swipeDistHorizontal = (new Vector3 (currentTouch.position.x, 0, 0) - new Vector3 (swipeStartPos.x, 0, 0)).magnitude;
									if (swipeDistHorizontal > minSwipeDist) {
										float swipeValue = Mathf.Sign (currentTouch.position.x - swipeStartPos.x);
										if (swipeValue > 0) {
											//right swipe
											choosePreviousWeapon ();
										} else if (swipeValue < 0) {
											//left swipe
											chooseNextWeapon ();
										}
									}
									touching = false;
								}
							}
						}
					} 
				}
				if (!aimingInThirdPerson && !aimingInFirstPerson) {
					for (int i = 0; i < weaponsSlotsAmount; i++) {
						if (Input.GetKeyDown ("" + (i + 1))) {
							for (int k = 0; k < weaponsList.Count; k++) {
								if (weaponsList [k].weapon.weaponSettings.numberKey == (i + 1) && choosedWeapon != k) {
									if (weaponsList [k].weaponEnabled) {
										choosedWeapon = k;
										currentIKWeapon.currentWeapon = false;
										if (carryingWeaponInThirdPerson || carryingWeaponInFirstPerson) {
											changingWeapon = true;
										} else {
											weaponChanged ();
										}
									}
								}
							}
						}
					}
					//select the power using the mouse wheel or the change power buttons
					if (input.checkInputButton ("Next Power", inputManager.buttonType.posMouseWheel)) {
						chooseNextWeapon ();
					}
					if (input.checkInputButton ("Previous Power", inputManager.buttonType.negMouseWheel)) {
						choosePreviousWeapon ();
					}
					if (input.checkInputButton ("Next Power", inputManager.buttonType.getKeyDown)) {
						chooseNextWeapon ();
					}
					if (input.checkInputButton ("Previous Power", inputManager.buttonType.getKeyDown)) {
						choosePreviousWeapon ();
					}
				}
			}
		}
		if (changingWeapon) {
			if (!keepingWeapon) {
				drawOrKeepWeapon (false);
				keepingWeapon = true;
				//print ("keep weapon");
			}
			if (!currentIKWeapon.moving) {
				weaponChanged ();
				drawOrKeepWeapon (true);
				keepingWeapon = false;
				changingWeapon = false;
			}
		}
	}

	public void shootWeapon (bool state)
	{
		if (!pauseManager.usingDevice && !pauseManager.usingSubMenu) {
			if (state) {
				if (!currentWeaponSystem.reloading && currentWeaponSystem.weaponSettings.clipSize > 0) {
					shooting = true;
				} else {
					shooting = false;
				}
				currentWeaponSystem.shootWeapon (isThirdPersonView);
				setLastTimeFired ();
			} else {
				shooting = false;
			}
		}
	}

	public void setLastTimeFired ()
	{
		lastTimeFired = Time.time;
	}

	public float getLastTimeFired ()
	{
		return lastTimeFired;
	}

	void FixedUpdate ()
	{
		if (IKManager.currentAimMode == IKSystem.aimMode.weapons && anyWeaponAvaliable) {
			if (currentIKWeapon.currentWeapon && carryingWeaponInFirstPerson) {
				currentWeaponSway ();
			}
		}
	}

	public void currentWeaponSway ()
	{
		currentIKWeapon.currentWeaponSway (playerCameraManager.x, playerCameraManager.y, playerManager.v, playerManager.h, powersManager.running, shooting, playerManager.onGround);
	}

	public void drawOrKeepWeapon (bool state)
	{
		if (isThirdPersonView) {
			drawOrKeepWeaponThirdPerson (state);
			currentWeaponSystem.setWeaponCarryState (state, false);
		} else {
			drawOrKeepWeaponFirstPerson (state);
			currentWeaponSystem.setWeaponCarryState (false, state);
		}
		setLastTimeFired ();
	}

	public void aimCurrentWeapon (bool state)
	{
		if (isThirdPersonView) {
			aimOrDrawWeaponThirdPerson (state);
		} else {
			aimOrDrawWeaponFirstPerson (state);
		}
		setLastTimeFired ();
	}
	//third person
	public void drawOrKeepWeaponThirdPerson (bool state)
	{
		if (!canUseWeapons () && state) {
			return;
		}
		if (playerManager.crouch) {
			playerManager.crouching ();
		}
		carryingWeaponInThirdPerson = state;
		enableOrDisableWeaponsHUD (carryingWeaponInThirdPerson);
		getCurrentWeapon ();
		if (carryingWeaponInThirdPerson) {
			updateWeaponHUDInfo ();
			updateAmmo ();
			//enable the use of IK in hands
			currentIKWeapon.setIKWeight (1, 1);
			IKManager.weaponsState (carryingWeaponInThirdPerson, currentIKWeapon.thirdPersonWeaponInfo);
		} else {
			currentWeaponSystem.enableHUD (false);
			if (aimingInThirdPerson) {
				powersManager.deactivateAimMode ();
				aimingInThirdPerson = state;
			} 
			currentWeaponSystem.setWeaponAimState (false, false);
			IKManager.weaponsState (carryingWeaponInThirdPerson, currentIKWeapon.thirdPersonWeaponInfo);
			if (currentIKWeapon.carrying) {
				weaponReadyToMove ();
			}
		}
	}

	public void weaponReadyToMove ()
	{
		currentIKWeapon.drawOrKeepWeaponThirdPerson (carryingWeaponInThirdPerson);
	}

	public void aimOrDrawWeaponThirdPerson (bool state)
	{
		if (state != aimingInThirdPerson) {
			if (!canUseWeapons () && state) {
				return;
			}
			int c = 0;
			for (int i = 0; i < currentIKWeapon.thirdPersonWeaponInfo.handsInfo.Count; i++) {
				if (currentIKWeapon.thirdPersonWeaponInfo.handsInfo [i].handInPositionToDraw) {
					c++;
				}
			}
			if (c < 2) {
				return;
			}
			powersManager.aimsettings.aiming = !powersManager.aimsettings.aiming;
			aimingInThirdPerson = state;
			currentIKWeapon.aimOrDrawWeaponThirdPerson (aimingInThirdPerson);
			currentWeaponSystem.setWeaponAimState (aimingInThirdPerson, false);
			currentWeaponSystem.enableHUD (aimingInThirdPerson);
			if (aimingInThirdPerson) {
				powersManager.activateAimMode ();
			} else {
				powersManager.deactivateAimMode ();
			}
		}
	}
		
	//first person
	public void drawOrKeepWeaponFirstPerson (bool state)
	{
		if (!canUseWeapons () && state) {
			return;
		}
		if (playerManager.crouch) {
			playerManager.crouching ();
		}
		carryingWeaponInFirstPerson = state;
		enableOrDisableWeaponsHUD (carryingWeaponInFirstPerson);
		getCurrentWeapon ();
		if (carryingWeaponInFirstPerson) {
			updateWeaponHUDInfo ();
			updateAmmo ();
		} else {
			if (aimingInFirstPerson) {
				aimingInFirstPerson = state;
				currentWeaponSystem.setWeaponAimState (false, false);
				changeCameraFov (aimingInFirstPerson);
			}
			IKManager.setUsingWeaponsState (false);
		}
		currentIKWeapon.drawOrKeepWeaponFirstPerson (carryingWeaponInFirstPerson);
		powersManager.aimsettings.aiming = state;
		currentWeaponSystem.enableHUD (carryingWeaponInFirstPerson);
	}

	public void aimOrDrawWeaponFirstPerson (bool state)
	{
		if (!canUseWeapons () && state) {
			return;
		}
		if (currentIKWeapon.canAimInFirstPerson) {
			aimingInFirstPerson = state;
			powersManager.aimsettings.aiming = aimingInFirstPerson;
			currentIKWeapon.aimOrDrawWeaponFirstPerson (aimingInFirstPerson);
			if (currentWeaponSystem.weaponSettings.disableHUDInFirstPersonAim) {
				currentWeaponSystem.enableHUD (!aimingInFirstPerson);
			}
			changeCameraFov (aimingInFirstPerson);
			if (currentIKWeapon.useLowerRotationSpeedAimed) {
				if (aimingInFirstPerson) {
					playerCameraManager.changeRotationSpeedValue (currentIKWeapon.rotationSpeedAimedInFirstPerson);
				} else {
					playerCameraManager.setOriginalRotationSpeed ();
				}
			}

			currentWeaponSystem.setWeaponAimState (false, aimingInFirstPerson);
			playerManager.enableOrDisableAimingInFirstPerson (aimingInFirstPerson);
		}
	}

	public void changeCameraFov (bool increaseFov)
	{
		playerCameraManager.disableZoom ();
		if (increaseFov) {
			playerCameraManager.checkFovCoroutine (currentIKWeapon.aimFovValue, currentIKWeapon.aimFovSpeed);
		} else {
			playerCameraManager.checkFovCoroutine (originalFov, currentIKWeapon.aimFovSpeed);
		}
		if (weaponsCamera.fieldOfView != originalWeaponsCameraFov) {
			changeWeaponsCameraFov (false, originalWeaponsCameraFov, currentIKWeapon.aimFovSpeed);
		}
	}

	public void changeWeaponsCameraFov (bool increaseFov, float targetFov, float speed)
	{
		if (cameraFovCoroutine != null) {
			StopCoroutine (cameraFovCoroutine);
		}
		cameraFovCoroutine = StartCoroutine (changeWeaponsCameraFovCoroutine (increaseFov, targetFov, speed));
	}

	IEnumerator changeWeaponsCameraFovCoroutine (bool increaseFov, float targetFov, float speed)
	{
		float targetValue = originalWeaponsCameraFov;
		if (increaseFov) {
			targetValue = targetFov;
		}
		while (weaponsCamera.fieldOfView != targetValue) {
			weaponsCamera.fieldOfView = Mathf.MoveTowards (weaponsCamera.fieldOfView, targetValue, Time.deltaTime * speed);
			yield return null;
		}
	}

	public void setCurrentWeaponsParent (bool isFirstPerson)
	{
		string newLayer = "Default";

		bool quickDrawWeaponThirdPerson = false;

		if (isFirstPerson) {
			if (carryingWeaponInThirdPerson) {
				//print ("from third to first");
				carryingWeaponInThirdPerson = false;
				currentWeaponSystem.enableHUD (false);
				IKManager.weaponsState (carryingWeaponInThirdPerson, currentIKWeapon.thirdPersonWeaponInfo);
				weaponReadyToMove ();
				changingWeapon = true;
				currentWeaponSystem.setWeaponCarryState (false, true);
			}
		} else {
			if (carryingWeaponInFirstPerson) {
				//print ("from first to third");
				carryingWeaponInFirstPerson = false;
				enableOrDisableWeaponsHUD (false);
				currentWeaponSystem.enableHUD (false);
				IKManager.weaponsState (false, currentIKWeapon.firstPersonWeaponInfo);
				currentIKWeapon.quickKeepWeaponFirstPerson ();
				powersManager.aimsettings.aiming = false;
				currentWeaponSystem.setWeaponAimState (false, false);
				if (aimingInFirstPerson) {
					aimingInFirstPerson = false;
					changeCameraFov (false);
				}

				//changingWeapon = true;
				quickDrawWeaponThirdPerson = true;
				currentWeaponSystem.setWeaponCarryState (true, false);
			}
		}

		if (isFirstPerson) {
			weaponsParent.SetParent (firstPersonParent);
			weaponsParent.localRotation = weaponsTransformInFirstPerson.localRotation;
			weaponsParent.localPosition = weaponsTransformInFirstPerson.localPosition;
			newLayer = weaponsLayer;
		} else {
			weaponsParent.SetParent (thirdPersonParent);
			weaponsParent.localPosition = weaponsTransformInThirdPerson.localPosition;
			weaponsParent.localRotation = weaponsTransformInThirdPerson.localRotation;
		}
		for (int k = 0; k < weaponsList.Count; k++) {
			weaponsList [k].weapon.enableHUD (true);
			Component[] components = weaponsList [k].weapon.weaponSettings.weaponMesh.GetComponentsInChildren (typeof(Transform));
			foreach (Component c in components) {
				c.gameObject.layer = LayerMask.NameToLayer (newLayer);
			}
//			weaponsList [k].weapon.weaponSettings.shell.layer = LayerMask.NameToLayer (newLayer);
//			weaponsList [k].weapon.weaponSettings.shell.transform.GetChild (0).gameObject.layer = LayerMask.NameToLayer (newLayer);
			weaponsList [k].weapon.enableHUD (false);
			if (weaponsList [k].weaponEnabled) {
				weaponsList [k].enableOrDisableWeaponMesh (!isFirstPerson);
			}
			if (!isFirstPerson) {
				weaponsList [k].weapon.weaponSettings.weapon.transform.SetParent (weaponsList [k].thirdPersonWeaponInfo.keepPosition.parent);
				weaponsList [k].weapon.weaponSettings.weapon.transform.localPosition = weaponsList [k].thirdPersonWeaponInfo.keepPosition.localPosition;
				weaponsList [k].weapon.weaponSettings.weapon.transform.localRotation = weaponsList [k].thirdPersonWeaponInfo.keepPosition.localRotation;
				weaponsList [k].enableOrDisableFirstPersonArms (false);
				weaponsList [k].weapon.weaponSettings.weaponMesh.transform.localPosition = Vector3.zero;
				weaponsList [k].weapon.weaponSettings.weaponMesh.transform.localRotation = Quaternion.identity;
			}
			weaponsList [k].weapon.changeHUDPosition (!isFirstPerson);
		}
		if (quickDrawWeaponThirdPerson) {
			carryingWeaponInThirdPerson = true;
			enableOrDisableWeaponsHUD (true);
			updateWeaponHUDInfo ();
			updateAmmo ();
			currentIKWeapon.quickDrawWeaponThirdPerson ();
			IKManager.quickDrawWeaponState (currentIKWeapon.thirdPersonWeaponInfo);
		}
	}

	public void enableOrDisableWeaponsMesh(bool state){
		for (int k = 0; k < weaponsList.Count; k++) {
			weaponsList [k].weaponInfo.weapon.SetActive (state);
		}
	}

	public bool canUseWeapons ()
	{
		bool value = false;
		if (!playerManager.powerActive && ((playerManager.onGround && isThirdPersonView) || !isThirdPersonView) && IKManager.currentAimMode == IKSystem.aimMode.weapons) {
			value = true;
		}
		return value;
	}
	//select next or previous weapon
	void chooseNextWeapon ()
	{
		if (!moreThanOneWeaponAvaliable ()) {
			return;
		}
		//check the index and get the correctly weapon 
		int max = 0;
		currentIKWeapon.currentWeapon = false;
		int currentWeaponIndex = weaponsList [choosedWeapon].weapon.weaponSettings.numberKey;
		currentWeaponIndex++;
		if (currentWeaponIndex > weaponsSlotsAmount) {
			currentWeaponIndex = 1;
		}
		bool exit = false;
		while (!exit) {
			for (int k = 0; k < weaponsList.Count; k++) {
				if (weaponsList [k].weaponEnabled && weaponsList [k].weapon.weaponSettings.numberKey == currentWeaponIndex) {
					choosedWeapon = k;
					exit = true;
				}
			}
			max++;
			if (max > 100) {
				return;
			}
			//get the current weapon index
			currentWeaponIndex++;
			if (currentWeaponIndex > weaponsSlotsAmount) {
				currentWeaponIndex = 1;
			}
		}
		//set the current weapon 
		if (carryingWeaponInThirdPerson || carryingWeaponInFirstPerson) {
			changingWeapon = true;
		} else {
			weaponChanged ();
		}
	}

	void choosePreviousWeapon ()
	{
		if (!moreThanOneWeaponAvaliable ()) {
			return;
		}
		int max = 0;
		currentIKWeapon.currentWeapon = false;
		int currentWeaponIndex = weaponsList [choosedWeapon].weapon.weaponSettings.numberKey;
		currentWeaponIndex--;
		if (currentWeaponIndex < 1) {
			currentWeaponIndex = weaponsSlotsAmount;
		}
		bool exit = false;
		while (!exit) {
			for (int k = weaponsList.Count - 1; k >= 0; k--) {
				if (weaponsList [k].weaponEnabled && weaponsList [k].weapon.weaponSettings.numberKey == currentWeaponIndex) {
					choosedWeapon = k;
					exit = true;
				}
			}
			max++;
			if (max > 100) {
				return;
			}
			currentWeaponIndex--;
			if (currentWeaponIndex < 1) {
				currentWeaponIndex = weaponsSlotsAmount;
			}
		}
		if (carryingWeaponInThirdPerson || carryingWeaponInFirstPerson) {
			changingWeapon = true;
		} else {
			weaponChanged ();
		}
	}
	//set the info of the selected weapon in the hud
	void weaponChanged ()
	{
		weaponsList [choosedWeapon].currentWeapon = true;
		getCurrentWeapon ();
		updateWeaponHUDInfo ();
		updateAmmo ();
		powersManager.getCurrentWeapon (weaponsList [choosedWeapon]);
	}

	public void updateWeaponHUDInfo ()
	{
		currentWeaponNameText.text = currentWeaponSystem.weaponSettings.Name;
		currentWeaponAmmoText.text = currentWeaponSystem.weaponSettings.clipSize.ToString ();
		ammoSlider.maxValue = currentWeaponSystem.weaponSettings.ammoPerClip;
		ammoSlider.value = currentWeaponSystem.weaponSettings.clipSize;
	}

	public void updateAmmo ()
	{
		if (currentWeaponSystem) {
			if (!currentWeaponSystem.weaponSettings.infiniteAmmo) {
				currentWeaponAmmoText.text = currentWeaponSystem.weaponSettings.clipSize.ToString () + "/" + currentWeaponSystem.weaponSettings.remainAmmo;
			} else {
				currentWeaponAmmoText.text = currentWeaponSystem.weaponSettings.clipSize.ToString () + "/" + "Infinite";
			}
			ammoSlider.value = currentWeaponSystem.weaponSettings.clipSize;
		}
	}

	public void enableOrDisableWeaponsHUD (bool state)
	{
		weaponsHUD.SetActive (state);
	}
	//get the current weapon which is being used by the player right now
	public void getCurrentWeapon ()
	{
		for (int i = 0; i < weaponsList.Count; i++) {
			if (weaponsList [i].currentWeapon) {
				currentWeaponSystem = weaponsList [i].weapon;
				currentIKWeapon = weaponsList [i];
				currentWeaponName = currentWeaponSystem.weaponSettings.Name;
			} 
		}
	}
	//check if there is any weapon which can be used by the player
	public bool checkWeaponsAvaliable ()
	{
		for (int i = 0; i < weaponsList.Count; i++) {
			if (weaponsList [i].weaponEnabled) {
				weaponsList [i].currentWeapon = true;
				anyWeaponAvaliable = true;
				choosedWeapon = i;
				return true;
			}
		}
		return false;
	}
	//check if there is any more that one weapon which can be used, so the player doesn't change between the same weapon
	public bool moreThanOneWeaponAvaliable ()
	{
		int number = 0;
		for (int i = 0; i < weaponsList.Count; i++) {
			if (weaponsList [i].weaponEnabled) {
				number++;
			}
		}
		if (number > 1) {
			return true;
		} else {
			return false;
		}
	}
	//check if a weapon can be picked or is already avaliable to be used by the player
	public bool checkIfWeaponAvaliable (string weaponName)
	{
		for (int i = 0; i < weaponsList.Count; i++) {
			if (weaponsList [i].weapon.weaponSettings.Name == weaponName && weaponsList [i].weaponEnabled) {
				return true;
			}
		}
		return false;
	}

	public void AddAmmo (int amount, string weaponName)
	{
		for (int i = 0; i < weaponsList.Count; i++) {
			if (weaponsList [i].weapon.weaponSettings.Name == weaponName) {
				weaponsList [i].weapon.getAmmo (amount);
				updateAmmo ();
				return;
			}
		}
	}

	public void playerInAir ()
	{
		if (isThirdPersonView && GetComponent<IKSystem> ().currentAimMode == IKSystem.aimMode.weapons) {
			drawOrKeepWeapon (false);
		} 
	}

	//Coroutine getWeaponListCoroutineWaiting;
	public void getWeaponList (){
//	{
//		if (getWeaponListCoroutineWaiting != null) {
//			StopCoroutine (getWeaponListCoroutineWaiting);
//		}
//		getWeaponListCoroutineWaiting = StartCoroutine (getWeaponListCoroutine ());
//	}
//	IEnumerator getWeaponListCoroutine(){
//		yield return new WaitForSeconds (0.1f);
		GameObject model = GameObject.Find ("Player Controller");
		Animator anim = model.transform.GetChild(0).GetComponentInChildren<Animator> ();
		weaponsList.Clear ();
		Component[] components = GetComponentsInChildren (typeof(IKWeaponSystem));
		foreach (Component c in components) {
			IKWeaponSystem currentWeapon = c.GetComponent<IKWeaponSystem> ();
			currentWeapon.player = gameObject;
			bool weaponAssignedCorrectly = true;
			//print (anim.name);
			//currentWeapon.setHandTransform ();
			for (int i = 0; i < currentWeapon.weaponInfo.handsInfo.Count; i++) {
				if (currentWeapon.weaponInfo.handsInfo [i].limb == AvatarIKGoal.RightHand) {
					if (currentWeapon.weaponInfo.handsInfo [i].handTransform == null) {
						currentWeapon.weaponInfo.handsInfo [i].handTransform = anim.GetBoneTransform (HumanBodyBones.RightHand);
						weaponAssignedCorrectly = false;
						print ("Warning: Assign right hand into hand list in the IKWeaponSystem inspector of the weapon: " + currentWeapon.weapon.weaponSettings.Name);
						#if UNITY_EDITOR
						EditorUtility.SetDirty (currentWeapon);
						#endif
					} 
				}
				if (currentWeapon.weaponInfo.handsInfo [i].limb == AvatarIKGoal.LeftHand) {
					if (currentWeapon.weaponInfo.handsInfo [i].handTransform == null ) {
						weaponAssignedCorrectly = false;
						currentWeapon.weaponInfo.handsInfo [i].handTransform = anim.GetBoneTransform (HumanBodyBones.LeftHand);
						print ("Warning: Assign left hand into hand list in the IKWeaponSystem inspector of the weapon: " + currentWeapon.weapon.weaponSettings.Name);
						#if UNITY_EDITOR
						EditorUtility.SetDirty (currentWeapon);
						#endif
					}
				}
			}
			if (currentWeapon.weapon.weaponSettings.weaponParent == null) {
				weaponAssignedCorrectly = false;
				print ("Warning: Assign weapon parent into PlayerWeaponSystem inspector of the weapon: " + currentWeapon.weapon.weaponSettings.Name);
				print (anim.GetBoneTransform (HumanBodyBones.Spine) +" Assigned by default, change it in the player weapon system inspector");
				currentWeapon.weapon.weaponSettings.weaponParent = anim.GetBoneTransform (HumanBodyBones.Spine);
				#if UNITY_EDITOR
				EditorUtility.SetDirty (currentWeapon.weapon);
				#endif
			}
			currentWeapon.weapon.setCharacter (gameObject);
			if (weaponAssignedCorrectly) {
				weaponsList.Add (currentWeapon);
			} else {
				currentWeapon.weaponEnabled = false;
			}
		}
		#if UNITY_EDITOR
		EditorUtility.SetDirty (GetComponent<playerWeaponsManager> ());
		#endif
	}
	//pick a weapon unable to be used before
	public void pickWeapon (string weaponName)
	{
		for (int i = 0; i < weaponsList.Count; i++) {
			if (weaponsList [i].weapon.weaponSettings.Name == weaponName) {
				if (!weaponsList [i].weaponEnabled) {
					weaponsList [i].weaponEnabled = true;
					choosedWeapon = i;
					if (anyWeaponAvaliable) {
						currentIKWeapon.currentWeapon = false;
						//print ("deactive " + currentIKWeapon.weapon.weaponSettings.Name);
					} else {
						//print ("first weapon picked");
						checkWeaponsAvaliable ();
						getCurrentWeapon ();
					}
					if (isThirdPersonView) {
						weaponsList [i].enableOrDisableWeaponMesh (true);
					}
					powersManager.getCurrentWeapon (weaponsList [i]);	
					if (IKManager.currentAimMode == IKSystem.aimMode.weapons) {
						if (carryingWeaponInFirstPerson || carryingWeaponInThirdPerson) {
							//print ("Change weapon");
							changingWeapon = true;
						} else {
							if (anyWeaponAvaliable) {
								weaponsList [i].currentWeapon = true;
							}
							//print ("draw weapon");
							drawOrKeepWeapon (true);
						}
					} else {
						if (anyWeaponAvaliable) {
							weaponsList [i].currentWeapon = true;
						}
						getCurrentWeapon ();
					}
				}
			}
		}
	}
	//drop a weapon, so it is disabled and the player can't use it until that weapon is picked again
	public void dropWeapon ()
	{
		if (currentWeaponSystem && (!aimingInFirstPerson && !aimingInThirdPerson)) {
			GameObject weaponClone = (GameObject)Instantiate (currentIKWeapon.weaponPrefabModel, currentIKWeapon.weapon.gameObject.transform.position, currentIKWeapon.weapon.gameObject.transform.rotation);
			Vector3 forceDirection = transform.forward * 100;
			if (!isThirdPersonView) {
				forceDirection = playerCameraManager.mainCameraTransform.forward * 150;
			}
			weaponClone.GetComponent<Rigidbody> ().AddForce (forceDirection);
			Physics.IgnoreCollision (weaponClone.GetComponent<Collider> (), transform.GetComponent<Collider> ());
			if (moreThanOneWeaponAvaliable ()) {
				chooseNextWeapon ();
			} else {
				currentIKWeapon.currentWeapon = false;
			}
			if (isThirdPersonView) {
				currentIKWeapon.enableOrDisableWeaponMesh (false);
				carryingWeaponInThirdPerson = false;
				currentIKWeapon.quickKeepWeaponThirdPerson ();
				IKManager.quickKeepWeaponState ();
			} else {
				carryingWeaponInFirstPerson = false;
				currentIKWeapon.quickKeepWeaponFirstPerson ();
				currentIKWeapon.enableOrDisableFirstPersonArms (false);
			}
			currentIKWeapon.weaponEnabled = false;
			currentWeaponSystem.setWeaponCarryState (false, false);
			enableOrDisableWeaponsHUD (false);
			powersManager.aimsettings.aiming = false;
			currentWeaponSystem.enableHUD (false);
		}
	}

	public void disableCurrentWeapon ()
	{
		if (carryingWeaponInFirstPerson || carryingWeaponInThirdPerson) {
			if (aimingInThirdPerson) {
				//print ("deactivate");
				powersManager.deactivateAimMode ();
				IKManager.disableIKWeight ();
			}
			currentWeaponSystem.setWeaponAimState (false, false);
			currentWeaponSystem.setWeaponCarryState (false, false);
			aimingInFirstPerson = false;
			aimingInThirdPerson = false;

			if (carryingWeaponInThirdPerson) {
				carryingWeaponInThirdPerson = false;
				currentIKWeapon.quickKeepWeaponThirdPerson ();
				IKManager.quickKeepWeaponState ();
				IKManager.setUsingWeaponsState (false);

			} 
			if (carryingWeaponInFirstPerson) {
				carryingWeaponInFirstPerson = false;
				currentIKWeapon.quickKeepWeaponFirstPerson ();
				currentIKWeapon.enableOrDisableFirstPersonArms (false);
			}

			enableOrDisableWeaponsHUD (false);
			currentWeaponSystem.enableHUD (false);
			powersManager.aimsettings.aiming = false;
		}
	}

	public void getPlayerWeaponsManagerComponents(bool isFirstPerson){
		input = transform.parent.GetComponent<inputManager> ();
		IKManager = GetComponent<IKSystem> ();
		powersManager = GetComponent<otherPowers> ();
		playerManager = GetComponent<playerController> ();
		pauseManager = transform.parent.GetComponent<menuPause> ();
		playerCameraManager = cameraController.GetComponent<playerCamera> ();
		gravityManager = GetComponent<changeGravity> ();
		anyWeaponAvaliable = checkWeaponsAvaliable ();
		if (anyWeaponAvaliable) {
			getCurrentWeapon ();
			powersManager.getCurrentWeapon (currentIKWeapon);
		}
		originalFov = Camera.main.fieldOfView;
		touchPlatform = touchJoystick.checkTouchPlatform ();
		setHudZone ();
		originalWeaponsCameraFov = weaponsCamera.fieldOfView;
	}

	public void setWeaponsParent (bool isFirstPerson)
	{
		string newLayer = "Default";

		if (isFirstPerson) {
			weaponsParent.SetParent (firstPersonParent);
			weaponsParent.localRotation = weaponsTransformInFirstPerson.localRotation;
			weaponsParent.localPosition = weaponsTransformInFirstPerson.localPosition;
			newLayer = weaponsLayer;
		} else {
			weaponsParent.SetParent (thirdPersonParent);
			weaponsParent.localPosition = weaponsTransformInThirdPerson.localPosition;
			weaponsParent.localRotation = weaponsTransformInThirdPerson.localRotation;
			//print ("third person");
		}
		for (int k = 0; k < weaponsList.Count; k++) {
			weaponsList [k].weapon.enableHUD (true);
			Component[] components = weaponsList [k].weapon.weaponSettings.weaponMesh.GetComponentsInChildren (typeof(Transform));
			foreach (Component c in components) {
				c.gameObject.layer = LayerMask.NameToLayer (newLayer);
			}
			weaponsList [k].weapon.enableHUD (false);
			if (weaponsList [k].weaponEnabled) {
				weaponsList [k].enableOrDisableWeaponMesh (!isFirstPerson);
			}
			if (!isFirstPerson) {
				weaponsList [k].enableOrDisableFirstPersonArms (false);
				weaponsList [k].weapon.weaponSettings.weaponMesh.transform.localPosition = Vector3.zero;
				weaponsList [k].weapon.weaponSettings.weaponMesh.transform.localRotation = Quaternion.identity;
			}
			weaponsList [k].weapon.changeHUDPosition (!isFirstPerson);
		}
	}

	#if UNITY_EDITOR
	void OnDrawGizmos ()
	{
		DrawGizmos ();
	}

	void OnDrawGizmosSelected ()
	{
		DrawGizmos ();
	}

	void DrawGizmos ()
	{
		if (showGizmo) {
			//set the change weapon touch zone in the right upper corner of the scren, visile as gizmo
			if (!EditorApplication.isPlaying) {
				setHudZone ();
			}
			Gizmos.color = gizmoColor;
			Vector3 touchZone = new Vector3 (touchZoneRect.x + touchZoneRect.width / 2f, touchZoneRect.y + touchZoneRect.height / 2f, swipeCenterPosition.transform.position.z);
			Gizmos.DrawWireCube (touchZone, new Vector3 (touchZoneSize.x, touchZoneSize.y, 0f));
		}
	}
	#endif
	//get the correct size of the rect
	void setHudZone ()
	{
		if (!swipeCenterPosition) {
			swipeCenterPosition = GameObject.Find ("playerWeaponsSwipePosition");
		}
		touchZoneRect = new Rect (swipeCenterPosition.transform.position.x - touchZoneSize.x / 2f, swipeCenterPosition.transform.position.y - touchZoneSize.y / 2f, touchZoneSize.x, touchZoneSize.y);
	}
}