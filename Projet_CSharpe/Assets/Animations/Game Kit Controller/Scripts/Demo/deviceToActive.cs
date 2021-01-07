using UnityEngine;
using System.Collections;

public class deviceToActive : MonoBehaviour {

	public GameObject[] centers;
	public bool activate;
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (activate) {
			for(int i=0;i<centers.Length;i++){
				centers[i].transform.Rotate(0,200*Time.deltaTime,0);
			}
		}
	}

	void changeStateMachine(bool state){
		activate = state;
	}
}
