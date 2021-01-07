using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
//a simple editor to add a button in the buildPlayer script inspector
[CustomEditor(typeof(buildPlayer))]
public class buildPlayerEditor : Editor{
	public override void OnInspectorGUI(){
		if (!Application.isPlaying) {
			DrawDefaultInspector ();
			buildPlayer player = (buildPlayer)target;
			if (GUILayout.Button ("Build Player")) {
				player.buildBody ();
			}
		}
	}
}
#endif