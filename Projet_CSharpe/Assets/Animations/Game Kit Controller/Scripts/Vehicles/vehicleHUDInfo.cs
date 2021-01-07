using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
public class vehicleHUDInfo : MonoBehaviour {
	//this scripts allows to a vehicle to get all the hud elements, so all the sliders values and text info can be showed correctly
	public GameObject playerHUD;
	public GameObject vehicleHUD;
	public Slider vehicleHealth;
	public Slider vehicleBoost;
	public Slider vehicleAmmo;
	public Text weaponName;
	public Text ammoInfo;
	public GameObject ammoContent;
	public Text currentSpeed;
	public GameObject vehicleControlsMenu;
	public GameObject vehicleControlsMenuElement;
	int i,j;
	GameObject bottom;
	GameObject currentVehicle;
	List<GameObject> actionList=new List<GameObject>();

	void Start(){
		vehicleControlsMenu.SetActive(true);
		bottom = vehicleControlsMenuElement.transform.parent.gameObject.transform.GetChild(0).gameObject;
		vehicleControlsMenu.SetActive(false);
	}
	public void setControlList(inputActionManager manager){
		for (i = 0; i < actionList.Count; i++) {
			Destroy (actionList [i]);
		}
		actionList.Clear ();
		vehicleControlsMenuElement.SetActive (true);
		vehicleControlsMenu.SetActive(true);
		//every key field in the edit input button has an editButtonInput component, so create every of them
		for (i = 0; i < manager.inputActionList.Count; i++) {
			if (manager.inputActionList [i].showInControlsMenu) {
				GameObject buttonClone = (GameObject)Instantiate (vehicleControlsMenuElement, vehicleControlsMenuElement.transform.position, Quaternion.identity);
				buttonClone.transform.SetParent (vehicleControlsMenuElement.transform.parent);
				buttonClone.transform.localScale = Vector3.one;
				buttonClone.name = manager.inputActionList [i].name;
				buttonClone.transform.GetChild (0).GetComponent<Text> ().text = manager.inputActionList [i].name;
				for (j = 0; j < manager.input.axes.Count; j++) {
					if (manager.input.axes [j].Name == manager.inputActionList [i].inputActionName) {
						buttonClone.transform.GetChild (1).GetComponentInChildren<Text> ().text = manager.input.axes [j].keyButton;
					}
				}
				actionList.Add (buttonClone);
			}
		}
		//set the empty element of the list in the bottom of the list
		bottom.transform.SetParent(null);
		bottom.transform.SetParent(vehicleControlsMenuElement.transform.parent);
		//get the scroller in the edit input menu
		Scrollbar scroller = vehicleControlsMenu.GetComponentInChildren<Scrollbar> ();
		//set the scroller in the top position
		scroller.value = 1;
		//disable the menu
		vehicleControlsMenu.SetActive(false);
		vehicleControlsMenuElement.SetActive (false);
	}
	public void openOrCloseControlsMenu(bool state){
		vehicleControlsMenu.SetActive(state);
	}
	public void setCurrentVehicle(GameObject vehicle){
		currentVehicle = vehicle;
	}
	public void closeControlsMenu(){
		if (currentVehicle) {
			currentVehicle.GetComponent<vehicleHUDManager> ().IKDrivingManager.openOrCloseControlsMenu (false);
		}
	}
}