using UnityEngine;
using System.Collections;
public class hitCombat : MonoBehaviour {
	public float hitDamage=5;
	public float addForceMultiplier;
	GameObject player;

	void Update () {
	
	}
	//check the collision in the sphere colliders in the hands and feet of the player when the close combat system is active
	//else the sphere collider are disabled to avoid damage enemies just with touch it without fight
	void OnTriggerEnter(Collider col){
		if (!player) {
			getPlayer ();
		}
		if (col.gameObject != player && !col.isTrigger) {
			GameObject objectToDamage = col.gameObject;
			if (objectToDamage.layer != 2) {
				applyDamage.checkHealth (gameObject, objectToDamage, hitDamage, -transform.forward, transform.position, player, false);
			}
			if (applyDamage.canApplyForce(objectToDamage)) {
				objectToDamage.GetComponent<Rigidbody> ().AddForce (player.transform.up + player.transform.forward * addForceMultiplier * objectToDamage.GetComponent<Rigidbody> ().mass, ForceMode.Impulse);
			} else if (objectToDamage.GetComponent<vehicleDamageReceiver> ()) {
				GameObject vehicle = objectToDamage.GetComponent<vehicleDamageReceiver> ().vehicle;
				vehicle.GetComponent<Rigidbody> ().AddForce (player.transform.up + player.transform.forward * addForceMultiplier * vehicle.GetComponent<Rigidbody> ().mass, ForceMode.Impulse);
			}
		}
	}
	void getPlayer () {
		//ignore collision between the player and the sphere colliders, to avoid hurt him self
		player=GameObject.Find("Player Controller");
		Physics.IgnoreCollision(player.GetComponent<Collider>(), GetComponent<Collider>());
	}
}