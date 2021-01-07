using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class pickUpIconManager : MonoBehaviour
{
	public List<pickUpIconElement> pickUpList = new List<pickUpIconElement> ();
	public List<pickUpIcon> pickUpIconList = new List<pickUpIcon> ();
	public string[] managerPickUpList;
	public GameObject pickUpIconObject;
	public LayerMask layer;
	public checkIconType checkIcontype;
	public float maxDistanceIconEnabled;
	GameObject character;
	Camera mainCamera;
	Transform mainCameraTransform;
	pickUpManager manager;
	//how to check if the icon is visible,
	//		-using a raycast from the object to the camera
	//		-using distance from the object to the player position
	//		-visible always that the player is looking at the object position
	public enum checkIconType
	{
		raycast,
		distance,
		always_visible,
		nothing
	}

	void Start ()
	{
		mainCamera = Camera.main;
		mainCameraTransform = mainCamera.transform;
	}

	void Update ()
	{
		if (pickUpIconList.Count > 0) {
			for (int i = 0; i < pickUpIconList.Count; i++) {
				if (pickUpIconList [i].target) {
					//get the target position from global to local in the screen
					Vector3 screenPoint = mainCamera.WorldToScreenPoint (pickUpIconList [i].target.transform.position);
					//if the target is visible in the screen, enable the icon
					if (screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < Screen.width && screenPoint.y > 0 && screenPoint.y < Screen.height) {
						pickUpIconList [i].iconObject.transform.position = screenPoint;
						//use a raycast to check if the icon is visible
						if (checkIcontype == checkIconType.raycast) {
							float distance = Vector3.Distance (pickUpIconList [i].target.transform.position, mainCameraTransform.position);
							if (distance <= maxDistanceIconEnabled) {
								//set the direction of the raycast
								Vector3 direction = pickUpIconList [i].target.transform.position - mainCameraTransform.position;
								direction = direction / direction.magnitude;
								//Debug.DrawRay(target.transform.position,-direction*distance,Color.red);
								//if the raycast find an obstacle between the pick up and the camera, disable the icon
								if (Physics.Raycast (pickUpIconList [i].target.transform.position, -direction, distance, layer)) {
									if (pickUpIconList [i].iconObject.activeSelf) {
										enableOrDisableIcon (false, i);
									}
								} else {
									//else, the raycast reachs the camera, so enable the pick up icon
									if (!pickUpIconList [i].iconObject.activeSelf) {
										enableOrDisableIcon (true, i);
									}
								}
							} else {
								if (pickUpIconList [i].iconObject.activeSelf) {
									enableOrDisableIcon (false, i);
								}
							}
						} else if (checkIcontype == checkIconType.distance) {
							//if the icon uses the distance, then check it
							float distance = Vector3.Distance (pickUpIconList [i].target.transform.position, mainCameraTransform.position);
							if (distance <= maxDistanceIconEnabled) {
								if (!pickUpIconList [i].iconObject.activeSelf) {
									enableOrDisableIcon (true, i);
								}
							} else {
								if (pickUpIconList [i].iconObject.activeSelf) {
									enableOrDisableIcon (false, i);
								}
							}
						} else {
							//else, always visible when the player is looking at its direction
							if (!pickUpIconList [i].iconObject.activeSelf) {
								enableOrDisableIcon (true, i);
							}
						}
					} else {
						//else the icon is only disabled, when the player is not looking at its direction
						if (pickUpIconList [i].iconObject.activeSelf) {
							enableOrDisableIcon (false, i);
						}
					}
				} else {
					removeAtTarget (i);
				}
			}
		}
	}

	public void enableOrDisableIcon (bool state, int index)
	{
		pickUpIconList [index].iconObject.SetActive (state);
	}
	//set what type of pick up is this object, and the object that the icon has to follow
	public void setPickUpIcon (GameObject target, pickUpObject.pickUpType iconType)
	{
		if (checkIcontype == checkIconType.nothing) {
			return;
		}
		string iconName = "";
		switch (iconType) {
		case pickUpObject.pickUpType.health:
			iconName = "Health";
			break;
		case pickUpObject.pickUpType.energy:
			iconName = "Energy";
			break;
		case pickUpObject.pickUpType.ammo:
			iconName = "Ammo";
			break;
		case pickUpObject.pickUpType.inventory:
			iconName = "Inventory";
			break;
		case pickUpObject.pickUpType.jetpackFuel:
			iconName = "Fuel";
			break;
		case pickUpObject.pickUpType.weapon:
			iconName = "Weapon";
			break;
		}
		GameObject newIconElement = (GameObject)Instantiate (pickUpIconObject, pickUpIconObject.transform.position, Quaternion.identity);
		pickUpIcon newIcon = newIconElement.GetComponent<pickUpIcon> ();
		newIcon.iconObject.transform.SetParent (pickUpIconObject.transform.parent);
		newIcon.transform.localScale = Vector3.one;
		newIcon.target = target;
		newIcon.gameObject.SetActive (true);

		for (int i = 0; i < pickUpList.Count; i++) {
			if (pickUpList [i].pickUpType == iconName) {
				if (pickUpList [i].isRawImage) {
					newIcon.texture.AddComponent<RawImage> ().texture = pickUpList [i].iconTexture;
				} else {
					newIcon.texture.AddComponent<Image> ().sprite = pickUpList [i].iconTextureSprite;
				}
			}
		}
		pickUpIconList.Add (newIcon);
	}
	//destroy the icon
	public void removeTarget (GameObject target)
	{
		for (int i = 0; i < pickUpIconList.Count; i++) {
			if (pickUpIconList [i].target == target) {
				removeAtTarget (i);
				return;
			}
		}
	}

	public void removeAtTarget(int index){
		Destroy (pickUpIconList [index].iconObject);
		pickUpIconList.RemoveAt (index);
	}

	public void getManagerPickUpList ()
	{
		if (!character) {
			character = GameObject.Find ("Character");
			manager = character.GetComponent<pickUpManager> ();
		} 
		if (character) {
			managerPickUpList = new string[manager.mainPickUpList.Count];
			for (int i = 0; i < managerPickUpList.Length; i++) {
				managerPickUpList [i] = manager.mainPickUpList [i].pickUpType;
				bool pickUpAlreadyAdded = false;
				for (int j = 0; j < pickUpList.Count; j++) {
					if (pickUpList [j].pickUpType == managerPickUpList [i]) {
						pickUpAlreadyAdded = true;
					}
				}
				if (!pickUpAlreadyAdded) {
					pickUpIconElement newIcon = new pickUpIconElement ();
					newIcon.pickUpType = managerPickUpList [i];
					newIcon.typeIndex = i;
					pickUpList.Add (newIcon);
				}
			}
			#if UNITY_EDITOR
			EditorUtility.SetDirty (GetComponent<pickUpIconManager> ());
			#endif
		}
	}

	[System.Serializable]
	public class pickUpIconElement
	{
		public string pickUpType;
		public bool isRawImage;
		public Texture iconTexture;
		public Sprite iconTextureSprite;
		public int typeIndex;
	}
}