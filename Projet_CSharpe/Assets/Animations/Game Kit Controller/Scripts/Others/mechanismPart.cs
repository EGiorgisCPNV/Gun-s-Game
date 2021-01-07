using UnityEngine;
using System.Collections;
public class mechanismPart : MonoBehaviour {
	public bool enableRotation;
	public GameObject rotor;
	float speed=0;
	public Vector3 rotateDirection;
	public GameObject gear;
	bool gearActivated;
	bool rotatoryGearEngaged;
	public int mechanimType;
	GameObject player;

	void Start(){
		player=GameObject.Find("Player Controller");
	}
	void Update () {
		//the script checks if the object on rails has been engaged
		if(enableRotation && mechanimType==0){
			rotor.transform.Rotate(rotateDirection*(-speed*Time.deltaTime));
		}
		if(enableRotation && mechanimType==1){
			if(rotatoryGearEngaged){
				gear.transform.Rotate(new Vector3(0,0,speed*Time.deltaTime));
				rotor.transform.Rotate(rotateDirection*(-speed*Time.deltaTime));
			}
			if (gear && gearActivated) {
				if (gear.transform.localEulerAngles.z > 350) {
					player.GetComponent<grabObjects> ().dropObject ();
					gear.tag = "Untagged";
					gearActivated = false;
					rotatoryGearEngaged = true;
				}
				else if(gear.tag!="box"){
					gear.name="rotatoryGear";
					gear.tag="box";
				}
			}
		}
	}
	void setVelocity(float v){
		speed=v;
	}
	void engaged(){
		enableRotation=true;
	}
	void gearRotated(GameObject gearAsigned){
		gearActivated = true;
		gear = gearAsigned;
	}
}