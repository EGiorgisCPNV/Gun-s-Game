using UnityEngine;
using System.Collections;
public class artificialObjectGravity : MonoBehaviour {
	public bool onGround;	  
	public LayerMask layer;
	public float rayDistance;
	public PhysicMaterial highFrictionMaterial;
	public Vector3 normal;
	public Vector3 hitPoint;
	public Vector3 auxNormal;
	public bool active=true;
	public bool normalAssigned;
	RaycastHit hit;
	int c = 0;
	float groundAdherence = 10;

	//this script is added to an object with a rigidbody, to change its gravity, disabling the useGravity parameter, and adding force in a new direction
	//checking in the object is in its new ground or not
	void FixedUpdate () {
		//if nothing pauses the script and the gameObject has rigidbody and it is not kinematic
		if (active && GetComponent<Rigidbody>()) {
			if(!GetComponent<Rigidbody>().isKinematic){
				//check if the object is on ground or in the air, to apply or not force in its gravity direction
				if (onGround) {
					if (c == 0) {
						c = 1;
						GetComponent<Collider>().material = highFrictionMaterial;
					}
				} else {
					if (c == 1) {
						c = 0;
						GetComponent<Collider>().material = null;
					}
					GetComponent<Rigidbody>().AddForce (9.8f * GetComponent<Rigidbody>().mass * normal);
					if (GetComponent<Rigidbody>().useGravity) {
						GetComponent<Rigidbody>().useGravity = false;
					}
				}
				//use a raycast to check the ground
				if (Physics.Raycast (transform.position, normal, out hit, (rayDistance + transform.localScale.x / 2), layer)) {
					if (!hit.collider.isTrigger && !hit.rigidbody) {
						onGround = true;
						if (transform.InverseTransformDirection (GetComponent<Rigidbody>().velocity).y > .5f) {
							GetComponent<Rigidbody>().position = Vector3.MoveTowards (GetComponent<Rigidbody>().position, hit.point, Time.deltaTime * groundAdherence);
						}
						if (transform.InverseTransformDirection (GetComponent<Rigidbody>().velocity).y < .01f) {
							GetComponent<Rigidbody>().velocity = Vector3.zero;
						}
					}
				} else {
					onGround = false;
				}		
			}
		}
		//if the gameObject has not rigidbody, remove the script
		if (!GetComponent<Rigidbody>()) {
			gameObject.layer= LayerMask.NameToLayer ("Default");
			Destroy (GetComponent<artificialObjectGravity>());
		}
	}
	//when the object is dropped, set its forward direction to move until a surface will be detected
	public void enableGravity(LayerMask layer, PhysicMaterial frictionMaterial){
		this.layer = layer;
		highFrictionMaterial = frictionMaterial;
		GetComponent<Rigidbody>().useGravity = false;
		normal = transform.forward;
		normalAssigned = false;
	}
	public void removeGravity(){
		//set the layer again to default, active the gravity and remove the script
		gameObject.layer= LayerMask.NameToLayer ("Default");
		GetComponent<Rigidbody>().useGravity=true;
		Destroy (GetComponent<artificialObjectGravity> ());
	}
	void OnCollisionEnter(Collision col){
		//when the objects collides with anything, use the normal of the colision
		if (active && col.gameObject.layer!=LayerMask.NameToLayer ("Ignore Raycast") && !normalAssigned && !GetComponent<Rigidbody>().isKinematic){
			//get the normal of the collision
			Vector3 direction=col.contacts[0].normal;
			//Debug.DrawRay (transform.position,-direction, Color.red, 200,false);
			if (Physics.Raycast (transform.position, -direction, out hit, 3, layer)) {
				if(!hit.collider.isTrigger && !hit.rigidbody){
					normal = -hit.normal;
					//the hit point is used for the turret rotation
					hitPoint=hit.point;
					//check the type of object
					if (gameObject.name != "turret") {
						//if the direction is the actual ground, remove the script to set the regular gravity
						if (normal == -Vector3.up) {
							removeGravity ();
							return;
						}
						normalAssigned=true;
					}
					//if the object is an ally turret, call a function to set it kinematic when it touch the ground
					if (gameObject.name == "turret") {
						if(!gameObject.GetComponent<Rigidbody>().isKinematic){
							StartCoroutine (rotateToSurface ());
						}
					}
				}
			}
		}
	}
	//when an ally turret hits a surface, rotate the turret to that surface, so the player can set a turret in any place to help him
	IEnumerator rotateToSurface(){
		GetComponent<Rigidbody>().useGravity=true;
		GetComponent<Rigidbody>().isKinematic = true;
		//it rotates the turret in the same way that the player rotates with his gravity power
		Quaternion rot = transform.rotation;
		Vector3 myForward = Vector3.Cross (transform.right, -normal);
		Quaternion dstRot = Quaternion.LookRotation (myForward, -normal);
		for (float t = 0; t < 1;) {
			t += Time.deltaTime * 3;
			transform.rotation = Quaternion.Slerp (rot, dstRot, t);
			transform.position = Vector3.MoveTowards (transform.position, hitPoint + transform.up * 0.5f, t);
			yield return null;
		}
		gameObject.layer=LayerMask.NameToLayer ("Default");
		//if the surface is the regular ground, remove the artificial gravity, and make the turret stays kinematic when it will touch the ground
		if (-normal==Vector3.up) {
			SendMessage("enabledKinematic",false);
			removeGravity();
		}
	}
	//set directly a new normal
	public void setCurrentGravity(Vector3 newNormal){
		GetComponent<Rigidbody>().useGravity = false;
		normal = newNormal;
		normalAssigned = true;
	}
}