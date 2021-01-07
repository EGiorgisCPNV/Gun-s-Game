using UnityEngine;
using System.Collections;
public class useInventoryObject : MonoBehaviour {
	public GameObject objectNeeded;
	public GameObject objectToCall;
	public string functionName;
	public useInventoryObjectType useInventoryType;
	public string inventoryObjectAction;
	[TextArea(3,10)]
	public string objectUsedMessage;
	public bool enableObjectWhenActivate;
	public GameObject objectToEnable;
	public bool instantiateObjectUsed;
	public Transform placeToInstantiateObject;
	public bool useAnimation;
	public GameObject objectWithAnimation;
	public string animationName;
	public bool disableObjectActionAfterUse;
	public bool objectUsed;
	public enum useInventoryObjectType{
		menu, button, automatic
	}
	GameObject player;
	inventoryManager playerInventoryManager;
	string previousAction;
	deviceStringAction deviceStringActionManager;

	void Start () {
		deviceStringActionManager = GetComponent<deviceStringAction> ();
		if (deviceStringActionManager) {
			previousAction = deviceStringActionManager.deviceAction;
		}
	}
	void Update () {
	
	}
	public void useObject(){
		if (!objectUsed) {
			objectUsed = true;
			objectToCall.SendMessage (functionName);
			if (deviceStringActionManager) {
				if (disableObjectActionAfterUse) {
					deviceStringActionManager.showIcon = false;
					player.GetComponent<usingDevicesSytem> ().hideIconButton();
				} else {
					deviceStringActionManager.deviceAction = previousAction;
				}
			}
			if (instantiateObjectUsed) {
				Instantiate (objectNeeded, placeToInstantiateObject.position, placeToInstantiateObject.rotation);
			} else if (enableObjectWhenActivate) {
				objectToEnable.SetActive (true);
				if (useAnimation) {
					objectWithAnimation.GetComponent<Animation>().Play (animationName);
				}
			}
		}
	}
	public void OnTriggerEnter(Collider col){
		if (!objectUsed && col.tag == "Player" && !col.isTrigger) {
			if (!player) {
				player = col.gameObject;
				playerInventoryManager = player.GetComponent<inventoryManager> ();
			}
			if(!objectUsed){
				if (deviceStringActionManager) {
					deviceStringActionManager.deviceAction = inventoryObjectAction;
				}
				if (useInventoryType == useInventoryObjectType.button) {
					playerInventoryManager.setCurrenObjectByPrefab (objectNeeded);
					playerInventoryManager.objectNeedInventory (gameObject);
				} 
				else if (useInventoryType == useInventoryObjectType.menu) {
					playerInventoryManager.objectNeedInventory (gameObject);
				} 
				else if (useInventoryType == useInventoryObjectType.automatic) {
					playerInventoryManager.setCurrenObjectByPrefab (objectNeeded);
					playerInventoryManager.searchForObjectNeed (gameObject);
				}
			}
		}
	}
	public void OnTriggerExit(Collider col){
		if (!objectUsed && col.tag == "Player" && !col.isTrigger) {
			playerInventoryManager.objectNeedInventory (null);
		}
	}
}