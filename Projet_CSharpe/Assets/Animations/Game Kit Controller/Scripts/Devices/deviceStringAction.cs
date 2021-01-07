using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class deviceStringAction : MonoBehaviour {
	public string deviceName;
	public string deviceAction;
	public bool hideIconOnPress;
	public bool disableIconOnPress;
	public bool showIcon;
	public bool showTouchIconButton;
	public float actionOffset=1;
	public bool showGizmo;
	//just a string to set the action made by the device which has this script
	//the option disableIconOnPress allows to remove the icon of the action once it is done
	//the option showIcon allows to show the icon or not when the player is inside the device trigger
	//the option showTouchIconButton allows to show the touch button to use devices

	void OnDrawGizmos(){
		DrawGizmos();
	}
	void OnDrawGizmosSelected(){
		DrawGizmos();
	}
	//draw the pivot and the final positions of every door
	void DrawGizmos(){
		if (showGizmo) {
			Gizmos.color = Color.green;
			Gizmos.DrawSphere (transform.position+ transform.up*actionOffset, 0.3f);
		}
	}
}