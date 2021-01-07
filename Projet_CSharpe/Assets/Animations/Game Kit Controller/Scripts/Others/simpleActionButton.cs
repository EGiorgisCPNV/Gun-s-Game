using UnityEngine;
using System.Collections;
public class simpleActionButton : MonoBehaviour {
	public GameObject objectToActive;
	public string functionName;

	public void activateDevice(){
		objectToActive.SendMessage (functionName, SendMessageOptions.DontRequireReceiver);
	}
}