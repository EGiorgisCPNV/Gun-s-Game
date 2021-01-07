using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class eventTriggerSystem : MonoBehaviour {
	public List<eventInfo> eventList = new List<eventInfo>();
	public bool useSameFunctionInList;
	public List<string> sameFunctionList = new List<string>();
	public bool triggeredByButton;
	public bool useObjectToTrigger;
	public GameObject objectNeededToTrigger;
	public bool useTagToTrigger;
	public string tagNeededToTrigger;
	public bool callFunctionEveryTimeTriggered;
	public bool eventTriggered;
	public bool useSameDelay;
	public float generalDelay;
	public triggerType triggerEventType;
	public bool coroutineActive;
	public bool setParentToNull;

	public enum triggerType{
		enter,exit
	}
	void Start () {
	
	}
	void Update () {
	
	}

	public void activateEvent(){
		if (!eventTriggered || callFunctionEveryTimeTriggered) {
			eventTriggered = true;
			if (setParentToNull) {
				transform.SetParent (null);
			}
			StartCoroutine (activateEventInTime ());
		}
	}
	IEnumerator activateEventInTime(){
		coroutineActive = true;
		for (int i = 0; i < eventList.Count; i++) {
			if (useSameDelay) {
				yield return new WaitForSeconds (generalDelay);
			} else {
				yield return new WaitForSeconds (eventList [i].secondsDelay);
			}
			if (useSameFunctionInList) {
				for (int j = 0; j < sameFunctionList.Count; j++) {
					if (eventList [i].sendGameObject) {
						eventList [i].objectToCall.SendMessage (sameFunctionList [j],eventList [i].objectToSend , SendMessageOptions.DontRequireReceiver);
					} else {
						eventList [i].objectToCall.SendMessage (sameFunctionList [j], SendMessageOptions.DontRequireReceiver);
					}
				}
			} else {
				for (int j = 0; j < eventList.Count; j++) {
					if (eventList [i].sendGameObject) {
						eventList [i].objectToCall.SendMessage (eventList [i].functionNameList [j],eventList [i].objectToSend , SendMessageOptions.DontRequireReceiver);
					} else {
						eventList [i].objectToCall.SendMessage (eventList [i].functionNameList [j], SendMessageOptions.DontRequireReceiver);
					}
				}
			}
		}
		coroutineActive = false;
	}
	public void checkTriggerEventType(GameObject objectToCheck, triggerType trigger){
		if ((!eventTriggered || callFunctionEveryTimeTriggered) && !triggeredByButton) {
			if (trigger == triggerEventType) {
				if (useObjectToTrigger) {
					if (objectToCheck == objectNeededToTrigger) {
						activateEvent ();
					}
				}
				if (useTagToTrigger) {
					if (objectToCheck.tag == tagNeededToTrigger) {
						activateEvent ();
					}
				}
			}
		}
	}

	void OnTriggerEnter(Collider col){
		checkTriggerEventType (col.gameObject, triggerType.enter);
	}
	void OnTriggerExit(Collider col){
		checkTriggerEventType (col.gameObject, triggerType.exit);
	}
	public void addNewEvent(){
		eventInfo newEvent = new eventInfo ();
		eventList.Add (newEvent);
	}

	public void setSimpleFunctionByTag(string functionName, GameObject objectTocall, string tag){
		addNewEvent ();
		eventInfo newEvent = eventList [eventList.Count - 1];
		newEvent.objectToCall = objectTocall;
		newEvent.functionNameList.Add (functionName);
		useTagToTrigger = true;
		tagNeededToTrigger = tag;
	}
	[System.Serializable]
	public class eventInfo{
		public string name;
		public GameObject objectToCall;
		public List<string> functionNameList = new List<string>();
		public float secondsDelay;
		public bool sendGameObject;
		public GameObject objectToSend;
	}
}
