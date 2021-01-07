using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class pickUpObject : MonoBehaviour
{
	public pickUpType pickType;
	public float amount;
	public bool useSecondaryString;
	public string secondaryString;
	public AudioClip pickUpSound;
	public bool staticPickUp;
	public bool moveToPlayerOnTrigger = true;
	public pickUpMode pickUpOption;

	public enum pickUpType
	{
		health,
		energy,
		ammo,
		inventory,
		jetpackFuel,
		weapon, 
		inventoryExtraSpace,
		map
	}

	public enum pickUpMode
	{
		trigger,
		button
	}

	bool touched;
	GameObject player;
	GameObject vehicle;
	Rigidbody mainRigidbody;
	bool freeSpaceInInventorySlot;
	int inventoryAmountPicked;
	inventoryManager playerInventoryManager;
	inventoryObject inventoryObjectManager;
	pickUpsScreenInfo pickUpsScreenInfoManager;
	playerWeaponsManager weaponsManager;
	otherPowers playerPowersManager;
	vehicleHUDManager vehicleHUD;
	Vector3 pickUpTargetPosition;
	GameObject character;

	//if the pick up object has an icon in the inspector, instantiated in the hud
	void Start ()
	{
		character = GameObject.Find ("Character");
		mainRigidbody = GetComponent<Rigidbody> ();
		setUpIcon ();
		//if the pick up is static, set its rigibody to kinematic and reduce its radius, so the player has to come closer to get it
		if (staticPickUp) {
			mainRigidbody.isKinematic = true;
			transform.GetComponentInChildren<SphereCollider> ().radius = 1;
		}
		if (pickType == pickUpType.inventory) {
			inventoryObjectManager = GetComponentInChildren<inventoryObject> ();
		}
		if (pickType == pickUpType.inventoryExtraSpace) {
			inventoryObjectManager = GetComponentInChildren<inventoryObject> ();
		}
	}

	void Update ()
	{
		//if the player enters inside the object's trigger, translate the object's position to the player 
		if (touched && player) {
			if (vehicle) {
				pickUpTargetPosition = vehicle.transform.position;
			} else {
				pickUpTargetPosition = player.transform.position + player.transform.up * 1.5f;
			}
			transform.position = Vector3.MoveTowards (transform.position, pickUpTargetPosition, Time.deltaTime * 15);
			//if the object is close enough, increase the player's values, according to the type of object
			if (Vector3.Distance (transform.position, (player.transform.position + player.transform.up * 1.5f)) < 1) {
				pickObject ();
			}
		}
	}

	public void pickObject ()
	{
		//play the pick up sound effect
		Camera.main.GetComponent<AudioSource> ().PlayOneShot (pickUpSound);
		//check if this object has been grabbed by the player, to drop it, before destroy it
		checkIfGrabbed ();
		switch (pickType) {
		//the type of pickup is health
		case pickUpType.health:
			//if the player is not driving then
			if (!vehicle) {
				//increase its health
				playerPowersManager.getHealth (amount);
			} 
			//the player is driving so the pickup will recover its health
			else {
				vehicleHUD.getHealth (amount);
			}
			//set the info in the screen to show the type of object used and its amount
			pickUpsScreenInfoManager.recieveInfo ("Healht x " + amount.ToString ());
			break;
		//the other pickups works in the same way
		case pickUpType.energy:
			if (!vehicle) {
				playerPowersManager.getEnergy (amount);
			} else {
				vehicleHUD.getEnergy (amount);
			}
			pickUpsScreenInfoManager.recieveInfo ("Energy x " + amount.ToString ());
			break;
		case pickUpType.ammo:
			if (!vehicle) {
				weaponsManager.AddAmmo ((int)Mathf.Round (amount), secondaryString);
			} else {
				vehicleHUD.getAmmo (secondaryString, (int)Mathf.Round (amount));
			}
			pickUpsScreenInfoManager.recieveInfo ("Ammo " + secondaryString + " x " + Mathf.Round (amount).ToString ());
			break;
		case pickUpType.inventory:
			if (!vehicle) {
				player.GetComponent<inventoryManager> ().AddObjectToInventory (inventoryObjectManager.inventoryObjectInfo);
				string info = inventoryObjectManager.inventoryObjectInfo.Name + " Stored";
				if (inventoryObjectManager.inventoryObjectInfo.amount > 1) {
					info = inventoryObjectManager.inventoryObjectInfo.Name + " x ";
				}
				if (inventoryAmountPicked > 0) {
					info += inventoryAmountPicked;
				} else {
					info += inventoryObjectManager.inventoryObjectInfo.amount;
				}
				pickUpsScreenInfoManager.recieveInfo (info);
				if (freeSpaceInInventorySlot) {
					inventoryAmountPicked = 0;
					freeSpaceInInventorySlot = false;
					return;
				}
			}
			break;
		case pickUpType.jetpackFuel:
			if (!vehicle) {
				player.GetComponent<jetpackSystem> ().getJetpackFuel (amount);
			} 
			pickUpsScreenInfoManager.recieveInfo ("Fuel x " + amount.ToString ());
			break;
		case pickUpType.weapon:
			if (!vehicle) {
				weaponsManager.pickWeapon (secondaryString);
			} 
			pickUpsScreenInfoManager.recieveInfo (secondaryString + " Picked");
			break;
		case pickUpType.inventoryExtraSpace:
			if (!vehicle) {
				int extraSpaceAmount = inventoryObjectManager.inventoryObjectInfo.amount;
				player.GetComponent<inventoryManager> ().addInventoryExtraSpace (extraSpaceAmount);
				pickUpsScreenInfoManager.recieveInfo ("+" + extraSpaceAmount + " slots added to inventory");
			}
			break;
		case pickUpType.map:
			if (!vehicle) {
				GetComponent < mapZoneUnlocker> ().unlockMapZone ();
				pickUpsScreenInfoManager.recieveInfo ("Map Zone Picked");
			}
			break;
		}
		//remove the icon object
		character.GetComponent<pickUpIconManager> ().removeTarget (gameObject);
		Destroy (gameObject);
	}

	//instantiate the icon object to show the type of pick up in the player's HUD
	public void setUpIcon ()
	{
		character.GetComponent<pickUpIconManager> ().setPickUpIcon (gameObject, pickType);
	}

	public void pickObjectByButton ()
	{
		if (!checkIfCanBePicked ()) {
			return;
		}
		Physics.IgnoreCollision (player.GetComponent<Collider> (), transform.GetComponent<Collider> ());
		checkIfGrabbed ();
		pickObject ();
	}

	//check if the player is inside the object trigger
	public void OnTriggerEnter (Collider col)
	{
		if ((col.tag == "Player" && !col.isTrigger)) {
			player = col.GetComponent<Collider> ().gameObject;
			pickUpsScreenInfoManager = player.GetComponent<pickUpsScreenInfo> ();
			//check if the player needs this pickup
			if (pickUpOption == pickUpMode.trigger) {
				if (!checkIfCanBePicked ()) {
					return;
				}
				Physics.IgnoreCollision (player.GetComponent<Collider> (), transform.GetComponent<Collider> ());
				checkIfGrabbed ();
				mainRigidbody.isKinematic = true;
				if (moveToPlayerOnTrigger) {
					touched = true;
				} else {
					pickObject ();
				}
			}
		}
		//else check if the player is driving
		else if (col.GetComponent<vehicleHUDManager> ()) {
			if (col.GetComponent<vehicleHUDManager> ().driving) {
				//then set the vehicle as the object which use the pickup
				vehicle = col.GetComponent<Collider> ().gameObject;
				vehicleHUD = vehicle.GetComponent<vehicleHUDManager> ();
				player = vehicleHUD.IKDrivingManager.player;
				pickUpsScreenInfoManager = player.GetComponent<pickUpsScreenInfo> ();
				//check if the vehicle needs this pickup
				if (!checkIfCanBePicked ()) {
					return;
				}
				GetComponent<Collider> ().isTrigger = true;
				checkIfGrabbed ();
				mainRigidbody.isKinematic = true;
				if (moveToPlayerOnTrigger) {
					touched = true;
				} else {
					pickObject ();
				}
			}
		}
	}
	//check the values of health and energy according to the type of pickup, so the pickup will be used or not according to the values of health or energy
	//When the player/vehicle grabs a pickup, this will check if the amount of health, energy or ammo is filled or not,
	//so the player/vehicle only will get the neccessary objects to restore his state. In version 2.3, the player grabbed every pickup close to him.
	//for example, if the player has 90/100, he only will grab a health pickup
	bool checkIfCanBePicked ()
	{
		bool pick = false;
		if (pickType == pickUpType.health) {
			if (!vehicle) {
				//if the player is not driving then increase an auxiliar value to check the amount of the same pickup that the player will use at once 
				//for example, when the player is close to more than one pickup, if he has 90/100 of health and he is close to two health pickups, 
				//he only will grab one of them.
				playerPowersManager = player.GetComponent<otherPowers> ();
				if (playerPowersManager.auxHealthAmount < playerPowersManager.settings.healthBar.maxValue) {
					playerPowersManager.auxHealthAmount += amount;
					pick = true;
				}
			} else {
				//check the same if the player is driving and works in the same way for any type of pickup
				if (vehicleHUD.auxHealthAmount < vehicleHUD.vehicleHealth.maxValue) {
					vehicleHUD.auxHealthAmount += amount;
					pick = true;
				}
			}
		}
		if (pickType == pickUpType.energy) {
			if (!vehicle) {
				playerPowersManager = player.GetComponent<otherPowers> ();
				if (playerPowersManager.auxPowerAmount < playerPowersManager.settings.powerBar.maxValue) {
					playerPowersManager.auxPowerAmount += amount;
					pick = true;
				}
			} else {
				if (vehicleHUD.auxPowerAmount < vehicleHUD.vehicleBoost.maxValue) {
					vehicleHUD.auxPowerAmount += amount;
					pick = true;
				}
			}
		}
		if (pickType == pickUpType.ammo) {
			if (!vehicle) {
				weaponsManager = player.GetComponent<playerWeaponsManager> ();
				for (int i = 0; i < weaponsManager.weaponsList.Count; i++) {
					//print (ammoName + " " + weaponsManager.weaponsList [i].weapon.weaponSettings.Name);
					if (weaponsManager.weaponsList [i].weapon.weaponSettings.Name == secondaryString && !pick && weaponsManager.weaponsList [i].weaponEnabled) {
						pick = true;
					}
				}
			} else {
				if (vehicle.GetComponentInChildren<vehicleWeaponSystem> ()) {
					pick = true;
				}
			}
		}
		if (pickType == pickUpType.inventory) {
			if (!vehicle) {
				playerInventoryManager = player.GetComponent<inventoryManager> ();
				if (!playerInventoryManager.isInventoryFull ()) {
					pick = true;
				} else {
					int freeSpaceInSlot = playerInventoryManager.freeSpaceInSlot (inventoryObjectManager.inventoryObjectInfo.inventoryGameObject);
					if (freeSpaceInSlot > 0) {
						inventoryObjectManager.inventoryObjectInfo.amount -= freeSpaceInSlot;
						inventoryAmountPicked = playerInventoryManager.maxObjectsAmountPerSpace - freeSpaceInSlot;
						print (freeSpaceInSlot);
						freeSpaceInInventorySlot = true;
						playerInventoryManager.addAmountToInventorySlot (inventoryObjectManager.inventoryObjectInfo.inventoryGameObject,
							inventoryAmountPicked, freeSpaceInSlot);
						pick = true;
					} else {
						playerInventoryManager.showInventoryFullMessage ();
					}
				}
			} else {
				
			}
		}
		if (pickType == pickUpType.jetpackFuel) {
			if (!vehicle) {
				pick = true;
			}
		}
		if (pickType == pickUpType.weapon) {
			if (!vehicle) {
				weaponsManager = player.GetComponent<playerWeaponsManager> ();
				bool alreadyPicked = weaponsManager.checkIfWeaponAvaliable (secondaryString);
				print ("already picked " + alreadyPicked);
				if (!alreadyPicked) {
					pick = true;
				}
			}
		}
		if (pickType == pickUpType.inventoryExtraSpace) {
			if (!vehicle) {
				pick = true;
			}
		}
		if (pickType == pickUpType.map) {
			if (!vehicle) {
				pick = true;
			}
		}
		return pick;
	}
	//just to ignore the collisions with a turret when it explodes
	void OnCollisionEnter (Collision col)
	{
		if (col.gameObject.layer == LayerMask.NameToLayer ("Ignore Raycast")) {
			if (col.gameObject.GetComponent<Collider> ()) {
				Physics.IgnoreCollision (col.gameObject.GetComponent<Collider> (), transform.GetComponent<Collider> ());
			}
		}
	}
	//drop this object just in case the object has grabbed it to use it
	void checkIfGrabbed ()
	{
		player.GetComponent<grabObjects> ().checkIfDropObject (gameObject);
		player.GetComponent<otherPowers> ().checkIfDropObject (gameObject);
	}
	//enable the trigger of the pickup, so the player can use it
	public void activateObjectTrigger ()
	{
		if (!transform.GetComponentInChildren<SphereCollider> ().enabled) {
			transform.GetComponentInChildren<SphereCollider> ().enabled = true;
		}
	}
}