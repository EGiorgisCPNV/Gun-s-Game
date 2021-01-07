using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
//a simple editor to add button in the scanElementInfo script inspector
[CustomEditor(typeof(scanElementInfo))]
public class scanELementInfoEditor : Editor{
	public override void OnInspectorGUI(){
		DrawDefaultInspector ();
		scanElementInfo element = (scanElementInfo)target;
		if (GUILayout.Button ("Save Element")) {
			element.saveElement();
		}
		if (GUILayout.Button ("Remove Element")) {
			element.removeELement();
		}
	}
}
#endif