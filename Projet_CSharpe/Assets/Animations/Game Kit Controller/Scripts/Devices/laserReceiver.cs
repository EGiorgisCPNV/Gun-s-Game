using UnityEngine;
using System.Collections;

public class laserReceiver : MonoBehaviour {

	public Color colorNeeded;
	public bool connected;
	public GameObject objectToConnect;
	public string activeFunctionName;
	GameObject children;

	void Start () {
		//get the cylinder inside the laser receiver, to set the color needed, so the player can see it
		if (transform.GetChild (0)) {
			children=transform.GetChild (0).gameObject;
			children.GetComponent<Renderer>().material.color=colorNeeded;
		}
	}
	

	void Update () {
		//if the laser receiver is reached by the correct laser color, the cylinder rotates
		if (connected && children) {
			children.transform.Rotate(0,100*Time.deltaTime,0);
		}
	}

	//the receiver has been reached by the correct laser color, so call the active function in the object to connect
	public void laserConnected(Color col){
		if (col == colorNeeded) {
			connected=true;
			if(objectToConnect){
				objectToConnect.SendMessage (activeFunctionName, true);
			}
		}
	}
	//the laser has been disabled, so disable the object connected
	public void laserDisconnected(){
		connected = false;
		if (objectToConnect) {
			objectToConnect.SendMessage (activeFunctionName, false);
		}
	}
}
