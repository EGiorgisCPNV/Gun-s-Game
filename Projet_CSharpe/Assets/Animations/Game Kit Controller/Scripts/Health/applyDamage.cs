using UnityEngine;
using System.Collections;
public class applyDamage : MonoBehaviour {
	//check if the collided object has a health component, and apply damage to it
	//any object with health component will be damaged, so the friendly fire is allowed
	//also, check if the object is a vehicle to apply damage too
	public static void checkHealth (GameObject projectile, GameObject objectToDamage, float damageAmount, Vector3 direction, Vector3 position, GameObject projectileOwner, bool damageConstant){
		if (objectToDamage.GetComponent<characterDamageReceiver> ()) {
			objectToDamage.GetComponent<characterDamageReceiver> ().setDamage (damageAmount, direction, position, projectileOwner, projectile, damageConstant);
		}
		if (objectToDamage.GetComponent<vehicleDamageReceiver> ()) {
			objectToDamage.GetComponent<vehicleDamageReceiver> ().setDamage (damageAmount, direction, position, projectileOwner, projectile, damageConstant);
		}
		if (objectToDamage.GetComponent<vehicleHUDManager> ()) {
			objectToDamage.GetComponent<vehicleHUDManager> ().setDamage (damageAmount, direction, position, projectileOwner, projectile, damageConstant);
		}
	}
	public static bool checkIfDead (GameObject objectToCheck){
		bool value = false;
		if (objectToCheck.GetComponent<health> ()) {
			if (objectToCheck.GetComponent<health> ().healthAmount <= 0) {
				value = true;
			}
		}
		if (objectToCheck.GetComponent<vehicleHUDManager> ()) {
			if (objectToCheck.GetComponent<vehicleHUDManager> ().healthAmount <= 0) {
				value = true;
			}
		}
		return value;
	}
	public static bool checkIfMaxHealth (GameObject objectToCheck){
		bool value = false;
		if (objectToCheck.GetComponent<health> () && objectToCheck.tag=="Player") {
			if (objectToCheck.GetComponent<otherPowers> ().settings.healthBar.value >= objectToCheck.GetComponent<otherPowers> ().settings.healthBar.maxValue) {
				value = true;
			} 
		}
		if (objectToCheck.GetComponent<vehicleHUDManager> ()) {
			if (objectToCheck.GetComponent<vehicleHUDManager> ().healthAmount >= objectToCheck.GetComponent<vehicleHUDManager> ().maxhealthAmount) {
				value = true;
			}
		}
		if (objectToCheck.GetComponent<health> () && objectToCheck.tag!="Player") {
			if (objectToCheck.GetComponent<health> ().healthAmount >=objectToCheck.GetComponent<health> ().getMaxHealthAmount()) {
				value = true;
			} 
		}
		return value;
	}
	public static void setHeal(float healAmount, GameObject objectToHeal){
		if (objectToHeal.GetComponent<otherPowers> ()) {
			objectToHeal.GetComponent<otherPowers> ().getHealth (healAmount);
		}
		if (objectToHeal.GetComponent<vehicleHUDManager> ()) {
			objectToHeal.GetComponent<vehicleHUDManager> ().getHealth (healAmount);
		}
		if (objectToHeal.GetComponent<AIStateManager> () && objectToHeal.tag != "Player") {
			objectToHeal.GetComponent<AIStateManager> ().getHealth (healAmount);
		}
	}
	public static bool canApplyForce(GameObject objectToCheck){
		bool canReceiveForce=false;
		if (objectToCheck.GetComponent<Rigidbody> ()) {
			if (!objectToCheck.GetComponent<Rigidbody> ().isKinematic) {
				canReceiveForce = true;
				characterDamageReceiver damageReceiver = objectToCheck.GetComponent<characterDamageReceiver> ();
				if (damageReceiver) {
					if (damageReceiver.character.tag == "Player" || damageReceiver.character.tag == "enemy" || damageReceiver.character.tag == "friend") {
						canReceiveForce = false;
					}
				}
			}
		}
		return canReceiveForce;
	}
	public static float getRemainingHealth (GameObject character){
		float healthAmount = 0;
		if (character.GetComponent<characterDamageReceiver> ()) {
			healthAmount = character.GetComponent<characterDamageReceiver> ().character.GetComponent<health>().healthAmount;
		}
		if (character.GetComponent<vehicleDamageReceiver> ()) {
			healthAmount = character.GetComponent<vehicleDamageReceiver> ().vehicle.GetComponent<vehicleHUDManager>().healthAmount;
		}
		return healthAmount;
	}
}