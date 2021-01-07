using UnityEngine;
using System.Collections;

public class deathSpiral : MonoBehaviour {
	public float speed=20;

	//the script to the rotating gears in the scene, which kill the player
	void Update () {
		transform.RotateAround(transform.position,transform.up,speed*Time.deltaTime);
	}
	void OnCollisionEnter(Collision col){
		if (col.rigidbody) {
			col.rigidbody.AddExplosionForce ( 1000,col.transform.position,100);
		}
		if (col.collider.GetComponent<health> ()) {
			float damage = col.gameObject.GetComponent<health> ().healthAmount;
			col.collider.GetComponent<health> ().setDamage (damage, col.collider.transform.forward, col.contacts [0].point, gameObject, gameObject, false);
		}
	}
}
