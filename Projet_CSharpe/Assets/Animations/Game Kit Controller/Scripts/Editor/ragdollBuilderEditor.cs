using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
//a simple editor to add a button in the ragdollBuilder script inspector
[CustomEditor(typeof(ragdollBuilder))]
public class ragdollBuilderEditor : Editor{
	public enum ragdollStateValues{
		NO, YES,
	}	
	public ragdollStateValues ragdollCreated;
	bool checkState;
	public override void OnInspectorGUI(){
		if (!Application.isPlaying) {
			DrawDefaultInspector ();
			ragdollBuilder player = (ragdollBuilder)target;
			//check if the current player has a ragdoll or not, to show it in the inspector
			if(!checkState){
				if(player.ragdollAdded){
					ragdollCreated = ragdollStateValues.YES;
				}
				else{
					ragdollCreated = ragdollStateValues.NO;
				}
				checkState=true;
			}
			GUILayout.Label("RAGDOLL ADDED: "+ragdollCreated.ToString());
			if (GUILayout.Button ("Build Ragdoll")) {
				if (player.createRagdoll ()) {
					ragdollCreated = ragdollStateValues.YES;
				}
			}
			if (GUILayout.Button ("Remove Ragdoll")) {
				if (player.removeRagdoll ()) {
					ragdollCreated = ragdollStateValues.NO;
				}
			}
			if (GUILayout.Button ("Disable Ragdoll Colliders")) {
				player.disableRagdollColliders ();
			}
		}
	}
}
#endif