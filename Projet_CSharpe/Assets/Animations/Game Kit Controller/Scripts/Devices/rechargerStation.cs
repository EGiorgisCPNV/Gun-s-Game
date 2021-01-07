using UnityEngine;
using System.Collections;
public class rechargerStation : MonoBehaviour {
	public float healSpeed;
	public string animationName;
	public AudioClip sound;
	public GameObject button;
	GameObject player;
	bool healing;
	bool fullyHealed;
	bool inside;
	bool playingAnimationForward;

	//this station reloads the player's health and energy, just in case that any of their values are lower that their max values
	void Start () {
		//disable the button trigger
		GetComponent<AudioSource> ().clip = sound;
		button.GetComponent<Collider> ().enabled = false;
	}
	void Update () {
		//if the station is healing the player
		if (healing && !fullyHealed) {
			//refill health and energy
			if (player.GetComponent<otherPowers> ().settings.healthBar.value < player.GetComponent<otherPowers> ().settings.healthBar.maxValue) {
				player.GetComponent<otherPowers> ().getHealth (Time.deltaTime * healSpeed);
			}
			if (player.GetComponent<otherPowers> ().settings.powerBar.value < player.GetComponent<otherPowers> ().settings.powerBar.maxValue) {
				player.GetComponent<otherPowers> ().getEnergy (Time.deltaTime * healSpeed);
			}
			//if the healht and energy are both refilled, stop the station
			if (player.GetComponent<otherPowers> ().settings.healthBar.value >= player.GetComponent<otherPowers> ().settings.healthBar.maxValue &&
				player.GetComponent<otherPowers>().settings.powerBar.value>=player.GetComponent<otherPowers>().settings.powerBar.maxValue) {
				stopHealing ();
			}
		}
		//if the player enters in the station and the button is not enabled
		if (inside && !healing && !button.GetComponent<Collider> ().enabled) {
			//check the health and energy values
			if (player.GetComponent<otherPowers> ().settings.healthBar.value < player.GetComponent<otherPowers> ().settings.healthBar.maxValue ||
				player.GetComponent<otherPowers> ().settings.powerBar.value < player.GetComponent<otherPowers> ().settings.powerBar.maxValue) {
				//if they are not fulled, enable the button trigger
				fullyHealed = false;
				button.GetComponent<Collider> ().enabled = true;
			}
		}
		//if the player is full of energy and health, and the animation in the station is not being playing, then disable the station and play the disable animation
		if (playingAnimationForward && !GetComponent<Animation> ().IsPlaying (animationName) && fullyHealed) {
			playingAnimationForward = false;
			GetComponent<Animation>()[animationName].speed = -1; 
			GetComponent<Animation>()[animationName].time=GetComponent<Animation>()[animationName].length;
			GetComponent<Animation>().Play(animationName);
		}
	}
	//this function is called when the button is pressed
	public void healPlayer(){
		//check if the player is inside the station and his health or energy is not fulled
		if(inside && !fullyHealed){
			//start to heal him
			healing = true;
			playingAnimationForward = true;
			//play the station animation and the heal sound
			GetComponent<Animation>()[animationName].speed = 1; 
			GetComponent<Animation> ().Play (animationName);
			GetComponent<AudioSource> ().Play ();
			GetComponent<AudioSource> ().loop=true;
			button.GetComponent<Collider> ().enabled = false;
		}
	}
	//stop the station
	public void stopHealing(){
		healing = false;
		fullyHealed = true;
		GetComponent<AudioSource> ().loop = false;
	}
	void OnTriggerEnter(Collider col){
		//check if the player is inside the station
		if(col.gameObject.tag == "Player"){
			if (!player) {
				player = GameObject.Find ("Player Controller");
			}
			inside=true;
		}
	}
	void OnTriggerExit(Collider col){
		//if the player exits from the station and he was being healing, stop the station
		if(col.gameObject.tag == "Player"){
			inside=false;
			if (healing) {
				stopHealing ();
			}
		}
	}
}