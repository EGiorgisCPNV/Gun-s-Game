using UnityEngine;
using System.Collections;

public class enemyHackPanel : MonoBehaviour {
	public string whileHackingFunctionName;
	public string hackingResultName;
	public GameObject parent;
	public GameObject hackPanel;

	//activates a function in the parent, in this case it is used to activate the hacking of a turret, but it can be used to any other type of device
	public void activateDevice(){
		parent.SendMessage(whileHackingFunctionName);
		hackPanel.GetComponent<hackTerminal>().activeHack();
	}
	//send the hack result to the enemy
	public void hackResult(bool state){
		parent.SendMessage (hackingResultName, state);
	}
	//close the hack panel once the enemy has been hacked
	public void disablePanelHack(){
		StartCoroutine (disablePanelHackCoroutine ());
	}

	IEnumerator disablePanelHackCoroutine(){
		yield return new WaitForSeconds (1);
		hackPanel.GetComponent<hackTerminal>().moveHackTerminal(false);
	}
}