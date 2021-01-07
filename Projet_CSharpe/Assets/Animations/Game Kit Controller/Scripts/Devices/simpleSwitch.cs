using UnityEngine;
using System.Collections;
public class simpleSwitch : MonoBehaviour {
	public bool inside;
	public GameObject objectToActive;
	public string activeFunctionName;
	public AudioClip pressSound;
	public bool sendThisButton;
	public string switchAnimationName = "simpleSwitch";
	public float animationSpeed = 1;
	AudioSource audioSource;
	Animation buttonAnimation;

	void Start () {
		audioSource = GetComponent<AudioSource> ();
		buttonAnimation = GetComponent<Animation> ();
	}
	// a simple switch to active any device in the scene
	//the button is activated in the hacksystem script
//	void Update () {
//
//	}
	void activateDevice(){
		//check if the player is inside the trigger, and if he press the button to activate the devide
		if(inside && !buttonAnimation.IsPlaying(switchAnimationName)){
			buttonAnimation [switchAnimationName].speed = animationSpeed;
			buttonAnimation.Play(switchAnimationName);
			audioSource.PlayOneShot (pressSound);
			if(objectToActive){
				if (sendThisButton) {
					objectToActive.SendMessage (activeFunctionName,gameObject);
				} else {
					objectToActive.SendMessage (activeFunctionName);
				}
			}
		}
	}
	void OnTriggerEnter(Collider col){
		if(col.gameObject.tag == "Player"){
			inside=true;
		}
	}
	void OnTriggerExit(Collider col){
		if(col.gameObject.tag == "Player"){
			inside=false;
		}
	}
}