using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class launchedObjects : MonoBehaviour {
	float timer = 0;
	GameObject player;

	//this script is for the objects launched by the player, to check the object which collides with them
	void Start () {
		player=GameObject.Find("Player Controller");
	}

	void Update () {
		//if the launched objects does not collide with other object, remove the script
		if(timer > 0){
			timer -= Time.deltaTime;
			if(timer < 0){
				activateCollision();
			}
		}
	}
	
	void OnCollisionEnter(Collision col){
		if (!col.collider.isTrigger) {
			//if the object has a health script, it reduces the amount of life according to the launched object velocity
			float damage = GetComponent<Rigidbody>().velocity.magnitude;
			if (col.collider.GetComponent<characterDamageReceiver> () || col.collider.GetComponent<vehicleDamageReceiver>()) {
				applyDamage.checkHealth (gameObject, col.collider.gameObject, damage, -transform.forward, transform.position, player, false);
				activateCollision();
				if (col.collider.GetComponent<vehicleDamageReceiver> ()) {
					GameObject vehicle = col.collider.GetComponent<vehicleDamageReceiver> ().vehicle;
					vehicle.GetComponent<Rigidbody> ().AddForce (player.transform.up + player.transform.forward * damage * vehicle.GetComponent<Rigidbody> ().mass, ForceMode.Impulse);
				}
			}
			//else, set the timer to disable the script
			else{
				timer=1;
			}
		}
	}
	void activateCollision(){
		//Physics.IgnoreCollision (player.GetComponent<Collider> (), GetComponent<Collider> (), false);
		Destroy (GetComponent<launchedObjects> ());
	}
}