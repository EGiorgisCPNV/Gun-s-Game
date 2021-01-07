using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class chestSystem : MonoBehaviour {
	public List<chestPickUpElementInfo> chestPickUpList = new List<chestPickUpElementInfo> ();
	public List<pickUpElementInfo> managerPickUpList = new List<pickUpElementInfo> ();
	public GameObject pickUpIcon;
	public bool randomContent;
	public bool rachargeable;
	public float timeOpenedAfterEmtpy;
	public float refilledTime;
	public string openAnimationName;
	public int numberOfObjects;
	public int minAmount;
	public int maxAmount;
	public Transform placeWhereInstantiatePickUps;
	public Vector3 placeOffset;
	public Vector3 space;
	public Vector2 amount;
	public float pickUpScale;
	public bool showGizmo;
	public Color gizmoColor;
	public Color gizmoLabelColor;
	public float gizmoRadius;
	public bool settings;
	List<GameObject> objectsList =new List<GameObject> ();
	GameObject newObject;
	GameObject objectsParent;
	GameObject player;
	bool enter;
	bool opened;
	Animation chestAnim;
	GameObject character;
	pickUpManager manager;

	void Start () {
		objectsParent = transform.GetChild (0).gameObject;
		chestAnim = GetComponent<Animation> ();
		if (!pickUpIcon) {
			pickUpIcon = GameObject.Find ("pickUpObjectsIcons").transform.GetChild (0).gameObject;
		}
	}

	void Update () {
		//if the chest can be refilled once is has been opened, check if is empty, and then wait one second to close it again
		if (opened && rachargeable) {
			if (objectsParent.transform.childCount == 0) {
				StartCoroutine (waitTimeOpened ());
				opened = false;
			}
		}
	}
	//instantiate the objects inside the chest, setting their configuration
	void createObjects(){
		numberOfObjects = 0;
		for (int i = 0; i < chestPickUpList.Count; i++) {
			for (int k = 0; k < chestPickUpList [i].chestPickUpTypeList.Count; k++) {
				//of every object, create the amount set in the inspector, the ammo and the inventory objects will be added in future updates
				int maxAmount = chestPickUpList [i].chestPickUpTypeList[k].amount;
				int quantity = chestPickUpList [i].chestPickUpTypeList[k].quantity;
				if (randomContent) {
					maxAmount = (int)Random.Range (chestPickUpList [i].chestPickUpTypeList[k].amountLimits.x, chestPickUpList [i].chestPickUpTypeList[k].amountLimits.y);
				}
				for (int j = 0; j < maxAmount; j++) {
					if (randomContent) {
						quantity = (int)Random.Range (chestPickUpList [i].chestPickUpTypeList[k].quantityLimits.x, chestPickUpList [i].chestPickUpTypeList[k].quantityLimits.y);
					}
					GameObject objectToInstantiate = managerPickUpList [chestPickUpList [i].typeIndex].pickUpTypeList [chestPickUpList [i].chestPickUpTypeList[k].nameIndex].pickUpObject;
					newObject = (GameObject)Instantiate (objectToInstantiate, transform.position, Quaternion.identity);
					if (newObject.GetComponent<pickUpObject> ()) {
						newObject.GetComponent<pickUpObject> ().amount = quantity;
					}
					newObject.transform.localScale = Vector3.one * pickUpScale;
					addNewObject (newObject);
				}
				numberOfObjects += maxAmount;
			}
		}
		//set a fix position inside the chest, according to the amount of objects instantiated
		//the position of the first object
		Vector3 currentPosition = placeWhereInstantiatePickUps.position + placeOffset;
		//the original x and z values, to make rows of the objects
		float originalX = currentPosition.x;
		float originalZ = currentPosition.z;
		int rows = 0;
		//set the localposition of every object, so every object is actually inside the chest
		for (int i = 0; i < numberOfObjects; i++) {	
			objectsList [i].transform.position = currentPosition;
			currentPosition.x += space.x;
			if (i != 0 && (i + 1) % Mathf.Round (amount.y) == 0) {
				currentPosition.z -= space.z;
				currentPosition.x = originalX;
				rows++;
			}
			if (rows == Mathf.Round (amount.x)) {
				currentPosition.y += space.y;
				rows = 0;
				currentPosition.z = originalZ;
			}
		}
		objectsList.Clear ();
	}
	public void addNewObject(GameObject newObject){
		newObject.transform.parent = objectsParent.transform;
		newObject.transform.GetChild (0).GetComponent<SphereCollider> ().enabled = false;
		objectsList.Add (newObject);
	}
	//when the player press the interaction button, this function is called
	void activateDevice(){
		//check that the chest is not already opening, and play the open animation
		if (enter && !chestAnim.IsPlaying (openAnimationName)) {
			if (!opened) {
				chestAnim [openAnimationName].speed = 1; 
				chestAnim.Play (openAnimationName);
				opened = true;
				createObjects ();
			}
		}
	}
	IEnumerator waitTimeOpened(){
		yield return new WaitForSeconds (timeOpenedAfterEmtpy);
		//when the second ends, play the open animation reversed, to close it, enabling the icon of open chest again
		chestAnim [openAnimationName].speed = -1; 
		chestAnim [openAnimationName].time = chestAnim [openAnimationName].length;
		chestAnim.Play (openAnimationName);
		//wait the recharge time, so the chest can be reopened again
		yield return new WaitForSeconds (chestAnim [openAnimationName].length + refilledTime);
		//once the waiting time is over, enable the interaction button of the player
		tag = "device";
		if (enter) {
			//enable the open icon in the hud
			player.GetComponent<usingDevicesSytem> ().OnTriggerEnter (gameObject.GetComponent<Collider> ());
		}
	}
	//check when the player enters or exits in the trigger of the chest
	void OnTriggerEnter(Collider col){
		if (col.tag == "Player") {
			enter = true;
			if (!player) {
				player = col.gameObject;
			}
		} 
	}
	void OnTriggerExit(Collider col){
		if (col.tag == "Player") {
			enter = false;
			if(opened){
				tag="Untagged";
			}
		}
	}
	public void getManagerPickUpList(){
		if (!character) {
			character = GameObject.Find ("Character");
			manager = character.GetComponent<pickUpManager> ();
		} 
		if (character){
			managerPickUpList = manager.mainPickUpList;
			for (int i = 0; i < managerPickUpList.Count; i++) {
				print (managerPickUpList [i].pickUpType);
			}
			#if UNITY_EDITOR
			EditorUtility.SetDirty (GetComponent<chestSystem>() );
			#endif
		}
	}
	void OnDrawGizmos(){
		DrawGizmos ();
	}
	void OnDrawGizmosSelected(){
		DrawGizmos ();
	}
	void DrawGizmos(){
		if (!Application.isPlaying && showGizmo && placeWhereInstantiatePickUps) {
			Vector3 currentPosition = placeWhereInstantiatePickUps.position + placeOffset;
			//the original x and z values, to make rows of the objects
			float originalX = currentPosition.x;
			float originalZ = currentPosition.z;
			int rows = 0;
			//set the localposition of every object, so every object is actually inside the chest
			numberOfObjects = 0;
			if (randomContent) {
				minAmount = 0;
				maxAmount = 0;
				for (int i = 0; i < chestPickUpList.Count; i++) {	
					for (int j = 0; j < chestPickUpList [i].chestPickUpTypeList.Count; j++) {	
						minAmount += (int)chestPickUpList [i].chestPickUpTypeList [j].amountLimits.x;
						maxAmount += (int)chestPickUpList [i].chestPickUpTypeList [j].amountLimits.y;
					}
				}
				numberOfObjects = minAmount + maxAmount;
				for (int i = 0; i < numberOfObjects; i++) {	
					if (i < minAmount) {
						Gizmos.color = Color.blue;
						Gizmos.DrawSphere (currentPosition, gizmoRadius);
						Gizmos.color = gizmoLabelColor;
						Gizmos.DrawWireSphere (currentPosition, pickUpScale);
						currentPosition.x += space.x;
						if (i != 0 && (i + 1) % Mathf.Round (amount.y) == 0) {
							currentPosition.z -= space.z;
							currentPosition.x = originalX;
							rows++;
						}
						if (rows == Mathf.Round (amount.x)) {
							currentPosition.y += space.y;
							rows = 0;
							currentPosition.z = originalZ;
						}
					}
					if (i >= minAmount) {
						Gizmos.color = Color.red;
						Gizmos.DrawSphere (currentPosition, gizmoRadius);
						Gizmos.color = gizmoLabelColor;
						Gizmos.DrawWireSphere (currentPosition, pickUpScale);
						currentPosition.x += space.x;
						if (i != 0 && (i + 1) % Mathf.Round (amount.y) == 0) {
							currentPosition.z -= space.z;
							currentPosition.x = originalX;
							rows++;
						}
						if (rows == Mathf.Round (amount.x)) {
							currentPosition.y += space.y;
							rows = 0;
							currentPosition.z = originalZ;
						}
					}
				}
			} else {
				for (int i = 0; i < chestPickUpList.Count; i++) {	
					for (int j = 0; j < chestPickUpList [i].chestPickUpTypeList.Count; j++) {	
						numberOfObjects += chestPickUpList [i].chestPickUpTypeList [j].amount;
					}
				}
				for (int i = 0; i < numberOfObjects; i++) {	
					Gizmos.color = gizmoColor;
					Gizmos.DrawSphere (currentPosition, gizmoRadius);
					Gizmos.color = gizmoLabelColor;
					Gizmos.DrawWireSphere (currentPosition, pickUpScale);
					currentPosition.x += space.x;
					if (i != 0 && (i + 1) % Mathf.Round (amount.y) == 0) {
						currentPosition.z -= space.z;
						currentPosition.x = originalX;
						rows++;
					}
					if (rows == Mathf.Round (amount.x)) {
						currentPosition.y += space.y;
						rows = 0;
						currentPosition.z = originalZ;
					}
				}
			}
		}
	}
	[System.Serializable]
	public class chestPickUpElementInfo{
		public string pickUpType;
		public int typeIndex;
		public List<chestPickUpTypeElementInfo> chestPickUpTypeList = new List<chestPickUpTypeElementInfo> ();
	}
	[System.Serializable]
	public class chestPickUpTypeElementInfo{
		public string name;
		public int amount;
		public int quantity;
		public Vector2 amountLimits;
		public Vector2 quantityLimits;
		public int nameIndex;
	}
}