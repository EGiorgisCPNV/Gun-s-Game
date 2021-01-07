using UnityEngine;
using System.Collections;
public class teleportationPlatform : MonoBehaviour {
	public Transform platformToMove;
	public LayerMask layermask;
	public GameObject objectInside;
	public bool useButtonToActivate;
	teleportationPlatform platformToMoveManager;
	RaycastHit hit;
	GameObject player;
	grabObjects grabObjectsManager;

	void Start () {
		platformToMoveManager = platformToMove.GetComponent<teleportationPlatform> ();
	}
	void OnTriggerEnter(Collider col){
		if (col.gameObject.GetComponent<Rigidbody>() && !objectInside) {
			if (!player) {
				player = GameObject.FindGameObjectWithTag ("Player");
			}
			//if the object is being carried by the player, make him drop it
			if (player && !grabObjectsManager) {
				grabObjectsManager = player.GetComponent<grabObjects> ();
			}
			if (grabObjectsManager) {
				if (grabObjectsManager.objectHeld) {
					if (grabObjectsManager.objectHeld == col.gameObject) {
						grabObjectsManager.dropObject ();
					}
				}
			}
			objectInside = col.gameObject;
			if (!useButtonToActivate) {
				platformToMoveManager.sendObject (objectInside);
			}
		}
	}
	void OnTriggerExit(Collider col){
		if (objectInside && col.gameObject == objectInside) {
			objectInside = null;
		}
	}
	void activateDevice(){
		if (useButtonToActivate && objectInside) {
			platformToMoveManager.sendObject (objectInside);
		}
	}
	public void sendObject(GameObject objetToMove){
		if (Physics.Raycast (transform.position + transform.up * 2, -transform.up, out hit, Mathf.Infinity, layermask)) {
			objetToMove.transform.position = hit.point;
			objectInside = objetToMove;
		}
	}
}