using UnityEngine;
using System.Collections;

public class destroyGameObject : MonoBehaviour {
	public float timer=0.6f;

	//a simple script to destroy a gameObject according to the timer
	void Update () {
		if(timer>0){
			timer-=Time.deltaTime;
			if(timer<0){
				Destroy (gameObject);
			}
		}
	}
}