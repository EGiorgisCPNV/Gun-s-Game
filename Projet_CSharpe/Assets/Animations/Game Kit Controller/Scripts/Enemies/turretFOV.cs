using UnityEngine;
using System.Collections;

public class turretFOV : MonoBehaviour {
	public GameObject parent;
	//check any object that enter or exit the trigger of a turret
	void OnTriggerEnter(Collider col){
		if (col.gameObject.layer!=LayerMask.NameToLayer ("Ignore Raycast")) {
			parent.SendMessage ("enemyDetected", col.gameObject);
		}
	}
	void OnTriggerExit(Collider col){
		if (col.gameObject.layer!=LayerMask.NameToLayer ("Ignore Raycast")) {
			parent.SendMessage ("enemyLost", col.gameObject);
		}
	}
}
