using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Reflection;
public class powersListManager : MonoBehaviour {
	public bool powerListManagerEnabled;
	public GameObject powersSlotsMenu;
	public GameObject powersListContent;
	public GameObject powersListElement;
	public GameObject powersSlotsWheel;
	public GameObject completePowersWheel;
	public GameObject completePowersList;
	public Text currentPowerNameText;
	public GameObject slotArrow;
	public Vector2 range = new Vector2(5f, 3f);
	public float rotationHUDSpeed= 20;
	public float touchZoneRadius = 2;
	public float doubleTapTime = 0.5f;
	public float holdTapTime = 0.7f;
	[HideInInspector] public bool editingPowers;
	[HideInInspector] public bool selectingPower;
	powersListElement closestSlot;
	List<GameObject> powersListElements =new List<GameObject> ();
	List<powersListElement> powersWheelElements =new List<powersListElement> ();
	readonly List<RaycastResult> captureRaycastResults = new List<RaycastResult>();
	inputManager input; 
	GameObject player;
	GameObject buttonToMove;
	GameObject slotSelected;
	GameObject slotFound;
	GameObject centerScreen;
	otherPowers.Powers previousPower;
	float showSlotsTimer;
	float lastButtonTime;
	float screenWidth;
	float screenHeight;
	bool touchPlatform;
	bool touching;
	bool touchingScreenCenter;
	Vector2 mRot = Vector2.zero;
	Quaternion mStart;
	int i;
	int tapCount=0;
	int numberOfCurrentPowers;
	menuPause pauseManager;
	Touch currentTouch;
	Scrollbar scroller;
	otherPowers powersManager;
	Rect touchZoneRect;

	void Start () {
		//get the current screen size
		screenWidth=Screen.width;
		screenHeight=Screen.height;
		player=GameObject.Find("Player Controller");
		powersManager = player.GetComponent<otherPowers> ();
		//enable the power slots menu to get and set all the neccessary components
		completePowersList.SetActive(true);
		completePowersWheel.SetActive(true);
		//get the max amount of powers that the player can used currently
		numberOfCurrentPowers = powersManager.shootsettings.powersSlotsAmount;
		//get the final element in the list so all the created list elements will be visible
		GameObject bottom = powersListContent.transform.GetChild (0).gameObject;
		//for every power created in the otherPowers inspector, add to the list of the power manager
		for(i=0;i<powersManager.shootsettings.powersList.Count;i++){
			//create the list of power in the right side of the HUD
			GameObject powersListElementClone=(GameObject) Instantiate (powersListElement,Vector3.zero,Quaternion.identity);
			powersListElementClone.transform.SetParent(powersListContent.transform);
			powersListElementClone.transform.localScale=Vector3.one;
			//get the powerListElement from the instantiated object
			Component powerElement=powersListElementClone.GetComponentInChildren(typeof(powersListElement));
			//set its data according to the list of powers created in otherPowers
			powerElement.GetComponent<powersListElement>().setData(powersManager.shootsettings.powersList[i]);
			//add this element to the list
			powersListElements.Add(powersListElementClone);
			//if the number of powers enabled is higher thah the powers created, disable the other powers of the player
			if((i+1)>numberOfCurrentPowers){
				powersManager.shootsettings.powersList[i].enabled=false;
			}
		}
		//get the powers wheel elements
		Component[] components=powersSlotsWheel.GetComponentsInChildren(typeof(powersListElement));
		foreach (Component c in components) {
			powersWheelElements.Add (c.gameObject.GetComponent<powersListElement>());
		}
		//in every slot in the powers wheel, set the icon and info
		for (i=0; i<powersWheelElements.Count; i++) {
			if(i<powersManager.shootsettings.powersList.Count){
				powersWheelElements[i].setData(powersManager.shootsettings.powersList[i]);
			}
		}
		//get the scroller in the powers manager
		scroller = completePowersList.GetComponentInChildren<Scrollbar> ();
		scroller.value = 1;
		//disable the powers manager
		bottom.transform.SetParent(null);
		bottom.transform.SetParent(powersListContent.transform);
		completePowersList.SetActive(false);
		completePowersWheel.SetActive(false);
		//set the zone to touch to select power in the center of the touch screen
		setHudZone ();
		//get the rotation of the powers wheel
		mStart = completePowersWheel.transform.localRotation;
		//get the input and pause manager
		input = GetComponent<inputManager> ();
		pauseManager = GetComponent<menuPause> ();
		//check if the platform is a touch device or not
		touchPlatform=touchJoystick.checkTouchPlatform ();
	}

	void Update () {
		//if the edit power button is pressed, enable the power manager
		if (input.checkInputButton ("Show Powers Slots", inputManager.buttonType.getKeyDown)) {
			editPowersSlots();
		}
		//if the select power button is holding, enable the powers wheel to select power
		if (input.checkInputButton ("Select Power Slot", inputManager.buttonType.getKeyDown)) {
			selectPowersSlots();
		}
		//disable the powers wheel
		if (input.checkInputButton ("Select Power Slot", inputManager.buttonType.getKeyUp)) {
			selectPowersSlots();
		}
		//if the player is selecting, editing the powers, or the touch controls are enabled, then
		if (editingPowers || selectingPower || pauseManager.useTouchControls) {
			//check the mouse position in the screen if we are in the editor, or the finger position in a touch device
			int touchCount = Input.touchCount;
			if (!touchPlatform) {
				touchCount++;
			}
			for (int i = 0; i < touchCount; i++){
				if (!touchPlatform) {
					currentTouch = touchJoystick.convertMouseIntoFinger();
				}
				else{
					currentTouch = Input.GetTouch(i);
				}
				if (currentTouch.phase == TouchPhase.Began) {
					touching=true;
					if (touchZoneRect.Contains (currentTouch.position)) {
						touchingScreenCenter = true;
					}
					//if the finger tap is inside the rect zone in the upper left corner of the screen
					if(powersManager.touchZoneRect.Contains (currentTouch.position)){
						//check the time between taps, if the number is 2 and they are done quickly
						if (Time.time - lastButtonTime < doubleTapTime){
							//open the edit powers manager
							lastButtonTime=0;
							editPowersSlots();
						}
						lastButtonTime = Time.time;	
						tapCount++;
						//reset the tap count
						if(tapCount==2){
							tapCount=0;
							lastButtonTime=0;
						}
					}
					//if the edit powers manager is open, then
					if(editingPowers){
						//check where the mouse or the finger press, to get a power list element, to edit the powers
						captureRaycastResults.Clear();
						PointerEventData p = new PointerEventData(EventSystem.current);
						p.position = currentTouch.position;
						p.clickCount = i;
						p.dragging = false;
						EventSystem.current.RaycastAll(p, captureRaycastResults);
						foreach (RaycastResult r in captureRaycastResults) {
							//if the object pressed is a powerListElement, and it is enabled
							if(r.gameObject.GetComponent<powersListElement>()){
								powersListElement element = r.gameObject.GetComponent<powersListElement> ();
								if(element.enabled){
									//the power element pressed is in the wheel
									if(element.listType==global::powersListElement.powerListType.slot){
										//if the texture is enabled, grab the element, to remove from the wheel or to change its position
										if(r.gameObject.transform.GetChild(0).GetComponent<RawImage>().texture){
											buttonToMove=(GameObject)Instantiate(r.gameObject,r.gameObject.transform.position,Quaternion.identity);
											buttonToMove.GetComponent<powersListElement>().enabled=false;
											buttonToMove.transform.SetParent (powersSlotsMenu.transform.parent);
											r.gameObject.transform.GetChild(0).GetComponent<RawImage>().texture=null;
											slotSelected=r.gameObject;
										}
									}
									//the power element pressed is in the list in the right
									else{
										//grab the list element to drop it in the wheel
										buttonToMove=(GameObject)Instantiate(r.gameObject,r.gameObject.transform.position,Quaternion.identity);
										buttonToMove.transform.SetParent(powersSlotsMenu.transform.parent);
										buttonToMove.GetComponent<powersListElement>().enabled=false;
									}
								}
							}
						}
					}
				}
				//if the power list element is grabbed, follow the mouse/finger position in screen
				if ((currentTouch.phase == TouchPhase.Stationary || currentTouch.phase == TouchPhase.Moved)) {
					if(editingPowers){
						if (buttonToMove != null) {
							buttonToMove.GetComponent<RectTransform> ().position = new Vector2 (currentTouch.position.x, currentTouch.position.y);
						}
					}
				}
				//if the mouse/finger press is released, then
				if (currentTouch.phase == TouchPhase.Ended) {
					touching=false;
					touchingScreenCenter = false;
					showSlotsTimer=0;
					//if the player was editing the powers
					if(editingPowers){
						if (buttonToMove != null) {
							//get the elements in the position where the player released the power element
							captureRaycastResults.Clear();
							PointerEventData p = new PointerEventData(EventSystem.current);
							p.position = currentTouch.position;
							p.clickCount = i;
							p.dragging = false;
							EventSystem.current.RaycastAll(p, captureRaycastResults);
							foreach (RaycastResult r in captureRaycastResults) {
								if(r.gameObject!=buttonToMove){
									//if the power element was released above other power element from the wheel, store the power element from the wheel
									if(r.gameObject.GetComponent<powersListElement>() && r.gameObject.GetComponent<powersListElement>().listType==global::powersListElement.powerListType.slot){
										slotFound=r.gameObject;
									}
								}
							}
							//if the power element was released above other power element from the wheel, then
							if(slotFound){
								//check that the power dragged is not already in the wheel and that the power is released above the wheel, and not the list in the right
								if(slotFound.GetComponent<powersListElement>().listType==global::powersListElement.powerListType.slot &&
									 (!checkDuplicatedSlot(buttonToMove.GetComponent<powersListElement>().powerData.Name) || 
									 buttonToMove.GetComponent<powersListElement>().listType==global::powersListElement.powerListType.slot)){
									bool empty=true;
									//if the stored power element has a texture, then a power is going to be replaced, so store it to remove it after
									if(slotFound.GetComponent<powersListElement>().powerData.texture){
										empty=false;
										previousPower=slotFound.GetComponent<powersListElement>().powerData;
									}
									//set the data of the dragged power in the wheel
									slotFound.GetComponent<powersListElement>().setData(buttonToMove.GetComponent<powersListElement>().powerData);
									slotFound.GetComponent<powersListElement>().setKey();
									//if the element dragged and dropped was a power inside the wheel, then 
									if(slotSelected){
										if(slotSelected!=slotFound){
											//set the new data in that power
											slotSelected.GetComponent<powersListElement>().powerData=new otherPowers.Powers();
											//set the change in otherPowers
											powersManager.changePowerState(buttonToMove.GetComponent<powersListElement>().powerData,slotFound.GetComponent<powersListElement>().defaultKeyNumber,true,0);
											if(empty){
												//the dropped power is released in a empty element of the wheel
												//print ("changed to empty");
											}
											else{
												//the dropped power is released in anoter power element, so change the previous power for the new power
												powersManager.changePowerState(previousPower,slotFound.GetComponent<powersListElement>().defaultKeyNumber,false,-1);
												//print ("changed to occupied");
											}
										}
										else{
											//the dropped power is released in same position where it was previously
											//print("change to the same");
										}
									}
									//else, the element dragged and dropped was a power of the list in the right of the screen
									else{
										if(empty){
											//the dropped power is released in a empty element of the wheel
											//print("set in empty");

										}
										else{
											//the dropped power is released in anoter power element, so change the previous power for the new power
											//print ("set in occupied");
											powersManager.changePowerState(previousPower,slotFound.GetComponent<powersListElement>().defaultKeyNumber,false,-1);

										}
										//set the change in other powers
										powersManager.changePowerState(buttonToMove.GetComponent<powersListElement>().powerData,slotFound.GetComponent<powersListElement>().defaultKeyNumber,true,1);
									}
									buttonToMove.GetComponent<powersListElement>().enabled=true;
									//remove the dragged object
									Destroy(buttonToMove);
								}
								else{
									Destroy(buttonToMove);
									//print ("power already added");
								}
							}
							//the dragged power is released in any other position
							else{
								//check if the power was grabbed from the wheel, in that case the power has been removed, so change the info in otherpowers
								if(buttonToMove.GetComponent<powersListElement>().listType==global::powersListElement.powerListType.slot){
									powersManager.changePowerState(slotSelected.GetComponent<powersListElement>().powerData,slotSelected.GetComponent<powersListElement>().defaultKeyNumber,false,-1);
									//set the info in the powers wheel
									slotSelected.GetComponent<powersListElement>().powerData=new otherPowers.Powers();
									//print ("power removed");
								}
								//remove the dragged object
								Destroy(buttonToMove);
							}
							slotFound=null;
							slotSelected=null;
						}
					}
					//if the player is selecting a power, enable only the powers wheel 
					if(pauseManager.useTouchControls && selectingPower){
						selectPowersSlots();
					}
				}
				if(selectingPower){
					//make the slots wheel looks toward the mouse
					float halfWidth = screenWidth * 0.5f;
					float halfHeight = screenHeight * 0.5f;
					float x = Mathf.Clamp((currentTouch.position.x - halfWidth) / halfWidth, -1f, 1f);
					float y = Mathf.Clamp((currentTouch.position.y - halfHeight) / halfHeight, -1f, 1f);
					mRot = Vector2.Lerp(mRot, new Vector2(x, y), Time.deltaTime * rotationHUDSpeed);
					completePowersWheel.transform.localRotation = mStart * Quaternion.Euler(-mRot.y * range.y, mRot.x * range.x, 0f);
					//get the power inside the wheel closest to the mouse
					float distance=Mathf.Infinity;
					for (int k=0; k<powersWheelElements.Count; k++) {
						if(Vector3.Distance(powersWheelElements[k].transform.position,new Vector3(currentTouch.position.x,currentTouch.position.y,0))<distance){
							distance=Vector3.Distance(powersWheelElements[k].transform.position,new Vector3(currentTouch.position.x,currentTouch.position.y,0));
							closestSlot=powersWheelElements[k];
						}
					}
					//set the name of the closes power in the center of the powers wheel
					if(closestSlot){
						if(closestSlot.powerData.Name!=powersManager.shootsettings.powersList[powersManager.choosedPower].Name){
							powersManager.setPower(closestSlot.powerData);
						}
						if(closestSlot.powerData.Name!=""){
							currentPowerNameText.text=closestSlot.powerData.Name;
						}
					}
				}
				//get the arrow rotates toward the mouse, selecting the closest power to it
				Vector2 slotDirection=new Vector2(currentTouch.position.x,currentTouch.position.y) -  slotArrow.GetComponent<RectTransform>().anchoredPosition ;
				Vector2 screenCenter = new Vector2 (screenWidth, screenHeight) / 2;
				slotDirection -= screenCenter;
				float angle = Mathf.Atan2 (slotDirection.y, slotDirection.x);
				angle -= 90 * Mathf.Deg2Rad;
				slotArrow.transform.localRotation = Quaternion.Euler (0, 0, angle * Mathf.Rad2Deg);

				//in a touch device, if the finger is touching the screen
				//check if the player keeps its finger in the center of the screen for a second, to enable the powers wheel, and without releasing the pressing,
				//move the finger towards a power, when the tap is released, the closest power to the finger position is set as the current power
				if (touching && touchingScreenCenter) {
					//and the tap is being holding inside the touch rect zone in the center of the screen, then
					if (touchZoneRect.Contains (currentTouch.position)) {
						//if the player is not selecting a power, get the time of the holding
						if (!selectingPower) {
							showSlotsTimer += Time.deltaTime;
							//when the timer reachs its target value, enable the powers wheel 
							if (showSlotsTimer > holdTapTime) {
								showSlotsTimer = 0;
								selectPowersSlots ();
							}
						}
					}
				}
			}
			if (tapCount > 0 && Time.time > lastButtonTime + doubleTapTime) {
				tapCount = 0;
			}
		}
	}
	//enable the powers wheel and the list in the right to edit the current powers to select
	public void editPowersSlots(){
		//check that the game is not paused, that the player is not selecting a power, using a device and that the power manager can be enabled
		if ((canBeOpened() || editingPowers) && !selectingPower) {
			editingPowers = !editingPowers;
			pauseManager.openOrClosePlayerMenu (editingPowers);
			bool value = editingPowers;
			slotArrow.SetActive(!value);
			currentPowerNameText.text="";
			//enable the powers wheel and the list
			completePowersList.SetActive(value);
			completePowersWheel.SetActive (value);
			//set to visible the cursor
			pauseManager.showOrHideCursor (value);
			//disable the touch controls
			pauseManager.checkTouchControls(!editingPowers);
			//disable the camera rotation
			pauseManager.changeCameraState(!value);
			//reset the wheel rotation
			completePowersWheel.transform.localRotation=Quaternion.identity;
		}
	}
	//enable the powers wheel to select the current powers to use
	public void selectPowersSlots(){
		//check that the game is not paused, that the player is not editing the powers, using a device and that the power manager can be enabled
		if ((canBeOpened() || selectingPower) && !editingPowers) {
			selectingPower = !selectingPower;
			pauseManager.openOrClosePlayerMenu (selectingPower);
			bool value = selectingPower;
			//enable the powers wheel
			completePowersWheel.SetActive(value);
			//set to visible the cursor
			pauseManager.showOrHideCursor (value);
			closestSlot=null;
			//disable the touch controls
			pauseManager.checkTouchControls(!selectingPower);
			//disable the camera rotation
			pauseManager.changeCameraState(!value);
			//reset the arrow and the wheel rotation
			completePowersWheel.transform.localRotation=Quaternion.identity;
			slotArrow.transform.localRotation=Quaternion.identity;
		}
	}

	public bool canBeOpened(){
		bool result = false;
		if (!pauseManager.pauseGame && !pauseManager.usingDevice && powerListManagerEnabled && player.GetComponent<IKSystem> ().currentAimMode != IKSystem.aimMode.weapons
			&& !pauseManager.playerMenuActive) {
			result = true;
		}
		return result;
	}
	//check that the dropped power is not already in the wheel, using the power name
	public bool checkDuplicatedSlot(string powerName){
		for (i=0; i<powersWheelElements.Count; i++) {
			if(powersWheelElements[i].powerData.Name==powerName){
				return true;
			}
		}
		return false;
	}
	//create a rect zone in the center of the screen to check if the player hold his finger inside it, to enable the power wheel to select powers
	#if UNITY_EDITOR
	//draw a rect gizmo in the center of the screen
	void OnDrawGizmosSelected(){
		if (!EditorApplication.isPlaying) {
			setHudZone ();
		}
		Gizmos.color = Color.yellow;
		Vector3 touchZone = new Vector3(touchZoneRect.x + touchZoneRect.width / 2f,touchZoneRect.y + touchZoneRect.height / 2f,centerScreen.transform.position.z);
		Gizmos.DrawWireCube(touchZone,new Vector3(touchZoneRadius, touchZoneRadius, 0f));
	}
	#endif
	//use the screen size to set the size of the rect.
	void setHudZone(){
		//use the position of the object centerScreen as the center of the screen
		if (!centerScreen) {
			centerScreen=GameObject.Find("centerScreen");
		}
		touchZoneRect = new Rect (centerScreen.transform.position.x - touchZoneRadius / 2f, centerScreen.transform.position.y - touchZoneRadius / 2f, touchZoneRadius, touchZoneRadius);
	}
}