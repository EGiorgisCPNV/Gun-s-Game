using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
//a simple editor to add a button in the features manager script inspector
[CustomEditor(typeof(featuresManager))]
[CanEditMultipleObjects]
public class featuresManagerEditor : Editor{
	public override void OnInspectorGUI(){
		featuresManager manager = (featuresManager)target;
		if (!Application.isPlaying) {
			DrawDefaultInspector ();
			if (GUILayout.Button ("Set Configuration")) {
				manager.setConfiguration ();
			}
			if (GUILayout.Button ("Get Current Configuration")) {
				manager.getConfiguration ();
			}
		}
	}
}
#endif