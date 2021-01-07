using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class inventoryManager : MonoBehaviour
{
	public bool inventoryEnabled;
	public List<inventoryInfo> inventoryList = new List<inventoryInfo> ();
	public GameObject inventoryPanel;
	public GameObject inventoryListContent;
	public GameObject objectIcon;
	public GameObject useButton;
	public GameObject equipButton;
	public GameObject dropButton;
	public Text currentObjectName;
	public Text currentObjectInfo;
	public RawImage objectImage;
	public Color buttonUsable;
	public Color buttonNotUsable;
	public int inventorySpace;
	public int maxObjectsAmountPerSpace;
	public Camera inventoryCamera;
	public Transform lookObjectsPosition;
	public float rotationSpeed;
	public bool inventoryOpened;
	public GameObject emptyInventoryPrefab;
	public GameObject usedObjectMessage;
	public float usedObjectMessageTime;
	public string unableToUseObjectMessage;
	public GameObject fullInventoryMessage;
	public float fullinventoryMessageTime;
	public bool combineElementsAtDrop;
	public float zoomSpeed;
	public float maxZoomValue;
	public float minZoomValue;
	public bool showElementSettings;
	public bool useRelativePath;
	public string relativePath;
	public inventoryInfo currentObject;
	menuPause pauseManager;
	inputManager input;
	inventoryInfo duplicateObject;
	GameObject objectInCamera;
	int i;
	int objectsAmount;
	int bucle = 0;
	bool enableRotation;
	bool zoomingIn;
	bool zoomingOut;
	float originalFov;
	GameObject currentObjectThatNeedsInventory;
	Coroutine resetCameraFov;
	Coroutine inventoryFullCoroutine;
	List<GameObject> emptyInventoryList = new List<GameObject> ();

	void Start ()
	{
		input = transform.parent.GetComponent<inputManager> ();
		pauseManager = transform.parent.GetComponent<menuPause> ();
		Destroy (inventoryListContent.transform.GetChild (0).gameObject);
		setInventory ();
		inventoryPanel.SetActive (false);
		disableCurrentObjectInfo ();
		originalFov = inventoryCamera.fieldOfView;

	}

	void Update ()
	{
		if (inventoryEnabled) {
			if (input.checkInputButton ("Inventory", inputManager.buttonType.getKeyDown)) {
				openOrCloseInventory (!inventoryOpened);
			}
			if (enableRotation) {
				objectInCamera.transform.RotateAroundLocal (inventoryCamera.transform.up, -Mathf.Deg2Rad * rotationSpeed * input.getMovementAxis ("mouse").x);
				objectInCamera.transform.RotateAroundLocal (inventoryCamera.transform.right, Mathf.Deg2Rad * rotationSpeed * input.getMovementAxis ("mouse").y);
			}
			if (inventoryOpened) {
				if (zoomingIn) {
					if (inventoryCamera.fieldOfView > maxZoomValue) {
						inventoryCamera.fieldOfView -= Time.deltaTime * zoomSpeed;
					} else {
						inventoryCamera.fieldOfView = maxZoomValue;
					}
				}
				if (zoomingOut) {
					if (inventoryCamera.fieldOfView < minZoomValue) {
						inventoryCamera.fieldOfView += Time.deltaTime * zoomSpeed;
					} else {
						inventoryCamera.fieldOfView = minZoomValue;
					}
				}
			} 
		}
	}

	public void AddObjectToInventory (inventoryInfo obj)
	{
		if (isInventoryFull ()) {
			showInventoryFullMessage ();
			return;
		}
		inventoryInfo newObj = new inventoryInfo (obj);
		bool added = false;
		bool getTheRest = false;
		for (i = 0; i < inventoryList.Count; i++) {
			if (inventoryList [i].Name == newObj.Name && !added && !getTheRest && inventoryList [i].amount < maxObjectsAmountPerSpace) {
				if (inventoryList [i].amount + newObj.amount <= maxObjectsAmountPerSpace) {
					inventoryList [i].amount += newObj.amount;
					added = true;
				}
				if (inventoryList [i].amount + newObj.amount > maxObjectsAmountPerSpace && !added) {
//					while (inventoryList [i].amount + newObj.amount > maxObjectsAmountPerSpace) {
//						bucle++;
//						if (bucle > 100) {
//							print ("fallO");
//							return;
//						}
//					}
					print (inventoryList [i].amount + "+ " + newObj.amount + " -" + maxObjectsAmountPerSpace);
					newObj.amount = inventoryList [i].amount + newObj.amount - maxObjectsAmountPerSpace;
					inventoryList [i].amount = maxObjectsAmountPerSpace;
					getTheRest = true;
				}
			}
		}
		if (!added || getTheRest) {
			inventoryList.Add (newObj);
		}
		setInventory ();
	}

	public void setInventory ()
	{
		checkInventoryAmountPerSpace ();
		createInventoryIcon ();
	}

	public void checkInventoryAmountPerSpace ()
	{
		for (i = 0; i < inventoryList.Count; i++) {
			//if (i < inventorySpace - 1) {
			if (inventoryList [i].amount > maxObjectsAmountPerSpace) {
				while (inventoryList [i].amount > maxObjectsAmountPerSpace) {
					bucle++;
					if (bucle > 100) {
						return;
					}
					int amount = 0;
					if (inventoryList [i].amount - maxObjectsAmountPerSpace > 0) {
						amount = inventoryList [i].amount - maxObjectsAmountPerSpace;
						inventoryList [i].amount = maxObjectsAmountPerSpace;
					} else {
						amount = inventoryList [i].amount;
					}
					createObjectIcon (inventoryList [i], amount, true);
				}
			} else {
				createObjectIcon (inventoryList [i], inventoryList [i].amount, false);
			}
			//}
		}
	}

	public void createInventoryIcon ()
	{
		for (i = 0; i < inventoryList.Count; i++) {
			if (inventoryList [i].button != null) {
				Destroy (inventoryList [i].button.gameObject);
			}
		}
		for (i = 0; i < inventoryList.Count; i++) {
			GameObject newIconButton = (GameObject)Instantiate (objectIcon, Vector3.zero, Quaternion.identity);
			newIconButton.transform.SetParent (inventoryListContent.transform);
			newIconButton.transform.localScale = Vector3.one;
			newIconButton.transform.localPosition = Vector3.zero;
			inventoryMenuIconElement menuIconElement = newIconButton.GetComponent<inventoryMenuIconElement> ();
			menuIconElement.name.text = inventoryList [i].Name;
			menuIconElement.amount.text = inventoryList [i].amount.ToString ();
			menuIconElement.icon.texture = inventoryList [i].icon;
			menuIconElement.pressedIcon.SetActive (false);
			newIconButton.name = "inventoryObject-" + (i + 1).ToString ();
			Button button = menuIconElement.button;
			button.onClick.AddListener (() => {
				getPressedButton (button);
			});
			#if UNITY_EDITOR
			EditorUtility.SetDirty (button);
			#endif
			inventoryList [i].button = button;
			inventoryList [i].menuIconElement = menuIconElement;
		}
		for (i = 0; i < emptyInventoryList.Count; i++) {
			Destroy (emptyInventoryList [i]);
		}
		addEmptyInventoryIcons (inventorySpace - inventoryList.Count);
	}

	public void addEmptyInventoryIcons(int amount){
		for (i = 0; i < amount; i++) {
			GameObject newIconButton = (GameObject)Instantiate (objectIcon, Vector3.zero, Quaternion.identity);
			newIconButton.transform.SetParent (inventoryListContent.transform);
			newIconButton.transform.localScale = Vector3.one;
			newIconButton.transform.localPosition = Vector3.zero;
			inventoryMenuIconElement menuIconElement = newIconButton.GetComponent<inventoryMenuIconElement> ();
			menuIconElement.name.text = "Empty";
			menuIconElement.amount.text = "0";
			menuIconElement.icon.texture = null;
			menuIconElement.pressedIcon.SetActive (false);
			newIconButton.name = "inventoryObject-" + (inventorySpace + 1).ToString ();
			emptyInventoryList.Add (newIconButton);
		}
	}
	public void addInventoryExtraSpace(int amount){
		inventorySpace += amount;
		addEmptyInventoryIcons (amount);
	}

	public void createObjectIcon (inventoryInfo objectInfo, int amount, bool duplicated)
	{
		objectsAmount++;
		//print (amount);
		if (duplicated) {
			//print (i + " " + inventoryList [i].amount);
			int newIndexPosition = 0;
			if (i == inventoryList.Count - 1) {
				newIndexPosition = inventoryList.Count;
			} else {
				newIndexPosition = i + 1;
			}
			i++;
			duplicateObject = new inventoryInfo (objectInfo);
			duplicateObject.amount = amount;
			inventoryList.Insert (newIndexPosition, duplicateObject);
			//duplicateObject.amount = amount;
		}
	}

	public void getPressedButton (Button buttonObj)
	{
		for (i = 0; i < inventoryList.Count; i++) {
			if (inventoryList [i].button == buttonObj) {
				if (currentObject != null) {
					if (currentObject == inventoryList [i]) {
						return;
					}
				}
				setObjectInfo (i);
				if (inventoryCamera.fieldOfView != originalFov) {
					checkResetCameraFov (originalFov);
				}
				return;
			}
		}
	}

	public void disableCurrentObjectInfo ()
	{
		currentObjectName.text = "";
		currentObjectInfo.text = "";
		useButton.GetComponent<Image> ().color = buttonNotUsable;
		equipButton.GetComponent<Image> ().color = buttonNotUsable;
		dropButton.GetComponent<Image> ().color = buttonNotUsable;
		objectImage.enabled = false;
	}

	public void setCurrenObjectByPrefab (GameObject obj)
	{
		for (i = 0; i < inventoryList.Count; i++) {
			if (inventoryList [i].inventoryGameObject == obj) {
				currentObject = inventoryList [i];
			}
		}
	}

	public void searchForObjectNeed (GameObject obj)
	{
		objectNeedInventory (obj);
		useCurrentObject ();
	}

	public void objectNeedInventory (GameObject obj)
	{
		currentObjectThatNeedsInventory = obj;
	}

	public void useCurrentObject ()
	{
		if (currentObject.canBeUsed) {
			if (currentObjectThatNeedsInventory) {
				if (currentObjectThatNeedsInventory.GetComponent<useInventoryObject> ().objectNeeded == currentObject.inventoryGameObject) {
					currentObjectThatNeedsInventory.GetComponent<useInventoryObject> ().useObject ();
					StartCoroutine (showUsedObjectInfo (currentObjectThatNeedsInventory.GetComponent<useInventoryObject> ().objectUsedMessage));
					currentObject.amount--;
					updateAmount (currentObject.menuIconElement.amount, currentObject.amount);
					if (currentObject.amount == 0) {
						removeButton (currentObject);
					}
					openOrCloseInventory (false);
				} else {
					StartCoroutine (showUsedObjectInfo (currentObject.Name + " " + unableToUseObjectMessage));
				}
			} else {
				StartCoroutine (showUsedObjectInfo (currentObject.Name + " " + unableToUseObjectMessage));
			}
		}
	}

	IEnumerator showUsedObjectInfo (string info)
	{
		GetComponent<usingDevicesSytem> ().checkDeviceName ();
		usedObjectMessage.SetActive (true);
//		usedObjectMessage.GetComponent<Animation> () ["inventoryObjectUsedInfo"].speed = -1; 
//		usedObjectMessage.GetComponent<Animation> () ["inventoryObjectUsedInfo"].time = usedObjectMessage.GetComponent<Animation> () ["inventoryObjectUsedInfo"].length;
//		usedObjectMessage.GetComponent<Animation> ().Play ("inventoryObjectUsedInfo");
		usedObjectMessage.GetComponentInChildren<Text> ().text = info;
		yield return new WaitForSeconds (usedObjectMessageTime);
		usedObjectMessage.SetActive (false);
//		usedObjectMessage.GetComponent<Animation> () ["inventoryObjectUsedInfo"].speed = 1; 
//		usedObjectMessage.GetComponent<Animation> ().Play ("inventoryObjectUsedInfo");
	}

	public void equipCurrentObject ()
	{

	}

	public void dropCurrentObject ()
	{
		if (currentObject!=null && currentObject.amount > 0) {
			GameObject inventoryObjectClone = (GameObject)Instantiate (emptyInventoryPrefab, transform.position + transform.forward + transform.up, Quaternion.identity);
			GameObject inventoryMesh = (GameObject)Instantiate (currentObject.inventoryGameObject, transform.position + transform.forward + transform.up, Quaternion.identity);
			inventoryMesh.transform.SetParent (inventoryObjectClone.transform);
			inventoryMesh.transform.localPosition = Vector3.zero;

			Type type = inventoryMesh.GetComponent<Collider> ().GetType ();
			Component copy = inventoryObjectClone.AddComponent (type);
			BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
			PropertyInfo[] pinfos = type.GetProperties (flags);
			for (int i = 0; i < pinfos.Length; i++) {
				pinfos [i].SetValue (copy, pinfos [i].GetValue (inventoryMesh.GetComponent<Collider> (), null), null);
			}

			Destroy (inventoryMesh.GetComponent<Collider> ());

//			Component[] damageReceivers=inventoryMesh.GetComponentsInChildren(typeof(Collider));
//			foreach (Component c in damageReceivers) {
//				Type type = inventoryMesh.GetComponent<Collider> ().GetType ();
//				Component copy = inventoryObjectClone.AddComponent (type);
//				BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
//				PropertyInfo[] pinfos = type.GetProperties (flags);
//				for (int i = 0; i < pinfos.Length; i++) {
//					pinfos [i].SetValue (copy, pinfos [i].GetValue (inventoryMesh.GetComponent<Collider> (), null), null);
//				}
//
//				Destroy (inventoryMesh.GetComponent<Collider> ());
//			}
			currentObject.amount--;
			inventoryObject inventoryObjectManager = inventoryObjectClone.GetComponentInChildren<inventoryObject> ();
			if (inventoryObjectManager) {
				inventoryObjectManager.inventoryObjectInfo = new inventoryInfo (currentObject);
				inventoryObjectManager.inventoryObjectInfo.amount = 1;
				inventoryObjectClone.name = inventoryObjectManager.inventoryObjectInfo.Name + " (inventory)";
				inventoryObjectClone.GetComponentInChildren<deviceStringAction> ().deviceName = inventoryObjectManager.inventoryObjectInfo.Name;
				inventoryObjectClone.GetComponentInChildren<deviceStringAction> ().deviceAction = "Take ";

			}
			if (currentObject.amount > 0) {
				updateAmount (currentObject.menuIconElement.amount, currentObject.amount);
				if (combineElementsAtDrop) {
					//combine same objects when the amount of an object is lower than maxObjectsAmountPerSpace and there is another group equal to that object
					//for example if I have 10 cubes and 4 cubes with a maxObjectsAmountPerSpace of 10, and you drop 1 cube of the first group, this combines the other cubes
					//so after that, you have 9 cubes and 4 cubes, and then, this changes that into 10 cubes and 3 cubes
					//this only checks the next objects after current object
					int currentIndex = inventoryList.IndexOf (currentObject) + 1;
					int index = -1;
					for (i = currentIndex; i < inventoryList.Count; i++) {
						if (inventoryList [i].Name == currentObject.Name) {
							if (inventoryList [i].amount < maxObjectsAmountPerSpace && index == -1) {
								index = i;
							}
						}
					}
					//if there are more objects equals to the current object dropped, then check their remaining amount to combine their values
					if (index != -1) {
						int amountToChange = maxObjectsAmountPerSpace - currentObject.amount;
						if (amountToChange < inventoryList [index].amount) {
							inventoryList [index].amount -= amountToChange;
							currentObject.amount += amountToChange;
							updateAmount (currentObject.menuIconElement.amount, currentObject.amount);
							updateAmount (inventoryList [index].menuIconElement.amount, inventoryList [index].amount);
						} else if (amountToChange >= inventoryList [index].amount) {
							currentObject.amount += inventoryList [index].amount;
							inventoryList [index].amount -= inventoryList [index].amount;
							updateAmount (currentObject.menuIconElement.amount, currentObject.amount);
							removeButton (inventoryList [index]);
						}
					} else {
						//if  all the objects equal to this has the max amount per space, search the last one of them to drop an object
						//from its amount
						currentIndex =inventoryList.IndexOf (currentObject);
						for (i = inventoryList.Count - 1; i >= currentIndex; i--) {
							if (inventoryList [i].Name == currentObject.Name) {
								//|| i == inventoryList.Count - 1
								if ((inventoryList [i].amount == maxObjectsAmountPerSpace ) && index == -1) {
									index = i;
								}
							}
						}
						if (index != -1) {
							int amountToChange = maxObjectsAmountPerSpace - currentObject.amount;
							if (amountToChange < inventoryList [index].amount) {
								inventoryList [index].amount -= amountToChange;
								currentObject.amount += amountToChange;
								updateAmount (currentObject.menuIconElement.amount, currentObject.amount);
								updateAmount (inventoryList [index].menuIconElement.amount, inventoryList [index].amount);
							} else if (amountToChange >= inventoryList [index].amount) {
								currentObject.amount += inventoryList [index].amount;
								inventoryList [index].amount -= inventoryList [index].amount;
								updateAmount (currentObject.menuIconElement.amount, currentObject.amount);
								removeButton (inventoryList [index]);
							}
						}
					}
				}
			} else {
				removeButton (currentObject);
			}
		}
	}

	public void updateAmount (Text textAmount, int amount)
	{
		textAmount.text = amount.ToString ();
	}

	public void removeButton (inventoryInfo currentObj)
	{
		disableCurrentObjectInfo ();
		Destroy (currentObj.button.gameObject);
		inventoryList.Remove (currentObj);
		enableRotation = false;
		destroyObjectInCamera ();
		setMenuIconElementPressedState (false);
		currentObject = null;
		addEmptyInventoryIcons (1);
	}

	public void setMenuIconElementPressedState (bool state)
	{
		if (currentObject != null) {
			if (currentObject.menuIconElement != null) {
				currentObject.menuIconElement.pressedIcon.SetActive (state);
			}
		}
	}

	public void setObjectInfo (int index)
	{
		setMenuIconElementPressedState (false);
		currentObject = inventoryList [index];
		setMenuIconElementPressedState (true);
		currentObjectName.text = currentObject.Name;
		currentObjectInfo.text = currentObject.objectInfo;
		if (currentObject.canBeUsed) {
			useButton.GetComponent<Image> ().color = buttonUsable;
		} else {
			useButton.GetComponent<Image> ().color = buttonNotUsable;
		}
		if (currentObject.canBeEquiped) {
			equipButton.GetComponent<Image> ().color = buttonUsable;
		} else {
			equipButton.GetComponent<Image> ().color = buttonNotUsable;
		}
		if (currentObject.canBeDropped) {
			dropButton.GetComponent<Image> ().color = buttonUsable;
		} else {
			dropButton.GetComponent<Image> ().color = buttonNotUsable;
		}
		objectImage.enabled = true;
		destroyObjectInCamera ();
		objectInCamera = (GameObject)Instantiate (currentObject.inventoryGameObject, lookObjectsPosition.transform.position, Quaternion.identity);
		objectInCamera.transform.SetParent (lookObjectsPosition);
	}

	public void enableObjectRotation ()
	{
		if (objectInCamera) {
			enableRotation = true;
		}
	}

	public void disableObjectRotation ()
	{
		enableRotation = false;
	}

	public void destroyObjectInCamera ()
	{
		if (objectInCamera) {
			Destroy (objectInCamera);
		}
	}

	public void openOrCloseInventory (bool state)
	{
		if ((!pauseManager.playerMenuActive || inventoryOpened) && !pauseManager.usingDevice && !pauseManager.pauseGame) {
			inventoryOpened = state;
			setMenuIconElementPressedState (false);
			pauseManager.openOrClosePlayerMenu (inventoryOpened);
			inventoryPanel.SetActive (inventoryOpened);
			//set to visible the cursor
			pauseManager.showOrHideCursor (inventoryOpened);
			//disable the touch controls
			pauseManager.checkTouchControls (!inventoryOpened);
			//disable the camera rotation
			pauseManager.changeCameraState (!inventoryOpened);
			GetComponent<playerController> ().changeScriptState (!inventoryOpened);
			pauseManager.usingSubMenuState (inventoryOpened);
			destroyObjectInCamera ();
			if (!inventoryOpened) {
				disableCurrentObjectInfo ();
			}
			inventoryCamera.fieldOfView = originalFov;
			currentObject = null;
		}
	}

	public void openOrCLoseInventoryFromTouch ()
	{
		openOrCloseInventory (!inventoryOpened);
	}

	public void addNewInventoryObject ()
	{
		inventoryInfo newObject = new inventoryInfo ();
		inventoryList.Add (newObject);
		#if UNITY_EDITOR
		EditorUtility.SetDirty (GetComponent<inventoryManager> ());
		#endif
	}

	public bool isInventoryFull ()
	{
		if (inventoryList.Count >= inventorySpace) {
			return true;
		}
		return false;
	}

	public int freeSpaceInSlot (GameObject inventoryObjectMesh)
	{
		int freeSpaceAmount = -1;
		for (i = 0; i < inventoryList.Count; i++) {
			if (inventoryList [i].inventoryGameObject == inventoryObjectMesh) {
				freeSpaceAmount = maxObjectsAmountPerSpace - inventoryList [i].amount;
			}
		}
		return freeSpaceAmount;
	}

	public void addAmountToInventorySlot (GameObject inventoryObjectMesh, int currentSlotAmount, int extraAmount)
	{
		for (i = 0; i < inventoryList.Count; i++) {
			if (inventoryList [i].inventoryGameObject == inventoryObjectMesh && inventoryList [i].amount == currentSlotAmount) {
				inventoryList [i].amount += extraAmount;
				updateAmount (inventoryList [i].menuIconElement.amount, inventoryList [i].amount);
			}
		}
	}

	public void showInventoryFullMessage ()
	{
		if (inventoryFullCoroutine != null) {
			StopCoroutine (inventoryFullCoroutine);
		}
		inventoryFullCoroutine = StartCoroutine (showInventoryFullMessageCoroutine ());
	}

	IEnumerator showInventoryFullMessageCoroutine ()
	{
		fullInventoryMessage.SetActive (true);
		yield return new WaitForSeconds (fullinventoryMessageTime);
		fullInventoryMessage.SetActive (false);
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

	public void checkResetCameraFov (float targetValue)
	{
		if (resetCameraFov != null) {
			StopCoroutine (resetCameraFov);
		}
		resetCameraFov = StartCoroutine (resetCameraFovCorutine (targetValue));
	}

	IEnumerator resetCameraFovCorutine (float targetValue)
	{
		while (inventoryCamera.fieldOfView != targetValue) {
			inventoryCamera.fieldOfView = Mathf.MoveTowards (inventoryCamera.fieldOfView, targetValue, Time.deltaTime * zoomSpeed);
			yield return null;
		}
	}

	public void setInventoryCaptureIcon (inventoryInfo info, Texture2D texture)
	{
		for (i = 0; i < inventoryList.Count; i++) {
			if (inventoryList [i] == info) {
				inventoryList [i].icon = texture;
			}
		}
		#if UNITY_EDITOR
		EditorUtility.SetDirty (GetComponent<inventoryManager> ());
		#endif
	}

	public string getDataPath ()
	{
		string dataPath = "";
		if (useRelativePath) {
			if (!Directory.Exists (relativePath)) {
				Directory.CreateDirectory (relativePath);
			}
			dataPath = relativePath + "/";
		} else {
			dataPath = Application.persistentDataPath + "/";
		}
		return dataPath;
	}

	[System.Serializable]
	public class inventoryInfo
	{
		public string Name;
		public GameObject inventoryGameObject;
		[TextArea (3, 10)] public string objectInfo;
		public Texture icon;
		public int amount;
		public bool canBeUsed;
		public bool canBeEquiped;
		public bool canBeDropped;
		public Button button;
		public inventoryMenuIconElement menuIconElement;

		public inventoryInfo (inventoryInfo obj)
		{
			Name = obj.Name;
			inventoryGameObject = obj.inventoryGameObject;
			objectInfo = obj.objectInfo;
			icon = obj.icon;
			amount = obj.amount;
			canBeUsed = obj.canBeUsed;
			canBeEquiped = obj.canBeEquiped;
			canBeDropped = obj.canBeDropped;
			button = obj.button;
		}

		public inventoryInfo ()
		{
			Name = "New Object";
			objectInfo = "New Description";
		}
	}
}