using UnityEngine;
using System.Collections;
public class railMechanism : MonoBehaviour {
	public Transform objectOnRail;
	public GameObject stopPosition;
	public GameObject finalPosition;
	public GameObject rotor;
	public GameObject parent;
	public float rotorSpeed=500;
	public bool stopPosReached;
	public bool activated;
	public bool showGizmo;
	GameObject player;
	Vector3 initPos;
	float normalSpeed;

	void Start () {
		player=GameObject.Find("Player Controller");
		initPos=transform.position;
		normalSpeed = rotorSpeed;
	}
	void Update () {
		if (!activated) {
			//check the position of the object above the rail, if the object is too close of the extreme of the rail, set back to its original position
			if (Vector3.Distance (stopPosition.transform.position, objectOnRail.position) < 0.05f) {
				stopPosReached = true;
				player.GetComponent<grabObjects> ().dropObject ();
			}
			//if the object reachs the final position, and the player has used his power to slow down the rotor, engaged the mechanim
			//else move the object back, until the rotor moves slower
			if (Vector3.Distance (finalPosition.transform.position, objectOnRail.position) < 0.05f) {
				player.GetComponent<grabObjects> ().dropObject ();
				if (rotor) {
					if (rotorSpeed != normalSpeed) {
						engaged ();
					} else {
						StartCoroutine (error ());
					}
				} 
				//this script also can work without a spinning rotor, so the object only have to be moved from its original position to the final
				else {
					engaged ();
				}
			}
			//set the object to its original position, to avoid the player can move further
			if (stopPosReached) {
				objectOnRail.position = Vector3.MoveTowards (objectOnRail.position, initPos, Time.deltaTime * 2);
				if (initPos == objectOnRail.position) {
					stopPosReached = false;
				}	
			}
		}
		//if the rotor is not null, rotate it
		if(rotor){
			rotor.transform.Rotate(0,0,rotorSpeed*Time.deltaTime);
		}
	}
	//the power of the player to slow objects reduce the movement speed of the rotor
	void reduceVelocity(float factor){
		rotorSpeed *= factor;
	}
	//after a while, the speed backs to its normal value
	void normalVelocity(){
		rotorSpeed=normalSpeed;
		parent.SendMessage("setVelocity",rotorSpeed);
	}
	//the mechanims has been engaged, so the player does not have to use it
	void engaged(){
		parent.SendMessage("engaged");
		parent.SendMessage("setVelocity",rotorSpeed);
		if (rotor) {
			rotor.tag = "Untagged";
		}
		activated = true;
		tag="Untagged";
	}
	//move the object back
	IEnumerator error(){
		for (float t = 0; t < 1; ){
			t += Time.deltaTime;
			transform.position=Vector3.Lerp(objectOnRail.position,initPos-objectOnRail.forward*2,Time.deltaTime*2);
			yield return null; 
		}
	}
	void OnDrawGizmos(){
		DrawGizmos();
	}
	void OnDrawGizmosSelected(){
		DrawGizmos();
	}
	void DrawGizmos(){
		if (showGizmo) {
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere (finalPosition.transform.position,0.2f);
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere (stopPosition.transform.position,0.2f);
			Gizmos.color = Color.green;
			Gizmos.DrawLine (objectOnRail.position,finalPosition.transform.position);
			Gizmos.color = Color.red;
			Gizmos.DrawLine (objectOnRail.position,stopPosition.transform.position);
			Gizmos.color = Color.yellow;
			Gizmos.DrawCube (objectOnRail.position,Vector3.one/3);
		}
	}
}