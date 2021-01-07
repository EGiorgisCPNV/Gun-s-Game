using UnityEngine;
using System.Collections;

public class simpleObject : MonoBehaviour {

	bool working;
	void Start () {
	
	}
	

	void Update () {
		if (working) {
			if(!GetComponent<Animation>().IsPlaying("piston")){
				if(GetComponent<Animation>()["piston"].speed == -1){
					GetComponent<Animation>()["piston"].speed = 1;
				}
				else{
					GetComponent<Animation>()["piston"].speed = -1; 
					GetComponent<Animation>()["piston"].time=GetComponent<Animation>()["piston"].length;
				}
				GetComponent<Animation>().Play("piston");
			}
		}
	}

	public void activate(){
		working = true;
	}
}
