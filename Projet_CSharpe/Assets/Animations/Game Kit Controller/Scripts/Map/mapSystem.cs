using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class mapSystem : MonoBehaviour
{
	public bool mapEnabled;
	//[Header ("Map Components")]
	public GameObject mapContent;
	public GameObject mapCamera;
	public GameObject player;
	public GameObject mapMenu;
	public RectTransform mapWindowTargetPosition;
	public RectTransform mapRender;
	public RectTransform mapWindow;
	public RectTransform playerMapIcon;
	public Button removeMarkButton;
	public Text mapObjectNameField;
	public Text mapObjectInfoField;
	public Button quickTravelButton;
	public Text currentFloorNumber;
	public bool useMapIndexWindow;
	public GameObject mapIndexWindow;
	public GameObject mapIndexWindowContent;
	public Scrollbar mapIndexWindowScroller;
	//[Header ("Map Options")]
	public float playerIconMovementSpeed;
	public float openMapSpeed;
	public float dragMapSpeed;
	public bool rotateMap;
	public bool smoothRotationMap;
	public float rotationSpeed;
	public bool showOffScreenIcons;
	public float borderOffScreen;
	public float iconSize;
	public float offScreenIconSize;
	public float openMapIconSizeMultiplier;
	public float changeIconSizeSpeed;
	public float zoomWhenOpen;
	public float zoomWhenClose;
	public float openCloseZoomSpeed;
	public float zoomSpeed;
	[Range (0, 100)] public float maxZoom;
	[Range (0, 100)] public float minZoom;
	public Color disabledRemoveMarkColor;
	public Color disabledQuickTravelColor;
	public bool showIconsByFloor;
	//[Header ("Mark Options")]
	public bool showOffScreenIcon = true;
	public bool showMapWindowIcon = true;
	public bool showDistance = true;
	public bool markVisibleInAllFloors = true;
	public bool useDefaultObjectiveRadius;
	public float markRadiusDistance = 6;
	//[Header ("Compass Components")]
	public RectTransform compassWindow;
	public RectTransform north;
	public RectTransform south;
	public RectTransform east;
	public RectTransform west;
	//[Header ("Compass Options")]
	public bool compassEnabled;
	public bool showIntermediateDirections;
	//[Header ("Map Floors")]
	public List<Transform> floors = new List<Transform> ();
	//[Header ("Map Icons")]
	public List<mapIconType> mapIconTypes = new List<mapIconType> ();
	public int currentFloor;
	[HideInInspector] public bool mapOpened;
	List<mapObjectInfo> mapObjects = new List<mapObjectInfo> ();
	int i;
	int reverseCurrentCompassRotation;
	int currentCompassRotation;
	float cameraOffset;
	float currenIconSize;
	float currentIconSizeMultiplier = 1;
	Vector2 originalPlayerMapIconSize;
	Vector2 originalMapPosition;
	Vector2 targetMapPosition;
	Vector2 originalMapScale;
	Vector2 targetScale;
	Vector3 beginTouchPosition;
	RectTransform playerIconChild;
	mapObjectInfo currentMark;
	Color originalRemoveMarkColor;
	Color originalQuickTravelColor;
	inputManager input;
	menuPause pauseManager;
	Coroutine moveMapCoroutine;
	Coroutine cameraSizeCoroutine;
	Touch currentTouch;
	readonly List<RaycastResult> captureRaycastResults = new List<RaycastResult> ();
	Camera mainMapCamera;
	bool touchPlatform;
	bool zoomingIn;
	bool zoomingOut;
	bool movingMap;
	GameObject currentQuickTravelStation;
	float currentCameraSize;
	bool changingCameraSize;
	bool mapIndexEnabled;

	void Start ()
	{
		if (mapEnabled) {
			input = GetComponent<inputManager> ();
			pauseManager = GetComponent<menuPause> ();
			cameraOffset = mapCamera.transform.localPosition.y;
			for (i = 0; i < floors.Count; i++) {
				floors [i].gameObject.SetActive (false);
			}
			originalMapPosition = mapWindow.anchoredPosition;
			originalMapScale = mapWindow.sizeDelta;
			touchPlatform = touchJoystick.checkTouchPlatform ();
			mainMapCamera = mapCamera.transform.GetChild (0).GetComponent<Camera> ();
			playerIconChild = playerMapIcon.GetChild (0).GetComponent<RectTransform> ();
			originalPlayerMapIconSize = playerIconChild.sizeDelta;
			originalRemoveMarkColor = removeMarkButton.GetComponent<Image> ().color;
			removeMarkButton.GetComponent<Image> ().color = disabledRemoveMarkColor;
			originalQuickTravelColor = quickTravelButton.GetComponent<Image> ().color;
			quickTravelButton.GetComponent<Image> ().color = disabledQuickTravelColor;
			mapObjectNameField.text = "";
			mapObjectInfoField.text = "";
			if (!compassEnabled) {
				compassWindow.gameObject.SetActive (false);
			} else {
				if (!showIntermediateDirections) {
					north.transform.GetChild (0).gameObject.SetActive (false);
					south.transform.GetChild (0).gameObject.SetActive (false);
					east.transform.GetChild (0).gameObject.SetActive (false);
					west.transform.GetChild (0).gameObject.SetActive (false);
				}
			}
			mapMenu.SetActive (false);
			if (useMapIndexWindow) {
				mapMenu.SetActive (true);
				showOrHideMapIndexWindow (true);
				GameObject iconInfoElement = mapIndexWindowContent.transform.GetChild (0).gameObject;
				//every key field in the edit input button has an editButtonInput component, so create every of them
				for (i = 0; i < mapIconTypes.Count; i++) {
					GameObject iconInfoElementClone = (GameObject)Instantiate (iconInfoElement, iconInfoElement.transform.position, Quaternion.identity);
					iconInfoElementClone.transform.SetParent (iconInfoElement.transform.parent);
					iconInfoElementClone.transform.localScale = Vector3.one;
					iconInfoElementClone.name = mapIconTypes [i].typeName;
					iconInfoElementClone.GetComponentInChildren<Text> ().text = mapIconTypes [i].typeName;
					iconInfoElementClone.GetComponentInChildren<RawImage> ().texture = mapIconTypes [i].icon.GetComponent<RawImage> ().texture;
					iconInfoElementClone.GetComponentInChildren<RawImage> ().color = mapIconTypes [i].icon.GetComponent<RawImage> ().color;
				}
				iconInfoElement.SetActive (false);
				//set the scroller in the top position
				mapIndexWindowScroller.value = 1;
				showOrHideMapIndexWindow (false);
				mapMenu.SetActive (false);
			}
		} else {
			mapContent.SetActive (false);
		}
	}

	void Update ()
	{
		if (mapEnabled) {
			if (input.checkInputButton ("Map", inputManager.buttonType.getKeyDown)) {
				openOrCloseMap (!mapOpened);
			}
			if (!mapOpened) {
				Vector3 newPosition = player.transform.position;
				mapCamera.transform.position = Vector3.Lerp (mapCamera.transform.position, new Vector3 (newPosition.x, newPosition.y + cameraOffset, newPosition.z), Time.deltaTime * playerIconMovementSpeed);
			}
			Vector3 playerPosition = mainMapCamera.WorldToViewportPoint (player.transform.position);
			float playerIconPositionX = (playerPosition.x * mapWindow.sizeDelta.x) - (mapWindow.sizeDelta.x * 0.5f);
			float playerIconPositionY = (playerPosition.y * mapWindow.sizeDelta.y) - (mapWindow.sizeDelta.y * 0.5f);
			Vector2 playerIconPosition = new Vector2 (playerIconPositionX, playerIconPositionY);
			playerMapIcon.anchoredPosition = playerIconPosition;

			playerIconChild.sizeDelta = Vector2.Lerp (playerIconChild.sizeDelta, originalPlayerMapIconSize * currentIconSizeMultiplier, Time.deltaTime * changeIconSizeSpeed);

			if (rotateMap) {
				Vector3 mapCameraRotation = mapCamera.transform.eulerAngles;
				mapCameraRotation.y = player.transform.eulerAngles.y;
				if (smoothRotationMap) {
					playerMapIcon.rotation = Quaternion.identity;
					mapCamera.transform.rotation = Quaternion.Slerp (mapCamera.transform.rotation, Quaternion.Euler (mapCameraRotation), Time.deltaTime * rotationSpeed);
				} else {
					mapCamera.transform.eulerAngles = mapCameraRotation;
				}
			} else {
				mapCamera.transform.eulerAngles = Vector3.zero;
				Vector3 mapCameraRotation = Vector3.zero;
				mapCameraRotation.z = -player.transform.eulerAngles.y;
				playerMapIcon.eulerAngles = mapCameraRotation;
			}

			for (i = 0; i < mapObjects.Count; i++) { 
				if (mapObjects [i].floorNumber == currentFloor || !showIconsByFloor || mapObjects [i].floorNumber == -1) {
					if (!mapObjects [i].mapIcon.gameObject.activeSelf) {
						mapObjects [i].mapIcon.gameObject.SetActive (true);
					}
					Vector3 mapObjectPosition = mapObjects [i].mapObject.transform.position;
					Vector2 mapObjectViewPoint = mainMapCamera.WorldToViewportPoint (mapObjectPosition);
					Vector2 mapObjectPosition2d = new Vector2 ((mapObjectViewPoint.x * mapWindow.sizeDelta.x) - (mapWindow.sizeDelta.x * 0.5f),
						                              (mapObjectViewPoint.y * mapWindow.sizeDelta.y) - (mapWindow.sizeDelta.y * 0.5f));
					if (showOffScreenIcons) {
						mapObjectPosition2d.x = Mathf.Clamp (mapObjectPosition2d.x, -((mapWindow.sizeDelta.x * 0.5f) - borderOffScreen), ((mapWindow.sizeDelta.x * 0.5f) - borderOffScreen));
						mapObjectPosition2d.y = Mathf.Clamp (mapObjectPosition2d.y, -((mapWindow.sizeDelta.y * 0.5f) - borderOffScreen), ((mapWindow.sizeDelta.y * 0.5f) - borderOffScreen));
					}
					currenIconSize = iconSize;
					if (mapObjectPosition2d.x == (mapWindow.sizeDelta.x * 0.5f) - borderOffScreen ||
					     mapObjectPosition2d.y == (mapWindow.sizeDelta.y * 0.5f) - borderOffScreen ||
					     -mapObjectPosition2d.x == (mapWindow.sizeDelta.x * 0.5f) - borderOffScreen ||
					     -mapObjectPosition2d.y == (mapWindow.sizeDelta.y * 0.5f) - borderOffScreen) {
						currenIconSize = offScreenIconSize;
					} else {
						currenIconSize = iconSize;
					}
					currenIconSize *= currentIconSizeMultiplier;
					mapObjects [i].mapIcon.anchoredPosition = mapObjectPosition2d;
					mapObjects [i].mapIcon.sizeDelta = Vector2.Lerp (mapObjects [i].mapIcon.sizeDelta, new Vector2 (currenIconSize, currenIconSize), Time.deltaTime * changeIconSizeSpeed);
					Quaternion mapIconRotation = Quaternion.identity;
					mapIconRotation.x = mapObjects [i].mapObject.transform.rotation.x;
					mapObjects [i].mapIcon.localRotation = mapIconRotation;
				} else {
					if (mapObjects [i].mapIcon.gameObject.activeSelf) {
						mapObjects [i].mapIcon.gameObject.SetActive (false);
					}
				}
			}	
			if (!mapOpened) {
				float distance = Mathf.Infinity;
				for (i = 0; i < floors.Count; i++) {
					float currentDistance = Mathf.Abs (player.transform.position.y - floors [i].position.y);
					if (currentDistance < distance) {
						distance = currentDistance;
						currentFloor = i;
					}
				}
			}
			for (i = 0; i < floors.Count; i++) {
				if (i == currentFloor) {
					if (!floors [i].gameObject.activeSelf) {
						floors [i].gameObject.SetActive (true);
					}
				} else {
					if (floors [i].gameObject.activeSelf) {
						floors [i].gameObject.SetActive (false);
					}
				}
			}
			if (mapOpened) {
				//check for touch input from the mouse or the finger
				int touchCount = Input.touchCount;
				if (!touchPlatform) {
					touchCount++;
				}
				for (i = 0; i < touchCount; i++) {
					if (!touchPlatform) {
						currentTouch = touchJoystick.convertMouseIntoFinger ();
					} else {
						currentTouch = Input.GetTouch (i);
					}
					//in the touch begin phase
					if (!movingMap) {
						//get a list with all the objects under mouse or the finger tap
						if (currentTouch.phase == TouchPhase.Began) {
							captureRaycastResults.Clear ();
							PointerEventData p = new PointerEventData (EventSystem.current);
							p.position = currentTouch.position;
							p.clickCount = i;
							p.dragging = false;
							EventSystem.current.RaycastAll (p, captureRaycastResults);
							foreach (RaycastResult r in captureRaycastResults) {
								if (r.gameObject == mapRender.gameObject) {
									//check the current key pressed with the finger
									movingMap = true;
									beginTouchPosition = new Vector3 (currentTouch.position.x, currentTouch.position.y, 0);
								}
								for (i = 0; i < mapObjects.Count; i++) { 
									if (mapObjects [i].mapIcon.gameObject == r.gameObject) {
										if (mapObjects [i].typeName == "Mark") {
											currentMark = mapObjects [i];
											removeMarkButton.GetComponent<Image> ().color = originalRemoveMarkColor;
										}
										if (mapObjects [i].mapObject.GetComponent<mapObjectInformation> ()) {
											mapObjectNameField.text = mapObjects [i].mapObject.GetComponent<mapObjectInformation> ().name;
											mapObjectInfoField.text = mapObjects [i].mapObject.GetComponent<mapObjectInformation> ().description;
											if (mapObjects [i].mapObject.GetComponent<mapObjectInformation> ().typeName == "Beacon") {
												quickTravelButton.GetComponent<Image> ().color = originalQuickTravelColor;
												currentQuickTravelStation = mapObjects [i].mapObject;
											} else {
												quickTravelButton.GetComponent<Image> ().color = disabledQuickTravelColor;
											}
										} else {
											mapObjectInfoField.text = "";
											mapObjectNameField.text = "";
										}
									}
								}
							}
						}
					}
					//the current touch press is being moved
					if ((currentTouch.phase == TouchPhase.Moved || currentTouch.phase == TouchPhase.Stationary) && movingMap) {
						Vector3 globalTouchPosition = new Vector3 (currentTouch.position.x, currentTouch.position.y, 0);
						Vector3 differenceVector = globalTouchPosition - beginTouchPosition;
						if (differenceVector.sqrMagnitude > 1 * 1) {
							differenceVector.Normalize ();
						}
						beginTouchPosition = globalTouchPosition;
						Vector3 moveInput = differenceVector.y * mapCamera.transform.forward + differenceVector.x * mapCamera.transform.right;	
//						if (moveInput.magnitude > 1) {
//							moveInput.Normalize ();
//						}
						//Vector3 localMove = mapCamera.transform.InverseTransformDirection (moveInput);
						mapCamera.transform.position -= moveInput * dragMapSpeed;
					}
					//if the touch ends, reset the rotation of the joystick, the current axis values and the zoom buttons positions
					if (currentTouch.phase == TouchPhase.Ended) {
						movingMap = false;
					}
				}
				if (input.checkInputButton ("Next Power", inputManager.buttonType.posMouseWheel)) {
					zoomInEnabled ();
				}
				if (input.checkInputButton ("Previous Power", inputManager.buttonType.negMouseWheel)) {
					zoomOutEnabled ();
				}
				if (!changingCameraSize) {
					currentCameraSize = mainMapCamera.orthographicSize;
					if (zoomingIn) {
						currentCameraSize -= Time.deltaTime * zoomSpeed;
						if (currentCameraSize < minZoom) {
							currentCameraSize = minZoom;
						} 
					}
					if (zoomingOut) {
						currentCameraSize += Time.deltaTime * zoomSpeed;
						if (currentCameraSize > maxZoom) {
							currentCameraSize = maxZoom;
						} 
					}
					mainMapCamera.orthographicSize = Mathf.Lerp (mainMapCamera.orthographicSize, currentCameraSize, Time.deltaTime * zoomSpeed);
				}

				currentFloorNumber.text = (currentFloor + 1).ToString ();
			}
			if (compassEnabled) {
				currentCompassRotation = (int)Mathf.Abs (player.transform.eulerAngles.y);
				//never greater than the maximum degree of rotation
				if (currentCompassRotation > 360) {
					currentCompassRotation = currentCompassRotation % 360;//return to 0 
				}
				reverseCurrentCompassRotation = currentCompassRotation;
				//opposite angle
				if (reverseCurrentCompassRotation > 180) {
					reverseCurrentCompassRotation = reverseCurrentCompassRotation - 360;
				}
				north.anchoredPosition = new Vector2 (-reverseCurrentCompassRotation * 2, 0);
				south.anchoredPosition = new Vector2 (-currentCompassRotation * 2 + 360, 0);
				east.anchoredPosition = new Vector2 (-reverseCurrentCompassRotation * 2 + 180, 0);
				west.anchoredPosition = new Vector2 (-currentCompassRotation * 2 + 540, 0);
			}
		}
	}

	public void setCameraSize (float value)
	{
		if (cameraSizeCoroutine != null) {
			StopCoroutine (cameraSizeCoroutine);
		}
		cameraSizeCoroutine = StartCoroutine (setCameraSizeCoroutine (value));
	}

	IEnumerator setCameraSizeCoroutine (float value)
	{
		changingCameraSize = true;
		float t = 0.0f;
		while (t < 1) {
			t += Time.deltaTime;
			if (mainMapCamera.orthographicSize != value) {
				mainMapCamera.orthographicSize = Mathf.Lerp (mainMapCamera.orthographicSize, value, t);
			}
			yield return null;
		}
		changingCameraSize = false;
	}

	public void placeMark ()
	{
		GameObject newMark = new GameObject ();
		newMark.transform.position = new Vector3 (mapCamera.transform.position.x, floors [currentFloor].position.y, mapCamera.transform.position.z);
		newMark.name = "mark";
		addMapObject (newMark, "Mark", true);
		float radius = -1;
		if (!useDefaultObjectiveRadius) {
			radius = markRadiusDistance;
		}
		player.GetComponent<setObjective> ().addElementToList (newMark, false, radius, showOffScreenIcon, showMapWindowIcon, showDistance);
	}

	public void removeMark ()
	{
		if (currentMark != null) {
			player.GetComponent<setObjective> ().removeGameObjectFromList (currentMark.mapObject);
			Destroy (currentMark.mapObject);
			Destroy (currentMark.mapIcon.gameObject);
			mapObjects.Remove (currentMark);
			currentMark = null;
			removeMarkButton.GetComponent<Image> ().color = disabledRemoveMarkColor;
		}
	}

	public void checkNextFloor ()
	{
		if (currentFloor + 1 <= floors.Count - 1) {
			currentFloor++;
		}
	}

	public void checkPrevoiusFloor ()
	{
		if ((currentFloor - 1) >= 0) {
			currentFloor--;
		}
	}

	public void openOrCloseMap (bool state)
	{
		if (mapEnabled && (!pauseManager.playerMenuActive || mapOpened) && !pauseManager.usingDevice && !pauseManager.pauseGame) {
			mapOpened = state;
			pauseManager.openOrClosePlayerMenu (mapOpened);
			if (mapOpened) {
				currentIconSizeMultiplier = openMapIconSizeMultiplier;
				targetMapPosition = mapWindowTargetPosition.anchoredPosition;
				targetScale = mapWindowTargetPosition.sizeDelta;
				mapObjectInfoField.text = "";
				mapObjectNameField.text = "";
			} else {
				currentIconSizeMultiplier = 1;
				targetMapPosition = originalMapPosition;
				targetScale = originalMapScale;
			}
			pauseManager.showOrHideCursor (mapOpened);
			//disable the touch controls
			pauseManager.checkTouchControls (!mapOpened);
			//disable the camera rotation
			pauseManager.changeCameraState (!mapOpened);
			player.GetComponent<playerController> ().changeScriptState (!mapOpened);
			pauseManager.usingSubMenuState (mapOpened);
			mapMenu.SetActive (mapOpened);
			checkChangeMapPositionCoroutine ();
			if (mapOpened) {
				setCameraSize (zoomWhenOpen);
			} else {
				setCameraSize (zoomWhenClose);
				if (mapIndexEnabled) {
					showOrHideMapIndexWindow (false);
				}
			}
		}
	}

	public void openOrCLoseMapFromTouch ()
	{
		openOrCloseMap (!mapOpened);
	}

	public void checkChangeMapPositionCoroutine ()
	{
		if (moveMapCoroutine != null) {
			StopCoroutine (moveMapCoroutine);
		}
		moveMapCoroutine = StartCoroutine (changeMapPositionCoroutine ());
	}

	IEnumerator changeMapPositionCoroutine ()
	{
		for (float t = 0; t < 1;) {
			t += Time.deltaTime;
			mapWindow.anchoredPosition = Vector2.Lerp (mapWindow.anchoredPosition, targetMapPosition, t);
			mapWindow.sizeDelta = Vector2.Lerp (mapWindow.sizeDelta, targetScale, t);
			mapRender.sizeDelta = Vector2.Lerp (mapRender.sizeDelta, targetScale, t);
			yield return null;
		}
	}
	//add a new object which will be visible in the radar. It can be an enemy, and ally or a target
	public void addMapObject (GameObject obj, string type, bool addingMark)
	{
		mapObjectInfo newMapObject = new mapObjectInfo ();
		bool alreadyAdded = false;
		for (i = 0; i < mapObjects.Count; i++) {
			if (mapObjects [i].mapObject == obj) {
				alreadyAdded = true;
				newMapObject = mapObjects [i];
				Destroy (newMapObject.mapIcon.gameObject);
			}
		}
		for (i = 0; i < mapIconTypes.Count; i++) {
			if (mapIconTypes [i].typeName == type) {
				GameObject icon = (GameObject)Instantiate (mapIconTypes [i].icon.gameObject, mapWindow.transform.position, Quaternion.identity);
				icon.transform.SetParent (mapWindow.transform);
				icon.transform.localScale = Vector3.one;
				newMapObject.typeName = type;
				newMapObject.mapIcon = icon.GetComponent<RectTransform> ();
			}
		}
		if (!alreadyAdded) {
			newMapObject.mapObject = obj;
			bool calculateFloor = false;
			if (obj.GetComponent<mapObjectInformation> ()) {
				if (obj.GetComponent<mapObjectInformation> ().floorIndex > 0) {
					newMapObject.floorNumber = obj.GetComponent<mapObjectInformation> ().floorIndex - 1;
				} else if (obj.GetComponent<mapObjectInformation> ().floorIndex < 0) {
					newMapObject.floorNumber = obj.GetComponent<mapObjectInformation> ().floorIndex;
				} else {
					calculateFloor = true;
				}
			} else {
				calculateFloor = true;
			}
			if (calculateFloor) {
				if (addingMark) {
					if (markVisibleInAllFloors) {
						newMapObject.floorNumber = -1;
					} else {
						newMapObject.floorNumber = currentFloor;
					}
				} else {
					float distance = Mathf.Infinity;
					for (i = 0; i < floors.Count; i++) {
						float currentDistance = Mathf.Abs (newMapObject.mapObject.transform.position.y - floors [i].position.y);
						if (currentDistance < distance) {
							distance = currentDistance;
							newMapObject.floorNumber = i;
						}
					}
				}
			}
			mapObjects.Add (newMapObject);
		}
	}
	//remove a dead enemy, ally or a reached target
	public void removeMapObject (GameObject obj, bool isPathElement)
	{
		for (i = 0; i < mapObjects.Count; i++) {
			if (mapObjects [i].mapObject == obj) {
				if (!isPathElement) {
					if (obj.GetComponent<mapObjectInformation> ()) {
						Destroy (obj.GetComponent<mapObjectInformation> ());
					}
				}
				Destroy (mapObjects [i].mapIcon.gameObject);
				mapObjects.RemoveAt (i);
			}
		}
	}

	public void zoomInEnabled ()
	{
		zoomingIn = true;
	}

	public void zoomInDisabled ()
	{
		zoomingIn = false;
	}

	public void zoomOutEnabled ()
	{
		zoomingOut = true;
	}

	public void zoomOutDisabled ()
	{
		zoomingOut = false;
	}

	public void disableButtons ()
	{
		removeMarkButton.GetComponent<Image> ().color = disabledRemoveMarkColor;
		quickTravelButton.GetComponent<Image> ().color = disabledQuickTravelColor;
	}

	public void activateQuickTravel ()
	{
		if (currentQuickTravelStation) {
			currentQuickTravelStation.GetComponent<quickTravelStationSystem> ().travelToThisStation ();
			currentQuickTravelStation = null;
			quickTravelButton.GetComponent<Image> ().color = disabledQuickTravelColor;
			openOrCloseMap (false);
		}
	}

	public int getIconTypeIndexByName (string iconTypeName)
	{
		for (i = 0; i < mapIconTypes.Count; i++) {
			if (mapIconTypes [i].typeName == iconTypeName) {
				return i;
			}
		}
		return -1;
	}

	public void changeMapIndexWindowState ()
	{
		showOrHideMapIndexWindow (!mapIndexWindow.activeSelf);
	}

	public void showOrHideMapIndexWindow (bool state)
	{
		mapIndexWindow.SetActive (state);
		mapIndexEnabled = state;
	}

	[System.Serializable]
	public class mapObjectInfo
	{
		public string typeName;
		public GameObject mapObject;
		public RectTransform mapIcon;
		public int floorNumber;
	}

	[System.Serializable]
	public class mapIconType
	{
		public string typeName;
		public RectTransform icon;
		public bool showIconPreview;
	}
}