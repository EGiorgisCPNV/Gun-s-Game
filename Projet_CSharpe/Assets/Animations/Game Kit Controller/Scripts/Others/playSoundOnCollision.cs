using UnityEngine;
using System.Collections;
public class playSoundOnCollision : MonoBehaviour {

	void Start () {
	
	}
	void Update () {
	
	}
	void OnCollisionEnter(Collision col){
		GetComponent<AudioSource> ().Play ();
	}
}