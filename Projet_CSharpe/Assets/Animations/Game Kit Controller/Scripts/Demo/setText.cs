using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class setText : MonoBehaviour {
	public bool editingText;
	TextMesh textMesh;
	Text text;

	//a script to set the text of every cartel in the scene, and checking if the player is inside the trigger
	void Start () {
		GetComponent<TextMesh>().text=GetComponent<Text>().text.Replace("|","\n");
		if (GetComponent<Collider> ()) {
			GetComponent<MeshRenderer> ().enabled = false;
		}
	}
	void OnTriggerEnter(Collider col){
		if (col.gameObject.tag=="Player") {
			GetComponent<MeshRenderer>().enabled=true;
		}
	}
	void OnTriggerExit(Collider col){
		if (col.gameObject.tag == "Player") {
			GetComponent<MeshRenderer> ().enabled = false;
		}
	}
	void OnDrawGizmos(){
		DrawGizmos ();
	}
	void OnDrawGizmosSelected(){
		DrawGizmos ();
	}
	void DrawGizmos(){
		if (!Application.isPlaying) {
			if (editingText) {
				if (!GetComponent<MeshRenderer> ().enabled) {
					GetComponent<MeshRenderer> ().enabled = true;
				}
				if (!textMesh || !text) {
					textMesh = GetComponent<TextMesh> ();
					text = GetComponent<Text> ();
				}
				textMesh.text = text.text.Replace ("|", "\n");
			} else {
				if (GetComponent<MeshRenderer> ().enabled) {
					GetComponent<MeshRenderer>().enabled=false;
				}
			}
		}
	}
}