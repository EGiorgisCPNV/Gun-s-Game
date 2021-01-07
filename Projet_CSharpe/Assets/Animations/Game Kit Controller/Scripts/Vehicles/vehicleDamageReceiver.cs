using UnityEngine;
using System.Collections;
public class vehicleDamageReceiver : MonoBehaviour {
	[Range(1,10)] public float damageMultiplier=1;
	[HideInInspector] public GameObject vehicle;
	//this script is added to every collider in a vehicle, so when a projectile hits the vehicle, its health component receives the damge
	//like this the damage detection is really accurated.
	//the function sends the amount of damage, the direction of the projectile, the position where hits, the object that fired the projectile, 
	//and if the damaged is done just once, like a bullet, or the damaged is constant like a laser
	public void setDamage (float amount, Vector3 fromDirection, Vector3 damagePos,GameObject bulletOwner,GameObject projectile,bool damageConstant) {
		vehicle.GetComponent<vehicleHUDManager> ().setDamage (amount * damageMultiplier, fromDirection, damagePos, bulletOwner, projectile, damageConstant);
	}
}