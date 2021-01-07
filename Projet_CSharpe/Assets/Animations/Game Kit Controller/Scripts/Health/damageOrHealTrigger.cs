using UnityEngine;
using System.Collections;
public class damageOrHealTrigger : MonoBehaviour {
	public triggerType typeOfTrigger;
	public bool useWithPlayer;
	public bool useWithVehicles;
	public bool useWithCharacters;
	public float healthValue;
	public float changeHealthRate;
	public enum triggerType{
		damage, heal
	}
	GameObject player;
	GameObject objectWithHealth;
	bool objectInside;
	float lastTime;

	void Start () {
	
	}
	void Update () {
		//if an object which can be damaged is inside the trigger, then
		if (objectInside && objectWithHealth) {
			//apply damage or heal it accordint to the time rate
			if (Time.time > lastTime + changeHealthRate) {
				//if the trigger damages
				if (typeOfTrigger == triggerType.damage) {
					//apply damage
					applyDamage.checkHealth (gameObject, objectWithHealth, healthValue, Vector3.zero, objectWithHealth.transform.position + objectWithHealth.transform.up, gameObject, true);
					lastTime = Time.time;
					//if the object inside the trigger is dead, stop applying damage
					if (applyDamage.checkIfDead (objectWithHealth)) {
						changeTriggerState (false, null, 0);
					}
				}
				//if the trigger heals
				if (typeOfTrigger == triggerType.heal) {
					//while the object is not fully healed, then 
					if (!applyDamage.checkIfMaxHealth (objectWithHealth)) {
						//heal it
						applyDamage.setHeal (healthValue, objectWithHealth);
						lastTime = Time.time;
					} else {
						//else, stop healing it
						changeTriggerState (false, null, 0);
					}
				}
			}
		}
	}
	void OnTriggerEnter(Collider col){
		//if the player enters the trigger and it can used with him, then
		if (col.gameObject.tag == "Player" && useWithPlayer) {
			//store the player
			if (!player) {
				player = col.gameObject;
			}
			if (player) {
				//if he is not driving, apply damage or heal
				if (!player.GetComponent<playerController> ().driving) {
					changeTriggerState (true, player, Time.time);
				}
			}
		} 
		//else, if a vehicle is inside the trigger and it can be used with vehicles, them
		else if (col.gameObject.tag == "device" && col.gameObject.GetComponent<vehicleHUDManager> () && useWithVehicles) {
			changeTriggerState (true, col.gameObject, Time.time);
		} 
		else if (col.gameObject.GetComponent<AIStateManager> () && useWithCharacters) {
			changeTriggerState (true, col.gameObject, Time.time);
		}
	}
	void OnTriggerExit(Collider col){
		//if the player or a vehicle exits, stop the healing or the damaging
		if (col.gameObject.tag == "Player" && useWithPlayer) {
			changeTriggerState (false, null, 0);
		} else if (col.gameObject.tag == "device" && col.gameObject.GetComponent<vehicleHUDManager> () && useWithVehicles) {
			changeTriggerState (false, null, 0);
		} else if (col.gameObject.GetComponent<AIStateManager> () && useWithCharacters) {
			changeTriggerState (false, null, 0);
		}
	}
	//stop or start the heal or damage action
	void changeTriggerState(bool inside, GameObject obj, float time){
		objectInside = inside;
		objectWithHealth = obj;
		lastTime = time;
	}
}