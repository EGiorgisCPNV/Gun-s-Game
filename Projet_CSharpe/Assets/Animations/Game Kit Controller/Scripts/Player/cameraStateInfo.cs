using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[System.Serializable]
public class cameraStateInfo {
	public string Name;
	public Vector3 camPositionOffset;
	public Vector3 pivotPositionOffset;
	public Vector2 yLimits;
	public bool showGizmo;
	public Color gizmoColor;
	public cameraStateInfo(cameraStateInfo newState){
		Name = newState.Name;
		camPositionOffset = newState.camPositionOffset;
		pivotPositionOffset = newState.pivotPositionOffset;
		yLimits = newState.yLimits;       
	}
}