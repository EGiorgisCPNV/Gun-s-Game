using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class inputActionManager : MonoBehaviour {
	public List<inputActionElementInfo> inputActionList = new List<inputActionElementInfo> ();
	public bool inputActivated;
	public List<inputElementInfo> currentInputList = new List<inputElementInfo> ();
	[HideInInspector] public inputManager input;
	int i;
	public bool getActionInput(string action){
		if (inputActivated) {
			for (i = 0; i < inputActionList.Count; i++) {
				if (action == inputActionList [i].name) {
					if (input.checkInputButton (inputActionList [i].inputActionName, inputActionList [i].keyInputType)){
						return true;
					}
				}
			}
		}
		return false;
	}
	public void enableOrDisableInput(bool state){
		inputActivated = state;
	}
	//get the input manager component
	public void getInputManager(GameObject manager){
		input = manager.GetComponent<inputManager> ();
	}
	public void getCurrentInputList(){
		currentInputList.Clear ();
		if (!input) {
			input = GameObject.Find ("Character").GetComponent<inputManager> ();
		}
		if (input) {
			for (i = 0; i < input.axes.Count; i++) {
				inputElementInfo newInputElement = new inputElementInfo ();
				newInputElement.name = input.axes [i].Name;
				newInputElement.keyButton = input.axes [i].keyButton;
				currentInputList.Add (newInputElement);
			}
			#if UNITY_EDITOR
			EditorUtility.SetDirty (GetComponent<inputActionManager> ());
			#endif
		}
	}
	[System.Serializable]
	public class inputActionElementInfo{
		public string name;
		public string inputActionName;
		public inputManager.buttonType keyInputType;
		public bool showInControlsMenu;
	}
	[System.Serializable]
	public class inputElementInfo{
		public string name;
		public string keyButton;
	}
}